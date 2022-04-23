CREATE TABLE IF NOT EXISTS `fmt_ticket` ( 
    `channel_id` bigint(20) NOT NULL UNIQUE,
    `user_id` bigint(20) NOT NULL,
    `controller_id` bigint(20) NOT NULL,
    `panel_id` bigint(20) NOT NULL,
    `open_date` datetime NOT NULL,
    PRIMARY KEY (`channel_id`)
) DEFAULT CHARSET=utf32;

CREATE TABLE IF NOT EXISTS `fmt_ticket_panel` ( 
    `message_id` bigint(20) NOT NULL UNIQUE,
    `channel_id` bigint(20) NOT NULL,
    `type` varchar(255) NOT NULL,
    `json_data` text NOT NULL,
    PRIMARY KEY (`message_id`)
) DEFAULT CHARSET=utf32;