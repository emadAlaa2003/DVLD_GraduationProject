package com.dvld.mobile.ui;

import android.content.Intent;
import android.content.res.ColorStateList;
import android.os.Bundle;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.dvld.mobile.R;
import com.dvld.mobile.model.TestAppointment;
import com.dvld.mobile.repository.ApiAppointmentsRepository;
import com.dvld.mobile.repository.AppointmentsRepository;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.chip.ChipGroup;

import java.time.ZoneId;
import java.time.Instant;
import java.util.List;
import java.util.ArrayList;
import java.util.Collections;

public class AppointmentsActivity extends AppCompatActivity {
    public static final String EXTRA_PERSON_ID = "personId";
    public static final String EXTRA_VIEW_MODE = "viewMode";
    public static final String MODE_APPOINTMENTS = "APPOINTMENTS";
    public static final String MODE_TESTS_RESULTS = "TESTS_AND_RESULTS";
    private static final String STATE_FILTER_ID = "appointmentsFilterId";
    private static final String STATE_VIEW_MODE = "appointmentsViewMode";

    private Integer personId;
    private AppointmentsRepository repository;
    private AppointmentsRepository.RequestHandle request;
    private AppointmentsTextMapper textMapper;
    private boolean resumed;
    private boolean loading;
    private boolean openingDestination;
    private long generation;
    private LinearLayout cards;
    private TextView emptyMessage;
    private TextView statusMessage;
    private ProgressBar progress;
    private MaterialButton refreshButton;
    private ChipGroup filters;
    private List<TestAppointment> loadedAppointments;
    private AppointmentsTextMapper.Filter selectedFilter = AppointmentsTextMapper.Filter.ALL;
    private AppointmentsTextMapper.ViewMode viewMode = AppointmentsTextMapper.ViewMode.APPOINTMENTS;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_appointments);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.appointments_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        personId = getIntent().hasExtra(EXTRA_PERSON_ID)
                ? getIntent().getIntExtra(EXTRA_PERSON_ID, -1) : null;
        cards = findViewById(R.id.appointments_cards);
        emptyMessage = findViewById(R.id.appointments_empty);
        statusMessage = findViewById(R.id.appointments_status);
        progress = findViewById(R.id.appointments_progress);
        refreshButton = findViewById(R.id.appointments_refresh);
        repository = new ApiAppointmentsRepository();
        textMapper = new AppointmentsTextMapper(this::getString, ZoneId.systemDefault());
        filters = findViewById(R.id.appointments_filters);
        AppointmentsTextMapper.ViewMode initialMode = AppointmentsTextMapper.ViewMode.fromValue(
                savedInstanceState == null ? getIntent().getStringExtra(EXTRA_VIEW_MODE)
                        : savedInstanceState.getString(STATE_VIEW_MODE, getIntent().getStringExtra(EXTRA_VIEW_MODE)));
        applyViewMode(initialMode, savedInstanceState == null ? R.id.appointments_filter_all
                : savedInstanceState.getInt(STATE_FILTER_ID, R.id.appointments_filter_all));
        filters.setOnCheckedStateChangeListener((group, checkedIds) -> {
            selectedFilter = AppointmentsTextMapper.filterForMode(filterForId(group.getCheckedChipId()), viewMode);
            int selectedId = idForFilter(selectedFilter);
            if (group.getCheckedChipId() != selectedId) {
                group.check(selectedId);
                return;
            }
            if (resumed && loadedAppointments != null) render(loadedAppointments);
        });
        refreshButton.setOnClickListener(view -> refreshAppointments());
        findViewById(R.id.appointments_back).setOnClickListener(view -> finish());

        BottomNavigationView navigation = findViewById(R.id.appointments_navigation);
        navigation.setItemActiveIndicatorColor(ColorStateList.valueOf(getColor(R.color.login_teal_soft)));
        navigation.setSelectedItemId(R.id.dashboard_nav_appointments);
        navigation.setOnItemSelectedListener(item -> {
            if (item.getItemId() == R.id.dashboard_nav_home) {
                finish();
                return false;
            }
            if (item.getItemId() == R.id.dashboard_nav_licenses) {
                openMyLicenses();
                return false;
            }
            if (item.getItemId() == R.id.dashboard_nav_applications) {
                openApplications();
                return false;
            }
            return item.getItemId() == R.id.dashboard_nav_appointments;
        });
        navigation.setOnItemReselectedListener(item -> {
            if (item.getItemId() == R.id.dashboard_nav_appointments
                    && viewMode == AppointmentsTextMapper.ViewMode.TESTS_AND_RESULTS) {
                applyViewMode(AppointmentsTextMapper.ViewMode.APPOINTMENTS, R.id.appointments_filter_all);
                if (resumed && loadedAppointments != null) render(loadedAppointments);
            }
        });
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        outState.putInt(STATE_FILTER_ID, filters.getCheckedChipId());
        outState.putString(STATE_VIEW_MODE, viewMode.name());
        super.onSaveInstanceState(outState);
    }

    private void applyViewMode(AppointmentsTextMapper.ViewMode mode, int filterId) {
        viewMode = mode;
        getIntent().putExtra(EXTRA_VIEW_MODE, mode.name());
        boolean testsResults = mode == AppointmentsTextMapper.ViewMode.TESTS_AND_RESULTS;
        ((TextView) findViewById(R.id.appointments_title)).setText(testsResults
                ? R.string.dashboard_tests_results : R.string.dashboard_my_appointments);
        ((TextView) findViewById(R.id.appointments_intro)).setText(testsResults
                ? R.string.tests_results_intro : R.string.appointments_intro);
        findViewById(R.id.appointments_filter_upcoming).setVisibility(testsResults ? View.GONE : View.VISIBLE);
        selectedFilter = AppointmentsTextMapper.filterForMode(filterForId(filterId), mode);
        filters.check(idForFilter(selectedFilter));
    }

    private int idForFilter(AppointmentsTextMapper.Filter filter) {
        switch (filter) {
            case UPCOMING: return R.id.appointments_filter_upcoming;
            case AWAITING_RESULT: return R.id.appointments_filter_awaiting_result;
            case PASSED: return R.id.appointments_filter_passed;
            case FAILED: return R.id.appointments_filter_failed;
            default: return R.id.appointments_filter_all;
        }
    }

    private AppointmentsTextMapper.Filter filterForId(int id) {
        if (id == R.id.appointments_filter_upcoming) return AppointmentsTextMapper.Filter.UPCOMING;
        if (id == R.id.appointments_filter_awaiting_result) return AppointmentsTextMapper.Filter.AWAITING_RESULT;
        if (id == R.id.appointments_filter_passed) return AppointmentsTextMapper.Filter.PASSED;
        if (id == R.id.appointments_filter_failed) return AppointmentsTextMapper.Filter.FAILED;
        return AppointmentsTextMapper.Filter.ALL;
    }

    @Override
    protected void onResume() {
        super.onResume();
        openingDestination = false;
        resumed = true;
        refreshAppointments();
    }

    @Override
    protected void onStop() {
        resumed = false;
        generation++;
        if (request != null) {
            request.cancel();
            request = null;
        }
        loading = false;
        super.onStop();
    }

    private void openMyLicenses() {
        if (openingDestination || personId == null || personId <= 0) return;
        openingDestination = true;
        Intent intent = new Intent(this, MyLicensesActivity.class);
        intent.putExtra(MyLicensesActivity.EXTRA_PERSON_ID, personId.intValue());
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        startActivity(intent);
        // Replace this tab so Home/Back in My Licenses still returns to the existing Dashboard.
        finish();
    }

    private void refreshAppointments() {
        if (!resumed || loading) return;
        if (personId == null || personId <= 0) {
            showError(AppointmentsRepository.AppointmentsError.NOT_FOUND);
            refreshButton.setEnabled(false);
            return;
        }
        loading = true;
        loadedAppointments = null;
        long current = ++generation;
        clearContent();
        progress.setVisibility(View.VISIBLE);
        refreshButton.setEnabled(false);
        refreshButton.setText(R.string.dashboard_refresh);
        statusMessage.setTextColor(getColor(R.color.login_text_secondary));
        statusMessage.setText(R.string.appointments_loading);
        request = repository.load(personId, new AppointmentsRepository.AppointmentsCallback() {
            @Override
            public void onSuccess(List<TestAppointment> appointments) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    loadedAppointments = appointments == null ? Collections.emptyList() : new ArrayList<>(appointments);
                    render(loadedAppointments);
                });
            }

            @Override
            public void onError(AppointmentsRepository.AppointmentsError error) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    showError(error);
                });
            }
        });
    }

    private boolean canRender(long current) {
        return resumed && current == generation && !isFinishing() && !isDestroyed();
    }

    private void finishLoad() {
        request = null;
        loading = false;
        progress.setVisibility(View.GONE);
        refreshButton.setEnabled(true);
    }

    private void clearContent() {
        cards.removeAllViews();
        emptyMessage.setVisibility(View.GONE);
    }

    private void showError(AppointmentsRepository.AppointmentsError error) {
        loadedAppointments = null;
        clearContent();
        int message;
        switch (error) {
            case NETWORK: message = R.string.login_network_error; break;
            case NOT_FOUND: message = R.string.appointments_not_found; break;
            case UNAUTHORIZED: message = R.string.dashboard_session_expired; break;
            case FORBIDDEN: message = R.string.dashboard_forbidden; break;
            default: message = R.string.appointments_server_error; break;
        }
        statusMessage.setTextColor(getColor(R.color.login_error));
        statusMessage.setText(message);
        refreshButton.setText(R.string.dashboard_retry);
    }

    private void render(List<TestAppointment> appointments) {
        clearContent();
        statusMessage.setTextColor(getColor(R.color.login_text_secondary));
        statusMessage.setText(R.string.dashboard_updated);
        refreshButton.setText(R.string.dashboard_refresh);
        Instant now = Instant.now();
        List<TestAppointment> sorted = textMapper.visibleAppointments(appointments, selectedFilter, viewMode, now);
        emptyMessage.setText(textMapper.emptyMessage(selectedFilter, viewMode));
        emptyMessage.setVisibility(sorted.isEmpty() ? View.VISIBLE : View.GONE);
        for (TestAppointment appointment : sorted) {
            View card = getLayoutInflater().inflate(R.layout.item_appointment_card, cards, false);
            ((TextView) card.findViewById(R.id.appointment_card_id)).setText(textMapper.appointmentId(appointment.getTestAppointmentID()));
            ((TextView) card.findViewById(R.id.appointment_card_type)).setText(
                    getString(R.string.application_details_test_type, textMapper.testType(appointment.getTestTypeID(), appointment.getTestTypeTitle())));
            ((TextView) card.findViewById(R.id.appointment_card_details)).setText(textMapper.details(appointment, viewMode));
            TextView badge = card.findViewById(R.id.appointment_card_status);
            AppointmentsTextMapper.Status status = textMapper.status(appointment, now);
            badge.setText(textMapper.statusText(status));
            badge.setContentDescription(getString(R.string.appointments_status, textMapper.statusText(status)));
            int background;
            int foreground;
            switch (status) {
                case PASSED: background = R.color.login_teal_soft; foreground = R.color.login_teal; break;
                case FAILED: background = R.color.login_error; foreground = R.color.white; break;
                case UPCOMING: background = R.color.login_navy; foreground = R.color.white; break;
                case AWAITING_RESULT: background = R.color.dashboard_gold_soft; foreground = R.color.login_navy; break;
                default: background = R.color.login_input_background; foreground = R.color.login_text_secondary; break;
            }
            badge.setBackgroundTintList(ColorStateList.valueOf(getColor(background)));
            badge.setTextColor(getColor(foreground));
            Integer id = appointment.getTestAppointmentID();
            if (id != null && id > 0) {
                card.setFocusable(true);
                card.setOnClickListener(view -> openTestAppointmentDetails(id));
            }
            cards.addView(card);
        }
    }
    private void openTestAppointmentDetails(int testAppointmentId) {
        if (openingDestination) return;
        openingDestination = true;
        Intent intent = new Intent(this, TestAppointmentDetailsActivity.class);
        intent.putExtra(TestAppointmentDetailsActivity.EXTRA_TEST_APPOINTMENT_ID, testAppointmentId);
        startActivity(intent);
    }

    private void openApplications() {
        if (openingDestination || personId == null || personId <= 0) return;
        openingDestination = true;
        Intent intent = new Intent(this, ApplicationsActivity.class);
        intent.putExtra(ApplicationsActivity.EXTRA_PERSON_ID, personId.intValue());
        intent.putExtra(ApplicationsActivity.EXTRA_ACTIVE_ONLY, false);
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
        startActivity(intent);
        finish();
    }
}
