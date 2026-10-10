package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.TestAppointmentDetails;
import com.dvld.mobile.model.TestResultDetails;

import java.time.Instant;
import java.time.ZoneId;

public final class TestAppointmentDetailsTextMapper {
    private final DashboardTextMapper.Strings strings;
    private final AppointmentsTextMapper appointmentsMapper;

    public TestAppointmentDetailsTextMapper(DashboardTextMapper.Strings strings, ZoneId zone) {
        this.strings = strings;
        appointmentsMapper = new AppointmentsTextMapper(strings, zone);
    }

    public String appointmentId(TestAppointmentDetails details) {
        return appointmentsMapper.appointmentId(details.getTestAppointmentID());
    }

    public String testType(TestAppointmentDetails details) {
        return appointmentsMapper.testType(details.getTestTypeID(), details.getTestTypeName());
    }

    public AppointmentsTextMapper.Status status(TestAppointmentDetails details, Instant now) {
        TestResultDetails test = details.getTest();
        return appointmentsMapper.status(test == null ? null : test.getTestResult(),
                details.getAppointmentDate(), now);
    }

    public String statusText(TestAppointmentDetails details, Instant now) {
        return appointmentsMapper.statusText(status(details, now));
    }

    public String commonInformation(TestAppointmentDetails details, Instant now) {
        String result = appointmentId(details) + "\n"
                + strings.get(R.string.application_details_test_type, testType(details)) + "\n"
                + strings.get(R.string.application_details_local_application_id, appointmentsMapper.id(details.getLocalDrivingLicenseApplicationID())) + "\n"
                + strings.get(R.string.application_details_appointment_date, appointmentsMapper.dateTime(details.getAppointmentDate())) + "\n"
                + strings.get(R.string.application_details_appointment_fees, appointmentsMapper.fees(details.getPaidFees())) + "\n"
                + strings.get(R.string.appointments_status, statusText(details, now));
        if (details.getRetakeTestApplicationID() != null) {
            result += "\n" + strings.get(R.string.test_appointment_details_retake_id, appointmentsMapper.id(details.getRetakeTestApplicationID()));
        }
        return result;
    }

    public String testInformation(TestAppointmentDetails details) {
        TestResultDetails test = details.getTest();
        if (test == null) return strings.get(R.string.test_appointment_details_no_result);
        String result = strings.get(R.string.application_details_test_id, appointmentsMapper.id(test.getTestID())) + "\n"
                + strings.get(R.string.appointments_result, appointmentsMapper.resultText(test.getTestResult()));
        if (test.getNotes() != null && !test.getNotes().trim().isEmpty()) {
            result += "\n" + strings.get(R.string.test_appointment_details_notes, test.getNotes());
        }
        return result;
    }
}
