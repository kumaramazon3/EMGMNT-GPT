/*
    Section Master scripts for Maruwa E-Management .NET Core MVC module.
    Execute in the EHRM SQL Server database after DepartmentMaster_ADONET_StoredProcedures.sql.
    AQIS/AQS Approval Department fields are intentionally excluded.
*/

IF OBJECT_ID('dbo.DepartmentSectionMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DepartmentSectionMaster
    (
        SectionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DepartmentSectionMaster PRIMARY KEY,
        SectionCode NVARCHAR(20) NOT NULL,
        Sectionname NVARCHAR(150) NOT NULL,
        Departmentcode NVARCHAR(20) NOT NULL,
        SubDepartmentName NVARCHAR(150) NOT NULL,
        issectionActive BIT NOT NULL CONSTRAINT DF_DepartmentSectionMaster_issectionActive DEFAULT 1,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_DepartmentSectionMaster_CreatedOn DEFAULT SYSDATETIME(),
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DepartmentMaster_DepartmentCode_All' AND object_id = OBJECT_ID('dbo.DepartmentMaster'))
BEGIN
    CREATE UNIQUE INDEX UX_DepartmentMaster_DepartmentCode_All ON dbo.DepartmentMaster(DepartmentCode);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DepartmentSectionMaster_DepartmentMaster')
BEGIN
    ALTER TABLE dbo.DepartmentSectionMaster WITH CHECK
    ADD CONSTRAINT FK_DepartmentSectionMaster_DepartmentMaster
        FOREIGN KEY (Departmentcode) REFERENCES dbo.DepartmentMaster(DepartmentCode);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DepartmentSectionMaster_SectionCode_Active' AND object_id = OBJECT_ID('dbo.DepartmentSectionMaster'))
BEGIN
    CREATE UNIQUE INDEX UX_DepartmentSectionMaster_SectionCode_Active
    ON dbo.DepartmentSectionMaster(SectionCode)
    WHERE issectionActive = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SectionMaster_DepartmentLookup
    @SearchText NVARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 100 DepartmentCode, DepartmentName
    FROM dbo.DepartmentMaster
    WHERE ActiveStatus = 1
      AND (NULLIF(@SearchText, '') IS NULL OR DepartmentCode LIKE '%' + @SearchText + '%' OR DepartmentName LIKE '%' + @SearchText + '%')
    ORDER BY DepartmentCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SectionMaster_GetPaged
    @GlobalSearch NVARCHAR(150) = NULL,
    @SectionCode NVARCHAR(50) = NULL,
    @Sectionname NVARCHAR(150) = NULL,
    @SectionId NVARCHAR(50) = NULL,
    @Departmentcode NVARCHAR(150) = NULL,
    @SubDepartmentName NVARCHAR(150) = NULL,
    @issectionActive NVARCHAR(20) = NULL,
    @CreatedBy NVARCHAR(50) = NULL,
    @EditedBy NVARCHAR(50) = NULL,
    @SortColumn NVARCHAR(50) = 'SectionId',
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
        SectionId INT NOT NULL,
        SectionCode NVARCHAR(20) NOT NULL,
        Sectionname NVARCHAR(150) NOT NULL,
        Departmentcode NVARCHAR(20) NOT NULL,
        SubDepartmentName NVARCHAR(150) NOT NULL,
        issectionActive BIT NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NULL,
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL
    );

    INSERT INTO #Filtered
    SELECT SectionId, SectionCode, Sectionname, Departmentcode, SubDepartmentName,
           issectionActive, CreatedBy, CreatedOn, EditedBy, EditedOn
    FROM dbo.DepartmentSectionMaster
    WHERE issectionActive = 1
      AND (NULLIF(@GlobalSearch, '') IS NULL OR
           SectionCode LIKE '%' + @GlobalSearch + '%' OR Sectionname LIKE '%' + @GlobalSearch + '%' OR
           CONVERT(NVARCHAR(20), SectionId) LIKE '%' + @GlobalSearch + '%' OR Departmentcode LIKE '%' + @GlobalSearch + '%' OR
           SubDepartmentName LIKE '%' + @GlobalSearch + '%' OR ISNULL(CreatedBy,'') LIKE '%' + @GlobalSearch + '%' OR ISNULL(EditedBy,'') LIKE '%' + @GlobalSearch + '%')
      AND (NULLIF(@SectionCode, '') IS NULL OR SectionCode LIKE '%' + @SectionCode + '%')
      AND (NULLIF(@Sectionname, '') IS NULL OR Sectionname LIKE '%' + @Sectionname + '%')
      AND (NULLIF(@SectionId, '') IS NULL OR CONVERT(NVARCHAR(20), SectionId) LIKE '%' + @SectionId + '%')
      AND (NULLIF(@Departmentcode, '') IS NULL OR Departmentcode LIKE '%' + @Departmentcode + '%' OR EXISTS (SELECT 1 FROM dbo.DepartmentMaster d WHERE d.DepartmentCode = DepartmentSectionMaster.Departmentcode AND d.DepartmentName LIKE '%' + @Departmentcode + '%'))
      AND (NULLIF(@SubDepartmentName, '') IS NULL OR SubDepartmentName LIKE '%' + @SubDepartmentName + '%')
      AND (NULLIF(@issectionActive, '') IS NULL OR CASE WHEN issectionActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @issectionActive + '%')
      AND (NULLIF(@CreatedBy, '') IS NULL OR ISNULL(CreatedBy,'') LIKE '%' + @CreatedBy + '%')
      AND (NULLIF(@EditedBy, '') IS NULL OR ISNULL(EditedBy,'') LIKE '%' + @EditedBy + '%');

    ;WITH Numbered AS
    (
        SELECT *, ROW_NUMBER() OVER
        (
            ORDER BY
            CASE WHEN @SortColumn = 'SectionCode' AND @SortDirection = 'ASC' THEN SectionCode END ASC,
            CASE WHEN @SortColumn = 'SectionCode' AND @SortDirection = 'DESC' THEN SectionCode END DESC,
            CASE WHEN @SortColumn = 'Sectionname' AND @SortDirection = 'ASC' THEN Sectionname END ASC,
            CASE WHEN @SortColumn = 'Sectionname' AND @SortDirection = 'DESC' THEN Sectionname END DESC,
            CASE WHEN @SortColumn = 'SectionId' AND @SortDirection = 'ASC' THEN SectionId END ASC,
            CASE WHEN @SortColumn = 'SectionId' AND @SortDirection = 'DESC' THEN SectionId END DESC,
            CASE WHEN @SortColumn = 'Departmentcode' AND @SortDirection = 'ASC' THEN Departmentcode END ASC,
            CASE WHEN @SortColumn = 'Departmentcode' AND @SortDirection = 'DESC' THEN Departmentcode END DESC,
            CASE WHEN @SortColumn = 'SubDepartmentName' AND @SortDirection = 'ASC' THEN SubDepartmentName END ASC,
            CASE WHEN @SortColumn = 'SubDepartmentName' AND @SortDirection = 'DESC' THEN SubDepartmentName END DESC,
            CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'ASC' THEN CreatedOn END ASC,
            CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'DESC' THEN CreatedOn END DESC,
            CASE WHEN @SortColumn = 'EditedOn' AND @SortDirection = 'ASC' THEN EditedOn END ASC,
            CASE WHEN @SortColumn = 'EditedOn' AND @SortDirection = 'DESC' THEN EditedOn END DESC,
            SectionId DESC
        ) AS RowNum
        FROM #Filtered
    )
    SELECT SectionId, SectionCode, Sectionname, Departmentcode, SubDepartmentName,
           issectionActive, CreatedBy, CreatedOn, EditedBy, EditedOn
    FROM Numbered
    WHERE @PageSize = 0 OR RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize)
    ORDER BY RowNum;

    SELECT COUNT(1) AS TotalCount FROM #Filtered;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SectionMaster_GetById
    @SectionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SectionId, SectionCode, Sectionname, Departmentcode, SubDepartmentName,
           issectionActive, CreatedBy, CreatedOn, EditedBy, EditedOn
    FROM dbo.DepartmentSectionMaster
    WHERE SectionId = @SectionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SectionMaster_Save
    @SectionId INT,
    @SectionCode NVARCHAR(20),
    @Sectionname NVARCHAR(150),
    @Departmentcode NVARCHAR(20),
    @SubDepartmentName NVARCHAR(150),
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @SectionCode = LTRIM(RTRIM(@SectionCode));
    SET @Departmentcode = LTRIM(RTRIM(@Departmentcode));

    IF NOT EXISTS (SELECT 1 FROM dbo.DepartmentMaster WHERE DepartmentCode = @Departmentcode AND ActiveStatus = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'Please select a valid Department Code';
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.DepartmentSectionMaster WHERE SectionCode = @SectionCode AND issectionActive = 1 AND SectionId <> ISNULL(@SectionId, 0))
    BEGIN
        SET @Status = 0;
        SET @Message = 'Sectioncode Already Exists';
        RETURN;
    END

    IF ISNULL(@SectionId, 0) = 0
    BEGIN
        INSERT INTO dbo.DepartmentSectionMaster(SectionCode, Sectionname, Departmentcode, SubDepartmentName, issectionActive, CreatedBy, CreatedOn)
        VALUES(@SectionCode, @Sectionname, @Departmentcode, @SubDepartmentName, 1, @EmployeeCode, SYSDATETIME());
        SET @Message = 'Section saved successfully';
    END
    ELSE
    BEGIN
        UPDATE dbo.DepartmentSectionMaster
        SET Sectionname = @Sectionname,
            Departmentcode = @Departmentcode,
            SubDepartmentName = @SubDepartmentName,
            EditedBy = @EmployeeCode,
            EditedOn = SYSDATETIME()
        WHERE SectionId = @SectionId AND issectionActive = 1;
        SET @Message = 'Section updated successfully';
    END
    SET @Status = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SectionMaster_Delete
    @SectionId INT,
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.DepartmentSectionMaster WHERE SectionId = @SectionId AND issectionActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'Section not found';
        RETURN;
    END

    UPDATE dbo.DepartmentSectionMaster
    SET issectionActive = 0,
        EditedBy = @EmployeeCode,
        EditedOn = SYSDATETIME()
    WHERE SectionId = @SectionId;

    SET @Status = 1;
    SET @Message = 'Section deleted successfully';
END
GO
