package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;
import java.util.LinkedHashMap;
import java.util.Map;

public final class PracticeExamQuestion {
    @SerializedName("questionID") private Integer questionID;
    @SerializedName("questionType") private String questionType;
    @SerializedName("questionText") private String questionText;
    @SerializedName("optionA") private String optionA;
    @SerializedName("optionB") private String optionB;
    @SerializedName("optionC") private String optionC;
    @SerializedName("optionD") private String optionD;

    public Integer getQuestionID() { return questionID; }
    public String getQuestionType() { return questionType; }
    public String getQuestionText() { return questionText; }

    public Map<String, String> getOptions() {
        Map<String, String> options = new LinkedHashMap<>();
        addOption(options, "A", optionA);
        addOption(options, "B", optionB);
        if ("MultipleChoice".equals(questionType)) {
            addOption(options, "C", optionC);
            addOption(options, "D", optionD);
        }
        return options;
    }

    public String optionText(String option) { return getOptions().get(option); }

    public boolean isValid() {
        if (questionID == null || questionID <= 0 || !hasText(questionText, 1000)
                || !hasText(optionA, 500) || !hasText(optionB, 500)) return false;
        return "TrueFalse".equals(questionType) || ("MultipleChoice".equals(questionType)
                && hasText(optionC, 500) && hasText(optionD, 500));
    }

    private void addOption(Map<String, String> options, String key, String text) {
        if (text != null && !text.trim().isEmpty()) options.put(key, text);
    }

    private boolean hasText(String text, int limit) {
        return text != null && !text.trim().isEmpty() && text.length() <= limit;
    }
}
