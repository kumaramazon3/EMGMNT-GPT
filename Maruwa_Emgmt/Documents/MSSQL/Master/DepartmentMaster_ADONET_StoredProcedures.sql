/*
    Department Master scripts for Maruwa E-Management .NET Core MVC module.
    Execute in the EHRM SQL Server database before using Department Master.
*/

IF OBJECT_ID('dbo.DepartmentMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DepartmentMaster
    (
        RecordNo INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DepartmentMaster PRIMARY KEY,
        DepartmentCode NVARCHAR(20) NOT NULL,
        DepartmentName NVARCHAR(150) NOT NULL,
        JapanHead NVARCHAR(10) NOT NULL,
        Office NVARCHAR(20) NOT NULL,
        GotSection NVARCHAR(10) NOT NULL,
        Prefix NVARCHAR(20) NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_DepartmentMaster_CreatedOn DEFAULT SYSDATETIME(),
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL,
        ActiveStatus BIT NOT NULL CONSTRAINT DF_DepartmentMaster_ActiveStatus DEFAULT 1
    );
END
GO

/*Start-modified on 17/06/2026 by kranthi*/
CREATE TABLE [dbo].[DepartmentMaster](
	[RecordNo] [int] IDENTITY(1,1) NOT NULL,
	[DepartmentCode] [nvarchar](20) NOT NULL,
	[DepartmentName] [nvarchar](150) NOT NULL,
	[JapanHead] [nvarchar](10) NOT NULL,
	[Office] [nvarchar](20) NOT NULL,
	[GotSection] [nvarchar](10) NOT NULL,
	[Prefix] [nvarchar](20) NOT NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[CreatedOn] [datetime2](0) NOT NULL,
	[EditedBy] [nvarchar](50) NULL,
	[EditedOn] [datetime2](0) NULL,
	[ActiveStatus] [bit] NOT NULL,
	[departmentID] [nvarchar](10) NULL,
	[idDeptActive] [varchar](2) NULL,
	[subDepartment] [varchar](2) NULL,
	[productionCode] [nvarchar](10) NULL,
	[deptID] [nvarchar](10) NULL,
 CONSTRAINT [PK_DepartmentMaster] PRIMARY KEY CLUSTERED 
(
	[RecordNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DepartmentMaster] ADD  CONSTRAINT [DF_DepartmentMaster_CreatedOn]  DEFAULT (sysdatetime()) FOR [CreatedOn]
GO

ALTER TABLE [dbo].[DepartmentMaster] ADD  CONSTRAINT [DF_DepartmentMaster_ActiveStatus]  DEFAULT ((1)) FOR [ActiveStatus]
GO

/*End -modified on 17/06/2026 by kranthi*/


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DepartmentMaster_DepartmentCode_Active' AND object_id = OBJECT_ID('dbo.DepartmentMaster'))
BEGIN
    CREATE UNIQUE INDEX UX_DepartmentMaster_DepartmentCode_Active
    ON dbo.DepartmentMaster(DepartmentCode)
    WHERE ActiveStatus = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DepartmentMaster_GetPaged
    @GlobalSearch NVARCHAR(150) = NULL,
    @DepartmentCode NVARCHAR(50) = NULL,
    @DepartmentName NVARCHAR(150) = NULL,
    @JapanHead NVARCHAR(10) = NULL,
    @Office NVARCHAR(20) = NULL,
    @GotSection NVARCHAR(10) = NULL,
    @Prefix NVARCHAR(20) = NULL,
    @SortColumn NVARCHAR(50) = 'RecordNo',
    @SortDirection NVARCHAR(4) = 'DESC',
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN ISNULL(@PageNumber, 0) <= 0 THEN 1 ELSE @PageNumber END;
    SET @PageSize = ISNULL(@PageSize, 10);
    SET @SortDirection = CASE WHEN UPPER(@SortDirection) = 'ASC' THEN 'ASC' ELSE 'DESC' END;

    CREATE TABLE #Filtered
    (
        RecordNo INT NOT NULL,
        DepartmentCode NVARCHAR(20) NOT NULL,
        DepartmentName NVARCHAR(150) NOT NULL,
        JapanHead NVARCHAR(10) NOT NULL,
        Office NVARCHAR(20) NOT NULL,
        GotSection NVARCHAR(10) NOT NULL,
        Prefix NVARCHAR(20) NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NULL,
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL,
        ActiveStatus BIT NOT NULL
    );

    INSERT INTO #Filtered
    SELECT RecordNo, DepartmentCode, DepartmentName, JapanHead, Office, GotSection, Prefix,
           CreatedBy, CreatedOn, EditedBy, EditedOn, ActiveStatus
    FROM dbo.DepartmentMaster
    WHERE ActiveStatus = 1
      AND (NULLIF(@GlobalSearch, '') IS NULL OR
           DepartmentCode LIKE '%' + @GlobalSearch + '%' OR DepartmentName LIKE '%' + @GlobalSearch + '%' OR
           JapanHead LIKE '%' + @GlobalSearch + '%' OR Office LIKE '%' + @GlobalSearch + '%' OR
           GotSection LIKE '%' + @GlobalSearch + '%' OR Prefix LIKE '%' + @GlobalSearch + '%' OR
           ISNULL(CreatedBy,'') LIKE '%' + @GlobalSearch + '%' OR ISNULL(EditedBy,'') LIKE '%' + @GlobalSearch + '%')
      AND (NULLIF(@DepartmentCode, '') IS NULL OR DepartmentCode LIKE '%' + @DepartmentCode + '%')
      AND (NULLIF(@DepartmentName, '') IS NULL OR DepartmentName LIKE '%' + @DepartmentName + '%')
      AND (NULLIF(@JapanHead, '') IS NULL OR JapanHead LIKE '%' + @JapanHead + '%')
      AND (NULLIF(@Office, '') IS NULL OR Office LIKE '%' + @Office + '%')
      AND (NULLIF(@GotSection, '') IS NULL OR GotSection LIKE '%' + @GotSection + '%')
      AND (NULLIF(@Prefix, '') IS NULL OR Prefix LIKE '%' + @Prefix + '%');

    ;WITH Numbered AS
    (
        SELECT *, ROW_NUMBER() OVER
        (
            ORDER BY
            CASE WHEN @SortColumn = 'DepartmentCode' AND @SortDirection = 'ASC' THEN DepartmentCode END ASC,
            CASE WHEN @SortColumn = 'DepartmentCode' AND @SortDirection = 'DESC' THEN DepartmentCode END DESC,
            CASE WHEN @SortColumn = 'DepartmentName' AND @SortDirection = 'ASC' THEN DepartmentName END ASC,
            CASE WHEN @SortColumn = 'DepartmentName' AND @SortDirection = 'DESC' THEN DepartmentName END DESC,
            CASE WHEN @SortColumn = 'JapanHead' AND @SortDirection = 'ASC' THEN JapanHead END ASC,
            CASE WHEN @SortColumn = 'JapanHead' AND @SortDirection = 'DESC' THEN JapanHead END DESC,
            CASE WHEN @SortColumn = 'Office' AND @SortDirection = 'ASC' THEN Office END ASC,
            CASE WHEN @SortColumn = 'Office' AND @SortDirection = 'DESC' THEN Office END DESC,
            CASE WHEN @SortColumn = 'GotSection' AND @SortDirection = 'ASC' THEN GotSection END ASC,
            CASE WHEN @SortColumn = 'GotSection' AND @SortDirection = 'DESC' THEN GotSection END DESC,
            CASE WHEN @SortColumn = 'Prefix' AND @SortDirection = 'ASC' THEN Prefix END ASC,
            CASE WHEN @SortColumn = 'Prefix' AND @SortDirection = 'DESC' THEN Prefix END DESC,
            CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'ASC' THEN CreatedOn END ASC,
            CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'DESC' THEN CreatedOn END DESC,
            CASE WHEN @SortColumn = 'EditedOn' AND @SortDirection = 'ASC' THEN EditedOn END ASC,
            CASE WHEN @SortColumn = 'EditedOn' AND @SortDirection = 'DESC' THEN EditedOn END DESC,
            RecordNo DESC
        ) AS RowNum
        FROM #Filtered
    )
    SELECT RecordNo, DepartmentCode, DepartmentName, JapanHead, Office, GotSection, Prefix,
           CreatedBy, CreatedOn, EditedBy, EditedOn, ActiveStatus
    FROM Numbered
    WHERE @PageSize = 0 OR RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize)
    ORDER BY RowNum;

    SELECT COUNT(1) AS TotalCount FROM #Filtered;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DepartmentMaster_GetById
    @RecordNo INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RecordNo, DepartmentCode, DepartmentName, JapanHead, Office, GotSection, Prefix,
           CreatedBy, CreatedOn, EditedBy, EditedOn, ActiveStatus
    FROM dbo.DepartmentMaster
    WHERE RecordNo = @RecordNo;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DepartmentMaster_Save
    @RecordNo INT,
    @DepartmentCode NVARCHAR(20),
    @DepartmentName NVARCHAR(150),
    @JapanHead NVARCHAR(10),
    @Office NVARCHAR(20),
    @GotSection NVARCHAR(10),
    @Prefix NVARCHAR(20),
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @DepartmentCode = LTRIM(RTRIM(@DepartmentCode));

    IF EXISTS (SELECT 1 FROM dbo.DepartmentMaster WHERE DepartmentCode = @DepartmentCode AND ActiveStatus = 1 AND RecordNo <> ISNULL(@RecordNo, 0))
    BEGIN
        SET @Status = 0;
        SET @Message = 'Department Already Exists';
        RETURN;
    END

    IF ISNULL(@RecordNo, 0) = 0
    BEGIN
        INSERT INTO dbo.DepartmentMaster(DepartmentCode, DepartmentName, JapanHead, Office, GotSection, Prefix, CreatedBy, CreatedOn, ActiveStatus)
        VALUES(@DepartmentCode, @DepartmentName, @JapanHead, @Office, @GotSection, @Prefix, @EmployeeCode, SYSDATETIME(), 1);
        SET @Message = 'Department saved successfully';
    END
    ELSE
    BEGIN
        UPDATE dbo.DepartmentMaster
        SET DepartmentName = @DepartmentName,
            JapanHead = @JapanHead,
            Office = @Office,
            GotSection = @GotSection,
            Prefix = @Prefix,
            EditedBy = @EmployeeCode,
            EditedOn = SYSDATETIME()
        WHERE RecordNo = @RecordNo AND ActiveStatus = 1;
        SET @Message = 'Department updated successfully';
    END
    SET @Status = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DepartmentMaster_Delete
    @RecordNo INT,
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.DepartmentMaster WHERE RecordNo = @RecordNo AND ActiveStatus = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'Department not found';
        RETURN;
    END

    UPDATE dbo.DepartmentMaster
    SET ActiveStatus = 0,
        EditedBy = @EmployeeCode,
        EditedOn = SYSDATETIME()
    WHERE RecordNo = @RecordNo;

    SET @Status = 1;
    SET @Message = 'Department deleted successfully';
END
GO
