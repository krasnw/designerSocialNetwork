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


INSERT INTO api_schema.user_report (reporter_id, reported_id, reason_id, report_date, description)
VALUES
(1, 2, 1, '2024-01-15', 'Spam report'),
(2, 3, 2, '2024-02-15', 'Inappropriate behavior'),
(3, 4, 3, '2024-03-15', 'Impersonation'),
(4, 5, 4, '2024-04-15', 'Inappropriate profile'),
(5, 6, 5, '2024-05-15', 'Other');



INSERT INTO api_schema.post_report (reporter_id, reported_id, reason_id, report_date, description)
VALUES
(1, 1, 1, '2024-01-25', 'Spam post'),
(2, 2, 2, '2024-02-25', 'Inappropriate content'),
(3, 3, 3, '2024-03-25', 'Content theft'),
(4, 4, 4, '2024-04-25', 'Misleading information'),
(5, 5, 5, '2024-05-25', 'Other');

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