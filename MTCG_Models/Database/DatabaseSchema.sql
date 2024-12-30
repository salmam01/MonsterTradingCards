CREATE TABLE IF NOT EXISTS player 
(
    id UUID DEFAULT gen_random_uuid() PRIMARY KEY,
    token VARCHAR(255),
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(120) NOT NULL
);

CREATE TABLE IF NOT EXISTS player_stats 
(
    elo INT DEFAULT 100 NOT NULL,
    coins INT DEFAULT 20 NOT NULL,
    games INT DEFAULT 0 NOT NULL,
    wins INT DEFAULT 0 NOT NULL,
    losses INT DEFAULT 0 NOT NULL,
    player_id UUID UNIQUE,
    FOREIGN KEY (player_id) REFERENCES player (id)
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
    player_id UUID,
    card_id VARCHAR(255),
    PRIMARY KEY (player_id, card_id),
    FOREIGN KEY (player_id, card_id) REFERENCES stack (player_id, card_id)
);

CREATE TABLE IF NOT EXISTS deck 
(
    player_id UUID,
    card_id VARCHAR(255),
    PRIMARY KEY (player_id, card_id),
    FOREIGN KEY (player_id, card_id) REFERENCES stack (player_id, card_id)
);