package com.dvld.mobile.network;

import org.junit.Test;

import java.util.Arrays;
import java.util.Collections;

import okhttp3.Cookie;
import okhttp3.HttpUrl;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

public class SessionCookieJarTest {
    private final HttpUrl url = HttpUrl.get("http://127.0.0.1:5277/api/mobile-auth/login");

    @Test
    public void retainsChunkedAuthCookiesAndReplacesTheirValues() {
        SessionCookieJar jar = new SessionCookieJar();
        jar.saveFromResponse(url, Arrays.asList(
                Cookie.parse(url, "DVLD.MobileAuth=chunks-2; Path=/; HttpOnly"),
                Cookie.parse(url, "DVLD.MobileAuthC1=first; Path=/; HttpOnly"),
                Cookie.parse(url, "DVLD.MobileAuthC2=second; Path=/; HttpOnly")));
        assertEquals(3, jar.loadForRequest(url.resolve("/api/people")).size());

        jar.saveFromResponse(url, Collections.singletonList(
                Cookie.parse(url, "DVLD.MobileAuthC1=updated; Path=/; HttpOnly")));
        assertEquals(3, jar.loadForRequest(url).size());
        assertTrue(jar.loadForRequest(url).stream().anyMatch(cookie ->
                cookie.name().equals("DVLD.MobileAuthC1") && cookie.value().equals("updated")));
        assertTrue(new SessionCookieJar().loadForRequest(url).isEmpty());
    }

    @Test
    public void honorsCookieHostPathAndSecureRestrictions() {
        SessionCookieJar jar = new SessionCookieJar();
        jar.saveFromResponse(url, Collections.singletonList(
                Cookie.parse(url, "DVLD.MobileAuth=session; Path=/api; Secure; HttpOnly")));
        assertTrue(jar.loadForRequest(url).isEmpty());
        assertTrue(jar.loadForRequest(HttpUrl.get("https://example.com/api/people")).isEmpty());
        assertTrue(jar.loadForRequest(HttpUrl.get("https://127.0.0.1:5277/other")).isEmpty());
        assertEquals(1, jar.loadForRequest(HttpUrl.get("https://127.0.0.1:5277/api/people")).size());
    }

    @Test
    public void serverDeletionAndExpirationRemoveCookies() {
        SessionCookieJar jar = new SessionCookieJar();
        jar.saveFromResponse(url, Collections.singletonList(
                Cookie.parse(url, "DVLD.MobileAuth=session; Path=/")));
        jar.saveFromResponse(url, Collections.singletonList(
                Cookie.parse(url, "DVLD.MobileAuth=; Max-Age=0; Path=/")));
        assertTrue(jar.loadForRequest(url).isEmpty());

        jar.saveFromResponse(url, Collections.singletonList(new Cookie.Builder()
                .name("DVLD.MobileAuth").value("expired").hostOnlyDomain(url.host())
                .path("/").expiresAt(System.currentTimeMillis() - 1).build()));
        assertTrue(jar.loadForRequest(url).isEmpty());
    }
}
