package com.dvld.mobile.repository;

import com.dvld.mobile.model.ApplicationDetails;
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

public class ApiApplicationDetailsRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private ApplicationDetailsRepository repository;
    private ApplicationDetailsRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=application-details-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create()).build().create(DashboardApiService.class);
        repository = new ApiApplicationDetailsRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void fetchesMatchingApplicationWithCookieAndParsesDetentionDetails() throws Exception {
        server.enqueue(json(200, "{\"applicationID\":24,\"applicantPersonID\":42,"
                + "\"applicationDate\":\"2026-10-08T09:00:00\",\"applicationTypeID\":5,"
                + "\"applicationTypeName\":\"ReleaseDetainedDrivingLicsense\",\"applicationStatus\":3,"
                + "\"statusText\":\"Completed\",\"lastStatusDate\":\"2026-10-09T09:00:00\","
                + "\"paidFees\":15.25,\"details\":{\"detainID\":8,\"licenseID\":7,"
                + "\"detainDate\":\"2026-10-01\",\"fineFees\":90.50,\"isReleased\":true,"
                + "\"releaseDate\":\"2026-10-09\",\"releaseApplicationID\":24}}"));
        Result result = load();
        assertNull(result.error);
        ApplicationDetails details = result.details;
        assertEquals(Integer.valueOf(24), details.getApplicationID());
        assertEquals(Integer.valueOf(42), details.getApplicantPersonID());
        assertEquals("2026-10-08T09:00:00", details.getApplicationDate());
        assertEquals(Integer.valueOf(5), details.getApplicationTypeID());
        assertEquals("ReleaseDetainedDrivingLicsense", details.getApplicationTypeName());
        assertEquals(Integer.valueOf(3), details.getApplicationStatus());
        assertEquals("Completed", details.getStatusText());
        assertEquals("2026-10-09T09:00:00", details.getLastStatusDate());
        assertEquals(Double.valueOf(15.25), details.getPaidFees());
        assertEquals(Integer.valueOf(8), details.getDetails().getDetainID());
        assertEquals(Integer.valueOf(7), details.getDetails().getLicenseID());
        assertEquals("2026-10-01", details.getDetails().getDetainDate());
        assertEquals(Double.valueOf(90.50), details.getDetails().getFineFees());
        assertEquals(Boolean.TRUE, details.getDetails().getIsReleased());
        assertEquals("2026-10-09", details.getDetails().getReleaseDate());
        assertEquals(Integer.valueOf(24), details.getDetails().getReleaseApplicationID());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertNotNull(request);
        assertEquals("GET", request.getMethod());
        assertEquals("/api/applications/24", request.getPath());
        assertEquals("DVLD.MobileAuth=application-details-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        assertEquals(1, server.getRequestCount());
    }

    @Test
    public void missingOrNullSpecificDetailsRemainSuccessful() throws Exception {
        for (String body : new String[] {"{\"applicationID\":24}", "{\"applicationID\":24,\"details\":null}"}) {
            server.enqueue(json(200, body));
            Result result = load();
            assertNull(result.error);
            assertNull(result.details.getDetails());
            assertNull(result.details.getApplicationDate());
            assertNull(result.details.getPaidFees());
        }
    }
    @Test
    public void maps404ToNotFound() throws Exception {
        assertError(404, "{}", ApplicationDetailsRepository.DetailsError.NOT_FOUND);
    }

    @Test
    public void maps401ToUnauthorized() throws Exception {
        assertError(401, "{}", ApplicationDetailsRepository.DetailsError.UNAUTHORIZED);
    }

    @Test
    public void maps403ToForbidden() throws Exception {
        assertError(403, "{}", ApplicationDetailsRepository.DetailsError.FORBIDDEN);
    }

    @Test
    public void maps500ToServerError() throws Exception {
        assertError(500, "{}", ApplicationDetailsRepository.DetailsError.SERVER);
    }

    @Test
    public void unreachableApiMapsToNetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.details);
        assertEquals(ApplicationDetailsRepository.DetailsError.NETWORK, result.error);
    }

    @Test
    public void wrongResponseIdIsAServerError() throws Exception {
        assertError(200, "{\"applicationID\":25}", ApplicationDetailsRepository.DetailsError.SERVER);
    }

    @Test
    public void malformedOrMissingDetailsAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}", "[]", "{\"applicationID\":24,\"details\":\"bad\"}"}) {
            assertError(200, body, ApplicationDetailsRepository.DetailsError.SERVER);
        }
    }

    @Test
    public void duplicateConcurrentLoadsReuseOneRequest() throws Exception {
        server.enqueue(json(200, "{\"applicationID\":24}").setHeadersDelay(400, TimeUnit.MILLISECONDS));
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
        server.enqueue(json(200, "{\"applicationID\":24}").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(24, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, "{\"applicationID\":24}"));
        assertNotNull(load().details);
    }

    @Test
    public void retryAfterServerFailureFetchesDetailsAgain() throws Exception {
        assertError(500, "{}", ApplicationDetailsRepository.DetailsError.SERVER);
        server.enqueue(json(200, "{\"applicationID\":24}"));
        assertNotNull(load().details);
        assertEquals(2, server.getRequestCount());
    }

    @Test
    public void invalidApplicationIdNeverSendsARequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(ApplicationDetailsRepository.DetailsError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(24, result);
        result.await();
        return result;
    }

    private void assertError(int status, String body, ApplicationDetailsRepository.DetailsError expected)
            throws InterruptedException {
        server.enqueue(json(status, body));
        Result result = load();
        assertNull(result.details);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status).addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements ApplicationDetailsRepository.DetailsCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private ApplicationDetails details;
        private ApplicationDetailsRepository.DetailsError error;

        @Override
        public void onSuccess(ApplicationDetails details) {
            this.details = details;
            done.countDown();
        }

        @Override
        public void onError(ApplicationDetailsRepository.DetailsError error) {
            this.error = error;
            done.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Details callback timed out", done.await(4, TimeUnit.SECONDS));
        }
    }
}
