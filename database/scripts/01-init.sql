\c postgres;

DROP DATABASE IF EXISTS api_database;
CREATE DATABASE api_database WITH ENCODING 'UTF8' LC_COLLATE='en_US.UTF-8' LC_CTYPE='en_US.UTF-8' TEMPLATE template0;
\c api_database;

DROP SCHEMA IF EXISTS api_schema CASCADE;
CREATE SCHEMA api_schema;
SET search_path TO api_schema;

ALTER USER api_user WITH CREATEDB;

CREATE TYPE api_schema.account_level AS ENUM ('user', 'admin');
CREATE TYPE api_schema.account_status AS ENUM ('active', 'frozen');
CREATE TYPE api_schema.tag_type AS ENUM ('ui element', 'style', 'color');
CREATE TYPE api_schema.access_level AS ENUM ('public', 'private', 'protected');
CREATE TYPE api_schema.request_status AS ENUM ('pending', 'accepted', 'completed');
CREATE TYPE api_schema.chat_status AS ENUM ('active', 'disabled');  -- changed from ('active', 'closed')
CREATE TYPE api_schema.message_type AS ENUM ('Complex', 'Transaction', 'TransactionApproval', 'EndRequest', 'EndRequestApproval');
CREATE TYPE api_schema.reason_type AS ENUM ('user', 'post');
CREATE TYPE api_schema.report_status AS ENUM ('pending', 'resolved', 'dismissed');

CREATE TABLE api_schema."user" (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(50) NOT NULL UNIQUE,
    user_password VARCHAR(255) NOT NULL,
    first_name VARCHAR(50) NOT NULL COLLATE "ucs_basic",
    last_name VARCHAR(50) NOT NULL COLLATE "ucs_basic",
    phone_number VARCHAR(25) NOT NULL UNIQUE,
    profile_description TEXT,
    profile_picture VARCHAR(100),
    join_date DATE NOT NULL,
    account_status account_status NOT NULL,
    account_level account_level NOT NULL,
    access_fee DECIMAL NOT NULL,
    completed_requests INTEGER NOT NULL DEFAULT 0
);

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
    access_date DATE NOT NULL,
    auto_renewal BOOLEAN DEFAULT false NOT NULL
);

CREATE TABLE api_schema.image_container (
    id SERIAL PRIMARY KEY,
    amount_of_images INTEGER NOT NULL,
    main_image_id INTEGER
);

CREATE TABLE api_schema.image (
    id SERIAL PRIMARY KEY,
    image_file_path VARCHAR(100) NOT NULL UNIQUE,
    container_id INTEGER REFERENCES image_container(id),
    user_id INTEGER REFERENCES "user"(id)
);

ALTER TABLE api_schema.image_container 
    ADD CONSTRAINT fk_main_image FOREIGN KEY (main_image_id) REFERENCES api_schema.image(id) ON DELETE SET NULL ON UPDATE CASCADE;

CREATE TABLE api_schema.tags (
    id SERIAL PRIMARY KEY,
    tag_name VARCHAR(20) NOT NULL UNIQUE,
    tag_type tag_type NOT NULL
);

CREATE TABLE api_schema.post (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES "user"(id),
    post_name VARCHAR(50) NOT NULL,
    post_text TEXT NOT NULL,
    container_id INTEGER REFERENCES image_container(id),
    post_date TIMESTAMP NOT NULL,
    likes INTEGER NOT NULL,
    access_level access_level NOT NULL
);

CREATE TABLE api_schema.post_tags (
    id SERIAL PRIMARY KEY,
    post_id INTEGER REFERENCES post(id),
    tag_id INTEGER REFERENCES tags(id)
);

CREATE TABLE api_schema.protected_post_access (
    id SERIAL PRIMARY KEY,
    post_id INTEGER REFERENCES post(id) ON DELETE CASCADE,
    access_hash VARCHAR(64) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE api_schema.post_likes (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES "user"(id) ON DELETE CASCADE,
    post_id INTEGER REFERENCES post(id) ON DELETE CASCADE,
    UNIQUE(user_id, post_id)
);

CREATE TABLE api_schema.request (
    id SERIAL PRIMARY KEY,
    buyer_id INTEGER REFERENCES "user"(id),
    seller_id INTEGER REFERENCES "user"(id),
    request_description TEXT NOT NULL,
    request_status request_status NOT NULL
);

CREATE TABLE api_schema.chat (
    id SERIAL PRIMARY KEY,
    buyer_id INTEGER REFERENCES "user"(id),
    seller_id INTEGER REFERENCES "user"(id),
    history_file_path VARCHAR(100) NOT NULL,
    start_date DATE NOT NULL,
    chat_status chat_status NOT NULL,
    message_id INTEGER
);

CREATE TABLE api_schema.message (
    id SERIAL PRIMARY KEY,
    sender_id INTEGER REFERENCES "user"(id),
    receiver_id INTEGER REFERENCES "user"(id),
    text_content TEXT,
    type message_type NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_read BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE api_schema.message_image (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES message(id) ON DELETE CASCADE,
    image_path VARCHAR(255) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE api_schema.transaction_message (
    id SERIAL PRIMARY KEY,
    message_id INTEGER REFERENCES message(id) ON DELETE CASCADE,
    chat_id INTEGER REFERENCES chat(id),
    transaction_number VARCHAR(50) NOT NULL UNIQUE,
    transaction_hash VARCHAR(64) NOT NULL UNIQUE,
    amount DECIMAL NOT NULL,
    is_approved BOOLEAN NOT NULL DEFAULT false,
    approved_at TIMESTAMP,
    approved_by_message_id INTEGER REFERENCES message(id),
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT unique_transaction_number UNIQUE (transaction_number),
    CONSTRAINT unique_transaction_hash UNIQUE (transaction_hash)
);

CREATE TABLE api_schema.reason (
    id SERIAL PRIMARY KEY,
    reason_name VARCHAR(50) NOT NULL,
    reason_type reason_type NOT NULL
);

CREATE TABLE api_schema.user_report (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER REFERENCES "user"(id),
    reported_id INTEGER REFERENCES "user"(id),
    reason_id INTEGER REFERENCES reason(id),
    description TEXT,
    report_date DATE NOT NULL
);

CREATE TABLE api_schema.post_report (
    id SERIAL PRIMARY KEY,
    reporter_id INTEGER REFERENCES "user"(id),
    reported_id INTEGER REFERENCES post(id),
    reason_id INTEGER REFERENCES reason(id),
    description TEXT,
    report_date DATE NOT NULL
);

CREATE TABLE api_schema.user_rating (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES "user"(id),
    total_likes INTEGER NOT NULL DEFAULT 0,
    last_updated TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_protected_post_hash ON api_schema.protected_post_access(access_hash);
CREATE INDEX idx_message_sender ON api_schema.message(sender_id);
CREATE INDEX idx_message_receiver ON api_schema.message(receiver_id);
CREATE INDEX idx_message_created_at ON api_schema.message(created_at);
CREATE INDEX idx_transaction_message_id ON api_schema.transaction_message(message_id);
CREATE INDEX idx_transaction_chat_id ON api_schema.transaction_message(chat_id);
CREATE INDEX idx_transaction_number ON api_schema.transaction_message(transaction_number);
CREATE INDEX idx_transaction_hash ON api_schema.transaction_message(transaction_hash);
CREATE INDEX idx_transaction_approved_by ON api_schema.transaction_message(approved_by_message_id);
CREATE INDEX idx_transaction_approved_at ON api_schema.transaction_message(approved_at);
CREATE INDEX idx_user_rating_total_likes ON api_schema.user_rating(total_likes DESC);

ALTER TABLE api_schema.chat 
    DROP CONSTRAINT IF EXISTS fk_end_request_message;
    
ALTER TABLE api_schema.chat 
    ADD CONSTRAINT fk_end_request_message 
    FOREIGN KEY (message_id) 
    REFERENCES api_schema.message(id)
    ON DELETE SET NULL;

GRANT USAGE ON SCHEMA api_schema TO api_user;
GRANT ALL ON ALL TABLES IN SCHEMA api_schema TO api_user;
GRANT ALL ON ALL SEQUENCES IN SCHEMA api_schema TO api_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA api_schema GRANT ALL ON TABLES TO api_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA api_schema GRANT ALL ON SEQUENCES TO api_user;
ALTER DATABASE api_database OWNER TO api_user;
ALTER SCHEMA api_schema OWNER TO api_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA api_schema TO api_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA api_schema TO api_user;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA api_schema TO api_user;
ALTER ROLE api_user SET search_path TO api_schema;
GRANT CREATE ON SCHEMA api_schema TO api_user;
GRANT USAGE ON ALL SEQUENCES IN SCHEMA api_schema TO api_user;
ALTER DEFAULT PRIVILEGES FOR ROLE api_user IN SCHEMA api_schema GRANT ALL ON TABLES TO api_user;
ALTER DEFAULT PRIVILEGES FOR ROLE api_user IN SCHEMA api_schema GRANT ALL ON SEQUENCES TO api_user;