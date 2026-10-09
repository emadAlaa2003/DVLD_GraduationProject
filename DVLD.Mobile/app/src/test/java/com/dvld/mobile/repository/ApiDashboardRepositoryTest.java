package com.dvld.mobile.repository;

import com.dvld.mobile.model.DashboardData;
import com.dvld.mobile.network.DashboardApiService;
import com.dvld.mobile.network.SessionCookieJar;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import java.util.Collections;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

import okhttp3.Cookie;
import okhttp3.OkHttpClient;
import okhttp3.mockwebserver.Dispatcher;
import okhttp3.mockwebserver.MockResponse;
import okhttp3.mockwebserver.MockWebServer;
import okhttp3.mockwebserver.RecordedRequest;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

import static org.junit.Assert.*;

public class ApiDashboardRepositoryTest {
    private static final String ROOT = "/api/people/42/";
    private final Map<String, MockResponse> responses = new ConcurrentHashMap<>();
    private MockWebServer server;
    private OkHttpClient client;
    private DashboardRepository repository;
    private DashboardRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        for (String path : new String[] {"licenses", "international-licenses", "applications", "test-appointments"}) {
            responses.put(ROOT + path, json(200, "[]"));
        }
        server.setDispatcher(new Dispatcher() {
            @Override
            public MockResponse dispatch(RecordedRequest request) {
                MockResponse response = responses.get(request.getPath());
                return response != null ? response.clone() : json(404, "{}");
            }
        });
        server.start();
        SessionCookieJar cookieJar = new SessionCookieJar();
        cookieJar.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=dashboard-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookieJar)
                .retryOnConnectionFailure(false).callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create())
                .build().create(DashboardApiService.class);
        repository = new ApiDashboardRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void requestsAllFourEndpointsWithTheExistingSessionCookie() throws Exception {
        responses.put(ROOT + "licenses", json(200, "[{\"licenseID\":1,\"className\":null}]"));
        responses.put(ROOT + "international-licenses", json(200, "[{\"internationalLicenseID\":2}]"));
        Result result = load();
        assertNull(result.error);
        assertEquals(2, result.data.getRegisteredLicenseCount());
        Set<String> paths = new HashSet<>();
        for (int i = 0; i < 4; i++) {
            RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
            assertNotNull(request);
            assertEquals("GET", request.getMethod());
            assertEquals("DVLD.MobileAuth=dashboard-fixture", request.getHeader("Cookie"));
            assertNull(request.getHeader("Authorization"));
            paths.add(request.getPath());
        }
        assertEquals(responses.keySet(), paths);
    }

    @Test
    public void emptyListsAreSuccessfulAndLaterRefreshSeesANewlyIssuedLicense() throws Exception {
        Result initial = load();
        assertNull(initial.error);
        assertEquals(0, initial.data.getRegisteredLicenseCount());
        assertEquals(0, initial.data.getActiveApplicationCount());
        responses.put(ROOT + "licenses", json(200, "[{\"licenseID\":20}]"));
        Result refreshed = load();
        assertNull(refreshed.error);
        assertEquals(1, refreshed.data.getRegisteredLicenseCount());
        assertEquals(Integer.valueOf(20), refreshed.data.getLocalLicenses().get(0).getLicenseID());
        assertEquals(8, server.getRequestCount());
    }

    @Test
    public void maps404SeparatelyFromEmptyDataAndMapsOtherHttpFailures() throws Exception {
        assertError(404, "{}", DashboardRepository.DashboardError.NOT_FOUND);
        assertError(500, "{}", DashboardRepository.DashboardError.SERVER);
        assertError(401, "{}", DashboardRepository.DashboardError.UNAUTHORIZED);
        assertError(403, "{}", DashboardRepository.DashboardError.FORBIDDEN);
    }

    @Test
    public void malformedAndNullBodiesAreServerErrorsRatherThanEmptyData() throws Exception {
        assertError(200, "not-json", DashboardRepository.DashboardError.SERVER);
        assertError(200, "null", DashboardRepository.DashboardError.SERVER);
        assertError(200, "", DashboardRepository.DashboardError.SERVER);
    }

    @Test
    public void unreachableApiIsANetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.data);
        assertEquals(DashboardRepository.DashboardError.NETWORK, result.error);
    }

    @Test
    public void duplicateConcurrentRefreshReusesOneBatch() throws Exception {
        responses.replaceAll((path, response) -> json(200, "[]").setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result first = new Result();
        Result duplicate = new Result();
        handle = repository.load(42, first);
        assertSame(handle, repository.load(42, duplicate));
        first.await();
        assertNotNull(first.data);
        assertEquals(4, server.getRequestCount());
        assertEquals(1L, duplicate.completed.getCount());
    }

    @Test
    public void cancellationSuppressesCallbackAndAllowsTheNextRefresh() throws Exception {
        responses.replaceAll((path, response) -> json(200, "[]").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(42, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.completed.await(650, TimeUnit.MILLISECONDS));
        responses.replaceAll((path, response) -> json(200, "[]"));
        assertNotNull(load().data);
    }

    @Test
    public void invalidPersonIdNeverSendsAnApiRequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(DashboardRepository.DashboardError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(42, result);
        result.await();
        return result;
    }

    private void assertError(int status, String body, DashboardRepository.DashboardError expected)
            throws InterruptedException {
        responses.put(ROOT + "licenses", json(status, body));
        Result result = load();
        assertNull(result.data);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status)
                .addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements DashboardRepository.DashboardCallback {
        private final CountDownLatch completed = new CountDownLatch(1);
        private DashboardData data;
        private DashboardRepository.DashboardError error;

        @Override
        public void onSuccess(DashboardData data) {
            this.data = data;
            completed.countDown();
        }

        @Override
        public void onError(DashboardRepository.DashboardError error) {
            this.error = error;
            completed.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Dashboard callback timed out", completed.await(4, TimeUnit.SECONDS));
        }
    }
}
