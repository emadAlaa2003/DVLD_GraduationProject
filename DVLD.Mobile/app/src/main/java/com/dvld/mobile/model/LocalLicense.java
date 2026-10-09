package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class LocalLicense {
    @SerializedName("licenseID")
    private Integer licenseID;

    @SerializedName("applicationID")
    private Integer applicationID;

    @SerializedName("className")
    private String className;

    @SerializedName("issueDate")
    private String issueDate;

    @SerializedName("expirationDate")
    private String expirationDate;

    @SerializedName("isActive")
    private Boolean isActive;

    @SerializedName("isDetained")
    private Boolean isDetained;

    public Integer getLicenseID() {
        return licenseID;
    }

    public Integer getApplicationID() {
        return applicationID;
    }

    public String getClassName() {
        return className;
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

    public Boolean getIsDetained() {
        return isDetained;
    }
}

