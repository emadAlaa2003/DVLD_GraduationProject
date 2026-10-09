package com.dvld.mobile.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/** Separate, immutable lists for the two license concepts. */
public final class LicensesData {
    private final List<LocalLicense> localLicenses;
    private final List<InternationalLicense> internationalLicenses;

    public LicensesData(List<LocalLicense> localLicenses, List<InternationalLicense> internationalLicenses) {
        this.localLicenses = snapshot(localLicenses);
        this.internationalLicenses = snapshot(internationalLicenses);
    }

    private static <T> List<T> snapshot(List<T> items) {
        List<T> result = new ArrayList<>();
        if (items != null) {
            for (T item : items) {
                if (item != null) result.add(item);
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
}
