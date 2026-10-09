package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.InternationalLicense;
import com.dvld.mobile.model.LocalLicense;
import com.google.gson.Gson;

import org.junit.BeforeClass;
import org.junit.Test;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.io.File;
import java.time.ZoneId;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

import javax.xml.parsers.DocumentBuilderFactory;

import static org.junit.Assert.*;

public class LicenseCardTextMapperTest {
    private static final Map<Integer, String> strings = new HashMap<>();
    private final Gson gson = new Gson();
    private final LicenseCardTextMapper mapper = new LicenseCardTextMapper((id, arguments) ->
            String.format(Locale.ROOT, strings.get(id), arguments), ZoneId.of("Asia/Hebron"));

    @BeforeClass
    public static void readActualArabicResources() throws Exception {
        NodeList nodes = DocumentBuilderFactory.newInstance().newDocumentBuilder()
                .parse(new File("src/main/res/values/strings.xml")).getElementsByTagName("string");
        for (int i = 0; i < nodes.getLength(); i++) {
            Node node = nodes.item(i);
            strings.put(R.string.class.getField(node.getAttributes().getNamedItem("name").getNodeValue())
                    .getInt(null), node.getTextContent());
        }
    }

    @Test
    public void localCardReusesArabicClassMappingAndDisplaysRealIdAndStatuses() {
        LocalLicense license = gson.fromJson("{\"licenseID\":7,\"className\":\"Class 3 - Ordinary driving license\","
                + "\"isActive\":true,\"isDetained\":false}", LocalLicense.class);
        assertEquals("الفئة الثالثة - رخصة قيادة عادية", mapper.localTitle(license));
        String details = mapper.localDetails(license);
        assertTrue(details.contains("رقم الرخصة: 7"));
        assertTrue(details.contains("حالة السريان: سارية"));
        assertTrue(details.contains("حالة الحجز: غير محجوزة"));
        assertEquals("Class 3 - Ordinary driving license", license.getClassName());
    }

    @Test
    public void localCardRendersInactiveAndDetainedFromResponseRatherThanAssumingActive() {
        LocalLicense license = gson.fromJson("{\"isActive\":false,\"isDetained\":true}", LocalLicense.class);
        assertTrue(mapper.localDetails(license).contains("حالة السريان: غير سارية"));
        assertTrue(mapper.localDetails(license).contains("حالة الحجز: محجوزة"));
    }

    @Test
    public void allNullableLocalFieldsUseUnknownWithoutInventingStatusOrClass() {
        LocalLicense license = gson.fromJson("{}", LocalLicense.class);
        assertEquals("رخصة محلية", mapper.localTitle(license));
        assertEquals("رقم الرخصة: —\nتاريخ الإصدار: —\nتاريخ الانتهاء: —\nحالة السريان: —\nحالة الحجز: —",
                mapper.localDetails(license));
    }

    @Test
    public void unknownClassIsPreservedAndBlankClassUsesNeutralTitle() {
        assertEquals("Future Class", mapper.localTitle(gson.fromJson(
                "{\"className\":\"Future Class\"}", LocalLicense.class)));
        assertEquals("رخصة محلية", mapper.localTitle(gson.fromJson(
                "{\"className\":\"  \"}", LocalLicense.class)));
    }

    @Test
    public void validDatesAreReadableAndInvalidDatesAreUnknown() {
        LocalLicense license = gson.fromJson("{\"issueDate\":\"2026-10-09T13:00:00+03:00\","
                + "\"expirationDate\":\"invalid\"}", LocalLicense.class);
        String details = mapper.localDetails(license);
        assertTrue(details.contains("تاريخ الإصدار:"));
        assertTrue(details.contains("2026"));
        assertFalse(details.contains("T13:00"));
        assertFalse(details.contains("+03:00"));
        assertTrue(details.contains("تاريخ الانتهاء: —"));
    }

    @Test
    public void internationalCardShowsRealIdsAndStatusWithoutInventingClassOrDetention() {
        InternationalLicense license = gson.fromJson("{\"internationalLicenseID\":9,"
                + "\"issuedUsingLocalLicenseID\":7,\"isActive\":true}", InternationalLicense.class);
        String details = mapper.internationalDetails(license);
        assertTrue(details.contains("رقم الرخصة الدولية: 9"));
        assertTrue(details.contains("رقم الرخصة المحلية المستخدمة للإصدار: 7"));
        assertTrue(details.contains("حالة السريان: سارية"));
        assertFalse(details.contains("الفئة"));
        assertFalse(details.contains("حالة الحجز"));
        assertTrue(mapper.internationalDetails(gson.fromJson("{\"isActive\":false}", InternationalLicense.class))
                .contains("حالة السريان: غير سارية"));
    }

    @Test
    public void allNullableInternationalFieldsUseUnknownWithoutInventedData() {
        assertEquals("رقم الرخصة الدولية: —\nرقم الرخصة المحلية المستخدمة للإصدار: —\nتاريخ الإصدار: —\nتاريخ الانتهاء: —\nحالة السريان: —",
                mapper.internationalDetails(gson.fromJson("{}", InternationalLicense.class)));
    }
}
