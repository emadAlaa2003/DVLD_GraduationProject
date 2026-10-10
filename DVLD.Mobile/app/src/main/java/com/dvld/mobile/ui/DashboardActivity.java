package com.dvld.mobile.ui;

import android.content.Intent;
import android.content.res.ColorStateList;
import android.os.Bundle;
import android.util.TypedValue;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ApiDateTime;
import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.model.DashboardData;
import com.dvld.mobile.model.TestAppointment;
import com.dvld.mobile.repository.ApiDashboardRepository;
import com.dvld.mobile.repository.DashboardRepository;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.button.MaterialButton;

import java.time.Instant;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.Locale;

public class DashboardActivity extends AppCompatActivity {
    public static final String EXTRA_PERSON_ID = "personId";
    public static final String EXTRA_FULL_NAME = "fullName";
    public static final String EXTRA_USERNAME = "username";

    private Integer personId;
    private String fullName;
    private String username;
    private DashboardRepository repository;
    private DashboardTextMapper textMapper;
    private DashboardRepository.RequestHandle refreshRequest;
    private boolean loading;
    private boolean resumed;
    private long requestGeneration;
    private boolean openingDestination;
    private TextView licenseMessage;
    private TextView localSummary;
    private TextView internationalSummary;
    private TextView registeredValue;
    private TextView activeValue;
    private TextView upcomingValue;
    private TextView applicationMessage;
    private TextView statusMessage;
    private ProgressBar progress;
    private MaterialButton refreshButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_dashboard);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.dashboard_root), (view, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        personId = getIntent().hasExtra(EXTRA_PERSON_ID)
                ? getIntent().getIntExtra(EXTRA_PERSON_ID, -1) : null;
        fullName = getIntent().getStringExtra(EXTRA_FULL_NAME);
        username = getIntent().getStringExtra(EXTRA_USERNAME);

        String displayName = fullName;
        if (displayName == null || displayName.trim().isEmpty()) {
            displayName = username;
        }
        TextView nameView = findViewById(R.id.dashboard_citizen_name);
        if (displayName == null || displayName.trim().isEmpty()) {
            nameView.setVisibility(View.GONE);
        } else {
            nameView.setText(displayName.trim());
        }

        BottomNavigationView navigation = findViewById(R.id.dashboard_bottom_navigation);
        navigation.setItemActiveIndicatorColor(ColorStateList.valueOf(getColor(R.color.login_teal_soft)));
        navigation.setSelectedItemId(R.id.dashboard_nav_home);
        navigation.setOnItemSelectedListener(item -> {
            if (item.getItemId() == R.id.dashboard_nav_licenses) {
                openMyLicenses();
                return false;
            }
            if (item.getItemId() == R.id.dashboard_nav_applications) {
                openApplications();
                return false;
            }
            return item.getItemId() == R.id.dashboard_nav_home;
        });
        findViewById(R.id.dashboard_action_licenses).setOnClickListener(view -> openMyLicenses());
        findViewById(R.id.dashboard_action_applications).setOnClickListener(view -> openApplications());

        licenseMessage = findViewById(R.id.license_summary_message);
        localSummary = findViewById(R.id.local_license_summary);
        internationalSummary = findViewById(R.id.international_license_summary);
        registeredValue = findViewById(R.id.registered_licenses_value);
        activeValue = findViewById(R.id.active_applications_value);
        upcomingValue = findViewById(R.id.upcoming_appointment_value);
        applicationMessage = findViewById(R.id.latest_application_message);
        statusMessage = findViewById(R.id.dashboard_status_message);
        progress = findViewById(R.id.dashboard_progress);
        refreshButton = findViewById(R.id.dashboard_refresh);
        repository = new ApiDashboardRepository();
        textMapper = new DashboardTextMapper(this::getString);
        refreshButton.setOnClickListener(view -> refreshDashboard());
    }

    @Override
    protected void onResume() {
        super.onResume();
        openingDestination = false;
        resumed = true;
        // One entry point handles initial loading, returning from Desktop changes and retry.
        refreshDashboard();
    }

    private void openMyLicenses() {
        if (openingDestination || personId == null || personId <= 0) return;
        openingDestination = true;
        Intent intent = new Intent(this, MyLicensesActivity.class);
        intent.putExtra(MyLicensesActivity.EXTRA_PERSON_ID, personId.intValue());
        startActivity(intent);
    }

    private void openApplications() {
        if (openingDestination || personId == null || personId <= 0) return;
        openingDestination = true;
        Intent intent = new Intent(this, ApplicationsActivity.class);
        intent.putExtra(ApplicationsActivity.EXTRA_PERSON_ID, personId.intValue());
        startActivity(intent);
    }

    @Override
    protected void onStop() {
        resumed = false;
        requestGeneration++;
        if (refreshRequest != null) {
            refreshRequest.cancel();
            refreshRequest = null;
        }
        loading = false;
        super.onStop();
    }

    private void refreshDashboard() {
        if (!resumed || loading) {
            return;
        }
        if (personId == null || personId <= 0) {
            showError(DashboardRepository.DashboardError.NOT_FOUND);
            refreshButton.setEnabled(false);
            return;
        }
        loading = true;
        long generation = ++requestGeneration;
        showLoading();
        refreshRequest = repository.load(personId, new DashboardRepository.DashboardCallback() {
            @Override
            public void onSuccess(DashboardData data) {
                runOnUiThread(() -> {
                    if (!canRender(generation)) {
                        return;
                    }
                    finishRefresh();
                    renderData(data);
                    statusMessage.setTextColor(getColor(R.color.login_text_secondary));
                    statusMessage.setText(R.string.dashboard_updated);
                    refreshButton.setText(R.string.dashboard_refresh);
                });
            }

            @Override
            public void onError(DashboardRepository.DashboardError error) {
                runOnUiThread(() -> {
                    if (!canRender(generation)) {
                        return;
                    }
                    finishRefresh();
                    showError(error);
                });
            }
        });
    }

    private boolean canRender(long generation) {
        return resumed && generation == requestGeneration && !isFinishing() && !isDestroyed();
    }

    private void finishRefresh() {
        refreshRequest = null;
        loading = false;
        progress.setVisibility(View.GONE);
        refreshButton.setEnabled(true);
    }

    private void showLoading() {
        progress.setVisibility(View.VISIBLE);
        refreshButton.setEnabled(false);
        refreshButton.setText(R.string.dashboard_refresh);
        statusMessage.setTextColor(getColor(R.color.login_text_secondary));
        statusMessage.setText(R.string.dashboard_loading);
        licenseMessage.setText(R.string.dashboard_loading);
        applicationMessage.setText(R.string.dashboard_loading);
        clearValues();
    }

    private void clearValues() {
        localSummary.setText(R.string.dashboard_unknown_value);
        internationalSummary.setText(R.string.dashboard_unknown_value);
        registeredValue.setText(R.string.dashboard_unknown_value);
        activeValue.setText(R.string.dashboard_unknown_value);
        upcomingValue.setTextSize(TypedValue.COMPLEX_UNIT_SP, 14);
        upcomingValue.setText(R.string.dashboard_unknown_value);
        upcomingValue.setContentDescription(null);
    }

    private void showError(DashboardRepository.DashboardError error) {
        int message;
        switch (error) {
            case NETWORK: message = R.string.login_network_error; break;
            case NOT_FOUND: message = R.string.dashboard_not_found; break;
            case UNAUTHORIZED: message = R.string.dashboard_session_expired; break;
            case FORBIDDEN: message = R.string.dashboard_forbidden; break;
            default: message = R.string.dashboard_server_error; break;
        }
        statusMessage.setTextColor(getColor(R.color.login_error));
        statusMessage.setText(message);
        licenseMessage.setText(R.string.dashboard_license_placeholder);
        applicationMessage.setText(R.string.dashboard_application_placeholder);
        clearValues();
        refreshButton.setText(R.string.dashboard_retry);
    }

    private void renderData(DashboardData data) {
        int internationalCount = data.getInternationalLicenses().size();
        int total = data.getRegisteredLicenseCount();
        ZoneId zone = ZoneId.systemDefault();
        licenseMessage.setText(total == 0 ? getString(R.string.dashboard_no_licenses)
                : getString(R.string.dashboard_available_licenses, total));
        localSummary.setText(textMapper.localLicenseSummary(data.getLocalLicenses(), zone));
        internationalSummary.setText(textMapper.internationalLicenseSummary(internationalCount));
        registeredValue.setText(String.valueOf(total));
        activeValue.setText(String.valueOf(data.getActiveApplicationCount()));

        TestAppointment upcoming = data.getUpcomingAppointment(Instant.now(), zone);
        if (upcoming == null) {
            upcomingValue.setText(R.string.dashboard_no_upcoming_appointment);
        } else {
            Instant date = ApiDateTime.parse(upcoming.getAppointmentDate(), zone);
            upcomingValue.setText(DateTimeFormatter.ofPattern("dd/MM/yyyy\nHH:mm", Locale.getDefault())
                    .withZone(zone).format(date));
            String spokenDate = DateTimeFormatter.ofLocalizedDateTime(FormatStyle.MEDIUM)
                    .withLocale(new Locale("ar")).withZone(zone).format(date);
            upcomingValue.setContentDescription(hasText(upcoming.getTestTypeTitle())
                    ? upcoming.getTestTypeTitle() + "\n" + spokenDate : spokenDate);
        }
        renderLatestApplication(data, zone);
    }

    private void renderLatestApplication(DashboardData data, ZoneId zone) {
        if (data.getApplications().isEmpty()) {
            applicationMessage.setText(R.string.dashboard_no_applications);
            return;
        }
        CitizenApplication latest = data.getLatestApplication(zone);
        if (latest == null) {
            applicationMessage.setText(R.string.dashboard_application_date_unavailable);
            return;
        }
        StringBuilder summary = new StringBuilder();
        if (latest.getApplicationID() != null) {
            appendLine(summary, getString(R.string.dashboard_application_id, latest.getApplicationID()));
        }
        String type = textMapper.applicationType(latest.getApplicationTypeID(), latest.getApplicationTypeName());
        if (hasText(type)) {
            appendLine(summary, getString(R.string.dashboard_application_type, type));
        }
        String status = applicationStatus(latest);
        if (hasText(status)) {
            appendLine(summary, getString(R.string.dashboard_application_status, status));
        }
        Instant date = ApiDateTime.parse(latest.getApplicationDate(), zone);
        String formattedDate = DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM)
                .withLocale(new Locale("ar")).withZone(zone).format(date);
        appendLine(summary, getString(R.string.dashboard_application_date, formattedDate));
        if (hasText(latest.getClassName())) {
            appendLine(summary, getString(R.string.dashboard_application_class,
                    textMapper.licenseClass(latest.getClassName())));
        }
        applicationMessage.setText(summary);
    }

    private String applicationStatus(CitizenApplication application) {
        Integer status = application.getApplicationStatus();
        if (Integer.valueOf(1).equals(status)) return getString(R.string.dashboard_status_new);
        if (Integer.valueOf(2).equals(status)) return getString(R.string.dashboard_status_cancelled);
        if (Integer.valueOf(3).equals(status)) return getString(R.string.dashboard_status_completed);
        String text = application.getStatusText();
        if ("New".equalsIgnoreCase(text)) return getString(R.string.dashboard_status_new);
        if ("Cancelled".equalsIgnoreCase(text)) return getString(R.string.dashboard_status_cancelled);
        if ("Completed".equalsIgnoreCase(text)) return getString(R.string.dashboard_status_completed);
        return text;
    }

    private static boolean hasText(String text) {
        return text != null && !text.trim().isEmpty();
    }

    private static void appendLine(StringBuilder builder, String line) {
        if (builder.length() > 0) builder.append('\n');
        builder.append(line);
    }
}
