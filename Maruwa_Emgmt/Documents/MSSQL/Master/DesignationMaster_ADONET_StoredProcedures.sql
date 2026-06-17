/*
    Designation Master ADO.NET SQL script
    Execute this script manually in SQL Server before using the Designation Master screen.
*/

IF OBJECT_ID('dbo.InsuranceCategoryMaster','U') IS NULL
BEGIN
    CREATE TABLE dbo.InsuranceCategoryMaster
    (
        InsuranceCategoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InsuranceCategoryMaster PRIMARY KEY,
        CategoryCode NVARCHAR(10) NOT NULL CONSTRAINT UQ_InsuranceCategoryMaster_Code UNIQUE,
        CategoryName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_InsuranceCategoryMaster_IsActive DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.InsuranceCategoryMaster WHERE CategoryCode IN ('A','B','C','D'))
BEGIN
    INSERT INTO dbo.InsuranceCategoryMaster(CategoryCode, CategoryName) VALUES
    ('A','Category A'),('B','Category B'),('C','Category C'),('D','Category D');
END
GO

IF OBJECT_ID('dbo.ProbationMaster','U') IS NULL
BEGIN
    CREATE TABLE dbo.ProbationMaster
    (
        ProbationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProbationMaster PRIMARY KEY,
        ProbationCode NVARCHAR(10) NOT NULL CONSTRAINT UQ_ProbationMaster_Code UNIQUE,
        ProbationName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ProbationMaster_IsActive DEFAULT(1)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ProbationMaster WHERE ProbationCode IN ('0','3','4','6'))
BEGIN
    INSERT INTO dbo.ProbationMaster(ProbationCode, ProbationName) VALUES
    ('0','0 Months'),('3','3 Months'),('4','4 Months'),('6','6 Months');
END
GO

IF OBJECT_ID('dbo.DesignationMaster','U') IS NULL
BEGIN
    CREATE TABLE dbo.DesignationMaster
    (
        Sno INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DesignationMaster PRIMARY KEY,
        insCatergory NVARCHAR(10) NOT NULL,
        dlevel NVARCHAR(20) NULL,
        probation NVARCHAR(10) NOT NULL,
        CTQLevel NVARCHAR(20) NULL,
        kpi NVARCHAR(20) NULL,
        positioned NVARCHAR(20) NULL,
        insamount DECIMAL(18,2) NOT NULL CONSTRAINT DF_DesignationMaster_insamount DEFAULT(0),
        designationcode NVARCHAR(50) NOT NULL,
        designationName NVARCHAR(150) NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_DesignationMaster_CreatedOn DEFAULT(SYSDATETIME()),
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL,
        isActive BIT NOT NULL CONSTRAINT DF_DesignationMaster_isActive DEFAULT(1),
        CONSTRAINT UQ_DesignationMaster_designationcode UNIQUE(designationcode),
        CONSTRAINT FK_DesignationMaster_InsuranceCategory FOREIGN KEY(insCatergory) REFERENCES dbo.InsuranceCategoryMaster(CategoryCode),
        CONSTRAINT FK_DesignationMaster_Probation FOREIGN KEY(probation) REFERENCES dbo.ProbationMaster(ProbationCode)
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsuranceCategoryMaster_Lookup
    @SearchText NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT InsuranceCategoryId, CategoryCode, CategoryName
    FROM dbo.InsuranceCategoryMaster
    WHERE IsActive = 1
      AND (ISNULL(@SearchText,'') = '' OR CategoryCode LIKE '%' + @SearchText + '%' OR CategoryName LIKE '%' + @SearchText + '%')
    ORDER BY CategoryCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ProbationMaster_Lookup
    @SearchText NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ProbationId, ProbationCode, ProbationName
    FROM dbo.ProbationMaster
    WHERE IsActive = 1
      AND (ISNULL(@SearchText,'') = '' OR ProbationCode LIKE '%' + @SearchText + '%' OR ProbationName LIKE '%' + @SearchText + '%')
    ORDER BY TRY_CONVERT(INT, ProbationCode), ProbationCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DesignationMaster_GetPaged
    @GlobalSearch NVARCHAR(200) = NULL,
    @designationcode NVARCHAR(100) = NULL,
    @designationName NVARCHAR(150) = NULL,
    @probation NVARCHAR(50) = NULL,
    @insCatergory NVARCHAR(50) = NULL,
    @insamount NVARCHAR(50) = NULL,
    @CreatedBy NVARCHAR(50) = NULL,
    @EditedBy NVARCHAR(50) = NULL,
    @isActive NVARCHAR(50) = NULL,
    @SortColumn NVARCHAR(50) = 'Sno',
    @SortDirection NVARCHAR(4) = 'DESC',
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageNumber < 1 SET @PageNumber = 1;

    ;WITH Filtered AS
    (
        SELECT Sno, insCatergory, dlevel, probation, CTQLevel, kpi, positioned, insamount, designationcode, designationName,
               CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
        FROM dbo.DesignationMaster
        WHERE (ISNULL(@GlobalSearch,'') = '' OR
               designationcode LIKE '%' + @GlobalSearch + '%' OR designationName LIKE '%' + @GlobalSearch + '%' OR
               probation LIKE '%' + @GlobalSearch + '%' OR insCatergory LIKE '%' + @GlobalSearch + '%' OR
               CONVERT(NVARCHAR(50), insamount) LIKE '%' + @GlobalSearch + '%' OR ISNULL(CreatedBy,'') LIKE '%' + @GlobalSearch + '%' OR
               ISNULL(EditedBy,'') LIKE '%' + @GlobalSearch + '%' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @GlobalSearch + '%')
          AND (ISNULL(@designationcode,'') = '' OR designationcode LIKE '%' + @designationcode + '%')
          AND (ISNULL(@designationName,'') = '' OR designationName LIKE '%' + @designationName + '%')
          AND (ISNULL(@probation,'') = '' OR probation LIKE '%' + @probation + '%')
          AND (ISNULL(@insCatergory,'') = '' OR insCatergory LIKE '%' + @insCatergory + '%')
          AND (ISNULL(@insamount,'') = '' OR CONVERT(NVARCHAR(50), insamount) LIKE '%' + @insamount + '%')
          AND (ISNULL(@CreatedBy,'') = '' OR ISNULL(CreatedBy,'') LIKE '%' + @CreatedBy + '%')
          AND (ISNULL(@EditedBy,'') = '' OR ISNULL(EditedBy,'') LIKE '%' + @EditedBy + '%')
          AND (ISNULL(@isActive,'') = '' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @isActive + '%')
    )
    SELECT *
    FROM Filtered
    ORDER BY
        CASE WHEN @SortColumn='designationcode' AND @SortDirection='ASC' THEN designationcode END ASC,
        CASE WHEN @SortColumn='designationcode' AND @SortDirection='DESC' THEN designationcode END DESC,
        CASE WHEN @SortColumn='designationName' AND @SortDirection='ASC' THEN designationName END ASC,
        CASE WHEN @SortColumn='designationName' AND @SortDirection='DESC' THEN designationName END DESC,
        CASE WHEN @SortColumn='probation' AND @SortDirection='ASC' THEN probation END ASC,
        CASE WHEN @SortColumn='probation' AND @SortDirection='DESC' THEN probation END DESC,
        CASE WHEN @SortColumn='insCatergory' AND @SortDirection='ASC' THEN insCatergory END ASC,
        CASE WHEN @SortColumn='insCatergory' AND @SortDirection='DESC' THEN insCatergory END DESC,
        CASE WHEN @SortColumn='insamount' AND @SortDirection='ASC' THEN insamount END ASC,
        CASE WHEN @SortColumn='insamount' AND @SortDirection='DESC' THEN insamount END DESC,
        CASE WHEN @SortColumn='CreatedOn' AND @SortDirection='ASC' THEN CreatedOn END ASC,
        CASE WHEN @SortColumn='CreatedOn' AND @SortDirection='DESC' THEN CreatedOn END DESC,
        CASE WHEN @SortColumn='EditedOn' AND @SortDirection='ASC' THEN EditedOn END ASC,
        CASE WHEN @SortColumn='EditedOn' AND @SortDirection='DESC' THEN EditedOn END DESC,
        Sno DESC
    OFFSET CASE WHEN @PageSize = 0 THEN 0 ELSE (@PageNumber - 1) * @PageSize END ROWS
    FETCH NEXT CASE WHEN @PageSize = 0 THEN 2147483647 ELSE @PageSize END ROWS ONLY;

    SELECT COUNT(1) AS TotalCount
    FROM dbo.DesignationMaster
    WHERE (ISNULL(@GlobalSearch,'') = '' OR
           designationcode LIKE '%' + @GlobalSearch + '%' OR designationName LIKE '%' + @GlobalSearch + '%' OR
           probation LIKE '%' + @GlobalSearch + '%' OR insCatergory LIKE '%' + @GlobalSearch + '%' OR
           CONVERT(NVARCHAR(50), insamount) LIKE '%' + @GlobalSearch + '%' OR ISNULL(CreatedBy,'') LIKE '%' + @GlobalSearch + '%' OR
           ISNULL(EditedBy,'') LIKE '%' + @GlobalSearch + '%' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @GlobalSearch + '%')
      AND (ISNULL(@designationcode,'') = '' OR designationcode LIKE '%' + @designationcode + '%')
      AND (ISNULL(@designationName,'') = '' OR designationName LIKE '%' + @designationName + '%')
      AND (ISNULL(@probation,'') = '' OR probation LIKE '%' + @probation + '%')
      AND (ISNULL(@insCatergory,'') = '' OR insCatergory LIKE '%' + @insCatergory + '%')
      AND (ISNULL(@insamount,'') = '' OR CONVERT(NVARCHAR(50), insamount) LIKE '%' + @insamount + '%')
      AND (ISNULL(@CreatedBy,'') = '' OR ISNULL(CreatedBy,'') LIKE '%' + @CreatedBy + '%')
      AND (ISNULL(@EditedBy,'') = '' OR ISNULL(EditedBy,'') LIKE '%' + @EditedBy + '%')
      AND (ISNULL(@isActive,'') = '' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @isActive + '%');
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DesignationMaster_GetById
    @Sno INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Sno, insCatergory, dlevel, probation, CTQLevel, kpi, positioned, insamount, designationcode, designationName,
           CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM dbo.DesignationMaster
    WHERE Sno = @Sno;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DesignationMaster_Save
    @Sno INT,
    @designationcode NVARCHAR(50),
    @designationName NVARCHAR(150),
    @insCatergory NVARCHAR(10),
    @dlevel NVARCHAR(20) = NULL,
    @probation NVARCHAR(10),
    @CTQLevel NVARCHAR(20) = NULL,
    @kpi NVARCHAR(20) = NULL,
    @positioned NVARCHAR(20) = NULL,
    @insamount DECIMAL(18,2),
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.DesignationMaster WHERE designationcode = @designationcode AND Sno <> ISNULL(@Sno,0))
    BEGIN
        SET @Status = 0; SET @Message = 'Designationcode Already Exists'; RETURN;
    END
    IF @Sno IS NULL OR @Sno = 0
    BEGIN
        INSERT INTO dbo.DesignationMaster(insCatergory,dlevel,probation,CTQLevel,kpi,positioned,insamount,designationcode,designationName,CreatedBy,CreatedOn,isActive)
        VALUES(@insCatergory,@dlevel,@probation,@CTQLevel,@kpi,@positioned,@insamount,@designationcode,@designationName,@EmployeeCode,SYSDATETIME(),1);
        SET @Status = 1; SET @Message = 'Designation saved successfully'; RETURN;
    END
    UPDATE dbo.DesignationMaster
       SET designationName=@designationName, insCatergory=@insCatergory, dlevel=@dlevel, probation=@probation, CTQLevel=@CTQLevel,
           kpi=@kpi, positioned=@positioned, insamount=@insamount, EditedBy=@EmployeeCode, EditedOn=SYSDATETIME(), isActive=1
     WHERE Sno=@Sno;
    SET @Status = 1; SET @Message = 'Designation updated successfully';
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DesignationMaster_Delete
    @Sno INT,
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.DesignationMaster WHERE Sno=@Sno)
    BEGIN
        SET @Status=0; SET @Message='Designation not found'; RETURN;
    END
    UPDATE dbo.DesignationMaster SET isActive=0, EditedBy=@EmployeeCode, EditedOn=SYSDATETIME() WHERE Sno=@Sno;
    SET @Status=1; SET @Message='Designation deleted successfully';
END
GO
