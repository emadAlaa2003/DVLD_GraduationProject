package com.dvld.mobile.network;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import okhttp3.Cookie;
import okhttp3.CookieJar;
import okhttp3.HttpUrl;

/** Process-memory cookies only, including ASP.NET's chunked authentication cookies. */
public final class SessionCookieJar implements CookieJar {
    private final List<Cookie> cookies = new ArrayList<>();

    @Override
    public synchronized void saveFromResponse(HttpUrl url, List<Cookie> receivedCookies) {
        long now = System.currentTimeMillis();
        for (Cookie received : receivedCookies) {
            Iterator<Cookie> iterator = cookies.iterator();
            while (iterator.hasNext()) {
                Cookie existing = iterator.next();
                if (existing.expiresAt() <= now ||
                        (existing.name().equals(received.name()) &&
                                existing.domain().equals(received.domain()) &&
                                existing.path().equals(received.path()))) {
                    iterator.remove();
                }
            }
            if (received.expiresAt() > now) {
                cookies.add(received);
            }
        }
    }

    @Override
    public synchronized List<Cookie> loadForRequest(HttpUrl url) {
        List<Cookie> matchingCookies = new ArrayList<>();
        long now = System.currentTimeMillis();
        Iterator<Cookie> iterator = cookies.iterator();
        while (iterator.hasNext()) {
            Cookie cookie = iterator.next();
            if (cookie.expiresAt() <= now) {
                iterator.remove();
            } else if (cookie.matches(url)) {
                matchingCookies.add(cookie);
            }
        }
        return matchingCookies;
    }
}
