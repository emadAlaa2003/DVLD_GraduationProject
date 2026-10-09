package com.dvld.mobile.repository;

import com.dvld.mobile.model.MobileLoginResponse;
import com.dvld.mobile.network.MobileAuthApiService;
import com.dvld.mobile.network.SessionCookieJar;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import java.util.UUID;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

import static org.junit.Assert.*;

public class ApiAuthRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private AuthRepository repository;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        client = new OkHttpClient.Builder()
                .cookieJar(new SessionCookieJar())
                .retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS)
                .build();
        MobileAuthApiService service = new Retrofit.Builder()
                .baseUrl(server.url("/"))
                .client(client)
                .callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create())
                .build().create(MobileAuthApiService.class);
        repository = new ApiAuthRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void postsExactContractAndReusesCookieOnSubsequentRequest() throws Exception {
        server.enqueue(json(200, "{\"personId\":42,\"fullName\":\"Sample Person\",\"username\":\"sample\"}")
                .addHeader("Set-Cookie", "DVLD.MobileAuth=session-fixture; Path=/; HttpOnly; SameSite=Strict"));
        Result result = new Result();
        String password = " \t" + UUID.randomUUID() + " \t";
        repository.login("  sample  ", password, result);
        result.await();
        assertNull(result.error);
        assertEquals(Integer.valueOf(42), result.response.getPersonId());
        assertEquals("Sample Person", result.response.getFullName());
        assertEquals("sample", result.response.getUsername());

        RecordedRequest login = server.takeRequest(3, TimeUnit.SECONDS);
        assertNotNull(login);
        assertEquals("POST", login.getMethod());
        assertEquals("/api/mobile-auth/login", login.getPath());
        JsonObject body = JsonParser.parseString(login.getBody().readUtf8()).getAsJsonObject();
        assertEquals(2, body.size());
        assertEquals("sample", body.get("username").getAsString());
        assertEquals(password, body.get("password").getAsString());
        assertNull(login.getHeader("Authorization"));

        server.enqueue(new MockResponse().setResponseCode(200));
        try (okhttp3.Response ignored = client.newCall(
                new Request.Builder().url(server.url("/api/session-check")).build()).execute()) {
            RecordedRequest next = server.takeRequest(3, TimeUnit.SECONDS);
            assertNotNull(next);
            assertEquals("DVLD.MobileAuth=session-fixture", next.getHeader("Cookie"));
            assertNull(next.getHeader("Authorization"));
        }
    }

    @Test
    public void mapsHttpErrorsWithoutExposingServerBody() throws Exception {
        assertError(401, "{\"message\":\"internal details\"}", AuthRepository.LoginError.INVALID_CREDENTIALS);
        assertError(403, "{}", AuthRepository.LoginError.INACTIVE_ACCOUNT);
        assertError(500, "{}", AuthRepository.LoginError.SERVER);
        assertError(400, "{}", AuthRepository.LoginError.SERVER);
    }

    @Test
    public void invalidOrMissingSuccessBodyDoesNotReportSuccess() throws Exception {
        assertError(200, "not-json", AuthRepository.LoginError.SERVER);
        assertError(200, "", AuthRepository.LoginError.SERVER);
        assertError(200, "{}", AuthRepository.LoginError.SERVER);
        assertError(204, "", AuthRepository.LoginError.SERVER);
    }

    @Test
    public void unreachableServerReportsNetworkFailure() throws Exception {
        server.shutdown();
        Result result = new Result();
        repository.login(UUID.randomUUID().toString(), UUID.randomUUID().toString(), result);
        result.await();
        assertNull(result.response);
        assertEquals(AuthRepository.LoginError.NETWORK, result.error);
    }

    @Test
    public void cancelledRequestDoesNotDeliverUiCallback() throws Exception {
        server.enqueue(json(200, "{}").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result result = new Result();
        AuthRepository.RequestHandle handle = repository.login(
                UUID.randomUUID().toString(), UUID.randomUUID().toString(), result);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(result.completed.await(800, TimeUnit.MILLISECONDS));
    }

    private void assertError(int status, String body, AuthRepository.LoginError expected) throws Exception {
        server.enqueue(json(status, body));
        Result result = new Result();
        repository.login(UUID.randomUUID().toString(), UUID.randomUUID().toString(), result);
        result.await();
        assertNull(result.response);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status)
                .addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements AuthRepository.LoginCallback {
        private final CountDownLatch completed = new CountDownLatch(1);
        private MobileLoginResponse response;
        private AuthRepository.LoginError error;

        @Override
        public void onSuccess(MobileLoginResponse response) {
            this.response = response;
            completed.countDown();
        }

        @Override
        public void onError(AuthRepository.LoginError error) {
            this.error = error;
            completed.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Login callback timed out", completed.await(4, TimeUnit.SECONDS));
        }
    }
}
