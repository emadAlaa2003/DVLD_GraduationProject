package com.dvld.mobile.repository;

import com.dvld.mobile.model.InternationalLicenseDetails;
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

public class ApiInternationalLicenseDetailsRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private InternationalLicenseDetailsRepository repository;
    private InternationalLicenseDetailsRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=international-details-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create()).build().create(DashboardApiService.class);
        repository = new ApiInternationalLicenseDetailsRepository(service);
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
        server.enqueue(json(200, "{\"internationalLicenseID\":24,\"applicationID\":8,\"personID\":42,"
                + "\"fullName\":\"Citizen Fixture\",\"nationalNo\":\"N-FIXTURE\",\"driverID\":5,"
                + "\"issuedUsingLocalLicenseID\":7,\"issueDate\":\"2026-10-09T09:00:00\","
                + "\"expirationDate\":\"2027-10-09T09:00:00\",\"isActive\":true,\"isExpired\":false,"
                + "\"isCurrentlyValid\":true,\"applicationDate\":\"2026-10-08T09:00:00\","
                + "\"applicationStatus\":3,\"paidFees\":15.0}"));
        Result result = load();
        assertNull(result.error);
        InternationalLicenseDetails details = result.details;
        assertEquals(Integer.valueOf(24), details.getInternationalLicenseID());
        assertEquals(Integer.valueOf(8), details.getApplicationID());
        assertEquals(Integer.valueOf(42), details.getPersonID());
        assertEquals("Citizen Fixture", details.getFullName());
        assertEquals("N-FIXTURE", details.getNationalNo());
        assertEquals(Integer.valueOf(5), details.getDriverID());
        assertEquals(Integer.valueOf(7), details.getIssuedUsingLocalLicenseID());
        assertEquals("2026-10-09T09:00:00", details.getIssueDate());
        assertEquals("2027-10-09T09:00:00", details.getExpirationDate());
        assertEquals(Boolean.TRUE, details.getIsActive());
        assertEquals(Boolean.FALSE, details.getIsExpired());
        assertEquals(Boolean.TRUE, details.getIsCurrentlyValid());
        assertEquals("2026-10-08T09:00:00", details.getApplicationDate());
        assertEquals(Integer.valueOf(3), details.getApplicationStatus());
        assertEquals(Double.valueOf(15), details.getPaidFees());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertNotNull(request);
        assertEquals("GET", request.getMethod());
        assertEquals("/api/international-licenses/24", request.getPath());
        assertEquals("DVLD.MobileAuth=international-details-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        assertEquals(1, server.getRequestCount());
    }

    @Test
    public void nullableOptionalDetailsRemainNullRatherThanInvented() throws Exception {
        server.enqueue(json(200, "{\"internationalLicenseID\":24,\"applicationDate\":null,\"isActive\":null}"));
        Result result = load();
        assertNull(result.error);
        assertNull(result.details.getApplicationDate());
        assertNull(result.details.getFullName());
        assertNull(result.details.getPaidFees());
        assertNull(result.details.getIsActive());
    }

    @Test
    public void maps404ToNotFound() throws Exception {
        assertError(404, "{}", InternationalLicenseDetailsRepository.DetailsError.NOT_FOUND);
    }

    @Test
    public void maps401ToUnauthorized() throws Exception {
        assertError(401, "{}", InternationalLicenseDetailsRepository.DetailsError.UNAUTHORIZED);
    }

    @Test
    public void maps403ToForbidden() throws Exception {
        assertError(403, "{}", InternationalLicenseDetailsRepository.DetailsError.FORBIDDEN);
    }

    @Test
    public void maps500ToServerError() throws Exception {
        assertError(500, "{}", InternationalLicenseDetailsRepository.DetailsError.SERVER);
    }

    @Test
    public void unreachableApiMapsToNetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.details);
        assertEquals(InternationalLicenseDetailsRepository.DetailsError.NETWORK, result.error);
    }

    @Test
    public void wrongResponseIdIsAServerError() throws Exception {
        assertError(200, "{\"internationalLicenseID\":25}", InternationalLicenseDetailsRepository.DetailsError.SERVER);
    }

    @Test
    public void malformedOrMissingDetailsAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}"}) {
            assertError(200, body, InternationalLicenseDetailsRepository.DetailsError.SERVER);
        }
    }

    @Test
    public void duplicateConcurrentLoadsReuseOneRequest() throws Exception {
        server.enqueue(json(200, "{\"internationalLicenseID\":24}").setHeadersDelay(400, TimeUnit.MILLISECONDS));
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
        server.enqueue(json(200, "{\"internationalLicenseID\":24}").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(24, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, "{\"internationalLicenseID\":24}"));
        assertNotNull(load().details);
    }

    @Test
    public void retryAfterServerFailureFetchesDetailsAgain() throws Exception {
        assertError(500, "{}", InternationalLicenseDetailsRepository.DetailsError.SERVER);
        server.enqueue(json(200, "{\"internationalLicenseID\":24}"));
        assertNotNull(load().details);
        assertEquals(2, server.getRequestCount());
    }

    @Test
    public void invalidLicenseIdNeverSendsARequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(InternationalLicenseDetailsRepository.DetailsError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(24, result);
        result.await();
        return result;
    }

    private void assertError(int status, String body, InternationalLicenseDetailsRepository.DetailsError expected)
            throws InterruptedException {
        server.enqueue(json(status, body));
        Result result = load();
        assertNull(result.details);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status).addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements InternationalLicenseDetailsRepository.DetailsCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private InternationalLicenseDetails details;
        private InternationalLicenseDetailsRepository.DetailsError error;

        @Override
        public void onSuccess(InternationalLicenseDetails details) {
            this.details = details;
            done.countDown();
        }

        @Override
        public void onError(InternationalLicenseDetailsRepository.DetailsError error) {
            this.error = error;
            done.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Details callback timed out", done.await(4, TimeUnit.SECONDS));
        }
    }
}
