/*
    Nature of Grievance / Complaint Master - DB Script
    Execute this script manually in the E-Management SQL Server database.
*/

IF OBJECT_ID('dbo.NatureOfGrievanceMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.NatureOfGrievanceMaster
    (
        NatureID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NatureOfGrievanceMaster PRIMARY KEY,
        NatureName NVARCHAR(300) NOT NULL,
        IsOther BIT NOT NULL CONSTRAINT DF_NatureOfGrievanceMaster_IsOther DEFAULT 0,
        CreatedBy NVARCHAR(100) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_NatureOfGrievanceMaster_CreatedOn DEFAULT SYSUTCDATETIME(),
        EditedBy NVARCHAR(100) NULL,
        EditedOn DATETIME2(0) NULL,
        isActive BIT NOT NULL CONSTRAINT DF_NatureOfGrievanceMaster_isActive DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_NatureOfGrievanceMaster_NatureName' AND object_id = OBJECT_ID('dbo.NatureOfGrievanceMaster'))
BEGIN
    CREATE UNIQUE INDEX UX_NatureOfGrievanceMaster_NatureName ON dbo.NatureOfGrievanceMaster(NatureName);
END
GO

DECLARE @Seed TABLE (NatureName NVARCHAR(300), IsOther BIT);
INSERT INTO @Seed (NatureName, IsOther) VALUES
(N'Unfair treatment / discrimination', 0),
(N'Harassment / bullying', 0),
(N'Work lapses', 0),
(N'Breach of company policy / SOP', 0),
(N'Safety, health & environment (OSH) concern', 0),
(N'Misconduct by supervisor / colleague', 0),
(N'Abuse of authority / power', 0),
(N'Working hours / shift scheduling issue', 0),
(N'Other (please specify in the below text box)', 1);

MERGE dbo.NatureOfGrievanceMaster AS T
USING @Seed AS S
ON LTRIM(RTRIM(T.NatureName)) = LTRIM(RTRIM(S.NatureName))
WHEN MATCHED THEN
    UPDATE SET IsOther = S.IsOther, isActive = 1
WHEN NOT MATCHED THEN
    INSERT (NatureName, IsOther, CreatedBy, CreatedOn, isActive)
    VALUES (S.NatureName, S.IsOther, 'SYSTEM', SYSUTCDATETIME(), 1);
GO

CREATE OR ALTER PROCEDURE dbo.usp_NatureOfGrievanceMaster_GetPaged
    @GlobalSearch NVARCHAR(200) = NULL,
    @NatureName NVARCHAR(300) = NULL,
    @IsOther NVARCHAR(20) = NULL,
    @CreatedBy NVARCHAR(100) = NULL,
    @EditedBy NVARCHAR(100) = NULL,
    @isActive NVARCHAR(20) = NULL,
    @SortColumn NVARCHAR(50) = 'NatureName',
    @SortDirection NVARCHAR(4) = 'ASC',
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 0 SET @PageSize = 10;

    SELECT
        NatureID, NatureName, IsOther, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    INTO #Filtered
    FROM dbo.NatureOfGrievanceMaster
    WHERE (@GlobalSearch IS NULL OR @GlobalSearch = '' OR NatureName LIKE '%' + @GlobalSearch + '%' OR CreatedBy LIKE '%' + @GlobalSearch + '%' OR EditedBy LIKE '%' + @GlobalSearch + '%' OR CASE WHEN IsOther = 1 THEN 'Yes' ELSE 'No' END LIKE '%' + @GlobalSearch + '%' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @GlobalSearch + '%')
      AND (@NatureName IS NULL OR @NatureName = '' OR NatureName LIKE '%' + @NatureName + '%')
      AND (@IsOther IS NULL OR @IsOther = '' OR CASE WHEN IsOther = 1 THEN 'Yes' ELSE 'No' END LIKE '%' + @IsOther + '%' OR CONVERT(NVARCHAR(5), IsOther) = @IsOther)
      AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CreatedBy LIKE '%' + @CreatedBy + '%')
      AND (@EditedBy IS NULL OR @EditedBy = '' OR EditedBy LIKE '%' + @EditedBy + '%')
      AND (@isActive IS NULL OR @isActive = '' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @isActive + '%' OR CONVERT(NVARCHAR(5), isActive) = @isActive);

    IF @PageSize = 0
    BEGIN
        SELECT * FROM #Filtered
        ORDER BY NatureName ASC;
    END
    ELSE
    BEGIN
        SELECT * FROM #Filtered
        ORDER BY
            CASE WHEN @SortColumn = 'NatureName' AND @SortDirection = 'ASC' THEN NatureName END ASC,
            CASE WHEN @SortColumn = 'NatureName' AND @SortDirection = 'DESC' THEN NatureName END DESC,
            CASE WHEN @SortColumn = 'IsOther' AND @SortDirection = 'ASC' THEN IsOther END ASC,
            CASE WHEN @SortColumn = 'IsOther' AND @SortDirection = 'DESC' THEN IsOther END DESC,
            CASE WHEN @SortColumn = 'CreatedBy' AND @SortDirection = 'ASC' THEN CreatedBy END ASC,
            CASE WHEN @SortColumn = 'CreatedBy' AND @SortDirection = 'DESC' THEN CreatedBy END DESC,
            CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'ASC' THEN CreatedOn END ASC,
            CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'DESC' THEN CreatedOn END DESC,
            CASE WHEN @SortColumn = 'EditedBy' AND @SortDirection = 'ASC' THEN EditedBy END ASC,
            CASE WHEN @SortColumn = 'EditedBy' AND @SortDirection = 'DESC' THEN EditedBy END DESC,
            CASE WHEN @SortColumn = 'EditedOn' AND @SortDirection = 'ASC' THEN EditedOn END ASC,
            CASE WHEN @SortColumn = 'EditedOn' AND @SortDirection = 'DESC' THEN EditedOn END DESC,
            CASE WHEN @SortColumn = 'isActive' AND @SortDirection = 'ASC' THEN isActive END ASC,
            CASE WHEN @SortColumn = 'isActive' AND @SortDirection = 'DESC' THEN isActive END DESC,
            NatureID ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
    END

    SELECT COUNT(1) AS TotalCount FROM #Filtered;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_NatureOfGrievanceMaster_GetById
    @NatureID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1) NatureID, NatureName, IsOther, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM dbo.NatureOfGrievanceMaster
    WHERE NatureID = @NatureID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_NatureOfGrievanceMaster_SearchActive
    @SearchText NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (50) NatureID, NatureName, IsOther, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM dbo.NatureOfGrievanceMaster
    WHERE isActive = 1
      AND (@SearchText IS NULL OR @SearchText = '' OR NatureName LIKE '%' + @SearchText + '%')
    ORDER BY NatureID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_NatureOfGrievanceMaster_Save
    @NatureID INT = 0,
    @NatureName NVARCHAR(300),
    @IsOther BIT = 0,
    @EmployeeCode NVARCHAR(100) = NULL,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SET @NatureName = LTRIM(RTRIM(ISNULL(@NatureName, '')));
        IF @NatureName = ''
        BEGIN
            SET @Status = 0;
            SET @Message = 'Nature of Grievance is required.';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM dbo.NatureOfGrievanceMaster WHERE NatureName = @NatureName AND NatureID <> ISNULL(@NatureID, 0))
        BEGIN
            SET @Status = 0;
            SET @Message = 'This Nature of Grievance already exists.';
            RETURN;
        END

        IF ISNULL(@NatureID, 0) = 0
        BEGIN
            INSERT INTO dbo.NatureOfGrievanceMaster (NatureName, IsOther, CreatedBy, CreatedOn, isActive)
            VALUES (@NatureName, @IsOther, @EmployeeCode, SYSUTCDATETIME(), 1);
            SET @Message = 'Nature of Grievance saved successfully.';
        END
        ELSE
        BEGIN
            UPDATE dbo.NatureOfGrievanceMaster
            SET NatureName = @NatureName,
                IsOther = @IsOther,
                EditedBy = @EmployeeCode,
                EditedOn = SYSUTCDATETIME()
            WHERE NatureID = @NatureID;
            SET @Message = 'Nature of Grievance updated successfully.';
        END

        SET @Status = 1;
    END TRY
    BEGIN CATCH
        SET @Status = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_NatureOfGrievanceMaster_Delete
    @NatureID INT,
    @EmployeeCode NVARCHAR(100) = NULL,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.NatureOfGrievanceMaster
        SET isActive = 0,
            EditedBy = @EmployeeCode,
            EditedOn = SYSUTCDATETIME()
        WHERE NatureID = @NatureID;

        SET @Status = 1;
        SET @Message = 'Nature of Grievance deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @Status = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

IF OBJECT_ID('dbo.EmployeeGrievanceComplaintNature', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeGrievanceComplaintNature
    (
        GrievanceNatureID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeGrievanceComplaintNature PRIMARY KEY,
        GrievanceID INT NOT NULL,
        NatureID INT NULL,
        NatureName NVARCHAR(300) NOT NULL,
        IsOther BIT NOT NULL CONSTRAINT DF_EGFComplaintNature_IsOther DEFAULT 0,
        OtherComplaintText NVARCHAR(500) NULL,
        CreatedBy NVARCHAR(100) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EGFComplaintNature_CreatedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EGFComplaintNature_EGFComplaint')
BEGIN
    ALTER TABLE dbo.EmployeeGrievanceComplaintNature
    ADD CONSTRAINT FK_EGFComplaintNature_EGFComplaint FOREIGN KEY (GrievanceID) REFERENCES dbo.EmployeeGrievanceComplaint(GrievanceID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EGFComplaintNature_NatureMaster')
BEGIN
    ALTER TABLE dbo.EmployeeGrievanceComplaintNature
    ADD CONSTRAINT FK_EGFComplaintNature_NatureMaster FOREIGN KEY (NatureID) REFERENCES dbo.NatureOfGrievanceMaster(NatureID);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_DeleteGrievanceNatures
    @GrievanceID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.EmployeeGrievanceComplaintNature WHERE GrievanceID = @GrievanceID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_SaveGrievanceNature
    @GrievanceID INT,
    @NatureID INT = NULL,
    @NatureName NVARCHAR(300),
    @IsOther BIT = 0,
    @OtherComplaintText NVARCHAR(500) = NULL,
    @CreatedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.EmployeeGrievanceComplaintNature
    (GrievanceID, NatureID, NatureName, IsOther, OtherComplaintText, CreatedBy)
    VALUES
    (@GrievanceID, NULLIF(@NatureID, 0), @NatureName, @IsOther, @OtherComplaintText, @CreatedBy);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetGrievanceNatures
    @GrievanceID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        GrievanceNatureID,
        GrievanceID,
        NatureID,
        NatureName,
        IsOther,
        OtherComplaintText
    FROM dbo.EmployeeGrievanceComplaintNature
    WHERE GrievanceID = @GrievanceID
    ORDER BY GrievanceNatureID;
END
GO
