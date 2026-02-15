-- Add SelectedPaletteId FK constraint to SystemSettings
ALTER TABLE `SystemSettings`
  ADD COLUMN `SelectedPaletteId` int(11) DEFAULT NULL AFTER `SecondaryColor`;

CREATE INDEX `IX_SystemSettings_SelectedPaletteId` ON `SystemSettings` (`SelectedPaletteId`);

ALTER TABLE `SystemSettings`
  ADD CONSTRAINT `FK_SystemSettings_SelectedPaletteId`
  FOREIGN KEY (`SelectedPaletteId`) REFERENCES `ColorPalettes` (`Id`) ON DELETE SET NULL;
