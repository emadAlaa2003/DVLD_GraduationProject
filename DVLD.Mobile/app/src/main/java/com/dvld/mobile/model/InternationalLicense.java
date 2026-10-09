package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class InternationalLicense {
    @SerializedName("internationalLicenseID")
    private Integer internationalLicenseID;

    @SerializedName("applicationID")
    private Integer applicationID;

    @SerializedName("issuedUsingLocalLicenseID")
    private Integer issuedUsingLocalLicenseID;

    @SerializedName("issueDate")
    private String issueDate;

    @SerializedName("expirationDate")
    private String expirationDate;

    @SerializedName("isActive")
    private Boolean isActive;

    public Integer getInternationalLicenseID() {
        return internationalLicenseID;
    }

    public Integer getApplicationID() {
        return applicationID;
    }

    public Integer getIssuedUsingLocalLicenseID() {
        return issuedUsingLocalLicenseID;
    }

    public String getIssueDate() {
        return issueDate;
    }

    public String getExpirationDate() {
        return expirationDate;
    }

    public Boolean getIsActive() {
        return isActive;
    }
}

