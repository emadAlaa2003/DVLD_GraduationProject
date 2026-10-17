package com.dvld.mobile.network;

import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public final class ApiClient {
    // Local prototype endpoint via adb reverse. Release builds do not permit HTTP.
    private static final String BASE_URL = "http://127.0.0.1:5277/";

    private ApiClient() {
    }

    private static final class Holder {
        // Shared for the lifetime of the application process, across Activity instances.
        private static final OkHttpClient HTTP_CLIENT = new OkHttpClient.Builder()
                .cookieJar(new SessionCookieJar())
                .connectTimeout(10, TimeUnit.SECONDS)
                .readTimeout(15, TimeUnit.SECONDS)
                .writeTimeout(15, TimeUnit.SECONDS)
                .callTimeout(30, TimeUnit.SECONDS)
                .retryOnConnectionFailure(false)
                .build();

        private static final MobileAuthApiService AUTH_SERVICE = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(HTTP_CLIENT)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(MobileAuthApiService.class);

        private static final DashboardApiService DASHBOARD_SERVICE = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(getHttpClient())
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(DashboardApiService.class);

        private static final PracticeExamApiService PRACTICE_SERVICE = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(getHttpClient())
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(PracticeExamApiService.class);

        // Clone retains the exact session CookieJar; other API timeouts stay unchanged.
        private static final OkHttpClient CHAT_HTTP_CLIENT = HTTP_CLIENT.newBuilder()
                .readTimeout(5, TimeUnit.MINUTES)
                .writeTimeout(5, TimeUnit.MINUTES)
                .callTimeout(5, TimeUnit.MINUTES)
                .build();

        private static final ChatApiService CHAT_SERVICE = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(CHAT_HTTP_CLIENT)
                .addConverterFactory(GsonConverterFactory.create())
                .build()
                .create(ChatApiService.class);
    }

    public static MobileAuthApiService getAuthService() {
        return Holder.AUTH_SERVICE;
    }

    public static DashboardApiService getDashboardService() {
        return Holder.DASHBOARD_SERVICE;
    }

    public static PracticeExamApiService getPracticeExamService() {
        return Holder.PRACTICE_SERVICE;
    }

    public static ChatApiService getChatService() { return Holder.CHAT_SERVICE; }

    public static OkHttpClient getChatHttpClient() { return Holder.CHAT_HTTP_CLIENT; }

    /** Future API services must reuse this client to send the same session cookies. */
    public static OkHttpClient getHttpClient() {
        return Holder.HTTP_CLIENT;
    }
}
