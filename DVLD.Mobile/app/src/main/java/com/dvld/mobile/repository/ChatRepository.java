package com.dvld.mobile.repository;

import com.dvld.mobile.model.ChatResponse;

public interface ChatRepository {
    RequestHandle ask(String question, ChatCallback callback);
    interface RequestHandle { void cancel(); }
    interface ChatCallback {
        void onSuccess(ChatResponse response);
        void onError(ChatError error);
    }
    enum ChatError { BAD_REQUEST, UNAUTHORIZED, FORBIDDEN, NETWORK, SERVER }
}
