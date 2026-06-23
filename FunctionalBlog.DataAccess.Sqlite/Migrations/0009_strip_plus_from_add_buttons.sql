-- The "+ Zutat" / "+ Schritt" add buttons already render a plus icon (SVG), so the leading "+ "
-- in the label text produced a duplicated plus. Drop the prefix from already-seeded rows. Each
-- UPDATE only touches rows still holding the original seeded text, so custom edits are preserved.

UPDATE translations SET text = 'Zutat'       WHERE key = 'recipe.add_ingredient' AND language = 'de' AND text = '+ Zutat';
UPDATE translations SET text = 'Ingredient'  WHERE key = 'recipe.add_ingredient' AND language = 'en' AND text = '+ Ingredient';
UPDATE translations SET text = 'Ingrediente' WHERE key = 'recipe.add_ingredient' AND language = 'it' AND text = '+ Ingrediente';
UPDATE translations SET text = 'Ingrédient'  WHERE key = 'recipe.add_ingredient' AND language = 'fr' AND text = '+ Ingrédient';

UPDATE translations SET text = 'Schritt'   WHERE key = 'recipe.add_step' AND language = 'de' AND text = '+ Schritt';
UPDATE translations SET text = 'Step'      WHERE key = 'recipe.add_step' AND language = 'en' AND text = '+ Step';
UPDATE translations SET text = 'Passaggio' WHERE key = 'recipe.add_step' AND language = 'it' AND text = '+ Passaggio';
UPDATE translations SET text = 'Étape'     WHERE key = 'recipe.add_step' AND language = 'fr' AND text = '+ Étape';
