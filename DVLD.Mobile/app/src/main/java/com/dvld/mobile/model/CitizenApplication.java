package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class CitizenApplication {
    @SerializedName("applicationID")
    private Integer applicationID;

    @SerializedName("applicantPersonID")
    private Integer applicantPersonID;

    @SerializedName("applicationDate")
    private String applicationDate;

    @SerializedName("applicationTypeID")
    private Integer applicationTypeID;

    @SerializedName("applicationTypeName")
    private String applicationTypeName;

    @SerializedName("applicationStatus")
    private Integer applicationStatus;

    @SerializedName("statusText")
    private String statusText;

    @SerializedName("lastStatusDate")
    private String lastStatusDate;

    @SerializedName("paidFees")
    private Double paidFees;

    @SerializedName("localDrivingLicenseApplicationID")
    private Integer localDrivingLicenseApplicationID;

    @SerializedName("licenseClassID")
    private Integer licenseClassID;

    @SerializedName("className")
    private String className;

    public Integer getApplicationID() {
        return applicationID;
    }

    public Integer getApplicantPersonID() {
        return applicantPersonID;
    }

    public String getApplicationDate() {
        return applicationDate;
    }

    public Integer getApplicationTypeID() {
        return applicationTypeID;
    }

    public String getApplicationTypeName() {
        return applicationTypeName;
    }

    public Integer getApplicationStatus() {
        return applicationStatus;
    }

    public String getStatusText() {
        return statusText;
    }

    public String getLastStatusDate() {
        return lastStatusDate;
    }

    public Double getPaidFees() {
        return paidFees;
    }

    public Integer getLocalDrivingLicenseApplicationID() {
        return localDrivingLicenseApplicationID;
    }

    public Integer getLicenseClassID() {
        return licenseClassID;
    }

    public String getClassName() {
        return className;
    }
}

