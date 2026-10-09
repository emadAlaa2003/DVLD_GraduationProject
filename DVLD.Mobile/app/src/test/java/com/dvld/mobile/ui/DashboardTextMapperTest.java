package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.LocalLicense;
import com.google.gson.Gson;

import org.junit.BeforeClass;
import org.junit.Test;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.io.File;
import java.time.ZoneId;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

import javax.xml.parsers.DocumentBuilderFactory;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;

public class DashboardTextMapperTest {
    private static final Map<Integer, String> strings = new HashMap<>();
    private final DashboardTextMapper mapper = new DashboardTextMapper((id, arguments) ->
            String.format(Locale.ROOT, strings.get(id), arguments));
    private final Gson gson = new Gson();
    private final ZoneId zone = ZoneId.of("Asia/Hebron");

    @BeforeClass
    public static void readActualArabicResources() throws Exception {
        // Verify the shipped Arabic text without adding an Android runtime dependency.
        NodeList nodes = DocumentBuilderFactory.newInstance().newDocumentBuilder()
                .parse(new File("src/main/res/values/strings.xml")).getElementsByTagName("string");
        for (int i = 0; i < nodes.getLength(); i++) {
            Node node = nodes.item(i);
            String name = node.getAttributes().getNamedItem("name").getNodeValue();
            int id = R.string.class.getField(name).getInt(null);
            strings.put(id, node.getTextContent());
        }
    }

    @Test
    public void allConfirmedLicenseClassNamesDisplayInArabic() {
        String[] names = {"Class 1 - Small Motorcycle", "Class 2 - Heavy Motorcycle License",
                "Class 3 - Ordinary driving license", "Class 4 - Commercial", "Class 5 - Agricultural",
                "Class 6 - Small and medium bus", "Class 7 - Truck and heavy vehicle"};
        String[] expected = {"الفئة الأولى - رخصة دراجة نارية صغيرة", "الفئة الثانية - رخصة دراجة نارية ثقيلة",
                "الفئة الثالثة - رخصة قيادة عادية", "الفئة الرابعة - رخصة قيادة تجارية",
                "الفئة الخامسة - رخصة مركبات زراعية", "الفئة السادسة - رخصة حافلات صغيرة ومتوسطة",
                "الفئة السابعة - رخصة شاحنات ومركبات ثقيلة"};
        for (int i = 0; i < names.length; i++) {
            assertEquals(expected[i], mapper.licenseClass(names[i]));
        }
    }

    @Test
    public void unknownAndNullableLicenseClassNamesArePreserved() {
        assertEquals("  Future license class  ", mapper.licenseClass("  Future license class  "));
        assertEquals("", mapper.licenseClass(""));
        assertNull(mapper.licenseClass(null));
    }

    @Test
    public void zeroLocalLicensesIsAnEmptyMessage() {
        assertEquals("لا توجد رخص محلية", mapper.localLicenseSummary(Collections.emptyList(), zone));
    }

    @Test
    public void oneLocalLicenseShowsSingularAndArabicClassEvenWithoutIssueDate() {
        assertEquals("لديك رخصة محلية واحدة\nالفئة الثالثة - رخصة قيادة عادية",
                mapper.localLicenseSummary(Collections.singletonList(
                        license("Class 3 - Ordinary driving license", null)), zone));
    }

    @Test
    public void oneLocalLicenseWithNullableClassShowsOnlySingular() {
        assertEquals("لديك رخصة محلية واحدة", mapper.localLicenseSummary(
                Collections.singletonList(license(null, null)), zone));
    }

    @Test
    public void multipleLocalLicensesSelectGreatestIssueInstantRegardlessOfOrdering() {
        LocalLicense ordinary = license("Class 3 - Ordinary driving license", "2026-10-09T10:30:00+03:00");
        LocalLicense motorcycle = license("Class 2 - Heavy Motorcycle License", "2026-10-09T08:00:00Z");
        String expected = "لديك 2 رخص محلية\nأحدث رخصة: الفئة الثانية - رخصة دراجة نارية ثقيلة";
        assertEquals(expected, mapper.localLicenseSummary(Arrays.asList(ordinary, motorcycle), zone));
        assertEquals(expected, mapper.localLicenseSummary(Arrays.asList(motorcycle, ordinary), zone));
    }

    @Test
    public void multipleLocalLicensesIgnoreMissingAndInvalidIssueDates() {
        assertEquals("لديك 3 رخص محلية\nأحدث رخصة: الفئة الثالثة - رخصة قيادة عادية",
                mapper.localLicenseSummary(Arrays.asList(license("Unknown", null),
                        license("Class 3 - Ordinary driving license", "2026-10-09T10:00:00"),
                        license("Class 2 - Heavy Motorcycle License", "invalid")), zone));
    }

    @Test
    public void multipleLocalLicensesWithoutValidDatesDoNotInventLatest() {
        assertEquals("لديك 2 رخص محلية", mapper.localLicenseSummary(Arrays.asList(
                license("Class 3 - Ordinary driving license", null), license("Unknown", "invalid")), zone));
    }

    @Test
    public void nullableLatestClassDoesNotDisplayAnOlderClassAsLatest() {
        assertEquals("لديك 2 رخص محلية", mapper.localLicenseSummary(Arrays.asList(
                license("Class 3 - Ordinary driving license", "2026-10-08"),
                license(null, "2026-10-09")), zone));
    }

    @Test
    public void internationalLicensesHaveSeparateZeroSingularAndPluralMessages() {
        assertEquals("لا توجد رخص دولية", mapper.internationalLicenseSummary(0));
        assertEquals("لديك رخصة دولية واحدة", mapper.internationalLicenseSummary(1));
        assertEquals("لديك 3 رخص دولية", mapper.internationalLicenseSummary(3));
    }

    @Test
    public void allApplicationTypesTranslateByStableIdAndByNameFallback() {
        String[] names = {"NewDrivingLicense", "RenewDrivingLicense", "ReplaceLostDrivingLicense",
                "ReplaceDamagedDrivingLicense", "ReleaseDetainedDrivingLicsense",
                "NewInternationalLicense", "RetakeTest"};
        String[] expected = {"إصدار رخصة قيادة جديدة", "تجديد رخصة قيادة", "بدل فاقد لرخصة قيادة",
                "بدل تالف لرخصة قيادة", "فك حجز رخصة قيادة", "إصدار رخصة دولية جديدة", "إعادة اختبار"};
        for (int i = 0; i < names.length; i++) {
            assertEquals(expected[i], mapper.applicationType(i + 1, names[i]));
            assertEquals(expected[i], mapper.applicationType(null, names[i]));
            assertEquals(expected[i], mapper.applicationType(99, names[i]));
        }
    }

    @Test
    public void stableApplicationIdTakesPriorityAndWorksWithMissingName() {
        assertEquals("تجديد رخصة قيادة", mapper.applicationType(2, "NewDrivingLicense"));
        assertEquals("تجديد رخصة قيادة", mapper.applicationType(2, null));
    }

    @Test
    public void unknownAndNullableApplicationTypesFallBackWithoutInventingText() {
        assertEquals("  FutureApplication  ", mapper.applicationType(99, "  FutureApplication  "));
        assertEquals("FutureApplication", mapper.applicationType(null, "FutureApplication"));
        assertEquals("", mapper.applicationType(null, ""));
        assertNull(mapper.applicationType(99, null));
        assertNull(mapper.applicationType(null, null));
    }

    private LocalLicense license(String className, String issueDate) {
        Map<String, String> values = new HashMap<>();
        values.put("className", className);
        values.put("issueDate", issueDate);
        return gson.fromJson(gson.toJson(values), LocalLicense.class);
    }
}
