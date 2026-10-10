package com.dvld.mobile.repository;

import com.dvld.mobile.model.CitizenApplication;

import java.util.List;

public interface ApplicationsRepository {
    enum ApplicationsError { NETWORK, SERVER, NOT_FOUND, UNAUTHORIZED, FORBIDDEN }

    interface RequestHandle {
        void cancel();
    }

    interface ApplicationsCallback {
        void onSuccess(List<CitizenApplication> applications);
        void onError(ApplicationsError error);
    }

    RequestHandle load(int personId, ApplicationsCallback callback);
}
