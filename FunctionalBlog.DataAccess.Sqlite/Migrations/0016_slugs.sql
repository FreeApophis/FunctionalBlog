-- Central registry mapping a globally-unique URL slug to the entity that owns it.
-- One current slug per (entity_type, entity_id); renames replace the row (no history).
-- Slugs are generated in application code (FunctionalBlog.Domain.Tags.Slug) and backfilled
-- at startup, so the Unicode normalization never has to be reimplemented in SQL.
CREATE TABLE slugs (
    slug        TEXT    NOT NULL PRIMARY KEY,
    entity_type TEXT    NOT NULL,
    entity_id   INTEGER NOT NULL
);

CREATE UNIQUE INDEX ux_slugs_entity ON slugs (entity_type, entity_id);
