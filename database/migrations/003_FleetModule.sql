-- Fleet Module: Vehicles, Telemetry, Maintenance
-- Schema: fleet

-- Vehicles Table (with PostGIS geospatial support)
CREATE TABLE fleet.vehicles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    qr_code VARCHAR(20) UNIQUE NOT NULL,
    type vehicle_type NOT NULL,
    status vehicle_status DEFAULT 'Available' NOT NULL,
    battery_level INT CHECK (battery_level BETWEEN 0 AND 100),
    location GEOGRAPHY(POINT, 4326), -- WGS 84 (lat, lon)
    last_ping_at TIMESTAMPTZ,
    hardware_id VARCHAR(50),
    manufacturer VARCHAR(50),
    model VARCHAR(50),
    purchase_date DATE,
    odometer_km INT DEFAULT 0 NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Indexes for Vehicles
CREATE INDEX idx_vehicles_status ON fleet.vehicles(status) WHERE status = 'Available';
CREATE INDEX idx_vehicles_qr_code ON fleet.vehicles(qr_code);
CREATE INDEX idx_vehicles_type ON fleet.vehicles(type);
CREATE INDEX idx_vehicles_battery ON fleet.vehicles(battery_level) WHERE status = 'Available';

-- Spatial index for geospatial queries (CRITICAL for performance)
CREATE INDEX idx_vehicles_location ON fleet.vehicles USING GIST(location);

COMMENT ON TABLE fleet.vehicles IS 'Fleet vehicles (scooters, e-bikes, bikes)';
COMMENT ON COLUMN fleet.vehicles.qr_code IS 'QR code on vehicle (format: ECO-0001)';
COMMENT ON COLUMN fleet.vehicles.location IS 'Current GPS location (PostGIS geography type)';
COMMENT ON COLUMN fleet.vehicles.hardware_id IS 'IoT device IMEI or serial number';

-- Telemetry Table (Vehicle tracking history)
CREATE TABLE fleet.telemetry (
    id BIGSERIAL PRIMARY KEY,
    vehicle_id UUID NOT NULL REFERENCES fleet.vehicles(id) ON DELETE CASCADE,
    location GEOGRAPHY(POINT, 4326),
    battery_level INT,
    speed INT, -- km/h
    is_moving BOOLEAN DEFAULT FALSE NOT NULL,
    timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_telemetry_vehicle_id ON fleet.telemetry(vehicle_id);
CREATE INDEX idx_telemetry_timestamp ON fleet.telemetry(timestamp DESC);
CREATE INDEX idx_telemetry_location ON fleet.telemetry USING GIST(location);

-- Partition telemetry by month for performance
-- ALTER TABLE fleet.telemetry SET (autovacuum_enabled = true, autovacuum_vacuum_scale_factor = 0.1);

COMMENT ON TABLE fleet.telemetry IS 'Historical vehicle location and status data';
COMMENT ON COLUMN fleet.telemetry.speed IS 'Current speed in kilometers per hour';

-- Maintenance Records Table
CREATE TABLE fleet.maintenance_records (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    vehicle_id UUID NOT NULL REFERENCES fleet.vehicles(id) ON DELETE CASCADE,
    type VARCHAR(50) NOT NULL, -- 'Repair', 'Inspection', 'BatteryReplacement'
    description VARCHAR(500),
    cost_cents BIGINT,
    performed_by VARCHAR(100),
    performed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_maintenance_vehicle_id ON fleet.maintenance_records(vehicle_id);
CREATE INDEX idx_maintenance_performed_at ON fleet.maintenance_records(performed_at DESC);

COMMENT ON TABLE fleet.maintenance_records IS 'Vehicle maintenance and repair history';
COMMENT ON COLUMN fleet.maintenance_records.cost_cents IS 'Maintenance cost in cents';

-- Trigger: Update updated_at timestamp
CREATE TRIGGER trigger_vehicles_updated_at
BEFORE UPDATE ON fleet.vehicles
FOR EACH ROW
EXECUTE FUNCTION security.update_updated_at_column();

-- Helper function: Find nearby vehicles
CREATE OR REPLACE FUNCTION fleet.find_nearby_vehicles(
    user_latitude DOUBLE PRECISION,
    user_longitude DOUBLE PRECISION,
    radius_meters INT DEFAULT 500
)
RETURNS TABLE (
    vehicle_id UUID,
    qr_code VARCHAR,
    vehicle_type vehicle_type,
    battery_level INT,
    latitude DOUBLE PRECISION,
    longitude DOUBLE PRECISION,
    distance_meters DOUBLE PRECISION
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        v.id,
        v.qr_code,
        v.type,
        v.battery_level,
        ST_Y(v.location::geometry) AS latitude,
        ST_X(v.location::geometry) AS longitude,
        ST_Distance(
            v.location,
            ST_SetSRID(ST_MakePoint(user_longitude, user_latitude), 4326)::geography
        ) AS distance_meters
    FROM fleet.vehicles v
    WHERE v.status = 'Available'
      AND v.battery_level >= 20
      AND ST_DWithin(
          v.location,
          ST_SetSRID(ST_MakePoint(user_longitude, user_latitude), 4326)::geography,
          radius_meters
      )
    ORDER BY distance_meters ASC
    LIMIT 20;
END;
$$ LANGUAGE plpgsql STABLE;

COMMENT ON FUNCTION fleet.find_nearby_vehicles IS 'Find available vehicles within radius (uses spatial index for performance)';
