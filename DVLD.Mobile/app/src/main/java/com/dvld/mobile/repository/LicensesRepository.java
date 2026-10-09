package com.dvld.mobile.repository;

import com.dvld.mobile.model.LicensesData;

public interface LicensesRepository {
    /** Concurrent refreshes reuse the pending batch and its original callback. */
    RequestHandle load(int personId, LicensesCallback callback);

    interface RequestHandle {
        void cancel();
    }

    interface LicensesCallback {
        void onSuccess(LicensesData data);
        void onError(LicensesError error);
    }

    enum LicensesError {
        NETWORK, SERVER, NOT_FOUND, UNAUTHORIZED, FORBIDDEN
    }
}
