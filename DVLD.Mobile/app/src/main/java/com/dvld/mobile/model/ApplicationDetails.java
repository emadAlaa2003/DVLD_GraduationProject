package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

/** Nullable fields mirror the read-only application details response. */
public final class ApplicationDetails {
    @SerializedName("applicationID") private Integer applicationID;
    @SerializedName("applicantPersonID") private Integer applicantPersonID;
    @SerializedName("applicationDate") private String applicationDate;
    @SerializedName("applicationTypeID") private Integer applicationTypeID;
    @SerializedName("applicationTypeName") private String applicationTypeName;
    @SerializedName("applicationStatus") private Integer applicationStatus;
    @SerializedName("statusText") private String statusText;
    @SerializedName("lastStatusDate") private String lastStatusDate;
    @SerializedName("paidFees") private Double paidFees;
    @SerializedName("details") private ApplicationSpecificDetails details;

    public Integer getApplicationID() { return applicationID; }
    public Integer getApplicantPersonID() { return applicantPersonID; }
    public String getApplicationDate() { return applicationDate; }
    public Integer getApplicationTypeID() { return applicationTypeID; }
    public String getApplicationTypeName() { return applicationTypeName; }
    public Integer getApplicationStatus() { return applicationStatus; }
    public String getStatusText() { return statusText; }
    public String getLastStatusDate() { return lastStatusDate; }
    public Double getPaidFees() { return paidFees; }
    public ApplicationSpecificDetails getDetails() { return details; }
}
