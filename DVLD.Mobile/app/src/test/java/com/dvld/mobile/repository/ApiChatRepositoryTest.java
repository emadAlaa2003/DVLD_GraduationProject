package com.dvld.mobile.repository;

import com.dvld.mobile.model.ChatResponse;
import com.dvld.mobile.model.ChatSource;
import com.dvld.mobile.network.ApiClient;
import com.dvld.mobile.network.ChatApiService;
import com.dvld.mobile.network.SessionCookieJar;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import java.util.Collections;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import okhttp3.Cookie;
import okhttp3.OkHttpClient;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import static org.junit.Assert.*;

public class ApiChatRepositoryTest {
    private static final String ANSWER = "{\"question\":\"السؤال\",\"answer\":\"إجابة مرورية\",";
    private MockWebServer server;
    private OkHttpClient client;
    private ChatRepository repository;
    @Before public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=chat-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        ChatApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run).addConverterFactory(GsonConverterFactory.create())
                .build().create(ChatApiService.class);
        repository = new ApiChatRepository(service);
    }
    @After public void tearDown() throws Exception {
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test public void successParsesAnswerAndAllSourceFields() throws Exception {
        server.enqueue(json(200, ANSWER + "\"sources\":[{\"documentID\":7,\"pageNumber\":101,\"chunkIndex\":147,"
                + "\"semanticScore\":0.645022,\"rerankerScore\":0.5442991}]}"));
        Result result = ask("السؤال");
        assertNull(result.error);
        assertEquals("إجابة مرورية", result.body.getAnswer());
        assertEquals("السؤال", result.body.getQuestion());
        ChatSource source = result.body.getSources().get(0);
        assertEquals(Long.valueOf(7), source.getDocumentID());
        assertEquals(Long.valueOf(101), source.getPageNumber());
        assertEquals(Long.valueOf(147), source.getChunkIndex());
        assertEquals(0.645022, source.getSemanticScore(), 0.0000001);
        assertEquals(0.5442991, source.getRerankerScore(), 0.0000001);
    }
    @Test public void sendsTrimmedStandaloneQuestionWithSharedCookieAndNoBearer() throws Exception {
        server.enqueue(json(200, ANSWER + "\"sources\":[]}"));
        ask("  السؤال  ");
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertEquals("POST", request.getMethod());
        assertEquals("/api/knowledge-documents/chat", request.getPath());
        assertEquals("DVLD.MobileAuth=chat-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        JsonObject body = JsonParser.parseString(request.getBody().readUtf8()).getAsJsonObject();
        assertEquals(1, body.size());
        assertEquals("السؤال", body.get("question").getAsString());
    }
    @Test public void emptySourcesAreValid() throws Exception {
        server.enqueue(json(200, ANSWER + "\"sources\":[]}"));
        assertTrue(ask("السؤال").body.getSources().isEmpty());
    }
    @Test public void missingSourcesAreValid() throws Exception {
        server.enqueue(json(200, "{\"question\":\"السؤال\",\"answer\":\"الجواب\"}"));
        assertTrue(ask("السؤال").body.getSources().isEmpty());
    }
    @Test public void nullSourcesAreValid() throws Exception {
        server.enqueue(json(200, ANSWER + "\"sources\":null}"));
        assertTrue(ask("السؤال").body.getSources().isEmpty());
    }
    @Test public void nullSourceEntriesAndNullableFieldsAreSafe() throws Exception {
        server.enqueue(json(200, ANSWER + "\"sources\":[null,{\"documentID\":null}]}"));
        ChatSource source = ask("السؤال").body.getSources().get(0);
        assertNull(source.getDocumentID());
        assertNull(source.getPageNumber());
        assertNull(source.getSemanticScore());
    }
    @Test public void badRequest400() throws Exception { error(400, "{}", ChatRepository.ChatError.BAD_REQUEST); }
    @Test public void unauthorized401() throws Exception { error(401, "{}", ChatRepository.ChatError.UNAUTHORIZED); }
    @Test public void forbidden403() throws Exception { error(403, "{}", ChatRepository.ChatError.FORBIDDEN); }
    @Test public void server500() throws Exception { error(500, "{}", ChatRepository.ChatError.SERVER); }
    @Test public void otherHttpCodesAreServerErrors() throws Exception {
        for (int code : new int[] {404, 409, 502, 204}) error(code, "", ChatRepository.ChatError.SERVER);
    }
    @Test public void networkFailure() throws Exception {
        server.shutdown();
        assertEquals(ChatRepository.ChatError.NETWORK, ask("السؤال").error);
    }
    @Test public void malformedBody() throws Exception {
        for (String body : new String[] {"not-json", "[]", "[", "{\"answer\":", ANSWER + "\"sources\":42}"})
            error(200, body, ChatRepository.ChatError.SERVER);
    }
    @Test public void nullAndEmptyBodies() throws Exception {
        for (String body : new String[] {"null", ""}) error(200, body, ChatRepository.ChatError.SERVER);
    }
    @Test public void missingOrBlankAnswerOrQuestionIsServerError() throws Exception {
        for (String body : new String[] {"{}", "{\"question\":\"سؤال\"}", "{\"question\":\"سؤال\",\"answer\":\" \"}",
                "{\"answer\":\"الجواب\"}", "{\"question\":null,\"answer\":\"الجواب\"}"})
            error(200, body, ChatRepository.ChatError.SERVER);
    }
    @Test public void blankInputNeverCallsServer() throws Exception {
        for (String question : new String[] {null, "", " \t\n "})
            assertEquals(ChatRepository.ChatError.BAD_REQUEST, ask(question).error);
        assertEquals(0, server.getRequestCount());
    }
    @Test public void delayedReplyIsAsynchronous() throws Exception {
        server.enqueue(json(200, ANSWER + "\"sources\":[]}").setBodyDelay(200, TimeUnit.MILLISECONDS));
        Result result = new Result();
        repository.ask("السؤال", result);
        assertEquals(1L, result.done.getCount());
        result.await();
        assertNotNull(result.body);
    }
    @Test public void dedicatedClientClonesCookieJarAndKeepsOrdinaryTimeouts() {
        OkHttpClient ordinary = ApiClient.getHttpClient();
        OkHttpClient chat = ApiClient.getChatHttpClient();
        assertNotSame(ordinary, chat);
        assertSame(ordinary.cookieJar(), chat.cookieJar());
        assertSame(ordinary.connectionPool(), chat.connectionPool());
        assertSame(ordinary.dispatcher(), chat.dispatcher());
        assertEquals(30000, ordinary.callTimeoutMillis());
        assertEquals(15000, ordinary.readTimeoutMillis());
        assertEquals(15000, ordinary.writeTimeoutMillis());
        assertEquals(300000, chat.callTimeoutMillis());
        assertEquals(300000, chat.readTimeoutMillis());
        assertEquals(300000, chat.writeTimeoutMillis());
        assertEquals(ordinary.connectTimeoutMillis(), chat.connectTimeoutMillis());
    }
    private Result ask(String question) throws Exception {
        Result result = new Result();
        repository.ask(question, result);
        result.await();
        return result;
    }
    private void error(int code, String body, ChatRepository.ChatError expected) throws Exception {
        server.enqueue(json(code, body));
        Result result = ask("السؤال");
        assertEquals(expected, result.error);
        assertNull(result.body);
    }
    private MockResponse json(int code, String body) {
        return new MockResponse().setResponseCode(code).setHeader("Content-Type", "application/json").setBody(body);
    }
    private static final class Result implements ChatRepository.ChatCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private ChatResponse body;
        private ChatRepository.ChatError error;
        @Override public void onSuccess(ChatResponse value) { body = value; done.countDown(); }
        @Override public void onError(ChatRepository.ChatError value) { error = value; done.countDown(); }
        private void await() throws Exception { assertTrue("callback", done.await(4, TimeUnit.SECONDS)); }
    }
}
