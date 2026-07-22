/*
    LeaveType Master table and stored procedures.
    Change implemented:
      - LeaveID is no longer the primary key.
      - New identity column SeqLeaveID is the primary key.
      - SeqLeaveID is hidden in UI and used internally for edit/delete/update.
      - LeaveID and LeaveType remain visible and searchable.
      - LeaveDescription column remains removed.

    Execute this script manually in the SQL Server database configured by EHRMConnection.
*/

IF OBJECT_ID('dbo.LeaveTypeMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveTypeMaster
    (
        SeqLeaveID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveTypeMaster PRIMARY KEY,
        LeaveID NVARCHAR(50) NOT NULL,
        LeaveType NVARCHAR(100) NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_LeaveTypeMaster_CreatedOn DEFAULT SYSUTCDATETIME(),
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL,
        isActive BIT NOT NULL CONSTRAINT DF_LeaveTypeMaster_isActive DEFAULT 1
    );
END
GO

IF COL_LENGTH('dbo.LeaveTypeMaster', 'LeaveDescription') IS NOT NULL
BEGIN
    ALTER TABLE dbo.LeaveTypeMaster DROP COLUMN LeaveDescription;
END
GO

IF COL_LENGTH('dbo.LeaveTypeMaster', 'SeqLeaveID') IS NULL
BEGIN
    ALTER TABLE dbo.LeaveTypeMaster ADD SeqLeaveID INT IDENTITY(1,1) NOT NULL;
END
GO

DECLARE @PkName NVARCHAR(128);
DECLARE @PkColumn NVARCHAR(128);

SELECT TOP 1
    @PkName = kc.name,
    @PkColumn = c.name
FROM sys.key_constraints kc
INNER JOIN sys.index_columns ic
    ON kc.parent_object_id = ic.object_id
   AND kc.unique_index_id = ic.index_id
INNER JOIN sys.columns c
    ON ic.object_id = c.object_id
   AND ic.column_id = c.column_id
WHERE kc.parent_object_id = OBJECT_ID('dbo.LeaveTypeMaster')
  AND kc.type = 'PK';

IF @PkName IS NOT NULL AND @PkColumn <> 'SeqLeaveID'
BEGIN
    EXEC('ALTER TABLE dbo.LeaveTypeMaster DROP CONSTRAINT ' + QUOTENAME(@PkName));
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.key_constraints kc
    INNER JOIN sys.index_columns ic
        ON kc.parent_object_id = ic.object_id
       AND kc.unique_index_id = ic.index_id
    INNER JOIN sys.columns c
        ON ic.object_id = c.object_id
       AND ic.column_id = c.column_id
    WHERE kc.parent_object_id = OBJECT_ID('dbo.LeaveTypeMaster')
      AND kc.type = 'PK'
      AND c.name = 'SeqLeaveID'
)
BEGIN
    ALTER TABLE dbo.LeaveTypeMaster
    ADD CONSTRAINT PK_LeaveTypeMaster PRIMARY KEY CLUSTERED (SeqLeaveID);
END
GO

IF COL_LENGTH('dbo.LeaveTypeMaster', 'LeaveID') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveID IS NULL OR LTRIM(RTRIM(LeaveID)) = '')
    BEGIN
        ;WITH EmptyLeaveIds AS
        (
            SELECT SeqLeaveID, ROW_NUMBER() OVER (ORDER BY SeqLeaveID) AS rn
            FROM dbo.LeaveTypeMaster
            WHERE LeaveID IS NULL OR LTRIM(RTRIM(LeaveID)) = ''
        )
        UPDATE l
           SET LeaveID = CONCAT('LT', RIGHT('0000' + CAST(e.rn AS VARCHAR(10)), 4))
        FROM dbo.LeaveTypeMaster l
        INNER JOIN EmptyLeaveIds e ON l.SeqLeaveID = e.SeqLeaveID;
    END

    ALTER TABLE dbo.LeaveTypeMaster ALTER COLUMN LeaveID NVARCHAR(50) NOT NULL;
END
GO

IF COL_LENGTH('dbo.LeaveTypeMaster', 'LeaveType') IS NOT NULL
BEGIN
    ALTER TABLE dbo.LeaveTypeMaster ALTER COLUMN LeaveType NVARCHAR(100) NOT NULL;
END
GO

IF OBJECT_ID('dbo.UX_LeaveTypeMaster_LeaveID_Active', 'UQ') IS NOT NULL
BEGIN
    ALTER TABLE dbo.LeaveTypeMaster DROP CONSTRAINT UX_LeaveTypeMaster_LeaveID_Active;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LeaveTypeMaster_LeaveID' AND object_id = OBJECT_ID('dbo.LeaveTypeMaster'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_LeaveTypeMaster_LeaveID ON dbo.LeaveTypeMaster (LeaveID);
END
GO

MERGE dbo.LeaveTypeMaster AS target
USING (VALUES
    ('AL','Annual'),
    ('CAL','Calamity'),
    ('CH','Company Holiday'),
    ('CL','Compassionate'),
    ('EA','Emergency- Annual'),
    ('EU','Emergency- Unpaid'),
    ('HL','Hospitalization'),
    ('MC','Medical'),
    ('ML','Marriage - Children'),
    ('MS','Marriage -Self'),
    ('PL','Paternity'),
    ('RL','Replacement'),
    ('UPL','Unpaid'),
    ('MAT','Maternity')
) AS source (LeaveID, LeaveType)
ON target.LeaveID = source.LeaveID
WHEN MATCHED THEN
    UPDATE SET LeaveType = source.LeaveType
WHEN NOT MATCHED BY TARGET THEN
    INSERT (LeaveID, LeaveType, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive)
    VALUES (source.LeaveID, source.LeaveType, 'SYSTEM', SYSUTCDATETIME(), NULL, NULL, 1);
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_GetPaged
    @GlobalSearch NVARCHAR(200) = NULL,
    @LeaveID NVARCHAR(50) = NULL,
    @LeaveType NVARCHAR(100) = NULL,
    @CreatedBy NVARCHAR(50) = NULL,
    @EditedBy NVARCHAR(50) = NULL,
    @isActive NVARCHAR(20) = NULL,
    @SortColumn NVARCHAR(50) = 'LeaveID',
    @SortDirection NVARCHAR(4) = 'ASC',
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SET @SortColumn = CASE WHEN @SortColumn IN ('SeqLeaveID','LeaveID','LeaveType','CreatedBy','CreatedOn','EditedBy','EditedOn','isActive') THEN @SortColumn ELSE 'LeaveID' END;
    SET @SortDirection = CASE WHEN UPPER(@SortDirection) = 'DESC' THEN 'DESC' ELSE 'ASC' END;
    SET @PageNumber = CASE WHEN ISNULL(@PageNumber, 0) <= 0 THEN 1 ELSE @PageNumber END;

    SELECT SeqLeaveID, LeaveID, LeaveType, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    INTO #FilteredLeaveType
    FROM dbo.LeaveTypeMaster
    WHERE
        (@GlobalSearch IS NULL OR @GlobalSearch = '' OR
            LeaveID LIKE '%' + @GlobalSearch + '%' OR
            LeaveType LIKE '%' + @GlobalSearch + '%' OR
            CreatedBy LIKE '%' + @GlobalSearch + '%' OR
            EditedBy LIKE '%' + @GlobalSearch + '%' OR
            CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @GlobalSearch + '%')
        AND (@LeaveID IS NULL OR @LeaveID = '' OR LeaveID LIKE '%' + @LeaveID + '%')
        AND (@LeaveType IS NULL OR @LeaveType = '' OR LeaveType LIKE '%' + @LeaveType + '%')
        AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CreatedBy LIKE '%' + @CreatedBy + '%')
        AND (@EditedBy IS NULL OR @EditedBy = '' OR EditedBy LIKE '%' + @EditedBy + '%')
        AND (@isActive IS NULL OR @isActive = '' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @isActive + '%');

    ;WITH Ordered AS
    (
        SELECT *, ROW_NUMBER() OVER
        (
            ORDER BY
                CASE WHEN @SortColumn = 'SeqLeaveID' AND @SortDirection = 'ASC' THEN SeqLeaveID END ASC,
                CASE WHEN @SortColumn = 'SeqLeaveID' AND @SortDirection = 'DESC' THEN SeqLeaveID END DESC,
                CASE WHEN @SortColumn = 'LeaveID' AND @SortDirection = 'ASC' THEN LeaveID END ASC,
                CASE WHEN @SortColumn = 'LeaveID' AND @SortDirection = 'DESC' THEN LeaveID END DESC,
                CASE WHEN @SortColumn = 'LeaveType' AND @SortDirection = 'ASC' THEN LeaveType END ASC,
                CASE WHEN @SortColumn = 'LeaveType' AND @SortDirection = 'DESC' THEN LeaveType END DESC,
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
                LeaveID ASC
        ) AS RowNum
        FROM #FilteredLeaveType
    )
    SELECT SeqLeaveID, LeaveID, LeaveType, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM Ordered
    WHERE @PageSize = 0 OR RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);

    SELECT COUNT(1) AS TotalCount FROM #FilteredLeaveType;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_GetById
    @SeqLeaveID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT SeqLeaveID, LeaveID, LeaveType, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM dbo.LeaveTypeMaster
    WHERE SeqLeaveID = @SeqLeaveID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_Save
    @SeqLeaveID INT = 0,
    @LeaveID NVARCHAR(50),
    @LeaveType NVARCHAR(100),
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @LeaveID = LTRIM(RTRIM(ISNULL(@LeaveID, '')));
    SET @LeaveType = LTRIM(RTRIM(ISNULL(@LeaveType, '')));

    IF @LeaveID = ''
    BEGIN
        SET @Status = 0;
        SET @Message = 'LeaveID is required';
        RETURN;
    END

    IF @LeaveType = ''
    BEGIN
        SET @Status = 0;
        SET @Message = 'LeaveType is required';
        RETURN;
    END

    IF ISNULL(@SeqLeaveID, 0) > 0 AND EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE SeqLeaveID = @SeqLeaveID)
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveID = @LeaveID AND SeqLeaveID <> @SeqLeaveID AND isActive = 1)
        BEGIN
            SET @Status = 0;
            SET @Message = 'LeaveID Already Exists';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveType = @LeaveType AND SeqLeaveID <> @SeqLeaveID AND isActive = 1)
        BEGIN
            SET @Status = 0;
            SET @Message = 'LeaveType Already Exists';
            RETURN;
        END

        UPDATE dbo.LeaveTypeMaster
           SET LeaveID = @LeaveID,
               LeaveType = @LeaveType,
               isActive = 1,
               EditedBy = @EmployeeCode,
               EditedOn = SYSUTCDATETIME()
         WHERE SeqLeaveID = @SeqLeaveID;

        SET @Status = 1;
        SET @Message = 'LeaveType updated successfully';
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveID = @LeaveID AND isActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'LeaveID Already Exists';
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveType = @LeaveType AND isActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'LeaveType Already Exists';
        RETURN;
    END

    INSERT INTO dbo.LeaveTypeMaster (LeaveID, LeaveType, CreatedBy, CreatedOn, isActive)
    VALUES (@LeaveID, @LeaveType, @EmployeeCode, SYSUTCDATETIME(), 1);

    SET @Status = 1;
    SET @Message = 'LeaveType saved successfully';
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_Delete
    @SeqLeaveID INT,
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE SeqLeaveID = @SeqLeaveID AND isActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'LeaveType not found or already inactive';
        RETURN;
    END

    UPDATE dbo.LeaveTypeMaster
       SET isActive = 0,
           EditedBy = @EmployeeCode,
           EditedOn = SYSUTCDATETIME()
     WHERE SeqLeaveID = @SeqLeaveID;

    SET @Status = 1;
    SET @Message = 'LeaveType deleted successfully';
END
GO
