-- TODO: REMOVE NEXT LINE IN PRODUCTION
\c postgres;

DROP DATABASE IF EXISTS api_database;

-- Creation of the database
CREATE DATABASE api_database;
-- connect to the database
\c api_database;

    
-- Drop the schema if it exists
DROP SCHEMA IF EXISTS api_schema CASCADE;
-- Creation of the schema
CREATE SCHEMA api_schema;
--switch to the new schema
SET search_path TO api_schema;

-- Update user's password
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'api_user') THEN
            ALTER USER api_user WITH PASSWORD 'api_user_password';
        ELSE
            CREATE USER api_user WITH PASSWORD 'api_user_password';
        END IF;
    END $$;

-- Creation of the tables and enums used in tables
-- User block
CREATE TYPE api_schema.account_level AS ENUM ('user', 'admin');
CREATE TYPE api_schema.account_status AS ENUM ('active', 'frozen');
CREATE TABLE api_schema."user" (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(50) NOT NULL UNIQUE,
    user_password CHAR(64) NOT NULL, -- SHA-256 hash
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    phone_number VARCHAR(25) NOT NULL UNIQUE,
    profile_description TEXT,
    profile_picture VARCHAR(100),
    join_date DATE NOT NULL,
    account_status account_status NOT NULL,
    account_level account_level NOT NULL,
    access_fee DECIMAL NOT NULL
);
-- end of user block

-- Outer payment block
CREATE TABLE api_schema.payment_method (
    id SERIAL PRIMARY KEY,
    method_name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE api_schema.currency_transaction (
    id SERIAL PRIMARY KEY,
    amount_outer DECIMAL(10, 2) NOT NULL,
    amount_inner INTEGER NOT NULL,
    rate_at_time DECIMAL(10, 2) NOT NULL,
    transaction_date DATE NOT NULL,
    user_id INTEGER REFERENCES "user"(id),
    outer_transaction_id VARCHAR(50) NOT NULL,
    payment_method_id INTEGER REFERENCES payment_method(id),
    card_number VARCHAR(50) NOT NULL,
    way BOOLEAN NOT NULL
);
-- end of outer payment block

-- Inner payment block
CREATE TABLE api_schema.wallet (
    id SERIAL PRIMARY KEY,
    amount INTEGER NOT NULL,
    user_id INTEGER REFERENCES "user"(id)
);

CREATE TABLE api_schema.inner_transaction (
    id SERIAL PRIMARY KEY,
    amount INTEGER NOT NULL,
    transaction_date DATE NOT NULL,
    user_id INTEGER REFERENCES "user"(id),
    wallet_id INTEGER REFERENCES wallet(id)
);

CREATE TABLE api_schema.private_access (
    id SERIAL PRIMARY KEY,
    buyer_id INTEGER REFERENCES "user"(id),
    seller_id INTEGER REFERENCES "user"(id),
    transaction_id INTEGER REFERENCES inner_transaction(id),
    price_at_time INTEGER NOT NULL,
    access_date DATE NOT NULL
);
-- end of inner payment block

-- Image block
CREATE TABLE api_schema.image_container (
    id SERIAL PRIMARY KEY,
    amount_of_images INTEGER NOT NULL
);

CREATE TABLE api_schema.image (
    id SERIAL PRIMARY KEY,
    image_file_path VARCHAR(100) NOT NULL UNIQUE,
    container_id INTEGER REFERENCES image_container(id),
    user_id INTEGER REFERENCES "user"(id)
);

ALTER TABLE api_schema.image_container 
ADD COLUMN main_image_id INTEGER REFERENCES api_schema.image(id) ON DELETE SET NULL ON UPDATE CASCADE;
-- end of image block

-- Post block
CREATE TYPE api_schema.tag_type AS ENUM ('UI element', 'Style', 'Color');
CREATE TABLE api_schema.tags (
    id SERIAL PRIMARY KEY,
    tag_name VARCHAR(20) NOT NULL UNIQUE,
    tag_type tag_type NOT NULL
);

CREATE TYPE api_schema.access_level AS ENUM ('public', 'private', 'protecteed');
CREATE TABLE api_schema.post (
    id INTEGER PRIMARY KEY,
    user_id INTEGER REFERENCES "user"(id),
    post_name VARCHAR(50) NOT NULL,
    post_text TEXT NOT NULL,
    container_id INTEGER REFERENCES image_container(id),
    post_date DATE NOT NULL,
    likes INTEGER NOT NULL,
    access_level access_level NOT NULL
);

CREATE TABLE api_schema.post_tags (
    id SERIAL PRIMARY KEY,
    post_id INTEGER REFERENCES post(id),
    tag_id INTEGER REFERENCES tags(id)
);

CREATE TABLE api_schema.powerup (
    id SERIAL PRIMARY KEY,
    post_id INTEGER REFERENCES post(id),
    transaction_id INTEGER REFERENCES inner_transaction(id),
    powerup_date DATE NOT NULL,
    power_days INTEGER NOT NULL
);
-- end of post block

-- Chat block
CREATE TYPE api_schema.request_status AS ENUM ('pending', 'accepted', 'completed');
CREATE TABLE api_schema.request (
    id SERIAL PRIMARY KEY,
    buyer_id INTEGER REFERENCES "user"(id),
    seller_id INTEGER REFERENCES "user"(id),
    request_description TEXT NOT NULL,
    request_status request_status NOT NULL
);


CREATE TYPE api_schema.chat_status AS ENUM ('active', 'closed');
CREATE TABLE api_schema.chat (
    id SERIAL PRIMARY KEY,
    buyer_id INTEGER REFERENCES "user"(id),
    seller_id INTEGER REFERENCES "user"(id),
    history_file_path VARCHAR(100) NOT NULL,
    start_date DATE NOT NULL,
    chat_status chat_status NOT NULL
);

CREATE TABLE api_schema.chat_image (
    id SERIAL PRIMARY KEY,
    chat_id INTEGER REFERENCES chat(id),
    container_id INTEGER REFERENCES image_container(id)
);

CREATE TABLE api_schema.chat_transaction (
    id SERIAL PRIMARY KEY,
    chat_id INTEGER REFERENCES chat(id),
    transaction_id INTEGER REFERENCES inner_transaction(id)
);
-- end of chat block

-- Report block
CREATE TABLE api_schema.reason (
    id SERIAL PRIMARY KEY,
    reason_name VARCHAR(50) NOT NULL
);

CREATE TABLE api_schema.user_report (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER REFERENCES "user"(id),
    reported_id INTEGER REFERENCES "user"(id),
    reason_id INTEGER REFERENCES reason(id),
    report_date DATE NOT NULL
);

CREATE TABLE api_schema.post_report (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER REFERENCES "user"(id),
    reported_id INTEGER REFERENCES post(id),
    reason_id INTEGER REFERENCES reason(id),
    report_date DATE NOT NULL
);

CREATE TABLE api_schema.image_report (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER REFERENCES "user"(id),
    reported_id INTEGER REFERENCES image_container(id),
    reason_id INTEGER REFERENCES reason(id),
    report_date DATE NOT NULL
);

CREATE TABLE api_schema.chat_report (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER REFERENCES "user"(id),
    reported_id INTEGER REFERENCES chat(id),
    reason_id INTEGER REFERENCES reason(id),
    report_date DATE NOT NULL
);

CREATE TABLE api_schema.chat_image_report (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER REFERENCES "user"(id),
    reported_id INTEGER REFERENCES chat_image(id),
    reason_id INTEGER REFERENCES reason(id),
    report_date DATE NOT NULL
);
-- end of report block

-- Rating block
CREATE TABLE api_schema.rating_list (
    id SERIAL PRIMARY KEY,
    tag_id INTEGER REFERENCES tags(id),
    list_file_path VARCHAR(100) NOT NULL
);

CREATE TABLE api_schema.user_rating (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES "user"(id),
    list_id INTEGER REFERENCES rating_list(id),
    rating INTEGER NOT NULL
);

CREATE TABLE api_schema.popular_list (
    id SERIAL PRIMARY KEY,
    list_id INTEGER REFERENCES rating_list(id),
    list_file_path VARCHAR(100) NOT NULL,
    tag_id INTEGER REFERENCES tags(id)
);

CREATE TABLE api_schema.post_popularity (
    id SERIAL PRIMARY KEY,
    post_id INTEGER REFERENCES post(id),
    list_id INTEGER REFERENCES rating_list(id),
    rating INTEGER NOT NULL
);
-- end of rating block

-- Grant privileges to everything in the schema/database
ALTER DATABASE api_database OWNER TO api_user;
ALTER SCHEMA api_schema OWNER TO api_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA api_schema TO api_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA api_schema TO api_user;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA api_schema TO api_user;