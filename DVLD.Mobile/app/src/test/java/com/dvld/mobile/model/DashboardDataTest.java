package com.dvld.mobile.model;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;

import org.junit.Test;

import java.time.Instant;
import java.time.ZoneId;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;

import static org.junit.Assert.*;

public class DashboardDataTest {
    private final Gson gson = new Gson();
    private final ZoneId zone = ZoneId.of("Asia/Hebron");

    @Test
    public void emptyAndNullableEntriesAreSafeAndCountsAreZero() {
        DashboardData data = new DashboardData(Arrays.asList((LocalLicense) null),
                Collections.emptyList(), Collections.emptyList(), Collections.emptyList());
        assertEquals(0, data.getRegisteredLicenseCount());
        assertEquals(0, data.getActiveApplicationCount());
        assertNull(data.getLatestApplication(zone));
        assertNull(data.getUpcomingAppointment(Instant.now(), zone));
    }

    @Test
    public void countsReturnedLocalAndInternationalListsSeparately() {
        LocalLicense local = gson.fromJson("{\"licenseID\":7,\"className\":null}", LocalLicense.class);
        InternationalLicense international = gson.fromJson(
                "{\"internationalLicenseID\":9,\"issuedUsingLocalLicenseID\":7}", InternationalLicense.class);
        DashboardData data = new DashboardData(Arrays.asList(local, local),
                Collections.singletonList(international), Collections.emptyList(), Collections.emptyList());
        assertEquals(2, data.getLocalLicenses().size());
        assertEquals(1, data.getInternationalLicenses().size());
        assertEquals(3, data.getRegisteredLicenseCount());
    }

    @Test
    public void onlyNewApplicationsAreActiveAndLatestIsSelectedByDate() {
        List<CitizenApplication> applications = gson.fromJson("["
                + "{\"applicationID\":1,\"applicationStatus\":1,\"applicationDate\":\"2026-10-08T10:00:00\"},"
                + "{\"applicationID\":2,\"applicationStatus\":3,\"applicationDate\":\"2026-10-09T09:00:00\",\"className\":null},"
                + "{\"applicationID\":3,\"applicationStatus\":2,\"applicationDate\":\"2026-10-07T10:00:00\"},"
                + "{\"applicationID\":4,\"applicationStatus\":1,\"applicationDate\":null},"
                + "{\"applicationID\":5,\"applicationStatus\":null,\"applicationDate\":\"invalid\"}]",
                new TypeToken<List<CitizenApplication>>() { }.getType());
        DashboardData data = new DashboardData(Collections.emptyList(), Collections.emptyList(),
                applications, Collections.emptyList());
        assertEquals(2, data.getActiveApplicationCount());
        assertEquals(Integer.valueOf(2), data.getLatestApplication(zone).getApplicationID());
        assertNull(data.getLatestApplication(zone).getClassName());
    }

    @Test
    public void missingDatesDoNotInventAMostRecentApplication() {
        CitizenApplication undated = gson.fromJson("{\"applicationID\":1}", CitizenApplication.class);
        DashboardData data = new DashboardData(Collections.emptyList(), Collections.emptyList(),
                Collections.singletonList(undated), Collections.emptyList());
        assertEquals(1, data.getApplications().size());
        assertNull(data.getLatestApplication(zone));
    }

    @Test
    public void selectsNearestFutureOpenAppointmentAndIgnoresCompletedAndPast() {
        List<TestAppointment> appointments = gson.fromJson("["
                + "{\"testAppointmentID\":1,\"appointmentDate\":\"2026-10-12T09:00:00Z\"},"
                + "{\"testAppointmentID\":2,\"appointmentDate\":\"2026-10-09T11:00:00Z\",\"testID\":null,\"testResult\":null,\"notes\":null},"
                + "{\"testAppointmentID\":3,\"appointmentDate\":\"2026-10-09T10:30:00Z\",\"testID\":8},"
                + "{\"testAppointmentID\":4,\"appointmentDate\":\"2026-10-09T10:40:00Z\",\"testResult\":false},"
                + "{\"testAppointmentID\":5,\"appointmentDate\":\"2026-10-09T10:50:00Z\",\"isLocked\":true},"
                + "{\"testAppointmentID\":6,\"appointmentDate\":\"2026-10-09T09:00:00Z\"},"
                + "{\"testAppointmentID\":7,\"appointmentDate\":null}]",
                new TypeToken<List<TestAppointment>>() { }.getType());
        DashboardData data = new DashboardData(Collections.emptyList(), Collections.emptyList(),
                Collections.emptyList(), appointments);
        assertEquals(Integer.valueOf(2), data.getUpcomingAppointment(
                Instant.parse("2026-10-09T10:00:00Z"), zone).getTestAppointmentID());
        assertNull(data.getUpcomingAppointment(Instant.parse("2026-11-01T00:00:00Z"), zone));
    }

    @Test
    public void parsesAspNetDatesWithOffsetsFractionalSecondsAndLocalTimes() {
        assertEquals(Instant.parse("2026-10-09T10:00:00Z"),
                ApiDateTime.parse("2026-10-09T13:00:00+03:00", zone));
        assertEquals(Instant.parse("2026-10-09T10:00:00Z"),
                ApiDateTime.parse("2026-10-09T13:00:00", zone));
        assertEquals(Instant.parse("2026-10-09T10:00:00.123456700Z"),
                ApiDateTime.parse("2026-10-09T10:00:00.1234567Z", zone));
        assertNotNull(ApiDateTime.parse("2026-10-09", zone));
        assertNull(ApiDateTime.parse("not-a-date", zone));
        assertNull(ApiDateTime.parse(null, zone));
    }
}
