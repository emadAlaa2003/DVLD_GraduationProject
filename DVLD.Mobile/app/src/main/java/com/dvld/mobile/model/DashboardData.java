package com.dvld.mobile.model;

import java.time.Instant;
import java.time.ZoneId;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/** A complete snapshot; local and international licenses retain their separate types. */
public final class DashboardData {
    private final List<LocalLicense> localLicenses;
    private final List<InternationalLicense> internationalLicenses;
    private final List<CitizenApplication> applications;
    private final List<TestAppointment> appointments;

    public DashboardData(List<LocalLicense> localLicenses,
                         List<InternationalLicense> internationalLicenses,
                         List<CitizenApplication> applications,
                         List<TestAppointment> appointments) {
        this.localLicenses = snapshot(localLicenses);
        this.internationalLicenses = snapshot(internationalLicenses);
        this.applications = snapshot(applications);
        this.appointments = snapshot(appointments);
    }

    private static <T> List<T> snapshot(List<T> items) {
        List<T> result = new ArrayList<>();
        for (T item : items) {
            if (item != null) {
                result.add(item);
            }
        }
        return Collections.unmodifiableList(result);
    }

    public List<LocalLicense> getLocalLicenses() {
        return localLicenses;
    }

    public List<InternationalLicense> getInternationalLicenses() {
        return internationalLicenses;
    }

    public List<CitizenApplication> getApplications() {
        return applications;
    }

    public int getRegisteredLicenseCount() {
        return localLicenses.size() + internationalLicenses.size();
    }

    public int getActiveApplicationCount() {
        int count = 0;
        for (CitizenApplication application : applications) {
            if (Integer.valueOf(1).equals(application.getApplicationStatus())) {
                count++;
            }
        }
        return count;
    }

    public CitizenApplication getLatestApplication(ZoneId zone) {
        CitizenApplication latest = null;
        Instant latestDate = null;
        for (CitizenApplication application : applications) {
            Instant date = ApiDateTime.parse(application.getApplicationDate(), zone);
            if (date != null && (latestDate == null || date.isAfter(latestDate))) {
                latest = application;
                latestDate = date;
            }
        }
        return latest;
    }

    public TestAppointment getUpcomingAppointment(Instant now, ZoneId zone) {
        TestAppointment upcoming = null;
        Instant upcomingDate = null;
        for (TestAppointment appointment : appointments) {
            if (Boolean.TRUE.equals(appointment.getIsLocked()) ||
                    appointment.getTestID() != null || appointment.getTestResult() != null) {
                continue;
            }
            Instant date = ApiDateTime.parse(appointment.getAppointmentDate(), zone);
            if (date != null && date.isAfter(now) &&
                    (upcomingDate == null || date.isBefore(upcomingDate))) {
                upcoming = appointment;
                upcomingDate = date;
            }
        }
        return upcoming;
    }
}
