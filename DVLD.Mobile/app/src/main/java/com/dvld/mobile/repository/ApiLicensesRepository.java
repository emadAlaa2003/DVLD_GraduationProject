package com.dvld.mobile.repository;

import com.dvld.mobile.model.InternationalLicense;
import com.dvld.mobile.model.LicensesData;
import com.dvld.mobile.model.LocalLicense;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.DashboardApiService;
import com.google.gson.stream.MalformedJsonException;

import java.io.EOFException;
import java.io.IOException;
import java.util.List;
import java.util.function.Consumer;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiLicensesRepository implements LicensesRepository {
    private final DashboardApiService service;
    private Batch inFlight;

    public ApiLicensesRepository() {
        this(ApiClient.getDashboardService());
    }

    public ApiLicensesRepository(DashboardApiService service) {
        this.service = service;
    }

    @Override
    public synchronized RequestHandle load(int personId, LicensesCallback callback) {
        if (inFlight != null) return inFlight;
        if (personId <= 0) {
            callback.onError(LicensesError.NOT_FOUND);
            return () -> { };
        }
        Batch batch = new Batch(personId, callback);
        inFlight = batch;
        batch.start();
        return batch;
    }

    private synchronized void release(Batch batch) {
        if (inFlight == batch) inFlight = null;
    }

    private final class Batch implements RequestHandle {
        private final LicensesCallback callback;
        private final Call<List<LocalLicense>> localCall;
        private final Call<List<InternationalLicense>> internationalCall;
        private List<LocalLicense> localLicenses;
        private List<InternationalLicense> internationalLicenses;
        private int remaining = 2;
        private boolean finished;

        private Batch(int personId, LicensesCallback callback) {
            this.callback = callback;
            localCall = service.getLocalLicenses(personId);
            internationalCall = service.getInternationalLicenses(personId);
        }

        private void start() {
            enqueue(localCall, items -> localLicenses = items);
            enqueue(internationalCall, items -> internationalLicenses = items);
        }

        private <T> void enqueue(Call<List<T>> call, Consumer<List<T>> store) {
            call.enqueue(new Callback<List<T>>() {
                @Override
                public void onResponse(Call<List<T>> completedCall, Response<List<T>> response) {
                    ResponseBody errorBody = response.errorBody();
                    if (errorBody != null) errorBody.close();
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
                    fail(network ? LicensesError.NETWORK : LicensesError.SERVER);
                }
            });
        }

        private <T> void accept(List<T> items, Consumer<List<T>> store) {
            LicensesData data;
            synchronized (this) {
                if (finished) return;
                store.accept(items);
                if (--remaining != 0) return;
                data = new LicensesData(localLicenses, internationalLicenses);
                finished = true;
            }
            release(this);
            callback.onSuccess(data);
        }

        private void fail(LicensesError error) {
            synchronized (this) {
                if (finished) return;
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
            localCall.cancel();
            internationalCall.cancel();
        }
    }

    private static LicensesError httpError(int status) {
        switch (status) {
            case 401: return LicensesError.UNAUTHORIZED;
            case 403: return LicensesError.FORBIDDEN;
            case 404: return LicensesError.NOT_FOUND;
            default: return LicensesError.SERVER;
        }
    }
}
