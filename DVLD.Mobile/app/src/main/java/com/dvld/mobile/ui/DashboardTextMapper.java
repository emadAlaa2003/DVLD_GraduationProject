package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.LocalLicense;

import java.time.Instant;
import java.time.ZoneId;
import java.util.List;

/** Display-only translations; backend values and license types stay unchanged. */
public final class DashboardTextMapper {
    public interface Strings {
        String get(int resourceId, Object... arguments);
    }

    private final Strings strings;

    public DashboardTextMapper(Strings strings) {
        this.strings = strings;
    }

    public String licenseClass(String backendName) {
        if (backendName == null) return null;
        int resource;
        switch (backendName.trim()) {
            case "Class 1 - Small Motorcycle": resource = R.string.license_class_1; break;
            case "Class 2 - Heavy Motorcycle License": resource = R.string.license_class_2; break;
            case "Class 3 - Ordinary driving license": resource = R.string.license_class_3; break;
            case "Class 4 - Commercial": resource = R.string.license_class_4; break;
            case "Class 5 - Agricultural": resource = R.string.license_class_5; break;
            case "Class 6 - Small and medium bus": resource = R.string.license_class_6; break;
            case "Class 7 - Truck and heavy vehicle": resource = R.string.license_class_7; break;
            default: return backendName;
        }
        return strings.get(resource);
    }

    public String applicationType(Integer typeId, String backendName) {
        int resource = applicationTypeResource(typeId == null ? 0 : typeId);
        if (resource == 0 && backendName != null) {
            // Names are a fallback when the stable ID is missing or unrecognized.
            switch (backendName.trim()) {
                case "NewDrivingLicense": resource = R.string.application_type_1; break;
                case "RenewDrivingLicense": resource = R.string.application_type_2; break;
                case "ReplaceLostDrivingLicense": resource = R.string.application_type_3; break;
                case "ReplaceDamagedDrivingLicense": resource = R.string.application_type_4; break;
                case "ReleaseDetainedDrivingLicsense": resource = R.string.application_type_5; break;
                case "NewInternationalLicense": resource = R.string.application_type_6; break;
                case "RetakeTest": resource = R.string.application_type_7; break;
                default: break;
            }
        }
        return resource == 0 ? backendName : strings.get(resource);
    }

    private static int applicationTypeResource(int typeId) {
        switch (typeId) {
            case 1: return R.string.application_type_1;
            case 2: return R.string.application_type_2;
            case 3: return R.string.application_type_3;
            case 4: return R.string.application_type_4;
            case 5: return R.string.application_type_5;
            case 6: return R.string.application_type_6;
            case 7: return R.string.application_type_7;
            default: return 0;
        }
    }

    public String localLicenseSummary(List<LocalLicense> licenses, ZoneId zone) {
        int count = licenses.size();
        if (count == 0) return strings.get(R.string.dashboard_no_local_licenses);
        if (count == 1) {
            String summary = strings.get(R.string.dashboard_one_local_license);
            String className = licenseClass(licenses.get(0).getClassName());
            return hasText(className) ? summary + "\n" + className : summary;
        }

        String summary = strings.get(R.string.dashboard_local_license_count, count);
        LocalLicense latest = null;
        Instant latestDate = null;
        for (LocalLicense license : licenses) {
            Instant date = ApiDateTime.parse(license.getIssueDate(), zone);
            if (date != null && (latestDate == null || date.isAfter(latestDate))) {
                latest = license;
                latestDate = date;
            }
        }
        String className = latest == null ? null : licenseClass(latest.getClassName());
        return hasText(className)
                ? summary + "\n" + strings.get(R.string.dashboard_latest_license_class, className)
                : summary;
    }

    public String internationalLicenseSummary(int count) {
        if (count == 0) return strings.get(R.string.dashboard_no_international_licenses);
        if (count == 1) return strings.get(R.string.dashboard_one_international_license);
        return strings.get(R.string.dashboard_international_license_count, count);
    }

    private static boolean hasText(String text) {
        return text != null && !text.trim().isEmpty();
    }
}
