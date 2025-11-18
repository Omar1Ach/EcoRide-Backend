-- Trip Module: Reservations, Trips, Routes
-- Schema: trip

-- Reservations Table
CREATE TABLE trip.reservations (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL, -- FK to security.users
    vehicle_id UUID NOT NULL, -- FK to fleet.vehicles
    status reservation_status DEFAULT 'Active' NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    cancelled_at TIMESTAMPTZ NULL
);

-- Indexes for Reservations
CREATE INDEX idx_reservations_user_id ON trip.reservations(user_id);
CREATE INDEX idx_reservations_vehicle_id ON trip.reservations(vehicle_id);
CREATE INDEX idx_reservations_expires_at ON trip.reservations(expires_at) WHERE status = 'Active';
CREATE INDEX idx_reservations_created_at ON trip.reservations(created_at DESC);

-- Unique constraint: One active reservation per user
CREATE UNIQUE INDEX idx_reservations_one_active_per_user
ON trip.reservations(user_id)
WHERE status = 'Active';

COMMENT ON TABLE trip.reservations IS 'Vehicle reservations (5-minute hold before trip starts)';
COMMENT ON COLUMN trip.reservations.expires_at IS 'Reservation expires if trip not started by this time';

-- Trips Table
CREATE TABLE trip.trips (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL, -- FK to security.users
    vehicle_id UUID, -- FK to fleet.vehicles (nullable if vehicle deleted)
    start_time TIMESTAMPTZ NOT NULL,
    end_time TIMESTAMPTZ NULL,
    start_location GEOGRAPHY(POINT, 4326),
    end_location GEOGRAPHY(POINT, 4326),
    duration_minutes INT,
    distance_meters INT,
    cost_cents BIGINT,
    status trip_status DEFAULT 'Active' NOT NULL,
    rating INT CHECK (rating BETWEEN 1 AND 5),
    review_comment VARCHAR(500),
    cancellation_reason VARCHAR(500),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    completed_at TIMESTAMPTZ NULL
);

-- Indexes for Trips
CREATE INDEX idx_trips_user_id ON trip.trips(user_id);
CREATE INDEX idx_trips_vehicle_id ON trip.trips(vehicle_id);
CREATE INDEX idx_trips_status ON trip.trips(status);
CREATE INDEX idx_trips_created_at ON trip.trips(created_at DESC);
CREATE INDEX idx_trips_start_time ON trip.trips(start_time DESC);
CREATE INDEX idx_trips_completed_at ON trip.trips(completed_at DESC) WHERE completed_at IS NOT NULL;

-- Spatial indexes
CREATE INDEX idx_trips_start_location ON trip.trips USING GIST(start_location);
CREATE INDEX idx_trips_end_location ON trip.trips USING GIST(end_location);

-- Unique constraint: One active trip per user
CREATE UNIQUE INDEX idx_trips_one_active_per_user
ON trip.trips(user_id)
WHERE status = 'Active';

COMMENT ON TABLE trip.trips IS 'Completed and active trips';
COMMENT ON COLUMN trip.trips.cost_cents IS 'Trip cost in cents (calculated: base + per-minute rate)';
COMMENT ON COLUMN trip.trips.duration_minutes IS 'Trip duration in minutes';
COMMENT ON COLUMN trip.trips.distance_meters IS 'Approximate distance traveled in meters';

-- Trip Route Points Table (GPS tracking during trip)
CREATE TABLE trip.trip_route_points (
    id BIGSERIAL PRIMARY KEY,
    trip_id UUID NOT NULL REFERENCES trip.trips(id) ON DELETE CASCADE,
    location GEOGRAPHY(POINT, 4326),
    timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_trip_route_points_trip_id ON trip.trip_route_points(trip_id);
CREATE INDEX idx_trip_route_points_location ON trip.trip_route_points USING GIST(location);
CREATE INDEX idx_trip_route_points_timestamp ON trip.trip_route_points(timestamp);

COMMENT ON TABLE trip.trip_route_points IS 'GPS breadcrumbs tracked during active trips';

-- Function: Calculate trip cost
CREATE OR REPLACE FUNCTION trip.calculate_trip_cost(
    duration_minutes INT
)
RETURNS BIGINT AS $$
DECLARE
    base_fare_cents BIGINT := 500; -- 5 MAD
    per_minute_rate_cents BIGINT := 150; -- 1.5 MAD per minute
    total_cost_cents BIGINT;
BEGIN
    total_cost_cents := base_fare_cents + (per_minute_rate_cents * duration_minutes);
    RETURN total_cost_cents;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

COMMENT ON FUNCTION trip.calculate_trip_cost IS 'Calculate trip cost: 5 MAD base + 1.5 MAD per minute';

-- Function: Expire old reservations (called by background job)
CREATE OR REPLACE FUNCTION trip.expire_old_reservations()
RETURNS INT AS $$
DECLARE
    expired_count INT;
BEGIN
    WITH updated AS (
        UPDATE trip.reservations
        SET status = 'Expired'
        WHERE status = 'Active'
          AND expires_at < CURRENT_TIMESTAMP
        RETURNING id
    )
    SELECT COUNT(*) INTO expired_count FROM updated;

    RETURN expired_count;
END;
$$ LANGUAGE plpgsql;

COMMENT ON FUNCTION trip.expire_old_reservations IS 'Marks expired reservations as "Expired" (run every minute via Hangfire)';

-- Function: Get user trip statistics
CREATE OR REPLACE FUNCTION trip.get_user_trip_stats(p_user_id UUID)
RETURNS TABLE (
    total_trips BIGINT,
    total_cost_cents BIGINT,
    total_duration_minutes BIGINT,
    total_distance_meters BIGINT,
    average_rating NUMERIC
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*)::BIGINT AS total_trips,
        COALESCE(SUM(t.cost_cents), 0) AS total_cost_cents,
        COALESCE(SUM(t.duration_minutes), 0) AS total_duration_minutes,
        COALESCE(SUM(t.distance_meters), 0) AS total_distance_meters,
        ROUND(AVG(t.rating)::NUMERIC, 2) AS average_rating
    FROM trip.trips t
    WHERE t.user_id = p_user_id
      AND t.status = 'Completed';
END;
$$ LANGUAGE plpgsql STABLE;

COMMENT ON FUNCTION trip.get_user_trip_stats IS 'Get aggregate statistics for a user';
