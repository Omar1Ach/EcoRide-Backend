-- Payment Module: Payments, Transactions, Payment Methods
-- Schema: payment

-- Payments Table
CREATE TABLE payment.payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL, -- FK to security.users
    trip_id UUID NULL, -- FK to trip.trips (nullable for wallet top-ups)
    amount_cents BIGINT NOT NULL CHECK (amount_cents > 0),
    currency VARCHAR(3) DEFAULT 'MAD' NOT NULL,
    payment_method payment_method_type NOT NULL,
    status payment_status DEFAULT 'Pending' NOT NULL,
    provider VARCHAR(50), -- 'Stripe', 'CMI', 'Wallet'
    provider_transaction_id VARCHAR(255),
    provider_customer_id VARCHAR(255), -- Stripe customer ID
    failure_reason VARCHAR(500),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Indexes for Payments
CREATE INDEX idx_payments_user_id ON payment.payments(user_id);
CREATE INDEX idx_payments_trip_id ON payment.payments(trip_id);
CREATE INDEX idx_payments_status ON payment.payments(status);
CREATE INDEX idx_payments_created_at ON payment.payments(created_at DESC);
CREATE INDEX idx_payments_provider_transaction_id ON payment.payments(provider_transaction_id);

COMMENT ON TABLE payment.payments IS 'Payment records for trips and wallet top-ups';
COMMENT ON COLUMN payment.payments.amount_cents IS 'Payment amount in cents';
COMMENT ON COLUMN payment.payments.provider_transaction_id IS 'External payment gateway transaction ID';

-- Payment Transactions Table (captures, refunds, authorizations)
CREATE TABLE payment.transactions (
    id BIGSERIAL PRIMARY KEY,
    payment_id UUID NOT NULL REFERENCES payment.payments(id) ON DELETE CASCADE,
    type transaction_type NOT NULL,
    amount_cents BIGINT,
    status payment_status NOT NULL,
    provider_transaction_id VARCHAR(255),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_transactions_payment_id ON payment.transactions(payment_id);
CREATE INDEX idx_transactions_type ON payment.transactions(type);
CREATE INDEX idx_transactions_created_at ON payment.transactions(created_at DESC);

COMMENT ON TABLE payment.transactions IS 'Detailed payment transaction log (auth, capture, refund)';
COMMENT ON COLUMN payment.transactions.type IS 'Transaction type: Authorization, Capture, or Refund';

-- Payment Methods Table (stored cards)
CREATE TABLE payment.payment_methods (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL, -- FK to security.users
    provider VARCHAR(50) NOT NULL, -- 'Stripe', 'CMI'
    provider_payment_method_id VARCHAR(255) NOT NULL, -- Stripe payment method ID
    type VARCHAR(20) CHECK (type IN ('Card', 'BankAccount')),
    card_brand VARCHAR(20), -- 'Visa', 'Mastercard', 'Amex'
    card_last4 VARCHAR(4),
    card_exp_month INT CHECK (card_exp_month BETWEEN 1 AND 12),
    card_exp_year INT,
    is_default BOOLEAN DEFAULT FALSE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_payment_methods_user_id ON payment.payment_methods(user_id);
CREATE INDEX idx_payment_methods_is_default ON payment.payment_methods(user_id, is_default) WHERE is_default = TRUE;

-- Unique constraint: Only ONE default payment method per user
CREATE UNIQUE INDEX idx_payment_methods_one_default_per_user
ON payment.payment_methods(user_id)
WHERE is_default = TRUE;

COMMENT ON TABLE payment.payment_methods IS 'Stored payment methods (tokenized, PCI-compliant)';
COMMENT ON COLUMN payment.payment_methods.provider_payment_method_id IS 'Stripe payment method token (actual card never stored)';
COMMENT ON COLUMN payment.payment_methods.card_last4 IS 'Last 4 digits for display only';

-- Trigger: Update updated_at timestamp
CREATE TRIGGER trigger_payments_updated_at
BEFORE UPDATE ON payment.payments
FOR EACH ROW
EXECUTE FUNCTION security.update_updated_at_column();

-- View: Daily Revenue
CREATE OR REPLACE VIEW payment.daily_revenue AS
SELECT
    DATE(created_at) AS date,
    COUNT(*) AS payment_count,
    SUM(amount_cents) AS total_revenue_cents,
    AVG(amount_cents) AS avg_payment_cents,
    SUM(CASE WHEN status = 'Succeeded' THEN amount_cents ELSE 0 END) AS successful_revenue_cents,
    SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END) AS failed_payment_count
FROM payment.payments
WHERE trip_id IS NOT NULL -- Exclude wallet top-ups
GROUP BY DATE(created_at)
ORDER BY date DESC;

COMMENT ON VIEW payment.daily_revenue IS 'Daily revenue analytics (trip payments only)';

-- View: Monthly Revenue
CREATE OR REPLACE VIEW payment.monthly_revenue AS
SELECT
    DATE_TRUNC('month', created_at) AS month,
    COUNT(*) AS payment_count,
    SUM(amount_cents) AS total_revenue_cents,
    AVG(amount_cents) AS avg_payment_cents,
    SUM(CASE WHEN status = 'Succeeded' THEN amount_cents ELSE 0 END) AS successful_revenue_cents
FROM payment.payments
WHERE trip_id IS NOT NULL
GROUP BY DATE_TRUNC('month', created_at)
ORDER BY month DESC;

COMMENT ON VIEW payment.monthly_revenue IS 'Monthly revenue analytics';
