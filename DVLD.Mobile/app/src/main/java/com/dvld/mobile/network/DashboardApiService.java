package com.dvld.mobile.network;

import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.model.ApplicationDetails;
import com.dvld.mobile.model.InternationalLicense;
import com.dvld.mobile.model.InternationalLicenseDetails;
import com.dvld.mobile.model.LocalLicense;
import com.dvld.mobile.model.LocalLicenseDetails;
import com.dvld.mobile.model.TestAppointment;
import com.dvld.mobile.model.TestAppointmentDetails;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Path;

public interface DashboardApiService {
    @GET("api/test-appointments/{testAppointmentId}")
    Call<TestAppointmentDetails> getTestAppointmentDetails(@Path("testAppointmentId") int testAppointmentId);

    @GET("api/applications/{applicationId}")
    Call<ApplicationDetails> getApplicationDetails(@Path("applicationId") int applicationId);

    @GET("api/international-licenses/{internationalLicenseId}")
    Call<InternationalLicenseDetails> getInternationalLicenseDetails(@Path("internationalLicenseId") int internationalLicenseId);

    @GET("api/licenses/{licenseId}")
    Call<LocalLicenseDetails> getLocalLicenseDetails(@Path("licenseId") int licenseId);

    @GET("api/people/{personId}/licenses")
    Call<List<LocalLicense>> getLocalLicenses(@Path("personId") int personId);

    @GET("api/people/{personId}/international-licenses")
    Call<List<InternationalLicense>> getInternationalLicenses(@Path("personId") int personId);

    @GET("api/people/{personId}/applications")
    Call<List<CitizenApplication>> getApplications(@Path("personId") int personId);

    @GET("api/people/{personId}/test-appointments")
    Call<List<TestAppointment>> getTestAppointments(@Path("personId") int personId);
}
