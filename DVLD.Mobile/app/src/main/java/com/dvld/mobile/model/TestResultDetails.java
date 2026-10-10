package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

/** Nullable fields mirror the read-only test appointment response. */
public final class TestResultDetails {
    @SerializedName("testID") private Integer testID;
    @SerializedName("testResult") private Boolean testResult;
    @SerializedName("resultText") private String resultText;
    @SerializedName("notes") private String notes;

    public Integer getTestID() { return testID; }
    public Boolean getTestResult() { return testResult; }
    public String getResultText() { return resultText; }
    public String getNotes() { return notes; }
}
