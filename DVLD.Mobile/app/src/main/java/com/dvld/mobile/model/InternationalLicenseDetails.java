package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

/** Nullable fields mirror the read-only international license details response. */
public final class InternationalLicenseDetails {
    @SerializedName("internationalLicenseID") private Integer internationalLicenseID;
    @SerializedName("applicationID") private Integer applicationID;
    @SerializedName("personID") private Integer personID;
    @SerializedName("fullName") private String fullName;
    @SerializedName("nationalNo") private String nationalNo;
    @SerializedName("driverID") private Integer driverID;
    @SerializedName("issuedUsingLocalLicenseID") private Integer issuedUsingLocalLicenseID;
    @SerializedName("issueDate") private String issueDate;
    @SerializedName("expirationDate") private String expirationDate;
    @SerializedName("isActive") private Boolean isActive;
    @SerializedName("isExpired") private Boolean isExpired;
    @SerializedName("isCurrentlyValid") private Boolean isCurrentlyValid;
    @SerializedName("applicationDate") private String applicationDate;
    @SerializedName("applicationStatus") private Integer applicationStatus;
    @SerializedName("paidFees") private Double paidFees;

    public Integer getInternationalLicenseID() { return internationalLicenseID; }
    public Integer getApplicationID() { return applicationID; }
    public Integer getPersonID() { return personID; }
    public String getFullName() { return fullName; }
    public String getNationalNo() { return nationalNo; }
    public Integer getDriverID() { return driverID; }
    public Integer getIssuedUsingLocalLicenseID() { return issuedUsingLocalLicenseID; }
    public String getIssueDate() { return issueDate; }
    public String getExpirationDate() { return expirationDate; }
    public Boolean getIsActive() { return isActive; }
    public Boolean getIsExpired() { return isExpired; }
    public Boolean getIsCurrentlyValid() { return isCurrentlyValid; }
    public String getApplicationDate() { return applicationDate; }
    public Integer getApplicationStatus() { return applicationStatus; }
    public Double getPaidFees() { return paidFees; }
}
