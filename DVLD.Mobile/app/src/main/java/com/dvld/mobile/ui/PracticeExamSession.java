package com.dvld.mobile.ui;

import com.dvld.mobile.model.*;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import java.util.ArrayList;
import java.util.Collections;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/** Only questions and citizen selections exist here; there is no answer key. */
public final class PracticeExamSession {
    private List<PracticeExamQuestion> questions;
    private Map<Integer, String> answers = new LinkedHashMap<>();
    private int currentIndex;

    public PracticeExamSession(PracticeExamStartResponse response) {
        if (response == null || response.getRequestedQuestionCount() == null
                || !PracticeExamContract.validStart(response, response.getRequestedQuestionCount()))
            throw new IllegalArgumentException("Invalid practice exam");
        questions = new ArrayList<>(response.getQuestions());
    }

    public int getCurrentIndex() { return currentIndex; }
    public int getQuestionCount() { return questions.size(); }
    public PracticeExamQuestion getCurrentQuestion() { return questions.get(currentIndex); }
    public List<PracticeExamQuestion> getQuestions() { return Collections.unmodifiableList(questions); }
    public String getSelectedOption(int questionID) { return answers.get(questionID); }
    public boolean select(String option) {
        PracticeExamQuestion question = getCurrentQuestion();
        if (!question.getOptions().containsKey(option)) return false;
        answers.put(question.getQuestionID(), option);
        return true;
    }
    public boolean next() {
        if (currentIndex == questions.size() - 1) return false;
        currentIndex++;
        return true;
    }
    public boolean previous() {
        if (currentIndex == 0) return false;
        currentIndex--;
        return true;
    }
    public int firstUnanswered() {
        for (int i = 0; i < questions.size(); i++)
            if (!answers.containsKey(questions.get(i).getQuestionID())) return i;
        return -1;
    }
    public boolean goToFirstUnanswered() {
        int index = firstUnanswered();
        if (index < 0) return false;
        currentIndex = index;
        return true;
    }
    public List<PracticeExamAnswer> buildAnswers() {
        if (firstUnanswered() >= 0) return Collections.emptyList();
        List<PracticeExamAnswer> result = new ArrayList<>();
        for (PracticeExamQuestion question : questions)
            result.add(new PracticeExamAnswer(question.getQuestionID(), answers.get(question.getQuestionID())));
        return Collections.unmodifiableList(result);
    }
    public String toJson() { return new GsonBuilder().disableHtmlEscaping().create().toJson(this); }
    public static PracticeExamSession restore(String json) {
        if (json == null) return null;
        try {
            PracticeExamSession session = new Gson().fromJson(json, PracticeExamSession.class);
            return session != null && session.isValid() ? session : null;
        } catch (RuntimeException invalidState) { return null; }
    }
    private boolean isValid() {
        if (!PracticeExamContract.validQuestions(questions) || answers == null
                || currentIndex < 0 || currentIndex >= questions.size()) return false;
        Map<Integer, PracticeExamQuestion> byId = new LinkedHashMap<>();
        for (PracticeExamQuestion question : questions) byId.put(question.getQuestionID(), question);
        for (Map.Entry<Integer, String> answer : answers.entrySet()) {
            PracticeExamQuestion question = byId.get(answer.getKey());
            if (question == null || !question.getOptions().containsKey(answer.getValue())) return false;
        }
        return true;
    }
}
