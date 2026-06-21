-- The foodblog import created two "Apfel" ingredients: id 26 (with real nutritional
-- data) and id 141 (an empty placeholder). The duplicate broke recipe saving and is
-- redundant, so drop the placeholder. No recipe_ingredients reference id 141.
DELETE FROM ingredients WHERE id = 141;
