-- migrate:up
CREATE TABLE IF NOT EXISTS departments (
    id UUID NOT NULL,
    identifier VARCHAR(150) NOT NULL,
    name VARCHAR(150) NOT NULL,
    path LTREE NOT NULL,
    depth SMALLINT NOT NULL,
    parent_id UUID,
    childrens_count INTEGER NOT NULL,
    attachments JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT pk_departments PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS idx_department_path ON departments USING GIST (path);

-- migrate:down
DROP INDEX IF EXISTS idx_department_path;

DROP TABLE IF EXISTS departments;
