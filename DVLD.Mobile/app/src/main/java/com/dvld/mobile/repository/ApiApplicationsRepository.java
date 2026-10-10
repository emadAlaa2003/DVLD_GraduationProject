package com.dvld.mobile.repository;

import com.dvld.mobile.model.CitizenApplication;
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

public final class ApiApplicationsRepository implements ApplicationsRepository {
    private final DashboardApiService service;
    private Request inFlight;

    public ApiApplicationsRepository() {
        this(ApiClient.getDashboardService());
    }

    public ApiApplicationsRepository(DashboardApiService service) {
        this.service = service;
    }

    @Override
    public synchronized RequestHandle load(int personId, ApplicationsCallback callback) {
        if (inFlight != null) return inFlight;
        if (personId <= 0) {
            callback.onError(ApplicationsError.NOT_FOUND);
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
        private final Call<List<CitizenApplication>> call;
        private final ApplicationsCallback callback;
        private boolean finished;

        private Request(int personId, ApplicationsCallback callback) {
            this.callback = callback;
            call = service.getApplications(personId);
        }

        private void start() {
            call.enqueue(new Callback<List<CitizenApplication>>() {
                @Override
                public void onResponse(Call<List<CitizenApplication>> completedCall,
                                       Response<List<CitizenApplication>> response) {
                    ResponseBody errorBody = response.errorBody();
                    if (errorBody != null) errorBody.close();
                    if (!complete()) return;
                    List<CitizenApplication> body = response.body();
                    if (response.code() == 200 && body != null) {
                        List<CitizenApplication> applications = new ArrayList<>();
                        for (CitizenApplication application : body) {
                            if (application != null) applications.add(application);
                        }
                        callback.onSuccess(Collections.unmodifiableList(applications));
                    } else {
                        callback.onError(httpError(response.code()));
                    }
                }

                @Override
                public void onFailure(Call<List<CitizenApplication>> failedCall, Throwable error) {
                    if (!complete()) return;
                    boolean network = error instanceof IOException &&
                            !(error instanceof MalformedJsonException) && !(error instanceof EOFException);
                    callback.onError(network ? ApplicationsError.NETWORK : ApplicationsError.SERVER);
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

    private static ApplicationsError httpError(int status) {
        switch (status) {
            case 401: return ApplicationsError.UNAUTHORIZED;
            case 403: return ApplicationsError.FORBIDDEN;
            case 404: return ApplicationsError.NOT_FOUND;
            default: return ApplicationsError.SERVER;
        }
    }
}
