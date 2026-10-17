package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public final class ChatResponse {
    @SerializedName("question") private String question;
    @SerializedName("answer") private String answer;
    @SerializedName("sources") private List<ChatSource> sources;
    public String getQuestion() { return question; }
    public String getAnswer() { return answer; }
    public List<ChatSource> getSources() {
        if (sources == null) return Collections.emptyList();
        List<ChatSource> available = new ArrayList<>();
        for (ChatSource source : sources) if (source != null) available.add(source);
        return Collections.unmodifiableList(available);
    }
    public boolean isValid() {
        return question != null && !question.trim().isEmpty()
                && answer != null && !answer.trim().isEmpty();
    }
}
