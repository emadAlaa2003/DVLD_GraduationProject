package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.LocalLicenseDetails;

import java.time.Instant;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.Locale;

public final class LocalLicenseDetailsTextMapper {
    public enum Status { ACTIVE, EXPIRED, INACTIVE, UNKNOWN }

    private final DashboardTextMapper.Strings strings;
    private final DashboardTextMapper dashboardText;
    private final ZoneId zone;
    private final DateTimeFormatter dates;

    public LocalLicenseDetailsTextMapper(DashboardTextMapper.Strings strings, ZoneId zone) {
        this.strings = strings;
        dashboardText = new DashboardTextMapper(strings);
        this.zone = zone;
        dates = DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM)
                .withLocale(Locale.forLanguageTag("ar")).withZone(zone);
    }

    public Status status(LocalLicenseDetails details) {
        if (Boolean.TRUE.equals(details.getIsExpired())) return Status.EXPIRED;
        if (Boolean.FALSE.equals(details.getIsActive())) return Status.INACTIVE;
        if (Boolean.TRUE.equals(details.getIsActive()) && Boolean.FALSE.equals(details.getIsExpired())) {
            return Status.ACTIVE;
        }
        return Status.UNKNOWN;
    }

    public String statusText(LocalLicenseDetails details) {
        switch (status(details)) {
            case ACTIVE: return strings.get(R.string.licenses_active);
            case EXPIRED: return strings.get(R.string.license_details_expired);
            case INACTIVE: return strings.get(R.string.licenses_inactive);
            default: return unknown();
        }
    }

    public String detention(Boolean detained) {
        return detained == null ? unknown()
                : strings.get(detained ? R.string.licenses_detained : R.string.licenses_not_detained);
    }

    public boolean showDetainedBadge(LocalLicenseDetails details) {
        return Boolean.TRUE.equals(details.getIsDetained());
    }

    public String issueReason(Integer reason, String backendText) {
        if (reason != null) {
            switch (reason) {
                case 1: return strings.get(R.string.license_issue_reason_first);
                case 2: return strings.get(R.string.license_issue_reason_renew);
                case 3: return strings.get(R.string.license_issue_reason_damaged);
                case 4: return strings.get(R.string.license_issue_reason_lost);
                default: break;
            }
        }
        return text(backendText);
    }

    public String title(LocalLicenseDetails details) {
        String className = dashboardText.licenseClass(details.getClassName());
        return hasText(className) ? className : strings.get(R.string.licenses_local_card_title);
    }

    public String licenseId(LocalLicenseDetails details) {
        return strings.get(R.string.licenses_local_id, value(details.getLicenseID()));
    }

    public String citizenInformation(LocalLicenseDetails details) {
        return strings.get(R.string.license_details_full_name, text(details.getFullName())) + "\n"
                + strings.get(R.string.license_details_national_no, text(details.getNationalNo()));
    }

    public String licenseInformation(LocalLicenseDetails details) {
        String result = licenseId(details) + "\n"
                + strings.get(R.string.license_details_application_id, value(details.getApplicationID())) + "\n"
                + strings.get(R.string.license_details_class, text(dashboardText.licenseClass(details.getClassName())));
        if (hasText(details.getClassDescription())) {
            result += "\n" + strings.get(R.string.license_details_class_description, details.getClassDescription());
        }
        return result + "\n"
                + strings.get(R.string.licenses_issue_date, date(details.getIssueDate())) + "\n"
                + strings.get(R.string.licenses_expiration_date, date(details.getExpirationDate())) + "\n"
                + strings.get(R.string.license_details_paid_fees, fees(details.getPaidFees())) + "\n"
                + strings.get(R.string.license_details_issue_reason, issueReason(details.getIssueReason(), details.getIssueReasonText())) + "\n"
                + strings.get(R.string.license_details_status, statusText(details)) + "\n"
                + strings.get(R.string.licenses_detained_status, detention(details.getIsDetained()));
    }

    public String notes(String notes) {
        return hasText(notes) ? notes : strings.get(R.string.license_details_no_notes);
    }

    public String fees(Double fees) {
        return fees == null || !Double.isFinite(fees) ? unknown() : String.format(Locale.ROOT, "%.2f", fees);
    }

    private String date(String date) {
        Instant instant = ApiDateTime.parse(date, zone);
        return instant == null ? unknown() : dates.format(instant);
    }

    private String value(Integer value) {
        return value == null ? unknown() : String.valueOf(value);
    }

    private String text(String value) {
        return hasText(value) ? value : unknown();
    }

    private String unknown() {
        return strings.get(R.string.dashboard_unknown_value);
    }

    private static boolean hasText(String text) {
        return text != null && !text.trim().isEmpty();
    }
}
