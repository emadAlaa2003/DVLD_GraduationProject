package com.dvld.mobile.ui;

import com.dvld.mobile.model.ChatSource;
import com.dvld.mobile.repository.ChatRepository.ChatError;
import org.junit.Test;
import static org.junit.Assert.*;

public class ChatTextMapperTest extends ArabicResourcesTestSupport {
    private final ChatTextMapper mapper = new ChatTextMapper(arabic::get);
    @Test public void networkError() { assertEquals("تعذر الاتصال بالخادم. حاول مرة أخرى.", mapper.error(ChatError.NETWORK)); }
    @Test public void serverAndMissingError() {
        assertEquals("تعذر الحصول على إجابة حاليًا. حاول مرة أخرى.", mapper.error(ChatError.SERVER));
        assertEquals(mapper.error(ChatError.SERVER), mapper.error(null));
    }
    @Test public void unauthorized() { assertEquals("انتهت جلسة الدخول. يرجى تسجيل الدخول من جديد.", mapper.error(ChatError.UNAUTHORIZED)); }
    @Test public void forbidden() { assertEquals("لا يسمح لهذا الحساب باستخدام المساعد المروري.", mapper.error(ChatError.FORBIDDEN)); }
    @Test public void badRequest() { assertEquals("تعذر إرسال السؤال. تحقق من السؤال وحاول مرة أخرى.", mapper.error(ChatError.BAD_REQUEST)); }
    @Test public void sourceOnlyShowsDocumentAndPage() {
        ChatSource source = gson.fromJson("{\"documentID\":7,\"pageNumber\":101,\"chunkIndex\":147,\"semanticScore\":0.64}", ChatSource.class);
        assertEquals("المستند 7 — الصفحة 101", mapper.source(source));
    }
    @Test public void nullableSourceFields() {
        assertEquals("المستند — — الصفحة —", mapper.source(gson.fromJson("{}", ChatSource.class)));
        assertEquals("المستند 7 — الصفحة —", mapper.source(gson.fromJson("{\"documentID\":7,\"pageNumber\":null}", ChatSource.class)));
    }
    @Test public void invalidSourceNumbersFallBackSafely() {
        assertEquals("المستند — — الصفحة —", mapper.source(gson.fromJson("{\"documentID\":0,\"pageNumber\":-2}", ChatSource.class)));
    }
}
