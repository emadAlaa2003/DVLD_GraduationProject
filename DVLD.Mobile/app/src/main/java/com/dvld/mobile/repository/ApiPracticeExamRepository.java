package com.dvld.mobile.repository;

import com.dvld.mobile.model.*;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.PracticeExamApiService;
import com.google.gson.stream.MalformedJsonException;
import java.io.EOFException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.function.Predicate;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiPracticeExamRepository implements PracticeExamRepository {
    private final PracticeExamApiService service;
    private Request<?> inFlight;

    public ApiPracticeExamRepository() { this(ApiClient.getPracticeExamService()); }
    public ApiPracticeExamRepository(PracticeExamApiService service) { this.service = service; }

    @Override
    public synchronized RequestHandle startExam(int count, ExamCallback<PracticeExamStartResponse> callback) {
        if (inFlight != null) return inFlight;
        if (count < 1 || count > 20) {
            callback.onError(ExamError.BAD_REQUEST);
            return () -> { };
        }
        return execute(service.startExam(new PracticeExamStartRequest(count)),
                body -> PracticeExamContract.validStart(body, count), callback);
    }

    @Override
    public synchronized RequestHandle submitExam(List<PracticeExamAnswer> answers, ExamCallback<PracticeExamSubmitResponse> callback) {
        if (inFlight != null) return inFlight;
        if (!PracticeExamContract.validAnswers(answers)) {
            callback.onError(ExamError.BAD_REQUEST);
            return () -> { };
        }
        List<PracticeExamAnswer> snapshot = new ArrayList<>(answers);
        return execute(service.submitExam(new PracticeExamSubmitRequest(snapshot)),
                body -> PracticeExamContract.validSubmit(body, snapshot), callback);
    }

    private <T> RequestHandle execute(Call<T> call, Predicate<T> validBody, ExamCallback<T> callback) {
        Request<T> request = new Request<>(call, validBody, callback);
        inFlight = request;
        request.start();
        return request;
    }

    private synchronized void release(Request<?> request) {
        if (inFlight == request) inFlight = null;
    }

    private final class Request<T> implements RequestHandle {
        private final Call<T> call;
        private final Predicate<T> validBody;
        private final ExamCallback<T> callback;
        private boolean finished;
        private Request(Call<T> call, Predicate<T> validBody, ExamCallback<T> callback) {
            this.call = call;
            this.validBody = validBody;
            this.callback = callback;
        }
        private void start() {
            call.enqueue(new Callback<T>() {
                @Override
                public void onResponse(Call<T> completedCall, Response<T> response) {
                    ResponseBody errorBody = response.errorBody();
                    if (errorBody != null) errorBody.close();
                    if (!complete()) return;
                    if (response.code() == 200 && validBody.test(response.body())) callback.onSuccess(response.body());
                    else callback.onError(httpError(response.code()));
                }
                @Override
                public void onFailure(Call<T> failedCall, Throwable error) {
                    if (!complete()) return;
                    boolean network = error instanceof IOException && !(error instanceof MalformedJsonException)
                            && !(error instanceof EOFException);
                    callback.onError(network ? ExamError.NETWORK : ExamError.SERVER);
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
            synchronized (this) { finished = true; }
            call.cancel();
            release(this);
        }
    }

    private static ExamError httpError(int status) {
        switch (status) {
            case 400: return ExamError.BAD_REQUEST;
            case 401: return ExamError.UNAUTHORIZED;
            case 403: return ExamError.FORBIDDEN;
            case 409: return ExamError.CONFLICT;
            default: return ExamError.SERVER;
        }
    }
}
