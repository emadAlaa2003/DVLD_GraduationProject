package com.dvld.mobile.chat;

import android.Manifest;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;
import androidx.core.content.ContextCompat;
import com.dvld.mobile.R;
import com.dvld.mobile.repository.ApiChatRepository;
import com.dvld.mobile.ui.ChatbotActivity;
import com.dvld.mobile.ui.DashboardActivity;

/** Android wiring for the process-scoped manager. Retains application context only. */
public final class ChatRuntime {
    private static final String CHANNEL_ID = "dvld_chat_answers";
    private static final int NOTIFICATION_ID = 710;
    private static ChatRuntime instance;
    private final Context applicationContext;
    private final ChatRequestManager manager;
    private boolean permissionPrompted;
    private Integer personId;
    private String fullName;
    private String username;

    private ChatRuntime(Context context) {
        applicationContext = context.getApplicationContext();
        Handler main = new Handler(Looper.getMainLooper());
        manager = new ChatRequestManager(new ApiChatRepository(), main::post, this::notifyAnswerReady);
        NotificationManager notifications = applicationContext.getSystemService(NotificationManager.class);
        NotificationChannel channel = new NotificationChannel(CHANNEL_ID,
                applicationContext.getString(R.string.chat_notification_channel), NotificationManager.IMPORTANCE_DEFAULT);
        channel.setDescription(applicationContext.getString(R.string.chat_notification_channel_description));
        notifications.createNotificationChannel(channel);
    }

    public static synchronized ChatRuntime get(Context context) {
        if (instance == null) instance = new ChatRuntime(context);
        return instance;
    }
    public ChatRequestManager getManager() { return manager; }

    public void rememberDashboard(Intent incoming) {
        if (!incoming.hasExtra(DashboardActivity.EXTRA_PERSON_ID)) return;
        personId = incoming.getIntExtra(DashboardActivity.EXTRA_PERSON_ID, -1);
        fullName = incoming.getStringExtra(DashboardActivity.EXTRA_FULL_NAME);
        username = incoming.getStringExtra(DashboardActivity.EXTRA_USERNAME);
    }

    public boolean claimPermissionPrompt() {
        if (permissionPrompted) return false;
        permissionPrompted = true;
        return true;
    }

    public void clearNotification() {
        NotificationManagerCompat.from(applicationContext).cancel(NOTIFICATION_ID);
    }

    private void addCitizenExtras(Intent intent) {
        if (personId != null) intent.putExtra(DashboardActivity.EXTRA_PERSON_ID, personId.intValue());
        intent.putExtra(DashboardActivity.EXTRA_FULL_NAME, fullName);
        intent.putExtra(DashboardActivity.EXTRA_USERNAME, username);
    }

    private void notifyAnswerReady() {
        if (Build.VERSION.SDK_INT >= 33 && ContextCompat.checkSelfPermission(applicationContext,
                Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED) return;
        NotificationManagerCompat notifications = NotificationManagerCompat.from(applicationContext);
        if (!notifications.areNotificationsEnabled()) return;
        // Explicit parent ensures Back works even if Android removed the previous Activity stack.
        Intent dashboard = new Intent(applicationContext, DashboardActivity.class)
                .addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        Intent chat = new Intent(applicationContext, ChatbotActivity.class)
                .addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        addCitizenExtras(dashboard);
        addCitizenExtras(chat);
        PendingIntent openChat = PendingIntent.getActivities(applicationContext, NOTIFICATION_ID,
                new Intent[] {dashboard, chat}, PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
        try {
            notifications.notify(NOTIFICATION_ID, new NotificationCompat.Builder(applicationContext, CHANNEL_ID)
                    .setSmallIcon(R.drawable.ic_chat_notification)
                    .setContentTitle(applicationContext.getString(R.string.dashboard_traffic_assistant))
                    .setContentText(applicationContext.getString(R.string.chat_answer_ready))
                    .setContentIntent(openChat).setAutoCancel(true)
                    .setCategory(NotificationCompat.CATEGORY_MESSAGE).build());
        } catch (SecurityException ignored) {
            // Permission may be revoked between checking and posting; the answer remains in memory.
        }
    }
}
