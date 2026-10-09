package com.dvld.mobile.repository;

import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.model.DashboardData;
import com.dvld.mobile.model.InternationalLicense;
import com.dvld.mobile.model.LocalLicense;
import com.dvld.mobile.model.TestAppointment;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.DashboardApiService;
import com.google.gson.stream.MalformedJsonException;

import java.io.EOFException;
import java.io.IOException;
import java.util.Arrays;
import java.util.List;
import java.util.function.Consumer;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiDashboardRepository implements DashboardRepository {
    private final DashboardApiService service;
    private Batch inFlight;

    public ApiDashboardRepository() {
        this(ApiClient.getDashboardService());
    }

    public ApiDashboardRepository(DashboardApiService service) {
        this.service = service;
    }

    @Override
    public synchronized RequestHandle load(int personId, DashboardCallback callback) {
        if (inFlight != null) {
            return inFlight;
        }
        if (personId <= 0) {
            callback.onError(DashboardError.NOT_FOUND);
            return () -> { };
        }
        Batch batch = new Batch(personId, callback);
        inFlight = batch;
        batch.start();
        return batch;
    }

    private synchronized void release(Batch batch) {
        if (inFlight == batch) {
            inFlight = null;
        }
    }

    private final class Batch implements RequestHandle {
        private final DashboardCallback callback;
        private final Call<List<LocalLicense>> localCall;
        private final Call<List<InternationalLicense>> internationalCall;
        private final Call<List<CitizenApplication>> applicationsCall;
        private final Call<List<TestAppointment>> appointmentsCall;
        private final List<Call<?>> calls;
        private List<LocalLicense> localLicenses;
        private List<InternationalLicense> internationalLicenses;
        private List<CitizenApplication> applications;
        private List<TestAppointment> appointments;
        private int remaining = 4;
        private boolean finished;

        private Batch(int personId, DashboardCallback callback) {
            this.callback = callback;
            localCall = service.getLocalLicenses(personId);
            internationalCall = service.getInternationalLicenses(personId);
            applicationsCall = service.getApplications(personId);
            appointmentsCall = service.getTestAppointments(personId);
            calls = Arrays.asList(localCall, internationalCall, applicationsCall, appointmentsCall);
        }

        private void start() {
            enqueue(localCall, value -> localLicenses = value);
            enqueue(internationalCall, value -> internationalLicenses = value);
            enqueue(applicationsCall, value -> applications = value);
            enqueue(appointmentsCall, value -> appointments = value);
        }

        private <T> void enqueue(Call<List<T>> call, Consumer<List<T>> store) {
            call.enqueue(new Callback<List<T>>() {
                @Override
                public void onResponse(Call<List<T>> completedCall, Response<List<T>> response) {
                    ResponseBody errorBody = response.errorBody();
                    if (errorBody != null) {
                        errorBody.close();
                    }
                    if (response.code() == 200 && response.body() != null) {
                        accept(response.body(), store);
                    } else {
                        fail(httpError(response.code()));
                    }
                }

                @Override
                public void onFailure(Call<List<T>> failedCall, Throwable error) {
                    boolean network = error instanceof IOException &&
                            !(error instanceof MalformedJsonException) && !(error instanceof EOFException);
                    fail(network ? DashboardError.NETWORK : DashboardError.SERVER);
                }
            });
        }

        private <T> void accept(List<T> items, Consumer<List<T>> store) {
            DashboardData data;
            synchronized (this) {
                if (finished) {
                    return;
                }
                store.accept(items);
                if (--remaining != 0) {
                    return;
                }
                data = new DashboardData(localLicenses, internationalLicenses, applications, appointments);
                finished = true;
            }
            release(this);
            callback.onSuccess(data);
        }

        private void fail(DashboardError error) {
            synchronized (this) {
                if (finished) {
                    return;
                }
                finished = true;
            }
            cancelCalls();
            release(this);
            callback.onError(error);
        }

        @Override
        public void cancel() {
            synchronized (this) {
                finished = true;
            }
            cancelCalls();
            release(this);
        }

        private void cancelCalls() {
            for (Call<?> call : calls) {
                call.cancel();
            }
        }
    }

    private static DashboardError httpError(int status) {
        switch (status) {
            case 401: return DashboardError.UNAUTHORIZED;
            case 403: return DashboardError.FORBIDDEN;
            case 404: return DashboardError.NOT_FOUND;
            default: return DashboardError.SERVER;
        }
    }
}
