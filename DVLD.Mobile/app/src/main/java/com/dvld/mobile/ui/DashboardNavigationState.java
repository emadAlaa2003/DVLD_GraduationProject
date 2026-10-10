package com.dvld.mobile.ui;

import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.model.TestAppointment;

/** Only IDs from the currently displayed snapshot can be opened. */
public final class DashboardNavigationState {
    private Integer latestApplicationId;
    private Integer upcomingAppointmentId;

    public void clear() {
        latestApplicationId = null;
        upcomingAppointmentId = null;
    }

    public void setLatestApplication(CitizenApplication application) {
        latestApplicationId = validId(application == null ? null : application.getApplicationID());
    }

    public void setUpcomingAppointment(TestAppointment appointment) {
        upcomingAppointmentId = validId(appointment == null ? null : appointment.getTestAppointmentID());
    }

    public Integer getLatestApplicationId() { return latestApplicationId; }
    public Integer getUpcomingAppointmentId() { return upcomingAppointmentId; }

    private static Integer validId(Integer id) {
        return id != null && id > 0 ? id : null;
    }
}
