package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public final class PracticeExamStartResponse {
    @SerializedName("requestedQuestionCount") private Integer requestedQuestionCount;
    @SerializedName("questionCount") private Integer questionCount;
    @SerializedName("questions") private List<PracticeExamQuestion> questions;
    public Integer getRequestedQuestionCount() { return requestedQuestionCount; }
    public Integer getQuestionCount() { return questionCount; }
    public List<PracticeExamQuestion> getQuestions() { return questions; }
}
