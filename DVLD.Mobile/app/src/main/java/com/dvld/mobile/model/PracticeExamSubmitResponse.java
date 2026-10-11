package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public final class PracticeExamSubmitResponse {
    @SerializedName("questionCount") private Integer questionCount;
    @SerializedName("answeredCount") private Integer answeredCount;
    @SerializedName("correctAnswers") private Integer correctAnswers;
    @SerializedName("percentage") private Double percentage;
    @SerializedName("results") private List<PracticeExamAnswerResult> results;
    public Integer getQuestionCount() { return questionCount; }
    public Integer getAnsweredCount() { return answeredCount; }
    public Integer getCorrectAnswers() { return correctAnswers; }
    public Double getPercentage() { return percentage; }
    public List<PracticeExamAnswerResult> getResults() { return results; }
}
