-- Re-slug existing tags to the canonical transliterated form (mirrors
-- FunctionalBlog.Domain.Tags.Slug) and merge any duplicates this produces.
--
-- Why: migration 0011 originally slugged tags as a plain lowercase of the name, so databases
-- migrated before the slug fix kept umlaut slugs (e.g. "süss"). DbUp never re-runs an applied
-- script, so those slugs were never corrected. Meanwhile 0012 added a correctly-slugged "suess"
-- tag for the macaron article — leaving the recipes on "süss" and the article on "suess", so
-- /tag/suess found only the article. This migration converges them.
--
-- It is a no-op on databases already migrated with the corrected 0011 (every slug is already
-- canonical, so the mapping is the identity and nothing merges).

-- 1. The canonical slug for every existing tag.
CREATE TEMP TABLE _reslug AS
SELECT id AS tag_id,
       replace(
           lower(
               replace(replace(replace(replace(replace(replace(replace(
                   slug,
                   'ä', 'ae'), 'ö', 'oe'), 'ü', 'ue'),
                   'Ä', 'ae'), 'Ö', 'oe'), 'Ü', 'ue'),
                   'ß', 'ss')
           ),
           ' ', '-') AS new_slug
FROM tags;

-- 2. One surviving tag per canonical slug (lowest id wins).
CREATE TEMP TABLE _survivor AS
SELECT new_slug, min(tag_id) AS keep_id
FROM _reslug GROUP BY new_slug;

-- 3. Map every tag to its survivor.
CREATE TEMP TABLE _remap AS
SELECT r.tag_id AS old_id, s.keep_id AS keep_id
FROM _reslug r JOIN _survivor s ON s.new_slug = r.new_slug;

-- 4. Move taggings off the duplicate tags onto the survivor (dedup), then drop the originals.
--    Done explicitly rather than via ON DELETE CASCADE because foreign keys are off during the
--    DbUp migration.
INSERT OR IGNORE INTO taggings (taggable_id, tag_id)
SELECT tg.taggable_id, m.keep_id
FROM taggings tg
JOIN _remap m ON m.old_id = tg.tag_id
WHERE m.old_id <> m.keep_id;

DELETE FROM taggings
WHERE tag_id IN (SELECT old_id FROM _remap WHERE old_id <> keep_id);

-- 5. Delete the now-orphaned duplicate tags.
DELETE FROM tags
WHERE id IN (SELECT old_id FROM _remap WHERE old_id <> keep_id);

-- 6. Point each surviving tag at its canonical slug.
UPDATE tags
SET slug = (SELECT new_slug FROM _reslug WHERE tag_id = tags.id)
WHERE id IN (SELECT keep_id FROM _survivor);

DROP TABLE _reslug;
DROP TABLE _survivor;
DROP TABLE _remap;
