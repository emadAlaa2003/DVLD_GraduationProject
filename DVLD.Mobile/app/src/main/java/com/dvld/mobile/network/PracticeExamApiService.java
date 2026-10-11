package com.dvld.mobile.network;

import com.dvld.mobile.model.PracticeExamStartRequest;
import com.dvld.mobile.model.PracticeExamStartResponse;
import com.dvld.mobile.model.PracticeExamSubmitRequest;
import com.dvld.mobile.model.PracticeExamSubmitResponse;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface PracticeExamApiService {
    @POST("api/practice-exams/start")
    Call<PracticeExamStartResponse> startExam(@Body PracticeExamStartRequest request);
    @POST("api/practice-exams/submit")
    Call<PracticeExamSubmitResponse> submitExam(@Body PracticeExamSubmitRequest request);
}
