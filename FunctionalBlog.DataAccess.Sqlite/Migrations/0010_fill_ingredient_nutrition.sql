-- Fill nutrition for ingredients that were imported with placeholder energy. calorific_value is in
-- kilojoules per 100 g; protein/fat/carbohydrates/sugar/fiber are grams per 100 g. Values are
-- typical reference figures for the foodstuff. Each UPDATE is guarded by `calorific_value <= 1`, so
-- only placeholder rows are filled — rows already carrying real values (or edited in the admin UI)
-- are left untouched. piece_count and density are intentionally not changed.

UPDATE ingredients SET calorific_value = 2850, protein = 1,  fat = 75, carbohydrates = 2,  sugar = 1,  fiber = 0  WHERE id = 1   AND calorific_value <= 1; -- Mayonnaise
UPDATE ingredients SET calorific_value = 540,  protein = 0,  fat = 0,  carbohydrates = 36, sugar = 30, fiber = 1  WHERE id = 2   AND calorific_value <= 1; -- Sweet Pickle Relish [estimate]
UPDATE ingredients SET calorific_value = 450,  protein = 6,  fat = 4,  carbohydrates = 9,  sugar = 5,  fiber = 3  WHERE id = 3   AND calorific_value <= 1; -- Senf
UPDATE ingredients SET calorific_value = 90,   protein = 0,  fat = 0,  carbohydrates = 1,  sugar = 1,  fiber = 0  WHERE id = 4   AND calorific_value <= 1; -- Weissweinessig
UPDATE ingredients SET calorific_value = 1390, protein = 17, fat = 1,  carbohydrates = 73, sugar = 2,  fiber = 9  WHERE id = 5   AND calorific_value <= 1; -- Knoblauchpulver
UPDATE ingredients SET calorific_value = 1430, protein = 10, fat = 1,  carbohydrates = 79, sugar = 7,  fiber = 15 WHERE id = 6   AND calorific_value <= 1; -- Zwiebelpulver
UPDATE ingredients SET calorific_value = 1180, protein = 14, fat = 13, carbohydrates = 54, sugar = 10, fiber = 35 WHERE id = 7   AND calorific_value <= 1; -- Paprikapulver
UPDATE ingredients SET calorific_value = 1300, protein = 10, fat = 3,  carbohydrates = 65, sugar = 3,  fiber = 21 WHERE id = 8   AND calorific_value <= 1; -- Kurkuma
UPDATE ingredients SET calorific_value = 0,    protein = 0,  fat = 0,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 9   AND calorific_value <= 1; -- Salz
UPDATE ingredients SET calorific_value = 1050, protein = 18, fat = 20, carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 10  AND calorific_value <= 1; -- Hackfleisch (Rind)
UPDATE ingredients SET calorific_value = 1650, protein = 13, fat = 5,  carbohydrates = 72, sugar = 6,  fiber = 4  WHERE id = 14  AND calorific_value <= 1; -- Paniermehl
UPDATE ingredients SET calorific_value = 1120, protein = 9,  fat = 3,  carbohydrates = 49, sugar = 3,  fiber = 3  WHERE id = 15  AND calorific_value <= 1; -- Semmel
UPDATE ingredients SET calorific_value = 420,  protein = 6,  fat = 2,  carbohydrates = 24, sugar = 0,  fiber = 14 WHERE id = 17  AND calorific_value <= 1; -- Thymian (frisch)
UPDATE ingredients SET calorific_value = 1050, protein = 10, fat = 3,  carbohydrates = 64, sugar = 1,  fiber = 25 WHERE id = 18  AND calorific_value <= 1; -- Pfeffer
UPDATE ingredients SET calorific_value = 150,  protein = 3,  fat = 1,  carbohydrates = 6,  sugar = 1,  fiber = 3  WHERE id = 19  AND calorific_value <= 1; -- Petersilie
UPDATE ingredients SET calorific_value = 125,  protein = 3,  fat = 1,  carbohydrates = 4,  sugar = 2,  fiber = 3  WHERE id = 20  AND calorific_value <= 1; -- Schnittlauch
UPDATE ingredients SET calorific_value = 1140, protein = 13, fat = 7,  carbohydrates = 60, sugar = 4,  fiber = 40 WHERE id = 21  AND calorific_value <= 1; -- Majoran (getrocknet)
UPDATE ingredients SET calorific_value = 625,  protein = 6,  fat = 1,  carbohydrates = 33, sugar = 1,  fiber = 2  WHERE id = 23  AND calorific_value <= 1; -- Knoblauchzehe
UPDATE ingredients SET calorific_value = 150,  protein = 2,  fat = 1,  carbohydrates = 6,  sugar = 3,  fiber = 2  WHERE id = 24  AND calorific_value <= 1; -- Sambal Oelek [estimate]
UPDATE ingredients SET calorific_value = 0,    protein = 0,  fat = 0,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 27  AND calorific_value <= 1; -- Wasser
UPDATE ingredients SET calorific_value = 1050, protein = 0,  fat = 0,  carbohydrates = 62, sugar = 60, fiber = 1  WHERE id = 28  AND calorific_value <= 1; -- Erdbeermarmelade
UPDATE ingredients SET calorific_value = 95,   protein = 0,  fat = 0,  carbohydrates = 7,  sugar = 3,  fiber = 0  WHERE id = 31  AND calorific_value <= 1; -- Zitronensaft
UPDATE ingredients SET calorific_value = 120,  protein = 1,  fat = 0,  carbohydrates = 9,  sugar = 3,  fiber = 3  WHERE id = 32  AND calorific_value <= 1; -- Zitrone
UPDATE ingredients SET calorific_value = 490,  protein = 18, fat = 4,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 33  AND calorific_value <= 1; -- Räucherlachs
UPDATE ingredients SET calorific_value = 180,  protein = 3,  fat = 1,  carbohydrates = 7,  sugar = 0,  fiber = 2  WHERE id = 35  AND calorific_value <= 1; -- Dill
UPDATE ingredients SET calorific_value = 135,  protein = 1,  fat = 0,  carbohydrates = 8,  sugar = 5,  fiber = 2  WHERE id = 36  AND calorific_value <= 1; -- Erdbeeren
UPDATE ingredients SET calorific_value = 1560, protein = 13, fat = 2,  carbohydrates = 75, sugar = 3,  fiber = 3  WHERE id = 37  AND calorific_value <= 1; -- Spaghetti
UPDATE ingredients SET calorific_value = 870,  protein = 20, fat = 13, carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 38  AND calorific_value <= 1; -- Lachsfilet
UPDATE ingredients SET calorific_value = 1240, protein = 2,  fat = 30, carbohydrates = 3,  sugar = 3,  fiber = 0  WHERE id = 39  AND calorific_value <= 1; -- Creme Fraiche
UPDATE ingredients SET calorific_value = 1400, protein = 2,  fat = 35, carbohydrates = 3,  sugar = 3,  fiber = 0  WHERE id = 42  AND calorific_value <= 1; -- Rahm
UPDATE ingredients SET calorific_value = 550,  protein = 3,  fat = 6,  carbohydrates = 20, sugar = 0,  fiber = 14 WHERE id = 43  AND calorific_value <= 1; -- Rosmarin (frisch)
UPDATE ingredients SET calorific_value = 1470, protein = 25, fat = 27, carbohydrates = 1,  sugar = 1,  fiber = 0  WHERE id = 45  AND calorific_value <= 1; -- Käse zum Überbacken [estimate]
UPDATE ingredients SET calorific_value = 1040, protein = 3,  fat = 24, carbohydrates = 3,  sugar = 2,  fiber = 0  WHERE id = 46  AND calorific_value <= 1; -- Creme Fraiche mit Kräutern
UPDATE ingredients SET calorific_value = 25,   protein = 0,  fat = 0,  carbohydrates = 1,  sugar = 0,  fiber = 0  WHERE id = 47  AND calorific_value <= 1; -- Gemüsebouillon [prepared/estimate]
UPDATE ingredients SET calorific_value = 460,  protein = 1,  fat = 0,  carbohydrates = 26, sugar = 22, fiber = 0  WHERE id = 48  AND calorific_value <= 1; -- Ketchup
UPDATE ingredients SET calorific_value = 330,  protein = 1,  fat = 0,  carbohydrates = 19, sugar = 15, fiber = 0  WHERE id = 49  AND calorific_value <= 1; -- Worcestershiresauce
UPDATE ingredients SET calorific_value = 1370, protein = 13, fat = 14, carbohydrates = 56, sugar = 3,  fiber = 33 WHERE id = 50  AND calorific_value <= 1; -- Currypulver
UPDATE ingredients SET calorific_value = 380,  protein = 0,  fat = 0,  carbohydrates = 19, sugar = 15, fiber = 0  WHERE id = 51  AND calorific_value <= 1; -- weisser Balsamico
UPDATE ingredients SET calorific_value = 50,   protein = 1,  fat = 1,  carbohydrates = 1,  sugar = 0,  fiber = 1  WHERE id = 52  AND calorific_value <= 1; -- Tabascosauce
UPDATE ingredients SET calorific_value = 1270, protein = 0,  fat = 0,  carbohydrates = 82, sugar = 82, fiber = 0  WHERE id = 53  AND calorific_value <= 1; -- Honig
UPDATE ingredients SET calorific_value = 200,  protein = 2,  fat = 0,  carbohydrates = 16, sugar = 4,  fiber = 10 WHERE id = 55  AND calorific_value <= 1; -- Zitronenschale
UPDATE ingredients SET calorific_value = 220,  protein = 11, fat = 0,  carbohydrates = 1,  sugar = 1,  fiber = 0  WHERE id = 58  AND calorific_value <= 1; -- Eiweiss
UPDATE ingredients SET calorific_value = 1350, protein = 16, fat = 27, carbohydrates = 4,  sugar = 1,  fiber = 0  WHERE id = 59  AND calorific_value <= 1; -- Eigelb
UPDATE ingredients SET calorific_value = 1040, protein = 4,  fat = 1,  carbohydrates = 81, sugar = 2,  fiber = 53 WHERE id = 60  AND calorific_value <= 1; -- Zimt
UPDATE ingredients SET calorific_value = 90,   protein = 1,  fat = 0,  carbohydrates = 4,  sugar = 1,  fiber = 2  WHERE id = 61  AND calorific_value <= 1; -- Rhabarber
UPDATE ingredients SET calorific_value = 2280, protein = 6,  fat = 38, carbohydrates = 46, sugar = 35, fiber = 11 WHERE id = 62  AND calorific_value <= 1; -- Zartbitterschokolade
UPDATE ingredients SET calorific_value = 2240, protein = 8,  fat = 30, carbohydrates = 59, sugar = 52, fiber = 3  WHERE id = 63  AND calorific_value <= 1; -- Vollmilchschokolade
UPDATE ingredients SET calorific_value = 2890, protein = 9,  fat = 72, carbohydrates = 14, sugar = 4,  fiber = 10 WHERE id = 64  AND calorific_value <= 1; -- Pekannüsse
UPDATE ingredients SET calorific_value = 85,   protein = 1,  fat = 0,  carbohydrates = 5,  sugar = 2,  fiber = 2  WHERE id = 66  AND calorific_value <= 1; -- grüne Paprika
UPDATE ingredients SET calorific_value = 67,   protein = 1,  fat = 0,  carbohydrates = 3,  sugar = 1,  fiber = 2  WHERE id = 67  AND calorific_value <= 1; -- Stangen Sellerie
UPDATE ingredients SET calorific_value = 1250, protein = 16, fat = 26, carbohydrates = 2,  sugar = 0,  fiber = 0  WHERE id = 68  AND calorific_value <= 1; -- Andouille [estimate]
UPDATE ingredients SET calorific_value = 1310, protein = 8,  fat = 8,  carbohydrates = 75, sugar = 0,  fiber = 26 WHERE id = 69  AND calorific_value <= 1; -- Lorbeerblätter
UPDATE ingredients SET calorific_value = 1330, protein = 12, fat = 17, carbohydrates = 57, sugar = 10, fiber = 27 WHERE id = 71  AND calorific_value <= 1; -- Cayennepfeffer
UPDATE ingredients SET calorific_value = 1110, protein = 9,  fat = 4,  carbohydrates = 69, sugar = 4,  fiber = 43 WHERE id = 72  AND calorific_value <= 1; -- Oregano (getrocknet)
UPDATE ingredients SET calorific_value = 690,  protein = 20, fat = 9,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 73  AND calorific_value <= 1; -- Hähnchenfleisch
UPDATE ingredients SET calorific_value = 25,   protein = 1,  fat = 0,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 74  AND calorific_value <= 1; -- Fleischbouillon [prepared/estimate]
UPDATE ingredients SET calorific_value = 345,  protein = 4,  fat = 1,  carbohydrates = 19, sugar = 12, fiber = 4  WHERE id = 76  AND calorific_value <= 1; -- Tomatenmark
UPDATE ingredients SET calorific_value = 1610, protein = 0,  fat = 0,  carbohydrates = 91, sugar = 0,  fiber = 1  WHERE id = 77  AND calorific_value <= 1; -- Speisestärke
UPDATE ingredients SET calorific_value = 75,   protein = 1,  fat = 0,  carbohydrates = 4,  sugar = 3,  fiber = 1  WHERE id = 78  AND calorific_value <= 1; -- Tomaten
UPDATE ingredients SET calorific_value = 70,   protein = 1,  fat = 0,  carbohydrates = 3,  sugar = 2,  fiber = 1  WHERE id = 80  AND calorific_value <= 1; -- Zucchini
UPDATE ingredients SET calorific_value = 1530, protein = 7,  fat = 1,  carbohydrates = 79, sugar = 0,  fiber = 1  WHERE id = 81  AND calorific_value <= 1; -- Langkornreis (ungekocht)
UPDATE ingredients SET calorific_value = 340,  protein = 0,  fat = 0,  carbohydrates = 3,  sugar = 1,  fiber = 0  WHERE id = 82  AND calorific_value <= 1; -- Weisswein
UPDATE ingredients SET calorific_value = 1640, protein = 27, fat = 30, carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 83  AND calorific_value <= 1; -- geriebener Käse [estimate]
UPDATE ingredients SET calorific_value = 0,    protein = 0,  fat = 0,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 84  AND calorific_value <= 1; -- spritziges Mineralwasser
UPDATE ingredients SET calorific_value = 955,  protein = 20, fat = 14, carbohydrates = 58, sugar = 2,  fiber = 37 WHERE id = 85  AND calorific_value <= 1; -- Backkakao
UPDATE ingredients SET calorific_value = 400,  protein = 12, fat = 4,  carbohydrates = 4,  sugar = 4,  fiber = 0  WHERE id = 86  AND calorific_value <= 1; -- Halbfettquark
UPDATE ingredients SET calorific_value = 1490, protein = 1,  fat = 1,  carbohydrates = 85, sugar = 0,  fiber = 0  WHERE id = 87  AND calorific_value <= 1; -- Vanillepuddingpulver
UPDATE ingredients SET calorific_value = 1490, protein = 7,  fat = 1,  carbohydrates = 78, sugar = 0,  fiber = 1  WHERE id = 88  AND calorific_value <= 1; -- Sushireis
UPDATE ingredients SET calorific_value = 90,   protein = 0,  fat = 0,  carbohydrates = 1,  sugar = 1,  fiber = 0  WHERE id = 89  AND calorific_value <= 1; -- Reisessig [ungesüsst/estimate]
UPDATE ingredients SET calorific_value = 135,  protein = 2,  fat = 0,  carbohydrates = 7,  sugar = 2,  fiber = 3  WHERE id = 90  AND calorific_value <= 1; -- Frühlingszwiebeln
UPDATE ingredients SET calorific_value = 650,  protein = 3,  fat = 15, carbohydrates = 4,  sugar = 4,  fiber = 0  WHERE id = 91  AND calorific_value <= 1; -- saure Sahne [~15% Fett/estimate]
UPDATE ingredients SET calorific_value = 1100, protein = 14, fat = 21, carbohydrates = 4,  sugar = 1,  fiber = 0  WHERE id = 92  AND calorific_value <= 1; -- Feta
UPDATE ingredients SET calorific_value = 85,   protein = 0,  fat = 0,  carbohydrates = 1,  sugar = 0,  fiber = 0  WHERE id = 104 AND calorific_value <= 1; -- Essig
UPDATE ingredients SET calorific_value = 600,  protein = 1,  fat = 15, carbohydrates = 4,  sugar = 0,  fiber = 3  WHERE id = 107 AND calorific_value <= 1; -- Oliven
UPDATE ingredients SET calorific_value = 1790, protein = 5,  fat = 44, carbohydrates = 4,  sugar = 2,  fiber = 0  WHERE id = 113 AND calorific_value <= 1; -- Mascarpone
UPDATE ingredients SET calorific_value = 190,  protein = 1,  fat = 1,  carbohydrates = 8,  sugar = 6,  fiber = 4  WHERE id = 114 AND calorific_value <= 1; -- Waldbeeren (frisch oder TK)
UPDATE ingredients SET calorific_value = 125,  protein = 1,  fat = 0,  carbohydrates = 11, sugar = 2,  fiber = 3  WHERE id = 117 AND calorific_value <= 1; -- Limette
UPDATE ingredients SET calorific_value = 1400, protein = 86, fat = 0,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 118 AND calorific_value <= 1; -- Gelatine
UPDATE ingredients SET calorific_value = 220,  protein = 1,  fat = 1,  carbohydrates = 12, sugar = 4,  fiber = 7  WHERE id = 119 AND calorific_value <= 1; -- Himbeeren (frisch oder TK)
UPDATE ingredients SET calorific_value = 105,  protein = 0,  fat = 0,  carbohydrates = 8,  sugar = 2,  fiber = 0  WHERE id = 120 AND calorific_value <= 1; -- Limettensaft
UPDATE ingredients SET calorific_value = 67,   protein = 1,  fat = 0,  carbohydrates = 3,  sugar = 1,  fiber = 1  WHERE id = 121 AND calorific_value <= 1; -- Chinakohl
UPDATE ingredients SET calorific_value = 2460, protein = 25, fat = 50, carbohydrates = 20, sugar = 9,  fiber = 6  WHERE id = 122 AND calorific_value <= 1; -- Erdnussbutter
UPDATE ingredients SET calorific_value = 630,  protein = 10, fat = 5,  carbohydrates = 15, sugar = 5,  fiber = 10 WHERE id = 129 AND calorific_value <= 1; -- Fleischgewürz [estimate]
UPDATE ingredients SET calorific_value = 690,  protein = 22, fat = 8,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 131 AND calorific_value <= 1; -- Hähnchen-Minutenfilets
UPDATE ingredients SET calorific_value = 295,  protein = 0,  fat = 0,  carbohydrates = 18, sugar = 16, fiber = 1  WHERE id = 132 AND calorific_value <= 1; -- Fruchtcocktail (Konserve)
UPDATE ingredients SET calorific_value = 1180, protein = 14, fat = 13, carbohydrates = 54, sugar = 10, fiber = 35 WHERE id = 133 AND calorific_value <= 1; -- Paprika Edelsüss
UPDATE ingredients SET calorific_value = 1180, protein = 14, fat = 13, carbohydrates = 54, sugar = 10, fiber = 35 WHERE id = 134 AND calorific_value <= 1; -- Paprika Scharf
UPDATE ingredients SET calorific_value = 1530, protein = 7,  fat = 1,  carbohydrates = 79, sugar = 0,  fiber = 1  WHERE id = 135 AND calorific_value <= 1; -- Reis
UPDATE ingredients SET calorific_value = 1900, protein = 24, fat = 38, carbohydrates = 2,  sugar = 1,  fiber = 0  WHERE id = 136 AND calorific_value <= 1; -- Chorizo
UPDATE ingredients SET calorific_value = 1500, protein = 22, fat = 28, carbohydrates = 1,  sugar = 0,  fiber = 0  WHERE id = 137 AND calorific_value <= 1; -- Raclettekäse nature
UPDATE ingredients SET calorific_value = 610,  protein = 21, fat = 6,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 138 AND calorific_value <= 1; -- Schweineschnitzel
UPDATE ingredients SET calorific_value = 440,  protein = 8,  fat = 2,  carbohydrates = 18, sugar = 0,  fiber = 6  WHERE id = 142 AND calorific_value <= 1; -- Hefe (frisch/estimate)
UPDATE ingredients SET calorific_value = 0,    protein = 0,  fat = 0,  carbohydrates = 0,  sugar = 0,  fiber = 0  WHERE id = 143 AND calorific_value <= 1; -- lauwarmes Wasser
UPDATE ingredients SET calorific_value = 305,  protein = 1,  fat = 1,  carbohydrates = 18, sugar = 7,  fiber = 7  WHERE id = 151 AND calorific_value <= 1; -- Holunderbeeren
