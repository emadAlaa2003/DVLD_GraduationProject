package com.dvld.mobile;

import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.inputmethod.EditorInfo;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;

public class MainActivity extends AppCompatActivity {

    private TextInputLayout usernameLayout;
    private TextInputLayout passwordLayout;
    private TextInputEditText usernameInput;
    private TextInputEditText passwordInput;

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

        clearErrorOnEdit(usernameInput, usernameLayout);
        clearErrorOnEdit(passwordInput, passwordLayout);
        findViewById(R.id.login_button).setOnClickListener(v -> validateLoginInputs());
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
        // Valid input leaves the form ready for future API integration.
    }

    private void clearValidationError(TextInputLayout layout) {
        layout.setError(null);
        layout.setErrorEnabled(false);
    }
}
