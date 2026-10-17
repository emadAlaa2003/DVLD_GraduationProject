package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;
import java.util.ArrayList;
import java.util.List;

public final class PracticeExamSubmitRequest {
    @SerializedName("answers") private final List<PracticeExamAnswer> answers;
    public PracticeExamSubmitRequest(List<PracticeExamAnswer> answers) {
        this.answers = new ArrayList<>(answers);
    }
}
