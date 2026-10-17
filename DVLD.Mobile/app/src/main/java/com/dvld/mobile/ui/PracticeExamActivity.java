package com.dvld.mobile.ui;

import android.content.Intent;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.widget.RadioGroup;
import android.widget.TextView;
import androidx.activity.EdgeToEdge;
import androidx.activity.OnBackPressedCallback;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import com.dvld.mobile.R;
import com.dvld.mobile.model.*;
import com.dvld.mobile.repository.ApiPracticeExamRepository;
import com.dvld.mobile.repository.PracticeExamRepository;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.dialog.MaterialAlertDialogBuilder;
import com.google.android.material.progressindicator.LinearProgressIndicator;
import com.google.android.material.radiobutton.MaterialRadioButton;
import java.util.LinkedHashMap;
import java.util.Map;

public class PracticeExamActivity extends AppCompatActivity {
    private static final String STATE_SESSION = "practiceSession";
    private static final String STATE_LOAD_FAILED = "practiceLoadFailed";
    private static final String STATE_MESSAGE = "practiceMessage";
    private static final String STATE_INTERRUPTED = "practiceSubmitInterrupted";
    private PracticeExamRepository repository;
    private PracticeExamRepository.RequestHandle request;
    private PracticeExamSession session;
    private PracticeExamTextMapper textMapper;
    private boolean resumed;
    private boolean loading;
    private boolean loadFailed;
    private boolean openingDestination;
    private long generation;
    private View content;
    private View loadingIndicator;
    private TextView message;
    private TextView questionText;
    private TextView questionProgress;
    private RadioGroup options;
    private LinearProgressIndicator questionIndicator;
    private MaterialButton previous;
    private MaterialButton next;
    private MaterialButton retry;
    private MaterialButton back;
    private AlertDialog leaveDialog;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_practice_exam);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.practice_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });
        repository = new ApiPracticeExamRepository();
        textMapper = new PracticeExamTextMapper(this::getString);
        content = findViewById(R.id.practice_content);
        loadingIndicator = findViewById(R.id.practice_loading);
        message = findViewById(R.id.practice_message);
        questionText = findViewById(R.id.practice_question);
        questionProgress = findViewById(R.id.practice_question_progress);
        questionIndicator = findViewById(R.id.practice_question_indicator);
        options = findViewById(R.id.practice_options);
        previous = findViewById(R.id.practice_previous);
        next = findViewById(R.id.practice_next);
        retry = findViewById(R.id.practice_retry);
        back = findViewById(R.id.practice_back);
        if (savedInstanceState != null) {
            session = PracticeExamSession.restore(savedInstanceState.getString(STATE_SESSION));
            loadFailed = savedInstanceState.getBoolean(STATE_LOAD_FAILED);
            message.setText(savedInstanceState.getString(STATE_MESSAGE, ""));
            if (savedInstanceState.getBoolean(STATE_INTERRUPTED))
                message.setText(R.string.practice_submit_interrupted);
        }
        previous.setOnClickListener(view -> {
            if (!loading && session != null && session.previous()) renderQuestion();
        });
        next.setOnClickListener(view -> {
            if (loading || session == null) return;
            if (session.getCurrentIndex() == session.getQuestionCount() - 1) submitExam();
            else { session.next(); renderQuestion(); }
        });
        retry.setOnClickListener(view -> {
            if (session == null) startExam(); else submitExam();
        });
        back.setOnClickListener(view -> confirmLeave());
        getOnBackPressedDispatcher().addCallback(this, new OnBackPressedCallback(true) {
            @Override public void handleOnBackPressed() { confirmLeave(); }
        });
    }

    @Override
    protected void onResume() {
        super.onResume();
        resumed = true;
        if (session != null) renderQuestion();
        else if (!loadFailed) startExam();
        else retry.setVisibility(View.VISIBLE);
    }

    @Override
    protected void onStop() {
        resumed = false;
        generation++;
        if (loading && session != null) message.setText(R.string.practice_submit_interrupted);
        if (request != null) { request.cancel(); request = null; }
        loading = false;
        loadingIndicator.setVisibility(View.GONE);
        super.onStop();
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        if (session != null) outState.putString(STATE_SESSION, session.toJson());
        outState.putBoolean(STATE_LOAD_FAILED, loadFailed);
        outState.putString(STATE_MESSAGE, message.getText().toString());
        outState.putBoolean(STATE_INTERRUPTED, loading && session != null);
        super.onSaveInstanceState(outState);
    }

    @Override
    protected void onDestroy() {
        if (leaveDialog != null) leaveDialog.dismiss();
        super.onDestroy();
    }

    private boolean canRender(long current) {
        return resumed && generation == current && !isFinishing() && !isDestroyed() && !openingDestination;
    }

    private void startExam() {
        if (!resumed || loading || openingDestination) return;
        loadFailed = false;
        session = null;
        content.setVisibility(View.GONE);
        setLoading(true);
        message.setText(R.string.practice_loading_questions);
        long current = ++generation;
        request = repository.startExam(10, new PracticeExamRepository.ExamCallback<PracticeExamStartResponse>() {
            @Override public void onSuccess(PracticeExamStartResponse response) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    request = null;
                    session = new PracticeExamSession(response);
                    setLoading(false);
                    message.setText("");
                    renderQuestion();
                });
            }
            @Override public void onError(PracticeExamRepository.ExamError error) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    loadFailed = true;
                    showError(error, false);
                });
            }
        });
    }

    private void submitExam() {
        if (!resumed || loading || session == null || openingDestination) return;
        if (session.goToFirstUnanswered()) {
            renderQuestion();
            message.setTextColor(getColor(R.color.login_error));
            message.setText(R.string.practice_answer_all);
            return;
        }
        setLoading(true);
        message.setText(R.string.practice_submitting);
        long current = ++generation;
        request = repository.submitExam(session.buildAnswers(),
                new PracticeExamRepository.ExamCallback<PracticeExamSubmitResponse>() {
            @Override public void onSuccess(PracticeExamSubmitResponse response) {
                runOnUiThread(() -> {
                    if (!canRender(current)) return;
                    PracticeExamReview review = PracticeExamReview.create(
                            session.getQuestions(), session.buildAnswers(), response);
                    if (review == null) {
                        showError(PracticeExamRepository.ExamError.SERVER, true);
                        return;
                    }
                    request = null;
                    openingDestination = true;
                    Intent intent = new Intent(PracticeExamActivity.this, ExamResultActivity.class);
                    intent.putExtra(ExamResultActivity.EXTRA_REVIEW, review.toJson());
                    startActivity(intent);
                    finish();
                });
            }
            @Override public void onError(PracticeExamRepository.ExamError error) {
                runOnUiThread(() -> {
                    if (canRender(current)) showError(error, true);
                });
            }
        });
    }

    private void showError(PracticeExamRepository.ExamError error, boolean submitting) {
        request = null;
        setLoading(false);
        message.setTextColor(getColor(R.color.login_error));
        message.setText(textMapper.error(error, submitting));
        retry.setVisibility(View.VISIBLE);
    }

    private void setLoading(boolean value) {
        loading = value;
        loadingIndicator.setVisibility(value ? View.VISIBLE : View.GONE);
        retry.setVisibility(View.GONE);
        message.setTextColor(getColor(R.color.login_text_secondary));
        updateActions();
    }

    private void updateActions() {
        back.setEnabled(!loading || session == null);
        previous.setEnabled(!loading && session != null && session.getCurrentIndex() > 0);
        next.setEnabled(!loading && session != null);
        for (int i = 0; i < options.getChildCount(); i++) options.getChildAt(i).setEnabled(!loading);
    }

    private void renderQuestion() {
        if (session == null) return;
        content.setVisibility(View.VISIBLE);
        PracticeExamQuestion question = session.getCurrentQuestion();
        questionText.setText(question.getQuestionText());
        questionProgress.setText(textMapper.questionProgress(session.getCurrentIndex(), session.getQuestionCount()));
        questionIndicator.setMax(session.getQuestionCount());
        questionIndicator.setProgressCompat(session.getCurrentIndex() + 1, false);
        next.setText(session.getCurrentIndex() == session.getQuestionCount() - 1
                ? R.string.practice_finish : R.string.practice_next);
        options.setOnCheckedChangeListener(null);
        options.removeAllViews();
        Map<Integer, String> optionCodes = new LinkedHashMap<>();
        String selected = session.getSelectedOption(question.getQuestionID());
        int selectedId = View.NO_ID;
        for (Map.Entry<String, String> option : question.getOptions().entrySet()) {
            MaterialRadioButton button = new MaterialRadioButton(this);
            button.setId(View.generateViewId());
            button.setText(option.getValue());
            button.setTextSize(16);
            button.setTextColor(getColor(R.color.login_navy));
            button.setUseMaterialThemeColors(true);
            button.setGravity(Gravity.START | Gravity.CENTER_VERTICAL);
            button.setMinHeight(dp(56));
            button.setPadding(dp(8), dp(8), dp(8), dp(8));
            RadioGroup.LayoutParams layout = new RadioGroup.LayoutParams(
                    RadioGroup.LayoutParams.MATCH_PARENT, RadioGroup.LayoutParams.WRAP_CONTENT);
            layout.topMargin = dp(8);
            options.addView(button, layout);
            optionCodes.put(button.getId(), option.getKey());
            if (option.getKey().equals(selected)) selectedId = button.getId();
        }
        if (selectedId != View.NO_ID) options.check(selectedId);
        options.setOnCheckedChangeListener((group, checkedId) -> {
            String code = optionCodes.get(checkedId);
            if (!loading && code != null && session.select(code)) message.setText("");
        });
        updateActions();
    }

    private int dp(int value) { return Math.round(value * getResources().getDisplayMetrics().density); }

    private void confirmLeave() {
        if (openingDestination || (loading && session != null)) return;
        if (session == null) { finish(); return; }
        if (leaveDialog != null && leaveDialog.isShowing()) return;
        leaveDialog = new MaterialAlertDialogBuilder(this)
                .setMessage(R.string.practice_leave_confirmation)
                .setNegativeButton(R.string.practice_continue, (dialog, which) -> dialog.dismiss())
                .setPositiveButton(R.string.practice_leave, (dialog, which) -> finish())
                .show();
    }
}
