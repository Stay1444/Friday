CREATE TABLE IF NOT EXISTS `antiraid_settings` (
    `id` bigint(64) NOT NULL UNIQUE,
    `settings` text NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `antiraid_whitelist` (
    `id` bigint(64) NOT NULL AUTO_INCREMENT UNIQUE,
    `guild_id` bigint(64) NOT NULL,
    `user_id` bigint(64) NOT NULL,
    PRIMARY KEY (`id`)
);