package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.ApplicationDetails;
import com.dvld.mobile.model.ApplicationSpecificDetails;

import java.time.Instant;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.Locale;

public final class ApplicationDetailsTextMapper {
    private final DashboardTextMapper.Strings strings;
    private final DashboardTextMapper dashboardMapper;
    private final ApplicationsTextMapper applicationsMapper;
    private final LocalLicenseDetailsTextMapper localLicenseMapper;
    private final ZoneId zone;

    public ApplicationDetailsTextMapper(DashboardTextMapper.Strings strings, ZoneId zone) {
        this.strings = strings;
        this.zone = zone;
        dashboardMapper = new DashboardTextMapper(strings);
        applicationsMapper = new ApplicationsTextMapper(strings, zone);
        localLicenseMapper = new LocalLicenseDetailsTextMapper(strings, zone);
    }

    public String applicationId(ApplicationDetails application) {
        return strings.get(R.string.applications_application_id, id(application.getApplicationID()));
    }

    public String applicationType(ApplicationDetails application) {
        return text(dashboardMapper.applicationType(application.getApplicationTypeID(), application.getApplicationTypeName()));
    }

    public ApplicationsTextMapper.Status status(ApplicationDetails application) {
        return applicationsMapper.status(application.getApplicationStatus(), application.getStatusText());
    }

    public String statusText(ApplicationDetails application) {
        return applicationsMapper.statusText(application.getApplicationStatus(), application.getStatusText());
    }

    public String commonInformation(ApplicationDetails application) {
        return applicationId(application) + "\n"
                + strings.get(R.string.dashboard_application_type, applicationType(application)) + "\n"
                + strings.get(R.string.dashboard_application_status, statusText(application)) + "\n"
                + strings.get(R.string.application_details_submission_date, applicationsMapper.date(application.getApplicationDate())) + "\n"
                + strings.get(R.string.applications_last_status_date, applicationsMapper.date(application.getLastStatusDate())) + "\n"
                + strings.get(R.string.application_details_fees, applicationsMapper.fees(application.getPaidFees()));
    }

    public String additionalInformation(ApplicationDetails application) {
        ApplicationSpecificDetails details = application.getDetails();
        if (details == null) return strings.get(R.string.application_details_no_extra);
        StringBuilder lines = new StringBuilder();
        int type = application.getApplicationTypeID() == null ? 0 : application.getApplicationTypeID();
        switch (type) {
            case 1:
                addId(lines, R.string.application_details_local_application_id, details.getLocalDrivingLicenseApplicationID());
                addClass(lines, details.getClassName());
                break;
            case 2:
            case 3:
            case 4:
                addId(lines, R.string.licenses_local_id, details.getLicenseID());
                addClass(lines, details.getClassName());
                addDate(lines, R.string.licenses_issue_date, details.getIssueDate());
                addDate(lines, R.string.licenses_expiration_date, details.getExpirationDate());
                if (details.getIsActive() != null) {
                    add(lines, R.string.license_details_status, strings.get(details.getIsActive()
                            ? R.string.licenses_active : R.string.licenses_inactive));
                }
                if (details.getIssueReason() != null || hasText(details.getIssueReasonText())) {
                    add(lines, R.string.license_details_issue_reason,
                            localLicenseMapper.issueReason(details.getIssueReason(), details.getIssueReasonText()));
                }
                break;
            case 5:
                addId(lines, R.string.application_details_detain_id, details.getDetainID());
                addId(lines, R.string.licenses_local_id, details.getLicenseID());
                addDate(lines, R.string.application_details_detain_date, details.getDetainDate());
                addFees(lines, R.string.application_details_fine_fees, details.getFineFees());
                if (lines.length() > 0 || details.getIsReleased() != null
                        || details.getReleaseDate() != null || details.getReleaseApplicationID() != null) {
                    add(lines, R.string.licenses_detained_status, releaseStatus(details.getIsReleased()));
                }
                addDate(lines, R.string.application_details_release_date, details.getReleaseDate());
                addId(lines, R.string.application_details_release_application_id, details.getReleaseApplicationID());
                break;
            case 6:
                addId(lines, R.string.licenses_international_id, details.getInternationalLicenseID());
                addId(lines, R.string.international_license_details_driver_id, details.getDriverID());
                addId(lines, R.string.licenses_issued_using_local_id, details.getIssuedUsingLocalLicenseID());
                addDate(lines, R.string.licenses_issue_date, details.getIssueDate());
                addDate(lines, R.string.licenses_expiration_date, details.getExpirationDate());
                if (lines.length() > 0 || details.getIsCurrentlyValid() != null
                        || details.getIsActive() != null || details.getIsExpired() != null) {
                    add(lines, R.string.licenses_active_status, internationalValidity(details));
                }
                break;
            case 7:
                addId(lines, R.string.application_details_test_appointment_id, details.getTestAppointmentID());
                if (details.getTestTypeID() != null || hasText(details.getTestTypeName())) {
                    add(lines, R.string.application_details_test_type, testType(details.getTestTypeID(), details.getTestTypeName()));
                }
                addId(lines, R.string.application_details_local_application_id, details.getLocalDrivingLicenseApplicationID());
                if (details.getAppointmentDate() != null) {
                    add(lines, R.string.application_details_appointment_date, appointmentDate(details.getAppointmentDate()));
                }
                addFees(lines, R.string.application_details_appointment_fees, details.getAppointmentPaidFees());
                if (lines.length() > 0 || details.getIsLocked() != null || details.getTestID() != null) {
                    add(lines, R.string.application_details_appointment_status, appointmentStatus(details.getIsLocked()));
                }
                addId(lines, R.string.application_details_test_id, details.getTestID());
                break;
            default: break;
        }
        return lines.length() == 0 ? strings.get(R.string.application_details_no_extra) : lines.toString();
    }

    public String internationalValidity(ApplicationSpecificDetails details) {
        if (Boolean.TRUE.equals(details.getIsCurrentlyValid())) return strings.get(R.string.licenses_active);
        if (Boolean.TRUE.equals(details.getIsExpired())) return strings.get(R.string.license_details_expired);
        if (Boolean.FALSE.equals(details.getIsCurrentlyValid()) || Boolean.FALSE.equals(details.getIsActive())) {
            return strings.get(R.string.licenses_inactive);
        }
        return unknown();
    }

    public String releaseStatus(Boolean released) {
        return released == null ? unknown() : strings.get(released
                ? R.string.application_details_released : R.string.application_details_still_detained);
    }

    public String appointmentStatus(Boolean locked) {
        return locked == null ? unknown() : strings.get(locked
                ? R.string.application_details_appointment_locked : R.string.application_details_appointment_unlocked);
    }

    private String testType(Integer type, String backendText) {
        if (type != null) {
            switch (type) {
                case 1: return strings.get(R.string.application_details_vision_test);
                case 2: return strings.get(R.string.application_details_written_test);
                case 3: return strings.get(R.string.application_details_street_test);
                default: break;
            }
        }
        if (backendText != null) {
            switch (backendText.trim()) {
                case "VisionTest": return strings.get(R.string.application_details_vision_test);
                case "WrittenTest": return strings.get(R.string.application_details_written_test);
                case "StreetTest": return strings.get(R.string.application_details_street_test);
                default: break;
            }
        }
        return text(backendText);
    }

    private String appointmentDate(String value) {
        Instant instant = ApiDateTime.parse(value, zone);
        return instant == null ? unknown() : DateTimeFormatter.ofLocalizedDateTime(FormatStyle.MEDIUM)
                .withLocale(Locale.forLanguageTag("ar")).withZone(zone).format(instant);
    }

    private void addClass(StringBuilder lines, String value) {
        if (hasText(value)) add(lines, R.string.license_details_class, dashboardMapper.licenseClass(value));
    }

    private void addId(StringBuilder lines, int resource, Integer value) {
        if (value != null) add(lines, resource, id(value));
    }

    private void addDate(StringBuilder lines, int resource, String value) {
        if (value != null) add(lines, resource, applicationsMapper.date(value));
    }

    private void addFees(StringBuilder lines, int resource, Double value) {
        if (value != null) add(lines, resource, applicationsMapper.fees(value));
    }

    private void add(StringBuilder lines, int resource, String value) {
        if (lines.length() > 0) lines.append('\n');
        lines.append(strings.get(resource, value));
    }

    private String id(Integer value) {
        return value == null || value <= 0 ? unknown() : String.valueOf(value);
    }

    private String text(String value) {
        return hasText(value) ? value : unknown();
    }

    private String unknown() {
        return strings.get(R.string.dashboard_unknown_value);
    }

    private static boolean hasText(String value) {
        return value != null && !value.trim().isEmpty();
    }
}
