CREATE TABLE IF NOT EXISTS `gcs_channels` (
    `id` bigint(64) NOT NULL UNIQUE,
    `guild_id` bigint(64) NOT NULL,
    `value` text NOT NULL
)