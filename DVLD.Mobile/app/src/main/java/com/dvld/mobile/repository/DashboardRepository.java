package com.dvld.mobile.repository;

import com.dvld.mobile.model.DashboardData;

public interface DashboardRepository {
    /** Concurrent refreshes reuse the pending request and its original callback. */
    RequestHandle load(int personId, DashboardCallback callback);

    interface RequestHandle {
        void cancel();
    }

    interface DashboardCallback {
        void onSuccess(DashboardData data);
        void onError(DashboardError error);
    }

    enum DashboardError {
        NETWORK,
        SERVER,
        NOT_FOUND,
        UNAUTHORIZED,
        FORBIDDEN
    }
}
