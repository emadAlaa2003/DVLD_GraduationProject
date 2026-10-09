package com.dvld.mobile;

import android.content.Intent;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.ProgressBar;
import android.widget.TextView;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;
import com.google.android.material.button.MaterialButton;
import com.dvld.mobile.model.MobileLoginResponse;
import com.dvld.mobile.repository.ApiAuthRepository;
import com.dvld.mobile.repository.AuthRepository;
import com.dvld.mobile.ui.DashboardActivity;

public class MainActivity extends AppCompatActivity {

    private TextInputLayout usernameLayout;
    private TextInputLayout passwordLayout;
    private TextInputEditText usernameInput;
    private TextInputEditText passwordInput;
    private MaterialButton loginButton;
    private TextView loginError;
    private ProgressBar loginProgress;
    private AuthRepository authRepository;
    private AuthRepository.RequestHandle loginRequest;
    private boolean loginInProgress;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_main);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.main), (v, insets) -> {
            Insets systemBars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            v.setPadding(systemBars.left, systemBars.top, systemBars.right, systemBars.bottom);
            return insets;
        });
        setupLoginForm();
    }

    private void setupLoginForm() {
        usernameLayout = findViewById(R.id.username_layout);
        passwordLayout = findViewById(R.id.password_layout);
        usernameInput = findViewById(R.id.username_input);
        passwordInput = findViewById(R.id.password_input);
        // Do not include the password in the Activity's saved view state.
        passwordInput.setSaveEnabled(false);
        loginButton = findViewById(R.id.login_button);
        loginError = findViewById(R.id.login_error);
        loginProgress = findViewById(R.id.login_progress);
        authRepository = new ApiAuthRepository();

        clearErrorOnEdit(usernameInput, usernameLayout);
        clearErrorOnEdit(passwordInput, passwordLayout);
        loginButton.setOnClickListener(v -> validateLoginInputs());
        passwordInput.setOnEditorActionListener((v, actionId, event) -> {
            if (actionId == EditorInfo.IME_ACTION_DONE) {
                validateLoginInputs();
                return true;
            }
            return false;
        });
    }

    private void clearErrorOnEdit(TextInputEditText input, TextInputLayout layout) {
        input.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                clearValidationError(layout);
            }

            @Override
            public void afterTextChanged(Editable s) {
            }
        });
    }

    private void validateLoginInputs() {
        if (loginInProgress) {
            return;
        }
        Editable username = usernameInput.getText();
        Editable password = passwordInput.getText();
        boolean usernameMissing = username == null || username.toString().trim().isEmpty();
        boolean passwordMissing = password == null || password.length() == 0;

        if (usernameMissing) {
            usernameLayout.setError(getString(R.string.login_username_required));
        } else {
            clearValidationError(usernameLayout);
        }

        if (passwordMissing) {
            passwordLayout.setError(getString(R.string.login_password_required));
        } else {
            clearValidationError(passwordLayout);
        }

        if (usernameMissing) {
            usernameInput.requestFocus();
        } else if (passwordMissing) {
            passwordInput.requestFocus();
        }
        if (!usernameMissing && !passwordMissing) {
            startLogin(username.toString().trim(), password.toString());
        }
    }

    private void startLogin(String username, String password) {
        loginError.setText(null);
        loginError.setVisibility(View.GONE);
        setLoginInProgress(true);
        loginRequest = authRepository.login(username, password, new AuthRepository.LoginCallback() {
            @Override
            public void onSuccess(MobileLoginResponse response) {
                runOnUiThread(() -> {
                    if (isFinishing() || isDestroyed()) {
                        return;
                    }
                    loginRequest = null;
                    setLoginInProgress(false);
                    Intent dashboard = new Intent(MainActivity.this, DashboardActivity.class);
                    dashboard.putExtra(DashboardActivity.EXTRA_PERSON_ID, response.getPersonId().intValue());
                    dashboard.putExtra(DashboardActivity.EXTRA_FULL_NAME, response.getFullName());
                    dashboard.putExtra(DashboardActivity.EXTRA_USERNAME, response.getUsername());
                    startActivity(dashboard);
                    finish();
                });
            }

            @Override
            public void onError(AuthRepository.LoginError error) {
                runOnUiThread(() -> {
                    if (isFinishing() || isDestroyed()) {
                        return;
                    }
                    loginRequest = null;
                    setLoginInProgress(false);
                    showLoginError(error);
                });
            }
        });
    }

    private void setLoginInProgress(boolean inProgress) {
        loginInProgress = inProgress;
        loginProgress.setVisibility(inProgress ? View.VISIBLE : View.GONE);
        loginButton.setEnabled(!inProgress);
    }

    private void showLoginError(AuthRepository.LoginError error) {
        int message;
        switch (error) {
            case INVALID_CREDENTIALS:
                message = R.string.login_invalid_credentials;
                break;
            case INACTIVE_ACCOUNT:
                message = R.string.login_inactive_account;
                break;
            case NETWORK:
                message = R.string.login_network_error;
                break;
            default:
                message = R.string.login_server_error;
                break;
        }
        loginError.setText(message);
        loginError.setVisibility(View.VISIBLE);
    }

    @Override
    protected void onDestroy() {
        if (loginRequest != null) {
            loginRequest.cancel();
            loginRequest = null;
        }
        super.onDestroy();
    }

    private void clearValidationError(TextInputLayout layout) {
        layout.setError(null);
        layout.setErrorEnabled(false);
    }
}
