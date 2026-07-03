-- Add 'name' column to existing users table
ALTER TABLE users
    ADD COLUMN name VARCHAR(255) NOT NULL DEFAULT '';

-- Populate name with existing usernames as initial value
UPDATE users SET name = username WHERE name = '';
