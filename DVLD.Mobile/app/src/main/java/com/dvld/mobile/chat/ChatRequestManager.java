package com.dvld.mobile.chat;

import com.dvld.mobile.model.ChatResponse;
import com.dvld.mobile.repository.ChatRepository;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;
import java.util.concurrent.Executor;

/** Process-memory conversation. No Activity, View, or Android framework dependencies. */
public final class ChatRequestManager {
    public interface Listener { void onChanged(List<Turn> history); }
    public interface CompletionNotifier { void onAnswerReady(); }
    public enum StartResult { STARTED, BLANK, BUSY }
    public enum State { LOADING, SUCCESS, ERROR }

    public static final class Turn {
        private final String question;
        private final State state;
        private final ChatResponse response;
        private final ChatRepository.ChatError error;
        private Turn(String question, State state, ChatResponse response, ChatRepository.ChatError error) {
            this.question = question; this.state = state; this.response = response; this.error = error;
        }
        public String getQuestion() { return question; }
        public State getState() { return state; }
        public ChatResponse getResponse() { return response; }
        public ChatRepository.ChatError getError() { return error; }
    }

    private final ChatRepository repository;
    private final Executor callbackExecutor;
    private final CompletionNotifier notifier;
    private final List<Turn> history = new ArrayList<>();
    private final Set<Listener> listeners = new LinkedHashSet<>();
    private int activeIndex = -1;
    // Owned here, never handed to an Activity or cancelled when a listener leaves.
    private ChatRepository.RequestHandle activeRequest;

    public ChatRequestManager(ChatRepository repository, Executor callbackExecutor, CompletionNotifier notifier) {
        this.repository = repository; this.callbackExecutor = callbackExecutor; this.notifier = notifier;
    }

    public synchronized List<Turn> getHistory() {
        return Collections.unmodifiableList(new ArrayList<>(history));
    }
    public synchronized boolean isLoading() { return activeIndex >= 0; }
    public synchronized void addListener(Listener listener) {
        listeners.add(listener);
        listener.onChanged(getHistory());
    }
    public synchronized void removeListener(Listener listener) { listeners.remove(listener); }

    public synchronized StartResult ask(String question) {
        if (question == null || question.trim().isEmpty()) return StartResult.BLANK;
        if (isLoading()) return StartResult.BUSY;
        String trimmed = question.trim();
        int index = history.size();
        activeIndex = index;
        history.add(new Turn(trimmed, State.LOADING, null, null));
        publish();
        try {
            ChatRepository.RequestHandle handle = repository.ask(trimmed, new ChatRepository.ChatCallback() {
                @Override public void onSuccess(ChatResponse response) {
                    callbackExecutor.execute(() -> complete(index, response, null));
                }
                @Override public void onError(ChatRepository.ChatError error) {
                    callbackExecutor.execute(() -> complete(index, null, error));
                }
            });
            // A test repository may complete synchronously before ask returns.
            if (activeIndex == index) activeRequest = handle;
        } catch (RuntimeException exception) {
            complete(index, null, ChatRepository.ChatError.SERVER);
        }
        return StartResult.STARTED;
    }

    private synchronized void complete(int index, ChatResponse response, ChatRepository.ChatError error) {
        if (activeIndex != index) return;
        boolean success = error == null && response != null && response.isValid();
        Turn original = history.get(index);
        history.set(index, new Turn(original.question, success ? State.SUCCESS : State.ERROR,
                success ? response : null, success ? null : error == null ? ChatRepository.ChatError.SERVER : error));
        activeIndex = -1;
        activeRequest = null;
        boolean notify = success && listeners.isEmpty();
        publish();
        if (notify) notifier.onAnswerReady();
    }

    private void publish() {
        List<Turn> snapshot = getHistory();
        for (Listener listener : new ArrayList<>(listeners)) {
            if (listeners.contains(listener)) listener.onChanged(snapshot);
        }
    }
}
