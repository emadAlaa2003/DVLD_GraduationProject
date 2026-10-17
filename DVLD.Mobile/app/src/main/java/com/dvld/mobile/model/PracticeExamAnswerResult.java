package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class PracticeExamAnswerResult {
    @SerializedName("questionID") private Integer questionID;
    @SerializedName("selectedOption") private String selectedOption;
    @SerializedName("correctOption") private String correctOption;
    @SerializedName("isCorrect") private Boolean isCorrect;
    @SerializedName("explanation") private String explanation;
    public Integer getQuestionID() { return questionID; }
    public String getSelectedOption() { return selectedOption; }
    public String getCorrectOption() { return correctOption; }
    public Boolean getIsCorrect() { return isCorrect; }
    public String getExplanation() { return explanation; }
}
