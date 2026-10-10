package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

/** Nullable fields mirror the read-only application details response. */
public final class ApplicationSpecificDetails {
    @SerializedName("localDrivingLicenseApplicationID") private Integer localDrivingLicenseApplicationID;
    @SerializedName("licenseClassID") private Integer licenseClassID;
    @SerializedName("className") private String className;
    @SerializedName("licenseID") private Integer licenseID;
    @SerializedName("issueDate") private String issueDate;
    @SerializedName("expirationDate") private String expirationDate;
    @SerializedName("isActive") private Boolean isActive;
    @SerializedName("issueReason") private Integer issueReason;
    @SerializedName("issueReasonText") private String issueReasonText;
    @SerializedName("internationalLicenseID") private Integer internationalLicenseID;
    @SerializedName("driverID") private Integer driverID;
    @SerializedName("issuedUsingLocalLicenseID") private Integer issuedUsingLocalLicenseID;
    @SerializedName("isExpired") private Boolean isExpired;
    @SerializedName("isCurrentlyValid") private Boolean isCurrentlyValid;
    @SerializedName("detainID") private Integer detainID;
    @SerializedName("detainDate") private String detainDate;
    @SerializedName("fineFees") private Double fineFees;
    @SerializedName("isReleased") private Boolean isReleased;
    @SerializedName("releaseDate") private String releaseDate;
    @SerializedName("releaseApplicationID") private Integer releaseApplicationID;
    @SerializedName("testAppointmentID") private Integer testAppointmentID;
    @SerializedName("testTypeID") private Integer testTypeID;
    @SerializedName("testTypeName") private String testTypeName;
    @SerializedName("appointmentDate") private String appointmentDate;
    @SerializedName("appointmentPaidFees") private Double appointmentPaidFees;
    @SerializedName("isLocked") private Boolean isLocked;
    @SerializedName("testID") private Integer testID;

    public Integer getLocalDrivingLicenseApplicationID() { return localDrivingLicenseApplicationID; }
    public Integer getLicenseClassID() { return licenseClassID; }
    public String getClassName() { return className; }
    public Integer getLicenseID() { return licenseID; }
    public String getIssueDate() { return issueDate; }
    public String getExpirationDate() { return expirationDate; }
    public Boolean getIsActive() { return isActive; }
    public Integer getIssueReason() { return issueReason; }
    public String getIssueReasonText() { return issueReasonText; }
    public Integer getInternationalLicenseID() { return internationalLicenseID; }
    public Integer getDriverID() { return driverID; }
    public Integer getIssuedUsingLocalLicenseID() { return issuedUsingLocalLicenseID; }
    public Boolean getIsExpired() { return isExpired; }
    public Boolean getIsCurrentlyValid() { return isCurrentlyValid; }
    public Integer getDetainID() { return detainID; }
    public String getDetainDate() { return detainDate; }
    public Double getFineFees() { return fineFees; }
    public Boolean getIsReleased() { return isReleased; }
    public String getReleaseDate() { return releaseDate; }
    public Integer getReleaseApplicationID() { return releaseApplicationID; }
    public Integer getTestAppointmentID() { return testAppointmentID; }
    public Integer getTestTypeID() { return testTypeID; }
    public String getTestTypeName() { return testTypeName; }
    public String getAppointmentDate() { return appointmentDate; }
    public Double getAppointmentPaidFees() { return appointmentPaidFees; }
    public Boolean getIsLocked() { return isLocked; }
    public Integer getTestID() { return testID; }
}
