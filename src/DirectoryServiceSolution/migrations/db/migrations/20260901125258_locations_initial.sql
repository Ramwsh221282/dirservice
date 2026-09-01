-- migrate:up
CREATE TABLE IF NOT EXISTS locations (
    id UUID NOT NULL,
    address JSONB NOT NULL,
    name VARCHAR(120) NOT NULL,
    time_zone TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT pk_locations PRIMARY KEY (id)
);

-- migrate:down
DROP TABLE IF EXISTS locations;
