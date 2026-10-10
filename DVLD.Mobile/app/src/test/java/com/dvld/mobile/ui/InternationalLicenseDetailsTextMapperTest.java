package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.InternationalLicenseDetails;
import com.google.gson.Gson;

import org.junit.BeforeClass;
import org.junit.Test;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.io.File;
import java.time.ZoneId;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

import javax.xml.parsers.DocumentBuilderFactory;

import static org.junit.Assert.*;

public class InternationalLicenseDetailsTextMapperTest {
    private static final Map<Integer, String> strings = new HashMap<>();
    private final Gson gson = new Gson();
    private final InternationalLicenseDetailsTextMapper mapper = new InternationalLicenseDetailsTextMapper(
            (id, arguments) -> String.format(Locale.ROOT, strings.get(id), arguments), ZoneId.of("Asia/Hebron"));

    @BeforeClass
    public static void readActualArabicResources() throws Exception {
        NodeList nodes = DocumentBuilderFactory.newInstance().newDocumentBuilder()
                .parse(new File("src/main/res/values/strings.xml")).getElementsByTagName("string");
        for (int i = 0; i < nodes.getLength(); i++) {
            Node node = nodes.item(i);
            strings.put(R.string.class.getField(node.getAttributes().getNamedItem("name").getNodeValue())
                    .getInt(null), node.getTextContent());
        }
    }

    @Test
    public void currentlyValidIsAuthoritativeAndTakesPriorityOverOtherFlags() {
        for (String body : new String[] {"{\"isCurrentlyValid\":true}",
                "{\"isCurrentlyValid\":true,\"isActive\":false,\"isExpired\":true}"}) {
            assertEquals(InternationalLicenseDetailsTextMapper.Status.ACTIVE, mapper.status(details(body)));
            assertEquals("سارية", mapper.statusText(details(body)));
        }
    }

    @Test
    public void expiredTakesPriorityOverInactiveWhenNotCurrentlyValid() {
        for (String body : new String[] {"{\"isExpired\":true}",
                "{\"isCurrentlyValid\":false,\"isActive\":false,\"isExpired\":true}"}) {
            assertEquals(InternationalLicenseDetailsTextMapper.Status.EXPIRED, mapper.status(details(body)));
            assertEquals("منتهية", mapper.statusText(details(body)));
        }
    }

    @Test
    public void inactiveIsDisplayedWhenNotCurrentlyValidOrExpired() {
        InternationalLicenseDetails details = details("{\"isCurrentlyValid\":false,\"isExpired\":false,\"isActive\":false}");
        assertEquals(InternationalLicenseDetailsTextMapper.Status.INACTIVE, mapper.status(details));
        assertEquals("غير سارية", mapper.statusText(details));
    }

    @Test
    public void activeAloneAndNullableFlagsDoNotInventCurrentValidity() {
        for (String body : new String[] {"{}", "{\"isActive\":true}",
                "{\"isActive\":true,\"isExpired\":false}",
                "{\"isCurrentlyValid\":false,\"isActive\":true,\"isExpired\":false}"}) {
            assertEquals(InternationalLicenseDetailsTextMapper.Status.UNKNOWN, mapper.status(details(body)));
            assertEquals("—", mapper.statusText(details(body)));
        }
    }

    @Test
    public void applicationStatusesAreArabicAndUnknownValuesStayUnknown() {
        assertEquals("جديد", mapper.applicationStatus(1));
        assertEquals("ملغي", mapper.applicationStatus(2));
        assertEquals("مكتمل", mapper.applicationStatus(3));
        assertEquals("—", mapper.applicationStatus(null));
        assertEquals("—", mapper.applicationStatus(0));
        assertEquals("—", mapper.applicationStatus(99));
    }

    @Test
    public void nullableFieldsRenderUnknownWithoutInventingClassOrCitizenData() {
        InternationalLicenseDetails details = details("{}");
        assertEquals("الاسم الكامل: —\nالرقم الوطني: —", mapper.citizenInformation(details));
        assertEquals("رقم الرخصة الدولية: —\nرقم الطلب: —\nرقم السائق: —\n"
                + "رقم الرخصة المحلية المستخدمة للإصدار: —\nتاريخ الإصدار: —\nتاريخ الانتهاء: —\n"
                + "الرسوم المدفوعة: —\nحالة الرخصة: —", mapper.licenseInformation(details));
        assertEquals("تاريخ الطلب: —\nالحالة: —", mapper.applicationInformation(details));
    }

    @Test
    public void invalidIdsBlankCitizenFieldsAndInvalidFeesRenderUnknown() {
        InternationalLicenseDetails details = details("{\"internationalLicenseID\":0,\"applicationID\":-1,"
                + "\"driverID\":0,\"issuedUsingLocalLicenseID\":-2,\"fullName\":\"  \",\"nationalNo\":\"\",\"paidFees\":-1}");
        assertEquals("رقم الرخصة الدولية: —", mapper.licenseId(details));
        assertEquals("الاسم الكامل: —\nالرقم الوطني: —", mapper.citizenInformation(details));
        assertTrue(mapper.licenseInformation(details).contains("رقم الطلب: —"));
        assertTrue(mapper.licenseInformation(details).contains("رقم السائق: —"));
        assertTrue(mapper.licenseInformation(details).contains("الرسوم المدفوعة: —"));
    }

    @Test
    public void feesHaveTwoDecimalPlacesAndNoCurrencyWithSafeInvalidFallback() {
        assertEquals("15.00", mapper.fees(15.0));
        assertEquals("15.25", mapper.fees(15.25));
        assertEquals("0.00", mapper.fees(0.0));
        assertEquals("—", mapper.fees(null));
        assertEquals("—", mapper.fees(-1.0));
        assertEquals("—", mapper.fees(Double.NaN));
        assertEquals("—", mapper.fees(Double.POSITIVE_INFINITY));
    }

    @Test
    public void datesUseArabicFormattingAndPhoneZoneWhileInvalidDatesAreUnknown() {
        InternationalLicenseDetails details = details("{\"issueDate\":\"2026-10-08T23:30:00Z\","
                + "\"expirationDate\":\"2026-10-09T02:30:00+03:00\",\"applicationDate\":\"2026-10-09T02:30:00\"}");
        String[] lines = mapper.licenseInformation(details).split("\n");
        String issueDate = lines[4].substring("تاريخ الإصدار: ".length());
        String expirationDate = lines[5].substring("تاريخ الانتهاء: ".length());
        assertEquals(issueDate, expirationDate);
        assertTrue(issueDate.contains("2026"));
        assertFalse(issueDate.contains("T23:30"));
        assertTrue(mapper.applicationInformation(details).startsWith("تاريخ الطلب: " + issueDate + "\n"));
        InternationalLicenseDetails invalid = details("{\"issueDate\":\"invalid\",\"applicationDate\":\"invalid\"}");
        assertTrue(mapper.licenseInformation(invalid).contains("تاريخ الإصدار: —\nتاريخ الانتهاء: —"));
        assertEquals("تاريخ الطلب: —\nالحالة: —", mapper.applicationInformation(invalid));
    }

    @Test
    public void realCitizenLicenseAndApplicationValuesAreDisplayedInTheirOwnSections() {
        InternationalLicenseDetails details = details("{\"internationalLicenseID\":24,\"applicationID\":8,"
                + "\"fullName\":\"Citizen Fixture\",\"nationalNo\":\"N-FIXTURE\",\"driverID\":5,"
                + "\"issuedUsingLocalLicenseID\":7,\"paidFees\":15,\"isCurrentlyValid\":true,\"applicationStatus\":3}");
        assertEquals("الاسم الكامل: Citizen Fixture\nالرقم الوطني: N-FIXTURE", mapper.citizenInformation(details));
        String information = mapper.licenseInformation(details);
        assertTrue(information.startsWith("رقم الرخصة الدولية: 24\nرقم الطلب: 8\nرقم السائق: 5\n"));
        assertTrue(information.contains("رقم الرخصة المحلية المستخدمة للإصدار: 7"));
        assertTrue(information.contains("الرسوم المدفوعة: 15.00"));
        assertTrue(information.contains("حالة الرخصة: سارية"));
        assertFalse(information.contains("فئة"));
        assertEquals("تاريخ الطلب: —\nالحالة: مكتمل", mapper.applicationInformation(details));
    }

    private InternationalLicenseDetails details(String json) {
        return gson.fromJson(json, InternationalLicenseDetails.class);
    }
}
