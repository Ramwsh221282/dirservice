-- migrate:up
CREATE TABLE IF NOT EXISTS users (
    id UUID NOT NULL,
    login TEXT NOT NULL,
    password TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT pk_users PRIMARY KEY (id),
    CONSTRAINT uq_users_login UNIQUE (login)
);

-- migrate:down
DROP TABLE IF EXISTS users;
