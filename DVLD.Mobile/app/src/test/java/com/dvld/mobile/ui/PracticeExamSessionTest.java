package com.dvld.mobile.ui;

import com.dvld.mobile.PracticeExamFixtures;
import com.dvld.mobile.model.*;
import com.google.gson.Gson;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import org.junit.Test;
import java.util.List;
import static org.junit.Assert.*;

public class PracticeExamSessionTest {
    private final Gson gson = new Gson();
    private PracticeExamSession session(int count) {
        return new PracticeExamSession(gson.fromJson(PracticeExamFixtures.startJson(10, count), PracticeExamStartResponse.class));
    }
    @Test public void navigatesAllTenQuestionsWithBoundedPreviousAndNext() {
        PracticeExamSession session = session(10);
        assertEquals(10, session.getQuestionCount());
        assertEquals(0, session.getCurrentIndex());
        assertFalse(session.previous());
        for (int i = 0; i < 9; i++) {
            assertEquals(Integer.valueOf(1051 + i), session.getCurrentQuestion().getQuestionID());
            assertTrue(session.next());
        }
        assertEquals(9, session.getCurrentIndex());
        assertFalse(session.next());
        for (int i = 9; i > 0; i--) assertTrue(session.previous());
        assertEquals(0, session.getCurrentIndex());
    }
    @Test public void selectionsPersistByQuestionIdWhenMovingBackAndForth() {
        PracticeExamSession session = session(10);
        assertTrue(session.select("C"));
        session.next();
        assertTrue(session.select("B"));
        session.previous();
        assertEquals("C", session.getSelectedOption(1051));
        session.next();
        assertEquals("B", session.getSelectedOption(1052));
        session.select("A");
        assertEquals("A", session.getSelectedOption(1052));
        assertEquals("C", session.getSelectedOption(1051));
    }
    @Test public void trueFalseOnlyOffersAAndBAndCannotSelectCOrD() {
        PracticeExamSession session = session(2);
        session.next();
        assertEquals(2, session.getCurrentQuestion().getOptions().size());
        assertEquals("صح", session.getCurrentQuestion().optionText("A"));
        assertEquals("خطأ", session.getCurrentQuestion().optionText("B"));
        assertFalse(session.select("C"));
        assertFalse(session.select("D"));
        assertNull(session.getSelectedOption(1052));
    }
    @Test public void blankOrNullOptionsAreOmittedAndTrueFalseNeverShowsExtraOptions() {
        JsonObject start = JsonParser.parseString(PracticeExamFixtures.startJson(10, 2)).getAsJsonObject();
        JsonObject mc = start.getAsJsonArray("questions").get(0).getAsJsonObject();
        mc.addProperty("optionC", " ");
        mc.add("optionD", null);
        PracticeExamQuestion question = gson.fromJson(mc, PracticeExamQuestion.class);
        assertEquals(2, question.getOptions().size());
        assertNull(question.optionText("C"));
        JsonObject tf = start.getAsJsonArray("questions").get(1).getAsJsonObject();
        tf.addProperty("optionC", "Unused");
        tf.addProperty("optionD", "Unused");
        assertEquals(2, gson.fromJson(tf, PracticeExamQuestion.class).getOptions().size());
    }
    @Test public void incompleteExamCannotBuildPartialSubmitAnswers() {
        PracticeExamSession session = session(10);
        session.select("A");
        assertEquals(1, session.firstUnanswered());
        assertTrue(session.buildAnswers().isEmpty());
    }
    @Test public void movesToFirstUnansweredIncludingAnEarlierSkippedQuestion() {
        PracticeExamSession session = session(10);
        session.next();
        session.select("B");
        for (int i = 1; i < 9; i++) session.next();
        assertTrue(session.goToFirstUnanswered());
        assertEquals(0, session.getCurrentIndex());
        session.select("A");
        assertEquals(2, session.firstUnanswered());
        assertTrue(session.goToFirstUnanswered());
        assertEquals(2, session.getCurrentIndex());
    }
    @Test public void completeSubmitAnswersHaveEveryRealQuestionIdAndOnlySelectedOptions() {
        PracticeExamSession session = session(10);
        for (int i = 0; i < 10; i++) { session.select("A"); session.next(); }
        assertEquals(-1, session.firstUnanswered());
        assertFalse(session.goToFirstUnanswered());
        List<PracticeExamAnswer> answers = session.buildAnswers();
        assertEquals(10, answers.size());
        for (int i = 0; i < 10; i++) {
            assertEquals(1051 + i, answers.get(i).getQuestionID());
            assertEquals("A", answers.get(i).getSelectedOption());
        }
        JsonObject request = JsonParser.parseString(gson.toJson(new PracticeExamSubmitRequest(answers))).getAsJsonObject();
        for (com.google.gson.JsonElement answer : request.getAsJsonArray("answers")) {
            assertEquals(2, answer.getAsJsonObject().size());
            assertFalse(answer.getAsJsonObject().has("correctOption"));
        }
    }
    @Test public void fewerQuestionsUseActualCountAndHaveNoInventedQuestions() {
        PracticeExamSession session = session(2);
        assertEquals(2, session.getQuestionCount());
        session.select("A");
        session.next();
        session.select("B");
        assertFalse(session.next());
        assertEquals(2, session.buildAnswers().size());
    }
    @Test public void stateRestorationKeepsQuestionsCurrentIndexAndChoices() {
        PracticeExamSession original = session(10);
        original.select("D");
        original.next();
        original.select("B");
        original.next();
        PracticeExamSession restored = PracticeExamSession.restore(original.toJson());
        assertNotNull(restored);
        assertEquals(10, restored.getQuestionCount());
        assertEquals(2, restored.getCurrentIndex());
        assertEquals("D", restored.getSelectedOption(1051));
        assertEquals("B", restored.getSelectedOption(1052));
        assertEquals("سؤال 3", restored.getCurrentQuestion().getQuestionText());
        assertEquals(2, restored.firstUnanswered());
    }
    @Test public void badSavedStateIsRejectedSafely() {
        for (String json : new String[] {null, "null", "", "bad", "{}", "[]"})
            assertNull(PracticeExamSession.restore(json));
        JsonObject state = JsonParser.parseString(session(2).toJson()).getAsJsonObject();
        state.addProperty("currentIndex", 5);
        assertNull(PracticeExamSession.restore(state.toString()));
        state.addProperty("currentIndex", 0);
        state.getAsJsonObject("answers").addProperty("999", "A");
        assertNull(PracticeExamSession.restore(state.toString()));
    }
    @Test public void wrongTrueFalseChoiceInSavedStateIsRejected() {
        PracticeExamSession session = session(2);
        session.next();
        session.select("B");
        JsonObject state = JsonParser.parseString(session.toJson()).getAsJsonObject();
        state.getAsJsonObject("answers").addProperty("1052", "D");
        assertNull(PracticeExamSession.restore(state.toString()));
    }
    @Test public void startAndSavedSessionHaveNoAnswerKeyEvenIfUnexpectedFieldsArrive() {
        JsonObject start = JsonParser.parseString(PracticeExamFixtures.startJson(10, 2)).getAsJsonObject();
        JsonObject question = start.getAsJsonArray("questions").get(0).getAsJsonObject();
        question.addProperty("correctOption", "C");
        question.addProperty("explanation", "Not allowed before submit");
        PracticeExamSession session = new PracticeExamSession(gson.fromJson(start, PracticeExamStartResponse.class));
        String saved = session.toJson();
        assertFalse(saved.contains("correctOption"));
        assertFalse(saved.contains("explanation"));
        assertFalse(saved.contains("Not allowed before submit"));
    }
}
