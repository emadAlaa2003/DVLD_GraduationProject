package com.dvld.mobile.repository;

import com.dvld.mobile.model.InternationalLicenseDetails;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.DashboardApiService;
import com.google.gson.stream.MalformedJsonException;

import java.io.EOFException;
import java.io.IOException;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiInternationalLicenseDetailsRepository implements InternationalLicenseDetailsRepository {
    private final DashboardApiService service;
    private Request inFlight;

    public ApiInternationalLicenseDetailsRepository() {
        this(ApiClient.getDashboardService());
    }

    public ApiInternationalLicenseDetailsRepository(DashboardApiService service) {
        this.service = service;
    }

    @Override
    public synchronized RequestHandle load(int internationalLicenseId, DetailsCallback callback) {
        if (inFlight != null) return inFlight;
        if (internationalLicenseId <= 0) {
            callback.onError(DetailsError.NOT_FOUND);
            return () -> { };
        }
        Request request = new Request(internationalLicenseId, callback);
        inFlight = request;
        request.start();
        return request;
    }

    private synchronized void release(Request request) {
        if (inFlight == request) inFlight = null;
    }

    private final class Request implements RequestHandle {
        private final Call<InternationalLicenseDetails> call;
        private final DetailsCallback callback;
        private final int internationalLicenseId;
        private boolean finished;

        private Request(int internationalLicenseId, DetailsCallback callback) {
            this.internationalLicenseId = internationalLicenseId;
            this.callback = callback;
            call = service.getInternationalLicenseDetails(internationalLicenseId);
        }

        private void start() {
            call.enqueue(new Callback<InternationalLicenseDetails>() {
                @Override
                public void onResponse(Call<InternationalLicenseDetails> completedCall, Response<InternationalLicenseDetails> response) {
                    ResponseBody errorBody = response.errorBody();
                    if (errorBody != null) errorBody.close();
                    if (!complete()) return;
                    InternationalLicenseDetails body = response.body();
                    if (response.code() == 200 && body != null &&
                            Integer.valueOf(internationalLicenseId).equals(body.getInternationalLicenseID())) {
                        callback.onSuccess(body);
                    } else {
                        callback.onError(httpError(response.code()));
                    }
                }

                @Override
                public void onFailure(Call<InternationalLicenseDetails> failedCall, Throwable error) {
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
