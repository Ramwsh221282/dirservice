-- migrate:up
CREATE TABLE IF NOT EXISTS department_locations (
    department_id UUID NOT NULL,
    location_id UUID NOT NULL,
    CONSTRAINT "PK_department_locations" PRIMARY KEY (department_id, location_id),
    CONSTRAINT "FK_department_locations_departments_department_id"
        FOREIGN KEY (department_id) REFERENCES departments (id) ON DELETE CASCADE,
    CONSTRAINT "FK_department_locations_locations_location_id"
        FOREIGN KEY (location_id) REFERENCES locations (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_department_locations_location_id"
    ON department_locations (location_id);

-- migrate:down
DROP INDEX IF EXISTS "IX_department_locations_location_id";

DROP TABLE IF EXISTS department_locations;
