CREATE TABLE articles (
    id           INTEGER PRIMARY KEY,
    title        TEXT    NOT NULL,
    teaser       TEXT    NOT NULL,
    text         TEXT    NOT NULL,
    author_id    INTEGER NOT NULL,
    published_at TEXT    NOT NULL
);

CREATE TABLE users (
    id            INTEGER PRIMARY KEY,
    email         TEXT    NOT NULL UNIQUE,
    display_name  TEXT    NOT NULL,
    password_hash TEXT    NOT NULL,
    created_at    TEXT    NOT NULL
);

CREATE TABLE user_roles (
    user_id   INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_name TEXT    NOT NULL,
    PRIMARY KEY (user_id, role_name)
);

CREATE TABLE sessions (
    token      TEXT    PRIMARY KEY,
    user_id    INTEGER NOT NULL,
    expires_at TEXT    NOT NULL
);

CREATE TABLE password_reset_tokens (
    token      TEXT    PRIMARY KEY,
    user_id    INTEGER NOT NULL,
    expires_at TEXT    NOT NULL,
    consumed   INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE roles (
    id   INTEGER PRIMARY KEY,
    name TEXT    NOT NULL UNIQUE
);

CREATE TABLE permission_rules (
    role_id      INTEGER NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    action_name  TEXT    NOT NULL,
    resource_key TEXT    NOT NULL,
    PRIMARY KEY (role_id, action_name, resource_key)
);

CREATE TABLE ingredients (
    id              INTEGER PRIMARY KEY,
    name            TEXT NOT NULL,
    image           TEXT NOT NULL,
    description     TEXT NOT NULL,
    density         REAL NOT NULL,
    piece_count     REAL NOT NULL,
    calorific_value REAL NOT NULL,
    protein         REAL NOT NULL,
    fat             REAL NOT NULL,
    carbohydrates   REAL NOT NULL,
    sugar           REAL NOT NULL,
    fiber           REAL NOT NULL
);

CREATE TABLE recipes (
    id          INTEGER PRIMARY KEY,
    name        TEXT    NOT NULL,
    description TEXT    NOT NULL,
    author_id   INTEGER NOT NULL,
    difficulty  INTEGER NOT NULL,
    portions    INTEGER NOT NULL
);

CREATE TABLE recipe_steps (
    recipe_id  INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    sort_order INTEGER NOT NULL,
    text       TEXT    NOT NULL,
    PRIMARY KEY (recipe_id, sort_order)
);

CREATE TABLE recipe_tags (
    recipe_id INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    tag       TEXT    NOT NULL,
    PRIMARY KEY (recipe_id, tag)
);

CREATE TABLE recipe_ingredients (
    recipe_id         INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    sort_order        INTEGER NOT NULL,
    ingredient_id     INTEGER NOT NULL,
    amount            REAL    NOT NULL,
    unit_abbreviation TEXT    NOT NULL,
    PRIMARY KEY (recipe_id, sort_order)
);

CREATE TABLE recipe_images (
    recipe_id  INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    sort_order INTEGER NOT NULL,
    url        TEXT    NOT NULL,
    PRIMARY KEY (recipe_id, sort_order)
);

CREATE TABLE recipe_hints (
    recipe_id  INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    sort_order INTEGER NOT NULL,
    text       TEXT    NOT NULL,
    PRIMARY KEY (recipe_id, sort_order)
);

CREATE TABLE translations (
    key      TEXT NOT NULL,
    language TEXT NOT NULL,
    variant  TEXT NOT NULL DEFAULT '',
    text     TEXT NOT NULL,
    PRIMARY KEY (key, language, variant)
);
