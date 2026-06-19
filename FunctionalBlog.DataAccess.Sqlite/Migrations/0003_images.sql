CREATE TABLE images (
    id           INTEGER PRIMARY KEY,
    file_name    TEXT    NOT NULL,
    content_type TEXT    NOT NULL,
    data         BLOB    NOT NULL,
    byte_size    INTEGER NOT NULL,
    uploaded_by  INTEGER NOT NULL,
    created_at   TEXT    NOT NULL
);

ALTER TABLE articles ADD COLUMN cover_image_id INTEGER NULL;
