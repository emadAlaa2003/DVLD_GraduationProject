package com.dvld.mobile.network;

import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.model.InternationalLicense;
import com.dvld.mobile.model.LocalLicense;
import com.dvld.mobile.model.TestAppointment;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.GET;
import retrofit2.http.Path;

public interface DashboardApiService {
    @GET("api/people/{personId}/licenses")
    Call<List<LocalLicense>> getLocalLicenses(@Path("personId") int personId);

    @GET("api/people/{personId}/international-licenses")
    Call<List<InternationalLicense>> getInternationalLicenses(@Path("personId") int personId);

    @GET("api/people/{personId}/applications")
    Call<List<CitizenApplication>> getApplications(@Path("personId") int personId);

    @GET("api/people/{personId}/test-appointments")
    Call<List<TestAppointment>> getTestAppointments(@Path("personId") int personId);
}
