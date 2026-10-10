package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.TestAppointment;

import java.time.Instant;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Locale;

public final class AppointmentsTextMapper {
    public enum Status { AWAITING_RESULT, UPCOMING, PASSED, FAILED, UNKNOWN }
    public enum Filter { ALL, UPCOMING, AWAITING_RESULT, PASSED, FAILED }
    public enum ViewMode {
        APPOINTMENTS, TESTS_AND_RESULTS;

        public static ViewMode fromValue(String value) {
            return TESTS_AND_RESULTS.name().equals(value) ? TESTS_AND_RESULTS : APPOINTMENTS;
        }
    }

    private final DashboardTextMapper.Strings strings;
    private final DashboardTextMapper dashboardMapper;
    private final ApplicationsTextMapper valueMapper;
    private final ZoneId zone;
    private final DateTimeFormatter dates;

    public AppointmentsTextMapper(DashboardTextMapper.Strings strings, ZoneId zone) {
        this.strings = strings;
        this.zone = zone;
        dashboardMapper = new DashboardTextMapper(strings);
        valueMapper = new ApplicationsTextMapper(strings, zone);
        dates = DateTimeFormatter.ofLocalizedDateTime(FormatStyle.MEDIUM)
                .withLocale(Locale.forLanguageTag("ar")).withZone(zone);
    }

    public String testType(Integer id, String backendTitle) {
        if (id != null) {
            switch (id) {
                case 1: return strings.get(R.string.appointments_vision_test);
                case 2: return strings.get(R.string.appointments_written_test);
                case 3: return strings.get(R.string.appointments_street_test);
                default: break;
            }
        }
        return text(backendTitle);
    }

    public String appointmentId(Integer id) {
        return strings.get(R.string.application_details_test_appointment_id, id(id));
    }

    public Status status(TestAppointment appointment, Instant now) {
        return status(appointment.getTestResult(), appointment.getAppointmentDate(), now);
    }

    public Status status(Boolean result, String date, Instant now) {
        if (result != null) return result ? Status.PASSED : Status.FAILED;
        Instant instant = ApiDateTime.parse(date, zone);
        if (instant == null) return Status.UNKNOWN;
        return instant.isAfter(now) ? Status.UPCOMING : Status.AWAITING_RESULT;
    }

    public String statusText(Status status) {
        switch (status) {
            case PASSED: return strings.get(R.string.appointments_passed);
            case FAILED: return strings.get(R.string.appointments_failed);
            case UPCOMING: return strings.get(R.string.appointments_upcoming);
            case AWAITING_RESULT: return strings.get(R.string.appointments_awaiting_result);
            default: return unknown();
        }
    }

    public String resultText(Boolean result) {
        return result == null ? unknown() : strings.get(result ? R.string.appointments_passed : R.string.appointments_failed);
    }

    public String details(TestAppointment appointment) {
        String result = strings.get(R.string.license_details_class, text(dashboardMapper.licenseClass(appointment.getClassName())))
                + "\n" + strings.get(R.string.application_details_appointment_date, dateTime(appointment.getAppointmentDate()))
                + "\n" + strings.get(R.string.application_details_appointment_fees, fees(appointment.getPaidFees()));
        if (appointment.getTestResult() != null) {
            result += "\n" + strings.get(R.string.appointments_result, resultText(appointment.getTestResult()));
        }
        return result;
    }

    public String details(TestAppointment appointment, ViewMode mode) {
        if (mode == ViewMode.APPOINTMENTS) return details(appointment);
        String result = strings.get(R.string.license_details_class, text(dashboardMapper.licenseClass(appointment.getClassName())))
                + "\n" + strings.get(R.string.tests_results_test_date, dateTime(appointment.getAppointmentDate()));
        if (appointment.getTestID() != null && appointment.getTestID() > 0) {
            result += "\n" + strings.get(R.string.application_details_test_id, id(appointment.getTestID()));
        }
        if (appointment.getPaidFees() != null) {
            result += "\n" + strings.get(R.string.application_details_appointment_fees, fees(appointment.getPaidFees()));
        }
        return result;
    }

    public static Filter filterForMode(Filter filter, ViewMode mode) {
        return filter == null || (mode == ViewMode.TESTS_AND_RESULTS && filter == Filter.UPCOMING)
                ? Filter.ALL : filter;
    }

    public List<TestAppointment> sortedAppointments(List<TestAppointment> appointments, Instant now) {
        return visibleAppointments(appointments, Filter.ALL, now);
    }

    public List<TestAppointment> visibleAppointments(List<TestAppointment> appointments, Filter filter, Instant now) {
        return visibleAppointments(appointments, filter, ViewMode.APPOINTMENTS, now);
    }

    public List<TestAppointment> visibleAppointments(List<TestAppointment> appointments, Filter filter, ViewMode mode, Instant now) {
        List<TestAppointment> sorted = new ArrayList<>();
        if (appointments != null) {
            for (TestAppointment appointment : appointments) {
                if (appointment == null) continue;
                Status status = status(appointment, now);
                if (matchesMode(status, mode) && matches(status, filter)) sorted.add(appointment);
            }
        }
        sorted.sort((left, right) -> {
            Status leftStatus = status(left, now);
            Status rightStatus = status(right, now);
            int groupOrder = Integer.compare(leftStatus.ordinal(), rightStatus.ordinal());
            if (groupOrder != 0) return groupOrder;
            Instant leftDate = ApiDateTime.parse(left.getAppointmentDate(), zone);
            Instant rightDate = ApiDateTime.parse(right.getAppointmentDate(), zone);
            if (leftDate == null) return rightDate == null ? 0 : 1;
            if (rightDate == null) return -1;
            return leftStatus == Status.UPCOMING ? leftDate.compareTo(rightDate) : rightDate.compareTo(leftDate);
        });
        return Collections.unmodifiableList(sorted);
    }

    private boolean matchesMode(Status status, ViewMode mode) {
        return mode == ViewMode.APPOINTMENTS || status == Status.AWAITING_RESULT
                || status == Status.PASSED || status == Status.FAILED;
    }

    private boolean matches(Status status, Filter filter) {
        switch (filter) {
            case UPCOMING: return status == Status.UPCOMING;
            case AWAITING_RESULT: return status == Status.AWAITING_RESULT;
            case PASSED: return status == Status.PASSED;
            case FAILED: return status == Status.FAILED;
            default: return true;
        }
    }

    public String emptyMessage(Filter filter) {
        return emptyMessage(filter, ViewMode.APPOINTMENTS);
    }

    public String emptyMessage(Filter filter, ViewMode mode) {
        switch (filter) {
            case UPCOMING: return strings.get(R.string.appointments_empty_upcoming);
            case AWAITING_RESULT: return strings.get(R.string.appointments_empty_awaiting_result);
            case PASSED: return strings.get(R.string.appointments_empty_passed);
            case FAILED: return strings.get(R.string.appointments_empty_failed);
            default: return strings.get(mode == ViewMode.TESTS_AND_RESULTS
                    ? R.string.tests_results_empty : R.string.appointments_empty);
        }
    }

    public String dateTime(String value) {
        Instant date = ApiDateTime.parse(value, zone);
        return date == null ? unknown() : dates.format(date);
    }

    public String fees(Double value) { return valueMapper.fees(value); }
    public String id(Integer value) { return value == null || value <= 0 ? unknown() : String.valueOf(value); }
    private String text(String value) { return value == null || value.trim().isEmpty() ? unknown() : value; }
    private String unknown() { return strings.get(R.string.dashboard_unknown_value); }
}
