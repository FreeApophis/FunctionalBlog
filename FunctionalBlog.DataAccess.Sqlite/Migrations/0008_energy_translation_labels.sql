-- The ingredient energy column is stored in kilojoules, so its field label and validation
-- message now read "Energie"/"Energy" instead of "Kalorien"/"Calories". This corrects rows that
-- were already seeded with the old wording. Each UPDATE only touches rows still holding the
-- original seeded text, so any custom translation edits are left untouched.

UPDATE translations SET text = 'Energie' WHERE key = 'ingredient.field.calorific_value' AND language = 'de' AND text = 'Kalorien';
UPDATE translations SET text = 'Energy'  WHERE key = 'ingredient.field.calorific_value' AND language = 'en' AND text = 'Calories';
UPDATE translations SET text = 'Energia' WHERE key = 'ingredient.field.calorific_value' AND language = 'it' AND text = 'Calorie';
UPDATE translations SET text = 'Énergie' WHERE key = 'ingredient.field.calorific_value' AND language = 'fr' AND text = 'Calories';

UPDATE translations SET text = 'Der Energiewert muss ≥ 0 sein.'         WHERE key = 'ingredient.error.calorific_value_invalid' AND language = 'de' AND text = 'Der Kalorienwert muss ≥ 0 sein.';
UPDATE translations SET text = 'Energy value must be ≥ 0.'              WHERE key = 'ingredient.error.calorific_value_invalid' AND language = 'en' AND text = 'Calorific value must be ≥ 0.';
UPDATE translations SET text = 'Il valore energetico deve essere ≥ 0.' WHERE key = 'ingredient.error.calorific_value_invalid' AND language = 'it' AND text = 'Il valore calorico deve essere ≥ 0.';
UPDATE translations SET text = 'La valeur énergétique doit être ≥ 0.'  WHERE key = 'ingredient.error.calorific_value_invalid' AND language = 'fr' AND text = 'La valeur calorique doit être ≥ 0.';
