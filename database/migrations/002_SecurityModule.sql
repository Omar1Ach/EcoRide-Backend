-- Security Module: Users, Wallets, Authentication
-- Schema: security

-- Users Table
CREATE TABLE security.users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(20) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    role user_role DEFAULT 'User' NOT NULL,
    kyc_status kyc_status DEFAULT 'Pending' NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    phone_verified BOOLEAN DEFAULT FALSE NOT NULL,
    email_verified BOOLEAN DEFAULT FALSE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    deleted_at TIMESTAMPTZ NULL
);

-- Indexes for Users
CREATE INDEX idx_users_email ON security.users(email) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_phone ON security.users(phone) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_role ON security.users(role);
CREATE INDEX idx_users_created_at ON security.users(created_at DESC);

COMMENT ON TABLE security.users IS 'User accounts with authentication credentials';
COMMENT ON COLUMN security.users.password_hash IS 'BCrypt hashed password';
COMMENT ON COLUMN security.users.deleted_at IS 'Soft delete timestamp';

-- Wallets Table (1-to-1 with Users)
CREATE TABLE security.wallets (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID UNIQUE NOT NULL REFERENCES security.users(id) ON DELETE CASCADE,
    balance_cents BIGINT DEFAULT 0 NOT NULL CHECK (balance_cents >= 0),
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_wallets_user_id ON security.wallets(user_id);

COMMENT ON TABLE security.wallets IS 'User wallet balances (stored in cents to avoid decimal precision issues)';
COMMENT ON COLUMN security.wallets.balance_cents IS 'Balance in cents (e.g., 3500 = 35 MAD)';

-- OTP Codes Table (Phone Verification)
CREATE TABLE security.otp_codes (
    id BIGSERIAL PRIMARY KEY,
    phone VARCHAR(20) NOT NULL,
    code VARCHAR(6) NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    attempts INT DEFAULT 0 NOT NULL,
    verified BOOLEAN DEFAULT FALSE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

CREATE INDEX idx_otp_phone ON security.otp_codes(phone);
CREATE INDEX idx_otp_expires_at ON security.otp_codes(expires_at);

COMMENT ON TABLE security.otp_codes IS 'One-time passwords for phone verification';
COMMENT ON COLUMN security.otp_codes.attempts IS 'Number of verification attempts (rate limiting)';

-- Refresh Tokens Table (JWT Refresh)
CREATE TABLE security.refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES security.users(id) ON DELETE CASCADE,
    token VARCHAR(500) UNIQUE NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    revoked_at TIMESTAMPTZ NULL
);

CREATE INDEX idx_refresh_tokens_user_id ON security.refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON security.refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_expires_at ON security.refresh_tokens(expires_at) WHERE revoked_at IS NULL;

COMMENT ON TABLE security.refresh_tokens IS 'JWT refresh tokens for session management';
COMMENT ON COLUMN security.refresh_tokens.revoked_at IS 'Timestamp when token was revoked (logout)';

-- Trigger: Update updated_at timestamp
CREATE OR REPLACE FUNCTION security.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_users_updated_at
BEFORE UPDATE ON security.users
FOR EACH ROW
EXECUTE FUNCTION security.update_updated_at_column();

CREATE TRIGGER trigger_wallets_updated_at
BEFORE UPDATE ON security.wallets
FOR EACH ROW
EXECUTE FUNCTION security.update_updated_at_column();
