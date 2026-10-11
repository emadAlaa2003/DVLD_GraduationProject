package com.dvld.mobile.model;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

/** Small self-contained result payload for Intent and recreation, max 20 items. */
public final class PracticeExamReview {
    private Integer questionCount;
    private Integer correctAnswers;
    private Double percentage;
    private List<Item> items;

    public static PracticeExamReview create(List<PracticeExamQuestion> questions,
            List<PracticeExamAnswer> answers, PracticeExamSubmitResponse response) {
        if (!PracticeExamContract.validQuestions(questions) || answers == null || questions.size() != answers.size()
                || !PracticeExamContract.validSubmit(response, answers)) return null;
        Map<Integer, PracticeExamAnswerResult> byId = new HashMap<>();
        for (PracticeExamAnswerResult result : response.getResults()) byId.put(result.getQuestionID(), result);
        PracticeExamReview review = new PracticeExamReview();
        review.questionCount = response.getQuestionCount();
        review.correctAnswers = response.getCorrectAnswers();
        review.percentage = response.getPercentage();
        review.items = new ArrayList<>();
        for (PracticeExamQuestion question : questions) {
            Item item = new Item();
            item.question = question;
            item.result = byId.get(question.getQuestionID());
            review.items.add(item);
        }
        return review.isValid() ? review : null;
    }
    public int getQuestionCount() { return questionCount; }
    public int getCorrectAnswers() { return correctAnswers; }
    public int getIncorrectAnswers() { return questionCount - correctAnswers; }
    public double getPercentage() { return percentage; }
    public List<Item> getItems() { return Collections.unmodifiableList(items); }
    public String toJson() { return new GsonBuilder().disableHtmlEscaping().create().toJson(this); }

    public static PracticeExamReview restore(String json) {
        if (json == null) return null;
        try {
            PracticeExamReview review = new Gson().fromJson(json, PracticeExamReview.class);
            return review != null && review.isValid() ? review : null;
        } catch (RuntimeException invalidState) { return null; }
    }
    private boolean isValid() {
        if (questionCount == null || questionCount < 1 || questionCount > 20 || correctAnswers == null
                || correctAnswers < 0 || correctAnswers > questionCount || !PracticeExamContract.validPercentage(percentage)
                || items == null || items.size() != questionCount) return false;
        Set<Integer> ids = new HashSet<>();
        for (Item item : items) {
            if (item == null || item.question == null || !item.question.isValid() || item.result == null
                    || !ids.add(item.question.getQuestionID())
                    || !item.question.getQuestionID().equals(item.result.getQuestionID())
                    || item.result.getIsCorrect() == null || !PracticeExamContract.validExplanation(item.result.getExplanation())
                    || item.question.optionText(item.result.getSelectedOption()) == null
                    || item.question.optionText(item.result.getCorrectOption()) == null) return false;
        }
        return true;
    }

    public static final class Item {
        private PracticeExamQuestion question;
        private PracticeExamAnswerResult result;
        public PracticeExamQuestion getQuestion() { return question; }
        public PracticeExamAnswerResult getResult() { return result; }
    }
}
