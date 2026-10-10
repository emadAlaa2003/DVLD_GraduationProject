package com.dvld.mobile.repository;

import com.dvld.mobile.model.TestAppointment;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.DashboardApiService;
import com.google.gson.stream.MalformedJsonException;

import java.io.EOFException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiAppointmentsRepository implements AppointmentsRepository {
    private final DashboardApiService service;
    private Request inFlight;

    public ApiAppointmentsRepository() {
        this(ApiClient.getDashboardService());
    }

    public ApiAppointmentsRepository(DashboardApiService service) {
        this.service = service;
    }

    @Override
    public synchronized RequestHandle load(int personId, AppointmentsCallback callback) {
        if (inFlight != null) return inFlight;
        if (personId <= 0) {
            callback.onError(AppointmentsError.NOT_FOUND);
            return () -> { };
        }
        Request request = new Request(personId, callback);
        inFlight = request;
        request.start();
        return request;
    }

    private synchronized void release(Request request) {
        if (inFlight == request) inFlight = null;
    }

    private final class Request implements RequestHandle {
        private final Call<List<TestAppointment>> call;
        private final AppointmentsCallback callback;
        private boolean finished;

        private Request(int personId, AppointmentsCallback callback) {
            this.callback = callback;
            call = service.getTestAppointments(personId);
        }

        private void start() {
            call.enqueue(new Callback<List<TestAppointment>>() {
                @Override
                public void onResponse(Call<List<TestAppointment>> completedCall,
                                       Response<List<TestAppointment>> response) {
                    ResponseBody errorBody = response.errorBody();
                    if (errorBody != null) errorBody.close();
                    if (!complete()) return;
                    List<TestAppointment> body = response.body();
                    if (response.code() == 200 && body != null) {
                        List<TestAppointment> appointments = new ArrayList<>();
                        for (TestAppointment application : body) {
                            if (application != null) appointments.add(application);
                        }
                        callback.onSuccess(Collections.unmodifiableList(appointments));
                    } else {
                        callback.onError(httpError(response.code()));
                    }
                }

                @Override
                public void onFailure(Call<List<TestAppointment>> failedCall, Throwable error) {
                    if (!complete()) return;
                    boolean network = error instanceof IOException &&
                            !(error instanceof MalformedJsonException) && !(error instanceof EOFException);
                    callback.onError(network ? AppointmentsError.NETWORK : AppointmentsError.SERVER);
                }
            });
        }

        private boolean complete() {
            synchronized (this) {
                if (finished) return false;
                finished = true;
            }
            release(this);
            return true;
        }

        @Override
        public void cancel() {
            synchronized (this) {
                finished = true;
            }
            call.cancel();
            release(this);
        }
    }

    private static AppointmentsError httpError(int status) {
        switch (status) {
            case 401: return AppointmentsError.UNAUTHORIZED;
            case 403: return AppointmentsError.FORBIDDEN;
            case 404: return AppointmentsError.NOT_FOUND;
            default: return AppointmentsError.SERVER;
        }
    }
}
