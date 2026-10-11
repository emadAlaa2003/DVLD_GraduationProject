package com.dvld.mobile.repository;

import com.dvld.mobile.model.ChatRequest;
import com.dvld.mobile.model.ChatResponse;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.ChatApiService;
import com.google.gson.stream.MalformedJsonException;
import java.io.EOFException;
import java.io.IOException;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public final class ApiChatRepository implements ChatRepository {
    private final ChatApiService service;
    public ApiChatRepository() { this(ApiClient.getChatService()); }
    public ApiChatRepository(ChatApiService service) { this.service = service; }

    @Override public RequestHandle ask(String question, ChatCallback callback) {
        if (question == null || question.trim().isEmpty()) {
            callback.onError(ChatError.BAD_REQUEST);
            return () -> { };
        }
        Call<ChatResponse> call = service.ask(new ChatRequest(question.trim()));
        call.enqueue(new Callback<ChatResponse>() {
            @Override public void onResponse(Call<ChatResponse> completed, Response<ChatResponse> response) {
                ResponseBody errorBody = response.errorBody();
                if (errorBody != null) errorBody.close();
                if (completed.isCanceled()) return;
                ChatResponse body = response.body();
                if (response.code() == 200 && body != null && body.isValid()) callback.onSuccess(body);
                else callback.onError(httpError(response.code()));
            }
            @Override public void onFailure(Call<ChatResponse> failed, Throwable error) {
                if (failed.isCanceled()) return;
                boolean network = error instanceof IOException && !(error instanceof MalformedJsonException)
                        && !(error instanceof EOFException);
                callback.onError(network ? ChatError.NETWORK : ChatError.SERVER);
            }
        });
        return call::cancel;
    }

    private static ChatError httpError(int status) {
        switch (status) {
            case 400: return ChatError.BAD_REQUEST;
            case 401: return ChatError.UNAUTHORIZED;
            case 403: return ChatError.FORBIDDEN;
            default: return ChatError.SERVER;
        }
    }
}
