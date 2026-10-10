package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.CitizenApplication;

import java.time.Instant;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Locale;

public final class ApplicationsTextMapper {
    public enum Status { NEW, CANCELLED, COMPLETED, UNKNOWN }

    private final DashboardTextMapper.Strings strings;
    private final DashboardTextMapper dashboardMapper;
    private final ZoneId zone;
    private final DateTimeFormatter dateFormatter;

    public ApplicationsTextMapper(DashboardTextMapper.Strings strings, ZoneId zone) {
        this.strings = strings;
        this.zone = zone;
        dashboardMapper = new DashboardTextMapper(strings);
        dateFormatter = DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM)
                .withLocale(new Locale("ar")).withZone(zone);
    }

    public List<CitizenApplication> sortedApplications(List<CitizenApplication> applications) {
        List<CitizenApplication> sorted = new ArrayList<>();
        if (applications != null) {
            for (CitizenApplication application : applications) {
                if (application != null) sorted.add(application);
            }
        }
        sorted.sort(Comparator.comparing(
                application -> ApiDateTime.parse(application.getApplicationDate(), zone),
                Comparator.nullsLast(Comparator.<Instant>reverseOrder())));
        return Collections.unmodifiableList(sorted);
    }

    public String applicationId(CitizenApplication application) {
        Integer id = application.getApplicationID();
        return strings.get(R.string.applications_application_id,
                id != null && id > 0 ? String.valueOf(id) : unknown());
    }

    public String applicationType(CitizenApplication application) {
        String type = dashboardMapper.applicationType(application.getApplicationTypeID(),
                application.getApplicationTypeName());
        return hasText(type) ? type : unknown();
    }

    public Status status(CitizenApplication application) {
        Integer id = application.getApplicationStatus();
        if (Integer.valueOf(1).equals(id)) return Status.NEW;
        if (Integer.valueOf(2).equals(id)) return Status.CANCELLED;
        if (Integer.valueOf(3).equals(id)) return Status.COMPLETED;
        String text = application.getStatusText();
        if (text != null) {
            text = text.trim();
            if ("New".equalsIgnoreCase(text)) return Status.NEW;
            if ("Cancelled".equalsIgnoreCase(text)) return Status.CANCELLED;
            if ("Completed".equalsIgnoreCase(text)) return Status.COMPLETED;
        }
        return Status.UNKNOWN;
    }

    public String statusText(CitizenApplication application) {
        switch (status(application)) {
            case NEW: return strings.get(R.string.dashboard_status_new);
            case CANCELLED: return strings.get(R.string.dashboard_status_cancelled);
            case COMPLETED: return strings.get(R.string.dashboard_status_completed);
            default: return unknown();
        }
    }

    public String details(CitizenApplication application) {
        String details = strings.get(R.string.dashboard_application_date, date(application.getApplicationDate()))
                + "\n" + strings.get(R.string.applications_last_status_date, date(application.getLastStatusDate()))
                + "\n" + strings.get(R.string.license_details_paid_fees, fees(application.getPaidFees()));
        if (hasText(application.getClassName())) {
            details += "\n" + strings.get(R.string.dashboard_application_class,
                    dashboardMapper.licenseClass(application.getClassName()));
        }
        return details;
    }

    public String date(String value) {
        Instant parsed = ApiDateTime.parse(value, zone);
        return parsed == null ? unknown() : dateFormatter.format(parsed);
    }

    public String fees(Double value) {
        return value == null || !Double.isFinite(value) || value < 0 ? unknown()
                : String.format(Locale.ROOT, "%.2f", value);
    }

    private String unknown() {
        return strings.get(R.string.dashboard_unknown_value);
    }

    private static boolean hasText(String value) {
        return value != null && !value.trim().isEmpty();
    }
}
