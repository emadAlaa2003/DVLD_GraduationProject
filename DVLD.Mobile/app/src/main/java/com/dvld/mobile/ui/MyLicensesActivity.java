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
import com.dvld.mobile.model.InternationalLicense;
import com.dvld.mobile.model.LicensesData;
import com.dvld.mobile.model.LocalLicense;
import com.dvld.mobile.repository.ApiLicensesRepository;
import com.dvld.mobile.repository.LicensesRepository;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.button.MaterialButton;

import java.time.ZoneId;

public class MyLicensesActivity extends AppCompatActivity {
    public static final String EXTRA_PERSON_ID = "personId";

    private Integer personId;
    private LicensesRepository repository;
    private LicensesRepository.RequestHandle request;
    private LicenseCardTextMapper textMapper;
    private boolean resumed;
    private boolean loading;
    private long generation;
    private boolean openingDetails;
    private LinearLayout localCards;
    private LinearLayout internationalCards;
    private TextView localMessage;
    private TextView internationalMessage;
    private TextView statusMessage;
    private ProgressBar progress;
    private MaterialButton refreshButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_my_licenses);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.my_licenses_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });

        personId = getIntent().hasExtra(EXTRA_PERSON_ID)
                ? getIntent().getIntExtra(EXTRA_PERSON_ID, -1) : null;
        localCards = findViewById(R.id.my_local_license_cards);
        internationalCards = findViewById(R.id.my_international_license_cards);
        localMessage = findViewById(R.id.my_local_license_message);
        internationalMessage = findViewById(R.id.my_international_license_message);
        statusMessage = findViewById(R.id.my_licenses_status);
        progress = findViewById(R.id.my_licenses_progress);
        refreshButton = findViewById(R.id.my_licenses_refresh);
        repository = new ApiLicensesRepository();
        textMapper = new LicenseCardTextMapper(this::getString, ZoneId.systemDefault());
        refreshButton.setOnClickListener(view -> refreshLicenses());
        findViewById(R.id.my_licenses_back).setOnClickListener(view -> finish());

        BottomNavigationView navigation = findViewById(R.id.my_licenses_navigation);
        navigation.setItemActiveIndicatorColor(ColorStateList.valueOf(getColor(R.color.login_teal_soft)));
        navigation.setSelectedItemId(R.id.dashboard_nav_licenses);
        navigation.setOnItemSelectedListener(item -> {
            if (item.getItemId() == R.id.dashboard_nav_home) {
                // Dashboard is already below this Activity in the task's back stack.
                finish();
                return false;
            }
            return item.getItemId() == R.id.dashboard_nav_licenses;
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        openingDetails = false;
        resumed = true;
        refreshLicenses();
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

    private void refreshLicenses() {
        if (!resumed || loading) return;
        if (personId == null || personId <= 0) {
            showError(LicensesRepository.LicensesError.NOT_FOUND);
            refreshButton.setEnabled(false);
            return;
        }
        loading = true;
        long current = ++generation;
        clearCards();
        progress.setVisibility(View.VISIBLE);
        refreshButton.setEnabled(false);
        refreshButton.setText(R.string.dashboard_refresh);
        statusMessage.setTextColor(getColor(R.color.login_text_secondary));
        statusMessage.setText(R.string.licenses_loading);
        localMessage.setText(R.string.licenses_loading);
        internationalMessage.setText(R.string.licenses_loading);
        request = repository.load(personId, new LicensesRepository.LicensesCallback() {
            @Override
            public void onSuccess(LicensesData data) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    finishLoad();
                    render(data);
                });
            }

            @Override
            public void onError(LicensesRepository.LicensesError error) {
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

    private void clearCards() {
        localCards.removeAllViews();
        internationalCards.removeAllViews();
        localMessage.setVisibility(View.VISIBLE);
        internationalMessage.setVisibility(View.VISIBLE);
    }

    private void showError(LicensesRepository.LicensesError error) {
        clearCards();
        int message;
        switch (error) {
            case NETWORK: message = R.string.login_network_error; break;
            case NOT_FOUND: message = R.string.licenses_not_found; break;
            case UNAUTHORIZED: message = R.string.dashboard_session_expired; break;
            case FORBIDDEN: message = R.string.licenses_forbidden; break;
            default: message = R.string.licenses_server_error; break;
        }
        statusMessage.setTextColor(getColor(R.color.login_error));
        statusMessage.setText(message);
        localMessage.setText(R.string.licenses_unavailable);
        internationalMessage.setText(R.string.licenses_unavailable);
        refreshButton.setText(R.string.dashboard_retry);
    }

    private void render(LicensesData data) {
        statusMessage.setTextColor(getColor(R.color.login_text_secondary));
        statusMessage.setText(R.string.dashboard_updated);
        refreshButton.setText(R.string.dashboard_refresh);
        localMessage.setText(R.string.dashboard_no_local_licenses);
        internationalMessage.setText(R.string.dashboard_no_international_licenses);
        localMessage.setVisibility(data.getLocalLicenses().isEmpty() ? View.VISIBLE : View.GONE);
        internationalMessage.setVisibility(data.getInternationalLicenses().isEmpty() ? View.VISIBLE : View.GONE);
        for (LocalLicense license : data.getLocalLicenses()) {
            View card = addCard(localCards, textMapper.localTitle(license), textMapper.localDetails(license));
            Integer id = license.getLicenseID();
            if (id != null && id > 0) {
                card.setFocusable(true);
                card.setOnClickListener(view -> openLocalLicense(id));
            }
        }
        for (InternationalLicense license : data.getInternationalLicenses()) {
            addCard(internationalCards, getString(R.string.licenses_international_card_title),
                    textMapper.internationalDetails(license));
        }
    }

    private void openLocalLicense(int licenseId) {
        if (openingDetails) return;
        openingDetails = true;
        Intent intent = new Intent(this, LocalLicenseDetailsActivity.class);
        intent.putExtra(LocalLicenseDetailsActivity.EXTRA_LICENSE_ID, licenseId);
        startActivity(intent);
    }

    private View addCard(LinearLayout container, String title, String details) {
        View card = getLayoutInflater().inflate(R.layout.item_license_card, container, false);
        ((TextView) card.findViewById(R.id.license_card_title)).setText(title);
        ((TextView) card.findViewById(R.id.license_card_details)).setText(details);
        container.addView(card);
        return card;
    }
}
