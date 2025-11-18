﻿CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `AuthorPlans` (
    `AuthorPlanId` int NOT NULL AUTO_INCREMENT,
    `AuthorId` int NOT NULL,
    `PlanId` int NOT NULL,
    `StartDate` datetime(6) NOT NULL,
    `EndDate` datetime(6) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `TrialUsed` tinyint(1) NOT NULL,
    `PaymentReference` varchar(255) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `CancelledAt` datetime(6) NULL,
    `CancellationReason` varchar(500) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AuthorPlans` PRIMARY KEY (`AuthorPlanId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `BookVersions` (
    `BookVersionId` int NOT NULL AUTO_INCREMENT,
    `BookId` int NOT NULL,
    `ChapterId` int NULL,
    `ContentSnapshot` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ChangedByUserId` int NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `Reason` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_BookVersions` PRIMARY KEY (`BookVersionId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Categories` (
    `CategoryId` int NOT NULL AUTO_INCREMENT,
    `CategoryName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Categories` PRIMARY KEY (`CategoryId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Languages` (
    `LanguageId` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Country` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Status` varchar(12) CHARACTER SET utf8mb4 NOT NULL,
    `IsActive` tinyint(100) NOT NULL,
    CONSTRAINT `PK_Languages` PRIMARY KEY (`LanguageId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Plans` (
    `PlanId` int NOT NULL AUTO_INCREMENT,
    `PlanName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PlanDescription` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PlanRate` decimal(10,2) NOT NULL,
    `PlanDays` int NOT NULL,
    `PlanHours` int NOT NULL,
    `MaxEBooks` int NOT NULL,
    `AllowDownloads` tinyint(1) NOT NULL,
    `AllowFullDashboard` tinyint(1) NOT NULL,
    `AllowAnalytics` tinyint(1) NOT NULL,
    `AllowPublishing` tinyint(1) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreateddAt` datetime(6) NOT NULL,
    `MaxPages` int NOT NULL,
    `MaxChapters` int NOT NULL,
    CONSTRAINT `PK_Plans` PRIMARY KEY (`PlanId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `PubCosts` (
    `CostId` int NOT NULL AUTO_INCREMENT,
    `AuthorId` int NOT NULL,
    `AuthorCode` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `BookId` int NOT NULL,
    `PriceId` int NOT NULL,
    `Amount` decimal(10,2) NOT NULL,
    `Currency` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `Status` varchar(12) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_PubCosts` PRIMARY KEY (`CostId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `RecordStatus` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Status` varchar(12) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `CreatedDate` datetime(6) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    CONSTRAINT `PK_RecordStatus` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Roles` (
    `RoleId` int NOT NULL AUTO_INCREMENT,
    `RoleName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AllowDownloads` tinyint(1) NOT NULL,
    `AllowFullDashboard` tinyint(1) NOT NULL,
    `AllowAnalytics` tinyint(1) NOT NULL,
    `AllowPublishing` tinyint(1) NOT NULL,
    `AllowDelete` tinyint(1) NOT NULL,
    `AllowEdit` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`RoleId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `UserId` int NOT NULL AUTO_INCREMENT,
    `FullName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `UserEmail` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Password` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ConfirmPassword` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SecretQuestion` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SecretQuestionAnswer` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `LastLoginAt` datetime(6) NOT NULL,
    `RoleId` int NOT NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`UserId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Authors` (
    `AuthorId` int NOT NULL AUTO_INCREMENT,
    `AuthorCode` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `FullName` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Compellation` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AuthorEmail` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Specialization` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Qualification` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CategoryId` int NOT NULL,
    `Country` longtext CHARACTER SET utf8mb4 NOT NULL,
    `City` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Region` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PostalCode` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CountryCode` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Phone` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Address` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Newsletter` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `AuthorPlansAuthorPlanId` int NULL,
    CONSTRAINT `PK_Authors` PRIMARY KEY (`AuthorId`),
    CONSTRAINT `FK_Authors_AuthorPlans_AuthorPlansAuthorPlanId` FOREIGN KEY (`AuthorPlansAuthorPlanId`) REFERENCES `AuthorPlans` (`AuthorPlanId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Books` (
    `BookId` int NOT NULL AUTO_INCREMENT,
    `AuthorId` int NOT NULL,
    `CategoryId` int NOT NULL,
    `Title` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
    `Subtitle` longtext CHARACTER SET utf8mb4 NULL,
    `AuthorCode` longtext CHARACTER SET utf8mb4 NOT NULL,
    `LanguageId` int NOT NULL,
    `CoverImagePath` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ManuscriptPath` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Genre` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `WordCount` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CategoriesCategoryId` int NULL,
    CONSTRAINT `PK_Books` PRIMARY KEY (`BookId`),
    CONSTRAINT `FK_Books_Authors_AuthorId` FOREIGN KEY (`AuthorId`) REFERENCES `Authors` (`AuthorId`) ON DELETE CASCADE,
    CONSTRAINT `FK_Books_Categories_CategoriesCategoryId` FOREIGN KEY (`CategoriesCategoryId`) REFERENCES `Categories` (`CategoryId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `BookPrices` (
    `PriceId` int NOT NULL AUTO_INCREMENT,
    `BookVersionId` int NOT NULL,
    `BookId` int NOT NULL,
    `AuthorId` int NOT NULL,
    `AuthorCode` longtext CHARACTER SET utf8mb4 NOT NULL,
    `bookPrice` decimal(10,2) NOT NULL,
    `Currency` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `BooksBookId` int NULL,
    CONSTRAINT `PK_BookPrices` PRIMARY KEY (`PriceId`),
    CONSTRAINT `FK_BookPrices_Books_BooksBookId` FOREIGN KEY (`BooksBookId`) REFERENCES `Books` (`BookId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Chapters` (
    `ChapterId` int NOT NULL AUTO_INCREMENT,
    `BookId` int NOT NULL,
    `Title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `SubTitle` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `LanguageId` int NOT NULL,
    `OrderIndex` int NOT NULL,
    `WordCount` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `UpdatedByUserId` int NOT NULL,
    `IsPublished` tinyint(1) NOT NULL,
    `Status` longtext CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_Chapters` PRIMARY KEY (`ChapterId`),
    CONSTRAINT `FK_Chapters_Books_BookId` FOREIGN KEY (`BookId`) REFERENCES `Books` (`BookId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_Authors_AuthorCode` ON `Authors` (`AuthorCode`);

CREATE INDEX `IX_Authors_AuthorPlansAuthorPlanId` ON `Authors` (`AuthorPlansAuthorPlanId`);

CREATE INDEX `IX_BookPrices_BooksBookId` ON `BookPrices` (`BooksBookId`);

CREATE INDEX `IX_Books_AuthorId` ON `Books` (`AuthorId`);

CREATE INDEX `IX_Books_CategoriesCategoryId` ON `Books` (`CategoriesCategoryId`);

CREATE INDEX `IX_Chapters_BookId` ON `Chapters` (`BookId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250928103205_InitialCreate', '9.0.9');

CREATE INDEX `IX_Users_RoleId` ON `Users` (`RoleId`);

ALTER TABLE `Users` ADD CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`RoleId`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250929172128_1stMigration', '9.0.9');

INSERT INTO `Plans` (`PlanId`, `AllowAnalytics`, `AllowDownloads`, `AllowFullDashboard`, `AllowPublishing`, `CreateddAt`, `IsActive`, `MaxChapters`, `MaxEBooks`, `MaxPages`, `PlanDays`, `PlanDescription`, `PlanHours`, `PlanName`, `PlanRate`)
VALUES (1, FALSE, FALSE, FALSE, FALSE, TIMESTAMP '2025-09-30 20:07:30', FALSE, 0, 0, 0, 30, 'Free 1-month trial', 0, 'Free Trial', 0.0),
(2, FALSE, FALSE, FALSE, FALSE, TIMESTAMP '2025-09-30 20:07:30', FALSE, 0, 0, 0, 30, 'Basic monthly subscription', 0, 'Basic Plan', 9.99),
(3, FALSE, FALSE, FALSE, FALSE, TIMESTAMP '2025-09-30 20:07:30', FALSE, 0, 0, 0, 365, 'Yearly subscription', 0, 'Pro Plan', 99.99);

CREATE INDEX `IX_AuthorPlans_PlanId` ON `AuthorPlans` (`PlanId`);

ALTER TABLE `AuthorPlans` ADD CONSTRAINT `FK_AuthorPlans_Plans_PlanId` FOREIGN KEY (`PlanId`) REFERENCES `Plans` (`PlanId`) ON DELETE CASCADE;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250930200732_1stMigrationSk', '9.0.9');

CREATE TABLE `Features` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Key` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `Price` decimal(10,2) NOT NULL,
    `Type` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Features` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `TemporaryFeatures` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `SessionId` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FeatureId` int NOT NULL,
    `UserId` longtext CHARACTER SET utf8mb4 NOT NULL,
    `AddedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_TemporaryFeatures` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_TemporaryFeatures_Features_FeatureId` FOREIGN KEY (`FeatureId`) REFERENCES `Features` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserFeatures` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` longtext CHARACTER SET utf8mb4 NOT NULL,
    `FeatureId` int NOT NULL,
    `AuthorPlanId` int NULL,
    `AddedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_UserFeatures` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserFeatures_AuthorPlans_AuthorPlanId` FOREIGN KEY (`AuthorPlanId`) REFERENCES `AuthorPlans` (`AuthorPlanId`) ON DELETE SET NULL,
    CONSTRAINT `FK_UserFeatures_Features_FeatureId` FOREIGN KEY (`FeatureId`) REFERENCES `Features` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

INSERT INTO `Features` (`Id`, `CreatedAt`, `Description`, `IsActive`, `Key`, `Name`, `Price`, `Type`)
VALUES (1, TIMESTAMP '2025-10-15 18:29:18', 'Create and format professional eBooks with our easy-to-use editor.', TRUE, 'ebook_creation', 'EBook Creation', 0.0, 'Basic'),
(2, TIMESTAMP '2025-10-15 18:29:18', 'Design stunning book covers with our AI-powered tool.', TRUE, 'cover_design', 'Cover Design', 19.99, 'Premium'),
(3, TIMESTAMP '2025-10-15 18:29:18', 'Convert your book into a professional audiobook.', TRUE, 'audio_book', 'Audio Book', 49.99, 'Premium'),
(4, TIMESTAMP '2025-10-15 18:29:18', 'Professional proofreading and editing services.', TRUE, 'proofreading', 'Proofreading', 29.99, 'Premium'),
(5, TIMESTAMP '2025-10-15 18:29:18', 'Track your sales and reader engagement with detailed analytics.', TRUE, 'analytics', 'Analytics', 14.99, 'Premium'),
(6, TIMESTAMP '2025-10-15 18:29:18', 'Promote your book with our integrated marketing suite.', TRUE, 'marketing_tools', 'Marketing Tools', 24.99, 'Marketing');

UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-10-15 18:29:18'
WHERE `PlanId` = 1;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-10-15 18:29:18'
WHERE `PlanId` = 2;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-10-15 18:29:18'
WHERE `PlanId` = 3;
SELECT ROW_COUNT();


INSERT INTO `Plans` (`PlanId`, `AllowAnalytics`, `AllowDownloads`, `AllowFullDashboard`, `AllowPublishing`, `CreateddAt`, `IsActive`, `MaxChapters`, `MaxEBooks`, `MaxPages`, `PlanDays`, `PlanDescription`, `PlanHours`, `PlanName`, `PlanRate`)
VALUES (4, FALSE, FALSE, FALSE, FALSE, TIMESTAMP '2025-10-15 18:29:18', FALSE, 0, 0, 0, 365, 'Premium yearly subscription with all features', 0, 'Premium Plan', 199.99);

INSERT INTO `Roles` (`RoleId`, `AllowAnalytics`, `AllowDelete`, `AllowDownloads`, `AllowEdit`, `AllowFullDashboard`, `AllowPublishing`, `Description`, `RoleName`)
VALUES (1, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, 'Administrator with full access', 'Admin'),
(2, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, 'Author with publishing access', 'Author'),
(3, FALSE, FALSE, FALSE, FALSE, FALSE, FALSE, 'Reader with limited access', 'Reader');

CREATE INDEX `IX_TemporaryFeatures_FeatureId` ON `TemporaryFeatures` (`FeatureId`);

CREATE INDEX `IX_UserFeatures_AuthorPlanId` ON `UserFeatures` (`AuthorPlanId`);

CREATE INDEX `IX_UserFeatures_FeatureId` ON `UserFeatures` (`FeatureId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251015182920_AddFeatureCartTables', '9.0.9');

UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `Id` = 1;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `Id` = 2;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `Id` = 3;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `Id` = 4;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `Id` = 5;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `Id` = 6;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `PlanId` = 1;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `PlanId` = 2;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `PlanId` = 3;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-10-15 18:33:05'
WHERE `PlanId` = 4;
SELECT ROW_COUNT();


INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251015183306_UpdateFeatureCartTables', '9.0.9');

UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `Id` = 1;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `Id` = 2;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `Id` = 3;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `Id` = 4;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `Id` = 5;
SELECT ROW_COUNT();


UPDATE `Features` SET `CreatedAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `Id` = 6;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `PlanId` = 1;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `PlanId` = 2;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `PlanId` = 3;
SELECT ROW_COUNT();


UPDATE `Plans` SET `CreateddAt` = TIMESTAMP '2025-01-01 00:00:00'
WHERE `PlanId` = 4;
SELECT ROW_COUNT();


CREATE TABLE `UserPreferences` (
    `PreferenceId` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Value` varchar(500) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_UserPreferences` PRIMARY KEY (`PreferenceId`),
    CONSTRAINT `FK_UserPreferences_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_UserPreferences_UserId_Key` ON `UserPreferences` (`UserId`, `Key`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20251015183409_FixDateTimeSeedData', '9.0.9');

COMMIT;

