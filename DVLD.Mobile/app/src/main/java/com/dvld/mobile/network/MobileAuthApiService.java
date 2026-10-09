package com.dvld.mobile.network;

import com.dvld.mobile.model.MobileLoginRequest;
import com.dvld.mobile.model.MobileLoginResponse;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface MobileAuthApiService {
    @POST("api/mobile-auth/login")
    Call<MobileLoginResponse> login(@Body MobileLoginRequest request);
}
