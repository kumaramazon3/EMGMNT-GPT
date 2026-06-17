/*
    LeaveType Master table and stored procedures.
    Execute this script manually in SQL Server database configured by EHRMConnection.
*/

IF OBJECT_ID('dbo.LeaveTypeMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveTypeMaster
    (
        LeaveID NVARCHAR(50) NOT NULL CONSTRAINT PK_LeaveTypeMaster PRIMARY KEY,
        LeaveType NVARCHAR(100) NOT NULL,
        LeaveDescription NVARCHAR(500) NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_LeaveTypeMaster_CreatedOn DEFAULT SYSUTCDATETIME(),
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL,
        isActive BIT NOT NULL CONSTRAINT DF_LeaveTypeMaster_isActive DEFAULT 1
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_GetPaged
    @GlobalSearch NVARCHAR(200) = NULL,
    @LeaveID NVARCHAR(50) = NULL,
    @LeaveType NVARCHAR(100) = NULL,
    @LeaveDescription NVARCHAR(500) = NULL,
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

    SET @SortColumn = CASE WHEN @SortColumn IN ('LeaveID','LeaveType','LeaveDescription','CreatedBy','CreatedOn','EditedBy','EditedOn','isActive') THEN @SortColumn ELSE 'LeaveID' END;
    SET @SortDirection = CASE WHEN UPPER(@SortDirection) = 'DESC' THEN 'DESC' ELSE 'ASC' END;
    SET @PageNumber = CASE WHEN ISNULL(@PageNumber, 0) <= 0 THEN 1 ELSE @PageNumber END;

    SELECT LeaveID, LeaveType, LeaveDescription, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    INTO #FilteredLeaveType
    FROM dbo.LeaveTypeMaster
    WHERE
        (@GlobalSearch IS NULL OR @GlobalSearch = '' OR
            LeaveID LIKE '%' + @GlobalSearch + '%' OR
            LeaveType LIKE '%' + @GlobalSearch + '%' OR
            LeaveDescription LIKE '%' + @GlobalSearch + '%' OR
            CreatedBy LIKE '%' + @GlobalSearch + '%' OR
            EditedBy LIKE '%' + @GlobalSearch + '%' OR
            CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @GlobalSearch + '%')
        AND (@LeaveID IS NULL OR @LeaveID = '' OR LeaveID LIKE '%' + @LeaveID + '%')
        AND (@LeaveType IS NULL OR @LeaveType = '' OR LeaveType LIKE '%' + @LeaveType + '%')
        AND (@LeaveDescription IS NULL OR @LeaveDescription = '' OR LeaveDescription LIKE '%' + @LeaveDescription + '%')
        AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CreatedBy LIKE '%' + @CreatedBy + '%')
        AND (@EditedBy IS NULL OR @EditedBy = '' OR EditedBy LIKE '%' + @EditedBy + '%')
        AND (@isActive IS NULL OR @isActive = '' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @isActive + '%');

    ;WITH Ordered AS
    (
        SELECT *, ROW_NUMBER() OVER
        (
            ORDER BY
                CASE WHEN @SortColumn = 'LeaveID' AND @SortDirection = 'ASC' THEN LeaveID END ASC,
                CASE WHEN @SortColumn = 'LeaveID' AND @SortDirection = 'DESC' THEN LeaveID END DESC,
                CASE WHEN @SortColumn = 'LeaveType' AND @SortDirection = 'ASC' THEN LeaveType END ASC,
                CASE WHEN @SortColumn = 'LeaveType' AND @SortDirection = 'DESC' THEN LeaveType END DESC,
                CASE WHEN @SortColumn = 'LeaveDescription' AND @SortDirection = 'ASC' THEN LeaveDescription END ASC,
                CASE WHEN @SortColumn = 'LeaveDescription' AND @SortDirection = 'DESC' THEN LeaveDescription END DESC,
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
    SELECT LeaveID, LeaveType, LeaveDescription, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM Ordered
    WHERE @PageSize = 0 OR RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);

    SELECT COUNT(1) AS TotalCount FROM #FilteredLeaveType;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_GetById
    @LeaveID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT LeaveID, LeaveType, LeaveDescription, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM dbo.LeaveTypeMaster
    WHERE LeaveID = @LeaveID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_Save
    @LeaveID NVARCHAR(50),
    @LeaveType NVARCHAR(100),
    @LeaveDescription NVARCHAR(500),
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveID = @LeaveID AND isActive = 1)
    BEGIN
        UPDATE dbo.LeaveTypeMaster
           SET LeaveType = @LeaveType,
               LeaveDescription = @LeaveDescription,
               EditedBy = @EmployeeCode,
               EditedOn = SYSUTCDATETIME()
         WHERE LeaveID = @LeaveID;
        SET @Status = 1;
        SET @Message = 'LeaveType updated successfully';
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveType = @LeaveType AND LeaveID <> @LeaveID AND isActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'LeaveType Already Exists';
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveID = @LeaveID AND isActive = 0)
    BEGIN
        UPDATE dbo.LeaveTypeMaster
           SET LeaveType = @LeaveType,
               LeaveDescription = @LeaveDescription,
               isActive = 1,
               EditedBy = @EmployeeCode,
               EditedOn = SYSUTCDATETIME()
         WHERE LeaveID = @LeaveID;
        SET @Status = 1;
        SET @Message = 'LeaveType restored and updated successfully';
        RETURN;
    END

    INSERT INTO dbo.LeaveTypeMaster (LeaveID, LeaveType, LeaveDescription, CreatedBy, CreatedOn, isActive)
    VALUES (@LeaveID, @LeaveType, @LeaveDescription, @EmployeeCode, SYSUTCDATETIME(), 1);

    SET @Status = 1;
    SET @Message = 'LeaveType saved successfully';
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_LeaveTypeMaster_Delete
    @LeaveID NVARCHAR(50),
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypeMaster WHERE LeaveID = @LeaveID AND isActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'LeaveType not found or already inactive';
        RETURN;
    END

    UPDATE dbo.LeaveTypeMaster
       SET isActive = 0,
           EditedBy = @EmployeeCode,
           EditedOn = SYSUTCDATETIME()
     WHERE LeaveID = @LeaveID;

    SET @Status = 1;
    SET @Message = 'LeaveType deleted successfully';
END
GO
