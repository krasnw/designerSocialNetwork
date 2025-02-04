-- Reset sequences and clear data
ALTER SEQUENCE api_schema.post_id_seq RESTART WITH 1;
ALTER SEQUENCE api_schema.image_container_id_seq RESTART WITH 1;
ALTER SEQUENCE api_schema.image_id_seq RESTART WITH 1;
ALTER SEQUENCE api_schema.tags_id_seq RESTART WITH 1;
TRUNCATE api_schema.post CASCADE;

INSERT INTO api_schema."user" 
(username, email, user_password, first_name, last_name, phone_number, join_date, account_status, account_level, access_fee) 
VALUES
('user1', 'user1@example.com', '$2b$12$8UG9xihkkW/VtEzS/n4HB.Tp9eaUh4Qo6Z1hpu/dFB5grWQX2Shm.', 'Jan', 'Kowalski', '123-456-7890', '2024-01-01', 'active', 'user', 10),
('user2', 'user2@example.com', '$2b$12$KUxOZK5MrhmhPJReda8Rq.Cojxm9fZMqvOpEhfI7bAWGbsAsdVVOK', 'Anna', 'Nowak', '123-456-7891', '2024-02-01', 'active', 'user', 20),
('user3', 'user3@example.com', '$2b$12$QSLvy0lErSHHd4AQUnoDkewfIhXgL9MVZMqbvbRMxtPN5ugD9HZD.', 'Marek', 'Wójcik', '123-456-7892', '2024-03-01', 'active', 'admin', 30),
('user4', 'user4@example.com', '$2b$12$/c3vkgR93VHl7UQVYVGVBO7eSS8eZE1HZxIpq.le89MLwH9vmtTNy', 'Katarzyna', 'Jankowska', '123-456-7893', '2024-04-01', 'frozen', 'user', 40),
('user5', 'user5@example.com', '$2b$12$X7..8Kln2qjRqMsML/wyGuQ83qWqdsHGKHAZRXwOr4aM3S2Y3GEb2', 'Piotr', 'Kaczmarek', '123-456-7894', '2024-05-01', 'active', 'user', 50),
('user6', 'user6@example.com', '$2b$12$KrCdxUbDqocNwtatcBKRre.guqpz6eoikJ1lCUNRh.EdAXrPH1hIm', 'Agnieszka', 'Zielińska', '123-456-7895', '2024-06-01', 'active', 'user', 60),
('user7', 'user7@example.com', '$2b$12$2uiTCG5bDKMplmLSq99PVO1v6EwqZJjw8Tyv6SBmhtxyEEOpjn/im', 'Tomasz', 'Pawlowski', '123-456-7896', '2024-07-01', 'frozen', 'user', 70),
('admin1', 'admin1@example.com', '$2b$12$FCn4zsA2YQKTWtHtzqV4nuum0bQZ65YPnAzKr5gsAlm3tHsd5vooO', 'Ewa', 'Woźniak', '123-456-7897', '2024-08-01', 'active', 'admin', 80),
('user9', 'user9@example.com', '$2b$12$aEYTtqGca3UNHJJrAloXwu4jt2qnxEOuiPYvgTDjJt4gyr40s9Pra', 'Michał', 'Mazur', '123-456-7898', '2024-09-01', 'active', 'user', 90),
('admin2', 'admin2@example.com', '$2b$12$XpD6cuyZta5OIVe1Rcq.SuZZjiIdwSxi3an021cLPYx5gl2D0jGdq', 'Joanna', 'Szymańska', '123-456-7899', '2024-10-01', 'active', 'admin', 100);

INSERT INTO api_schema.payment_method (method_name) 
VALUES 
('Karta płatnicza'), 
('Google Pay'), 
('BLIK');


INSERT INTO api_schema.currency_transaction 
(amount_outer, amount_inner, rate_at_time, transaction_date, user_id, outer_transaction_id, payment_method_id, card_number, way) 
VALUES
(100.00, 100, 1.0, '2024-01-10', 1, 'txn_001', 1, '1234-5678-1234-5678', TRUE),
(150.00, 150, 1.0, '2024-02-10', 2, 'txn_002', 2, '1234-5678-1234-5679', TRUE),
(200.00, 200, 1.0, '2024-03-10', 3, 'txn_003', 3, '1234-5678-1234-5680', TRUE),
(250.00, 250, 1.0, '2024-04-10', 4, 'txn_004', 1, '1234-5678-1234-5681', TRUE),
(300.00, 300, 1.0, '2024-05-10', 5, 'txn_005', 2, '1234-5678-1234-5682', TRUE),
(350.00, 350, 1.0, '2024-06-10', 6, 'txn_006', 3, '1234-5678-1234-5683', TRUE),
(400.00, 400, 1.0, '2024-07-10', 7, 'txn_007', 1, '1234-5678-1234-5684', TRUE),
(450.00, 450, 1.0, '2024-08-10', 8, 'txn_008', 2, '1234-5678-1234-5685', TRUE),
(500.00, 500, 1.0, '2024-09-10', 9, 'txn_009', 3, '1234-5678-1234-5686', TRUE),
(550.00, 550, 1.0, '2024-10-10', 10, 'txn_010', 1, '1234-5678-1234-5687', TRUE);


INSERT INTO api_schema.wallet (amount, user_id) 
VALUES
(100, 1),
(150, 2),
(200, 3),
(250, 4),
(300, 5),
(350, 6),
(400, 7),
(450, 8),
(500, 9),
(550, 10);


INSERT INTO api_schema.inner_transaction (amount, transaction_date, user_id, wallet_id) 
VALUES
(100, '2024-01-15', 1, 1),
(150, '2024-02-15', 2, 2),
(200, '2024-03-15', 3, 3),
(250, '2024-04-15', 4, 4),
(300, '2024-05-15', 5, 5),
(350, '2024-06-15', 6, 6),
(400, '2024-07-15', 7, 7),
(450, '2024-08-15', 8, 8),
(500, '2024-09-15', 9, 9),
(550, '2024-10-15', 10, 10);

INSERT INTO api_schema.private_access (buyer_id, seller_id, transaction_id, price_at_time, access_date) 
VALUES
(1, 2, 1, 100, '2024-01-20'),
(2, 3, 2, 150, '2024-02-20'),
(3, 4, 3, 200, '2024-03-20'),
(4, 5, 4, 250, '2024-04-20'),
(5, 6, 5, 300, '2024-05-20'),
(6, 7, 6, 350, '2024-06-20'),
(7, 8, 7, 400, '2024-07-20'),
(8, 9, 8, 450, '2024-08-20'),
(9, 10, 9, 500, '2024-09-20'),
(10, 1, 10, 550, '2024-10-20');


INSERT INTO api_schema.image_container (amount_of_images) 
VALUES 
(3),
(3),
(2),
(2);

INSERT INTO api_schema.image (image_file_path, container_id, user_id) 
VALUES
('/images/img1.jpg', 1, 1),
('/images/img2.jpg', 1, 2),
('/images/img3.jpg', 1, 3),
('/images/img4.jpg', 2, 4),
('/images/img5.jpg', 2, 5),
('/images/img6.jpg', 2, 6),
('/images/img7.jpg', 3, 7),
('/images/img8.jpg', 3, 8),
('/images/img9.jpg', 4, 9),
('/images/img10.jpg', 4, 10);

-- Clear existing tags and reinsert with correct case
TRUNCATE api_schema.tags CASCADE;

INSERT INTO api_schema.tags (tag_name, tag_type) VALUES
('Logo', 'ui element'),
('Button', 'ui element'),
('Icon', 'ui element'),
('Card', 'ui element'),
('Web', 'style'),
('App', 'style'),
('Dashboard', 'style'),
('Graphics', 'style'),
('Morphism', 'style'),
('Branding', 'style'),
('Red', 'color'),
('Blue', 'color'),
('Green', 'color'),
('Yellow', 'color'),
('Black', 'color'),
('White', 'color'),
('Gray', 'color'),
('Purple', 'color'),
('Orange', 'color'),
('Brown', 'color');



INSERT INTO api_schema.post (user_id, post_name, post_text, container_id, post_date, likes, access_level) 
VALUES
(1, 'UI Design', 'Sample post content 1', 1, '2024-01-10 10:30:00', 10, 'public'),
(2, 'Logo Design', 'Sample post content 2', 2, '2024-02-10 11:45:00', 20, 'public'),
(3, 'Button Design', 'Sample post content 3', 3, '2024-03-10 14:20:00', 15, 'private'),
(4, 'Icon Design', 'Sample post content 4', 4, '2024-04-10 09:15:00', 5, 'protected'),
(5, 'UX Design', 'Sample post content 5', 1, '2024-05-10 16:00:00', 50, 'public'),
(6, 'Glass Morphism', 'Sample post content 2', 1, '2024-06-10 13:45:00', 100, 'private'),
(7, 'Website Layout', 'Sample post content 3', 2, '2024-07-10 08:30:00', 25, 'public'),
(8, 'App Design', 'Sample post content 8', 4, '2024-08-10 11:00:00', 30, 'protected'),
(9, 'Dashboard Design', 'Sample post content 1', 4, '2024-09-10 14:50:00', 75, 'private'),
(10, 'Card Design', 'Sample post content 10', 3, '2024-10-10 10:10:00', 90, 'public');

INSERT INTO api_schema.post_tags (post_id, tag_id)
SELECT p.id, t.id
FROM api_schema.post p, api_schema.tags t
WHERE (p.post_name, t.tag_name) IN
(('UI Design', 'Button'),
 ('Logo Design', 'Card'),
 ('Button Design', 'Icon'),
 ('Icon Design', 'Logo'),
 ('UX Design', 'Icon'),
 ('Glass Morphism', 'Morphism'),
 ('Website Layout', 'Web'),
 ('App Design', 'App'),
 ('Dashboard Design', 'Dashboard'),
 ('Card Design', 'Graphics'));

INSERT INTO api_schema.reason (reason_name, reason_type)
VALUES
('Spam', 'user'),
('Niewłaściwe zachowanie', 'user'),
('Podszywanie się', 'user'),
('Nieodpowiedni profil', 'user'),
('Inne', 'user'),
('Spam', 'post'),
('Nieodpowiednia treść', 'post'),
('Kradzież treści', 'post'),
('Wprowadzające w błąd informacje', 'post'),
('Inne', 'post');


INSERT INTO api_schema.user_report (reporter_id, reported_id, reason_id, report_date, status)
VALUES
(1, 2, 1, '2024-01-15', 'pending'),
(2, 3, 2, '2024-02-15', 'pending'),
(3, 4, 3, '2024-03-15', 'resolved'),
(4, 5, 4, '2024-04-15', 'pending'),
(5, 6, 5, '2024-05-15', 'dismissed');



INSERT INTO api_schema.post_report (reporter_id, reported_id, reason_id, report_date, status)
VALUES
(1, 1, 1, '2024-01-25', 'pending'),
(2, 2, 2, '2024-02-25', 'resolved'),
(3, 3, 3, '2024-03-25', 'pending'),
(4, 4, 4, '2024-04-25', 'dismissed'),
(5, 5, 5, '2024-05-25', 'pending');


INSERT INTO api_schema.image_report (reporter_id, reported_id, reason_id, report_date, status)
VALUES
(1, 1, 1, '2024-01-30', 'pending'),
(2, 2, 2, '2024-02-25', 'resolved'),
(3, 3, 3, '2024-03-30', 'pending'),
(4, 4, 4, '2024-04-30', 'dismissed');

-- Add this new section instead
INSERT INTO api_schema.user_rating (user_id, total_likes, last_updated)
VALUES
(1, 100, CURRENT_TIMESTAMP),
(2, 85, CURRENT_TIMESTAMP),
(3, 150, CURRENT_TIMESTAMP),
(4, 75, CURRENT_TIMESTAMP),
(5, 200, CURRENT_TIMESTAMP),
(6, 90, CURRENT_TIMESTAMP),
(7, 120, CURRENT_TIMESTAMP),
(8, 180, CURRENT_TIMESTAMP),
(9, 95, CURRENT_TIMESTAMP),
(10, 160, CURRENT_TIMESTAMP);

-- Add sample transaction messages with proper hash generation
WITH inserted_messages AS (
    INSERT INTO api_schema.message (sender_id, receiver_id, text_content, type, created_at)
    SELECT 
        sender.id as sender_id,
        receiver.id as receiver_id,
        text_content,
        'Transaction'::api_schema.message_type as type,
        created_at
    FROM (VALUES
        (1, 2, 'Payment for design work', CURRENT_TIMESTAMP - INTERVAL '1 day'),
        (3, 4, 'Logo design payment', CURRENT_TIMESTAMP - INTERVAL '2 days'),
        (5, 6, 'UI consultation fee', CURRENT_TIMESTAMP - INTERVAL '3 days')
    ) as data(sender_id, receiver_id, text_content, created_at)
    JOIN api_schema."user" sender ON sender.id = data.sender_id
    JOIN api_schema."user" receiver ON receiver.id = data.receiver_id
    RETURNING id, sender_id, receiver_id, text_content, created_at
),
message_data AS (
    SELECT 
        m.id as message_id, 
        c.id as chat_id,
        (SELECT username FROM api_schema."user" WHERE id = m.sender_id) as sender_username,
        (SELECT username FROM api_schema."user" WHERE id = m.receiver_id) as receiver_username,
        CASE 
            WHEN m.text_content = 'Payment for design work' THEN 100.00
            WHEN m.text_content = 'Logo design payment' THEN 250.00
            WHEN m.text_content = 'UI consultation fee' THEN 150.00
        END as amount,
        CASE 
            WHEN m.text_content = 'Logo design payment' THEN true
            ELSE false
        END as is_approved,
        m.created_at
    FROM inserted_messages m
    JOIN api_schema.chat c ON 
        (c.buyer_id = m.sender_id AND c.seller_id = m.receiver_id) OR
        (c.seller_id = m.sender_id AND c.buyer_id = m.receiver_id)
)
INSERT INTO api_schema.transaction_message (
    message_id, 
    chat_id, 
    transaction_number,
    transaction_hash,
    amount, 
    is_approved
)
SELECT 
    message_id,
    chat_id,
    'TR-' || EXTRACT(EPOCH FROM CURRENT_TIMESTAMP)::bigint || '-' || chat_id || '-' || message_id as transaction_number,
    encode(sha256(concat_ws(':', 
        sender_username, 
        receiver_username, 
        amount::text, 
        EXTRACT(EPOCH FROM created_at)::bigint
    )::bytea), 'hex') as transaction_hash,
    amount,
    is_approved
FROM message_data;


