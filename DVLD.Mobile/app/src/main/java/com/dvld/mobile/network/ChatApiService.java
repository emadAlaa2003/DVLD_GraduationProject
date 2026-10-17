package com.dvld.mobile.network;

import com.dvld.mobile.model.ChatRequest;
import com.dvld.mobile.model.ChatResponse;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.POST;

public interface ChatApiService {
    @POST("api/knowledge-documents/chat")
    Call<ChatResponse> ask(@Body ChatRequest request);
}
