-- Units become a database-driven, editable catalog.
-- category: 0 = Weight, 1 = Volume, 2 = Piece.
-- unit_factor is relative to the category base (kg = 1, l = 1, piece = 1).
-- name_key / abbreviation_key reference the translations table (convention unit.<id>.name / .abbr).

CREATE TABLE units (
    id               INTEGER PRIMARY KEY,
    category         INTEGER NOT NULL,
    unit_factor      REAL    NOT NULL,
    name_key         TEXT    NOT NULL,
    abbreviation_key TEXT    NOT NULL
);

INSERT INTO units (id, category, unit_factor, name_key, abbreviation_key) VALUES
    (1,  0, 0.001,    'unit.1.name',  'unit.1.abbr'),
    (2,  0, 1,        'unit.2.name',  'unit.2.abbr'),
    (3,  1, 0.001,    'unit.3.name',  'unit.3.abbr'),
    (4,  1, 1,        'unit.4.name',  'unit.4.abbr'),
    (5,  1, 0.000625, 'unit.5.name',  'unit.5.abbr'),
    (6,  1, 0.005,    'unit.6.name',  'unit.6.abbr'),
    (7,  1, 0.015,    'unit.7.name',  'unit.7.abbr'),
    (8,  1, 0.25,     'unit.8.name',  'unit.8.abbr'),
    (9,  2, 1,        'unit.9.name',  'unit.9.abbr'),
    (10, 2, 12,       'unit.10.name', 'unit.10.abbr'),
    (11, 0, 0.000001, 'unit.11.name', 'unit.11.abbr'),
    (12, 0, 0.01,     'unit.12.name', 'unit.12.abbr'),
    (13, 1, 0.000001, 'unit.13.name', 'unit.13.abbr'),
    (14, 1, 0.01,     'unit.14.name', 'unit.14.abbr'),
    (15, 1, 0.1,      'unit.15.name', 'unit.15.abbr'),
    (16, 1, 0.00005,  'unit.16.name', 'unit.16.abbr'),
    (17, 1, 0.0006,   'unit.17.name', 'unit.17.abbr'),
    (18, 1, 0.025,    'unit.18.name', 'unit.18.abbr'),
    (19, 1, 0.00025,  'unit.19.name', 'unit.19.abbr'),
    (20, 2, 144,      'unit.20.name', 'unit.20.abbr'),
    (21, 2, 1,        'unit.21.name', 'unit.21.abbr'),
    (22, 2, 1,        'unit.22.name', 'unit.22.abbr'),
    (23, 2, 1,        'unit.23.name', 'unit.23.abbr'),
    (24, 2, 1,        'unit.24.name', 'unit.24.abbr');

-- The German name/abbreviation text for each unit is seeded by TranslationSeeder at startup
-- (alongside the rest of the translations), so a freshly migrated database stays free of data.

-- Switch recipe_ingredients from a free-text abbreviation to a unit_id foreign key.
-- Rebuild the table (rather than DROP COLUMN) so unit_id can be NOT NULL with a FK.
CREATE TABLE recipe_ingredients_new (
    recipe_id     INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    sort_order    INTEGER NOT NULL,
    ingredient_id INTEGER NOT NULL,
    amount        REAL    NOT NULL,
    unit_id       INTEGER NOT NULL REFERENCES units(id),
    PRIMARY KEY (recipe_id, sort_order)
);

INSERT INTO recipe_ingredients_new (recipe_id, sort_order, ingredient_id, amount, unit_id)
SELECT recipe_id, sort_order, ingredient_id, amount,
    CASE unit_abbreviation
        WHEN 'g'     THEN 1
        WHEN 'kg'    THEN 2
        WHEN 'ml'    THEN 3
        WHEN 'l'     THEN 4
        WHEN 'dl'    THEN 15
        WHEN 'EL'    THEN 7
        WHEN 'TL'    THEN 6
        WHEN 'Prise' THEN 5
        WHEN 'Stück' THEN 9
        ELSE 9
    END
FROM recipe_ingredients;

DROP TABLE recipe_ingredients;
ALTER TABLE recipe_ingredients_new RENAME TO recipe_ingredients;
