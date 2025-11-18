-- EcoRide Database - Master Migration Script
-- Run all migrations in order
-- PostgreSQL 16 with PostGIS

-- Usage:
-- psql -h localhost -U ecoride_user -d EcoRide -f 000_RunAllMigrations.sql

\echo 'Starting EcoRide database migrations...'
\echo ''

\echo '[1/7] Initializing database with schemas and extensions...'
\i 001_InitializeDatabase.sql
\echo ''

\echo '[2/7] Creating Security module tables...'
\i 002_SecurityModule.sql
\echo ''

\echo '[3/7] Creating Fleet module tables...'
\i 003_FleetModule.sql
\echo ''

\echo '[4/7] Creating Trip module tables...'
\i 004_TripModule.sql
\echo ''

\echo '[5/7] Creating Payment module tables...'
\i 005_PaymentModule.sql
\echo ''

\echo '[6/7] Creating Notification and Events tables...'
\i 006_NotificationAndEventsModule.sql
\echo ''

\echo '[7/7] Creating views and loading seed data...'
\i 007_ViewsAndSeedData.sql
\echo ''

\echo '✅ All migrations completed successfully!'
\echo ''
\echo 'Database Summary:'
SELECT
    'Schemas' AS type,
    COUNT(*)::TEXT AS count
FROM information_schema.schemata
WHERE schema_name IN ('security', 'fleet', 'trip', 'payment', 'notification', 'events')
UNION ALL
SELECT
    'Tables' AS type,
    COUNT(*)::TEXT AS count
FROM information_schema.tables
WHERE table_schema IN ('security', 'fleet', 'trip', 'payment', 'notification', 'events')
UNION ALL
SELECT
    'Views' AS type,
    COUNT(*)::TEXT AS count
FROM information_schema.views
WHERE table_schema IN ('security', 'fleet', 'trip', 'payment', 'notification', 'events')
UNION ALL
SELECT
    'Functions' AS type,
    COUNT(*)::TEXT AS count
FROM information_schema.routines
WHERE routine_schema IN ('security', 'fleet', 'trip', 'payment', 'notification', 'events');

\echo ''
\echo 'Test Credentials:'
\echo '  Admin: admin@ecoride.ma / Admin@123'
\echo '  User:  tourist@example.com / Test@123'
\echo ''
\echo 'Ready for development! 🚀'
