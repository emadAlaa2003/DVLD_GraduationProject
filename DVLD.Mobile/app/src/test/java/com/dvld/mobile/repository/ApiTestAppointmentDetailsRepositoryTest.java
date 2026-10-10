package com.dvld.mobile.repository;

import com.dvld.mobile.model.TestAppointmentDetails;
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

public class ApiTestAppointmentDetailsRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private TestAppointmentDetailsRepository repository;
    private TestAppointmentDetailsRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=appointment-details-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create()).build().create(DashboardApiService.class);
        repository = new ApiTestAppointmentDetailsRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void fetchesMatchingAppointmentWithCookieAndNestedResult() throws Exception {
        server.enqueue(json(200, "{\"testAppointmentID\":24,\"testTypeID\":3,\"testTypeName\":\"StreetTest\",\"localDrivingLicenseApplicationID\":12,\"appointmentDate\":\"2026-10-09T15:30:00\",\"paidFees\":25.5,\"isLocked\":true,\"retakeTestApplicationID\":30,\"test\":{\"testID\":9,\"testResult\":true,\"resultText\":\"Passed\",\"notes\":\"Fixture notes\"}}"));
        Result result = load();
        assertNull(result.error);
        TestAppointmentDetails d = result.details;
        assertEquals(Integer.valueOf(24), d.getTestAppointmentID());
        assertEquals(Integer.valueOf(3), d.getTestTypeID());
        assertEquals("StreetTest", d.getTestTypeName());
        assertEquals(Integer.valueOf(12), d.getLocalDrivingLicenseApplicationID());
        assertEquals("2026-10-09T15:30:00", d.getAppointmentDate());
        assertEquals(Double.valueOf(25.5), d.getPaidFees());
        assertEquals(Boolean.TRUE, d.getIsLocked());
        assertEquals(Integer.valueOf(30), d.getRetakeTestApplicationID());
        assertEquals(Integer.valueOf(9), d.getTest().getTestID());
        assertEquals(Boolean.TRUE, d.getTest().getTestResult());
        assertEquals("Passed", d.getTest().getResultText());
        assertEquals("Fixture notes", d.getTest().getNotes());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertNotNull(request);
        assertEquals("GET", request.getMethod());
        assertEquals("/api/test-appointments/24", request.getPath());
        assertEquals("DVLD.MobileAuth=appointment-details-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
    }

    @Test
    public void missingOrNullTestRemainsSuccessful() throws Exception {
        for (String body : new String[] {"{\"testAppointmentID\":24}", "{\"testAppointmentID\":24,\"test\":null}"}) {
            server.enqueue(json(200, body));
            Result result = load();
            assertNull(result.error);
            assertNull(result.details.getTest());
            assertNull(result.details.getAppointmentDate());
        }
    }
    @Test
    public void maps404ToNotFound() throws Exception {
        assertError(404, "{}", TestAppointmentDetailsRepository.DetailsError.NOT_FOUND);
    }

    @Test
    public void maps401ToUnauthorized() throws Exception {
        assertError(401, "{}", TestAppointmentDetailsRepository.DetailsError.UNAUTHORIZED);
    }

    @Test
    public void maps403ToForbidden() throws Exception {
        assertError(403, "{}", TestAppointmentDetailsRepository.DetailsError.FORBIDDEN);
    }

    @Test
    public void maps500ToServerError() throws Exception {
        assertError(500, "{}", TestAppointmentDetailsRepository.DetailsError.SERVER);
    }

    @Test
    public void unreachableApiMapsToNetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.details);
        assertEquals(TestAppointmentDetailsRepository.DetailsError.NETWORK, result.error);
    }

    @Test
    public void wrongResponseIdIsAServerError() throws Exception {
        assertError(200, "{\"testAppointmentID\":25}", TestAppointmentDetailsRepository.DetailsError.SERVER);
    }

    @Test
    public void malformedOrMissingDetailsAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}", "[]", "{\"testAppointmentID\":24,\"test\":\"bad\"}"}) {
            assertError(200, body, TestAppointmentDetailsRepository.DetailsError.SERVER);
        }
    }

    @Test
    public void duplicateConcurrentLoadsReuseOneRequest() throws Exception {
        server.enqueue(json(200, "{\"testAppointmentID\":24}").setHeadersDelay(400, TimeUnit.MILLISECONDS));
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
        server.enqueue(json(200, "{\"testAppointmentID\":24}").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(24, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, "{\"testAppointmentID\":24}"));
        assertNotNull(load().details);
    }

    @Test
    public void retryAfterServerFailureFetchesDetailsAgain() throws Exception {
        assertError(500, "{}", TestAppointmentDetailsRepository.DetailsError.SERVER);
        server.enqueue(json(200, "{\"testAppointmentID\":24}"));
        assertNotNull(load().details);
        assertEquals(2, server.getRequestCount());
    }

    @Test
    public void invalidTestAppointmentIdNeverSendsARequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(TestAppointmentDetailsRepository.DetailsError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(24, result);
        result.await();
        return result;
    }

    private void assertError(int status, String body, TestAppointmentDetailsRepository.DetailsError expected)
            throws InterruptedException {
        server.enqueue(json(status, body));
        Result result = load();
        assertNull(result.details);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status).addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements TestAppointmentDetailsRepository.DetailsCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private TestAppointmentDetails details;
        private TestAppointmentDetailsRepository.DetailsError error;

        @Override
        public void onSuccess(TestAppointmentDetails details) {
            this.details = details;
            done.countDown();
        }

        @Override
        public void onError(TestAppointmentDetailsRepository.DetailsError error) {
            this.error = error;
            done.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Details callback timed out", done.await(4, TimeUnit.SECONDS));
        }
    }
}
