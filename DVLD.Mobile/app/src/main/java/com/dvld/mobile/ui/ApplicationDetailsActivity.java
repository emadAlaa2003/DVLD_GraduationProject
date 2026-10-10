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
import com.dvld.mobile.model.ApplicationDetails;
import com.dvld.mobile.repository.ApiApplicationDetailsRepository;
import com.dvld.mobile.repository.ApplicationDetailsRepository;
import com.google.android.material.button.MaterialButton;

import java.time.ZoneId;

public class ApplicationDetailsActivity extends AppCompatActivity {
    public static final String EXTRA_APPLICATION_ID = "applicationId";

    private Integer applicationId;
    private ApplicationDetailsRepository repository;
    private ApplicationDetailsRepository.RequestHandle request;
    private ApplicationDetailsTextMapper textMapper;
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
    private TextView commonInformation;
    private TextView additionalInformation;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_application_details);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.application_details_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        applicationId = getIntent().hasExtra(EXTRA_APPLICATION_ID)
                ? getIntent().getIntExtra(EXTRA_APPLICATION_ID, -1) : null;
        content = findViewById(R.id.application_details_content);
        progress = findViewById(R.id.application_details_progress);
        retryButton = findViewById(R.id.application_details_retry);
        message = findViewById(R.id.application_details_message);
        title = findViewById(R.id.application_details_summary_title);
        number = findViewById(R.id.application_details_number);
        badge = findViewById(R.id.application_details_badge);
        commonInformation = findViewById(R.id.application_details_common_information);
        additionalInformation = findViewById(R.id.application_details_additional_information);
        repository = new ApiApplicationDetailsRepository();
        textMapper = new ApplicationDetailsTextMapper(this::getString, ZoneId.systemDefault());
        findViewById(R.id.application_details_back).setOnClickListener(view -> finish());
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
        if (applicationId == null || applicationId <= 0) {
            showError(ApplicationDetailsRepository.DetailsError.NOT_FOUND);
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
        message.setText(R.string.application_details_loading);
        request = repository.load(applicationId, new ApplicationDetailsRepository.DetailsCallback() {
            @Override
            public void onSuccess(ApplicationDetails details) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    render(details);
                });
            }

            @Override
            public void onError(ApplicationDetailsRepository.DetailsError error) {
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

    private void showError(ApplicationDetailsRepository.DetailsError error) {
        content.setVisibility(View.GONE);
        int text;
        switch (error) {
            case NETWORK: text = R.string.login_network_error; break;
            case NOT_FOUND: text = R.string.application_details_not_found; break;
            case UNAUTHORIZED: text = R.string.dashboard_session_expired; break;
            case FORBIDDEN: text = R.string.dashboard_forbidden; break;
            default: text = R.string.application_details_server_error; break;
        }
        message.setTextColor(getColor(R.color.login_error));
        message.setText(text);
        retryButton.setText(R.string.dashboard_retry);
    }

    private void render(ApplicationDetails details) {
        title.setText(textMapper.applicationType(details));
        number.setText(textMapper.applicationId(details));
        badge.setText(textMapper.statusText(details));
        int background = R.color.login_input_background;
        int foreground = R.color.login_navy;
        switch (textMapper.status(details)) {
            case COMPLETED:
                background = R.color.login_teal_soft;
                foreground = R.color.login_teal;
                break;
            case NEW:
                background = R.color.login_teal_soft;
                break;
            case CANCELLED:
                background = R.color.login_error;
                foreground = R.color.white;
                break;
            default: break;
        }
        badge.setBackgroundTintList(ColorStateList.valueOf(getColor(background)));
        badge.setTextColor(getColor(foreground));
        commonInformation.setText(textMapper.commonInformation(details));
        additionalInformation.setText(textMapper.additionalInformation(details));
        content.setVisibility(View.VISIBLE);
        message.setTextColor(getColor(R.color.login_text_secondary));
        message.setText(R.string.dashboard_updated);
        retryButton.setText(R.string.dashboard_refresh);
    }
}
