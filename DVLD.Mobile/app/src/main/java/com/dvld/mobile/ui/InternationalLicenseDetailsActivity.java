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
import com.dvld.mobile.model.InternationalLicenseDetails;
import com.dvld.mobile.repository.ApiInternationalLicenseDetailsRepository;
import com.dvld.mobile.repository.InternationalLicenseDetailsRepository;
import com.google.android.material.button.MaterialButton;

import java.time.ZoneId;

public class InternationalLicenseDetailsActivity extends AppCompatActivity {
    public static final String EXTRA_INTERNATIONAL_LICENSE_ID = "internationalLicenseId";

    private Integer internationalLicenseId;
    private InternationalLicenseDetailsRepository repository;
    private InternationalLicenseDetailsRepository.RequestHandle request;
    private InternationalLicenseDetailsTextMapper textMapper;
    private boolean loading;
    private boolean resumed;
    private long generation;
    private View content;
    private ProgressBar progress;
    private MaterialButton retryButton;
    private TextView message;
    private TextView number;
    private TextView badge;
    private TextView citizenInformation;
    private TextView licenseInformation;
    private TextView applicationInformation;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_international_license_details);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.international_license_details_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        internationalLicenseId = getIntent().hasExtra(EXTRA_INTERNATIONAL_LICENSE_ID)
                ? getIntent().getIntExtra(EXTRA_INTERNATIONAL_LICENSE_ID, -1) : null;
        content = findViewById(R.id.international_license_details_content);
        progress = findViewById(R.id.international_license_details_progress);
        retryButton = findViewById(R.id.international_license_details_retry);
        message = findViewById(R.id.international_license_details_message);
        number = findViewById(R.id.international_license_details_number);
        badge = findViewById(R.id.international_license_details_badge);
        citizenInformation = findViewById(R.id.international_license_details_citizen_information);
        licenseInformation = findViewById(R.id.international_license_details_license_information);
        applicationInformation = findViewById(R.id.international_license_details_application_information);
        repository = new ApiInternationalLicenseDetailsRepository();
        textMapper = new InternationalLicenseDetailsTextMapper(this::getString, ZoneId.systemDefault());
        findViewById(R.id.international_license_details_back).setOnClickListener(view -> finish());
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
        if (internationalLicenseId == null || internationalLicenseId <= 0) {
            showError(InternationalLicenseDetailsRepository.DetailsError.NOT_FOUND);
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
        message.setText(R.string.international_license_details_loading);
        request = repository.load(internationalLicenseId, new InternationalLicenseDetailsRepository.DetailsCallback() {
            @Override
            public void onSuccess(InternationalLicenseDetails details) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    render(details);
                });
            }

            @Override
            public void onError(InternationalLicenseDetailsRepository.DetailsError error) {
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

    private void showError(InternationalLicenseDetailsRepository.DetailsError error) {
        content.setVisibility(View.GONE);
        int text;
        switch (error) {
            case NETWORK: text = R.string.login_network_error; break;
            case NOT_FOUND: text = R.string.international_license_details_not_found; break;
            case UNAUTHORIZED: text = R.string.dashboard_session_expired; break;
            case FORBIDDEN: text = R.string.licenses_forbidden; break;
            default: text = R.string.international_license_details_server_error; break;
        }
        message.setTextColor(getColor(R.color.login_error));
        message.setText(text);
        retryButton.setText(R.string.dashboard_retry);
    }

    private void render(InternationalLicenseDetails details) {
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
        citizenInformation.setText(textMapper.citizenInformation(details));
        licenseInformation.setText(textMapper.licenseInformation(details));
        applicationInformation.setText(textMapper.applicationInformation(details));
        content.setVisibility(View.VISIBLE);
        message.setTextColor(getColor(R.color.login_text_secondary));
        message.setText(R.string.dashboard_updated);
        retryButton.setText(R.string.dashboard_refresh);
    }
}
