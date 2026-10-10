package com.dvld.mobile.repository;

import com.dvld.mobile.model.TestAppointmentDetails;

public interface TestAppointmentDetailsRepository {
    /** Concurrent loads reuse the pending request and its original callback. */
    RequestHandle load(int testAppointmentId, DetailsCallback callback);

    interface RequestHandle {
        void cancel();
    }

    interface DetailsCallback {
        void onSuccess(TestAppointmentDetails details);
        void onError(DetailsError error);
    }

    enum DetailsError {
        NETWORK, SERVER, NOT_FOUND, UNAUTHORIZED, FORBIDDEN
    }
}
