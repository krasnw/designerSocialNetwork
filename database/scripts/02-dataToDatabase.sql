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



INSERT INTO api_schema.post (id, user_id, post_name, post_text, container_id, post_date, likes, access_level) 
VALUES
(1, 1, 'UI Design', 'Sample post content 1', 1, '2024-01-10', 10, 'public'),
(2, 2, 'Logo Design', 'Sample post content 2', 2, '2024-02-10', 20, 'public'),
(3, 3, 'Button Design', 'Sample post content 3', 3, '2024-03-10', 15, 'private'),
(4, 4, 'Icon Design', 'Sample post content 4', 4, '2024-04-10', 5, 'protecteed'),
(5, 5, 'UX Design', 'Sample post content 5', 1, '2024-05-10', 50, 'public'),
(6, 6, 'Glass Morphism', 'Sample post content 2', 1, '2024-06-10', 100, 'private'),
(7, 7, 'Website Layout', 'Sample post content 3', 2, '2024-07-10', 25, 'public'),
(8, 8, 'App Design', 'Sample post content 8', 4, '2024-08-10', 30, 'protecteed'),
(9, 9, 'Dashboard Design', 'Sample post content 1', 4, '2024-09-10', 75, 'private'),
(10, 10, 'Card Design', 'Sample post content 10', 3, '2024-10-10', 90, 'public');



INSERT INTO api_schema.post_tags (post_id, tag_id)
VALUES
(1, 2),
(2, 4),
(3, 3), 
(4, 1), 
(5, 3), 
(6, 9), 
(7, 5), 
(8, 6), 
(9, 7), 
(10, 8); 


INSERT INTO api_schema.powerup (post_id, transaction_id, powerup_date, power_days)
VALUES
(1, 1, '2024-01-20', 30),
(2, 2, '2024-02-20', 15),
(3, 3, '2024-03-20', 60),
(4, 4, '2024-04-20', 20),
(5, 5, '2024-05-20', 45),
(6, 6, '2024-06-20', 10),
(7, 7, '2024-07-20', 40),
(8, 8, '2024-08-20', 25),
(9, 9, '2024-09-20', 35),
(10, 10, '2024-10-20', 50);


INSERT INTO api_schema.chat (buyer_id, seller_id, history_file_path, start_date, chat_status)
VALUES
(1, 2, '/chats/chat1.txt', '2024-01-05', 'active'),
(2, 3, '/chats/chat2.txt', '2024-02-05', 'closed'),
(3, 4, '/chats/chat3.txt', '2024-03-05', 'active'),
(4, 5, '/chats/chat4.txt', '2024-04-05', 'closed'),
(5, 6, '/chats/chat5.txt', '2024-05-05', 'active'),
(6, 7, '/chats/chat6.txt', '2024-06-05', 'closed'),
(7, 8, '/chats/chat7.txt', '2024-07-05', 'active'),
(8, 9, '/chats/chat8.txt', '2024-08-05', 'closed'),
(9, 10, '/chats/chat9.txt', '2024-09-05', 'active'),
(10, 1, '/chats/chat10.txt', '2024-10-05', 'active');


INSERT INTO api_schema.chat_image (chat_id, container_id)
VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 4),
(5, 1),
(6, 2),
(7, 3),
(8, 4),
(9, 1),
(10, 2);

INSERT INTO api_schema.chat_transaction (chat_id, transaction_id)
VALUES
(1, 1),
(2, 2),
(3, 3),
(4, 4),
(5, 5),
(6, 6),
(7, 7),
(8, 8),
(9, 9),
(10, 10);


INSERT INTO api_schema.reason (reason_name)
VALUES
('Spam'),
('Przemoc'),
('Nieodpowiednia treść do tematyki platformy'),
('Kradzież treści'),
('Inne');


INSERT INTO api_schema.user_report (reporter_id, reported_id, reason_id, report_date)
VALUES
(1, 2, 1, '2024-01-15'),
(2, 3, 2, '2024-02-15'),
(3, 4, 3, '2024-03-15'),
(4, 5, 4, '2024-04-15'),
(5, 6, 5, '2024-05-15');



INSERT INTO api_schema.post_report (reporter_id, reported_id, reason_id, report_date)
VALUES
(1, 1, 1, '2024-01-25'),
(2, 2, 2, '2024-02-25'),
(3, 3, 3, '2024-03-25'),
(4, 4, 4, '2024-04-25'),
(5, 5, 5, '2024-05-25');


INSERT INTO api_schema.image_report (reporter_id, reported_id, reason_id, report_date)
VALUES
(1, 1, 1, '2024-01-30'),
(2, 2, 2, '2024-02-25'),
(3, 3, 3, '2024-03-30'),
(4, 4, 4, '2024-04-30');



INSERT INTO api_schema.chat_report (reporter_id, reported_id, reason_id, report_date)
VALUES
(1, 1, 1, '2024-01-20'),
(2, 2, 2, '2024-02-20'),
(3, 3, 3, '2024-03-20'),
(4, 4, 4, '2024-04-20'),
(5, 5, 5, '2024-05-20');



INSERT INTO api_schema.chat_image_report (reporter_id, reported_id, reason_id, report_date)
VALUES
(1, 1, 1, '2024-01-25'),
(2, 2, 2, '2024-02-25'),
(3, 3, 3, '2024-03-25'),
(4, 4, 4, '2024-04-25'),
(5, 5, 5, '2024-05-25');




INSERT INTO api_schema.rating_list (tag_id, list_file_path)
VALUES
(1, '/ratings/design_ratings.txt'),
(2, '/ratings/ui_ratings.txt'),
(3, '/ratings/ux_ratings.txt'),
(4, '/ratings/logo_ratings.txt'),
(5, '/ratings/web_ratings.txt'),
(6, '/ratings/app_ratings.txt'),
(7, '/ratings/dashboard_ratings.txt'),
(8, '/ratings/graphics_ratings.txt');



INSERT INTO api_schema.user_rating (user_id, list_id, rating)
VALUES
(1, 1, 5),
(2, 2, 4),
(3, 3, 3),
(4, 4, 5),
(5, 5, 4),
(6, 6, 5),
(7, 7, 3),
(8, 8, 4),
(9, 1, 5),
(10, 2, 3);


INSERT INTO api_schema.popular_list (list_id, list_file_path)
VALUES
(1, '/popular/design_popular.txt'),
(2, '/popular/ui_popular.txt'),
(3, '/popular/ux_popular.txt'),
(4, '/popular/logo_popular.txt'),
(5, '/popular/web_popular.txt'),
(6, '/popular/app_popular.txt');


INSERT INTO api_schema.post_popularity (post_id, list_id, rating)
VALUES
(1, 1, 90),
(2, 2, 85),
(3, 3, 80),
(4, 4, 95),
(5, 5, 75),
(6, 6, 70),
(7, 1, 65),
(8, 2, 60),
(9, 3, 55),
(10, 4, 50);


