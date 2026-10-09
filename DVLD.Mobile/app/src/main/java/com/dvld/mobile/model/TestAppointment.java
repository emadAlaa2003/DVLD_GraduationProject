package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class TestAppointment {
    @SerializedName("testAppointmentID")
    private Integer testAppointmentID;

    @SerializedName("testTypeID")
    private Integer testTypeID;

    @SerializedName("testTypeTitle")
    private String testTypeTitle;

    @SerializedName("localDrivingLicenseApplicationID")
    private Integer localDrivingLicenseApplicationID;

    @SerializedName("applicationID")
    private Integer applicationID;

    @SerializedName("licenseClassID")
    private Integer licenseClassID;

    @SerializedName("className")
    private String className;

    @SerializedName("appointmentDate")
    private String appointmentDate;

    @SerializedName("paidFees")
    private Double paidFees;

    @SerializedName("isLocked")
    private Boolean isLocked;

    @SerializedName("retakeTestApplicationID")
    private Integer retakeTestApplicationID;

    @SerializedName("testID")
    private Integer testID;

    @SerializedName("testResult")
    private Boolean testResult;

    @SerializedName("notes")
    private String notes;

    public Integer getTestAppointmentID() {
        return testAppointmentID;
    }

    public Integer getTestTypeID() {
        return testTypeID;
    }

    public String getTestTypeTitle() {
        return testTypeTitle;
    }

    public Integer getLocalDrivingLicenseApplicationID() {
        return localDrivingLicenseApplicationID;
    }

    public Integer getApplicationID() {
        return applicationID;
    }

    public Integer getLicenseClassID() {
        return licenseClassID;
    }

    public String getClassName() {
        return className;
    }

    public String getAppointmentDate() {
        return appointmentDate;
    }

    public Double getPaidFees() {
        return paidFees;
    }

    public Boolean getIsLocked() {
        return isLocked;
    }

    public Integer getRetakeTestApplicationID() {
        return retakeTestApplicationID;
    }

    public Integer getTestID() {
        return testID;
    }

    public Boolean getTestResult() {
        return testResult;
    }

    public String getNotes() {
        return notes;
    }
}

