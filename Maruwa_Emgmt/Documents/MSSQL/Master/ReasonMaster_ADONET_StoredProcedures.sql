/*
    Reason Master table and stored procedures.
    Execute this script manually in SQL Server database configured by EHRMConnection.
*/

IF OBJECT_ID('dbo.ReasonTypeMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReasonTypeMaster
    (
        ReasonID NVARCHAR(50) NOT NULL CONSTRAINT PK_ReasonTypeMaster PRIMARY KEY,
        ReasonType NVARCHAR(100) NOT NULL,
        ReasonDescription NVARCHAR(500) NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_ReasonTypeMaster_CreatedOn DEFAULT SYSUTCDATETIME(),
        EditedBy NVARCHAR(50) NULL,
        EditedOn DATETIME2(0) NULL,
        isActive BIT NOT NULL CONSTRAINT DF_ReasonTypeMaster_isActive DEFAULT 1
    );
END
GO

MERGE dbo.ReasonTypeMaster AS target
USING (VALUES
('1','Allergy','Allergy'),
('2','Back Pain','Back Pain'),
('3','Body Pain','Body Pain'),
('4','Chest Pain','Chest Pain'),
('5','Chicken Pox','Chicken Pox'),
('6','Cough','Cough'),
('7','Diarhea','Diarhea'),
('8','Dressing wound','Dressing wound'),
('9','Eye infection/conjuctivities','Eye infection/conjuctivities'),
('10','Fainted','Fainted'),
('11','Fever','Fever'),
('12','Flu','Flu'),
('13','Gastric','Gastric'),
('14','Headache','Headache'),
('15','High Blood pressure','High Blood pressure'),
('16','Dehydration','Dehydration'),
('17','Hypertension','Hypertension'),
('18','Injured','Injured'),
('19','Low Blood Pressure','Low Blood Pressure'),
('20','Muscle Pain','Muscle Pain'),
('21','Menstrual Pain','Menstrual Pain'),
('22','Post Trauma','Post Trauma'),
('23','Skin Infection','Skin Infection'),
('24','Stomach ache','Stomach ache'),
('25','Tissue Injury','Tissue Injury'),
('26','Tooth ache','Tooth ache'),
('27','Vomitting','Vomitting'),
('28','Sprained','Sprained'),
('29','Hand Pain','Hand Pain'),
('30','Leg Pain','Leg Pain')
) AS source (ReasonID, ReasonType, ReasonDescription)
ON target.ReasonID = source.ReasonID
WHEN MATCHED THEN
    UPDATE SET ReasonType = source.ReasonType, ReasonDescription = source.ReasonDescription, isActive = 1
WHEN NOT MATCHED THEN
    INSERT (ReasonID, ReasonType, ReasonDescription, CreatedBy, CreatedOn, isActive)
    VALUES (source.ReasonID, source.ReasonType, source.ReasonDescription, 'SYSTEM', SYSUTCDATETIME(), 1);
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReasonTypeMaster_GetPaged
    @GlobalSearch NVARCHAR(200) = NULL,
    @ReasonID NVARCHAR(50) = NULL,
    @ReasonType NVARCHAR(100) = NULL,
    @ReasonDescription NVARCHAR(500) = NULL,
    @CreatedBy NVARCHAR(50) = NULL,
    @EditedBy NVARCHAR(50) = NULL,
    @isActive NVARCHAR(20) = NULL,
    @SortColumn NVARCHAR(50) = 'ReasonID',
    @SortDirection NVARCHAR(4) = 'ASC',
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SET @SortColumn = CASE WHEN @SortColumn IN ('ReasonID','ReasonType','ReasonDescription','CreatedBy','CreatedOn','EditedBy','EditedOn','isActive') THEN @SortColumn ELSE 'ReasonID' END;
    SET @SortDirection = CASE WHEN UPPER(@SortDirection) = 'DESC' THEN 'DESC' ELSE 'ASC' END;
    SET @PageNumber = CASE WHEN ISNULL(@PageNumber, 0) <= 0 THEN 1 ELSE @PageNumber END;

    SELECT ReasonID, ReasonType, ReasonDescription, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    INTO #FilteredReason
    FROM dbo.ReasonTypeMaster
    WHERE
        (@GlobalSearch IS NULL OR @GlobalSearch = '' OR
            ReasonID LIKE '%' + @GlobalSearch + '%' OR
            ReasonType LIKE '%' + @GlobalSearch + '%' OR
            ReasonDescription LIKE '%' + @GlobalSearch + '%' OR
            CreatedBy LIKE '%' + @GlobalSearch + '%' OR
            EditedBy LIKE '%' + @GlobalSearch + '%' OR
            CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @GlobalSearch + '%')
        AND (@ReasonID IS NULL OR @ReasonID = '' OR ReasonID LIKE '%' + @ReasonID + '%')
        AND (@ReasonType IS NULL OR @ReasonType = '' OR ReasonType LIKE '%' + @ReasonType + '%')
        AND (@ReasonDescription IS NULL OR @ReasonDescription = '' OR ReasonDescription LIKE '%' + @ReasonDescription + '%')
        AND (@CreatedBy IS NULL OR @CreatedBy = '' OR CreatedBy LIKE '%' + @CreatedBy + '%')
        AND (@EditedBy IS NULL OR @EditedBy = '' OR EditedBy LIKE '%' + @EditedBy + '%')
        AND (@isActive IS NULL OR @isActive = '' OR CASE WHEN isActive = 1 THEN 'Active' ELSE 'Inactive' END LIKE '%' + @isActive + '%');

    ;WITH Ordered AS
    (
        SELECT *, ROW_NUMBER() OVER
        (
            ORDER BY
                CASE WHEN @SortColumn = 'ReasonID' AND @SortDirection = 'ASC' THEN ReasonID END ASC,
                CASE WHEN @SortColumn = 'ReasonID' AND @SortDirection = 'DESC' THEN ReasonID END DESC,
                CASE WHEN @SortColumn = 'ReasonType' AND @SortDirection = 'ASC' THEN ReasonType END ASC,
                CASE WHEN @SortColumn = 'ReasonType' AND @SortDirection = 'DESC' THEN ReasonType END DESC,
                CASE WHEN @SortColumn = 'ReasonDescription' AND @SortDirection = 'ASC' THEN ReasonDescription END ASC,
                CASE WHEN @SortColumn = 'ReasonDescription' AND @SortDirection = 'DESC' THEN ReasonDescription END DESC,
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
                TRY_CONVERT(INT, ReasonID) ASC,
                ReasonID ASC
        ) AS RowNum
        FROM #FilteredReason
    )
    SELECT ReasonID, ReasonType, ReasonDescription, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM Ordered
    WHERE @PageSize = 0 OR RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);

    SELECT COUNT(1) AS TotalCount FROM #FilteredReason;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReasonTypeMaster_GetById
    @ReasonID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ReasonID, ReasonType, ReasonDescription, CreatedBy, CreatedOn, EditedBy, EditedOn, isActive
    FROM dbo.ReasonTypeMaster
    WHERE ReasonID = @ReasonID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReasonTypeMaster_Save
    @ReasonID NVARCHAR(50) = NULL,
    @ReasonType NVARCHAR(100),
    @ReasonDescription NVARCHAR(500),
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @ReasonID = NULLIF(LTRIM(RTRIM(@ReasonID)), '');

    IF @ReasonID IS NULL
    BEGIN
        SELECT @ReasonID = CONVERT(NVARCHAR(50), ISNULL(MAX(TRY_CONVERT(INT, ReasonID)), 0) + 1)
        FROM dbo.ReasonTypeMaster;
    END

    IF EXISTS (SELECT 1 FROM dbo.ReasonTypeMaster WHERE ReasonID = @ReasonID AND isActive = 1)
    BEGIN
        UPDATE dbo.ReasonTypeMaster
           SET ReasonType = @ReasonType,
               ReasonDescription = @ReasonDescription,
               EditedBy = @EmployeeCode,
               EditedOn = SYSUTCDATETIME()
         WHERE ReasonID = @ReasonID;
        SET @Status = 1;
        SET @Message = 'Reason updated successfully';
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.ReasonTypeMaster WHERE ReasonType = @ReasonType AND ReasonID <> @ReasonID AND isActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'ReasonType Already Exists';
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.ReasonTypeMaster WHERE ReasonID = @ReasonID AND isActive = 0)
    BEGIN
        UPDATE dbo.ReasonTypeMaster
           SET ReasonType = @ReasonType,
               ReasonDescription = @ReasonDescription,
               isActive = 1,
               EditedBy = @EmployeeCode,
               EditedOn = SYSUTCDATETIME()
         WHERE ReasonID = @ReasonID;
        SET @Status = 1;
        SET @Message = 'Reason restored and updated successfully';
        RETURN;
    END

    INSERT INTO dbo.ReasonTypeMaster (ReasonID, ReasonType, ReasonDescription, CreatedBy, CreatedOn, isActive)
    VALUES (@ReasonID, @ReasonType, @ReasonDescription, @EmployeeCode, SYSUTCDATETIME(), 1);

    SET @Status = 1;
    SET @Message = 'Reason saved successfully';
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ReasonTypeMaster_Delete
    @ReasonID NVARCHAR(50) = NULL,
    @EmployeeCode NVARCHAR(50),
    @Status INT OUTPUT,
    @Message NVARCHAR(250) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.ReasonTypeMaster WHERE ReasonID = @ReasonID AND isActive = 1)
    BEGIN
        SET @Status = 0;
        SET @Message = 'Reason not found or already inactive';
        RETURN;
    END

    UPDATE dbo.ReasonTypeMaster
       SET isActive = 0,
           EditedBy = @EmployeeCode,
           EditedOn = SYSUTCDATETIME()
     WHERE ReasonID = @ReasonID;

    SET @Status = 1;
    SET @Message = 'Reason deleted successfully';
END
GO
