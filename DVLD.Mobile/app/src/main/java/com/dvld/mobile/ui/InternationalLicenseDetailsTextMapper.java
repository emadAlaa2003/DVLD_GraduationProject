package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.InternationalLicenseDetails;

import java.time.Instant;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.Locale;

public final class InternationalLicenseDetailsTextMapper {
    public enum Status { ACTIVE, EXPIRED, INACTIVE, UNKNOWN }

    private final DashboardTextMapper.Strings strings;
    private final ZoneId zone;
    private final DateTimeFormatter dates;

    public InternationalLicenseDetailsTextMapper(DashboardTextMapper.Strings strings, ZoneId zone) {
        this.strings = strings;
        this.zone = zone;
        dates = DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM)
                .withLocale(Locale.forLanguageTag("ar")).withZone(zone);
    }

    public Status status(InternationalLicenseDetails details) {
        // The server's validity flag is authoritative; IsActive alone cannot imply validity.
        if (Boolean.TRUE.equals(details.getIsCurrentlyValid())) return Status.ACTIVE;
        if (Boolean.TRUE.equals(details.getIsExpired())) return Status.EXPIRED;
        if (Boolean.FALSE.equals(details.getIsActive())) return Status.INACTIVE;
        return Status.UNKNOWN;
    }

    public String statusText(InternationalLicenseDetails details) {
        switch (status(details)) {
            case ACTIVE: return strings.get(R.string.licenses_active);
            case EXPIRED: return strings.get(R.string.license_details_expired);
            case INACTIVE: return strings.get(R.string.licenses_inactive);
            default: return unknown();
        }
    }

    public String applicationStatus(Integer status) {
        if (status != null) {
            switch (status) {
                case 1: return strings.get(R.string.dashboard_status_new);
                case 2: return strings.get(R.string.dashboard_status_cancelled);
                case 3: return strings.get(R.string.dashboard_status_completed);
                default: break;
            }
        }
        return unknown();
    }

    public String licenseId(InternationalLicenseDetails details) {
        return strings.get(R.string.licenses_international_id, id(details.getInternationalLicenseID()));
    }

    public String citizenInformation(InternationalLicenseDetails details) {
        return strings.get(R.string.license_details_full_name, text(details.getFullName())) + "\n"
                + strings.get(R.string.license_details_national_no, text(details.getNationalNo()));
    }

    public String licenseInformation(InternationalLicenseDetails details) {
        return licenseId(details) + "\n"
                + strings.get(R.string.license_details_application_id, id(details.getApplicationID())) + "\n"
                + strings.get(R.string.international_license_details_driver_id, id(details.getDriverID())) + "\n"
                + strings.get(R.string.licenses_issued_using_local_id, id(details.getIssuedUsingLocalLicenseID())) + "\n"
                + strings.get(R.string.licenses_issue_date, date(details.getIssueDate())) + "\n"
                + strings.get(R.string.licenses_expiration_date, date(details.getExpirationDate())) + "\n"
                + strings.get(R.string.license_details_paid_fees, fees(details.getPaidFees())) + "\n"
                + strings.get(R.string.license_details_status, statusText(details));
    }

    public String applicationInformation(InternationalLicenseDetails details) {
        return strings.get(R.string.dashboard_application_date, date(details.getApplicationDate())) + "\n"
                + strings.get(R.string.dashboard_application_status, applicationStatus(details.getApplicationStatus()));
    }

    public String fees(Double fees) {
        return fees == null || !Double.isFinite(fees) || fees < 0
                ? unknown() : String.format(Locale.ROOT, "%.2f", fees);
    }

    private String date(String date) {
        Instant instant = ApiDateTime.parse(date, zone);
        return instant == null ? unknown() : dates.format(instant);
    }

    private String id(Integer id) {
        return id == null || id <= 0 ? unknown() : String.valueOf(id);
    }

    private String text(String text) {
        return text == null || text.trim().isEmpty() ? unknown() : text;
    }

    private String unknown() {
        return strings.get(R.string.dashboard_unknown_value);
    }
}
