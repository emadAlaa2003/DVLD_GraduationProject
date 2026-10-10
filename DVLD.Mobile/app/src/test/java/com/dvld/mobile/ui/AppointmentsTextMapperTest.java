package com.dvld.mobile.ui;

import com.dvld.mobile.model.TestAppointment;
import com.dvld.mobile.model.ApiDateTime;
import org.junit.Test;
import java.time.Instant;
import java.util.Arrays;
import java.util.List;
import static org.junit.Assert.*;

public class AppointmentsTextMapperTest extends ArabicResourcesTestSupport {
    private final AppointmentsTextMapper mapper = new AppointmentsTextMapper(arabic, ZONE);

    @Test
    public void knownTypesUseIdsAndUnknownTypesFallBackSafely() {
        assertEquals("فحص النظر", mapper.testType(1, "Other"));
        assertEquals("الاختبار النظري", mapper.testType(2, null));
        assertEquals("اختبار القيادة العملي", mapper.testType(3, "StreetTest"));
        assertEquals("Future Test", mapper.testType(99, "Future Test"));
        assertEquals("—", mapper.testType(null, null));
        assertEquals("—", mapper.testType(99, " "));
    }

    @Test
    public void passedAndFailedResultsTakePriorityOverLockedFlags() {
        TestAppointment passed = appointment("{\"isLocked\":true,\"testResult\":true}");
        TestAppointment failed = appointment("{\"isLocked\":true,\"testResult\":false}");
        assertEquals(AppointmentsTextMapper.Status.PASSED, mapper.status(passed, NOW));
        assertEquals(AppointmentsTextMapper.Status.FAILED, mapper.status(failed, NOW));
        assertEquals("ناجح", mapper.statusText(mapper.status(passed, NOW)));
        assertEquals("راسب", mapper.statusText(mapper.status(failed, NOW)));
        assertTrue(mapper.details(failed).endsWith("النتيجة: راسب"));
    }

    @Test
    public void awaitingAndUpcomingAreBasedOnDateWithoutInventingResults() {
        assertEquals(AppointmentsTextMapper.Status.UPCOMING,
                mapper.status(appointment("{\"appointmentDate\":\"2026-10-11\",\"isLocked\":false}"), NOW));
        assertEquals(AppointmentsTextMapper.Status.AWAITING_RESULT,
                mapper.status(appointment("{\"appointmentDate\":\"2026-10-09\",\"isLocked\":false}"), NOW));
        assertEquals("موعد قادم", mapper.statusText(AppointmentsTextMapper.Status.UPCOMING));
        assertEquals("بانتظار تسجيل النتيجة", mapper.statusText(AppointmentsTextMapper.Status.AWAITING_RESULT));
        assertFalse(mapper.details(appointment("{\"isLocked\":false}")).contains("النتيجة:"));
    }

    @Test
    public void invalidDatesWithoutResultsAreUnknownRegardlessOfLockAndTestId() {
        assertEquals(AppointmentsTextMapper.Status.UNKNOWN, mapper.status(appointment("{\"isLocked\":true}"), NOW));
        assertEquals(AppointmentsTextMapper.Status.UNKNOWN, mapper.status(appointment("{\"appointmentDate\":\"bad\",\"isLocked\":false}"), NOW));
        assertEquals("—", mapper.statusText(AppointmentsTextMapper.Status.UNKNOWN));
        assertEquals(AppointmentsTextMapper.Status.UNKNOWN, mapper.status(appointment("{}"), NOW));
        assertEquals(AppointmentsTextMapper.Status.UNKNOWN, mapper.status(appointment("{\"testID\":9,\"isLocked\":false}"), NOW));
        assertEquals("—", mapper.resultText(null));
    }

    @Test
    public void upcomingAppointmentsSortNearestFirstBeforePreviousTests() {
        TestAppointment near = appointment("{\"appointmentDate\":\"2026-10-11\",\"isLocked\":false}");
        TestAppointment far = appointment("{\"appointmentDate\":\"2026-10-12\",\"isLocked\":false}");
        TestAppointment previous = appointment("{\"appointmentDate\":\"2026-10-09\",\"testResult\":true}");
        assertEquals(Arrays.asList(near, far, previous), mapper.sortedAppointments(Arrays.asList(previous, far, near), NOW));
    }

    @Test
    public void previousAppointmentsSortNewestFirstAndDoNotModifyApiOrder() {
        TestAppointment older = appointment("{\"appointmentDate\":\"2026-10-08\"}");
        TestAppointment recent = appointment("{\"appointmentDate\":\"2026-10-09\",\"isLocked\":true}");
        List<TestAppointment> original = Arrays.asList(older, recent);
        assertEquals(Arrays.asList(recent, older), mapper.sortedAppointments(original, NOW));
        assertEquals(Arrays.asList(older, recent), original);
    }

    @Test
    public void resultlessFutureAppointmentsIgnoreLockAndTestIdWhileCompletedTestsKeepResults() {
        TestAppointment upcoming = appointment("{\"appointmentDate\":\"2026-10-13\",\"isLocked\":false}");
        TestAppointment locked = appointment("{\"appointmentDate\":\"2026-10-14\",\"isLocked\":true,\"testID\":9}");
        TestAppointment completed = appointment("{\"appointmentDate\":\"2026-10-15\",\"testResult\":false}");
        assertEquals(AppointmentsTextMapper.Status.UPCOMING, mapper.status(locked, NOW));
        assertEquals(Arrays.asList(upcoming, locked, completed), mapper.sortedAppointments(Arrays.asList(locked, completed, upcoming), NOW));
    }

    @Test
    public void invalidDatesAreLastAndNullEntriesAreSkipped() {
        TestAppointment invalid = appointment("{\"appointmentDate\":\"bad\"}");
        TestAppointment missing = appointment("{}");
        TestAppointment valid = appointment("{\"appointmentDate\":\"2026-10-09\"}");
        assertEquals(Arrays.asList(valid, invalid, missing), mapper.sortedAppointments(Arrays.asList(invalid, null, missing, valid), NOW));
        assertTrue(mapper.sortedAppointments(null, NOW).isEmpty());
    }

    @Test
    public void datesIncludeTimeAndUsePhoneZoneWhileNullAndInvalidAreDashes() {
        String date = mapper.dateTime("2026-10-09T15:30:00+03:00");
        assertEquals(date, mapper.dateTime("2026-10-09T12:30:00Z"));
        assertEquals(date, mapper.dateTime("2026-10-09T15:30:00"));
        assertTrue(date.matches(".*\\p{Nd}{1,2}:\\p{Nd}{2}.*"));
        assertTrue(date.contains("2026"));
        assertEquals("—", mapper.dateTime(null));
        assertEquals("—", mapper.dateTime("bad"));
    }

    @Test
    public void feesHaveTwoDecimalsWithoutCurrencyAndInvalidValuesAreDashes() {
        assertEquals("25.50", mapper.fees(25.5));
        assertEquals("0.00", mapper.fees(0.0));
        for (Double value : new Double[] {null, -1.0, Double.NaN, Double.POSITIVE_INFINITY}) {
            assertEquals("—", mapper.fees(value));
        }
    }

    @Test
    public void cardsReuseArabicClassMappingAndHandleNullableFields() {
        assertEquals("فئة الرخصة: —\nتاريخ الموعد: —\nرسوم الموعد: —", mapper.details(appointment("{}")));
        assertTrue(mapper.details(appointment("{\"className\":\"Class 3 - Ordinary driving license\"}"))
                .startsWith("فئة الرخصة: الفئة الثالثة - رخصة قيادة عادية"));
        assertTrue(mapper.details(appointment("{\"className\":\"FutureClass\"}")).startsWith("فئة الرخصة: FutureClass"));
        assertEquals("رقم موعد الاختبار: —", mapper.appointmentId(0));
        assertEquals("رقم موعد الاختبار: 18", mapper.appointmentId(18));
    }

    @Test
    public void phoneAppointment143At1817IsAwaitingResultAt1832AndAtExactAppointmentTime() {
        TestAppointment appointment = appointment("{\"testAppointmentID\":143,\"appointmentDate\":\"2026-10-10T18:17:00\",\"testResult\":null}");
        Instant atAppointment = ApiDateTime.parse(appointment.getAppointmentDate(), ZONE);
        assertEquals(AppointmentsTextMapper.Status.UPCOMING, mapper.status(appointment, atAppointment.minusSeconds(1)));
        assertEquals(AppointmentsTextMapper.Status.AWAITING_RESULT, mapper.status(appointment, atAppointment));
        assertEquals("بانتظار تسجيل النتيجة", mapper.statusText(mapper.status(appointment,
                ApiDateTime.parse("2026-10-10T18:32:00", ZONE))));
    }

    @Test
    public void recordedResultsOverrideBothFutureAndInvalidDates() {
        for (String date : new String[] {"2026-10-11", "bad"}) {
            assertEquals(AppointmentsTextMapper.Status.PASSED, mapper.status(true, date, NOW));
            assertEquals(AppointmentsTextMapper.Status.FAILED, mapper.status(false, date, NOW));
        }
    }

    @Test
    public void allGroupsSortAwaitingThenUpcomingThenPassedThenFailedThenUnknown() {
        TestAppointment awaiting = appointment("{\"appointmentDate\":\"2026-10-08\"}");
        TestAppointment upcoming = appointment("{\"appointmentDate\":\"2026-10-11\"}");
        TestAppointment passed = appointment("{\"appointmentDate\":\"bad\",\"testResult\":true}");
        TestAppointment failed = appointment("{\"appointmentDate\":\"2026-10-09\",\"testResult\":false}");
        TestAppointment unknown = appointment("{}");
        assertEquals(Arrays.asList(awaiting, upcoming, passed, failed, unknown),
                mapper.sortedAppointments(Arrays.asList(unknown, failed, passed, upcoming, awaiting), NOW));
    }

    @Test
    public void passedAndFailedGroupsSortNewestFirstWithMissingAndInvalidDatesLastInTheirGroup() {
        TestAppointment passedOld = appointment("{\"appointmentDate\":\"2026-10-07\",\"testResult\":true}");
        TestAppointment passedNew = appointment("{\"appointmentDate\":\"2026-10-09\",\"testResult\":true}");
        TestAppointment passedMissing = appointment("{\"testResult\":true}");
        TestAppointment failedOld = appointment("{\"appointmentDate\":\"2026-10-08\",\"testResult\":false}");
        TestAppointment failedNew = appointment("{\"appointmentDate\":\"2026-10-09\",\"testResult\":false}");
        TestAppointment failedInvalid = appointment("{\"appointmentDate\":\"bad\",\"testResult\":false}");
        List<TestAppointment> input = Arrays.asList(failedOld, passedMissing, passedOld, failedInvalid, failedNew, passedNew);
        assertEquals(Arrays.asList(passedNew, passedOld, passedMissing, failedNew, failedOld, failedInvalid),
                mapper.sortedAppointments(input, NOW));
    }

    @Test
    public void allFiveFiltersOnlyReturnTheirMatchingStatusAndKeepSortedOrder() {
        TestAppointment awaitingOld = appointment("{\"appointmentDate\":\"2026-10-08\"}");
        TestAppointment awaitingNew = appointment("{\"appointmentDate\":\"2026-10-09\"}");
        TestAppointment upcomingNear = appointment("{\"appointmentDate\":\"2026-10-11\"}");
        TestAppointment upcomingFar = appointment("{\"appointmentDate\":\"2026-10-12\"}");
        TestAppointment passed = appointment("{\"testResult\":true}");
        TestAppointment failed = appointment("{\"testResult\":false}");
        TestAppointment unknown = appointment("{}");
        List<TestAppointment> input = Arrays.asList(unknown, upcomingFar, failed, awaitingOld, passed, upcomingNear, awaitingNew, null);
        assertEquals(Arrays.asList(awaitingNew, awaitingOld, upcomingNear, upcomingFar, passed, failed, unknown),
                mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.ALL, NOW));
        assertEquals(Arrays.asList(upcomingNear, upcomingFar), mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.UPCOMING, NOW));
        assertEquals(Arrays.asList(awaitingNew, awaitingOld), mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.AWAITING_RESULT, NOW));
        assertEquals(Arrays.asList(passed), mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.PASSED, NOW));
        assertEquals(Arrays.asList(failed), mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.FAILED, NOW));
        assertEquals(unknown, input.get(0));
        assertEquals(8, input.size());
    }

    @Test
    public void emptyFilterResultsHaveTheirOwnArabicMessageIncludingWhenOtherStatusesExist() {
        AppointmentsTextMapper.Filter[] filters = AppointmentsTextMapper.Filter.values();
        String[] messages = {"لا توجد مواعيد أو اختبارات مسجلة", "لا توجد مواعيد قادمة",
                "لا توجد اختبارات بانتظار تسجيل النتيجة", "لا توجد اختبارات ناجحة", "لا توجد اختبارات راسبة"};
        for (int index = 0; index < filters.length; index++) {
            assertTrue(mapper.visibleAppointments(null, filters[index], NOW).isEmpty());
            assertTrue(mapper.visibleAppointments(Arrays.asList((TestAppointment) null), filters[index], NOW).isEmpty());
            assertEquals(messages[index], mapper.emptyMessage(filters[index]));
            if (filters[index] != AppointmentsTextMapper.Filter.ALL) {
                assertTrue(mapper.visibleAppointments(Arrays.asList(appointment("{}")), filters[index], NOW).isEmpty());
            }
        }
    }

    @Test
    public void filteringReevaluatesTimeWhenUpcomingAppointmentBecomesAwaitingResult() {
        TestAppointment appointment = appointment("{\"appointmentDate\":\"2026-10-10T18:17:00\"}");
        List<TestAppointment> input = Arrays.asList(appointment);
        Instant date = ApiDateTime.parse(appointment.getAppointmentDate(), ZONE);
        assertEquals(input, mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.UPCOMING, date.minusSeconds(1)));
        assertTrue(mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.UPCOMING, date).isEmpty());
        assertEquals(input, mapper.visibleAppointments(input, AppointmentsTextMapper.Filter.AWAITING_RESULT, date));
    }

    private TestAppointment appointment(String json) { return gson.fromJson(json, TestAppointment.class); }
}
