CREATE TABLE IF NOT EXISTS `rr_reaction_roles` (
    `id` bigint(64) NOT NULL UNIQUE AUTO_INCREMENT,
    `guild_id` bigint(64) NOT NULL,
    `channel_id` bigint(64) NOT NULL,
    `message_id` bigint(64) NOT NULL,
    `role_ids` text NOT NULL,
    `behaviour` tinyint(8) NOT NULL DEFAULT 0,
    `emoji` varchar(128),
    `button_id` varchar(128),
    `send_message` tinyint(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (`id`)
) DEFAULT CHARSET utf8mb4 COLLATE utf8mb4_bin;