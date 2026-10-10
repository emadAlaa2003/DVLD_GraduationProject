package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

/** Nullable fields mirror the read-only test appointment response. */
public final class TestAppointmentDetails {
    @SerializedName("testAppointmentID") private Integer testAppointmentID;
    @SerializedName("testTypeID") private Integer testTypeID;
    @SerializedName("testTypeName") private String testTypeName;
    @SerializedName("localDrivingLicenseApplicationID") private Integer localDrivingLicenseApplicationID;
    @SerializedName("appointmentDate") private String appointmentDate;
    @SerializedName("paidFees") private Double paidFees;
    @SerializedName("isLocked") private Boolean isLocked;
    @SerializedName("retakeTestApplicationID") private Integer retakeTestApplicationID;
    @SerializedName("test") private TestResultDetails test;

    public Integer getTestAppointmentID() { return testAppointmentID; }
    public Integer getTestTypeID() { return testTypeID; }
    public String getTestTypeName() { return testTypeName; }
    public Integer getLocalDrivingLicenseApplicationID() { return localDrivingLicenseApplicationID; }
    public String getAppointmentDate() { return appointmentDate; }
    public Double getPaidFees() { return paidFees; }
    public Boolean getIsLocked() { return isLocked; }
    public Integer getRetakeTestApplicationID() { return retakeTestApplicationID; }
    public TestResultDetails getTest() { return test; }
}
