package com.dvld.mobile;

import com.dvld.mobile.model.PracticeExamAnswer;
import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import java.util.Arrays;
import java.util.List;

public final class PracticeExamFixtures {
    private PracticeExamFixtures() { }
    public static String startJson(int requested, int count) {
        JsonObject body = new JsonObject();
        body.addProperty("requestedQuestionCount", requested);
        body.addProperty("questionCount", count);
        JsonArray questions = new JsonArray();
        for (int i = 0; i < count; i++) {
            JsonObject question = new JsonObject();
            question.addProperty("questionID", 1051 + i);
            question.addProperty("questionType", i % 2 == 0 ? "MultipleChoice" : "TrueFalse");
            question.addProperty("questionText", "سؤال " + (i + 1));
            question.addProperty("optionA", i % 2 == 0 ? "الخيار الأول" : "صح");
            question.addProperty("optionB", i % 2 == 0 ? "الخيار الثاني" : "خطأ");
            if (i % 2 == 0) {
                question.addProperty("optionC", "الخيار الثالث");
                question.addProperty("optionD", "الخيار الرابع");
            }
            questions.add(question);
        }
        body.add("questions", questions);
        return body.toString();
    }
    public static List<PracticeExamAnswer> answers() {
        return Arrays.asList(new PracticeExamAnswer(1051, "A"), new PracticeExamAnswer(1052, "B"));
    }
    public static String submitJson() {
        return "{\"questionCount\":2,\"answeredCount\":2,\"correctAnswers\":1,\"percentage\":50.0,\"results\":["
                + "{\"questionID\":1051,\"selectedOption\":\"A\",\"correctOption\":\"C\",\"isCorrect\":false,\"explanation\":\"شرح السؤال\"},"
                + "{\"questionID\":1052,\"selectedOption\":\"B\",\"correctOption\":\"B\",\"isCorrect\":true,\"explanation\":null}]}";
    }
}
