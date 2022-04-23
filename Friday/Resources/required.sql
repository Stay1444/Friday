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