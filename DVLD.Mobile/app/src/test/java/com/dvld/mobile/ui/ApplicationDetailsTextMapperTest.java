package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApplicationDetails;
import com.dvld.mobile.model.ApplicationSpecificDetails;
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

public class ApplicationDetailsTextMapperTest {
    private static final Map<Integer, String> strings = new HashMap<>();
    private final Gson gson = new Gson();
    private final ApplicationDetailsTextMapper mapper = new ApplicationDetailsTextMapper(
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
    public void commonFieldsAlwaysDisplayAndApplicationFeesHaveOwnLabel() {
        ApplicationDetails application = application("{\"applicationID\":24,\"applicationTypeID\":5,"
                + "\"applicationStatus\":3,\"paidFees\":15.25,\"applicationDate\":\"invalid\",\"lastStatusDate\":null}");
        assertEquals("رقم الطلب: 24\nنوع الطلب: فك حجز رخصة قيادة\nالحالة: مكتمل\n"
                + "تاريخ تقديم الطلب: —\nآخر تحديث للحالة: —\nرسوم الطلب: 15.25", mapper.commonInformation(application));
    }

    @Test
    public void allSevenTypesReuseExistingArabicTranslations() {
        String[] expected = {"إصدار رخصة قيادة جديدة", "تجديد رخصة قيادة", "بدل فاقد لرخصة قيادة",
                "بدل تالف لرخصة قيادة", "فك حجز رخصة قيادة", "إصدار رخصة دولية جديدة", "إعادة اختبار"};
        for (int i = 1; i <= 7; i++) {
            assertEquals(expected[i - 1], mapper.applicationType(application("{\"applicationTypeID\":" + i
                    + ",\"applicationTypeName\":\"DifferentName\"}")));
        }
        assertEquals("إعادة اختبار", mapper.applicationType(application("{\"applicationTypeName\":\"RetakeTest\"}")));
    }

    @Test
    public void numericStatusesAndBackendFallbackUseExistingMapping() {
        String[] expected = {"جديد", "ملغي", "مكتمل"};
        String[] names = {" New ", "CANCELLED", "Completed"};
        ApplicationsTextMapper.Status[] states = {ApplicationsTextMapper.Status.NEW,
                ApplicationsTextMapper.Status.CANCELLED, ApplicationsTextMapper.Status.COMPLETED};
        for (int i = 1; i <= 3; i++) {
            ApplicationDetails application = application("{\"applicationStatus\":" + i + "}");
            assertEquals(expected[i - 1], mapper.statusText(application));
            assertEquals(states[i - 1], mapper.status(application));
            assertEquals(expected[i - 1], mapper.statusText(application(
                    "{\"applicationStatus\":99,\"statusText\":\"" + names[i - 1] + "\"}")));
        }
    }

    @Test
    public void nullOrUnknownCommonValuesNeverInventData() {
        assertEquals("رقم الطلب: —\nنوع الطلب: —\nالحالة: —\n"
                + "تاريخ تقديم الطلب: —\nآخر تحديث للحالة: —\nرسوم الطلب: —", mapper.commonInformation(application("{}")));
        ApplicationDetails unknown = application("{\"applicationTypeID\":99,\"applicationTypeName\":\"FutureType\","
                + "\"applicationStatus\":99,\"statusText\":\"FutureStatus\",\"applicationID\":0}");
        assertEquals("FutureType", mapper.applicationType(unknown));
        assertEquals("—", mapper.statusText(unknown));
        assertEquals("رقم الطلب: —", mapper.applicationId(unknown));
    }

    @Test
    public void datesUsePhoneZoneAndInvalidDatesAreDashes() {
        ApplicationDetails valid = application("{\"applicationDate\":\"2026-10-08T23:30:00Z\","
                + "\"lastStatusDate\":\"2026-10-09T02:30:00+03:00\"}");
        String[] lines = mapper.commonInformation(valid).split("\n");
        String date = lines[3].substring("تاريخ تقديم الطلب: ".length());
        assertEquals(date, lines[4].substring("آخر تحديث للحالة: ".length()));
        assertTrue(date.contains("2026"));
        assertFalse(date.contains("T23:30"));
        assertTrue(mapper.commonInformation(application("{\"applicationDate\":\"2026-02-30\",\"lastStatusDate\":\"invalid\"}"))
                .contains("تاريخ تقديم الطلب: —\nآخر تحديث للحالة: —"));
    }

    @Test
    public void feesAreTwoDecimalsAndNullNegativeOrNonFiniteFeesAreSafe() {
        for (String fee : new String[] {"null", "-1", "\"NaN\"", "\"Infinity\""}) {
            assertTrue(mapper.commonInformation(application("{\"paidFees\":" + fee + "}")).endsWith("رسوم الطلب: —"));
            if (!"null".equals(fee)) {
                assertTrue(mapper.additionalInformation(typed(5, "{\"fineFees\":" + fee + "}"))
                        .startsWith("قيمة المخالفة: —"));
            }
        }
        assertTrue(mapper.commonInformation(application("{\"paidFees\":0}")).endsWith("رسوم الطلب: 0.00"));
    }

    @Test
    public void missingNullAndEmptyDetailsAreSuccessfulPresentationStates() {
        for (int type = 1; type <= 7; type++) {
            for (String details : new String[] {"null", "{}"}) {
                ApplicationDetails application = typed(type, details);
                assertEquals("لا توجد تفاصيل إضافية متاحة لهذا الطلب", mapper.additionalInformation(application));
                assertTrue(mapper.commonInformation(application).contains("نوع الطلب: "));
            }
        }
        assertEquals("لا توجد تفاصيل إضافية متاحة لهذا الطلب", mapper.additionalInformation(application("{}")));
        assertEquals("لا توجد تفاصيل إضافية متاحة لهذا الطلب", mapper.additionalInformation(typed(99, "{\"licenseID\":7}")));
    }

    @Test
    public void newLocalApplicationShowsLocalIdAndTranslatedClassOnly() {
        ApplicationDetails application = typed(1, "{\"localDrivingLicenseApplicationID\":12,\"licenseClassID\":3,"
                + "\"className\":\"Class 3 - Ordinary driving license\",\"licenseID\":7}");
        assertEquals(Integer.valueOf(3), application.getDetails().getLicenseClassID());
        assertEquals("رقم طلب الرخصة المحلية: 12\nفئة الرخصة: الفئة الثالثة - رخصة قيادة عادية",
                mapper.additionalInformation(application));
        assertEquals("رقم طلب الرخصة المحلية: —", mapper.additionalInformation(typed(1,
                "{\"localDrivingLicenseApplicationID\":0,\"className\":\" \"}")));
        assertEquals("فئة الرخصة: FutureClass", mapper.additionalInformation(typed(1, "{\"className\":\"FutureClass\"}")));
    }

    @Test
    public void renewLostAndDamagedShowActualLocalLicenseDetails() {
        for (int type : new int[] {2, 3, 4}) {
            String information = mapper.additionalInformation(typed(type, "{\"licenseID\":7,"
                    + "\"className\":\"Class 2 - Heavy Motorcycle License\",\"issueDate\":\"invalid\","
                    + "\"expirationDate\":\"invalid\",\"isActive\":false,\"issueReason\":2}"));
            assertEquals("رقم الرخصة: 7\nفئة الرخصة: الفئة الثانية - رخصة دراجة نارية ثقيلة\n"
                    + "تاريخ الإصدار: —\nتاريخ الانتهاء: —\nحالة الرخصة: غير سارية\nسبب الإصدار: تجديد", information);
        }
        assertEquals("حالة الرخصة: سارية", mapper.additionalInformation(typed(2, "{\"isActive\":true}")));
    }

    @Test
    public void issueReasonsReuseCurrentMappingAndUnknownUsesSafeBackendText() {
        String[] expected = {"إصدار لأول مرة", "تجديد", "بدل تالف", "بدل فاقد"};
        for (int reason = 1; reason <= 4; reason++) {
            assertEquals("سبب الإصدار: " + expected[reason - 1], mapper.additionalInformation(typed(2,
                    "{\"issueReason\":" + reason + ",\"issueReasonText\":\"DifferentName\"}")));
        }
        assertEquals("سبب الإصدار: FutureReason", mapper.additionalInformation(typed(2,
                "{\"issueReason\":99,\"issueReasonText\":\"FutureReason\"}")));
        assertEquals("سبب الإصدار: —", mapper.additionalInformation(typed(2, "{\"issueReason\":99}")));
    }

    @Test
    public void internationalDetailsShowSeparateIdentifiersAndDates() {
        String information = mapper.additionalInformation(typed(6, "{\"internationalLicenseID\":20,\"driverID\":5,"
                + "\"issuedUsingLocalLicenseID\":7,\"issueDate\":\"2026-10-09\",\"expirationDate\":\"invalid\","
                + "\"isCurrentlyValid\":true}"));
        assertTrue(information.startsWith("رقم الرخصة الدولية: 20\nرقم السائق: 5\nرقم الرخصة المحلية المستخدمة للإصدار: 7\n"));
        assertTrue(information.contains("تاريخ الإصدار: "));
        assertTrue(information.contains("تاريخ الانتهاء: —"));
        assertTrue(information.endsWith("حالة السريان: سارية"));
        assertFalse(information.contains("فئة"));
    }

    @Test
    public void internationalValidityUsesCurrentlyValidAsPrimarySource() {
        assertEquals("سارية", mapper.internationalValidity(specific("{\"isCurrentlyValid\":true,\"isActive\":false,\"isExpired\":true}")));
        assertEquals("غير سارية", mapper.internationalValidity(specific("{\"isCurrentlyValid\":false,\"isActive\":true}")));
        assertEquals("منتهية", mapper.internationalValidity(specific("{\"isCurrentlyValid\":false,\"isExpired\":true}")));
        assertEquals("—", mapper.internationalValidity(specific("{\"isActive\":true}")));
        assertEquals("—", mapper.internationalValidity(specific("{}")));
        assertEquals("غير سارية", mapper.internationalValidity(specific("{\"isActive\":false}")));
    }

    @Test
    public void detentionFineAndApplicationFeesNeverMix() {
        ApplicationDetails application = application("{\"applicationTypeID\":5,\"paidFees\":15.25,\"details\":{"
                + "\"detainID\":8,\"licenseID\":7,\"detainDate\":\"2026-10-01\",\"fineFees\":90.5,\"isReleased\":true}} ");
        assertTrue(mapper.commonInformation(application).endsWith("رسوم الطلب: 15.25"));
        String additional = mapper.additionalInformation(application);
        assertTrue(additional.startsWith("رقم الحجز: 8\nرقم الرخصة: 7\nتاريخ الحجز: "));
        assertTrue(additional.contains("قيمة المخالفة: 90.50"));
        assertFalse(additional.contains("15.25"));
        assertFalse(mapper.commonInformation(application).contains("90.50"));
    }

    @Test
    public void releasedDetentionShowsReleaseDateAndReleaseApplicationId() {
        String information = mapper.additionalInformation(typed(5, "{\"isReleased\":true,\"releaseDate\":\"2026-10-09\",\"releaseApplicationID\":24}"));
        assertTrue(information.startsWith("حالة الحجز: تم فك الحجز\nتاريخ فك الحجز: "));
        assertTrue(information.endsWith("رقم طلب فك الحجز: 24"));
    }

    @Test
    public void unreleasedDetentionDoesNotInventReleaseDateOrReleaseId() {
        assertEquals("حالة الحجز: الرخصة ما زالت محجوزة", mapper.additionalInformation(typed(5,
                "{\"isReleased\":false,\"releaseDate\":null,\"releaseApplicationID\":null}")));
        assertEquals("حالة الحجز: تم فك الحجز", mapper.additionalInformation(typed(5,
                "{\"isReleased\":true,\"releaseDate\":null}")));
        assertEquals("—", mapper.releaseStatus(null));
        assertTrue(mapper.additionalInformation(typed(5, "{\"detainID\":8,\"isReleased\":null}")).endsWith("حالة الحجز: —"));
    }

    @Test
    public void retakeTestShowsAppointmentTimeAndSeparateFeesWithoutInventingTestId() {
        ApplicationDetails application = application("{\"applicationTypeID\":7,\"paidFees\":10,\"details\":{"
                + "\"testAppointmentID\":18,\"testTypeID\":2,\"testTypeName\":\"WrittenTest\","
                + "\"localDrivingLicenseApplicationID\":12,\"appointmentDate\":\"2026-10-09T15:30:00\","
                + "\"appointmentPaidFees\":25.5,\"isLocked\":false,\"testID\":null}} ");
        String information = mapper.additionalInformation(application);
        assertTrue(information.startsWith("رقم موعد الاختبار: 18\nنوع الاختبار: الاختبار النظري\nرقم طلب الرخصة المحلية: 12\nتاريخ الموعد: "));
        assertTrue(information.contains(":"));
        assertTrue(information.contains("رسوم الموعد: 25.50"));
        assertTrue(information.endsWith("حالة الموعد: الموعد غير مغلق"));
        assertFalse(information.contains("رقم الاختبار:"));
        assertTrue(mapper.commonInformation(application).endsWith("رسوم الطلب: 10.00"));
        assertFalse(information.contains("رسوم الطلب"));
    }

    @Test
    public void retakeLockedStateInvalidDateAndExistingTestIdAreSafe() {
        assertEquals("تاريخ الموعد: —\nحالة الموعد: الموعد مغلق\nرقم الاختبار: 9", mapper.additionalInformation(typed(7,
                "{\"appointmentDate\":\"invalid\",\"isLocked\":true,\"testID\":9}")));
        assertEquals("—", mapper.appointmentStatus(null));
        assertEquals("رقم موعد الاختبار: —\nحالة الموعد: —", mapper.additionalInformation(typed(7,
                "{\"testAppointmentID\":-1,\"isLocked\":null}")));
    }

    @Test
    public void testTypesUseConfirmedIdsOrNamesAndUnknownNamesRemainSafe() {
        String[] expected = {"اختبار النظر", "الاختبار النظري", "الاختبار العملي"};
        String[] names = {"VisionTest", "WrittenTest", "StreetTest"};
        for (int type = 1; type <= 3; type++) {
            assertEquals("نوع الاختبار: " + expected[type - 1], mapper.additionalInformation(typed(7,
                    "{\"testTypeID\":" + type + "}" )).split("\n")[0]);
            assertEquals("نوع الاختبار: " + expected[type - 1], mapper.additionalInformation(typed(7,
                    "{\"testTypeName\":\"" + names[type - 1] + "\"}" )).split("\n")[0]);
        }
        assertTrue(mapper.additionalInformation(typed(7, "{\"testTypeID\":99,\"testTypeName\":\"FutureTest\"}"))
                .startsWith("نوع الاختبار: FutureTest"));
    }

    private ApplicationDetails typed(int type, String details) {
        return application("{\"applicationTypeID\":" + type + ",\"details\":" + details + "}");
    }

    private ApplicationDetails application(String json) {
        return gson.fromJson(json, ApplicationDetails.class);
    }

    private ApplicationSpecificDetails specific(String json) {
        return gson.fromJson(json, ApplicationSpecificDetails.class);
    }
}
