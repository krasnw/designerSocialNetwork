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

CREATE TYPE account_level AS ENUM ('user', 'admin');
CREATE TYPE account_status AS ENUM ('active', 'frozen');
CREATE TABLE "user" (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    email VARCHAR(50) NOT NULL,
    password CHAR(64) NOT NULL, -- SHA-256 hash
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    phone_number VARCHAR(25) NOT NULL,
    join_date DATE NOT NULL,
    freeze_date DATE,
    account_status account_status NOT NULL,
    account_level account_level NOT NULL,
    access_fee DECIMAL(10,2) NOT NULL
);

-- User definition
CREATE USER api WITH PASSWORD '${API_USER_PASSWORD}';

-- Grant privileges to everything in the schema/database
ALTER DATABASE api_database OWNER TO api;
ALTER SCHEMA api_schema OWNER TO api;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA api_schema TO api;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA api_schema TO api;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA api_schema TO api;