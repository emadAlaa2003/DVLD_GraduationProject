package com.dvld.mobile.repository;

import com.dvld.mobile.model.PracticeExamAnswer;
import com.dvld.mobile.model.PracticeExamStartResponse;
import com.dvld.mobile.model.PracticeExamSubmitResponse;
import java.util.List;

public interface PracticeExamRepository {
    // Concurrent operations share the pending handle and its original callback.
    RequestHandle startExam(int questionCount, ExamCallback<PracticeExamStartResponse> callback);
    RequestHandle submitExam(List<PracticeExamAnswer> answers, ExamCallback<PracticeExamSubmitResponse> callback);
    interface RequestHandle { void cancel(); }
    interface ExamCallback<T> {
        void onSuccess(T response);
        void onError(ExamError error);
    }
    enum ExamError { NETWORK, UNAUTHORIZED, FORBIDDEN, CONFLICT, BAD_REQUEST, SERVER }
}
