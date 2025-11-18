-- Update existing database with new tables and data

-- Add missing columns to existing tables if they don't exist
ALTER TABLE `Features` ADD COLUMN IF NOT EXISTS `CreatedAt` datetime(6) NOT NULL DEFAULT '2025-01-01 00:00:00';

ALTER TABLE `Plans` ADD COLUMN IF NOT EXISTS `CreateddAt` datetime(6) NOT NULL DEFAULT '2025-01-01 00:00:00';

-- Create missing tables
CREATE TABLE IF NOT EXISTS `UserPreferences` (
    `PreferenceId` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`PreferenceId`),
    INDEX `IX_UserPreferences_UserId_Key` (`UserId`, `Key`),
    CONSTRAINT `FK_UserPreferences_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `TemporaryFeatures` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `SessionId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `FeatureId` int NOT NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `AddedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    INDEX `IX_TemporaryFeatures_FeatureId` (`FeatureId`),
    CONSTRAINT `FK_TemporaryFeatures_Features_FeatureId` FOREIGN KEY (`FeatureId`) REFERENCES `Features` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS `UserFeatures` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `FeatureId` int NOT NULL,
    `AuthorPlanId` int NULL,
    `AddedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    INDEX `IX_UserFeatures_FeatureId` (`FeatureId`),
    INDEX `IX_UserFeatures_AuthorPlanId` (`AuthorPlanId`),
    CONSTRAINT `FK_UserFeatures_Features_FeatureId` FOREIGN KEY (`FeatureId`) REFERENCES `Features` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserFeatures_AuthorPlans_AuthorPlanId` FOREIGN KEY (`AuthorPlanId`) REFERENCES `AuthorPlans` (`AuthorPlanId`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

-- Update existing feature records with CreatedAt values
UPDATE `Features` SET `CreatedAt` = '2025-01-01 00:00:00' WHERE `Id` = 1 AND (`CreatedAt` = '0000-00-00 00:00:00' OR `CreatedAt` IS NULL);
UPDATE `Features` SET `CreatedAt` = '2025-01-01 00:00:00' WHERE `Id` = 2 AND (`CreatedAt` = '0000-00-00 00:00:00' OR `CreatedAt` IS NULL);
UPDATE `Features` SET `CreatedAt` = '2025-01-01 00:00:00' WHERE `Id` = 3 AND (`CreatedAt` = '0000-00-00 00:00:00' OR `CreatedAt` IS NULL);
UPDATE `Features` SET `CreatedAt` = '2025-01-01 00:00:00' WHERE `Id` = 4 AND (`CreatedAt` = '0000-00-00 00:00:00' OR `CreatedAt` IS NULL);
UPDATE `Features` SET `CreatedAt` = '2025-01-01 00:00:00' WHERE `Id` = 5 AND (`CreatedAt` = '0000-00-00 00:00:00' OR `CreatedAt` IS NULL);
UPDATE `Features` SET `CreatedAt` = '2025-01-01 00:00:00' WHERE `Id` = 6 AND (`CreatedAt` = '0000-00-00 00:00:00' OR `CreatedAt` IS NULL);

-- Update existing plan records with CreateddAt values
UPDATE `Plans` SET `CreateddAt` = '2025-01-01 00:00:00' WHERE `PlanId` = 1 AND (`CreateddAt` = '0000-00-00 00:00:00' OR `CreateddAt` IS NULL);
UPDATE `Plans` SET `CreateddAt` = '2025-01-01 00:00:00' WHERE `PlanId` = 2 AND (`CreateddAt` = '0000-00-00 00:00:00' OR `CreateddAt` IS NULL);
UPDATE `Plans` SET `CreateddAt` = '2025-01-01 00:00:00' WHERE `PlanId` = 3 AND (`CreateddAt` = '0000-00-00 00:00:00' OR `CreateddAt` IS NULL);
UPDATE `Plans` SET `CreateddAt` = '2025-01-01 00:00:00' WHERE `PlanId` = 4 AND (`CreateddAt` = '0000-00-00 00:00:00' OR `CreateddAt` IS NULL);

-- Add missing plans if they don't exist
INSERT IGNORE INTO `Plans` (`PlanId`, `AllowAnalytics`, `AllowDownloads`, `AllowFullDashboard`, `AllowPublishing`, `CreateddAt`, `IsActive`, `MaxChapters`, `MaxEBooks`, `MaxPages`, `PlanDays`, `PlanDescription`, `PlanHours`, `PlanName`, `PlanRate`)
VALUES (4, FALSE, FALSE, FALSE, FALSE, '2025-01-01 00:00:00', FALSE, 0, 0, 0, 365, 'Premium yearly subscription with all features', 0, 'Premium Plan', 199.99);

-- Add missing features if they don't exist
INSERT IGNORE INTO `Features` (`Id`, `CreatedAt`, `Description`, `IsActive`, `Key`, `Name`, `Price`, `Type`)
VALUES 
(1, '2025-01-01 00:00:00', 'Create and format professional eBooks with our easy-to-use editor.', TRUE, 'ebook_creation', 'EBook Creation', 0.0, 'Basic'),
(2, '2025-01-01 00:00:00', 'Design stunning book covers with our AI-powered tool.', TRUE, 'cover_design', 'Cover Design', 19.99, 'Premium'),
(3, '2025-01-01 00:00:00', 'Convert your book into a professional audiobook.', TRUE, 'audio_book', 'Audio Book', 49.99, 'Premium'),
(4, '2025-01-01 00:00:00', 'Professional proofreading and editing services.', TRUE, 'proofreading', 'Proofreading', 29.99, 'Premium'),
(5, '2025-01-01 00:00:00', 'Track your sales and reader engagement with detailed analytics.', TRUE, 'analytics', 'Analytics', 14.99, 'Premium'),
(6, '2025-01-01 00:00:00', 'Promote your book with our integrated marketing suite.', TRUE, 'marketing_tools', 'Marketing Tools', 24.99, 'Marketing');

-- Update the migration history to mark these migrations as applied
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES 
('20251015182920_AddFeatureCartTables', '9.0.9'),
('20251015183306_UpdateFeatureCartTables', '9.0.9'),
('20251015183409_FixDateTimeSeedData', '9.0.9');