-- Notification & Events Module
-- Schema: notification, events

-- ===== NOTIFICATION SCHEMA =====

-- Notifications Table
CREATE TABLE notification.notifications (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID NULL, -- FK to security.users (nullable for broadcast notifications)
    type VARCHAR(50) NOT NULL, -- 'TripStarted', 'TripCompleted', 'PaymentSucceeded', 'ReservationExpiring'
    title VARCHAR(200),
    message VARCHAR(500) NOT NULL,
    status notification_status DEFAULT 'Pending' NOT NULL,
    channel notification_channel NOT NULL,
    sent_at TIMESTAMPTZ NULL,
    read_at TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_notifications_user_id ON notification.notifications(user_id);
CREATE INDEX idx_notifications_status ON notification.notifications(status) WHERE status = 'Pending';
CREATE INDEX idx_notifications_created_at ON notification.notifications(created_at DESC);
CREATE INDEX idx_notifications_read_at ON notification.notifications(read_at) WHERE read_at IS NULL;

COMMENT ON TABLE notification.notifications IS 'Push, SMS, and email notifications';
COMMENT ON COLUMN notification.notifications.type IS 'Notification event type for routing and templates';

-- ===== EVENTS SCHEMA (Event Sourcing) =====

-- Domain Events Table (audit trail + event sourcing)
CREATE TABLE events.domain_events (
    id BIGSERIAL PRIMARY KEY,
    aggregate_id UUID NOT NULL, -- Trip ID, Reservation ID, Payment ID, etc.
    aggregate_type VARCHAR(50) NOT NULL, -- 'Trip', 'Reservation', 'Payment', 'User'
    event_type VARCHAR(100) NOT NULL, -- 'TripStarted', 'TripEnded', 'PaymentProcessed'
    event_data JSONB NOT NULL, -- JSON payload with event details
    user_id UUID NULL, -- Who triggered the event
    occurred_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    processed_at TIMESTAMPTZ NULL -- For event handlers
);

CREATE INDEX idx_domain_events_aggregate_id ON events.domain_events(aggregate_id);
CREATE INDEX idx_domain_events_aggregate_type ON events.domain_events(aggregate_type);
CREATE INDEX idx_domain_events_event_type ON events.domain_events(event_type);
CREATE INDEX idx_domain_events_occurred_at ON events.domain_events(occurred_at DESC);
CREATE INDEX idx_domain_events_user_id ON events.domain_events(user_id) WHERE user_id IS NOT NULL;

-- GIN index for JSONB queries
CREATE INDEX idx_domain_events_data ON events.domain_events USING GIN(event_data);

COMMENT ON TABLE events.domain_events IS 'Event store for domain events (audit trail and event sourcing)';
COMMENT ON COLUMN events.domain_events.event_data IS 'JSON payload containing event-specific data';

-- Integration Event Log (for RabbitMQ publishing via Outbox pattern)
CREATE TABLE events.integration_event_log (
    id BIGSERIAL PRIMARY KEY,
    event_id UUID UNIQUE NOT NULL DEFAULT uuid_generate_v4(),
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    state event_state DEFAULT 'NotPublished' NOT NULL,
    times_sent INT DEFAULT 0 NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    published_at TIMESTAMPTZ NULL
);

CREATE INDEX idx_integration_event_log_state ON events.integration_event_log(state) WHERE state = 'NotPublished';
CREATE INDEX idx_integration_event_log_created_at ON events.integration_event_log(created_at);

COMMENT ON TABLE events.integration_event_log IS 'Outbox pattern: reliable event publishing to RabbitMQ';
COMMENT ON COLUMN events.integration_event_log.state IS 'Publishing state for retry logic';

-- Outbox Table (transactional event publishing)
CREATE TABLE events.outbox (
    id BIGSERIAL PRIMARY KEY,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    processed_at TIMESTAMPTZ NULL
);

CREATE INDEX idx_outbox_processed_at ON events.outbox(processed_at) WHERE processed_at IS NULL;

COMMENT ON TABLE events.outbox IS 'Outbox pattern table for transactional event publishing';

-- ===== AUDIT LOGS =====

-- Audit Logs Table (for compliance and debugging)
CREATE TABLE events.audit_logs (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID NULL, -- FK to security.users
    action VARCHAR(100) NOT NULL, -- 'Login', 'UpdateProfile', 'StartTrip', 'MakePayment'
    entity_type VARCHAR(50), -- 'User', 'Trip', 'Payment'
    entity_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_audit_logs_user_id ON events.audit_logs(user_id);
CREATE INDEX idx_audit_logs_action ON events.audit_logs(action);
CREATE INDEX idx_audit_logs_entity_type ON events.audit_logs(entity_type);
CREATE INDEX idx_audit_logs_entity_id ON events.audit_logs(entity_id);
CREATE INDEX idx_audit_logs_timestamp ON events.audit_logs(timestamp DESC);

COMMENT ON TABLE events.audit_logs IS 'Audit trail for security, compliance, and debugging';
COMMENT ON COLUMN events.audit_logs.old_values IS 'Previous state (JSON) for UPDATE operations';
COMMENT ON COLUMN events.audit_logs.new_values IS 'New state (JSON) for INSERT/UPDATE operations';
