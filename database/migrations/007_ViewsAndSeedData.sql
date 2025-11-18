-- Analytics Views and Seed Data
-- Views for Admin Dashboard & Reporting

-- ===== ANALYTICS VIEWS =====

-- Fleet Status View
CREATE OR REPLACE VIEW fleet.fleet_status AS
SELECT
    v.type AS vehicle_type,
    v.status,
    COUNT(*) AS vehicle_count,
    ROUND(AVG(v.battery_level)::NUMERIC, 2) AS avg_battery_level,
    SUM(CASE WHEN v.battery_level < 20 THEN 1 ELSE 0 END) AS low_battery_count
FROM fleet.vehicles v
GROUP BY v.type, v.status;

COMMENT ON VIEW fleet.fleet_status IS 'Real-time fleet status overview';

-- Active Trips View
CREATE OR REPLACE VIEW trip.active_trips AS
SELECT
    t.id AS trip_id,
    t.user_id,
    u.full_name AS user_name,
    u.phone AS user_phone,
    t.vehicle_id,
    v.qr_code AS vehicle_qr_code,
    v.type AS vehicle_type,
    t.start_time,
    EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - t.start_time)) / 60 AS duration_minutes,
    trip.calculate_trip_cost(CEIL(EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - t.start_time)) / 60)::INT) AS estimated_cost_cents
FROM trip.trips t
INNER JOIN security.users u ON t.user_id = u.id
LEFT JOIN fleet.vehicles v ON t.vehicle_id = v.id
WHERE t.status = 'Active';

COMMENT ON VIEW trip.active_trips IS 'All currently active trips with real-time cost estimates';

-- User Statistics View
CREATE OR REPLACE VIEW security.user_statistics AS
SELECT
    u.id AS user_id,
    u.full_name,
    u.email,
    u.phone,
    u.role,
    u.kyc_status,
    u.created_at AS registered_at,
    w.balance_cents AS wallet_balance_cents,
    COUNT(DISTINCT t.id) AS total_trips,
    COALESCE(SUM(t.cost_cents), 0) AS total_spent_cents,
    COALESCE(AVG(t.rating)::NUMERIC(3,2), 0) AS avg_rating,
    MAX(t.created_at) AS last_trip_at
FROM security.users u
LEFT JOIN security.wallets w ON u.id = w.user_id
LEFT JOIN trip.trips t ON u.id = t.user_id AND t.status = 'Completed'
WHERE u.deleted_at IS NULL
GROUP BY u.id, u.full_name, u.email, u.phone, u.role, u.kyc_status, u.created_at, w.balance_cents;

COMMENT ON VIEW security.user_statistics IS 'User profiles with trip statistics and wallet balances';

-- Revenue Trends View (Last 30 Days)
CREATE OR REPLACE VIEW payment.revenue_trends_30d AS
SELECT
    DATE(p.created_at) AS date,
    COUNT(DISTINCT p.user_id) AS unique_users,
    COUNT(*) AS payment_count,
    SUM(p.amount_cents) AS total_revenue_cents,
    AVG(p.amount_cents) AS avg_payment_cents,
    SUM(CASE WHEN p.status = 'Succeeded' THEN 1 ELSE 0 END) AS successful_payments,
    SUM(CASE WHEN p.status = 'Failed' THEN 1 ELSE 0 END) AS failed_payments
FROM payment.payments p
WHERE p.created_at >= CURRENT_DATE - INTERVAL '30 days'
  AND p.trip_id IS NOT NULL
GROUP BY DATE(p.created_at)
ORDER BY date DESC;

COMMENT ON VIEW payment.revenue_trends_30d IS 'Daily revenue trends for the last 30 days';

-- ===== SEED DATA (Development Only) =====

-- Admin User
INSERT INTO security.users (id, email, phone, password_hash, full_name, role, kyc_status, is_active, phone_verified, email_verified)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'admin@ecoride.ma',
    '+212600000001',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5kuLXdQ5hOjJC', -- Password: Admin@123
    'System Administrator',
    'Admin',
    'Approved',
    TRUE,
    TRUE,
    TRUE
) ON CONFLICT (id) DO NOTHING;

-- Admin Wallet
INSERT INTO security.wallets (user_id, balance_cents)
VALUES ('00000000-0000-0000-0000-000000000001', 100000) -- 1000 MAD
ON CONFLICT (user_id) DO NOTHING;

-- Test User
INSERT INTO security.users (id, email, phone, password_hash, full_name, role, kyc_status, is_active, phone_verified, email_verified)
VALUES (
    '00000000-0000-0000-0000-000000000002',
    'tourist@example.com',
    '+212600000002',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5kuLXdQ5hOjJC', -- Password: Test@123
    'John Doe',
    'User',
    'Approved',
    TRUE,
    TRUE,
    TRUE
) ON CONFLICT (id) DO NOTHING;

-- Test User Wallet
INSERT INTO security.wallets (user_id, balance_cents)
VALUES ('00000000-0000-0000-0000-000000000002', 5000) -- 50 MAD
ON CONFLICT (user_id) DO NOTHING;

-- Seed Vehicles (10 vehicles in Rabat city center)
-- Rabat coordinates: ~34.0209° N, 6.8416° W

INSERT INTO fleet.vehicles (id, qr_code, type, status, battery_level, location, manufacturer, model, purchase_date)
VALUES
    ('10000000-0000-0000-0000-000000000001', 'ECO-0001', 'Scooter', 'Available', 95, ST_SetSRID(ST_MakePoint(-6.8416, 34.0209), 4326)::geography, 'Xiaomi', 'M365 Pro', '2025-01-01'),
    ('10000000-0000-0000-0000-000000000002', 'ECO-0002', 'Scooter', 'Available', 88, ST_SetSRID(ST_MakePoint(-6.8420, 34.0215), 4326)::geography, 'Xiaomi', 'M365 Pro', '2025-01-01'),
    ('10000000-0000-0000-0000-000000000003', 'ECO-0003', 'EBike', 'Available', 100, ST_SetSRID(ST_MakePoint(-6.8410, 34.0200), 4326)::geography, 'Cowboy', 'Cowboy 4', '2025-01-02'),
    ('10000000-0000-0000-0000-000000000004', 'ECO-0004', 'Scooter', 'Available', 72, ST_SetSRID(ST_MakePoint(-6.8425, 34.0220), 4326)::geography, 'Segway', 'Ninebot Max', '2025-01-02'),
    ('10000000-0000-0000-0000-000000000005', 'ECO-0005', 'Bike', 'Available', NULL, ST_SetSRID(ST_MakePoint(-6.8400, 34.0190), 4326)::geography, 'Trek', 'FX 2', '2025-01-03'),
    ('10000000-0000-0000-0000-000000000006', 'ECO-0006', 'Scooter', 'Available', 65, ST_SetSRID(ST_MakePoint(-6.8430, 34.0225), 4326)::geography, 'Xiaomi', 'M365 Pro', '2025-01-03'),
    ('10000000-0000-0000-0000-000000000007', 'ECO-0007', 'EBike', 'Available', 92, ST_SetSRID(ST_MakePoint(-6.8405, 34.0195), 4326)::geography, 'VanMoof', 'S3', '2025-01-04'),
    ('10000000-0000-0000-0000-000000000008', 'ECO-0008', 'Scooter', 'Maintenance', 15, ST_SetSRID(ST_MakePoint(-6.8435, 34.0230), 4326)::geography, 'Segway', 'Ninebot Max', '2025-01-04'),
    ('10000000-0000-0000-0000-000000000009', 'ECO-0009', 'Bike', 'Available', NULL, ST_SetSRID(ST_MakePoint(-6.8395, 34.0185), 4326)::geography, 'Giant', 'Escape 3', '2025-01-05'),
    ('10000000-0000-0000-0000-000000000010', 'ECO-0010', 'Scooter', 'Available', 80, ST_SetSRID(ST_MakePoint(-6.8440, 34.0235), 4326)::geography, 'Xiaomi', 'Pro 2', '2025-01-05')
ON CONFLICT (id) DO NOTHING;

-- ===== HELPFUL QUERIES FOR DEVELOPMENT =====

COMMENT ON DATABASE "EcoRide" IS 'EcoRide: Tourism-focused micro-mobility platform | DDD + CQRS + Event-Driven Architecture';

-- Grant appropriate permissions (adjust for production)
-- GRANT USAGE ON SCHEMA security, fleet, trip, payment, notification, events TO ecoride_user;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA security, fleet, trip, payment, notification, events TO ecoride_user;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA security, fleet, trip, payment, notification, events TO ecoride_user;
