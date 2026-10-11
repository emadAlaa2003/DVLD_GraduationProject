package com.dvld.mobile.chat;

import com.dvld.mobile.model.ChatResponse;
import com.dvld.mobile.repository.ChatRepository;
import com.google.gson.Gson;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.Executor;
import org.junit.Before;
import org.junit.Test;
import static org.junit.Assert.*;

public class ChatRequestManagerTest {
    private FakeRepository repository;
    private ChatRequestManager manager;
    private int notifications;
    private ChatResponse answer;
    @Before public void setUp() {
        repository = new FakeRepository();
        manager = new ChatRequestManager(repository, Runnable::run, () -> notifications++);
        answer = new Gson().fromJson("{\"question\":\"السؤال\",\"answer\":\"إجابة\",\"sources\":[]}", ChatResponse.class);
    }
    @Test public void initialIdleAndEmpty() {
        assertFalse(manager.isLoading());
        assertTrue(manager.getHistory().isEmpty());
    }
    @Test public void startsQuestionImmediatelyAndTrimsIt() {
        assertEquals(ChatRequestManager.StartResult.STARTED, manager.ask("  السؤال  "));
        assertTrue(manager.isLoading());
        assertEquals("السؤال", repository.question);
        assertEquals("السؤال", manager.getHistory().get(0).getQuestion());
        assertEquals(ChatRequestManager.State.LOADING, manager.getHistory().get(0).getState());
    }
    @Test public void preventsSecondConcurrentQuestion() {
        manager.ask("السؤال");
        assertEquals(ChatRequestManager.StartResult.BUSY, manager.ask("سؤال آخر"));
        assertEquals(1, repository.calls);
        assertEquals(1, manager.getHistory().size());
    }
    @Test public void rejectsNullEmptyAndWhitespace() {
        for (String value : new String[] {null, "", " \t\n "})
            assertEquals(ChatRequestManager.StartResult.BLANK, manager.ask(value));
        assertEquals(0, repository.calls);
        assertTrue(manager.getHistory().isEmpty());
    }
    @Test public void departingListenerDoesNotCancelAndLoadingRemains() {
        Recorder activity = new Recorder();
        manager.addListener(activity);
        manager.ask("السؤال");
        manager.removeListener(activity);
        assertTrue(manager.isLoading());
        assertEquals(0, repository.cancellations);
        assertEquals(ChatRequestManager.State.LOADING, manager.getHistory().get(0).getState());
    }
    @Test public void recreatedActivityReattachesWithoutAnotherRequest() {
        Recorder oldActivity = new Recorder();
        manager.addListener(oldActivity);
        manager.ask("السؤال");
        manager.removeListener(oldActivity);
        Recorder newActivity = new Recorder();
        manager.addListener(newActivity);
        assertEquals(1, repository.calls);
        assertEquals(ChatRequestManager.State.LOADING, newActivity.latest.get(0).getState());
        assertEquals("السؤال", newActivity.latest.get(0).getQuestion());
    }
    @Test public void successUpdatesSameTurnAndClearsLoading() {
        manager.ask("السؤال");
        repository.callback.onSuccess(answer);
        assertFalse(manager.isLoading());
        assertEquals(1, manager.getHistory().size());
        assertEquals(ChatRequestManager.State.SUCCESS, manager.getHistory().get(0).getState());
        assertSame(answer, manager.getHistory().get(0).getResponse());
    }
    @Test public void failureUpdatesSameTurnAndClearsLoading() {
        manager.ask("السؤال");
        repository.callback.onError(ChatRepository.ChatError.NETWORK);
        assertFalse(manager.isLoading());
        assertEquals(1, manager.getHistory().size());
        assertEquals(ChatRequestManager.State.ERROR, manager.getHistory().get(0).getState());
        assertEquals(ChatRepository.ChatError.NETWORK, manager.getHistory().get(0).getError());
        assertEquals(0, notifications);
    }
    @Test public void allowsNextQuestionAfterSuccessAndKeepsHistory() {
        manager.ask("السؤال");
        repository.callback.onSuccess(answer);
        assertEquals(ChatRequestManager.StartResult.STARTED, manager.ask("سؤال ثان"));
        assertEquals(2, repository.calls);
        assertEquals(2, manager.getHistory().size());
        assertEquals(ChatRequestManager.State.SUCCESS, manager.getHistory().get(0).getState());
        assertEquals(ChatRequestManager.State.LOADING, manager.getHistory().get(1).getState());
        assertEquals("سؤال ثان", repository.question);
    }
    @Test public void permitsRetryAfterFailureAndRetainsFailedHistory() {
        manager.ask("السؤال");
        repository.callback.onError(ChatRepository.ChatError.SERVER);
        assertEquals(ChatRequestManager.StartResult.STARTED, manager.ask("السؤال"));
        assertEquals(2, repository.calls);
        assertEquals(ChatRequestManager.State.ERROR, manager.getHistory().get(0).getState());
        repository.callback.onSuccess(answer);
        assertEquals(ChatRequestManager.State.SUCCESS, manager.getHistory().get(1).getState());
    }
    @Test public void optionalSourcesDoNotPreventSuccess() {
        manager.ask("السؤال");
        repository.callback.onSuccess(new Gson().fromJson("{\"question\":\"السؤال\",\"answer\":\"جواب\"}", ChatResponse.class));
        assertEquals(ChatRequestManager.State.SUCCESS, manager.getHistory().get(0).getState());
        assertTrue(manager.getHistory().get(0).getResponse().getSources().isEmpty());
    }
    @Test public void noVisibleListenerNotifiesWhenReady() {
        Recorder activity = new Recorder();
        manager.addListener(activity);
        manager.ask("السؤال");
        manager.removeListener(activity);
        repository.callback.onSuccess(answer);
        assertEquals(1, notifications);
        Recorder returned = new Recorder();
        manager.addListener(returned);
        assertEquals(ChatRequestManager.State.SUCCESS, returned.latest.get(0).getState());
        assertEquals(1, repository.calls);
    }
    @Test public void visibleListenerGetsAnswerWithoutNotification() {
        Recorder activity = new Recorder();
        manager.addListener(activity);
        manager.ask("السؤال");
        repository.callback.onSuccess(answer);
        assertEquals(0, notifications);
        assertEquals(ChatRequestManager.State.SUCCESS, activity.latest.get(0).getState());
    }
    @Test public void oneRemainingVisibleListenerSuppressesNotification() {
        Recorder first = new Recorder();
        Recorder second = new Recorder();
        manager.addListener(first); manager.addListener(second);
        manager.ask("السؤال");
        manager.removeListener(first);
        repository.callback.onSuccess(answer);
        assertEquals(0, notifications);
        assertEquals(ChatRequestManager.State.LOADING, first.latest.get(0).getState());
        assertEquals(ChatRequestManager.State.SUCCESS, second.latest.get(0).getState());
    }
    @Test public void historySnapshotsAreImmutableAndDetached() {
        manager.ask("السؤال");
        List<ChatRequestManager.Turn> snapshot = manager.getHistory();
        try { snapshot.clear(); fail("mutable history"); } catch (UnsupportedOperationException expected) { }
        repository.callback.onSuccess(answer);
        assertEquals(ChatRequestManager.State.LOADING, snapshot.get(0).getState());
        assertEquals(ChatRequestManager.State.SUCCESS, manager.getHistory().get(0).getState());
    }
    @Test public void staleOrDuplicateCallbackCannotChangeNewActiveQuestion() {
        manager.ask("السؤال");
        ChatRepository.ChatCallback old = repository.callback;
        old.onSuccess(answer);
        manager.ask("سؤال ثان");
        old.onError(ChatRepository.ChatError.SERVER);
        old.onSuccess(answer);
        assertTrue(manager.isLoading());
        assertEquals(ChatRequestManager.State.LOADING, manager.getHistory().get(1).getState());
        assertEquals(1, notifications);
    }
    @Test public void callbacksUseProvidedExecutorAndCurrentVisibility() {
        List<Runnable> pending = new ArrayList<>();
        Executor queued = pending::add;
        manager = new ChatRequestManager(repository, queued, () -> notifications++);
        manager.ask("السؤال");
        repository.callback.onSuccess(answer);
        assertTrue(manager.isLoading());
        Recorder activity = new Recorder();
        manager.addListener(activity);
        pending.remove(0).run();
        assertFalse(manager.isLoading());
        assertEquals(0, notifications);
        assertEquals(ChatRequestManager.State.SUCCESS, activity.latest.get(0).getState());
    }
    @Test public void synchronousRepositoryDoesNotLeaveManagerBusy() {
        manager = new ChatRequestManager((question, callback) -> {
            callback.onSuccess(answer); return () -> { };
        }, Runnable::run, () -> notifications++);
        manager.ask("السؤال");
        assertFalse(manager.isLoading());
        manager.ask("السؤال الثاني");
        assertEquals(2, manager.getHistory().size());
        assertEquals(2, notifications);
    }
    @Test public void unexpectedRepositoryFailureDoesNotLeaveLoadingStuck() {
        manager = new ChatRequestManager((question, callback) -> { throw new IllegalStateException("fixture"); },
                Runnable::run, () -> notifications++);
        manager.ask("السؤال");
        assertFalse(manager.isLoading());
        assertEquals(ChatRepository.ChatError.SERVER, manager.getHistory().get(0).getError());
    }
    @Test public void invalidSuccessBecomesServerErrorWithoutNotification() {
        for (ChatResponse response : new ChatResponse[] {null, new Gson().fromJson("{}", ChatResponse.class)}) {
            manager.ask("السؤال");
            repository.callback.onSuccess(response);
            assertFalse(manager.isLoading());
            assertEquals(ChatRepository.ChatError.SERVER,
                    manager.getHistory().get(manager.getHistory().size() - 1).getError());
        }
        assertEquals(0, notifications);
    }
    private static final class Recorder implements ChatRequestManager.Listener {
        private List<ChatRequestManager.Turn> latest;
        @Override public void onChanged(List<ChatRequestManager.Turn> history) { latest = history; }
    }
    private static final class FakeRepository implements ChatRepository {
        private int calls;
        private int cancellations;
        private String question;
        private ChatCallback callback;
        @Override public RequestHandle ask(String question, ChatCallback callback) {
            calls++; this.question = question; this.callback = callback;
            return () -> cancellations++;
        }
    }
}
