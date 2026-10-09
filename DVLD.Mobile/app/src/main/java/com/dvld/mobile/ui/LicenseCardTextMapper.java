package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.InternationalLicense;
import com.dvld.mobile.model.LocalLicense;

import java.time.Instant;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.Locale;

/** Arabic card text, with an explicit unknown value for missing API fields. */
public final class LicenseCardTextMapper {
    private final DashboardTextMapper.Strings strings;
    private final DashboardTextMapper dashboardText;
    private final DateTimeFormatter dates;
    private final ZoneId zone;

    public LicenseCardTextMapper(DashboardTextMapper.Strings strings, ZoneId zone) {
        this.strings = strings;
        this.dashboardText = new DashboardTextMapper(strings);
        this.zone = zone;
        dates = DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM)
                .withLocale(Locale.forLanguageTag("ar")).withZone(zone);
    }

    public String localTitle(LocalLicense license) {
        String className = dashboardText.licenseClass(license.getClassName());
        return className != null && !className.trim().isEmpty()
                ? className : strings.get(R.string.licenses_local_card_title);
    }

    public String localDetails(LocalLicense license) {
        return strings.get(R.string.licenses_local_id, value(license.getLicenseID())) + "\n"
                + commonDetails(license.getIssueDate(), license.getExpirationDate(), license.getIsActive()) + "\n"
                + strings.get(R.string.licenses_detained_status, detention(license.getIsDetained()));
    }

    public String internationalDetails(InternationalLicense license) {
        return strings.get(R.string.licenses_international_id, value(license.getInternationalLicenseID())) + "\n"
                + strings.get(R.string.licenses_issued_using_local_id, value(license.getIssuedUsingLocalLicenseID())) + "\n"
                + commonDetails(license.getIssueDate(), license.getExpirationDate(), license.getIsActive());
    }

    private String commonDetails(String issueDate, String expirationDate, Boolean isActive) {
        return strings.get(R.string.licenses_issue_date, date(issueDate)) + "\n"
                + strings.get(R.string.licenses_expiration_date, date(expirationDate)) + "\n"
                + strings.get(R.string.licenses_active_status, active(isActive));
    }

    private String date(String value) {
        Instant instant = ApiDateTime.parse(value, zone);
        return instant == null ? unknown() : dates.format(instant);
    }

    private String active(Boolean value) {
        return value == null ? unknown()
                : strings.get(value ? R.string.licenses_active : R.string.licenses_inactive);
    }

    private String detention(Boolean value) {
        return value == null ? unknown()
                : strings.get(value ? R.string.licenses_detained : R.string.licenses_not_detained);
    }

    private String value(Integer value) {
        return value == null ? unknown() : String.valueOf(value);
    }

    private String unknown() {
        return strings.get(R.string.dashboard_unknown_value);
    }
}
