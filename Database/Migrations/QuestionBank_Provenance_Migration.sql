/*
DVLD - Question Bank provenance schema migration (development database).
Run after the base QuestionBank table has been created, before running
clsQuestionBankData code that writes SourceEvidence and SourceChunkIndex.
Safe to run again: each column is added only when it is missing.
Take a database backup first. This does not approve or create questions.
*/
USE [DVLD_Grad]; -- Change only if your development database has another name.
GO
SET XACT_ABORT ON;
GO
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.QuestionBank', N'U') IS NULL
    BEGIN
        THROW 51010, 'QuestionBank table does not exist. Apply the base database schema first.', 1;
    END;

    IF COL_LENGTH(N'dbo.QuestionBank', N'SourceEvidence') IS NULL
    BEGIN
        ALTER TABLE dbo.QuestionBank
        ADD SourceEvidence NVARCHAR(4000) NULL;
    END;

    IF COL_LENGTH(N'dbo.QuestionBank', N'SourceChunkIndex') IS NULL
    BEGIN
        ALTER TABLE dbo.QuestionBank
        ADD SourceChunkIndex INT NULL;
    END;

    COMMIT TRANSACTION;
    PRINT 'QuestionBank source-evidence columns are ready.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SELECT
    COL_LENGTH(N'dbo.QuestionBank', N'SourceEvidence') AS SourceEvidenceColumnBytes,
    COL_LENGTH(N'dbo.QuestionBank', N'SourceChunkIndex') AS SourceChunkIndexColumnBytes;
GO
