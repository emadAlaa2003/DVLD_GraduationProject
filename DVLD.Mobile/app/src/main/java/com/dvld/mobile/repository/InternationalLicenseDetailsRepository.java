package com.dvld.mobile.repository;

import com.dvld.mobile.model.InternationalLicenseDetails;

public interface InternationalLicenseDetailsRepository {
    /** Concurrent loads reuse the pending request and its original callback. */
    RequestHandle load(int internationalLicenseId, DetailsCallback callback);

    interface RequestHandle {
        void cancel();
    }

    interface DetailsCallback {
        void onSuccess(InternationalLicenseDetails details);
        void onError(DetailsError error);
    }

    enum DetailsError {
        NETWORK, SERVER, NOT_FOUND, UNAUTHORIZED, FORBIDDEN
    }
}
