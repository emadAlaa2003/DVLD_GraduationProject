package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.PracticeExamReview;
import com.dvld.mobile.repository.PracticeExamRepository.ExamError;
import java.text.DecimalFormat;
import java.text.DecimalFormatSymbols;
import java.util.Locale;

public final class PracticeExamTextMapper {
    private final DashboardTextMapper.Strings strings;
    public PracticeExamTextMapper(DashboardTextMapper.Strings strings) { this.strings = strings; }
    public String questionProgress(int index, int count) {
        return strings.get(R.string.practice_question_progress, index + 1, count);
    }
    public String score(PracticeExamReview review) {
        return strings.get(R.string.practice_result_score, review.getCorrectAnswers(), review.getQuestionCount());
    }
    public String percentage(double value) {
        return strings.get(R.string.practice_result_percentage,
                new DecimalFormat("0.#", DecimalFormatSymbols.getInstance(Locale.ROOT)).format(value));
    }
    public String selectedAnswer(PracticeExamReview.Item item) {
        return strings.get(R.string.practice_selected_answer,
                item.getQuestion().optionText(item.getResult().getSelectedOption()));
    }
    public String correctAnswer(PracticeExamReview.Item item) {
        return strings.get(R.string.practice_correct_answer,
                item.getQuestion().optionText(item.getResult().getCorrectOption()));
    }
    public String resultBadge(PracticeExamReview.Item item) {
        return strings.get(Boolean.TRUE.equals(item.getResult().getIsCorrect())
                ? R.string.practice_answer_correct : R.string.practice_answer_incorrect);
    }
    public String explanation(PracticeExamReview.Item item) {
        String explanation = item.getResult().getExplanation();
        return explanation == null || explanation.trim().isEmpty() ? null
                : strings.get(R.string.practice_explanation, explanation);
    }
    public String error(ExamError error, boolean submitting) {
        switch (error) {
            case NETWORK: return strings.get(R.string.login_network_error);
            case UNAUTHORIZED: return strings.get(R.string.dashboard_session_expired);
            case FORBIDDEN: return strings.get(R.string.dashboard_forbidden);
            case CONFLICT: return strings.get(submitting ? R.string.practice_submit_error : R.string.practice_no_questions);
            case BAD_REQUEST: return strings.get(R.string.practice_invalid_request);
            default: return strings.get(submitting ? R.string.practice_submit_error : R.string.practice_load_error);
        }
    }
}
