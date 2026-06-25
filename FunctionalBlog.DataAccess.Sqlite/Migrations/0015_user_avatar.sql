-- Optional avatar image for a user, referencing the shared image library.
ALTER TABLE users ADD COLUMN avatar_image_id INTEGER NULL REFERENCES images(id);
