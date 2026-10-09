package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.LocalLicenseDetails;
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

public class LocalLicenseDetailsTextMapperTest {
    private static final Map<Integer, String> strings = new HashMap<>();
    private final Gson gson = new Gson();
    private final LocalLicenseDetailsTextMapper mapper = new LocalLicenseDetailsTextMapper((id, arguments) ->
            String.format(Locale.ROOT, strings.get(id), arguments), ZoneId.of("Asia/Hebron"));

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
    public void activeAndNotExpiredDisplaysActive() {
        LocalLicenseDetails details = details("{\"isActive\":true,\"isExpired\":false}");
        assertEquals(LocalLicenseDetailsTextMapper.Status.ACTIVE, mapper.status(details));
        assertEquals("سارية", mapper.statusText(details));
    }

    @Test
    public void expiredDisplaysExpiredEvenWhenInactiveOrActiveIsUnknown() {
        for (String active : new String[] {"true", "false", "null"}) {
            LocalLicenseDetails details = details("{\"isActive\":" + active + ",\"isExpired\":true}");
            assertEquals(LocalLicenseDetailsTextMapper.Status.EXPIRED, mapper.status(details));
            assertEquals("منتهية", mapper.statusText(details));
        }
    }

    @Test
    public void inactiveDisplaysInactiveWhenNotExpiredOrExpirationFlagIsUnknown() {
        assertEquals("غير سارية", mapper.statusText(details("{\"isActive\":false,\"isExpired\":false}")));
        assertEquals("غير سارية", mapper.statusText(details("{\"isActive\":false}")));
    }

    @Test
    public void incompleteStatusFlagsDoNotInventAnActiveLicense() {
        for (String body : new String[] {"{}", "{\"isActive\":true}", "{\"isExpired\":false}"}) {
            assertEquals(LocalLicenseDetailsTextMapper.Status.UNKNOWN, mapper.status(details(body)));
            assertEquals("—", mapper.statusText(details(body)));
        }
    }

    @Test
    public void detentionUsesTrueFalseAndUnknownSeparately() {
        assertEquals("محجوزة", mapper.detention(true));
        assertEquals("غير محجوزة", mapper.detention(false));
        assertEquals("—", mapper.detention(null));
    }

    @Test
    public void detainedBadgeIsVisibleRegardlessOfLicenseStatusAndDetailedFieldIsKept() {
        for (String status : new String[] {"\"isActive\":true,\"isExpired\":false",
                "\"isActive\":false,\"isExpired\":false", "\"isExpired\":true"}) {
            LocalLicenseDetails details = details("{\"isDetained\":true," + status + "}");
            assertTrue(mapper.showDetainedBadge(details));
            assertTrue(mapper.licenseInformation(details).contains("حالة الحجز: محجوزة"));
        }
    }

    @Test
    public void notDetainedBadgeIsHiddenAndDetailedFieldIsKept() {
        LocalLicenseDetails details = details("{\"isDetained\":false}");
        assertFalse(mapper.showDetainedBadge(details));
        assertTrue(mapper.licenseInformation(details).contains("حالة الحجز: غير محجوزة"));
    }

    @Test
    public void nullableDetentionDoesNotShowAWarningBadge() {
        assertFalse(mapper.showDetainedBadge(details("{}")));
    }

    @Test
    public void stableIssueReasonsUseArabicRatherThanBackendText() {
        assertEquals("إصدار لأول مرة", mapper.issueReason(1, "First Time"));
        assertEquals("تجديد", mapper.issueReason(2, "Renew"));
        assertEquals("بدل تالف", mapper.issueReason(3, "Damaged Replacement"));
        assertEquals("بدل فاقد", mapper.issueReason(4, "Lost Replacement"));
        assertEquals("تجديد", mapper.issueReason(2, null));
    }

    @Test
    public void unknownIssueReasonPreservesBackendTextOrDisplaysUnknown() {
        assertEquals("Future Reason", mapper.issueReason(99, "Future Reason"));
        assertEquals("Future Reason", mapper.issueReason(null, "Future Reason"));
        assertEquals("—", mapper.issueReason(99, null));
        assertEquals("—", mapper.issueReason(null, "  "));
    }

    @Test
    public void licenseClassReusesArabicMappingAndUnknownClassFallsBackSafely() {
        assertEquals("الفئة الثانية - رخصة دراجة نارية ثقيلة", mapper.title(details(
                "{\"className\":\"Class 2 - Heavy Motorcycle License\"}")));
        assertEquals("Future Class", mapper.title(details("{\"className\":\"Future Class\"}")));
        assertEquals("رخصة محلية", mapper.title(details("{}")));
    }

    @Test
    public void nullableOrBlankNotesShowEmptyNotesMessageAndRealNotesRemainUnchanged() {
        assertEquals("لا توجد ملاحظات", mapper.notes(null));
        assertEquals("لا توجد ملاحظات", mapper.notes(""));
        assertEquals("لا توجد ملاحظات", mapper.notes(" \n "));
        assertEquals("ملاحظة من النظام", mapper.notes("ملاحظة من النظام"));
    }

    @Test
    public void feesHaveTwoDecimalPlacesAndNoInventedCurrency() {
        assertEquals("15.00", mapper.fees(15.0));
        assertEquals("0.00", mapper.fees(0.0));
        assertEquals("15.25", mapper.fees(15.25));
        assertEquals("—", mapper.fees(null));
        assertEquals("—", mapper.fees(Double.NaN));
        assertEquals("—", mapper.fees(Double.POSITIVE_INFINITY));
    }

    @Test
    public void nullableFieldsAndInvalidDatesAreSafeAndAbsentDescriptionIsOmitted() {
        LocalLicenseDetails details = details("{\"licenseID\":24,\"issueDate\":\"invalid\"}");
        assertEquals("الاسم الكامل: —\nالرقم الوطني: —", mapper.citizenInformation(details));
        assertEquals("رقم الرخصة: 24\nرقم الطلب: —\nفئة الرخصة: —\nتاريخ الإصدار: —\nتاريخ الانتهاء: —\n"
                + "الرسوم المدفوعة: —\nسبب الإصدار: —\nحالة الرخصة: —\nحالة الحجز: —",
                mapper.licenseInformation(details));
    }

    @Test
    public void realCitizenFieldsDescriptionAndDatesAreIncluded() {
        LocalLicenseDetails details = details("{\"licenseID\":24,\"applicationID\":8,"
                + "\"fullName\":\"Citizen Fixture\",\"nationalNo\":\"N-FIXTURE\","
                + "\"classDescription\":\"Description from API\",\"issueDate\":\"2026-10-09T13:00:00+03:00\"}");
        assertEquals("الاسم الكامل: Citizen Fixture\nالرقم الوطني: N-FIXTURE", mapper.citizenInformation(details));
        String information = mapper.licenseInformation(details);
        assertTrue(information.contains("رقم الطلب: 8"));
        assertTrue(information.contains("وصف الفئة: Description from API"));
        assertTrue(information.contains("2026"));
        assertFalse(information.contains("T13:00"));
        assertFalse(information.contains("+03:00"));
    }

    private LocalLicenseDetails details(String json) {
        return gson.fromJson(json, LocalLicenseDetails.class);
    }
}
