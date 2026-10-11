package com.dvld.mobile.ui;

import android.content.Intent;
import android.content.res.ColorStateList;
import android.os.Bundle;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.TextView;
import androidx.activity.EdgeToEdge;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import com.dvld.mobile.R;
import com.dvld.mobile.model.PracticeExamReview;

public class ExamResultActivity extends AppCompatActivity {
    public static final String EXTRA_REVIEW = "practiceExamReview";
    private boolean openingDestination;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        EdgeToEdge.enable(this);
        setContentView(R.layout.activity_exam_result);
        ViewCompat.setOnApplyWindowInsetsListener(findViewById(R.id.exam_result_root), (view, insets) -> {
            Insets bars = insets.getInsets(WindowInsetsCompat.Type.systemBars());
            view.setPadding(bars.left, bars.top, bars.right, bars.bottom);
            return WindowInsetsCompat.CONSUMED;
        });
        // The self-contained original Intent is retained on rotation/process recreation.
        PracticeExamReview review = PracticeExamReview.restore(getIntent().getStringExtra(EXTRA_REVIEW));
        findViewById(R.id.exam_result_home).setOnClickListener(view -> finish());
        findViewById(R.id.exam_result_new).setOnClickListener(view -> {
            if (openingDestination) return;
            openingDestination = true;
            startActivity(new Intent(this, PracticeExamActivity.class));
            finish();
        });
        if (review == null) {
            findViewById(R.id.exam_result_invalid).setVisibility(View.VISIBLE);
            return;
        }
        findViewById(R.id.exam_result_content).setVisibility(View.VISIBLE);
        PracticeExamTextMapper mapper = new PracticeExamTextMapper(this::getString);
        ((TextView) findViewById(R.id.exam_result_score)).setText(mapper.score(review));
        ((TextView) findViewById(R.id.exam_result_percentage)).setText(mapper.percentage(review.getPercentage()));
        ((TextView) findViewById(R.id.exam_result_counts)).setText(
                getString(R.string.practice_result_total, review.getQuestionCount()) + "\n"
                + getString(R.string.practice_result_correct_count, review.getCorrectAnswers()) + "\n"
                + getString(R.string.practice_result_incorrect_count, review.getIncorrectAnswers()));
        LinearLayout cards = findViewById(R.id.exam_result_reviews);
        int number = 0;
        for (PracticeExamReview.Item item : review.getItems()) {
            View card = getLayoutInflater().inflate(R.layout.item_exam_review, cards, false);
            ((TextView) card.findViewById(R.id.exam_review_number)).setText(getString(R.string.practice_question_number, ++number));
            ((TextView) card.findViewById(R.id.exam_review_question)).setText(item.getQuestion().getQuestionText());
            ((TextView) card.findViewById(R.id.exam_review_selected)).setText(mapper.selectedAnswer(item));
            ((TextView) card.findViewById(R.id.exam_review_correct)).setText(mapper.correctAnswer(item));
            TextView badge = card.findViewById(R.id.exam_review_status);
            badge.setText(mapper.resultBadge(item));
            boolean correct = Boolean.TRUE.equals(item.getResult().getIsCorrect());
            badge.setBackgroundTintList(ColorStateList.valueOf(getColor(correct ? R.color.login_teal_soft : R.color.login_error)));
            badge.setTextColor(getColor(correct ? R.color.login_teal : R.color.white));
            TextView explanation = card.findViewById(R.id.exam_review_explanation);
            String text = mapper.explanation(item);
            explanation.setVisibility(text == null ? View.GONE : View.VISIBLE);
            explanation.setText(text);
            cards.addView(card);
        }
    }
}
