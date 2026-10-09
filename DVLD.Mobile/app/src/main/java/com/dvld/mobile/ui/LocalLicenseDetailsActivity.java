package com.dvld.mobile.ui;

import android.content.res.ColorStateList;
import android.os.Bundle;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.dvld.mobile.R;
import com.dvld.mobile.model.LocalLicenseDetails;
import com.dvld.mobile.repository.ApiLocalLicenseDetailsRepository;
import com.dvld.mobile.repository.LocalLicenseDetailsRepository;
import com.google.android.material.button.MaterialButton;

import java.time.ZoneId;

public class LocalLicenseDetailsActivity extends AppCompatActivity {
    public static final String EXTRA_LICENSE_ID = "licenseId";

    private Integer licenseId;
    private LocalLicenseDetailsRepository repository;
    private LocalLicenseDetailsRepository.RequestHandle request;
    private LocalLicenseDetailsTextMapper textMapper;
    private boolean loading;
    private boolean resumed;
    private long generation;
    private View content;
    private ProgressBar progress;
    private MaterialButton retryButton;
    private TextView message;
    private TextView title;
    private TextView number;
    private TextView badge;
    private TextView detainedBadge;
    private TextView citizenInformation;
    private TextView licenseInformation;
    private TextView notes;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_local_license_details);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.license_details_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        licenseId = getIntent().hasExtra(EXTRA_LICENSE_ID)
                ? getIntent().getIntExtra(EXTRA_LICENSE_ID, -1) : null;
        content = findViewById(R.id.license_details_content);
        progress = findViewById(R.id.license_details_progress);
        retryButton = findViewById(R.id.license_details_retry);
        message = findViewById(R.id.license_details_message);
        title = findViewById(R.id.license_details_class_title);
        number = findViewById(R.id.license_details_number);
        badge = findViewById(R.id.license_details_badge);
        detainedBadge = findViewById(R.id.license_details_detained_badge);
        citizenInformation = findViewById(R.id.license_details_citizen_information);
        licenseInformation = findViewById(R.id.license_details_license_information);
        notes = findViewById(R.id.license_details_notes);
        repository = new ApiLocalLicenseDetailsRepository();
        textMapper = new LocalLicenseDetailsTextMapper(this::getString, ZoneId.systemDefault());
        findViewById(R.id.license_details_back).setOnClickListener(view -> finish());
        retryButton.setOnClickListener(view -> loadDetails());
    }

    @Override
    protected void onResume() {
        super.onResume();
        resumed = true;
        loadDetails();
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

    private void loadDetails() {
        if (!resumed || loading) return;
        if (licenseId == null || licenseId <= 0) {
            showError(LocalLicenseDetailsRepository.DetailsError.NOT_FOUND);
            retryButton.setEnabled(false);
            return;
        }
        loading = true;
        long current = ++generation;
        content.setVisibility(View.GONE);
        progress.setVisibility(View.VISIBLE);
        retryButton.setEnabled(false);
        retryButton.setText(R.string.dashboard_refresh);
        message.setTextColor(getColor(R.color.login_text_secondary));
        message.setText(R.string.license_details_loading);
        request = repository.load(licenseId, new LocalLicenseDetailsRepository.DetailsCallback() {
            @Override
            public void onSuccess(LocalLicenseDetails details) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    render(details);
                });
            }

            @Override
            public void onError(LocalLicenseDetailsRepository.DetailsError error) {
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
        retryButton.setEnabled(true);
    }

    private void showError(LocalLicenseDetailsRepository.DetailsError error) {
        content.setVisibility(View.GONE);
        int text;
        switch (error) {
            case NETWORK: text = R.string.login_network_error; break;
            case NOT_FOUND: text = R.string.license_details_not_found; break;
            case UNAUTHORIZED: text = R.string.dashboard_session_expired; break;
            case FORBIDDEN: text = R.string.licenses_forbidden; break;
            default: text = R.string.license_details_server_error; break;
        }
        message.setTextColor(getColor(R.color.login_error));
        message.setText(text);
        retryButton.setText(R.string.dashboard_retry);
    }

    private void render(LocalLicenseDetails details) {
        title.setText(textMapper.title(details));
        number.setText(textMapper.licenseId(details));
        badge.setText(textMapper.statusText(details));
        int background = R.color.login_input_background;
        int foreground = R.color.login_navy;
        switch (textMapper.status(details)) {
            case ACTIVE:
                background = R.color.login_teal_soft;
                foreground = R.color.login_teal;
                break;
            case EXPIRED:
                background = R.color.dashboard_gold_soft;
                break;
            default: break;
        }
        badge.setBackgroundTintList(ColorStateList.valueOf(getColor(background)));
        badge.setTextColor(getColor(foreground));
        detainedBadge.setVisibility(textMapper.showDetainedBadge(details) ? View.VISIBLE : View.GONE);
        citizenInformation.setText(textMapper.citizenInformation(details));
        licenseInformation.setText(textMapper.licenseInformation(details));
        notes.setText(textMapper.notes(details.getNotes()));
        content.setVisibility(View.VISIBLE);
        message.setTextColor(getColor(R.color.login_text_secondary));
        message.setText(R.string.dashboard_updated);
        retryButton.setText(R.string.dashboard_refresh);
    }
}
