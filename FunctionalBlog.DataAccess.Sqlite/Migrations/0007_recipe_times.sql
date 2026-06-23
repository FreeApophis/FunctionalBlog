-- Recipes gain three presentation fields shown in the detail stat strip:
-- preparation_time and cooking_time in minutes, calorific_value in kcal.
-- Existing rows default to 0 (unknown).

ALTER TABLE recipes ADD COLUMN preparation_time INTEGER NOT NULL DEFAULT 0;
ALTER TABLE recipes ADD COLUMN cooking_time     INTEGER NOT NULL DEFAULT 0;
ALTER TABLE recipes ADD COLUMN calorific_value  INTEGER NOT NULL DEFAULT 0;
