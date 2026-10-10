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
import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.repository.ApiApplicationsRepository;
import com.dvld.mobile.repository.ApplicationsRepository;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.button.MaterialButton;

import java.time.ZoneId;
import java.util.List;

public class ApplicationsActivity extends AppCompatActivity {
    public static final String EXTRA_PERSON_ID = "personId";

    private Integer personId;
    private ApplicationsRepository repository;
    private ApplicationsRepository.RequestHandle request;
    private ApplicationsTextMapper textMapper;
    private boolean resumed;
    private boolean loading;
    private boolean openingDestination;
    private long generation;
    private LinearLayout cards;
    private TextView emptyMessage;
    private TextView statusMessage;
    private ProgressBar progress;
    private MaterialButton refreshButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_applications);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.applications_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        personId = getIntent().hasExtra(EXTRA_PERSON_ID)
                ? getIntent().getIntExtra(EXTRA_PERSON_ID, -1) : null;
        cards = findViewById(R.id.applications_cards);
        emptyMessage = findViewById(R.id.applications_empty);
        statusMessage = findViewById(R.id.applications_status);
        progress = findViewById(R.id.applications_progress);
        refreshButton = findViewById(R.id.applications_refresh);
        repository = new ApiApplicationsRepository();
        textMapper = new ApplicationsTextMapper(this::getString, ZoneId.systemDefault());
        refreshButton.setOnClickListener(view -> refreshApplications());
        findViewById(R.id.applications_back).setOnClickListener(view -> finish());

        BottomNavigationView navigation = findViewById(R.id.applications_navigation);
        navigation.setItemActiveIndicatorColor(ColorStateList.valueOf(getColor(R.color.login_teal_soft)));
        navigation.setSelectedItemId(R.id.dashboard_nav_applications);
        navigation.setOnItemSelectedListener(item -> {
            if (item.getItemId() == R.id.dashboard_nav_home) {
                finish();
                return false;
            }
            if (item.getItemId() == R.id.dashboard_nav_licenses) {
                openMyLicenses();
                return false;
            }
            return item.getItemId() == R.id.dashboard_nav_applications;
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        openingDestination = false;
        resumed = true;
        refreshApplications();
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

    private void refreshApplications() {
        if (!resumed || loading) return;
        if (personId == null || personId <= 0) {
            showError(ApplicationsRepository.ApplicationsError.NOT_FOUND);
            refreshButton.setEnabled(false);
            return;
        }
        loading = true;
        long current = ++generation;
        clearContent();
        progress.setVisibility(View.VISIBLE);
        refreshButton.setEnabled(false);
        refreshButton.setText(R.string.dashboard_refresh);
        statusMessage.setTextColor(getColor(R.color.login_text_secondary));
        statusMessage.setText(R.string.applications_loading);
        request = repository.load(personId, new ApplicationsRepository.ApplicationsCallback() {
            @Override
            public void onSuccess(List<CitizenApplication> applications) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    render(applications);
                });
            }

            @Override
            public void onError(ApplicationsRepository.ApplicationsError error) {
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

    private void showError(ApplicationsRepository.ApplicationsError error) {
        clearContent();
        int message;
        switch (error) {
            case NETWORK: message = R.string.login_network_error; break;
            case NOT_FOUND: message = R.string.applications_not_found; break;
            case UNAUTHORIZED: message = R.string.dashboard_session_expired; break;
            case FORBIDDEN: message = R.string.dashboard_forbidden; break;
            default: message = R.string.applications_server_error; break;
        }
        statusMessage.setTextColor(getColor(R.color.login_error));
        statusMessage.setText(message);
        refreshButton.setText(R.string.dashboard_retry);
    }

    private void render(List<CitizenApplication> applications) {
        clearContent();
        statusMessage.setTextColor(getColor(R.color.login_text_secondary));
        statusMessage.setText(R.string.dashboard_updated);
        refreshButton.setText(R.string.dashboard_refresh);
        List<CitizenApplication> sorted = textMapper.sortedApplications(applications);
        emptyMessage.setVisibility(sorted.isEmpty() ? View.VISIBLE : View.GONE);
        for (CitizenApplication application : sorted) {
            View card = getLayoutInflater().inflate(R.layout.item_application_card, cards, false);
            ((TextView) card.findViewById(R.id.application_card_id)).setText(textMapper.applicationId(application));
            ((TextView) card.findViewById(R.id.application_card_type)).setText(
                    getString(R.string.dashboard_application_type, textMapper.applicationType(application)));
            ((TextView) card.findViewById(R.id.application_card_details)).setText(textMapper.details(application));
            TextView badge = card.findViewById(R.id.application_card_status);
            badge.setText(textMapper.statusText(application));
            badge.setContentDescription(getString(R.string.dashboard_application_status, textMapper.statusText(application)));
            int background;
            int foreground;
            switch (textMapper.status(application)) {
                case NEW: background = R.color.login_navy; foreground = R.color.white; break;
                case COMPLETED: background = R.color.login_teal_soft; foreground = R.color.login_teal; break;
                case CANCELLED: background = R.color.login_error; foreground = R.color.white; break;
                default: background = R.color.login_input_background; foreground = R.color.login_text_secondary; break;
            }
            badge.setBackgroundTintList(ColorStateList.valueOf(getColor(background)));
            badge.setTextColor(getColor(foreground));
            Integer id = application.getApplicationID();
            if (id != null && id > 0) {
                card.setFocusable(true);
                card.setOnClickListener(view -> openApplicationDetails(id));
            }
            cards.addView(card);
        }
    }

    private void openApplicationDetails(int applicationId) {
        if (openingDestination) return;
        openingDestination = true;
        Intent intent = new Intent(this, ApplicationDetailsActivity.class);
        intent.putExtra(ApplicationDetailsActivity.EXTRA_APPLICATION_ID, applicationId);
        startActivity(intent);
    }
}
