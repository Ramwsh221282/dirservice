-- migrate:up
CREATE TABLE IF NOT EXISTS department_positions (
    department_id UUID NOT NULL,
    position_id UUID NOT NULL,
    CONSTRAINT "PK_department_positions" PRIMARY KEY (department_id, position_id),
    CONSTRAINT "FK_department_positions_departments_department_id"
        FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE,
    CONSTRAINT "FK_department_positions_positions_position_id"
        FOREIGN KEY (position_id) REFERENCES positions (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_department_positions_position_id"
    ON department_positions (position_id);

-- migrate:down
DROP INDEX IF EXISTS "IX_department_positions_position_id";

DROP TABLE IF EXISTS department_positions;
