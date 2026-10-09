package com.dvld.mobile.model;

import java.time.Instant;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.time.OffsetDateTime;
import java.time.ZoneId;
import java.time.format.DateTimeParseException;

public final class ApiDateTime {
    private ApiDateTime() {
    }

    /** ASP.NET dates without an offset are interpreted in the phone's local time zone. */
    public static Instant parse(String value, ZoneId zone) {
        if (value == null || value.trim().isEmpty()) {
            return null;
        }
        String date = value.trim();
        try {
            return OffsetDateTime.parse(date).toInstant();
        } catch (DateTimeParseException ignored) {
        }
        try {
            return LocalDateTime.parse(date).atZone(zone).toInstant();
        } catch (DateTimeParseException ignored) {
        }
        try {
            return LocalDate.parse(date).atStartOfDay(zone).toInstant();
        } catch (DateTimeParseException ignored) {
            return null;
        }
    }
}
