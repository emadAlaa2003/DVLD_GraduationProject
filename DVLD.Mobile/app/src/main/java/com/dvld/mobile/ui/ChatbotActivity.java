package com.dvld.mobile.ui;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;
import androidx.activity.EdgeToEdge;
import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import com.dvld.mobile.R;
import com.dvld.mobile.chat.ChatRequestManager;
import com.dvld.mobile.chat.ChatRuntime;
import com.dvld.mobile.model.ChatSource;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;
import java.util.List;

public class ChatbotActivity extends AppCompatActivity {
    private ChatRuntime runtime;
    private ChatRequestManager manager;
    private ChatTextMapper textMapper;
    private TextInputLayout inputLayout;
    private TextInputEditText input;
    private MaterialButton send;
    private LinearLayout history;
    private ScrollView scroll;
    private View empty;
    private boolean visible;
    private final ChatRequestManager.Listener listener = this::render;
    private final ActivityResultLauncher<String> notificationPermission = registerForActivityResult(
            new ActivityResultContracts.RequestPermission(), granted -> { });

    @Override protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_chatbot);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.chat_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            Insets keyboard = insets.getInsets(WindowInsetsCompat.Type.ime());
            view.setPadding(bars.left, bars.top, bars.right, Math.max(bars.bottom, keyboard.bottom));
            return WindowInsetsCompat.CONSUMED;
        });
        runtime = ChatRuntime.get(getApplicationContext());
        runtime.rememberDashboard(getIntent());
        manager = runtime.getManager();
        textMapper = new ChatTextMapper(this::getString);
        inputLayout = findViewById(R.id.chat_input_layout);
        input = findViewById(R.id.chat_input);
        send = findViewById(R.id.chat_send);
        history = findViewById(R.id.chat_history);
        scroll = findViewById(R.id.chat_scroll);
        empty = findViewById(R.id.chat_empty);
        findViewById(R.id.chat_back).setOnClickListener(view -> finish());
        send.setOnClickListener(view -> sendQuestion());
        input.setOnEditorActionListener((view, action, event) -> {
            if (action == EditorInfo.IME_ACTION_SEND) { sendQuestion(); return true; }
            return false;
        });
        input.addTextChangedListener(new TextWatcher() {
            @Override public void beforeTextChanged(CharSequence s, int start, int count, int after) { }
            @Override public void onTextChanged(CharSequence s, int start, int before, int count) { inputLayout.setError(null); }
            @Override public void afterTextChanged(Editable text) { }
        });
        if (Build.VERSION.SDK_INT >= 33 && ContextCompat.checkSelfPermission(this,
                Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED
                && runtime.claimPermissionPrompt()) {
            notificationPermission.launch(Manifest.permission.POST_NOTIFICATIONS);
        }
    }

    @Override protected void onStart() {
        super.onStart();
        visible = true;
        runtime.clearNotification();
        manager.addListener(listener);
    }

    @Override protected void onStop() {
        visible = false;
        manager.removeListener(listener);
        // The process-scoped manager keeps its request running; only this UI detaches.
        super.onStop();
    }

    @Override protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        runtime.rememberDashboard(intent);
        runtime.clearNotification();
        if (visible) render(manager.getHistory());
    }

    private void sendQuestion() {
        if (!visible || manager.isLoading()) return;
        String question = input.getText() == null ? null : input.getText().toString();
        ChatRequestManager.StartResult result = manager.ask(question);
        if (result == ChatRequestManager.StartResult.BLANK) {
            inputLayout.setError(getString(R.string.chat_question_required));
            input.requestFocus();
        } else if (result == ChatRequestManager.StartResult.STARTED) {
            input.setText("");
            inputLayout.setError(null);
        }
    }

    private void render(List<ChatRequestManager.Turn> turns) {
        if (!visible || isFinishing() || isDestroyed()) return;
        boolean loading = manager.isLoading();
        input.setEnabled(!loading);
        send.setEnabled(!loading);
        empty.setVisibility(turns.isEmpty() ? View.VISIBLE : View.GONE);
        history.removeAllViews();
        for (ChatRequestManager.Turn turn : turns) {
            View item = getLayoutInflater().inflate(R.layout.item_chat_turn, history, false);
            ((TextView) item.findViewById(R.id.chat_question_text)).setText(turn.getQuestion());
            TextView answer = item.findViewById(R.id.chat_answer_text);
            View waiting = item.findViewById(R.id.chat_waiting);
            MaterialButton retry = item.findViewById(R.id.chat_retry);
            LinearLayout sources = item.findViewById(R.id.chat_sources);
            if (turn.getState() == ChatRequestManager.State.LOADING) {
                answer.setText(R.string.chat_preparing);
                waiting.setVisibility(View.VISIBLE);
            } else if (turn.getState() == ChatRequestManager.State.ERROR) {
                answer.setText(textMapper.error(turn.getError()));
                answer.setTextColor(getColor(R.color.login_error));
                retry.setVisibility(View.VISIBLE);
                retry.setEnabled(!loading);
                retry.setOnClickListener(view -> {
                    if (visible && !manager.isLoading()) manager.ask(turn.getQuestion());
                });
            } else {
                answer.setText(turn.getResponse().getAnswer());
                List<ChatSource> references = turn.getResponse().getSources();
                if (!references.isEmpty()) {
                    sources.setVisibility(View.VISIBLE);
                    for (ChatSource source : references) {
                        TextView reference = new TextView(this);
                        reference.setText(textMapper.source(source));
                        reference.setTextColor(getColor(R.color.login_text_secondary));
                        reference.setTextSize(13);
                        reference.setPadding(0, dp(6), 0, 0);
                        sources.addView(reference);
                    }
                }
            }
            history.addView(item);
        }
        scroll.post(() -> {
            if (visible && !isDestroyed()) scroll.fullScroll(View.FOCUS_DOWN);
        });
    }

    private int dp(int value) { return Math.round(value * getResources().getDisplayMetrics().density); }
}
