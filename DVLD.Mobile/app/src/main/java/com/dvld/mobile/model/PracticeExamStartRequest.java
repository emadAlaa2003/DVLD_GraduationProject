package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class PracticeExamStartRequest {
    @SerializedName("questionCount") private final int questionCount;
    public PracticeExamStartRequest(int questionCount) { this.questionCount = questionCount; }
}
