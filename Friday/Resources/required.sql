CREATE TABLE IF NOT EXISTS `guild_config` (
    `id` bigint(64) NOT NULL UNIQUE,
    `prefix` varchar(16) NOT NULL,
    `language` varchar(16) NOT NULL,
    PRIMARY KEY (`id`)
) DEFAULT CHARSET=utf32;

CREATE TABLE IF NOT EXISTS `user_config` (
    `id` bigint(64) NOT NULL UNIQUE,
    `prefix_override` varchar(16),
    `language_override` varchar(16),
    PRIMARY KEY (`id`)
) DEFAULT CHARSET=utf32;

CREATE TABLE IF NOT EXISTS `friday_moderators` (
    `id` bigint(64) NOT NULL UNIQUE AUTO_INCREMENT,
    `user_id` bigint(64) NOT NULL,
    PRIMARY KEY (`id`)
) DEFAULT CHARSET=utf32;

CREATE TABLE IF NOT EXISTS `friday_verified_servers` (
    `id` bigint(64) NOT NULL UNIQUE AUTO_INCREMENT,
    `server_id` bigint(64) NOT NULL,
    PRIMARY KEY (`id`)
) DEFAULT CHARSET=utf32;



