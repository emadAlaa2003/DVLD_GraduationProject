package com.dvld.mobile.ui;

import com.dvld.mobile.model.TestAppointmentDetails;
import com.dvld.mobile.model.ApiDateTime;
import java.time.Instant;
import org.junit.Test;
import static org.junit.Assert.*;

public class TestAppointmentDetailsTextMapperTest extends ArabicResourcesTestSupport {
    private final TestAppointmentDetailsTextMapper mapper = new TestAppointmentDetailsTextMapper(arabic, ZONE);

    @Test
    public void allTestTypesAreArabicAndUnknownNamesStaySafe() {
        String[] expected = {"فحص النظر", "الاختبار النظري", "اختبار القيادة العملي"};
        for (int id = 1; id <= 3; id++) assertEquals(expected[id - 1], mapper.testType(details("{\"testTypeID\":" + id + "}")));
        assertEquals("FutureTest", mapper.testType(details("{\"testTypeID\":99,\"testTypeName\":\"FutureTest\"}")));
        assertEquals("—", mapper.testType(details("{}")));
    }

    @Test
    public void commonFieldsContainRealIdentifiersAndTwoDecimalAppointmentFees() {
        String information = mapper.commonInformation(details("{\"testAppointmentID\":18,\"testTypeID\":2,"
                + "\"localDrivingLicenseApplicationID\":12,\"paidFees\":25.5,\"isLocked\":true}"), NOW);
        assertEquals("رقم موعد الاختبار: 18\nنوع الاختبار: الاختبار النظري\nرقم طلب الرخصة المحلية: 12\n"
                + "تاريخ الموعد: —\nرسوم الموعد: 25.50\nحالة الموعد / الاختبار: —", information);
    }

    @Test
    public void dateTimeContainsTimeAndNormalizesPhoneZone() {
        String local = mapper.commonInformation(details("{\"appointmentDate\":\"2026-10-09T15:30:00\"}"), NOW);
        String utc = mapper.commonInformation(details("{\"appointmentDate\":\"2026-10-09T12:30:00Z\"}"), NOW);
        assertEquals(local, utc);
        String date = local.split("\n")[3].substring("تاريخ الموعد: ".length());
        assertTrue(date.matches(".*\\p{Nd}{1,2}:\\p{Nd}{2}.*"));
    }

    @Test
    public void absentTestIsANormalNoResultState() {
        for (String json : new String[] {"{}", "{\"test\":null}"}) {
            assertEquals("لم يتم تسجيل نتيجة الاختبار بعد", mapper.testInformation(details(json)));
        }
    }

    @Test
    public void passedAndFailedAreBasedOnBooleanResultWithoutMixingEnglishText() {
        TestAppointmentDetails passed = details("{\"isLocked\":true,\"test\":{\"testID\":9,\"testResult\":true,\"resultText\":\"Failed\"}}");
        TestAppointmentDetails failed = details("{\"test\":{\"testID\":10,\"testResult\":false,\"resultText\":\"Passed\"}}");
        assertEquals("رقم الاختبار: 9\nالنتيجة: ناجح", mapper.testInformation(passed));
        assertEquals("رقم الاختبار: 10\nالنتيجة: راسب", mapper.testInformation(failed));
        assertEquals(AppointmentsTextMapper.Status.PASSED, mapper.status(passed, NOW));
        assertEquals(AppointmentsTextMapper.Status.FAILED, mapper.status(failed, NOW));
    }

    @Test
    public void notesAreOnlyShownWhenPresent() {
        assertEquals("رقم الاختبار: 9\nالنتيجة: ناجح\nالملاحظات: ملاحظة", mapper.testInformation(details(
                "{\"test\":{\"testID\":9,\"testResult\":true,\"notes\":\"ملاحظة\"}}")));
        assertEquals("رقم الاختبار: —\nالنتيجة: —", mapper.testInformation(details("{\"test\":{\"notes\":\" \"}}")));
    }

    @Test
    public void retakeIdOnlyAppearsWhenProvided() {
        assertFalse(mapper.commonInformation(details("{\"retakeTestApplicationID\":null}"), NOW).contains("رقم طلب إعادة الاختبار"));
        assertTrue(mapper.commonInformation(details("{\"retakeTestApplicationID\":30}"), NOW).endsWith("رقم طلب إعادة الاختبار: 30"));
        assertTrue(mapper.commonInformation(details("{\"retakeTestApplicationID\":0}"), NOW).endsWith("رقم طلب إعادة الاختبار: —"));
    }

    @Test
    public void nullOrInvalidValuesNeverInventDatesFeesIdsOrResults() {
        assertEquals("رقم موعد الاختبار: —\nنوع الاختبار: —\nرقم طلب الرخصة المحلية: —\n"
                + "تاريخ الموعد: —\nرسوم الموعد: —\nحالة الموعد / الاختبار: —", mapper.commonInformation(details("{}"), NOW));
        for (String fee : new String[] {"-1", "\"NaN\"", "\"Infinity\""}) {
            assertTrue(mapper.commonInformation(details("{\"paidFees\":" + fee + ",\"appointmentDate\":\"bad\"}"), NOW)
                    .contains("تاريخ الموعد: —\nرسوم الموعد: —"));
        }
        assertEquals("رقم الاختبار: —\nالنتيجة: —", mapper.testInformation(details("{\"test\":{\"testID\":0,\"resultText\":\"Passed\"}}")));
    }

    @Test
    public void awaitingUpcomingAndUnknownStatesAreConsistentRegardlessOfLock() {
        assertEquals(AppointmentsTextMapper.Status.UPCOMING, mapper.status(details("{\"appointmentDate\":\"2026-10-11\",\"isLocked\":false}"), NOW));
        assertEquals(AppointmentsTextMapper.Status.AWAITING_RESULT, mapper.status(details("{\"appointmentDate\":\"2026-10-09\",\"isLocked\":false}"), NOW));
        assertEquals(AppointmentsTextMapper.Status.UNKNOWN, mapper.status(details("{\"isLocked\":true}"), NOW));
        assertEquals(AppointmentsTextMapper.Status.AWAITING_RESULT, mapper.status(details("{\"appointmentDate\":\"2026-10-09\",\"test\":{},\"isLocked\":true}"), NOW));
        assertEquals(AppointmentsTextMapper.Status.UNKNOWN, mapper.status(details("{\"test\":{},\"isLocked\":false}"), NOW));
    }

    @Test
    public void phoneAppointment143ShowsAwaitingAfterTimeButKeepsNoResultSection() {
        TestAppointmentDetails details = details("{\"testAppointmentID\":143,\"appointmentDate\":\"2026-10-10T18:17:00\",\"test\":null}");
        Instant atAppointment = ApiDateTime.parse(details.getAppointmentDate(), ZONE);
        assertEquals("موعد قادم", mapper.statusText(details, atAppointment.minusSeconds(1)));
        assertEquals("بانتظار تسجيل النتيجة", mapper.statusText(details, atAppointment));
        Instant after = ApiDateTime.parse("2026-10-10T18:32:00", ZONE);
        assertEquals("بانتظار تسجيل النتيجة", mapper.statusText(details, after));
        assertTrue(mapper.commonInformation(details, after).endsWith("حالة الموعد / الاختبار: بانتظار تسجيل النتيجة"));
        assertEquals("لم يتم تسجيل نتيجة الاختبار بعد", mapper.testInformation(details));
    }

    private TestAppointmentDetails details(String json) { return gson.fromJson(json, TestAppointmentDetails.class); }
}
