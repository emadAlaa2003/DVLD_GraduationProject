package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.google.gson.Gson;
import org.junit.BeforeClass;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.io.File;
import java.time.Instant;
import java.time.ZoneId;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;
import javax.xml.parsers.DocumentBuilderFactory;

/** Tests use the actual Arabic resources without an Android runtime dependency. */
public abstract class ArabicResourcesTestSupport {
    private static final Map<Integer, String> resources = new HashMap<>();
    protected static final ZoneId ZONE = ZoneId.of("Asia/Hebron");
    protected static final Instant NOW = Instant.parse("2026-10-10T10:00:00Z");
    protected final Gson gson = new Gson();
    protected final DashboardTextMapper.Strings arabic =
            (id, arguments) -> String.format(Locale.ROOT, resources.get(id), arguments);

    @BeforeClass
    public static void readArabicResources() throws Exception {
        NodeList nodes = DocumentBuilderFactory.newInstance().newDocumentBuilder()
                .parse(new File("src/main/res/values/strings.xml")).getElementsByTagName("string");
        for (int i = 0; i < nodes.getLength(); i++) {
            Node node = nodes.item(i);
            resources.put(R.string.class.getField(node.getAttributes().getNamedItem("name").getNodeValue())
                    .getInt(null), node.getTextContent());
        }
    }
}
