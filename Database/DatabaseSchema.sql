CREATE TABLE IF NOT EXISTS users 
(
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    token VARCHAR(255),
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(128) NOT NULL,
    name VARCHAR(20),
    bio VARCHAR(255),
    image VARCHAR(10)
);

CREATE TABLE IF NOT EXISTS user_stats 
(
    elo INT DEFAULT 100 NOT NULL,
    coins INT DEFAULT 20 NOT NULL,
    games_played INT DEFAULT 0 NOT NULL,
    wins INT DEFAULT 0 NOT NULL,
    losses INT DEFAULT 0 NOT NULL,
    user_id UUID UNIQUE,
    FOREIGN KEY (user_id) REFERENCES users (id)
);

CREATE TABLE IF NOT EXISTS shop 
(
    id INT PRIMARY KEY
);

CREATE TABLE IF NOT EXISTS package
(
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    shop_id INT,
    FOREIGN KEY (shop_id) REFERENCES shop(id)
);

CREATE TABLE IF NOT EXISTS card 
(
    id VARCHAR(255) PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    damage DOUBLE PRECISION NOT NULL,
    package_id UUID,
    FOREIGN KEY (package_id) REFERENCES package(id)
);

CREATE TABLE IF NOT EXISTS stack 
(
    user_id UUID,
    card_id VARCHAR(255),
    PRIMARY KEY (user_id, card_id),
    FOREIGN KEY (user_id) REFERENCES user_stats (user_id),
    FOREIGN KEY (card_id) REFERENCES card (id)
);

CREATE TABLE IF NOT EXISTS deck 
(
    user_id UUID,
    card_id VARCHAR(255),
    PRIMARY KEY (user_id, card_id),
    FOREIGN KEY (user_id, card_id) REFERENCES stack (user_id, card_id)
);
