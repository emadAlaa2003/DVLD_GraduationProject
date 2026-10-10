package com.dvld.mobile.repository;

import com.dvld.mobile.model.ApplicationDetails;

public interface ApplicationDetailsRepository {
    /** Concurrent loads reuse the pending request and its original callback. */
    RequestHandle load(int applicationId, DetailsCallback callback);

    interface RequestHandle {
        void cancel();
    }

    interface DetailsCallback {
        void onSuccess(ApplicationDetails details);
        void onError(DetailsError error);
    }

    enum DetailsError {
        NETWORK, SERVER, NOT_FOUND, UNAUTHORIZED, FORBIDDEN
    }
}
