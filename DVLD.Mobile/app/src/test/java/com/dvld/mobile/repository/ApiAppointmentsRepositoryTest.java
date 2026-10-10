package com.dvld.mobile.repository;

import com.dvld.mobile.model.TestAppointment;
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

public class ApiAppointmentsRepositoryTest {
    private MockWebServer server;
    private OkHttpClient client;
    private AppointmentsRepository repository;
    private AppointmentsRepository.RequestHandle handle;

    @Before
    public void setUp() throws Exception {
        server = new MockWebServer();
        server.start();
        SessionCookieJar cookies = new SessionCookieJar();
        cookies.saveFromResponse(server.url("/"), Collections.singletonList(
                Cookie.parse(server.url("/"), "DVLD.MobileAuth=appointments-fixture; Path=/; HttpOnly")));
        client = new OkHttpClient.Builder().cookieJar(cookies).retryOnConnectionFailure(false)
                .callTimeout(2, TimeUnit.SECONDS).build();
        DashboardApiService service = new Retrofit.Builder().baseUrl(server.url("/"))
                .client(client).callbackExecutor(Runnable::run)
                .addConverterFactory(GsonConverterFactory.create()).build().create(DashboardApiService.class);
        repository = new ApiAppointmentsRepository(service);
    }

    @After
    public void tearDown() throws Exception {
        if (handle != null) handle.cancel();
        server.shutdown();
        client.dispatcher().executorService().shutdownNow();
        client.connectionPool().evictAll();
    }

    @Test
    public void fetchesMultipleAppointmentsWithCookieAndFullContract() throws Exception {
        server.enqueue(json(200, "[{\"testAppointmentID\":18,\"testTypeID\":2,\"testTypeTitle\":\"Written Test\",\"localDrivingLicenseApplicationID\":12,\"applicationID\":24,\"licenseClassID\":3,\"className\":\"Class 3 - Ordinary driving license\",\"appointmentDate\":\"2026-10-09T15:30:00\",\"paidFees\":25.5,\"isLocked\":true,\"retakeTestApplicationID\":30,\"testID\":9,\"testResult\":false,\"notes\":\"Fixture notes\"},{\"testAppointmentID\":19}]"));
        Result result = load();
        assertNull(result.error);
        assertEquals(2, result.appointments.size());
        TestAppointment a = result.appointments.get(0);
        assertEquals(Integer.valueOf(18), a.getTestAppointmentID());
        assertEquals(Integer.valueOf(2), a.getTestTypeID());
        assertEquals("Written Test", a.getTestTypeTitle());
        assertEquals(Integer.valueOf(12), a.getLocalDrivingLicenseApplicationID());
        assertEquals(Integer.valueOf(24), a.getApplicationID());
        assertEquals(Integer.valueOf(3), a.getLicenseClassID());
        assertEquals("Class 3 - Ordinary driving license", a.getClassName());
        assertEquals("2026-10-09T15:30:00", a.getAppointmentDate());
        assertEquals(Double.valueOf(25.5), a.getPaidFees());
        assertEquals(Boolean.TRUE, a.getIsLocked());
        assertEquals(Integer.valueOf(30), a.getRetakeTestApplicationID());
        assertEquals(Integer.valueOf(9), a.getTestID());
        assertEquals(Boolean.FALSE, a.getTestResult());
        assertEquals("Fixture notes", a.getNotes());
        RecordedRequest request = server.takeRequest(3, TimeUnit.SECONDS);
        assertNotNull(request);
        assertEquals("GET", request.getMethod());
        assertEquals("/api/people/42/test-appointments", request.getPath());
        assertEquals("DVLD.MobileAuth=appointments-fixture", request.getHeader("Cookie"));
        assertNull(request.getHeader("Authorization"));
        assertEquals(1, server.getRequestCount());
    }

    @Test
    public void emptyListIsSuccessfulEmptyData() throws Exception {
        server.enqueue(json(200, "[]"));
        Result result = load();
        assertNull(result.error);
        assertNotNull(result.appointments);
        assertTrue(result.appointments.isEmpty());
    }

    @Test
    public void nullEntriesAreSkippedAndNullableResultsStayNull() throws Exception {
        server.enqueue(json(200, "[null,{\"testAppointmentID\":18},null]"));
        Result result = load();
        assertNull(result.error);
        assertEquals(1, result.appointments.size());
        assertNull(result.appointments.get(0).getTestResult());
        assertNull(result.appointments.get(0).getTestID());
        assertNull(result.appointments.get(0).getRetakeTestApplicationID());
        assertNull(result.appointments.get(0).getNotes());
        assertNull(result.appointments.get(0).getAppointmentDate());
    }
    @Test
    public void maps401ToUnauthorized() throws Exception {
        assertError(401, "{}", AppointmentsRepository.AppointmentsError.UNAUTHORIZED);
    }

    @Test
    public void maps403ToForbidden() throws Exception {
        assertError(403, "{}", AppointmentsRepository.AppointmentsError.FORBIDDEN);
    }

    @Test
    public void maps404ToNotFound() throws Exception {
        assertError(404, "{}", AppointmentsRepository.AppointmentsError.NOT_FOUND);
    }

    @Test
    public void maps500AndOtherUnexpectedResponsesToServerError() throws Exception {
        for (int status : new int[] {500, 502, 400, 204}) {
            assertError(status, "", AppointmentsRepository.AppointmentsError.SERVER);
        }
    }

    @Test
    public void unreachableApiMapsToNetworkError() throws Exception {
        server.shutdown();
        Result result = load();
        assertNull(result.appointments);
        assertEquals(AppointmentsRepository.AppointmentsError.NETWORK, result.error);
    }

    @Test
    public void malformedOrMissingListsAreServerErrors() throws Exception {
        for (String body : new String[] {"not-json", "null", "", "{}", "[", "[{\"testAppointmentID\":\"bad\"}]"}) {
            assertError(200, body, AppointmentsRepository.AppointmentsError.SERVER);
        }
    }

    @Test
    public void duplicateConcurrentLoadsReuseOneRequest() throws Exception {
        server.enqueue(json(200, "[{\"testAppointmentID\":8}]").setHeadersDelay(400, TimeUnit.MILLISECONDS));
        Result first = new Result();
        Result duplicate = new Result();
        handle = repository.load(42, first);
        assertSame(handle, repository.load(42, duplicate));
        first.await();
        assertEquals(1, first.appointments.size());
        assertEquals(1, server.getRequestCount());
        assertEquals(1L, duplicate.done.getCount());
    }

    @Test
    public void cancellationSuppressesCallbacksAndAllowsRetry() throws Exception {
        server.enqueue(json(200, "[{\"testAppointmentID\":8}]").setHeadersDelay(500, TimeUnit.MILLISECONDS));
        Result cancelled = new Result();
        handle = repository.load(42, cancelled);
        assertNotNull(server.takeRequest(3, TimeUnit.SECONDS));
        handle.cancel();
        assertFalse(cancelled.done.await(650, TimeUnit.MILLISECONDS));
        server.enqueue(json(200, "[]"));
        assertTrue(load().appointments.isEmpty());
    }

    @Test
    public void retryAfterServerFailureFetchesAgain() throws Exception {
        assertError(500, "{}", AppointmentsRepository.AppointmentsError.SERVER);
        server.enqueue(json(200, "[]"));
        assertTrue(load().appointments.isEmpty());
        assertEquals(2, server.getRequestCount());
    }

    @Test
    public void invalidPersonIdNeverSendsARequest() throws Exception {
        Result result = new Result();
        handle = repository.load(0, result);
        result.await();
        assertEquals(AppointmentsRepository.AppointmentsError.NOT_FOUND, result.error);
        assertEquals(0, server.getRequestCount());
    }

    private Result load() throws InterruptedException {
        Result result = new Result();
        handle = repository.load(42, result);
        result.await();
        return result;
    }

    private void assertError(int status, String body, AppointmentsRepository.AppointmentsError expected)
            throws InterruptedException {
        server.enqueue(json(status, body));
        Result result = load();
        assertNull(result.appointments);
        assertEquals(expected, result.error);
    }

    private MockResponse json(int status, String body) {
        return new MockResponse().setResponseCode(status).addHeader("Content-Type", "application/json").setBody(body);
    }

    private static final class Result implements AppointmentsRepository.AppointmentsCallback {
        private final CountDownLatch done = new CountDownLatch(1);
        private List<TestAppointment> appointments;
        private AppointmentsRepository.AppointmentsError error;

        @Override
        public void onSuccess(List<TestAppointment> appointments) {
            this.appointments = appointments;
            done.countDown();
        }

        @Override
        public void onError(AppointmentsRepository.AppointmentsError error) {
            this.error = error;
            done.countDown();
        }

        private void await() throws InterruptedException {
            assertTrue("Appointments callback timed out", done.await(4, TimeUnit.SECONDS));
        }
    }
}
