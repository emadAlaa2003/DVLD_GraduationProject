package com.dvld.mobile.ui;

import android.content.res.ColorStateList;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;

import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;

import com.dvld.mobile.R;
import com.google.android.material.bottomnavigation.BottomNavigationView;

public class DashboardActivity extends AppCompatActivity {
    public static final String EXTRA_PERSON_ID = "personId";
    public static final String EXTRA_FULL_NAME = "fullName";
    public static final String EXTRA_USERNAME = "username";

    private Integer personId;
    private String fullName;
    private String username;

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
        // Other destinations remain visual placeholders until their screens are implemented.
        navigation.setOnItemSelectedListener(item -> item.getItemId() == R.id.dashboard_nav_home);
    }
}
