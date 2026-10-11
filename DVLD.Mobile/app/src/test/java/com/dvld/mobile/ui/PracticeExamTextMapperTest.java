package com.dvld.mobile.ui;

import com.dvld.mobile.PracticeExamFixtures;
import com.dvld.mobile.model.*;
import com.dvld.mobile.repository.PracticeExamRepository;
import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import org.junit.Test;
import static org.junit.Assert.*;

public class PracticeExamTextMapperTest extends ArabicResourcesTestSupport {
    private final PracticeExamTextMapper mapper = new PracticeExamTextMapper(arabic);
    private PracticeExamSession completeSession() {
        PracticeExamSession session = new PracticeExamSession(gson.fromJson(
                PracticeExamFixtures.startJson(10, 2), PracticeExamStartResponse.class));
        session.select("A");
        session.next();
        session.select("B");
        return session;
    }
    private PracticeExamReview review(String results) {
        PracticeExamSession session = completeSession();
        return PracticeExamReview.create(session.getQuestions(), session.buildAnswers(),
                gson.fromJson(results, PracticeExamSubmitResponse.class));
    }
    @Test public void progressAndSummaryUseActualBackendCountsAndPercentage() {
        PracticeExamReview review = review(PracticeExamFixtures.submitJson());
        assertEquals("السؤال 1 من 2", mapper.questionProgress(0, 2));
        assertEquals("السؤال 2 من 2", mapper.questionProgress(1, 2));
        assertEquals("1 / 2", mapper.score(review));
        assertEquals("50%", mapper.percentage(review.getPercentage()));
        assertEquals("33.3%", mapper.percentage(100.0 / 3));
        assertEquals(1, review.getCorrectAnswers());
        assertEquals(1, review.getIncorrectAnswers());
    }
    @Test public void mapsSelectedAndCorrectOptionsToTheirFullTextAfterSubmit() {
        PracticeExamReview review = review(PracticeExamFixtures.submitJson());
        assertEquals("إجابتك: الخيار الأول", mapper.selectedAnswer(review.getItems().get(0)));
        assertEquals("الإجابة الصحيحة: الخيار الثالث", mapper.correctAnswer(review.getItems().get(0)));
        assertEquals("إجابتك: خطأ", mapper.selectedAnswer(review.getItems().get(1)));
        assertEquals("الإجابة الصحيحة: خطأ", mapper.correctAnswer(review.getItems().get(1)));
    }
    @Test public void correctAndIncorrectBadgesUseServerResultFlags() {
        PracticeExamReview review = review(PracticeExamFixtures.submitJson());
        assertEquals("خاطئة", mapper.resultBadge(review.getItems().get(0)));
        assertEquals("صحيحة", mapper.resultBadge(review.getItems().get(1)));
        // The client displays the returned flag, rather than grading locally.
        JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
        body.getAsJsonArray("results").get(0).getAsJsonObject().addProperty("isCorrect", true);
        assertEquals("صحيحة", mapper.resultBadge(review(body.toString()).getItems().get(0)));
    }
    @Test public void explanationsAreOptionalAndNeverProduceAnEmptySection() {
        PracticeExamReview review = review(PracticeExamFixtures.submitJson());
        assertEquals("الشرح: شرح السؤال", mapper.explanation(review.getItems().get(0)));
        assertNull(mapper.explanation(review.getItems().get(1)));
        for (String explanation : new String[] {"", " "}) {
            JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
            body.getAsJsonArray("results").get(0).getAsJsonObject().addProperty("explanation", explanation);
            assertNull(mapper.explanation(review(body.toString()).getItems().get(0)));
        }
    }
    @Test public void resultsAreMergedByIdAndKeptInOriginalQuestionOrder() {
        JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
        JsonArray original = body.getAsJsonArray("results");
        JsonArray reversed = new JsonArray();
        reversed.add(original.get(1));
        reversed.add(original.get(0));
        body.add("results", reversed);
        PracticeExamReview review = review(body.toString());
        assertEquals("سؤال 1", review.getItems().get(0).getQuestion().getQuestionText());
        assertEquals(Integer.valueOf(1051), review.getItems().get(0).getResult().getQuestionID());
        assertEquals("خاطئة", mapper.resultBadge(review.getItems().get(0)));
    }
    @Test public void reviewRoundTripRetainsEverythingForActivityRecreation() {
        PracticeExamReview original = review(PracticeExamFixtures.submitJson());
        PracticeExamReview restored = PracticeExamReview.restore(original.toJson());
        assertNotNull(restored);
        assertEquals(original.getQuestionCount(), restored.getQuestionCount());
        assertEquals(original.getPercentage(), restored.getPercentage(), 0.0);
        assertEquals("سؤال 1", restored.getItems().get(0).getQuestion().getQuestionText());
        assertEquals(mapper.correctAnswer(original.getItems().get(0)), mapper.correctAnswer(restored.getItems().get(0)));
        assertEquals("الشرح: شرح السؤال", mapper.explanation(restored.getItems().get(0)));
    }
    @Test public void invalidNullableReviewsAreRejectedInsteadOfCrashing() {
        for (String json : new String[] {null, "", "bad", "null", "{}", "[]"}) assertNull(PracticeExamReview.restore(json));
        PracticeExamSession session = completeSession();
        assertNull(PracticeExamReview.create(session.getQuestions(), null, null));
        assertNull(PracticeExamReview.create(session.getQuestions(), session.buildAnswers(), null));
        JsonObject body = JsonParser.parseString(PracticeExamFixtures.submitJson()).getAsJsonObject();
        body.getAsJsonArray("results").get(1).getAsJsonObject().addProperty("correctOption", "C");
        assertNull(review(body.toString())); // No C text exists on a TrueFalse question.
        JsonObject saved = JsonParser.parseString(review(PracticeExamFixtures.submitJson()).toJson()).getAsJsonObject();
        saved.add("items", null);
        assertNull(PracticeExamReview.restore(saved.toString()));
    }
    @Test public void errorMessagesCoverAllNetworkAndHttpStates() {
        assertEquals("لا توجد أسئلة تجريبية معتمدة متاحة حاليًا",
                mapper.error(PracticeExamRepository.ExamError.CONFLICT, false));
        assertEquals("تعذر تصحيح الاختبار، حاول مرة أخرى",
                mapper.error(PracticeExamRepository.ExamError.CONFLICT, true));
        assertEquals("تعذر الاتصال بالخادم، تحقق من الاتصال وحاول مرة أخرى",
                mapper.error(PracticeExamRepository.ExamError.NETWORK, false));
        assertEquals("انتهت جلسة الدخول، يرجى تسجيل الدخول من جديد",
                mapper.error(PracticeExamRepository.ExamError.UNAUTHORIZED, false));
        assertEquals("لا يمكن الوصول إلى هذه البيانات",
                mapper.error(PracticeExamRepository.ExamError.FORBIDDEN, false));
        assertEquals("تعذر معالجة طلب الاختبار، حاول مرة أخرى",
                mapper.error(PracticeExamRepository.ExamError.BAD_REQUEST, true));
        assertEquals("تعذر تحميل الاختبار، حاول مرة أخرى",
                mapper.error(PracticeExamRepository.ExamError.SERVER, false));
    }
    @Test public void maximumTwentyQuestionPayloadStaysSmallAndRestorable() {
        JsonObject start = JsonParser.parseString(PracticeExamFixtures.startJson(20, 20)).getAsJsonObject();
        for (com.google.gson.JsonElement element : start.getAsJsonArray("questions")) {
            JsonObject question = element.getAsJsonObject();
            question.addProperty("questionType", "MultipleChoice");
            question.addProperty("questionText", "<".repeat(1000));
            for (String option : new String[] {"optionA", "optionB", "optionC", "optionD"})
                question.addProperty(option, "\"".repeat(500));
        }
        PracticeExamSession session = new PracticeExamSession(gson.fromJson(start, PracticeExamStartResponse.class));
        JsonObject submit = new JsonObject();
        submit.addProperty("questionCount", 20);
        submit.addProperty("answeredCount", 20);
        submit.addProperty("correctAnswers", 20);
        submit.addProperty("percentage", 100);
        JsonArray results = new JsonArray();
        for (int index = 0; index < 20; index++) {
            session.select("A");
            JsonObject result = new JsonObject();
            result.addProperty("questionID", session.getCurrentQuestion().getQuestionID());
            result.addProperty("selectedOption", "A");
            result.addProperty("correctOption", "A");
            result.addProperty("isCorrect", true);
            result.addProperty("explanation", "<".repeat(2000));
            results.add(result);
            session.next();
        }
        submit.add("results", results);
        PracticeExamReview review = PracticeExamReview.create(session.getQuestions(), session.buildAnswers(),
                gson.fromJson(submit, PracticeExamSubmitResponse.class));
        assertNotNull(review);
        assertTrue(review.toJson().length() * 2 < 512 * 1024);
        assertNotNull(PracticeExamReview.restore(review.toJson()));
        assertNotNull(PracticeExamSession.restore(session.toJson()));
    }
}
