-- DraftGap Database Schema
-- MySQL 8.0+
-- Created: February 2026
-- FK constraints moved to bottom for easier relationship inspection

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

CREATE DATABASE IF NOT EXISTS `draftgap` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `draftgap`;

-- =====================================================
-- Core User & Authentication Tables
-- =====================================================

CREATE TABLE `users` (
  `user_id` CHAR(36) NOT NULL, -- GUID format
  `email` VARCHAR(255) NOT NULL,
  `password_hash` VARCHAR(255) NOT NULL,
  `riot_id` VARCHAR(100) NULL, -- Format: GameName#TagLine
  `riot_puuid` VARCHAR(78) NULL, -- Riot permanent player ID
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `last_sync` TIMESTAMP NULL, -- Last Riot API data sync
  `is_active` BOOLEAN NOT NULL DEFAULT TRUE,

  PRIMARY KEY (`user_id`),
  UNIQUE INDEX `uq_email` (`email`),
  UNIQUE INDEX `uq_riot_id` (`riot_id`),
  UNIQUE INDEX `uq_riot_puuid` (`riot_puuid`),
  INDEX `idx_last_sync` (`last_sync`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Riot API Player Data Tables
-- =====================================================

CREATE TABLE `players` (
  `player_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `puuid` VARCHAR(78) NOT NULL,
  `summoner_id` VARCHAR(63) NULL,
  `summoner_name` VARCHAR(100) NULL,
  `profile_icon_id` INT NULL,
  `summoner_level` INT NULL,
  `region` VARCHAR(10) NOT NULL, -- EUW1, NA1, KR, etc.
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  PRIMARY KEY (`player_id`),
  UNIQUE INDEX `uq_puuid` (`puuid`),
  INDEX `idx_summoner_id` (`summoner_id`),
  INDEX `idx_region` (`region`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `player_ranked_stats` (
  `ranked_stat_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `puuid` VARCHAR(78) NOT NULL,
  `queue_type` VARCHAR(50) NOT NULL, -- RANKED_SOLO_5x5, RANKED_FLEX_SR
  `tier` VARCHAR(20) NULL, -- IRON..CHALLENGER
  `rank` VARCHAR(5) NULL, -- I, II, III, IV
  `league_points` INT NULL,
  `wins` INT NOT NULL DEFAULT 0,
  `losses` INT NOT NULL DEFAULT 0,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  PRIMARY KEY (`ranked_stat_id`),
  UNIQUE INDEX `uq_puuid_queue` (`puuid`, `queue_type`),
  INDEX `idx_tier_rank` (`tier`, `rank`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Match Data Tables
-- =====================================================

CREATE TABLE `matches` (
  `match_id` VARCHAR(50) NOT NULL, -- e.g., EUW1_6847231920
  `game_creation` BIGINT NOT NULL, -- Unix timestamp in ms
  `game_duration` INT NOT NULL, -- seconds
  `game_mode` VARCHAR(50) NOT NULL, -- CLASSIC, ARAM, URF...
  `game_type` VARCHAR(50) NOT NULL, -- MATCHED_GAME, CUSTOM_GAME
  `queue_id` INT NOT NULL, -- 420=Ranked Solo, 440=Flex, etc.
  `platform_id` VARCHAR(10) NOT NULL,
  `game_version` VARCHAR(20) NOT NULL, -- Patch version
  `fetched_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (`match_id`),
  INDEX `idx_game_creation` (`game_creation`),
  INDEX `idx_queue_date` (`queue_id`, `game_creation`),
  INDEX `idx_platform_date` (`platform_id`, `game_creation`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `match_participants` (
  `participant_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `match_id` VARCHAR(50) NOT NULL,
  `puuid` VARCHAR(78) NOT NULL,

  `champion_id` INT NOT NULL,
  `champion_name` VARCHAR(50) NOT NULL,
  `team_id` INT NOT NULL, -- 100=Blue, 200=Red
  `team_position` VARCHAR(20) NOT NULL, -- TOP,JUNGLE,MIDDLE,BOTTOM,UTILITY
  `win` BOOLEAN NOT NULL,

  `kills` INT NOT NULL DEFAULT 0,
  `deaths` INT NOT NULL DEFAULT 0,
  `assists` INT NOT NULL DEFAULT 0,

  `gold_earned` INT NOT NULL DEFAULT 0,
  `total_damage_dealt` INT NOT NULL DEFAULT 0,
  `total_damage_dealt_to_champions` INT NOT NULL DEFAULT 0,
  `total_damage_taken` INT NOT NULL DEFAULT 0,
  `vision_score` INT NOT NULL DEFAULT 0,
  `cs` INT NOT NULL DEFAULT 0, -- Total minions killed

  `double_kills` INT NOT NULL DEFAULT 0,
  `triple_kills` INT NOT NULL DEFAULT 0,
  `quadra_kills` INT NOT NULL DEFAULT 0,
  `penta_kills` INT NOT NULL DEFAULT 0,
  `first_blood` BOOLEAN NOT NULL DEFAULT FALSE,

  `summoner1_id` INT NULL,
  `summoner2_id` INT NULL,

  `item0` INT NULL,
  `item1` INT NULL,
  `item2` INT NULL,
  `item3` INT NULL,
  `item4` INT NULL,
  `item5` INT NULL,
  `item6` INT NULL,

  `perk_primary_style` INT NULL,
  `perk_sub_style` INT NULL,
  `perk0` INT NULL,
  `perk1` INT NULL,
  `perk2` INT NULL,
  `perk3` INT NULL,
  `perk4` INT NULL,
  `perk5` INT NULL,

  PRIMARY KEY (`participant_id`),
  UNIQUE INDEX `uq_match_puuid` (`match_id`, `puuid`),
  INDEX `idx_puuid_analytics` (`puuid`, `win`, `champion_id`),
  INDEX `idx_champion_role` (`champion_id`, `team_position`),
  INDEX `idx_champion_role_win` (`champion_id`, `team_position`, `win`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Aggregated Statistics Tables (Performance)
-- =====================================================

CREATE TABLE `player_statistics_summary` (
  `summary_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `puuid` VARCHAR(78) NOT NULL,
  `queue_type` VARCHAR(50) NOT NULL, -- ALL, RANKED_SOLO, RANKED_FLEX, NORMAL

  `total_games` INT NOT NULL DEFAULT 0,
  `wins` INT NOT NULL DEFAULT 0,
  `losses` INT NOT NULL DEFAULT 0,
  `winrate` DECIMAL(5,2) NOT NULL DEFAULT 0.00,

  `avg_kills` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  `avg_deaths` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  `avg_assists` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  `kda_ratio` DECIMAL(5,2) NOT NULL DEFAULT 0.00,

  `avg_cs` DECIMAL(6,2) NOT NULL DEFAULT 0.00,
  `avg_vision_score` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  `avg_damage_dealt` DECIMAL(10,2) NOT NULL DEFAULT 0.00,

  `total_pentas` INT NOT NULL DEFAULT 0,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  PRIMARY KEY (`summary_id`),
  UNIQUE INDEX `uq_puuid_queue_summary` (`puuid`, `queue_type`),
  INDEX `idx_winrate_summary` (`winrate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `champion_pool_stats` (
  `champion_pool_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `puuid` VARCHAR(78) NOT NULL,

  `champion_id` INT NOT NULL,
  `champion_name` VARCHAR(50) NOT NULL,

  `games_played` INT NOT NULL DEFAULT 0,
  `wins` INT NOT NULL DEFAULT 0,
  `losses` INT NOT NULL DEFAULT 0,
  `winrate` DECIMAL(5,2) NOT NULL DEFAULT 0.00,

  `avg_kills` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  `avg_deaths` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  `avg_assists` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  `kda_ratio` DECIMAL(5,2) NOT NULL DEFAULT 0.00,

  `avg_cs` DECIMAL(6,2) NOT NULL DEFAULT 0.00,
  `avg_damage` DECIMAL(10,2) NOT NULL DEFAULT 0.00,

  `most_played_role` VARCHAR(20) NULL,
  `last_played` TIMESTAMP NULL,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  PRIMARY KEY (`champion_pool_id`),
  UNIQUE INDEX `uq_puuid_champion_pool` (`puuid`, `champion_id`),
  INDEX `idx_games_played_pool` (`games_played` DESC),
  INDEX `idx_winrate_pool` (`winrate` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `champion_matchups` (
  `matchup_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `puuid` VARCHAR(78) NOT NULL,

  `champion_id` INT NOT NULL,
  `opponent_champion_id` INT NOT NULL,
  `role` VARCHAR(20) NOT NULL,

  `games` INT NOT NULL DEFAULT 0,
  `wins` INT NOT NULL DEFAULT 0,
  `losses` INT NOT NULL DEFAULT 0,
  `winrate` DECIMAL(5,2) NOT NULL DEFAULT 0.00,

  `total_kills` INT NOT NULL DEFAULT 0,
  `total_deaths` INT NOT NULL DEFAULT 0,
  `total_assists` INT NOT NULL DEFAULT 0,
  `avg_cs_diff_15` DECIMAL(6,2) NULL,

  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  PRIMARY KEY (`matchup_id`),
  UNIQUE INDEX `uq_matchup` (`puuid`, `champion_id`, `opponent_champion_id`, `role`),
  INDEX `idx_champion_opponent` (`champion_id`, `opponent_champion_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Team Tracking Feature
-- =====================================================

CREATE TABLE `user_teams` (
  `team_id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `team_name` VARCHAR(100) NOT NULL,
  `team_tag` VARCHAR(10) NULL,
  `created_by` CHAR(36) NOT NULL, -- FIX: must match users.user_id type
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  PRIMARY KEY (`team_id`),
  INDEX `idx_created_by` (`created_by`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `user_team_members` (
  `team_id` INT UNSIGNED NOT NULL,
  `user_id` CHAR(36) NOT NULL, -- FIX: must match users.user_id type
  `role_in_team` VARCHAR(20) NULL,
  `joined_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (`team_id`, `user_id`),
  INDEX `idx_user_teams` (`user_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Data Sync Tracking
-- =====================================================

CREATE TABLE `sync_jobs` (
  `job_id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `puuid` VARCHAR(78) NOT NULL,
  `job_type` VARCHAR(50) NOT NULL, -- MATCH_HISTORY, RANKED_STATS, FULL_SYNC
  `status` VARCHAR(20) NOT NULL, -- PENDING, IN_PROGRESS, COMPLETED, FAILED
  `matches_processed` INT NOT NULL DEFAULT 0,
  `started_at` TIMESTAMP NULL,
  `completed_at` TIMESTAMP NULL,
  `error_message` TEXT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

  PRIMARY KEY (`job_id`),
  INDEX `idx_status` (`status`, `created_at`),
  INDEX `idx_puuid_status` (`puuid`, `status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- Static Data Cache (Optional)
-- =====================================================

CREATE TABLE `champions` (
  `champion_id` INT NOT NULL,
  `champion_key` VARCHAR(50) NOT NULL,
  `champion_name` VARCHAR(100) NOT NULL,
  `title` VARCHAR(100) NULL,
  `image_url` VARCHAR(255) NULL,
  `version` VARCHAR(20) NOT NULL,
  PRIMARY KEY (`champion_id`),
  INDEX `idx_champion_key` (`champion_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `items` (
  `item_id` INT NOT NULL,
  `item_name` VARCHAR(300) NOT NULL,
  `description` TEXT NULL,
  `gold_cost` INT NULL,
  `image_url` VARCHAR(255) NULL,
  `version` VARCHAR(20) NOT NULL,
  PRIMARY KEY (`item_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `summoner_spells` (
  `spell_id` INT NOT NULL,
  `spell_key` VARCHAR(50) NOT NULL,
  `spell_name` VARCHAR(50) NOT NULL,
  `description` TEXT NULL,
  `cooldown` INT NULL,
  `image_url` VARCHAR(255) NULL,
  `version` VARCHAR(20) NOT NULL,
  PRIMARY KEY (`spell_id`),
  INDEX `idx_spell_key` (`spell_key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =====================================================
-- FOREIGN KEYS (all in one place)
-- =====================================================

ALTER TABLE `player_ranked_stats`
  ADD CONSTRAINT `fk_player_ranked_stats_players_puuid`
  FOREIGN KEY (`puuid`) REFERENCES `players` (`puuid`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `match_participants`
  ADD CONSTRAINT `fk_match_participants_matches_match_id`
  FOREIGN KEY (`match_id`) REFERENCES `matches` (`match_id`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `match_participants`
  ADD CONSTRAINT `fk_match_participants_players_puuid`
  FOREIGN KEY (`puuid`) REFERENCES `players` (`puuid`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `player_statistics_summary`
  ADD CONSTRAINT `fk_player_statistics_summary_players_puuid`
  FOREIGN KEY (`puuid`) REFERENCES `players` (`puuid`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `champion_pool_stats`
  ADD CONSTRAINT `fk_champion_pool_stats_players_puuid`
  FOREIGN KEY (`puuid`) REFERENCES `players` (`puuid`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `champion_matchups`
  ADD CONSTRAINT `fk_champion_matchups_players_puuid`
  FOREIGN KEY (`puuid`) REFERENCES `players` (`puuid`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `user_teams`
  ADD CONSTRAINT `fk_user_teams_users_created_by`
  FOREIGN KEY (`created_by`) REFERENCES `users` (`user_id`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `user_team_members`
  ADD CONSTRAINT `fk_user_team_members_user_teams_team_id`
  FOREIGN KEY (`team_id`) REFERENCES `user_teams` (`team_id`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `user_team_members`
  ADD CONSTRAINT `fk_user_team_members_users_user_id`
  FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`)
  ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `sync_jobs`
  ADD CONSTRAINT `fk_sync_jobs_players_puuid`
  FOREIGN KEY (`puuid`) REFERENCES `players` (`puuid`)
  ON DELETE CASCADE ON UPDATE CASCADE;

-- =====================================================
-- Stored Procedures for Aggregation
-- =====================================================

DELIMITER $$

CREATE PROCEDURE `UpdatePlayerStatistics`(IN p_puuid VARCHAR(78))
BEGIN
  INSERT INTO player_statistics_summary (
    puuid, queue_type, total_games, wins, losses, winrate,
    avg_kills, avg_deaths, avg_assists, kda_ratio, avg_cs, avg_vision_score, avg_damage_dealt, total_pentas
  )
  SELECT
    p_puuid,
    'ALL',
    COUNT(*),
    SUM(CASE WHEN win = TRUE THEN 1 ELSE 0 END),
    SUM(CASE WHEN win = FALSE THEN 1 ELSE 0 END),
    ROUND((SUM(CASE WHEN win = TRUE THEN 1 ELSE 0 END) / COUNT(*)) * 100, 2),
    ROUND(AVG(kills), 2),
    ROUND(AVG(deaths), 2),
    ROUND(AVG(assists), 2),
    ROUND(CASE WHEN AVG(deaths) = 0 THEN AVG(kills + assists) ELSE AVG((kills + assists) / deaths) END, 2),
    ROUND(AVG(cs), 2),
    ROUND(AVG(vision_score), 2),
    ROUND(AVG(total_damage_dealt_to_champions), 2),
    SUM(penta_kills)
  FROM match_participants
  WHERE puuid = p_puuid
  ON DUPLICATE KEY UPDATE
    total_games = VALUES(total_games),
    wins = VALUES(wins),
    losses = VALUES(losses),
    winrate = VALUES(winrate),
    avg_kills = VALUES(avg_kills),
    avg_deaths = VALUES(avg_deaths),
    avg_assists = VALUES(avg_assists),
    kda_ratio = VALUES(kda_ratio),
    avg_cs = VALUES(avg_cs),
    avg_vision_score = VALUES(avg_vision_score),
    avg_damage_dealt = VALUES(avg_damage_dealt),
    total_pentas = VALUES(total_pentas),
    updated_at = CURRENT_TIMESTAMP;
END$$

CREATE PROCEDURE `UpdateChampionPoolStats`(IN p_puuid VARCHAR(78))
BEGIN
  INSERT INTO champion_pool_stats (
    puuid, champion_id, champion_name, games_played, wins, losses, winrate,
    avg_kills, avg_deaths, avg_assists, kda_ratio, avg_cs, avg_damage, most_played_role, last_played
  )
  SELECT
    mp.puuid,
    mp.champion_id,
    mp.champion_name,
    COUNT(*) as games,
    SUM(CASE WHEN mp.win = TRUE THEN 1 ELSE 0 END) as wins,
    SUM(CASE WHEN mp.win = FALSE THEN 1 ELSE 0 END) as losses,
    ROUND((SUM(CASE WHEN mp.win = TRUE THEN 1 ELSE 0 END) / COUNT(*)) * 100, 2) as wr,
    ROUND(AVG(mp.kills), 2),
    ROUND(AVG(mp.deaths), 2),
    ROUND(AVG(mp.assists), 2),
    ROUND(CASE WHEN AVG(mp.deaths) = 0 THEN AVG(mp.kills + mp.assists) ELSE AVG((mp.kills + mp.assists) / mp.deaths) END, 2),
    ROUND(AVG(mp.cs), 2),
    ROUND(AVG(mp.total_damage_dealt_to_champions), 2),
    (SELECT team_position
     FROM match_participants
     WHERE puuid = mp.puuid AND champion_id = mp.champion_id
     GROUP BY team_position
     ORDER BY COUNT(*) DESC
     LIMIT 1) as role,
    MAX(m.game_creation)
  FROM match_participants mp
  INNER JOIN matches m ON mp.match_id = m.match_id
  WHERE mp.puuid = p_puuid
  GROUP BY mp.puuid, mp.champion_id, mp.champion_name
  ON DUPLICATE KEY UPDATE
    games_played = VALUES(games_played),
    wins = VALUES(wins),
    losses = VALUES(losses),
    winrate = VALUES(winrate),
    avg_kills = VALUES(avg_kills),
    avg_deaths = VALUES(avg_deaths),
    avg_assists = VALUES(avg_assists),
    kda_ratio = VALUES(kda_ratio),
    avg_cs = VALUES(avg_cs),
    avg_damage = VALUES(avg_damage),
    most_played_role = VALUES(most_played_role),
    last_played = VALUES(last_played),
    updated_at = CURRENT_TIMESTAMP;
END$$

DELIMITER ;

-- =====================================================
-- Initial Admin User (Change password immediately!)
-- =====================================================

-- Password: 'admin123' - CHANGE THIS IN PRODUCTION!
INSERT INTO `users` (`user_id`, `email`, `password_hash`, `is_active`)
VALUES ('aaaaaaaa-0000-1111-2222-bbbbbbbbbbbb', 'admin@draftgap.local', '$2a$12$i5pRBHCbqnrfCMsXmw6/1e2hz4/FgKWXTsdMV5B5.vpEHxsNwYOTe', TRUE);

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
