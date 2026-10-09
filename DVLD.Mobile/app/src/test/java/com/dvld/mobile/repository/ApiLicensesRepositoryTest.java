package com.dvld.mobile.repository;

import com.dvld.mobile.model.LicensesData;
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

public class ApiLicensesRepositoryTest {
    private static final String ROOT = "/api/people/42/";
    private final Map<String, MockResponse> responses = new ConcurrentHashMap<>();
    private MockWebServer server;
    private OkHttpClient client;
    private LicensesRepository repository;
    private LicensesRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        responses.put(ROOT + "licenses", json(200, "[]"));
        responses.put(ROOT + "international-licenses", json(200, "[]"));
        server.setDispatcher(new Dispatcher() {
            @Override
            public MockResponse dispatch(RecordedRequest request) {
                MockResponse response = responses.get(request.getPath());
                return response == null ? json(404, "{}") : response.clone();
            }
        });
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=licenses-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create()).build().create(DashboardApiService.class);
        repository = new ApiLicensesRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void onlyTheTwoLicenseEndpointsAreRequestedWithTheSessionCookie() throws Exception {
        assertNotNull(load().data);
        Set<String> paths = new HashSet<>();
        for (int i = 0; i < 2; i++) {
            RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
            assertNotNull(request);
            assertEquals("GET", request.getMethod());
            assertEquals("DVLD.MobileAuth=licenses-fixture", request.getHeader("Cookie"));
            assertNull(request.getHeader("Authorization"));
            paths.add(request.getPath());
        }
        assertEquals(responses.keySet(), paths);
        assertEquals(2, server.getRequestCount());
    }

    @Test
    public void zeroLocalAndInternationalLicensesIsSuccessRatherThanError() throws Exception {
        Result result = load();
        assertNull(result.error);
        assertTrue(result.data.getLocalLicenses().isEmpty());
        assertTrue(result.data.getInternationalLicenses().isEmpty());
    }

    @Test
    public void oneLocalLicenseIsRetainedAndInternationalCanBeIndependentlyEmpty() throws Exception {
        responses.put(ROOT + "licenses", json(200, "[{\"licenseID\":7,\"className\":\"Class 3 - Ordinary driving license\"}]"));
        Result result = load();
        assertEquals(1, result.data.getLocalLicenses().size());
        assertEquals(Integer.valueOf(7), result.data.getLocalLicenses().get(0).getLicenseID());
        assertEquals("Class 3 - Ordinary driving license", result.data.getLocalLicenses().get(0).getClassName());
        assertTrue(result.data.getInternationalLicenses().isEmpty());
    }

    @Test
    public void multipleLocalAndInternationalLicensesRemainSeparateAndNoneAreDropped() throws Exception {
        responses.put(ROOT + "licenses", json(200, "[{\"licenseID\":1},{\"licenseID\":2,\"isActive\":false},{\"licenseID\":3}]"));
        responses.put(ROOT + "international-licenses", json(200,
                "[{\"internationalLicenseID\":9,\"issuedUsingLocalLicenseID\":1},{\"internationalLicenseID\":10}]"));
        Result result = load();
        assertEquals(3, result.data.getLocalLicenses().size());
        assertEquals(Integer.valueOf(3), result.data.getLocalLicenses().get(2).getLicenseID());
        assertEquals(Boolean.FALSE, result.data.getLocalLicenses().get(1).getIsActive());
        assertEquals(2, result.data.getInternationalLicenses().size());
        assertEquals(Integer.valueOf(9), result.data.getInternationalLicenses().get(0).getInternationalLicenseID());
        assertEquals(Integer.valueOf(1), result.data.getInternationalLicenses().get(0).getIssuedUsingLocalLicenseID());
        assertEquals(Integer.valueOf(10), result.data.getInternationalLicenses().get(1).getInternationalLicenseID());
    }

    @Test
    public void oneInternationalLicenseCanExistWithNoLocalLicenseInTheResponse() throws Exception {
        responses.put(ROOT + "international-licenses", json(200, "[{\"internationalLicenseID\":9}]"));
        Result result = load();
        assertTrue(result.data.getLocalLicenses().isEmpty());
        assertEquals(1, result.data.getInternationalLicenses().size());
    }

    @Test
    public void nullableFieldsAndNullListEntriesAreSafeWithoutInventedValues() throws Exception {
        responses.put(ROOT + "licenses", json(200, "[null,{}]"));
        responses.put(ROOT + "international-licenses", json(200, "[{},null]"));
        Result result = load();
        assertEquals(1, result.data.getLocalLicenses().size());
        assertNull(result.data.getLocalLicenses().get(0).getLicenseID());
        assertNull(result.data.getLocalLicenses().get(0).getClassName());
        assertNull(result.data.getLocalLicenses().get(0).getIsActive());
        assertEquals(1, result.data.getInternationalLicenses().size());
        assertNull(result.data.getInternationalLicenses().get(0).getIssueDate());
    }

    @Test
    public void bothEndpointsMap404401403And500Safely() throws Exception {
        for (String endpoint : new String[] {"licenses", "international-licenses"}) {
            assertError(endpoint, 404, "{}", LicensesRepository.LicensesError.NOT_FOUND);
            assertError(endpoint, 401, "{}", LicensesRepository.LicensesError.UNAUTHORIZED);
            assertError(endpoint, 403, "{}", LicensesRepository.LicensesError.FORBIDDEN);
            assertError(endpoint, 500, "{}", LicensesRepository.LicensesError.SERVER);
            responses.put(ROOT + endpoint, json(200, "[]"));
        }
    }

    @Test
    public void malformedNullAndMissingBodiesAreServerErrorsRatherThanEmptyLists() throws Exception {
        for (String body : new String[] {"not-json", "null", ""}) {
            assertError("licenses", 200, body, LicensesRepository.LicensesError.SERVER);
        }
    }

    @Test
    public void unreachableApiMapsToNetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.data);
        assertEquals(LicensesRepository.LicensesError.NETWORK, result.error);
    }

    @Test
    public void duplicateRefreshReusesOneBatch() throws Exception {
        responses.replaceAll((path, response) -> json(200, "[]").setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result first = new Result();
        Result duplicate = new Result();
        handle = repository.load(42, first);
        assertSame(handle, repository.load(42, duplicate));
        first.await();
        assertNotNull(first.data);
        assertEquals(2, server.getRequestCount());
        assertEquals(1L, duplicate.done.getCount());
    }

    @Test
    public void cancellationSuppressesCallbacksAndAllowsAnotherLoad() throws Exception {
        responses.replaceAll((path, response) -> json(200, "[]").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(42, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        responses.replaceAll((path, response) -> json(200, "[]"));
        assertNotNull(load().data);
    }

    @Test
    public void laterRefreshFetchesNewLicenseData() throws Exception {
        assertTrue(load().data.getLocalLicenses().isEmpty());
        responses.put(ROOT + "licenses", json(200, "[{\"licenseID\":20}]"));
        assertEquals(Integer.valueOf(20), load().data.getLocalLicenses().get(0).getLicenseID());
        assertEquals(4, server.getRequestCount());
    }

    @Test
    public void missingPersonIdNeverSendsARequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(LicensesRepository.LicensesError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(42, result);
        result.await();
        return result;
    }

    private void assertError(String endpoint, int status, String body, LicensesRepository.LicensesError expected)
            throws InterruptedException {
        responses.put(ROOT + endpoint, json(status, body));
        Result result = load();
        assertNull(result.data);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status).addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements LicensesRepository.LicensesCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private LicensesData data;
        private LicensesRepository.LicensesError error;

        @Override
        public void onSuccess(LicensesData data) {
            this.data = data;
            done.countDown();
        }

        @Override
        public void onError(LicensesRepository.LicensesError error) {
            this.error = error;
            done.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Licenses callback timed out", done.await(4, TimeUnit.SECONDS));
        }
    }
}
