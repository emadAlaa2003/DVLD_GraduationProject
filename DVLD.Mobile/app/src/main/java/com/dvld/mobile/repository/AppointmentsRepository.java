package com.dvld.mobile.repository;

import com.dvld.mobile.model.TestAppointment;

import java.util.List;

public interface AppointmentsRepository {
    enum AppointmentsError { NETWORK, SERVER, NOT_FOUND, UNAUTHORIZED, FORBIDDEN }

    interface RequestHandle {
        void cancel();
    }

    interface AppointmentsCallback {
        void onSuccess(List<TestAppointment> appointments);
        void onError(AppointmentsError error);
    }

    RequestHandle load(int personId, AppointmentsCallback callback);
}
