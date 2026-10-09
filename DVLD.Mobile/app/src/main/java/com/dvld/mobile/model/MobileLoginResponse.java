package com.dvld.mobile.model;

import com.google.gson.annotations.SerializedName;

public final class MobileLoginResponse {
    @SerializedName("personId")
    private Integer personId;

    @SerializedName("fullName")
    private String fullName;

    @SerializedName("username")
    private String username;

    public Integer getPersonId() {
        return personId;
    }

    public String getFullName() {
        return fullName;
    }

    public String getUsername() {
        return username;
    }
}
