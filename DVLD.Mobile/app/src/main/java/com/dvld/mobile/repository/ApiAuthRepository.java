package com.dvld.mobile.repository;

import com.dvld.mobile.model.MobileLoginRequest;
import com.dvld.mobile.model.MobileLoginResponse;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.MobileAuthApiService;
import com.google.gson.JsonParseException;

import java.io.IOException;
import java.io.EOFException;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiAuthRepository implements AuthRepository {
    private final MobileAuthApiService service;

    public ApiAuthRepository() {
        this(ApiClient.getAuthService());
    }

    public ApiAuthRepository(MobileAuthApiService service) {
        this.service = service;
    }

    @Override
    public RequestHandle login(String username, String password, LoginCallback callback) {
        Call<MobileLoginResponse> call = service.login(
                new MobileLoginRequest(username.trim(), password));
        call.enqueue(new Callback<MobileLoginResponse>() {
            @Override
            public void onResponse(Call<MobileLoginResponse> completedCall,
                                   Response<MobileLoginResponse> response) {
                // No server response text or credentials are logged or exposed to the UI.
                ResponseBody errorBody = response.errorBody();
                if (errorBody != null) {
                    errorBody.close();
                }
                if (completedCall.isCanceled()) {
                    return;
                }

                MobileLoginResponse body = response.body();
                if (response.code() == 200 && body != null &&
                        body.getPersonId() != null && body.getUsername() != null &&
                        !body.getUsername().trim().isEmpty()) {
                    // FullName can be null in the current backend contract.
                    callback.onSuccess(body);
                } else if (response.code() == 401) {
                    callback.onError(LoginError.INVALID_CREDENTIALS);
                } else if (response.code() == 403) {
                    callback.onError(LoginError.INACTIVE_ACCOUNT);
                } else {
                    callback.onError(LoginError.SERVER);
                }
            }

            @Override
            public void onFailure(Call<MobileLoginResponse> failedCall, Throwable error) {
                if (!failedCall.isCanceled()) {
                    // Invalid JSON is a server response problem, not a connectivity message.
                    boolean networkFailure = error instanceof IOException &&
                            !(error instanceof com.google.gson.stream.MalformedJsonException) &&
                            !(error instanceof EOFException);
                    callback.onError(networkFailure && !(error instanceof JsonParseException)
                            ? LoginError.NETWORK : LoginError.SERVER);
                }
            }
        });
        return call::cancel;
    }
}
