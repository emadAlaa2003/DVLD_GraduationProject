package com.dvld.mobile.repository;

import com.dvld.mobile.model.TestAppointmentDetails;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.DashboardApiService;
import com.google.gson.stream.MalformedJsonException;

import java.io.EOFException;
import java.io.IOException;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiTestAppointmentDetailsRepository implements TestAppointmentDetailsRepository {
    private final DashboardApiService service;
    private Request inFlight;

    public ApiTestAppointmentDetailsRepository() {
        this(ApiClient.getDashboardService());
    }

    public ApiTestAppointmentDetailsRepository(DashboardApiService service) {
        this.service = service;
    }

    @Override
    public synchronized RequestHandle load(int testAppointmentId, DetailsCallback callback) {
        if (inFlight != null) return inFlight;
        if (testAppointmentId <= 0) {
            callback.onError(DetailsError.NOT_FOUND);
            return () -> { };
        }
        Request request = new Request(testAppointmentId, callback);
        inFlight = request;
        request.start();
        return request;
    }

    private synchronized void release(Request request) {
        if (inFlight == request) inFlight = null;
    }

    private final class Request implements RequestHandle {
        private final Call<TestAppointmentDetails> call;
        private final DetailsCallback callback;
        private final int testAppointmentId;
        private boolean finished;

        private Request(int testAppointmentId, DetailsCallback callback) {
            this.testAppointmentId = testAppointmentId;
            this.callback = callback;
            call = service.getTestAppointmentDetails(testAppointmentId);
        }

        private void start() {
            call.enqueue(new Callback<TestAppointmentDetails>() {
                @Override
                public void onResponse(Call<TestAppointmentDetails> completedCall, Response<TestAppointmentDetails> response) {
                    ResponseBody errorBody = response.errorBody();
                    if (errorBody != null) errorBody.close();
                    if (!complete()) return;
                    TestAppointmentDetails body = response.body();
                    if (response.code() == 200 && body != null &&
                            Integer.valueOf(testAppointmentId).equals(body.getTestAppointmentID())) {
                        callback.onSuccess(body);
                    } else {
                        callback.onError(httpError(response.code()));
                    }
                }

                @Override
                public void onFailure(Call<TestAppointmentDetails> failedCall, Throwable error) {
                    if (!complete()) return;
                    boolean network = error instanceof IOException &&
                            !(error instanceof MalformedJsonException) && !(error instanceof EOFException);
                    callback.onError(network ? DetailsError.NETWORK : DetailsError.SERVER);
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

    private static DetailsError httpError(int status) {
        switch (status) {
            case 401: return DetailsError.UNAUTHORIZED;
            case 403: return DetailsError.FORBIDDEN;
            case 404: return DetailsError.NOT_FOUND;
            default: return DetailsError.SERVER;
        }
    }
}
