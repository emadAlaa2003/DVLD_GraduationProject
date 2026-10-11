package com.dvld.mobile.model;

import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

/** Checks response structure; scoring always comes from the server. */
public final class PracticeExamContract {
    private PracticeExamContract() { }

    public static boolean validQuestions(List<PracticeExamQuestion> questions) {
        if (questions == null || questions.isEmpty() || questions.size() > 20) return false;
        Set<Integer> ids = new HashSet<>();
        for (PracticeExamQuestion question : questions) {
            if (question == null || !question.isValid() || !ids.add(question.getQuestionID())) return false;
        }
        return true;
    }

    public static boolean validStart(PracticeExamStartResponse response, int requestedCount) {
        return requestedCount >= 1 && requestedCount <= 20 && response != null
                && Integer.valueOf(requestedCount).equals(response.getRequestedQuestionCount())
                && validQuestions(response.getQuestions()) && response.getQuestions().size() <= requestedCount
                && Integer.valueOf(response.getQuestions().size()).equals(response.getQuestionCount());
    }

    public static boolean validAnswers(List<PracticeExamAnswer> answers) {
        if (answers == null || answers.isEmpty() || answers.size() > 20) return false;
        Set<Integer> ids = new HashSet<>();
        for (PracticeExamAnswer answer : answers) {
            if (answer == null || answer.getQuestionID() <= 0 || !ids.add(answer.getQuestionID())
                    || !validOption(answer.getSelectedOption())) return false;
        }
        return true;
    }

    public static boolean validSubmit(PracticeExamSubmitResponse response, List<PracticeExamAnswer> answers) {
        if (response == null || !validAnswers(answers)
                || !Integer.valueOf(answers.size()).equals(response.getQuestionCount())
                || !Integer.valueOf(answers.size()).equals(response.getAnsweredCount())
                || response.getCorrectAnswers() == null || response.getCorrectAnswers() < 0
                || response.getCorrectAnswers() > answers.size() || !validPercentage(response.getPercentage())
                || response.getResults() == null || response.getResults().size() != answers.size()) return false;
        Map<Integer, String> selected = new HashMap<>();
        for (PracticeExamAnswer answer : answers) selected.put(answer.getQuestionID(), answer.getSelectedOption());
        for (PracticeExamAnswerResult result : response.getResults()) {
            if (result == null || result.getQuestionID() == null || !validOption(result.getCorrectOption())
                    || !validOption(result.getSelectedOption())
                    || result.getIsCorrect() == null || !validExplanation(result.getExplanation())
                    || !result.getSelectedOption().equals(selected.remove(result.getQuestionID()))) return false;
        }
        return selected.isEmpty();
    }

    public static boolean validOption(String option) {
        return "A".equals(option) || "B".equals(option) || "C".equals(option) || "D".equals(option);
    }
    public static boolean validPercentage(Double percentage) {
        return percentage != null && Double.isFinite(percentage) && percentage >= 0 && percentage <= 100;
    }
    public static boolean validExplanation(String explanation) {
        return explanation == null || explanation.length() <= 2000;
    }
}
