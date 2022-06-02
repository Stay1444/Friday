CREATE TABLE IF NOT EXISTS `mgs_2048_leaderboard` (
    `id` bigint(64) UNIQUE NOT NULL,
    `max_score` bigint(64) NOT NULL,
    `total_score` bigint(64) NOT NULL,
    `played` bigint(64) NOT NULL,
    `playtime_seconds` bigint(64) NOT NULL,
    `recorded_username` TEXT NOT NULL,
    PRIMARY KEY (`id`)
)