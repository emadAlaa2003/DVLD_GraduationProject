package com.dvld.mobile.ui;

import com.dvld.mobile.model.CitizenApplication;
import com.dvld.mobile.model.DashboardData;
import com.dvld.mobile.model.TestAppointment;
import com.google.gson.Gson;
import org.junit.Test;
import java.time.Instant;
import java.time.ZoneId;
import java.util.Arrays;
import java.util.Collections;
import static org.junit.Assert.*;

public class DashboardNavigationStateTest {
    private final Gson gson = new Gson();
    private final DashboardNavigationState state = new DashboardNavigationState();

    @Test
    public void usesTheExactLatestApplicationSelectedByDashboardRatherThanListOrder() {
        CitizenApplication older = gson.fromJson("{\"applicationID\":24,\"applicationDate\":\"2026-10-08\"}", CitizenApplication.class);
        CitizenApplication newer = gson.fromJson("{\"applicationID\":25,\"applicationDate\":\"2026-10-09\"}", CitizenApplication.class);
        DashboardData data = new DashboardData(Collections.emptyList(), Collections.emptyList(),
                Arrays.asList(newer, older), Collections.emptyList());
        state.setLatestApplication(data.getLatestApplication(ZoneId.of("Asia/Hebron")));
        assertEquals(Integer.valueOf(25), state.getLatestApplicationId());
    }

    @Test
    public void invalidLatestIdsAndMissingApplicationsCannotBeOpened() {
        for (String json : new String[] {"{}", "{\"applicationID\":0}", "{\"applicationID\":-1}"}) {
            state.setLatestApplication(gson.fromJson(json, CitizenApplication.class));
            assertNull(state.getLatestApplicationId());
        }
        state.setLatestApplication(null);
        assertNull(state.getLatestApplicationId());
    }

    @Test
    public void loadingAndErrorsClearBothPreviouslyDisplayedDestinations() {
        state.setLatestApplication(gson.fromJson("{\"applicationID\":24}", CitizenApplication.class));
        state.setUpcomingAppointment(gson.fromJson("{\"testAppointmentID\":18}", TestAppointment.class));
        assertNotNull(state.getLatestApplicationId());
        assertNotNull(state.getUpcomingAppointmentId());
        state.clear();
        assertNull(state.getLatestApplicationId());
        assertNull(state.getUpcomingAppointmentId());
    }

    @Test
    public void upcomingUsesDashboardSelectionAndEmptyOrInvalidCannotOpenStaleId() {
        TestAppointment valid = gson.fromJson("{\"testAppointmentID\":18,\"appointmentDate\":\"2026-10-11\",\"isLocked\":false}", TestAppointment.class);
        DashboardData data = new DashboardData(Collections.emptyList(), Collections.emptyList(),
                Collections.emptyList(), Collections.singletonList(valid));
        state.setUpcomingAppointment(data.getUpcomingAppointment(Instant.parse("2026-10-10T10:00:00Z"), ZoneId.of("Asia/Hebron")));
        assertEquals(Integer.valueOf(18), state.getUpcomingAppointmentId());
        state.setUpcomingAppointment(null);
        assertNull(state.getUpcomingAppointmentId());
        state.setUpcomingAppointment(gson.fromJson("{\"testAppointmentID\":0}", TestAppointment.class));
        assertNull(state.getUpcomingAppointmentId());
        state.setLatestApplication(null);
        assertNull(state.getLatestApplicationId());
    }
}
