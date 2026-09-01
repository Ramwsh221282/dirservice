-- migrate:up
CREATE TABLE IF NOT EXISTS sessions (
    session_id UUID NOT NULL,
    user_id UUID NOT NULL,
    refresh_token TEXT,
    access_token TEXT,
    created_at TIMESTAMPTZ NOT NULL,
    access_token_expires_at TIMESTAMPTZ NOT NULL,
    refresh_token_expires_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT pk_sessions PRIMARY KEY (session_id),
    CONSTRAINT fk_sessions_users_user_id
        FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_sessions_user_id ON sessions (user_id);

CREATE INDEX IF NOT EXISTS idx_sessions_refresh_token ON sessions (refresh_token);

CREATE INDEX IF NOT EXISTS idx_sessions_access_token_expires_at
    ON sessions (access_token_expires_at)
    WHERE access_token IS NOT NULL;

-- migrate:down
DROP INDEX IF EXISTS idx_sessions_access_token_expires_at;

DROP INDEX IF EXISTS idx_sessions_refresh_token;

DROP INDEX IF EXISTS idx_sessions_user_id;

DROP TABLE IF EXISTS sessions;
