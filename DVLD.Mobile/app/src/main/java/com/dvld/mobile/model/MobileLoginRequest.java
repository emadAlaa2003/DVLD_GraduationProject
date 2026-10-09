package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class MobileLoginRequest {
    @SerializedName("username")
    private final String username;

    @SerializedName("password")
    private final String password;

    public MobileLoginRequest(String username, String password) {
        this.username = username;
        this.password = password;
    }
}
