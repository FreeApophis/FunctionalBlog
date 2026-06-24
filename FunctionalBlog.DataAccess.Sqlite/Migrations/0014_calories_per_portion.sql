-- The calories stat is always normalized per serving, so its label now reads "Kalorien pro Portion"
-- instead of just "Kalorien". Each UPDATE only touches rows still holding the original seeded text,
-- leaving any custom translation edits untouched.

UPDATE translations SET text = 'Kalorien pro Portion'  WHERE key = 'recipe.calories' AND language = 'de' AND text = 'Kalorien';
UPDATE translations SET text = 'Calories per portion'  WHERE key = 'recipe.calories' AND language = 'en' AND text = 'Calories';
UPDATE translations SET text = 'Calorie per porzione'  WHERE key = 'recipe.calories' AND language = 'it' AND text = 'Calorie';
UPDATE translations SET text = 'Calories par portion'  WHERE key = 'recipe.calories' AND language = 'fr' AND text = 'Calories';
