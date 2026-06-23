-- Normalize tags into a polymorphic, integer-keyed model.
--
--   articles.taggable_id ─┐
--                         ├─→ taggables ──< taggings >── tags
--   recipes.taggable_id ──┘        (every join is on INTEGER)
--
-- Each taggable table owns a 1:1 FK into a shared `taggables` identity pool, so a
-- tagging never has to know — or join on — whether its owner is an article or a recipe.
-- This replaces the old free-text, recipe-only `recipe_tags` table.
--
-- Adding a NOT NULL FK column requires rebuilding the parent `articles`/`recipes`
-- tables. Foreign keys are ON during the DbUp migration (and PRAGMA foreign_keys=OFF is
-- a no-op inside its transaction), so dropping `recipes` performs an implicit delete that
-- cascades into its child tables. We therefore snapshot the recipe children first and
-- restore them after the rebuild.

CREATE TABLE taggables (
    id INTEGER PRIMARY KEY
);

CREATE TABLE tags (
    id   INTEGER PRIMARY KEY,
    slug TEXT    NOT NULL UNIQUE,
    name TEXT    NOT NULL
);

CREATE TABLE taggings (
    taggable_id INTEGER NOT NULL REFERENCES taggables(id) ON DELETE CASCADE,
    tag_id      INTEGER NOT NULL REFERENCES tags(id)      ON DELETE CASCADE,
    PRIMARY KEY (taggable_id, tag_id)
);

-- Allocate one fresh taggable id per existing article and recipe.
CREATE TEMP TABLE _taggable_map AS
SELECT kind, entity_id, ROW_NUMBER() OVER (ORDER BY kind, entity_id) AS taggable_id
FROM (
    SELECT 'article' AS kind, id AS entity_id FROM articles
    UNION ALL
    SELECT 'recipe'  AS kind, id AS entity_id FROM recipes
);

INSERT INTO taggables (id) SELECT taggable_id FROM _taggable_map ORDER BY taggable_id;

-- Migrate the existing free-text recipe tags into the normalized tables while
-- `recipe_tags` and `recipes` are still intact. The slug mirrors
-- FunctionalBlog.Domain.Tags.Slug: transliterate ä→ae ö→oe ü→ue ß→ss, lowercase, and
-- turn spaces into hyphens (e.g. "süss" → "suess", "Schweizer Küche" → "schweizer-kueche").
CREATE TEMP TABLE _tag_slugs AS
SELECT
    rt.recipe_id AS recipe_id,
    trim(rt.tag) AS name,
    replace(
        lower(
            replace(replace(replace(replace(replace(replace(replace(
                trim(rt.tag),
                'ä', 'ae'), 'ö', 'oe'), 'ü', 'ue'),
                'Ä', 'ae'), 'Ö', 'oe'), 'Ü', 'ue'),
                'ß', 'ss')
        ),
        ' ', '-') AS slug
FROM recipe_tags rt
WHERE trim(rt.tag) <> '';

-- name keeps a representative original spelling for the deduplicated slug.
INSERT INTO tags (slug, name)
SELECT slug, min(name) FROM _tag_slugs GROUP BY slug;

INSERT INTO taggings (taggable_id, tag_id)
SELECT DISTINCT m.taggable_id, t.id
FROM _tag_slugs ts
JOIN _taggable_map m ON m.kind = 'recipe' AND m.entity_id = ts.recipe_id
JOIN tags t ON t.slug = ts.slug;

-- Snapshot recipe child rows; dropping `recipes` below cascade-deletes them.
CREATE TEMP TABLE _steps       AS SELECT * FROM recipe_steps;
CREATE TEMP TABLE _ingredients AS SELECT * FROM recipe_ingredients;
CREATE TEMP TABLE _images      AS SELECT * FROM recipe_images;
CREATE TEMP TABLE _hints       AS SELECT * FROM recipe_hints;

-- Rebuild `articles` with a 1:1 taggable_id. Articles have no FK children to cascade.
CREATE TABLE articles_new (
    id             INTEGER PRIMARY KEY,
    title          TEXT    NOT NULL,
    teaser         TEXT    NOT NULL,
    text           TEXT    NOT NULL,
    author_id      INTEGER NOT NULL,
    published_at   TEXT    NOT NULL,
    cover_image_id INTEGER NULL,
    taggable_id    INTEGER NOT NULL UNIQUE REFERENCES taggables(id)
);

INSERT INTO articles_new (id, title, teaser, text, author_id, published_at, cover_image_id, taggable_id)
SELECT a.id, a.title, a.teaser, a.text, a.author_id, a.published_at, a.cover_image_id, m.taggable_id
FROM articles a
JOIN _taggable_map m ON m.kind = 'article' AND m.entity_id = a.id;

DROP TABLE articles;
ALTER TABLE articles_new RENAME TO articles;

-- Rebuild `recipes` the same way.
CREATE TABLE recipes_new (
    id               INTEGER PRIMARY KEY,
    name             TEXT    NOT NULL,
    description      TEXT    NOT NULL,
    author_id        INTEGER NOT NULL,
    difficulty       INTEGER NOT NULL,
    portions         INTEGER NOT NULL,
    preparation_time INTEGER NOT NULL DEFAULT 0,
    cooking_time     INTEGER NOT NULL DEFAULT 0,
    calorific_value  INTEGER NOT NULL DEFAULT 0,
    taggable_id      INTEGER NOT NULL UNIQUE REFERENCES taggables(id)
);

INSERT INTO recipes_new (id, name, description, author_id, difficulty, portions, preparation_time, cooking_time, calorific_value, taggable_id)
SELECT r.id, r.name, r.description, r.author_id, r.difficulty, r.portions, r.preparation_time, r.cooking_time, r.calorific_value, m.taggable_id
FROM recipes r
JOIN _taggable_map m ON m.kind = 'recipe' AND m.entity_id = r.id;

DROP TABLE recipes;
ALTER TABLE recipes_new RENAME TO recipes;

-- Restore the recipe children that the cascade emptied.
INSERT INTO recipe_steps       SELECT * FROM _steps;
INSERT INTO recipe_ingredients SELECT * FROM _ingredients;
INSERT INTO recipe_images      SELECT * FROM _images;
INSERT INTO recipe_hints       SELECT * FROM _hints;

DROP TABLE recipe_tags;
DROP TABLE _tag_slugs;
DROP TABLE _steps;
DROP TABLE _ingredients;
DROP TABLE _images;
DROP TABLE _hints;
DROP TABLE _taggable_map;
