-- Reset sequences and clear data
ALTER SEQUENCE api_schema.post_id_seq RESTART WITH 1;
ALTER SEQUENCE api_schema.image_container_id_seq RESTART WITH 1;
ALTER SEQUENCE api_schema.image_id_seq RESTART WITH 1;
ALTER SEQUENCE api_schema.tags_id_seq RESTART WITH 1;
TRUNCATE api_schema.post CASCADE;

INSERT INTO api_schema."user" 
(username, email, user_password, first_name, last_name, phone_number, join_date, account_status, account_level, access_fee, profile_picture, profile_description) 
VALUES
('JohnSmith', 'JohnSmith@icloud.com', '$2b$12$8UG9xihkkW/VtEzS/n4HB.Tp9eaUh4Qo6Z1hpu/dFB5grWQX2Shm.', 'John', 'Smith', '+48123123123', '2024-01-01', 'active', 'user', 49, 'u1a.png', E'Creating landing pages is my life. Also you can ask me to be your teacher'),
('Administrator', 'user2@gmail.com', '$2b$12$KUxOZK5MrhmhPJReda8Rq.Cojxm9fZMqvOpEhfI7bAWGbsAsdVVOK', 'Peter', 'Thompson', '+48234234234', '2024-02-01', 'active', 'admin', 39, null, E'fair judge if I am in a good mood'),
('EthanParker', 'EthanParker@gmail.com', '$2b$12$QSLvy0lErSHHd4AQUnoDkewfIhXgL9MVZMqbvbRMxtPN5ugD9HZD.', 'Ethan', 'Parker', '+48555444333', '2024-03-01', 'active', 'admin', 59, 'u3a.png', E'"Designer" is my last name, "The Best" is my first.\r\nIf you have any ideas, message me immediately');

INSERT INTO api_schema.wallet (amount, user_id) 
VALUES
(100, 1),
(150, 2),
(200, 3);

INSERT INTO api_schema.inner_transaction (amount, transaction_date, user_id, wallet_id) 
VALUES
(100, '2024-01-15', 1, 1),
(150, '2024-02-15', 2, 2),
(200, '2024-03-15', 3, 3);

INSERT INTO api_schema.private_access (buyer_id, seller_id, transaction_id, price_at_time, access_date) 
VALUES
(1, 3, 1, 100, CURRENT_TIMESTAMP);

INSERT INTO api_schema.image_container (amount_of_images) 
VALUES 
(1),
(1),
(1),
(8),
(7);

INSERT INTO api_schema.image (image_file_path, container_id, user_id) 
VALUES
('u1a.png', null, 1),
('u2p1.png', 1, 2),
('u3p1.png', 2, 3),
('u1p2.png', 3, 1),
('u3p2s1.png', 4, 3),
('u3p2s2.png', 4, 3),
('u3p2s3.png', 4, 3),
('u3p2s4.png', 4, 3),
('u3p2s5.png', 4, 3),
('u3p2s6.png', 4, 3),
('u3p2s7.png', 4, 3),
('u3p2s8.png', 4, 3),
('u3p3s1.png', 5, 3),
('u3p3s2.png', 5, 3),
('u3p3s3.png', 5, 3),
('u3p3s4.png', 5, 3),
('u3p3s5.png', 5, 3),
('u3p3s6.png', 5, 3),
('u3p3s7.png', 5, 3),
('u3a.png', null, 3);

UPDATE api_schema.image_container
SET main_image_id = 2
WHERE id = 1;

UPDATE api_schema.image_container
SET main_image_id = 3
WHERE id = 2;

UPDATE api_schema.image_container
SET main_image_id = 4
WHERE id = 3;

UPDATE api_schema.image_container
SET main_image_id = 5
WHERE id = 4;

UPDATE api_schema.image_container
SET main_image_id = 13
WHERE id = 5;

INSERT INTO api_schema.tags (tag_name, tag_type) VALUES
('Logo', 'ui element'),
('Button', 'ui element'),
('Icon', 'ui element'),
('Card', 'ui element'),
('Article', 'ui element'),
('Input', 'ui element'),
('Gallery', 'ui element'),
('Landing', 'ui element'),
('List', 'ui element'),
('Mobile', 'style'),
('Desktop', 'style'),
('Glassmorphism', 'style'),
('Dark', 'style'),
('Light', 'style'),
('Minimalism', 'style'),
('Material', 'style'),
('Neumorphism', 'style'),
('Flat', 'style'),
('Cartoon', 'style'),
('Red', 'color'),
('Blue', 'color'),
('Green', 'color'),
('Yellow', 'color'),
('Black', 'color'),
('White', 'color'),
('Gray', 'color'),
('Purple', 'color'),
('Orange', 'color'),
('Brown', 'color'),
('Gradient', 'color');

INSERT INTO api_schema.post (user_id, post_name, post_text, container_id, post_date, likes, access_level) 
VALUES
(2, 'Visual Hierarchy', E'Visual hierarchy is the arrangement of elements in a way that implies importance. In UI design, it helps users quickly understand where to look and what to do.\r\n\r\nKey techniques:\r\n– Size: Larger elements draw attention first\r\n– Color & contrast: Bright or bold elements stand out\r\n– Position: Top and center are priority zones\r\n– Spacing: Grouping related items improves clarity\r\n', 1, '2024-01-10 10:30:00', 1679, 'public'),
(3, 'Landing Page Design', E'Here is an example of a well-balanced first page for an online school landing page. The most important thing is to leave plenty of space for a casual effect.', 2, '2024-02-10 14:20:00', 552, 'public'),
(1, 'Font Pairing', E'Here are some font pairs, that you can use in your project.\r\n\r\nDon’t forget to subscribe for my channel and press the like button', 3, '2024-03-10 09:15:00', 2546, 'public'),
(3, 'Glass Effect in Figma', E'This example you can use in your portfolio. A delightful composition of dark interface and glasmorphism. Just a few steps in figma', 4, '2024-04-10 12:00:00', 253, 'private'),
(3, 'Fresh style for Button', E'Look how easily you can create an AI style button in Figma', 5, '2024-05-10 11:00:00', 2847, 'public');

INSERT INTO api_schema.post_tags (post_id, tag_id)
SELECT p.id, t.id
FROM api_schema.post p, api_schema.tags t
WHERE (p.post_name, t.tag_name) IN
(('Visual Hierarchy', 'Article'),
 ('Visual Hierarchy', 'Landing'),
 ('Visual Hierarchy', 'Material'),
 ('Landing Page Design', 'Landing'),
 ('Landing Page Design', 'Flat'),
 ('Landing Page Design', 'Light'),
 ('Font Pairing', 'Article'),
 ('Font Pairing', 'List'),
 ('Glass Effect in Figma', 'Glassmorphism'),
 ('Glass Effect in Figma', 'Dark'),
 ('Glass Effect in Figma', 'Gradient'),
 ('Fresh style for Button', 'Button'),
 ('Fresh style for Button', 'Purple'));

INSERT INTO api_schema.reason (reason_name, reason_type)
VALUES
('Spam', 'user'),
('Inappropriate behavior', 'user'),
('Impersonation', 'user'),
('Inappropriate profile', 'user'),
('Other', 'user'),
('Spam', 'post'),
('Inappropriate content', 'post'),
('Content theft', 'post'),
('Misleading information', 'post'),
('Other', 'post');

INSERT INTO api_schema.user_report (reporter_id, reported_id, reason_id, report_date, description)
VALUES
(3, 1, 4, '2024-04-15', 'The user advertises private lessons in their profile');

INSERT INTO api_schema.post_report (reporter_id, reported_id, reason_id, report_date, description)
VALUES
(1, 4, 3, '2024-03-25', 'He is not the author of this post'),
(3, 1, 4, '2024-05-25', 'AI generated content, no sense at all');

INSERT INTO api_schema.user_rating (user_id, total_likes, last_updated)
VALUES
(1, 100, CURRENT_TIMESTAMP),
(2, 85, CURRENT_TIMESTAMP),
(3, 150, CURRENT_TIMESTAMP);