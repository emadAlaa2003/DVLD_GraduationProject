package com.dvld.mobile.repository;

import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.network.DashboardApiService;
import com.dvld.mobile.network.SessionCookieJar;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import java.util.Collections;
import java.util.List;
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

public class ApiApplicationsRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private ApplicationsRepository repository;
    private ApplicationsRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=applications-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create()).build().create(DashboardApiService.class);
        repository = new ApiApplicationsRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void fetchesAllApplicationsWithExistingCookieAndParsesContract() throws Exception {
        server.enqueue(json(200, "[{\"applicationID\":8,\"applicantPersonID\":42,"
                + "\"applicationDate\":\"2026-10-08T09:00:00\",\"applicationTypeID\":1,"
                + "\"applicationTypeName\":\"NewDrivingLicense\",\"applicationStatus\":3,"
                + "\"statusText\":\"Completed\",\"lastStatusDate\":\"2026-10-09T10:00:00\","
                + "\"paidFees\":15.25,\"localDrivingLicenseApplicationID\":12,\"licenseClassID\":3,"
                + "\"className\":\"Class 3 - Ordinary driving license\"},{\"applicationID\":9}]"));
        Result result = load();
        assertNull(result.error);
        assertEquals(2, result.applications.size());
        CitizenApplication application = result.applications.get(0);
        assertEquals(Integer.valueOf(8), application.getApplicationID());
        assertEquals(Integer.valueOf(42), application.getApplicantPersonID());
        assertEquals("2026-10-08T09:00:00", application.getApplicationDate());
        assertEquals(Integer.valueOf(1), application.getApplicationTypeID());
        assertEquals("NewDrivingLicense", application.getApplicationTypeName());
        assertEquals(Integer.valueOf(3), application.getApplicationStatus());
        assertEquals("Completed", application.getStatusText());
        assertEquals("2026-10-09T10:00:00", application.getLastStatusDate());
        assertEquals(Double.valueOf(15.25), application.getPaidFees());
        assertEquals(Integer.valueOf(12), application.getLocalDrivingLicenseApplicationID());
        assertEquals(Integer.valueOf(3), application.getLicenseClassID());
        assertEquals("Class 3 - Ordinary driving license", application.getClassName());
        assertEquals(Integer.valueOf(9), result.applications.get(1).getApplicationID());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertNotNull(request);
        assertEquals("GET", request.getMethod());
        assertEquals("/api/people/42/applications", request.getPath());
        assertEquals("DVLD.MobileAuth=applications-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        assertEquals(1, server.getRequestCount());
    }

    @Test
    public void emptyListIsSuccessfulEmptyData() throws Exception {
        server.enqueue(json(200, "[]"));
        Result result = load();
        assertNull(result.error);
        assertNotNull(result.applications);
        assertTrue(result.applications.isEmpty());
    }

    @Test
    public void nullEntriesAreSkippedAndNullableFieldsArePreserved() throws Exception {
        server.enqueue(json(200, "[null,{\"applicationID\":8,\"className\":null,\"paidFees\":null},null]"));
        Result result = load();
        assertNull(result.error);
        assertEquals(1, result.applications.size());
        assertNull(result.applications.get(0).getClassName());
        assertNull(result.applications.get(0).getLicenseClassID());
        assertNull(result.applications.get(0).getLocalDrivingLicenseApplicationID());
        assertNull(result.applications.get(0).getPaidFees());
        assertNull(result.applications.get(0).getApplicationDate());
    }

    @Test
    public void maps401ToUnauthorized() throws Exception {
        assertError(401, "{}", ApplicationsRepository.ApplicationsError.UNAUTHORIZED);
    }

    @Test
    public void maps403ToForbidden() throws Exception {
        assertError(403, "{}", ApplicationsRepository.ApplicationsError.FORBIDDEN);
    }

    @Test
    public void maps404ToNotFound() throws Exception {
        assertError(404, "{}", ApplicationsRepository.ApplicationsError.NOT_FOUND);
    }

    @Test
    public void maps500AndOtherUnexpectedResponsesToServerError() throws Exception {
        for (int status : new int[] {500, 502, 400, 204}) {
            assertError(status, "", ApplicationsRepository.ApplicationsError.SERVER);
        }
    }

    @Test
    public void unreachableApiMapsToNetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.applications);
        assertEquals(ApplicationsRepository.ApplicationsError.NETWORK, result.error);
    }

    @Test
    public void malformedOrMissingListsAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}", "[", "[{\"applicationID\":\"bad\"}]"}) {
            assertError(200, body, ApplicationsRepository.ApplicationsError.SERVER);
        }
    }

    @Test
    public void duplicateConcurrentLoadsReuseOneRequest() throws Exception {
        server.enqueue(json(200, "[{\"applicationID\":8}]").setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result first = new Result();
        Result duplicate = new Result();
        handle = repository.load(42, first);
        assertSame(handle, repository.load(42, duplicate));
        first.await();
        assertEquals(1, first.applications.size());
        assertEquals(1, server.getRequestCount());
        assertEquals(1L, duplicate.done.getCount());
    }

    @Test
    public void cancellationSuppressesCallbacksAndAllowsRetry() throws Exception {
        server.enqueue(json(200, "[{\"applicationID\":8}]").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(42, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, "[]"));
        assertTrue(load().applications.isEmpty());
    }

    @Test
    public void retryAfterServerFailureFetchesAgain() throws Exception {
        assertError(500, "{}", ApplicationsRepository.ApplicationsError.SERVER);
        server.enqueue(json(200, "[]"));
        assertTrue(load().applications.isEmpty());
        assertEquals(2, server.getRequestCount());
    }

    @Test
    public void invalidPersonIdNeverSendsARequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(ApplicationsRepository.ApplicationsError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(42, result);
        result.await();
        return result;
    }

    private void assertError(int status, String body, ApplicationsRepository.ApplicationsError expected)
            throws InterruptedException {
        server.enqueue(json(status, body));
        Result result = load();
        assertNull(result.applications);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status).addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements ApplicationsRepository.ApplicationsCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private List<CitizenApplication> applications;
        private ApplicationsRepository.ApplicationsError error;

        @Override
        public void onSuccess(List<CitizenApplication> applications) {
            this.applications = applications;
            done.countDown();
        }

        @Override
        public void onError(ApplicationsRepository.ApplicationsError error) {
            this.error = error;
            done.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Applications callback timed out", done.await(4, TimeUnit.SECONDS));
        }
    }
}
