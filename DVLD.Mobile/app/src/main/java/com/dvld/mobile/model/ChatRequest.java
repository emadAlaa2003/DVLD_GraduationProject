package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class ChatRequest {
    @SerializedName("question") private final String question;
    public ChatRequest(String question) { this.question = question; }
    public String getQuestion() { return question; }
}
