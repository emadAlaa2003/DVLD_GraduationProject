package com.dvld.mobile.repository;

import com.dvld.mobile.model.MobileLoginResponse;

public interface AuthRepository {
    RequestHandle login(String username, String password, LoginCallback callback);

    interface RequestHandle {
        void cancel();
    }

    interface LoginCallback {
        void onSuccess(MobileLoginResponse response);
        void onError(LoginError error);
    }

    enum LoginError {
        INVALID_CREDENTIALS,
        INACTIVE_ACCOUNT,
        NETWORK,
        SERVER
    }
}
