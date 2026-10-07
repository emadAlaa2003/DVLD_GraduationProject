IF OBJECT_ID(N'dbo.MobileUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MobileUsers
    (
        MobileUserID INT IDENTITY(1,1) NOT NULL,
        PersonID INT NOT NULL,

        Username NVARCHAR(50)
            COLLATE Latin1_General_100_CI_AS
            NOT NULL,

        PasswordHash NVARCHAR(512) NOT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_MobileUsers_IsActive
            DEFAULT (1),

        CONSTRAINT PK_MobileUsers
            PRIMARY KEY (MobileUserID),

        CONSTRAINT FK_MobileUsers_People
            FOREIGN KEY (PersonID)
            REFERENCES dbo.People(PersonID),

        CONSTRAINT UQ_MobileUsers_PersonID
            UNIQUE (PersonID),

        CONSTRAINT UQ_MobileUsers_Username
            UNIQUE (Username),

        CONSTRAINT CK_MobileUsers_Username_NotEmpty
            CHECK (LEN(LTRIM(RTRIM(Username))) > 0),

        CONSTRAINT CK_MobileUsers_Username_Trimmed
            CHECK (Username = LTRIM(RTRIM(Username))),

        CONSTRAINT CK_MobileUsers_Username_NoSpaces
            CHECK (Username NOT LIKE N'% %'),

        CONSTRAINT CK_MobileUsers_PasswordHash_NotEmpty
            CHECK (LEN(PasswordHash) > 0)
    );
END;