/*
DVLD - Official written exam database migration for the teammate's DEVELOPMENT database.
Prerequisite: the original DVLD tables and QuestionBank already exist.
Creates ONLY the two tables used by the national-number-based exam backend.
Adds QuestionBank.ReviewStatus only if missing; does not approve questions.
Run ONCE on a development database after taking a backup. Do not rerun
on a database where these exam tables already exist.
This is a schema-only script; approved questions and existing appointments
are NOT copied or generated.
*/
USE [DVLD_Grad]; -- Change only if teammate's actual development DB has another name.
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.QuestionBank', N'U') IS NULL
       OR OBJECT_ID(N'dbo.TestAppointments', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Tests', N'U') IS NULL
       OR OBJECT_ID(N'dbo.People', N'U') IS NULL
    BEGIN
        THROW 51000, 'Required base DVLD tables/QuestionBank are missing. Apply the base schema first.', 1;
    END;

    IF OBJECT_ID(N'dbo.OfficialExamAttempts', N'U') IS NOT NULL
       OR OBJECT_ID(N'dbo.OfficialExamQuestions', N'U') IS NOT NULL
    BEGIN
        THROW 51001, 'Official exam tables already exist; do not apply this migration twice.', 1;
    END;

    IF COL_LENGTH(N'dbo.QuestionBank', N'ReviewStatus') IS NULL
    BEGIN
        ALTER TABLE dbo.QuestionBank
        ADD ReviewStatus NVARCHAR(20) NOT NULL
            CONSTRAINT DF_QuestionBank_ReviewStatus DEFAULT (N'Draft')
            WITH VALUES;

        ALTER TABLE dbo.QuestionBank
        ADD CONSTRAINT CK_QuestionBank_ReviewStatus_OfficialExam
            CHECK (ReviewStatus IN (N'Draft', N'Approved', N'Rejected'));
    END;

    -- Exactly one official exam attempt per booked appointment.
    -- If a candidate needs a retake, the existing DVLD retake workflow
    -- must create a NEW TestAppointmentID.
    CREATE TABLE dbo.OfficialExamAttempts
    (
        ExamAttemptID          BIGINT IDENTITY(1,1) NOT NULL,
        TestAppointmentID      INT NOT NULL,
        PersonID               INT NOT NULL,
        Status                 NVARCHAR(20) NOT NULL
            CONSTRAINT DF_OfficialExamAttempts_Status DEFAULT (N'InProgress'),
        QuestionCount          SMALLINT NOT NULL,
        RequiredCorrectAnswers SMALLINT NOT NULL,
        DurationSeconds        INT NOT NULL,
        StartedAtUtc           DATETIME2(0) NOT NULL
            CONSTRAINT DF_OfficialExamAttempts_StartedAtUtc DEFAULT SYSUTCDATETIME(),
        DeadlineAtUtc          DATETIME2(0) NOT NULL,
        FinishedAtUtc          DATETIME2(0) NULL,
        CorrectAnswers         SMALLINT NULL,
        Passed                 BIT NULL,
        TestID                 INT NULL,
        RowVersion             ROWVERSION NOT NULL,
        CONSTRAINT PK_OfficialExamAttempts PRIMARY KEY CLUSTERED (ExamAttemptID),
        CONSTRAINT UQ_OfficialExamAttempts_TestAppointmentID
            UNIQUE (TestAppointmentID),
        CONSTRAINT FK_OfficialExamAttempts_TestAppointments
            FOREIGN KEY (TestAppointmentID)
            REFERENCES dbo.TestAppointments(TestAppointmentID),
        CONSTRAINT FK_OfficialExamAttempts_People
            FOREIGN KEY (PersonID)
            REFERENCES dbo.People(PersonID),
        CONSTRAINT FK_OfficialExamAttempts_Tests
            FOREIGN KEY (TestID)
            REFERENCES dbo.Tests(TestID),
        CONSTRAINT CK_OfficialExamAttempts_Status
            CHECK (Status IN (N'InProgress', N'Submitted', N'TimedOut', N'Cancelled')),
        CONSTRAINT CK_OfficialExamAttempts_QuestionCount
            CHECK (QuestionCount BETWEEN 1 AND 200),
        CONSTRAINT CK_OfficialExamAttempts_RequiredCorrectAnswers
            CHECK (RequiredCorrectAnswers BETWEEN 1 AND QuestionCount),
        CONSTRAINT CK_OfficialExamAttempts_DurationSeconds
            CHECK (DurationSeconds > 0),
        CONSTRAINT CK_OfficialExamAttempts_Deadline
            CHECK (DeadlineAtUtc > StartedAtUtc),
        CONSTRAINT CK_OfficialExamAttempts_CorrectAnswers
            CHECK (CorrectAnswers IS NULL OR CorrectAnswers BETWEEN 0 AND QuestionCount)
    );

    CREATE UNIQUE INDEX UX_OfficialExamAttempts_TestID
        ON dbo.OfficialExamAttempts(TestID)
        WHERE TestID IS NOT NULL;

    CREATE INDEX IX_OfficialExamAttempts_Person_Status
        ON dbo.OfficialExamAttempts(PersonID, Status);

    -- Frozen question snapshot: later edits/disablement in QuestionBank
    -- do not alter an already-started official exam or its audit trail.
    -- The API MUST NOT send CorrectOption/IsCorrect to the candidate.
    CREATE TABLE dbo.OfficialExamQuestions
    (
        ExamQuestionID          BIGINT IDENTITY(1,1) NOT NULL,
        ExamAttemptID           BIGINT NOT NULL,
        QuestionID              INT NOT NULL,
        QuestionNumber          SMALLINT NOT NULL,
        QuestionType            NVARCHAR(20) NOT NULL,
        QuestionText            NVARCHAR(1000) NOT NULL,
        OptionA                 NVARCHAR(500) NOT NULL,
        OptionB                 NVARCHAR(500) NOT NULL,
        OptionC                 NVARCHAR(500) NULL,
        OptionD                 NVARCHAR(500) NULL,
        CorrectOption           CHAR(1) NOT NULL,
        Explanation             NVARCHAR(2000) NULL,
        SourceDocumentID        INT NULL,
        SourcePageNumber        INT NULL,
        SelectedOption          CHAR(1) NULL,
        AnsweredAtUtc           DATETIME2(0) NULL,
        IsCorrect               BIT NULL,
        CONSTRAINT PK_OfficialExamQuestions PRIMARY KEY CLUSTERED (ExamQuestionID),
        CONSTRAINT FK_OfficialExamQuestions_Attempts
            FOREIGN KEY (ExamAttemptID)
            REFERENCES dbo.OfficialExamAttempts(ExamAttemptID),
        CONSTRAINT FK_OfficialExamQuestions_QuestionBank
            FOREIGN KEY (QuestionID)
            REFERENCES dbo.QuestionBank(QuestionID),
        CONSTRAINT UQ_OfficialExamQuestions_Attempt_QuestionNumber
            UNIQUE (ExamAttemptID, QuestionNumber),
        CONSTRAINT UQ_OfficialExamQuestions_Attempt_Question
            UNIQUE (ExamAttemptID, QuestionID),
        CONSTRAINT CK_OfficialExamQuestions_QuestionNumber
            CHECK (QuestionNumber > 0),
        CONSTRAINT CK_OfficialExamQuestions_QuestionType
            CHECK (QuestionType IN (N'MultipleChoice', N'TrueFalse')),
        CONSTRAINT CK_OfficialExamQuestions_CorrectOption
            CHECK (CorrectOption IN ('A','B','C','D')),
        CONSTRAINT CK_OfficialExamQuestions_SelectedOption
            CHECK (SelectedOption IS NULL OR SelectedOption IN ('A','B','C','D'))
    );

    COMMIT TRANSACTION;
    PRINT 'Official written exam schema created. Approved questions must be reviewed/populated separately.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- Read-only check: two new tables and the QuestionBank approval column.
SELECT t.name AS ExamTable
FROM sys.tables AS t
WHERE t.schema_id = SCHEMA_ID(N'dbo')
  AND t.name IN (N'OfficialExamAttempts', N'OfficialExamQuestions')
ORDER BY t.name;
SELECT COL_LENGTH(N'dbo.QuestionBank', N'ReviewStatus') AS ReviewStatusColumnBytes;
GO
