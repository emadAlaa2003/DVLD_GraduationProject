package com.dvld.mobile.repository;

import com.dvld.mobile.PracticeExamFixtures;
import com.dvld.mobile.model.*;
import com.dvld.mobile.network.PracticeExamApiService;
import com.dvld.mobile.network.SessionCookieJar;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import java.util.Arrays;
import java.util.Collections;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;
import okhttp3.Cookie;
import okhttp3.OkHttpClient;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import static org.junit.Assert.*;

public class ApiPracticeExamRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private PracticeExamRepository repository;
    private PracticeExamRepository.RequestHandle handle;
    @Before public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=practice-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        PracticeExamApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run).addConverterFactory(GsonConverterFactory.create())
                .build().create(PracticeExamApiService.class);
        repository = new ApiPracticeExamRepository(service);
    }
    @After public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test public void start200SendsOnlyQuestionCountWithSharedCookieAndParsesTenQuestions() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.startJson(10, 10)));
        Result<PracticeExamStartResponse> result = start();
        assertNull(result.error);
        assertEquals(Integer.valueOf(10), result.body.getQuestionCount());
        assertEquals("سؤال 1", result.body.getQuestions().get(0).getQuestionText());
        assertEquals(4, result.body.getQuestions().get(0).getOptions().size());
        assertEquals(2, result.body.getQuestions().get(1).getOptions().size());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertEquals("POST", request.getMethod());
        assertEquals("/api/practice-exams/start", request.getPath());
        assertEquals("DVLD.MobileAuth=practice-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        JsonObject body = JsonParser.parseString(request.getBody().readUtf8()).getAsJsonObject();
        assertEquals(1, body.size());
        assertEquals(10, body.get("questionCount").getAsInt());
    }
    @Test public void start200FewerThanRequestedIsSuccessful() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.startJson(10, 2)));
        Result<PracticeExamStartResponse> result = start();
        assertNull(result.error);
        assertEquals(Integer.valueOf(10), result.body.getRequestedQuestionCount());
        assertEquals(Integer.valueOf(2), result.body.getQuestionCount());
    }
    @Test public void start400() throws Exception { startError(400, "{}", PracticeExamRepository.ExamError.BAD_REQUEST); }
    @Test public void start401() throws Exception { startError(401, "{}", PracticeExamRepository.ExamError.UNAUTHORIZED); }
    @Test public void start403() throws Exception { startError(403, "{}", PracticeExamRepository.ExamError.FORBIDDEN); }
    @Test public void start409() throws Exception { startError(409, "{}", PracticeExamRepository.ExamError.CONFLICT); }
    @Test public void start500AndUnexpectedCodes() throws Exception {
        for (int status : new int[] {500, 502, 404, 204}) startError(status, "", PracticeExamRepository.ExamError.SERVER);
    }
    @Test public void startNetworkFailure() throws Exception {
        server.shutdown();
        assertEquals(PracticeExamRepository.ExamError.NETWORK, start().error);
    }
    @Test public void startMalformedAndNullBodiesAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}", "[", "[]",
                "{\"questionCount\":\"bad\"}", PracticeExamFixtures.startJson(9, 2), PracticeExamFixtures.startJson(10, 0)})
            startError(200, body, PracticeExamRepository.ExamError.SERVER);
    }
    @Test public void startRejectsMissingOptionsNullQuestionsAndDuplicateIds() throws Exception {
        JsonObject missingOptions = JsonParser.parseString(PracticeExamFixtures.startJson(10, 2)).getAsJsonObject();
        missingOptions.getAsJsonArray("questions").get(0).getAsJsonObject().remove("optionD");
        startError(200, missingOptions.toString(), PracticeExamRepository.ExamError.SERVER);
        JsonObject duplicate = JsonParser.parseString(PracticeExamFixtures.startJson(10, 2)).getAsJsonObject();
        duplicate.getAsJsonArray("questions").get(1).getAsJsonObject().addProperty("questionID", 1051);
        startError(200, duplicate.toString(), PracticeExamRepository.ExamError.SERVER);
        missingOptions.add("questions", null);
        startError(200, missingOptions.toString(), PracticeExamRepository.ExamError.SERVER);
    }
    @Test public void startConcurrentCallsReuseTheOriginalRequest() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.startJson(10, 2)).setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result<PracticeExamStartResponse> first = new Result<>();
        Result<PracticeExamStartResponse> duplicate = new Result<>();
        handle = repository.startExam(10, first);
        assertSame(handle, repository.startExam(10, duplicate));
        first.await();
        assertNotNull(first.body);
        assertEquals(1, server.getRequestCount());
        assertEquals(1L, duplicate.done.getCount());
    }
    @Test public void startCancelSuppressesCallbackAndAllowsRetry() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.startJson(10, 2)).setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result<PracticeExamStartResponse> cancelled = new Result<>();
        handle = repository.startExam(10, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, PracticeExamFixtures.startJson(10, 2)));
        assertNotNull(start().body);
    }
    @Test public void invalidStartCountDoesNotCallApi() throws Exception {
        for (int count : new int[] {0, -1, 21}) {
            Result<PracticeExamStartResponse> result = new Result<>();
            handle = repository.startExam(count, result);
            result.await();
            assertEquals(PracticeExamRepository.ExamError.BAD_REQUEST, result.error);
        }
        assertEquals(0, server.getRequestCount());
    }

    @Test public void submit200ParsesBackendGradeAndSendsOnlyIdsAndSelectionsWithCookie() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.submitJson()));
        Result<PracticeExamSubmitResponse> result = submit();
        assertNull(result.error);
        assertEquals(Integer.valueOf(2), result.body.getQuestionCount());
        assertEquals(Integer.valueOf(2), result.body.getAnsweredCount());
        assertEquals(Integer.valueOf(1), result.body.getCorrectAnswers());
        assertEquals(Double.valueOf(50), result.body.getPercentage());
        assertEquals("C", result.body.getResults().get(0).getCorrectOption());
        assertEquals(Boolean.FALSE, result.body.getResults().get(0).getIsCorrect());
        assertEquals("شرح السؤال", result.body.getResults().get(0).getExplanation());
        assertNull(result.body.getResults().get(1).getExplanation());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertEquals("POST", request.getMethod());
        assertEquals("/api/practice-exams/submit", request.getPath());
        assertEquals("DVLD.MobileAuth=practice-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        JsonObject body = JsonParser.parseString(request.getBody().readUtf8()).getAsJsonObject();
        assertEquals(1, body.size());
        assertEquals(2, body.getAsJsonArray("answers").size());
        JsonObject answer = body.getAsJsonArray("answers").get(0).getAsJsonObject();
        assertEquals(2, answer.size());
        assertEquals(1051, answer.get("questionID").getAsInt());
        assertEquals("A", answer.get("selectedOption").getAsString());
        assertFalse(answer.has("correctOption"));
    }
    @Test public void submit400() throws Exception { submitError(400, "{}", PracticeExamRepository.ExamError.BAD_REQUEST); }
    @Test public void submit401() throws Exception { submitError(401, "{}", PracticeExamRepository.ExamError.UNAUTHORIZED); }
    @Test public void submit403() throws Exception { submitError(403, "{}", PracticeExamRepository.ExamError.FORBIDDEN); }
    @Test public void submit409() throws Exception { submitError(409, "{}", PracticeExamRepository.ExamError.CONFLICT); }
    @Test public void submit500AndUnexpectedCodes() throws Exception {
        for (int status : new int[] {500, 502, 404, 204}) submitError(status, "", PracticeExamRepository.ExamError.SERVER);
    }
    @Test public void submitNetworkFailure() throws Exception {
        server.shutdown();
        assertEquals(PracticeExamRepository.ExamError.NETWORK, submit().error);
    }
    @Test public void submitMalformedAndNullBodiesAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}", "[", "[]", "{\"percentage\":\"bad\"}"})
            submitError(200, body, PracticeExamRepository.ExamError.SERVER);
    }
    @Test public void submitRejectsIncompleteResultsUnknownIdsAndMissingSelections() throws Exception {
        for (String field : new String[] {"results", "questionCount", "answeredCount", "correctAnswers", "percentage"}) {
            JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
            body.remove(field);
            submitError(200, body.toString(), PracticeExamRepository.ExamError.SERVER);
        }
        for (String field : new String[] {"selectedOption", "correctOption", "isCorrect"}) {
            JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
            body.getAsJsonArray("results").get(0).getAsJsonObject().remove(field);
            submitError(200, body.toString(), PracticeExamRepository.ExamError.SERVER);
        }
        JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
        body.getAsJsonArray("results").get(0).getAsJsonObject().addProperty("questionID", 99);
        submitError(200, body.toString(), PracticeExamRepository.ExamError.SERVER);
        body.getAsJsonArray("results").get(0).getAsJsonObject().addProperty("questionID", 1052);
        submitError(200, body.toString(), PracticeExamRepository.ExamError.SERVER);
    }
    @Test public void submitRejectsInvalidPercentageAndChangedServerSelection() throws Exception {
        for (double value : new double[] {-1, 101, Double.NaN}) {
            JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
            body.addProperty("percentage", value);
            submitError(200, body.toString(), PracticeExamRepository.ExamError.SERVER);
        }
        JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
        body.getAsJsonArray("results").get(0).getAsJsonObject().addProperty("selectedOption", "B");
        submitError(200, body.toString(), PracticeExamRepository.ExamError.SERVER);
    }
    @Test public void submitConcurrentCallsReuseTheOriginalRequest() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.submitJson()).setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result<PracticeExamSubmitResponse> first = new Result<>();
        Result<PracticeExamSubmitResponse> duplicate = new Result<>();
        handle = repository.submitExam(PracticeExamFixtures.answers(), first);
        assertSame(handle, repository.submitExam(PracticeExamFixtures.answers(), duplicate));
        first.await();
        assertNotNull(first.body);
        assertEquals(1, server.getRequestCount());
        assertEquals(1L, duplicate.done.getCount());
    }
    @Test public void submitCancelSuppressesCallbackAndAllowsRetry() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.submitJson()).setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result<PracticeExamSubmitResponse> cancelled = new Result<>();
        handle = repository.submitExam(PracticeExamFixtures.answers(), cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, PracticeExamFixtures.submitJson()));
        assertNotNull(submit().body);
    }
    @Test public void operationsCannotOverlapAcrossStartAndSubmit() throws Exception {
        server.enqueue(json(200, PracticeExamFixtures.startJson(10, 2)).setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result<PracticeExamStartResponse> start = new Result<>();
        Result<PracticeExamSubmitResponse> duplicateSubmit = new Result<>();
        handle = repository.startExam(10, start);
        assertSame(handle, repository.submitExam(PracticeExamFixtures.answers(), duplicateSubmit));
        start.await();
        assertEquals(1L, duplicateSubmit.done.getCount());
        server.enqueue(json(200, PracticeExamFixtures.submitJson()).setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result<PracticeExamSubmitResponse> submit = new Result<>();
        Result<PracticeExamStartResponse> duplicateStart = new Result<>();
        handle = repository.submitExam(PracticeExamFixtures.answers(), submit);
        assertSame(handle, repository.startExam(10, duplicateStart));
        submit.await();
        assertEquals(1L, duplicateStart.done.getCount());
        assertEquals(2, server.getRequestCount());
    }
    @Test public void invalidSubmitInputNeverSendsPartialOrDuplicateAnswers() throws Exception {
        for (java.util.List<PracticeExamAnswer> answers : Arrays.asList(null, Collections.<PracticeExamAnswer>emptyList(),
                Arrays.asList(new PracticeExamAnswer(0, "A")), Arrays.asList(new PracticeExamAnswer(1051, "E")),
                Arrays.asList(new PracticeExamAnswer(1051, "A"), new PracticeExamAnswer(1051, "B")))) {
            Result<PracticeExamSubmitResponse> result = new Result<>();
            handle = repository.submitExam(answers, result);
            result.await();
            assertEquals(PracticeExamRepository.ExamError.BAD_REQUEST, result.error);
        }
        assertEquals(0, server.getRequestCount());
    }
    @Test public void operationsCanRetryAfterServerFailure() throws Exception {
        startError(500, "{}", PracticeExamRepository.ExamError.SERVER);
        server.enqueue(json(200, PracticeExamFixtures.startJson(10, 2)));
        assertNotNull(start().body);
        submitError(500, "{}", PracticeExamRepository.ExamError.SERVER);
        server.enqueue(json(200, PracticeExamFixtures.submitJson()));
        assertNotNull(submit().body);
        assertEquals(4, server.getRequestCount());
    }
    private Result<PracticeExamStartResponse> start() throws InterruptedException {
        Result<PracticeExamStartResponse> result = new Result<>();
        handle = repository.startExam(10, result);
        result.await();
        return result;
    }
    private Result<PracticeExamSubmitResponse> submit() throws InterruptedException {
        Result<PracticeExamSubmitResponse> result = new Result<>();
        handle = repository.submitExam(PracticeExamFixtures.answers(), result);
        result.await();
        return result;
    }
    private void startError(int code, String body, PracticeExamRepository.ExamError expected) throws InterruptedException {
        server.enqueue(json(code, body));
        Result<PracticeExamStartResponse> result = start();
        assertEquals(expected, result.error);
        assertNull(result.body);
    }
    private void submitError(int code, String body, PracticeExamRepository.ExamError expected) throws InterruptedException {
        server.enqueue(json(code, body));
        Result<PracticeExamSubmitResponse> result = submit();
        assertEquals(expected, result.error);
        assertNull(result.body);
    }
    private MockResponse json(int code, String body) {
        return new MockResponse().setResponseCode(code).addHeader("Content-Type", "application/json").setBody(body);
    }
    private static final class Result<T> implements PracticeExamRepository.ExamCallback<T> {
        private final CountDownLatch done = new CountDownLatch(1);
        private T body;
        private PracticeExamRepository.ExamError error;
        @Override public void onSuccess(T body) { this.body = body; done.countDown(); }
        @Override public void onError(PracticeExamRepository.ExamError error) { this.error = error; done.countDown(); }
        private void await() throws InterruptedException { assertTrue("Callback timed out", done.await(4, TimeUnit.SECONDS)); }
    }
}
