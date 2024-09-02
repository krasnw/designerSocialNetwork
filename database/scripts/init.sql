-- TODO: REMOVE NEXT LINE IN PRODUCTION
DROP DATABASE IF EXISTS api_database;

-- Creation of the database
CREATE DATABASE api_database;
-- connect to the database
\c database;

-- Creation of the schema
CREATE SCHEMA api_schema;
--switch to the new schema
SET search_path TO api_schema;

-- Creation of the tables and enums used in tables

-- User block
CREATE TYPE account_level AS ENUM ('user', 'admin');
CREATE TYPE account_status AS ENUM ('active', 'frozen');
CREATE TABLE "user" (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    email VARCHAR(50) NOT NULL,
    user_password CHAR(64) NOT NULL, -- SHA-256 hash
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    phone_number VARCHAR(25) NOT NULL,
    join_date DATE NOT NULL,
    freeze_date DATE,
    account_status account_status NOT NULL,
    account_level account_level NOT NULL,
    access_fee DECIMAL(10,2) NOT NULL
);
-- end of user block

-- Outer payment block
CREATE TABLE payment_method (
    id SERIAL PRIMARY KEY,
    method_name VARCHAR(50) NOT NULL,
);

CREATE TABLE currency_transaction (
    id SERIAL PRIMARY KEY,
    amount_outer DECIMAL(10, 2) NOT NULL,
    amount_inner INTEGER NOT NULL,
    rate_at_time DECIMAL(10, 2) NOT NULL,
    transaction_date DATE NOT NULL,
    user_id INTEGER REFERENCES "user"(id)
    outer_transaction_id VARCHAR(50) NOT NULL,
    payment_method_id INTEGER REFERENCES payment_method(id)
    card_number VARCHAR(50) NOT NULL,
    way BOOLEAN NOT NULL
);
-- end of outer payment block

-- Inner payment block
CREATE TABLE wallet (
    id SERIAL PRIMARY KEY,
    amount INTEGER NOT NULL,
    user_id INTEGER REFERENCES "user"(id)
);

CREATE TABLE inner_transaction (
    id SERIAL PRIMARY KEY,
    amount DECIMAL(10, 2) NOT NULL,
    transaction_date DATE NOT NULL,
    user_id INTEGER REFERENCES "user"(id),
    wallet_id INTEGER REFERENCES wallet(id)
);
-- end of inner payment block

-- Image block
CREATE TABLE image_container (
    id SERIAL PRIMARY KEY,
    amount_of_images INTEGER NOT NULL,
);

CREATE TABLE image (
    id SERIAL PRIMARY KEY,
    image_file_path VARCHAR(100) NOT NULL,
    container_id INTEGER REFERENCES image_container(id),
    user_id INTEGER REFERENCES "user"(id)
);
-- end of image block

-- Post block
CREATE TABLE tags (
    id SERIAL PRIMARY KEY,
    tag_name VARCHAR(20) NOT NULL,
);

CREATE TABLE post_tags (
    post_id INTEGER REFERENCES post(id),
    tag_id INTEGER REFERENCES tags(id),
    PRIMARY KEY (post_id, tag_id)
);

CREATE ENUM access_level AS ENUM ('public', 'private', 'protecteed');
CREATE TABLE post (
    id INTEGER PRIMARY KEY,
    user_id INTEGER REFERENCES "user"(id),
    post_name VARCHAR(50) NOT NULL,
    post_text TEXT NOT NULL,
    container_id INTEGER REFERENCES image_container(id),
    post_date DATE NOT NULL,
    post_tags_id INTEGER REFERENCES post_tags(post_id),
    likes INTEGER NOT NULL,
    access_level access_level NOT NULL
);

-- User definition
CREATE USER api WITH PASSWORD '${API_USER_PASSWORD}';

-- Grant privileges to everything in the schema/database
ALTER DATABASE api_database OWNER TO api;
ALTER SCHEMA api_schema OWNER TO api;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA api_schema TO api;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA api_schema TO api;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA api_schema TO api;