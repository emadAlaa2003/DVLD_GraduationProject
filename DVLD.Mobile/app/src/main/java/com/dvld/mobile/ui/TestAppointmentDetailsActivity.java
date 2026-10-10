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
import com.dvld.mobile.model.TestAppointmentDetails;
import com.dvld.mobile.repository.ApiTestAppointmentDetailsRepository;
import com.dvld.mobile.repository.TestAppointmentDetailsRepository;
import com.google.android.material.button.MaterialButton;

import java.time.ZoneId;
import java.time.Instant;

public class TestAppointmentDetailsActivity extends AppCompatActivity {
    public static final String EXTRA_TEST_APPOINTMENT_ID = "testAppointmentId";

    private Integer testAppointmentId;
    private TestAppointmentDetailsRepository repository;
    private TestAppointmentDetailsRepository.RequestHandle request;
    private TestAppointmentDetailsTextMapper textMapper;
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
        setContentView(R.layout.activity_test_appointment_details);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.test_appointment_details_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        testAppointmentId = getIntent().hasExtra(EXTRA_TEST_APPOINTMENT_ID)
                ? getIntent().getIntExtra(EXTRA_TEST_APPOINTMENT_ID, -1) : null;
        content = findViewById(R.id.test_appointment_details_content);
        progress = findViewById(R.id.test_appointment_details_progress);
        retryButton = findViewById(R.id.test_appointment_details_retry);
        message = findViewById(R.id.test_appointment_details_message);
        title = findViewById(R.id.test_appointment_details_summary_title);
        number = findViewById(R.id.test_appointment_details_number);
        badge = findViewById(R.id.test_appointment_details_badge);
        commonInformation = findViewById(R.id.test_appointment_details_common_information);
        additionalInformation = findViewById(R.id.test_appointment_details_additional_information);
        repository = new ApiTestAppointmentDetailsRepository();
        textMapper = new TestAppointmentDetailsTextMapper(this::getString, ZoneId.systemDefault());
        findViewById(R.id.test_appointment_details_back).setOnClickListener(view -> finish());
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
        if (testAppointmentId == null || testAppointmentId <= 0) {
            showError(TestAppointmentDetailsRepository.DetailsError.NOT_FOUND);
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
        message.setText(R.string.test_appointment_details_loading);
        request = repository.load(testAppointmentId, new TestAppointmentDetailsRepository.DetailsCallback() {
            @Override
            public void onSuccess(TestAppointmentDetails details) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    render(details);
                });
            }

            @Override
            public void onError(TestAppointmentDetailsRepository.DetailsError error) {
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

    private void showError(TestAppointmentDetailsRepository.DetailsError error) {
        content.setVisibility(View.GONE);
        int text;
        switch (error) {
            case NETWORK: text = R.string.login_network_error; break;
            case NOT_FOUND: text = R.string.test_appointment_details_not_found; break;
            case UNAUTHORIZED: text = R.string.dashboard_session_expired; break;
            case FORBIDDEN: text = R.string.dashboard_forbidden; break;
            default: text = R.string.test_appointment_details_server_error; break;
        }
        message.setTextColor(getColor(R.color.login_error));
        message.setText(text);
        retryButton.setText(R.string.dashboard_retry);
    }

    private void render(TestAppointmentDetails details) {
        Instant now = Instant.now();
        title.setText(textMapper.testType(details));
        number.setText(textMapper.appointmentId(details));
        badge.setText(textMapper.statusText(details, now));
        int background = R.color.login_input_background;
        int foreground = R.color.login_navy;
        switch (textMapper.status(details, now)) {
            case PASSED:
                background = R.color.login_teal_soft;
                foreground = R.color.login_teal;
                break;
            case UPCOMING:
                background = R.color.login_teal_soft;
                break;
            case FAILED:
                background = R.color.login_error;
                foreground = R.color.white;
                break;
            case AWAITING_RESULT:
                background = R.color.dashboard_gold_soft;
                break;
            default: break;
        }
        badge.setBackgroundTintList(ColorStateList.valueOf(getColor(background)));
        badge.setTextColor(getColor(foreground));
        commonInformation.setText(textMapper.commonInformation(details, now));
        additionalInformation.setText(textMapper.testInformation(details));
        content.setVisibility(View.VISIBLE);
        message.setTextColor(getColor(R.color.login_text_secondary));
        message.setText(R.string.dashboard_updated);
        retryButton.setText(R.string.dashboard_refresh);
    }
}
