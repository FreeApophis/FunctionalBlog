-- Tag the "Macarons selbst backen" blog post with the existing "süss" tag (slug "suess").
-- There is no UI for tagging articles yet, so this is done as a data migration. The tag
-- already exists from the recipe back-fill in 0011; upsert defensively in case it does not.

INSERT INTO tags (slug, name)
SELECT 'suess', 'süss'
WHERE NOT EXISTS (SELECT 1 FROM tags WHERE slug = 'suess');

INSERT OR IGNORE INTO taggings (taggable_id, tag_id)
SELECT a.taggable_id, t.id
FROM articles a, tags t
WHERE a.title = 'Macarons selbst backen' AND t.slug = 'suess';
