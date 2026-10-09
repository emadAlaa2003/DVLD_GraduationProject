package com.dvld.mobile.repository;

import com.dvld.mobile.model.LocalLicenseDetails;

public interface LocalLicenseDetailsRepository {
    /** Concurrent loads reuse the pending request and its original callback. */
    RequestHandle load(int licenseId, DetailsCallback callback);

    interface RequestHandle {
        void cancel();
    }

    interface DetailsCallback {
        void onSuccess(LocalLicenseDetails details);
        void onError(DetailsError error);
    }

    enum DetailsError {
        NETWORK, SERVER, NOT_FOUND, UNAUTHORIZED, FORBIDDEN
    }
}
