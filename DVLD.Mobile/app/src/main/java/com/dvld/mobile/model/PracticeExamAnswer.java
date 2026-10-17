package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class PracticeExamAnswer {
    @SerializedName("questionID") private final int questionID;
    @SerializedName("selectedOption") private final String selectedOption;
    public PracticeExamAnswer(int questionID, String selectedOption) {
        this.questionID = questionID;
        this.selectedOption = selectedOption;
    }
    public int getQuestionID() { return questionID; }
    public String getSelectedOption() { return selectedOption; }
}
