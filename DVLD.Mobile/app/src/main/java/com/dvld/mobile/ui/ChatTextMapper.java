package com.dvld.mobile.ui;

import com.dvld.mobile.R;
import com.dvld.mobile.model.ChatSource;
import com.dvld.mobile.repository.ChatRepository;

public final class ChatTextMapper {
    public interface Strings { String get(int resource, Object... args); }
    private final Strings strings;
    public ChatTextMapper(Strings strings) { this.strings = strings; }
    public String error(ChatRepository.ChatError error) {
        if (error == null) return strings.get(R.string.chat_error_server);
        switch (error) {
            case BAD_REQUEST: return strings.get(R.string.chat_error_bad_request);
            case UNAUTHORIZED: return strings.get(R.string.chat_error_unauthorized);
            case FORBIDDEN: return strings.get(R.string.chat_error_forbidden);
            case NETWORK: return strings.get(R.string.chat_error_network);
            default: return strings.get(R.string.chat_error_server);
        }
    }
    public String source(ChatSource source) {
        return strings.get(R.string.chat_source_reference,
                number(source.getDocumentID()), number(source.getPageNumber()));
    }
    private String number(Long value) {
        return value == null || value <= 0 ? strings.get(R.string.dashboard_unknown_value) : value.toString();
    }
}
