/*
    Employee Grievance Form - DB Script
    Execute this script manually in the E-Management SQL Server database.
    All application DB operations are performed through stored procedures/functions only.
*/

IF OBJECT_ID('dbo.EmployeeGrievanceComplaint', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeGrievanceComplaint
    (
        GrievanceID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeGrievanceComplaint PRIMARY KEY,
        ReferenceNo NVARCHAR(30) NOT NULL CONSTRAINT UQ_EmployeeGrievanceComplaint_ReferenceNo UNIQUE,
        ComplainantEmpId NVARCHAR(100) NOT NULL,
        ComplainantName NVARCHAR(200) NULL,
        Department NVARCHAR(200) NULL,
        PositionTitle NVARCHAR(200) NULL,
        DateOfReport DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeGrievanceComplaint_DateOfReport DEFAULT SYSUTCDATETIME(),

        UnfairTreatment BIT NOT NULL CONSTRAINT DF_EGF_UnfairTreatment DEFAULT 0,
        HarassmentBullying BIT NOT NULL CONSTRAINT DF_EGF_HarassmentBullying DEFAULT 0,
        WorkLapses BIT NOT NULL CONSTRAINT DF_EGF_WorkLapses DEFAULT 0,
        PolicySopBreach BIT NOT NULL CONSTRAINT DF_EGF_PolicySopBreach DEFAULT 0,
        OshaConcern BIT NOT NULL CONSTRAINT DF_EGF_OshaConcern DEFAULT 0,
        SupervisorMisconduct BIT NOT NULL CONSTRAINT DF_EGF_SupervisorMisconduct DEFAULT 0,
        AbuseOfAuthority BIT NOT NULL CONSTRAINT DF_EGF_AbuseOfAuthority DEFAULT 0,
        WorkingHoursIssue BIT NOT NULL CONSTRAINT DF_EGF_WorkingHoursIssue DEFAULT 0,
        OtherComplaint BIT NOT NULL CONSTRAINT DF_EGF_OtherComplaint DEFAULT 0,
        OtherComplaintText NVARCHAR(500) NULL,

        ConductDate DATE NULL,
        ConductTime TIME(0) NULL,
        Location NVARCHAR(300) NULL,
        IncidentDescription NVARCHAR(MAX) NULL,
        Witnesses NVARCHAR(MAX) NULL,
        SupportingDocumentsAttached BIT NOT NULL CONSTRAINT DF_EGF_SupportingDocumentsAttached DEFAULT 0,
        DesiredOutcome NVARCHAR(MAX) NULL,

        EmployeeSignaturePath NVARCHAR(1000) NULL,
        DeclarationEmployeeName NVARCHAR(200) NULL,
        DeclarationEmployeeId NVARCHAR(100) NULL,
        DeclarationDate DATETIME2(0) NULL,

        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_EGF_Status DEFAULT 'InProgress',
        HRRemarks NVARCHAR(MAX) NULL,
        CreatedBy NVARCHAR(100) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EGF_CreatedOn DEFAULT SYSUTCDATETIME(),
        EditedBy NVARCHAR(100) NULL,
        EditedOn DATETIME2(0) NULL,
        isActive BIT NOT NULL CONSTRAINT DF_EGF_isActive DEFAULT 1
    );
END
GO

IF COL_LENGTH('dbo.EmployeeGrievanceComplaint', 'HRRemarks') IS NULL
    ALTER TABLE dbo.EmployeeGrievanceComplaint ADD HRRemarks NVARCHAR(MAX) NULL;
GO

IF OBJECT_ID('dbo.EmployeeGrievanceInvolvedParty', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeGrievanceInvolvedParty
    (
        PartyID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeGrievanceInvolvedParty PRIMARY KEY,
        GrievanceID INT NOT NULL,
        EmployeeID NVARCHAR(100) NULL,
        EmployeeName NVARCHAR(200) NULL,
        PositionTitle NVARCHAR(200) NULL,
        Department NVARCHAR(200) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeGrievanceInvolvedParty_CreatedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID('dbo.EmployeeGrievanceAttachment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeGrievanceAttachment
    (
        AttachmentID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeGrievanceAttachment PRIMARY KEY,
        GrievanceID INT NOT NULL,
        OriginalFileName NVARCHAR(260) NOT NULL,
        StoredFileName NVARCHAR(260) NOT NULL,
        FilePath NVARCHAR(1000) NOT NULL,
        ContentType NVARCHAR(200) NULL,
        SizeBytes BIGINT NULL,
        UploadedBy NVARCHAR(100) NULL,
        UploadedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeGrievanceAttachment_UploadedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID('dbo.EmployeeGrievanceHRAction', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeGrievanceHRAction
    (
        HRActionID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeGrievanceHRAction PRIMARY KEY,
        GrievanceID INT NOT NULL CONSTRAINT UQ_EmployeeGrievanceHRAction_GrievanceID UNIQUE,
        HREmpId NVARCHAR(100) NULL,
        HRName NVARCHAR(200) NULL,
        ActionDate DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeGrievanceHRAction_ActionDate DEFAULT SYSUTCDATETIME(),
        ActionEmployeeId NVARCHAR(100) NULL,
        ActionEmployeeName NVARCHAR(200) NULL,
        InvestigationSummary NVARCHAR(MAX) NULL,
        EmployeeExplanation NVARCHAR(MAX) NULL,
        Remarks NVARCHAR(MAX) NULL,
        OutcomeResolved BIT NOT NULL CONSTRAINT DF_EGF_HR_OutcomeResolved DEFAULT 0,
        OutcomeReferredToER BIT NOT NULL CONSTRAINT DF_EGF_HR_OutcomeReferredToER DEFAULT 0,
        OutcomeReferredToDomesticInquiry BIT NOT NULL CONSTRAINT DF_EGF_HR_OutcomeReferredToDomesticInquiry DEFAULT 0,
        MinorMisconduct BIT NOT NULL CONSTRAINT DF_EGF_HR_MinorMisconduct DEFAULT 0,
        MajorMisconduct BIT NOT NULL CONSTRAINT DF_EGF_HR_MajorMisconduct DEFAULT 0,
        MajorMisconductText NVARCHAR(500) NULL,
        HRSignaturePath NVARCHAR(1000) NULL,
        Department NVARCHAR(200) NULL,
        CreatedBy NVARCHAR(100) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EGF_HR_CreatedOn DEFAULT SYSUTCDATETIME(),
        EditedBy NVARCHAR(100) NULL,
        EditedOn DATETIME2(0) NULL
    );
END
GO

IF COL_LENGTH('dbo.EmployeeGrievanceHRAction', 'ActionEmployeeId') IS NULL
    ALTER TABLE dbo.EmployeeGrievanceHRAction ADD ActionEmployeeId NVARCHAR(100) NULL;
GO
IF COL_LENGTH('dbo.EmployeeGrievanceHRAction', 'ActionEmployeeName') IS NULL
    ALTER TABLE dbo.EmployeeGrievanceHRAction ADD ActionEmployeeName NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.EmployeeGrievanceHRAction', 'Remarks') IS NULL
    ALTER TABLE dbo.EmployeeGrievanceHRAction ADD Remarks NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH('dbo.EmployeeGrievanceHRAction', 'HRSignaturePath') IS NULL
    ALTER TABLE dbo.EmployeeGrievanceHRAction ADD HRSignaturePath NVARCHAR(1000) NULL;
GO

IF OBJECT_ID('dbo.EmployeeGrievanceHRActionEmployee', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeGrievanceHRActionEmployee
    (
        HRActionEmployeeID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeGrievanceHRActionEmployee PRIMARY KEY,
        HRActionID INT NOT NULL,
        GrievanceID INT NOT NULL,
        EmployeeID NVARCHAR(100) NULL,
        EmployeeName NVARCHAR(200) NULL,
        PositionTitle NVARCHAR(200) NULL,
        Department NVARCHAR(200) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EGF_HRActionEmployee_CreatedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

IF COL_LENGTH('dbo.EmployeeGrievanceHRAction', 'EmployeeSignaturePath') IS NOT NULL
BEGIN
    -- Old column kept if already exists; new implementation uses HRSignaturePath only.
    PRINT 'EmployeeSignaturePath column exists from older script and will remain unused.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EGFParty_EGFComplaint')
BEGIN
    ALTER TABLE dbo.EmployeeGrievanceInvolvedParty
    ADD CONSTRAINT FK_EGFParty_EGFComplaint FOREIGN KEY (GrievanceID) REFERENCES dbo.EmployeeGrievanceComplaint(GrievanceID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EGFAttachment_EGFComplaint')
BEGIN
    ALTER TABLE dbo.EmployeeGrievanceAttachment
    ADD CONSTRAINT FK_EGFAttachment_EGFComplaint FOREIGN KEY (GrievanceID) REFERENCES dbo.EmployeeGrievanceComplaint(GrievanceID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EGFHRAction_EGFComplaint')
BEGIN
    ALTER TABLE dbo.EmployeeGrievanceHRAction
    ADD CONSTRAINT FK_EGFHRAction_EGFComplaint FOREIGN KEY (GrievanceID) REFERENCES dbo.EmployeeGrievanceComplaint(GrievanceID);
END
GO


IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EGFHRActionEmployee_EGFHRAction')
BEGIN
    ALTER TABLE dbo.EmployeeGrievanceHRActionEmployee
    ADD CONSTRAINT FK_EGFHRActionEmployee_EGFHRAction FOREIGN KEY (HRActionID) REFERENCES dbo.EmployeeGrievanceHRAction(HRActionID);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EGFHRActionEmployee_EGFComplaint')
BEGIN
    ALTER TABLE dbo.EmployeeGrievanceHRActionEmployee
    ADD CONSTRAINT FK_EGFHRActionEmployee_EGFComplaint FOREIGN KEY (GrievanceID) REFERENCES dbo.EmployeeGrievanceComplaint(GrievanceID);
END
GO

CREATE OR ALTER FUNCTION dbo.fn_EGF_GenerateReferenceNo(@ReportDate DATE)
RETURNS NVARCHAR(30)
AS
BEGIN
    DECLARE @Prefix NVARCHAR(6) = FORMAT(ISNULL(@ReportDate, GETDATE()), 'ddMMyy');
    DECLARE @NextNo INT;

    SELECT @NextNo = ISNULL(MAX(TRY_CONVERT(INT, RIGHT(ReferenceNo, LEN(ReferenceNo) - 7))), 0) + 1
    FROM dbo.EmployeeGrievanceComplaint
    WHERE ReferenceNo LIKE @Prefix + '-%';

    RETURN @Prefix + '-' + RIGHT('00' + CONVERT(NVARCHAR(10), @NextNo), 2);
END
GO

CREATE OR ALTER FUNCTION dbo.fn_EGF_FormatDepartment(@Department NVARCHAR(200))
RETURNS NVARCHAR(500)
AS
BEGIN
    DECLARE @DeptCode NVARCHAR(200) = LTRIM(RTRIM(ISNULL(@Department, '')));
    DECLARE @DeptName NVARCHAR(200) = NULL;

    IF @DeptCode = '' RETURN '';
    IF CHARINDEX(' - ', @DeptCode) > 0 RETURN @DeptCode;

    IF OBJECT_ID('dbo.DepartmentMaster', 'U') IS NOT NULL
    BEGIN
        SELECT TOP (1) @DeptName = LTRIM(RTRIM(ISNULL(DepartmentName, '')))
        FROM dbo.DepartmentMaster
        WHERE LTRIM(RTRIM(ISNULL(DepartmentCode, ''))) = @DeptCode;
    END

    IF ISNULL(@DeptName, '') = '' AND OBJECT_ID('dbo.master_Department', 'U') IS NOT NULL
    BEGIN
        SELECT TOP (1) @DeptName = LTRIM(RTRIM(ISNULL(departmentName, '')))
        FROM dbo.master_Department
        WHERE LTRIM(RTRIM(ISNULL(departmentCode, ''))) = @DeptCode;
    END

    RETURN @DeptCode + CASE WHEN ISNULL(@DeptName, '') <> '' THEN ' - ' + @DeptName ELSE '' END;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_SearchEmployee
    @SearchText NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (25)
        e.empCode AS EmpCode,
        e.empName AS EmpName,
        dbo.fn_EGF_FormatDepartment(e.department) AS Department,
        ISNULL(e.designation, '') AS PositionTitle,
        ISNULL(e.emailID, '') AS EmailID
    FROM dbo.empMaster e
    WHERE (@SearchText IS NULL OR @SearchText = '' OR e.empCode LIKE '%' + @SearchText + '%' OR e.empName LIKE '%' + @SearchText + '%')
    ORDER BY e.empCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetEmployeeByCode
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        e.empCode AS EmpCode,
        e.empName AS EmpName,
        dbo.fn_EGF_FormatDepartment(e.department) AS Department,
        ISNULL(e.designation, '') AS PositionTitle,
        ISNULL(e.emailID, '') AS EmailID
    FROM dbo.empMaster e
    WHERE e.empCode = @EmpCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetMyGrievanceList
    @LoggedInEmpCode NVARCHAR(100),
    @IsHrUser BIT = 0,
    @GlobalSearch NVARCHAR(200) = NULL,
    @ReferenceNo NVARCHAR(30) = NULL,
    @StatusFilter NVARCHAR(50) = NULL,
    @SortColumn NVARCHAR(50) = 'CreatedOn',
    @SortDirection NVARCHAR(4) = 'DESC',
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 10;

    SELECT
        GrievanceID,
        ReferenceNo,
        ComplainantEmpId,
        ComplainantName,
        dbo.fn_EGF_FormatDepartment(Department) AS Department,
        DateOfReport,
        LEFT(ISNULL(IncidentDescription, ''), 200) AS GrievanceSummary,
        Status,
        ISNULL(HRRemarks, '') AS HRRemarks,
        CreatedOn
    INTO #Filtered
    FROM dbo.EmployeeGrievanceComplaint
    WHERE isActive = 1
      AND (@IsHrUser = 1 OR ComplainantEmpId = @LoggedInEmpCode)
      AND (@GlobalSearch IS NULL OR @GlobalSearch = '' OR ReferenceNo LIKE '%' + @GlobalSearch + '%' OR ComplainantEmpId LIKE '%' + @GlobalSearch + '%' OR ComplainantName LIKE '%' + @GlobalSearch + '%' OR Department LIKE '%' + @GlobalSearch + '%' OR dbo.fn_EGF_FormatDepartment(Department) LIKE '%' + @GlobalSearch + '%' OR Status LIKE '%' + @GlobalSearch + '%' OR ISNULL(HRRemarks, '') LIKE '%' + @GlobalSearch + '%')
      AND (@ReferenceNo IS NULL OR @ReferenceNo = '' OR ReferenceNo LIKE '%' + @ReferenceNo + '%')
      AND (@StatusFilter IS NULL OR @StatusFilter = '' OR Status LIKE '%' + @StatusFilter + '%');

    SELECT *
    FROM #Filtered
    ORDER BY
        CASE WHEN @SortColumn = 'ReferenceNo' AND @SortDirection = 'ASC' THEN ReferenceNo END ASC,
        CASE WHEN @SortColumn = 'ReferenceNo' AND @SortDirection = 'DESC' THEN ReferenceNo END DESC,
        CASE WHEN @SortColumn = 'DateOfReport' AND @SortDirection = 'ASC' THEN DateOfReport END ASC,
        CASE WHEN @SortColumn = 'DateOfReport' AND @SortDirection = 'DESC' THEN DateOfReport END DESC,
        CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'ASC' THEN Status END ASC,
        CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'DESC' THEN Status END DESC,
        CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'ASC' THEN CreatedOn END ASC,
        CASE WHEN @SortColumn = 'CreatedOn' AND @SortDirection = 'DESC' THEN CreatedOn END DESC,
        GrievanceID DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(1) AS TotalCount FROM #Filtered;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_SaveComplaint
    @GrievanceID INT = 0,
    @ComplainantEmpId NVARCHAR(100),
    @ComplainantName NVARCHAR(200) = NULL,
    @Department NVARCHAR(200) = NULL,
    @PositionTitle NVARCHAR(200) = NULL,
    @DateOfReport DATETIME2(0) = NULL,
    @UnfairTreatment BIT = 0,
    @HarassmentBullying BIT = 0,
    @WorkLapses BIT = 0,
    @PolicySopBreach BIT = 0,
    @OshaConcern BIT = 0,
    @SupervisorMisconduct BIT = 0,
    @AbuseOfAuthority BIT = 0,
    @WorkingHoursIssue BIT = 0,
    @OtherComplaint BIT = 0,
    @OtherComplaintText NVARCHAR(500) = NULL,
    @ConductDate DATE = NULL,
    @ConductTime TIME(0) = NULL,
    @Location NVARCHAR(300) = NULL,
    @IncidentDescription NVARCHAR(MAX) = NULL,
    @Witnesses NVARCHAR(MAX) = NULL,
    @SupportingDocumentsAttached BIT = 0,
    @DesiredOutcome NVARCHAR(MAX) = NULL,
    @EmployeeSignaturePath NVARCHAR(1000) = NULL,
    @DeclarationEmployeeName NVARCHAR(200) = NULL,
    @DeclarationEmployeeId NVARCHAR(100) = NULL,
    @DeclarationDate DATETIME2(0) = NULL,
    @EmployeeCode NVARCHAR(100) = NULL,
    @GrievanceIDOut INT OUTPUT,
    @ReferenceNoOut NVARCHAR(30) OUTPUT,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF ISNULL(@GrievanceID, 0) = 0
        BEGIN
            DECLARE @ReferenceNo NVARCHAR(30) = dbo.fn_EGF_GenerateReferenceNo(CONVERT(DATE, ISNULL(@DateOfReport, GETDATE())));

            INSERT INTO dbo.EmployeeGrievanceComplaint
            (
                ReferenceNo, ComplainantEmpId, ComplainantName, Department, PositionTitle, DateOfReport,
                UnfairTreatment, HarassmentBullying, WorkLapses, PolicySopBreach, OshaConcern, SupervisorMisconduct,
                AbuseOfAuthority, WorkingHoursIssue, OtherComplaint, OtherComplaintText,
                ConductDate, ConductTime, Location, IncidentDescription, Witnesses, SupportingDocumentsAttached,
                DesiredOutcome, EmployeeSignaturePath, DeclarationEmployeeName, DeclarationEmployeeId, DeclarationDate,
                Status, CreatedBy, CreatedOn, isActive
            )
            VALUES
            (
                @ReferenceNo, @ComplainantEmpId, @ComplainantName, @Department, @PositionTitle, ISNULL(@DateOfReport, SYSUTCDATETIME()),
                @UnfairTreatment, @HarassmentBullying, @WorkLapses, @PolicySopBreach, @OshaConcern, @SupervisorMisconduct,
                @AbuseOfAuthority, @WorkingHoursIssue, @OtherComplaint, @OtherComplaintText,
                @ConductDate, @ConductTime, @Location, @IncidentDescription, @Witnesses, @SupportingDocumentsAttached,
                @DesiredOutcome, @EmployeeSignaturePath, @DeclarationEmployeeName, @DeclarationEmployeeId, ISNULL(@DeclarationDate, SYSUTCDATETIME()),
                'InProgress', @EmployeeCode, SYSUTCDATETIME(), 1
            );

            SET @GrievanceIDOut = SCOPE_IDENTITY();
            SET @ReferenceNoOut = @ReferenceNo;
            SET @Status = 1;
            SET @Message = 'Employee grievance form submitted successfully.';
        END
        ELSE
        BEGIN
            UPDATE dbo.EmployeeGrievanceComplaint
            SET
                ComplainantEmpId = @ComplainantEmpId,
                ComplainantName = @ComplainantName,
                Department = @Department,
                PositionTitle = @PositionTitle,
                UnfairTreatment = @UnfairTreatment,
                HarassmentBullying = @HarassmentBullying,
                WorkLapses = @WorkLapses,
                PolicySopBreach = @PolicySopBreach,
                OshaConcern = @OshaConcern,
                SupervisorMisconduct = @SupervisorMisconduct,
                AbuseOfAuthority = @AbuseOfAuthority,
                WorkingHoursIssue = @WorkingHoursIssue,
                OtherComplaint = @OtherComplaint,
                OtherComplaintText = @OtherComplaintText,
                ConductDate = @ConductDate,
                ConductTime = @ConductTime,
                Location = @Location,
                IncidentDescription = @IncidentDescription,
                Witnesses = @Witnesses,
                SupportingDocumentsAttached = @SupportingDocumentsAttached,
                DesiredOutcome = @DesiredOutcome,
                EmployeeSignaturePath = ISNULL(@EmployeeSignaturePath, EmployeeSignaturePath),
                DeclarationEmployeeName = @DeclarationEmployeeName,
                DeclarationEmployeeId = @DeclarationEmployeeId,
                DeclarationDate = ISNULL(@DeclarationDate, DeclarationDate),
                EditedBy = @EmployeeCode,
                EditedOn = SYSUTCDATETIME()
            WHERE GrievanceID = @GrievanceID;

            DELETE FROM dbo.EmployeeGrievanceInvolvedParty WHERE GrievanceID = @GrievanceID;

            SELECT @ReferenceNoOut = ReferenceNo FROM dbo.EmployeeGrievanceComplaint WHERE GrievanceID = @GrievanceID;
            SET @GrievanceIDOut = @GrievanceID;
            SET @Status = 1;
            SET @Message = 'Employee grievance form updated successfully.';
        END
    END TRY
    BEGIN CATCH
        SET @Status = 0;
        SET @Message = ERROR_MESSAGE();
        SET @GrievanceIDOut = ISNULL(@GrievanceID, 0);
        SET @ReferenceNoOut = '';
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_SaveInvolvedParty
    @GrievanceID INT,
    @EmployeeID NVARCHAR(100) = NULL,
    @EmployeeName NVARCHAR(200) = NULL,
    @PositionTitle NVARCHAR(200) = NULL,
    @Department NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.EmployeeGrievanceInvolvedParty (GrievanceID, EmployeeID, EmployeeName, PositionTitle, Department)
    VALUES (@GrievanceID, @EmployeeID, @EmployeeName, @PositionTitle, @Department);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_SaveAttachment
    @GrievanceID INT,
    @OriginalFileName NVARCHAR(260),
    @StoredFileName NVARCHAR(260),
    @FilePath NVARCHAR(1000),
    @ContentType NVARCHAR(200) = NULL,
    @SizeBytes BIGINT = NULL,
    @UploadedBy NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.EmployeeGrievanceAttachment (GrievanceID, OriginalFileName, StoredFileName, FilePath, ContentType, SizeBytes, UploadedBy)
    VALUES (@GrievanceID, @OriginalFileName, @StoredFileName, @FilePath, @ContentType, @SizeBytes, @UploadedBy);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetComplaintById
    @GrievanceID INT,
    @LoggedInEmpCode NVARCHAR(100),
    @IsHrUser BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        GrievanceID,
        ReferenceNo,
        ComplainantEmpId,
        ComplainantName,
        dbo.fn_EGF_FormatDepartment(Department) AS Department,
        PositionTitle,
        DateOfReport,
        UnfairTreatment,
        HarassmentBullying,
        WorkLapses,
        PolicySopBreach,
        OshaConcern,
        SupervisorMisconduct,
        AbuseOfAuthority,
        WorkingHoursIssue,
        OtherComplaint,
        OtherComplaintText,
        ConductDate,
        ConductTime,
        Location,
        IncidentDescription,
        Witnesses,
        SupportingDocumentsAttached,
        DesiredOutcome,
        EmployeeSignaturePath,
        DeclarationEmployeeName,
        DeclarationEmployeeId,
        DeclarationDate,
        Status,
        ISNULL(HRRemarks, '') AS HRRemarks,
        CreatedBy,
        CreatedOn,
        EditedBy,
        EditedOn,
        isActive
    FROM dbo.EmployeeGrievanceComplaint
    WHERE GrievanceID = @GrievanceID
      AND isActive = 1
      AND (@IsHrUser = 1 OR ComplainantEmpId = @LoggedInEmpCode);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetInvolvedParties
    @GrievanceID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PartyID, GrievanceID, EmployeeID, EmployeeName, PositionTitle, dbo.fn_EGF_FormatDepartment(Department) AS Department
    FROM dbo.EmployeeGrievanceInvolvedParty
    WHERE GrievanceID = @GrievanceID
    ORDER BY PartyID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetAttachments
    @GrievanceID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AttachmentID, GrievanceID, OriginalFileName, StoredFileName, FilePath, ContentType, SizeBytes, UploadedOn
    FROM dbo.EmployeeGrievanceAttachment
    WHERE GrievanceID = @GrievanceID
    ORDER BY AttachmentID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetHrAction
    @GrievanceID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        HRActionID,
        GrievanceID,
        HREmpId,
        HRName,
        ActionDate,
        ActionEmployeeId,
        ActionEmployeeName,
        InvestigationSummary,
        EmployeeExplanation,
        Remarks,
        OutcomeResolved,
        OutcomeReferredToER,
        OutcomeReferredToDomesticInquiry,
        MinorMisconduct,
        MajorMisconduct,
        MajorMisconductText,
        HRSignaturePath,
        Department
    FROM dbo.EmployeeGrievanceHRAction
    WHERE GrievanceID = @GrievanceID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_MarkViewedByHr
    @GrievanceID INT,
    @EmployeeCode NVARCHAR(100) = NULL,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.EmployeeGrievanceComplaint
        SET Status = 'Viewed by HR',
            EditedBy = @EmployeeCode,
            EditedOn = SYSUTCDATETIME()
        WHERE GrievanceID = @GrievanceID
          AND isActive = 1
          AND Status IN ('InProgress', 'Submitted');

        SET @Status = 1;
        SET @Message = 'Complaint status updated as Viewed by HR.';
    END TRY
    BEGIN CATCH
        SET @Status = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_UpdateHrRemarks
    @GrievanceID INT,
    @HRRemarks NVARCHAR(MAX) = NULL,
    @EmployeeCode NVARCHAR(100) = NULL,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE dbo.EmployeeGrievanceComplaint
        SET HRRemarks = @HRRemarks,
            EditedBy = @EmployeeCode,
            EditedOn = SYSUTCDATETIME()
        WHERE GrievanceID = @GrievanceID AND isActive = 1;

        SET @Status = 1;
        SET @Message = 'HR remarks updated successfully.';
    END TRY
    BEGIN CATCH
        SET @Status = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO


CREATE OR ALTER PROCEDURE dbo.usp_EGF_GetHrActionEmployees
    @GrievanceID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        HRActionEmployeeID,
        HRActionID,
        GrievanceID,
        EmployeeID,
        EmployeeName,
        PositionTitle,
        Department
    FROM dbo.EmployeeGrievanceHRActionEmployee
    WHERE GrievanceID = @GrievanceID
    ORDER BY HRActionEmployeeID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EGF_SaveHrAction
    @GrievanceID INT,
    @HREmpId NVARCHAR(100) = NULL,
    @HRName NVARCHAR(200) = NULL,
    @ActionEmployeeId NVARCHAR(100) = NULL,
    @ActionEmployeeName NVARCHAR(200) = NULL,
    @HrActionEmployeesJson NVARCHAR(MAX) = NULL,
    @InvestigationSummary NVARCHAR(MAX) = NULL,
    @EmployeeExplanation NVARCHAR(MAX) = NULL,
    @Remarks NVARCHAR(MAX) = NULL,
    @OutcomeResolved BIT = 0,
    @OutcomeReferredToER BIT = 0,
    @OutcomeReferredToDomesticInquiry BIT = 0,
    @MinorMisconduct BIT = 0,
    @MajorMisconduct BIT = 0,
    @MajorMisconductText NVARCHAR(500) = NULL,
    @HRSignaturePath NVARCHAR(1000) = NULL,
    @Department NVARCHAR(200) = 'HUMAN RESOURCE',
    @EmployeeCode NVARCHAR(100) = NULL,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @HRActionID INT;

        IF EXISTS (SELECT 1 FROM dbo.EmployeeGrievanceHRAction WHERE GrievanceID = @GrievanceID)
        BEGIN
            UPDATE dbo.EmployeeGrievanceHRAction
            SET HREmpId = @HREmpId,
                HRName = @HRName,
                ActionEmployeeId = @ActionEmployeeId,
                ActionEmployeeName = @ActionEmployeeName,
                InvestigationSummary = @InvestigationSummary,
                EmployeeExplanation = @EmployeeExplanation,
                Remarks = @Remarks,
                OutcomeResolved = @OutcomeResolved,
                OutcomeReferredToER = @OutcomeReferredToER,
                OutcomeReferredToDomesticInquiry = @OutcomeReferredToDomesticInquiry,
                MinorMisconduct = @MinorMisconduct,
                MajorMisconduct = @MajorMisconduct,
                MajorMisconductText = @MajorMisconductText,
                HRSignaturePath = ISNULL(@HRSignaturePath, HRSignaturePath),
                Department = @Department,
                EditedBy = @EmployeeCode,
                EditedOn = SYSUTCDATETIME()
            WHERE GrievanceID = @GrievanceID;

            SELECT @HRActionID = HRActionID
            FROM dbo.EmployeeGrievanceHRAction
            WHERE GrievanceID = @GrievanceID;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.EmployeeGrievanceHRAction
            (GrievanceID, HREmpId, HRName, ActionEmployeeId, ActionEmployeeName, InvestigationSummary, EmployeeExplanation, Remarks, OutcomeResolved, OutcomeReferredToER, OutcomeReferredToDomesticInquiry, MinorMisconduct, MajorMisconduct, MajorMisconductText, HRSignaturePath, Department, CreatedBy)
            VALUES
            (@GrievanceID, @HREmpId, @HRName, @ActionEmployeeId, @ActionEmployeeName, @InvestigationSummary, @EmployeeExplanation, @Remarks, @OutcomeResolved, @OutcomeReferredToER, @OutcomeReferredToDomesticInquiry, @MinorMisconduct, @MajorMisconduct, @MajorMisconductText, @HRSignaturePath, @Department, @EmployeeCode);

            SET @HRActionID = SCOPE_IDENTITY();
        END

        DELETE FROM dbo.EmployeeGrievanceHRActionEmployee
        WHERE GrievanceID = @GrievanceID;

        IF ISJSON(@HrActionEmployeesJson) = 1
        BEGIN
            INSERT INTO dbo.EmployeeGrievanceHRActionEmployee
            (HRActionID, GrievanceID, EmployeeID, EmployeeName, PositionTitle, Department)
            SELECT
                @HRActionID,
                @GrievanceID,
                EmployeeID,
                EmployeeName,
                PositionTitle,
                Department
            FROM OPENJSON(@HrActionEmployeesJson)
            WITH
            (
                EmployeeID NVARCHAR(100) '$.employeeID',
                EmployeeName NVARCHAR(200) '$.employeeName',
                PositionTitle NVARCHAR(200) '$.positionTitle',
                Department NVARCHAR(200) '$.department'
            ) J
            WHERE NULLIF(LTRIM(RTRIM(EmployeeID)), '') IS NOT NULL;
        END
        ELSE IF NULLIF(LTRIM(RTRIM(ISNULL(@ActionEmployeeId, ''))), '') IS NOT NULL
        BEGIN
            INSERT INTO dbo.EmployeeGrievanceHRActionEmployee
            (HRActionID, GrievanceID, EmployeeID, EmployeeName, PositionTitle, Department)
            VALUES
            (@HRActionID, @GrievanceID, @ActionEmployeeId, @ActionEmployeeName, NULL, NULL);
        END

        UPDATE dbo.EmployeeGrievanceComplaint
        SET Status = 'Completed',
            HRRemarks = @Remarks,
            EditedBy = @EmployeeCode,
            EditedOn = SYSUTCDATETIME()
        WHERE GrievanceID = @GrievanceID AND isActive = 1;

        SET @Status = 1;
        SET @Message = 'HR action submitted successfully.';
    END TRY
    BEGIN CATCH
        SET @Status = 0;
        SET @Message = ERROR_MESSAGE();
    END CATCH
END
GO
