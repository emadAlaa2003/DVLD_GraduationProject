package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.model.DashboardData;
import com.google.gson.Gson;

import org.junit.BeforeClass;
import org.junit.Test;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.io.File;
import java.time.LocalDate;
import java.time.ZoneId;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;

import javax.xml.parsers.DocumentBuilderFactory;

import static org.junit.Assert.*;

public class ApplicationsTextMapperTest {
    private static final Map<Integer, String> strings = new HashMap<>();
    private final Gson gson = new Gson();
    private final ApplicationsTextMapper mapper = new ApplicationsTextMapper(
            (id, arguments) -> String.format(Locale.ROOT, strings.get(id), arguments), ZoneId.of("Asia/Hebron"));

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
    public void allSevenTypesReuseArabicMappingsAndIdsTakePriority() {
        String[] expected = {"إصدار رخصة قيادة جديدة", "تجديد رخصة قيادة", "بدل فاقد لرخصة قيادة",
                "بدل تالف لرخصة قيادة", "فك حجز رخصة قيادة", "إصدار رخصة دولية جديدة", "إعادة اختبار"};
        for (int id = 1; id <= 7; id++) {
            assertEquals(expected[id - 1], mapper.applicationType(application(
                    "{\"applicationTypeID\":" + id + ",\"applicationTypeName\":\"DifferentBackendName\"}")));
        }
    }

    @Test
    public void unknownTypesUseBackendTextOrDashAndKnownNamesAreTranslated() {
        assertEquals("FutureType", mapper.applicationType(application(
                "{\"applicationTypeID\":99,\"applicationTypeName\":\"FutureType\"}")));
        assertEquals("إعادة اختبار", mapper.applicationType(application("{\"applicationTypeName\":\"RetakeTest\"}")));
        assertEquals("—", mapper.applicationType(application("{}")));
        assertEquals("—", mapper.applicationType(application("{\"applicationTypeID\":99,\"applicationTypeName\":\"  \"}")));
    }

    @Test
    public void numericStatusesHaveDistinctStatesAndArabicLabels() {
        String[] expected = {"جديد", "ملغي", "مكتمل"};
        ApplicationsTextMapper.Status[] states = {ApplicationsTextMapper.Status.NEW,
                ApplicationsTextMapper.Status.CANCELLED, ApplicationsTextMapper.Status.COMPLETED};
        for (int id = 1; id <= 3; id++) {
            CitizenApplication application = application("{\"applicationStatus\":" + id + "}");
            assertEquals(expected[id - 1], mapper.statusText(application));
            assertEquals(states[id - 1], mapper.status(application));
        }
    }

    @Test
    public void statusTextFallbackHandlesUnknownIdsWhitespaceAndCase() {
        String[] values = {" new ", "CANCELLED", "Completed"};
        String[] expected = {"جديد", "ملغي", "مكتمل"};
        for (int i = 0; i < values.length; i++) {
            assertEquals(expected[i], mapper.statusText(application(
                    "{\"applicationStatus\":99,\"statusText\":\"" + values[i] + "\"}")));
            assertEquals(expected[i], mapper.statusText(application("{\"statusText\":\"" + values[i] + "\"}")));
        }
        assertEquals("جديد", mapper.statusText(application("{\"applicationStatus\":1,\"statusText\":\"Completed\"}")));
    }

    @Test
    public void nullAndUnrecognizedStatusesStayUnknown() {
        for (String body : new String[] {"{}", "{\"applicationStatus\":99}",
                "{\"statusText\":\"FutureStatus\"}", "{\"statusText\":\" \"}"}) {
            assertEquals("—", mapper.statusText(application(body)));
            assertEquals(ApplicationsTextMapper.Status.UNKNOWN, mapper.status(application(body)));
        }
    }

    @Test
    public void validDatesUseArabicFormattingAndPhoneZone() {
        String date = mapper.date("2026-10-08T23:30:00Z");
        assertEquals(date, mapper.date("2026-10-09T02:30:00+03:00"));
        assertEquals(date, mapper.date("2026-10-09T02:30:00"));
        assertEquals(date, mapper.date("2026-10-09"));
        assertTrue(date.contains("2026"));
        // Arabic medium dates can use numeric months; do not require one regional month name.
        String englishDate = DateTimeFormatter.ofLocalizedDate(FormatStyle.MEDIUM)
                .withLocale(Locale.ENGLISH).format(LocalDate.of(2026, 10, 9));
        assertNotEquals(englishDate, date);
        assertFalse(date.contains("T23:30"));
        String details = mapper.details(application("{\"applicationDate\":\"2026-10-09\",\"lastStatusDate\":\"2026-10-09\"}"));
        assertTrue(details.startsWith("تاريخ الطلب: " + date + "\nآخر تحديث للحالة: " + date + "\n"));
    }

    @Test
    public void invalidAndNullDatesShowDash() {
        for (String value : new String[] {null, "", " ", "invalid", "2026-02-30"}) {
            assertEquals("—", mapper.date(value));
        }
    }

    @Test
    public void feesHaveTwoDecimalPlacesWithoutInventedCurrency() {
        assertEquals("15.00", mapper.fees(15.0));
        assertEquals("15.25", mapper.fees(15.25));
        assertEquals("0.00", mapper.fees(0.0));
        for (Double value : new Double[] {null, -1.0, Double.NaN, Double.POSITIVE_INFINITY, Double.NEGATIVE_INFINITY}) {
            assertEquals("—", mapper.fees(value));
        }
        assertTrue(mapper.details(application("{\"paidFees\":15.25}")).endsWith("الرسوم المدفوعة: 15.25"));
    }

    @Test
    public void nullableClassIsOmittedAndOtherMissingFieldsShowDash() {
        for (String body : new String[] {"{}", "{\"className\":null}", "{\"className\":\"  \"}"}) {
            assertEquals("تاريخ الطلب: —\nآخر تحديث للحالة: —\nالرسوم المدفوعة: —", mapper.details(application(body)));
        }
        assertEquals("رقم الطلب: —", mapper.applicationId(application("{}")));
        assertEquals("رقم الطلب: —", mapper.applicationId(application("{\"applicationID\":0}")));
        assertEquals("رقم الطلب: 8", mapper.applicationId(application("{\"applicationID\":8}")));
    }

    @Test
    public void allKnownLicenseClassesReuseArabicMappingsAndUnknownClassesRemainOriginal() {
        String[] backend = {"Class 1 - Small Motorcycle", "Class 2 - Heavy Motorcycle License",
                "Class 3 - Ordinary driving license", "Class 4 - Commercial", "Class 5 - Agricultural",
                "Class 6 - Small and medium bus", "Class 7 - Truck and heavy vehicle"};
        String[] expected = {"الفئة الأولى - رخصة دراجة نارية صغيرة", "الفئة الثانية - رخصة دراجة نارية ثقيلة",
                "الفئة الثالثة - رخصة قيادة عادية", "الفئة الرابعة - رخصة قيادة تجارية",
                "الفئة الخامسة - رخصة مركبات زراعية", "الفئة السادسة - رخصة حافلات صغيرة ومتوسطة",
                "الفئة السابعة - رخصة شاحنات ومركبات ثقيلة"};
        for (int i = 0; i < backend.length; i++) {
            assertTrue(mapper.details(application("{\"className\":\"" + backend[i] + "\"}"))
                    .endsWith("الفئة: " + expected[i]));
        }
        assertTrue(mapper.details(application("{\"className\":\"FutureClass\"}")).endsWith("الفئة: FutureClass"));
    }

    @Test
    public void sortsNewestFirstByApplicationDateWithoutChangingApiList() {
        CitizenApplication older = application("{\"applicationID\":8,\"applicationDate\":\"2026-10-08\"}");
        CitizenApplication newest = application("{\"applicationID\":9,\"applicationDate\":\"2026-10-10T09:00:00Z\"}");
        CitizenApplication middle = application("{\"applicationID\":10,\"applicationDate\":\"2026-10-09\"}");
        List<CitizenApplication> apiOrder = Arrays.asList(older, newest, middle);
        assertEquals(Arrays.asList(newest, middle, older), mapper.sortedApplications(apiOrder));
        assertEquals(Arrays.asList(older, newest, middle), apiOrder);
    }

    @Test
    public void sortComparesInstantsRatherThanBackendDateText() {
        CitizenApplication later = application("{\"applicationDate\":\"2026-10-09T00:30:00Z\"}");
        CitizenApplication earlier = application("{\"applicationDate\":\"2026-10-09T02:30:00+03:00\"}");
        assertEquals(Arrays.asList(later, earlier), mapper.sortedApplications(Arrays.asList(earlier, later)));
    }

    @Test
    public void invalidDatesFollowValidDatesWithStableOrderAndNullEntriesAreSkipped() {
        CitizenApplication invalid = application("{\"applicationDate\":\"invalid\"}");
        CitizenApplication missing = application("{}");
        CitizenApplication valid = application("{\"applicationDate\":\"2026-10-09\"}");
        assertEquals(Arrays.asList(valid, invalid, missing),
                mapper.sortedApplications(Arrays.asList(invalid, null, missing, valid)));
        assertTrue(mapper.sortedApplications(null).isEmpty());
        assertEquals("لا توجد طلبات مسجلة", strings.get(R.string.applications_empty));
    }

    private CitizenApplication application(String json) {
        return gson.fromJson(json, CitizenApplication.class);
    }

    @Test
    public void activeFilterExactlyMatchesDashboardNumericCountWithoutStatusTextFallback() {
        CitizenApplication active = application("{\"applicationStatus\":1,\"applicationDate\":\"2026-10-09\"}");
        CitizenApplication completed = application("{\"applicationStatus\":3}");
        CitizenApplication cancelled = application("{\"applicationStatus\":2}");
        CitizenApplication namedNewOnly = application("{\"statusText\":\"New\"}");
        CitizenApplication unknown = application("{\"applicationStatus\":99,\"statusText\":\"New\"}");
        List<CitizenApplication> applications = Arrays.asList(active, completed, cancelled, namedNewOnly, unknown, null);
        DashboardData data = new DashboardData(Collections.emptyList(), Collections.emptyList(), applications, Collections.emptyList());
        assertEquals(Collections.singletonList(active), mapper.visibleApplications(applications, true));
        assertEquals(data.getActiveApplicationCount(), mapper.visibleApplications(applications, true).size());
    }

    @Test
    public void allModeRetainsAllApplicationsAndEmptyActiveFilterIsSafe() {
        List<CitizenApplication> applications = Arrays.asList(application("{\"applicationStatus\":1}"),
                application("{\"applicationStatus\":2}"), application("{\"applicationStatus\":3}"), application("{}"));
        assertEquals(mapper.sortedApplications(applications), mapper.visibleApplications(applications, false));
        assertTrue(mapper.visibleApplications(Collections.singletonList(application("{\"applicationStatus\":3}")), true).isEmpty());
        assertTrue(mapper.visibleApplications(null, true).isEmpty());
    }
}
