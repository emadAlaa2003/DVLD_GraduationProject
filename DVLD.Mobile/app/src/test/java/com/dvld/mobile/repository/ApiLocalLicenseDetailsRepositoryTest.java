package com.dvld.mobile.repository;

import com.dvld.mobile.model.LocalLicenseDetails;
import com.dvld.mobile.network.DashboardApiService;
import com.dvld.mobile.network.SessionCookieJar;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

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

public class ApiLocalLicenseDetailsRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private LocalLicenseDetailsRepository repository;
    private LocalLicenseDetailsRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=details-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create()).build().create(DashboardApiService.class);
        repository = new ApiLocalLicenseDetailsRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void fetchesAndParsesDetailsWithExistingCookieAndNoAuthorizationHeader() throws Exception {
        server.enqueue(json(200, "{\"licenseID\":24,\"applicationID\":8,\"personID\":42,"
                + "\"fullName\":\"Citizen Fixture\",\"nationalNo\":\"N-FIXTURE\",\"licenseClassID\":3,"
                + "\"className\":\"Class 3 - Ordinary driving license\",\"classDescription\":\"Description\","
                + "\"issueDate\":\"2026-10-09T09:00:00\",\"expirationDate\":\"2036-10-09T09:00:00\","
                + "\"notes\":\"Note\",\"paidFees\":15.0,\"isActive\":true,\"isExpired\":false,"
                + "\"issueReason\":1,\"issueReasonText\":\"First Time\",\"isDetained\":false}"));
        Result result = load();
        assertNull(result.error);
        LocalLicenseDetails details = result.details;
        assertEquals(Integer.valueOf(24), details.getLicenseID());
        assertEquals(Integer.valueOf(8), details.getApplicationID());
        assertEquals(Integer.valueOf(42), details.getPersonID());
        assertEquals("Citizen Fixture", details.getFullName());
        assertEquals("N-FIXTURE", details.getNationalNo());
        assertEquals(Integer.valueOf(3), details.getLicenseClassID());
        assertEquals("Class 3 - Ordinary driving license", details.getClassName());
        assertEquals("Description", details.getClassDescription());
        assertEquals("2026-10-09T09:00:00", details.getIssueDate());
        assertEquals("2036-10-09T09:00:00", details.getExpirationDate());
        assertEquals("Note", details.getNotes());
        assertEquals(Double.valueOf(15), details.getPaidFees());
        assertEquals(Boolean.TRUE, details.getIsActive());
        assertEquals(Boolean.FALSE, details.getIsExpired());
        assertEquals(Integer.valueOf(1), details.getIssueReason());
        assertEquals("First Time", details.getIssueReasonText());
        assertEquals(Boolean.FALSE, details.getIsDetained());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertNotNull(request);
        assertEquals("GET", request.getMethod());
        assertEquals("/api/licenses/24", request.getPath());
        assertEquals("DVLD.MobileAuth=details-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        assertEquals(1, server.getRequestCount());
    }

    @Test
    public void nullableOptionalDetailsRemainNullRatherThanInvented() throws Exception {
        server.enqueue(json(200, "{\"licenseID\":24,\"notes\":null,\"isActive\":null}"));
        Result result = load();
        assertNull(result.error);
        assertNull(result.details.getNotes());
        assertNull(result.details.getFullName());
        assertNull(result.details.getPaidFees());
        assertNull(result.details.getIsActive());
    }

    @Test
    public void maps404ExpiredSessionForbiddenAndServerFailure() throws Exception {
        assertError(404, "{}", LocalLicenseDetailsRepository.DetailsError.NOT_FOUND);
        assertError(401, "{}", LocalLicenseDetailsRepository.DetailsError.UNAUTHORIZED);
        assertError(403, "{}", LocalLicenseDetailsRepository.DetailsError.FORBIDDEN);
        assertError(500, "{}", LocalLicenseDetailsRepository.DetailsError.SERVER);
    }

    @Test
    public void unreachableApiMapsToNetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.details);
        assertEquals(LocalLicenseDetailsRepository.DetailsError.NETWORK, result.error);
    }

    @Test
    public void malformedMissingOrMismatchedDetailsAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}", "{\"licenseID\":25}"}) {
            assertError(200, body, LocalLicenseDetailsRepository.DetailsError.SERVER);
        }
    }

    @Test
    public void duplicateConcurrentLoadsReuseOneRequest() throws Exception {
        server.enqueue(json(200, "{\"licenseID\":24}").setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result first = new Result();
        Result duplicate = new Result();
        handle = repository.load(24, first);
        assertSame(handle, repository.load(24, duplicate));
        first.await();
        assertNotNull(first.details);
        assertEquals(1, server.getRequestCount());
        assertEquals(1L, duplicate.done.getCount());
    }

    @Test
    public void cancellationSuppressesCallbacksAndAllowsRetry() throws Exception {
        server.enqueue(json(200, "{\"licenseID\":24}").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(24, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, "{\"licenseID\":24}"));
        assertNotNull(load().details);
    }

    @Test
    public void retryAfterServerFailureFetchesDetailsAgain() throws Exception {
        assertError(500, "{}", LocalLicenseDetailsRepository.DetailsError.SERVER);
        server.enqueue(json(200, "{\"licenseID\":24}"));
        assertNotNull(load().details);
        assertEquals(2, server.getRequestCount());
    }

    @Test
    public void invalidLicenseIdNeverSendsARequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(LocalLicenseDetailsRepository.DetailsError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(24, result);
        result.await();
        return result;
    }

    private void assertError(int status, String body, LocalLicenseDetailsRepository.DetailsError expected)
            throws InterruptedException {
        server.enqueue(json(status, body));
        Result result = load();
        assertNull(result.details);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status).addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements LocalLicenseDetailsRepository.DetailsCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private LocalLicenseDetails details;
        private LocalLicenseDetailsRepository.DetailsError error;

        @Override
        public void onSuccess(LocalLicenseDetails details) {
            this.details = details;
            done.countDown();
        }

        @Override
        public void onError(LocalLicenseDetailsRepository.DetailsError error) {
            this.error = error;
            done.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Details callback timed out", done.await(4, TimeUnit.SECONDS));
        }
    }
}
