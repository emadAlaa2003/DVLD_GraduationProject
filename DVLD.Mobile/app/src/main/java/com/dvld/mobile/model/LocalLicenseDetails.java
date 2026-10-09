package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

/** Nullable fields mirror the read-only local license details response. */
public final class LocalLicenseDetails {
    @SerializedName("licenseID") private Integer licenseID;
    @SerializedName("applicationID") private Integer applicationID;
    @SerializedName("personID") private Integer personID;
    @SerializedName("fullName") private String fullName;
    @SerializedName("nationalNo") private String nationalNo;
    @SerializedName("licenseClassID") private Integer licenseClassID;
    @SerializedName("className") private String className;
    @SerializedName("classDescription") private String classDescription;
    @SerializedName("issueDate") private String issueDate;
    @SerializedName("expirationDate") private String expirationDate;
    @SerializedName("notes") private String notes;
    @SerializedName("paidFees") private Double paidFees;
    @SerializedName("isActive") private Boolean isActive;
    @SerializedName("isExpired") private Boolean isExpired;
    @SerializedName("issueReason") private Integer issueReason;
    @SerializedName("issueReasonText") private String issueReasonText;
    @SerializedName("isDetained") private Boolean isDetained;

    public Integer getLicenseID() { return licenseID; }
    public Integer getApplicationID() { return applicationID; }
    public Integer getPersonID() { return personID; }
    public String getFullName() { return fullName; }
    public String getNationalNo() { return nationalNo; }
    public Integer getLicenseClassID() { return licenseClassID; }
    public String getClassName() { return className; }
    public String getClassDescription() { return classDescription; }
    public String getIssueDate() { return issueDate; }
    public String getExpirationDate() { return expirationDate; }
    public String getNotes() { return notes; }
    public Double getPaidFees() { return paidFees; }
    public Boolean getIsActive() { return isActive; }
    public Boolean getIsExpired() { return isExpired; }
    public Integer getIssueReason() { return issueReason; }
    public String getIssueReasonText() { return issueReasonText; }
    public Boolean getIsDetained() { return isDetained; }
}
