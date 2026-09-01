-- migrate:up
CREATE TABLE IF NOT EXISTS positions (
    id UUID NOT NULL,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(1000) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT pk_positions PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_positions_name" ON positions (name);

-- migrate:down
DROP INDEX IF EXISTS "IX_positions_name";

DROP TABLE IF EXISTS positions;
