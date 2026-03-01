-- Add SelectedPaletteId FK constraint to SystemSettings

SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;
SET time_zone = '+00:00';

ALTER TABLE `SystemSettings`
  ADD COLUMN `SelectedPaletteId` int(11) DEFAULT NULL AFTER `SecondaryColor`;

CREATE INDEX `IX_SystemSettings_SelectedPaletteId` ON `SystemSettings` (`SelectedPaletteId`);

ALTER TABLE `SystemSettings`
  ADD CONSTRAINT `FK_SystemSettings_SelectedPaletteId`
  FOREIGN KEY (`SelectedPaletteId`) REFERENCES `ColorPalettes` (`Id`) ON DELETE SET NULL;