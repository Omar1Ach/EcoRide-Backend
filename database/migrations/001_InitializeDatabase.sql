-- EcoRide Database Initialization
-- PostgreSQL 16 with PostGIS Extension
-- Created: 2025-11-18

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable PostGIS for geospatial support
CREATE EXTENSION IF NOT EXISTS postgis;

-- Create schemas for modular organization
CREATE SCHEMA IF NOT EXISTS security;
CREATE SCHEMA IF NOT EXISTS fleet;
CREATE SCHEMA IF NOT EXISTS trip;
CREATE SCHEMA IF NOT EXISTS payment;
CREATE SCHEMA IF NOT EXISTS notification;
CREATE SCHEMA IF NOT EXISTS events;

-- Create custom types
CREATE TYPE user_role AS ENUM ('User', 'Admin');
CREATE TYPE kyc_status AS ENUM ('Pending', 'Approved', 'Rejected');
CREATE TYPE vehicle_type AS ENUM ('Scooter', 'EBike', 'Bike');
CREATE TYPE vehicle_status AS ENUM ('Available', 'Reserved', 'InUse', 'Maintenance', 'Offline');
CREATE TYPE reservation_status AS ENUM ('Active', 'Expired', 'Converted', 'Cancelled');
CREATE TYPE trip_status AS ENUM ('Active', 'Completed', 'Cancelled');
CREATE TYPE payment_status AS ENUM ('Pending', 'Succeeded', 'Failed', 'Refunded');
CREATE TYPE payment_method_type AS ENUM ('Wallet', 'Card', 'Cash');
CREATE TYPE transaction_type AS ENUM ('Authorization', 'Capture', 'Refund');
CREATE TYPE notification_channel AS ENUM ('Push', 'Email', 'SMS');
CREATE TYPE notification_status AS ENUM ('Pending', 'Sent', 'Failed');
CREATE TYPE event_state AS ENUM ('NotPublished', 'InProgress', 'Published', 'PublishedFailed');

COMMENT ON SCHEMA security IS 'Authentication, Users, and Wallets';
COMMENT ON SCHEMA fleet IS 'Vehicles and Telemetry';
COMMENT ON SCHEMA trip IS 'Reservations and Trips';
COMMENT ON SCHEMA payment IS 'Payments and Payment Methods';
COMMENT ON SCHEMA notification IS 'Notifications';
COMMENT ON SCHEMA events IS 'Event Sourcing and Domain Events';
