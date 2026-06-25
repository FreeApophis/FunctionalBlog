-- Phase 7: fold tag URL slugs into the central `slugs` registry and drop the per-tag `slug` column,
-- so every entity type (article, recipe, page, ingredient, tag) shares the one slug namespace.
--
-- Tags join the global namespace (slug is the table's PRIMARY KEY — one slug per entity across all
-- types). The existing clean tag slugs are copied across verbatim; any that collide with a content
-- slug already registered in `slugs` are skipped here (INSERT OR IGNORE) and re-assigned a suffixed
-- slug by the application's startup SlugBackfill. In a freshly-migrated database the registry is
-- still empty at this point, so every tag slug copies in cleanly.

INSERT OR IGNORE INTO slugs (slug, entity_type, entity_id)
SELECT slug, 'tag', id FROM tags;

-- Rebuild `tags` without the slug column. Foreign keys are ON during the DbUp migration, so dropping
-- `tags` performs an implicit DELETE that cascades into `taggings` (tag_id REFERENCES tags ON DELETE
-- CASCADE). Snapshot and restore the taggings around the rebuild, exactly as 0011 does for recipe
-- children.
CREATE TEMP TABLE _taggings AS SELECT * FROM taggings;

CREATE TABLE tags_new (
    id   INTEGER PRIMARY KEY,
    name TEXT    NOT NULL
);

INSERT INTO tags_new (id, name) SELECT id, name FROM tags;

DROP TABLE tags;
ALTER TABLE tags_new RENAME TO tags;

CREATE INDEX ix_tags_name ON tags (name);

-- Restore the taggings the cascade emptied (tag ids are preserved, so the FKs still resolve).
DELETE FROM taggings;
INSERT INTO taggings SELECT * FROM _taggings;

DROP TABLE _taggings;
