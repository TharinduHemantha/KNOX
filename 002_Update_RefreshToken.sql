
-- Check which columns exist
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'RefreshTokens';

-- Add missing columns (run only for columns that don't exist)
ALTER TABLE [dbo].[RefreshTokens] ADD [RevokedBy] NVARCHAR(450) NULL;
ALTER TABLE [dbo].[RefreshTokens] ADD [ReplacedByTokenHash] NVARCHAR(450) NULL;
ALTER TABLE [dbo].[RefreshTokens] ADD [CreatedByIp] NVARCHAR(45) NULL;