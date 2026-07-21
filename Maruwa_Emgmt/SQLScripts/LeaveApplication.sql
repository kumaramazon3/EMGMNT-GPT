/*
    Leave Application / Leave Self Status module.
    Execute manually in SQL Server database configured by EHRMConnection.
    All screen data is accessed through stored procedures/functions only.
*/

IF OBJECT_ID('dbo.LeaveHalfDayTimeMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveHalfDayTimeMaster
    (
        HalfDayLeaveID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveHalfDayTimeMaster PRIMARY KEY,
        TimeText NVARCHAR(100) NOT NULL,
        SortOrder INT NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_LeaveHalfDayTimeMaster_CreatedOn DEFAULT SYSUTCDATETIME(),
        isActive BIT NOT NULL CONSTRAINT DF_LeaveHalfDayTimeMaster_isActive DEFAULT 1
    );
END
GO

MERGE dbo.LeaveHalfDayTimeMaster AS target
USING (VALUES
    (1, N'8.00 am – 12.35 pm'),
    (2, N'12.35 pm – 5.35 pm'),
    (3, N'7.30 am – 11.30 am'),
    (4, N'11.30 am – 3.00 pm'),
    (5, N'11.30 am – 3.00 pm'),
    (6, N'3.00 pm – 7.00 pm'),
    (7, N'3.00 pm – 7.00 pm'),
    (8, N'7.00 pm – 10.30 pm'),
    (9, N'7.30 am – 1.30 pm'),
    (10, N'1.30 pm – 7.30 pm')
) AS source (SortOrder, TimeText)
ON target.SortOrder = source.SortOrder
WHEN MATCHED THEN UPDATE SET TimeText = source.TimeText, isActive = 1
WHEN NOT MATCHED THEN INSERT (TimeText, SortOrder, CreatedBy, CreatedOn, isActive)
VALUES (source.TimeText, source.SortOrder, 'SYSTEM', SYSUTCDATETIME(), 1);
GO

IF OBJECT_ID('dbo.LeaveAnnualEntitlementMatrix', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveAnnualEntitlementMatrix
    (
        MatrixID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveAnnualEntitlementMatrix PRIMARY KEY,
        DesignationGroup NVARCHAR(150) NOT NULL,
        MinYears DECIMAL(5,2) NOT NULL,
        MaxYears DECIMAL(5,2) NULL,
        AnnualDays DECIMAL(5,2) NOT NULL,
        SortOrder INT NOT NULL,
        isActive BIT NOT NULL CONSTRAINT DF_LeaveAnnualEntitlementMatrix_isActive DEFAULT 1
    );
END
GO

MERGE dbo.LeaveAnnualEntitlementMatrix AS target
USING (VALUES
    (1, N'LINE_OPERATOR_TECHNICIAN', 0, 1.99, 8),
    (2, N'LINE_OPERATOR_TECHNICIAN', 2, 5.99, 12),
    (3, N'LINE_OPERATOR_TECHNICIAN', 6, NULL, 16),
    (4, N'ASST_ENGINEER_OFFICER_SUPERVISOR', 0, 1.99, 10),
    (5, N'ASST_ENGINEER_OFFICER_SUPERVISOR', 2, 5.99, 14),
    (6, N'ASST_ENGINEER_OFFICER_SUPERVISOR', 6, NULL, 18),
    (7, N'ENGINEER_SENIOR', 0, 1.99, 12),
    (8, N'ENGINEER_SENIOR', 2, 5.99, 16),
    (9, N'ENGINEER_SENIOR', 6, NULL, 20),
    (10, N'MANAGEMENT', 0, 1.99, 16),
    (11, N'MANAGEMENT', 2, 5.99, 20),
    (12, N'MANAGEMENT', 6, NULL, 24)
) AS source (SortOrder, DesignationGroup, MinYears, MaxYears, AnnualDays)
ON target.SortOrder = source.SortOrder
WHEN MATCHED THEN UPDATE SET DesignationGroup = source.DesignationGroup, MinYears = source.MinYears, MaxYears = source.MaxYears, AnnualDays = source.AnnualDays, isActive = 1
WHEN NOT MATCHED THEN INSERT (DesignationGroup, MinYears, MaxYears, AnnualDays, SortOrder, isActive)
VALUES (source.DesignationGroup, source.MinYears, source.MaxYears, source.AnnualDays, source.SortOrder, 1);
GO

IF OBJECT_ID('dbo.EmployeeLeaveApplication', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeLeaveApplication
    (
        AppNo INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeLeaveApplication PRIMARY KEY,
        EmpCode NVARCHAR(100) NOT NULL,
        ApplicationDate DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_ApplicationDate DEFAULT SYSUTCDATETIME(),
        FromDate DATE NOT NULL,
        ToDate DATE NOT NULL,
        LeaveDays DECIMAL(5,2) NOT NULL,
        LeaveTypeID NVARCHAR(50) NOT NULL,
        LeaveTypeName NVARCHAR(100) NOT NULL,
        ReasonID NVARCHAR(50) NULL,
        ReasonText NVARCHAR(500) NOT NULL,
        IsHalfDay BIT NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_IsHalfDay DEFAULT 0,
        HalfDayLeaveID INT NULL,
        HalfDayLeaveText NVARCHAR(100) NULL,
        BackDate BIT NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_BackDate DEFAULT 0,
        CarryForward BIT NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_CarryForward DEFAULT 0,
        Nocf DECIMAL(5,2) NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_Nocf DEFAULT 0,
        AnnualBalanceBefore DECIMAL(5,2) NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_AnnualBalanceBefore DEFAULT 0,
        MedicalBalanceBefore DECIMAL(5,2) NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_MedicalBalanceBefore DEFAULT 0,
        Status NVARCHAR(30) NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_Status DEFAULT N'Scheduled',
        StatusReason NVARCHAR(500) NULL,
        ApprovedBy NVARCHAR(100) NULL,
        ApprovedDate DATETIME2(0) NULL,
        CreatedBy NVARCHAR(100) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_CreatedOn DEFAULT SYSUTCDATETIME(),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedOn DATETIME2(0) NULL,
        isActive BIT NOT NULL CONSTRAINT DF_EmployeeLeaveApplication_isActive DEFAULT 1,
        CONSTRAINT FK_EmployeeLeaveApplication_HalfDay FOREIGN KEY (HalfDayLeaveID) REFERENCES dbo.LeaveHalfDayTimeMaster(HalfDayLeaveID)
    );
END
GO

IF OBJECT_ID('dbo.EmployeeLeaveApplicationPersonInCharge', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeLeaveApplicationPersonInCharge
    (
        LeavePICID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeLeaveApplicationPersonInCharge PRIMARY KEY,
        AppNo INT NOT NULL,
        PICOrder INT NOT NULL,
        EmpCode NVARCHAR(100) NOT NULL,
        EmpName NVARCHAR(200) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeLeaveApplicationPersonInCharge_CreatedOn DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_EmployeeLeaveApplicationPersonInCharge_AppNo FOREIGN KEY (AppNo) REFERENCES dbo.EmployeeLeaveApplication(AppNo)
    );
END
GO

IF OBJECT_ID('dbo.EmployeeLeaveApplicationStatusHistory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeLeaveApplicationStatusHistory
    (
        HistoryID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EmployeeLeaveApplicationStatusHistory PRIMARY KEY,
        AppNo INT NOT NULL,
        Status NVARCHAR(30) NOT NULL,
        Remarks NVARCHAR(500) NULL,
        CreatedBy NVARCHAR(100) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_EmployeeLeaveApplicationStatusHistory_CreatedOn DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_EmployeeLeaveApplicationStatusHistory_AppNo FOREIGN KEY (AppNo) REFERENCES dbo.EmployeeLeaveApplication(AppNo)
    );
END
GO

CREATE OR ALTER FUNCTION dbo.ufn_Leave_GetDesignationGroup(@Designation NVARCHAR(200))
RETURNS NVARCHAR(150)
AS
BEGIN
    DECLARE @d NVARCHAR(200) = UPPER(ISNULL(@Designation, ''));
    DECLARE @group NVARCHAR(150) = N'MANAGEMENT';

    IF @d LIKE '%OPERATOR%' OR @d LIKE '%LINE LEADER%' OR @d LIKE '%TECHNICIAN%'
        SET @group = N'LINE_OPERATOR_TECHNICIAN';
    ELSE IF @d LIKE '%ASST%' OR @d LIKE '%ASSISTANT%' OR @d LIKE '%OFFICER%' OR @d LIKE '%SUPERVISOR%'
        SET @group = N'ASST_ENGINEER_OFFICER_SUPERVISOR';
    ELSE IF @d LIKE '%ENGINEER%' OR @d LIKE '%SENIOR%'
        SET @group = N'ENGINEER_SENIOR';

    RETURN @group;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetLeaveTypes
AS
BEGIN
    SET NOCOUNT ON;
    SELECT LeaveID, LeaveType,
           CASE
               WHEN LeaveType IN (N'Annual', N'Emergency- Annual', N'Company Holiday') THEN N'Deduction from AL entitlement'
               WHEN LeaveType = N'Medical' THEN N'Deduct from MC entitlement'
               ELSE N'Do not deduct from AL entitlement'
           END AS Remark
    FROM dbo.LeaveTypeMaster
    WHERE isActive = 1
    ORDER BY TRY_CONVERT(INT, LeaveID), LeaveType;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetHalfDayLeaves
AS
BEGIN
    SET NOCOUNT ON;
    SELECT HalfDayLeaveID, TimeText
    FROM dbo.LeaveHalfDayTimeMaster
    WHERE isActive = 1
    ORDER BY SortOrder, HalfDayLeaveID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetReasons
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ReasonID, ReasonDescription
    FROM dbo.ReasonTypeMaster
    WHERE isActive = 1
    ORDER BY TRY_CONVERT(INT, ReasonID), ReasonDescription;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_SearchEmployees
    @SearchText NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (100)
           empCode AS EmpCode,
           CONCAT(empCode, N' - ', empName) AS EmpDisplay
    FROM dbo.empMaster
    WHERE (ISNULL(isResigned, '') NOT IN ('Y', 'Yes', 'YES', '1'))
      AND (@SearchText IS NULL OR @SearchText = '' OR empCode LIKE '%' + @SearchText + '%' OR empName LIKE '%' + @SearchText + '%')
    ORDER BY empName, empCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetEmployeeSummary
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @today DATE = CAST(GETDATE() AS DATE);
    DECLARE @yearStart DATE = DATEFROMPARTS(YEAR(@today), 1, 1);
    DECLARE @yearEnd DATE = DATEFROMPARTS(YEAR(@today), 12, 31);
    DECLARE @empName NVARCHAR(200), @designation NVARCHAR(200), @department NVARCHAR(200), @subDepartment NVARCHAR(200), @section NVARCHAR(200), @doj DATE;

    SELECT TOP 1
           @empName = empName,
           @designation = designation,
           @department = department,
           @subDepartment = subDepartment,
           @section = section,
           @doj = CAST(dateOfJoin AS DATE)
    FROM dbo.empMaster
    WHERE empCode = @EmpCode;

    DECLARE @serviceYears DECIMAL(10,2) = CASE WHEN @doj IS NULL THEN 0 ELSE DATEDIFF(DAY, @doj, @today) / 365.0 END;
    DECLARE @designationGroup NVARCHAR(150) = dbo.ufn_Leave_GetDesignationGroup(@designation);
    DECLARE @annualEntitlement DECIMAL(5,2) = 0;

    SELECT TOP 1 @annualEntitlement = AnnualDays
    FROM dbo.LeaveAnnualEntitlementMatrix
    WHERE isActive = 1
      AND DesignationGroup = @designationGroup
      AND @serviceYears >= MinYears
      AND (@serviceYears <= MaxYears OR MaxYears IS NULL)
    ORDER BY MinYears DESC;

    IF @doj IS NOT NULL AND YEAR(@doj) = YEAR(@today)
        SET @annualEntitlement = 0;

    DECLARE @medicalEntitlement DECIMAL(5,2) = 14;
    DECLARE @annualUtilised DECIMAL(5,2) = 0;
    DECLARE @medicalUtilised DECIMAL(5,2) = 0;
    DECLARE @carryForwardTotal DECIMAL(5,2) = 0;
    DECLARE @carryForwardUtilised DECIMAL(5,2) = 0;

    SELECT @annualUtilised = ISNULL(SUM(LeaveDays - ISNULL(Nocf,0)), 0),
           @carryForwardUtilised = ISNULL(SUM(ISNULL(Nocf,0)), 0)
    FROM dbo.EmployeeLeaveApplication
    WHERE EmpCode = @EmpCode
      AND FromDate BETWEEN @yearStart AND @yearEnd
      AND Status NOT IN (N'Rejected', N'CANCELLED')
      AND LeaveTypeName IN (N'Annual', N'Emergency- Annual', N'Company Holiday');

    SELECT @medicalUtilised = ISNULL(SUM(LeaveDays), 0)
    FROM dbo.EmployeeLeaveApplication
    WHERE EmpCode = @EmpCode
      AND FromDate BETWEEN @yearStart AND @yearEnd
      AND Status NOT IN (N'Rejected', N'CANCELLED')
      AND LeaveTypeName = N'Medical';

    SELECT @EmpCode AS EmpCode,
           ISNULL(@empName, '') AS EmpName,
           ISNULL(@designation, '') AS Designation,
           ISNULL(@department, '') AS Department,
           ISNULL(@subDepartment, '') AS SubDepartment,
           ISNULL(@section, '') AS Section,
           @doj AS DateOfJoin,
           @carryForwardTotal AS CarryForwardTotal,
           @carryForwardUtilised AS CarryForwardUtilised,
           CASE WHEN @carryForwardTotal - @carryForwardUtilised < 0 THEN 0 ELSE @carryForwardTotal - @carryForwardUtilised END AS CarryForwardBalance,
           @annualEntitlement AS AnnualEntitlement,
           @annualUtilised AS AnnualUtilised,
           CASE WHEN @annualEntitlement - @annualUtilised < 0 THEN 0 ELSE @annualEntitlement - @annualUtilised END AS AnnualBalance,
           @medicalEntitlement AS MedicalEntitlement,
           @medicalUtilised AS MedicalUtilised,
           CASE WHEN @medicalEntitlement - @medicalUtilised < 0 THEN 0 ELSE @medicalEntitlement - @medicalUtilised END AS MedicalBalance,
           (CASE WHEN @carryForwardTotal - @carryForwardUtilised < 0 THEN 0 ELSE @carryForwardTotal - @carryForwardUtilised END) +
           (CASE WHEN @annualEntitlement - @annualUtilised < 0 THEN 0 ELSE @annualEntitlement - @annualUtilised END) AS TotalEntitlementBalance;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetApplicationPageData
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.usp_Leave_GetEmployeeSummary @EmpCode = @EmpCode;
    EXEC dbo.usp_Leave_GetLeaveTypes;
    EXEC dbo.usp_Leave_GetHalfDayLeaves;
    EXEC dbo.usp_Leave_GetReasons;
    EXEC dbo.usp_Leave_SearchEmployees @SearchText = NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_Apply
    @EmpCode NVARCHAR(100),
    @LeaveTypeID NVARCHAR(50),
    @LeaveTypeName NVARCHAR(100),
    @FromDate DATE,
    @ToDate DATE,
    @LeaveDays DECIMAL(5,2),
    @IsHalfDay BIT,
    @HalfDayLeaveID INT = NULL,
    @HalfDayLeaveText NVARCHAR(100) = NULL,
    @ReasonID NVARCHAR(50) = NULL,
    @ReasonText NVARCHAR(500),
    @PersonInCharge1 NVARCHAR(100),
    @PersonInCharge1Name NVARCHAR(200),
    @PersonInCharge2 NVARCHAR(100) = NULL,
    @PersonInCharge2Name NVARCHAR(200) = NULL,
    @AnnualBalance DECIMAL(5,2) = 0,
    @MedicalBalance DECIMAL(5,2) = 0,
    @CarryForwardBalance DECIMAL(5,2) = 0,
    @CreatedBy NVARCHAR(100),
    @AppNo INT OUTPUT,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Status = 0;
    SET @Message = N'';
    SET @AppNo = 0;

    IF @LeaveTypeID IS NULL OR @LeaveTypeID = '' BEGIN SET @Message = N'Please select Leave Type.'; RETURN; END
    IF @FromDate IS NULL OR @ToDate IS NULL BEGIN SET @Message = N'Please select Leave Period.'; RETURN; END
    IF @FromDate > @ToDate BEGIN SET @Message = N'Please check the selected Leave Period dates.'; RETURN; END
    IF @LeaveDays < 0.5 BEGIN SET @Message = N'Leave should not apply below half day.'; RETURN; END
    IF @IsHalfDay = 1 AND ISNULL(@HalfDayLeaveID,0) <= 0 BEGIN SET @Message = N'Please select Half-Day Leave time.'; RETURN; END
    IF @ReasonID IS NULL OR @ReasonID = '' BEGIN SET @Message = N'Please select Reason.'; RETURN; END
    IF @PersonInCharge1 IS NULL OR @PersonInCharge1 = '' BEGIN SET @Message = N'Please select Person in Charge 1.'; RETURN; END
    IF @PersonInCharge1 = ISNULL(@PersonInCharge2, '') BEGIN SET @Message = N'Person in Charge 1 and Person in Charge 2 should not be same.'; RETURN; END

    DECLARE @actualDays DECIMAL(5,2) = DATEDIFF(DAY, @FromDate, @ToDate) + 1;
    IF @IsHalfDay = 1 SET @actualDays = 0.5;
    IF @actualDays <> @LeaveDays BEGIN SET @Message = N'Please check Leave Days against selected dates.'; RETURN; END

    DECLARE @today DATE = CAST(GETDATE() AS DATE);
    DECLARE @timeline INT = DATEDIFF(DAY, @today, @FromDate);
    DECLARE @backDate BIT = CASE WHEN @FromDate < @today THEN 1 ELSE 0 END;

    IF @LeaveTypeName IS NULL OR @LeaveTypeName = ''
        SELECT @LeaveTypeName = LeaveType FROM dbo.LeaveTypeMaster WHERE LeaveID = @LeaveTypeID;

    IF @ReasonText IS NULL OR @ReasonText = ''
        SELECT @ReasonText = ReasonDescription FROM dbo.ReasonTypeMaster WHERE ReasonID = @ReasonID;

    IF @IsHalfDay = 0 SET @HalfDayLeaveText = NULL;

    IF @LeaveTypeName = N'Annual'
    BEGIN
        IF @LeaveDays > (@AnnualBalance + @CarryForwardBalance) BEGIN SET @Message = N'No.of Days applied is greater than total entitlement balance.'; RETURN; END
        IF @LeaveDays = 0.5 AND @timeline < 1 BEGIN SET @Message = N'Leave should apply before one day.'; RETURN; END
        IF @LeaveDays = 1 AND @timeline < 3 BEGIN SET @Message = N'Leave should apply before three day.'; RETURN; END
        IF @LeaveDays > 1 AND @timeline < 7 BEGIN SET @Message = N'Leave should apply before seven day.'; RETURN; END
    END

    IF @LeaveTypeName = N'Medical' AND @LeaveDays > @MedicalBalance
    BEGIN
        SET @Message = N'Cannot Apply!! Leave Applied is more than available Medical Leave.';
        RETURN;
    END

    DECLARE @nocf DECIMAL(5,2) = 0;
    DECLARE @carryForward BIT = 0;
    IF @LeaveTypeName IN (N'Annual', N'Emergency- Annual', N'Company Holiday') AND @CarryForwardBalance > 0
    BEGIN
        SET @carryForward = 1;
        SET @nocf = CASE WHEN @LeaveDays >= @CarryForwardBalance THEN @CarryForwardBalance ELSE @LeaveDays END;
    END

    BEGIN TRANSACTION;
        INSERT INTO dbo.EmployeeLeaveApplication
        (
            EmpCode, ApplicationDate, FromDate, ToDate, LeaveDays, LeaveTypeID, LeaveTypeName,
            ReasonID, ReasonText, IsHalfDay, HalfDayLeaveID, HalfDayLeaveText, BackDate,
            CarryForward, Nocf, AnnualBalanceBefore, MedicalBalanceBefore, Status, CreatedBy, CreatedOn, isActive
        )
        VALUES
        (
            @EmpCode, SYSUTCDATETIME(), @FromDate, @ToDate, @LeaveDays, @LeaveTypeID, @LeaveTypeName,
            @ReasonID, @ReasonText, @IsHalfDay, @HalfDayLeaveID, @HalfDayLeaveText, @backDate,
            @carryForward, @nocf, @AnnualBalance, @MedicalBalance, N'Scheduled', @CreatedBy, SYSUTCDATETIME(), 1
        );

        SET @AppNo = SCOPE_IDENTITY();

        INSERT INTO dbo.EmployeeLeaveApplicationPersonInCharge (AppNo, PICOrder, EmpCode, EmpName)
        VALUES (@AppNo, 1, @PersonInCharge1, @PersonInCharge1Name);

        IF @PersonInCharge2 IS NOT NULL AND @PersonInCharge2 <> ''
        BEGIN
            INSERT INTO dbo.EmployeeLeaveApplicationPersonInCharge (AppNo, PICOrder, EmpCode, EmpName)
            VALUES (@AppNo, 2, @PersonInCharge2, @PersonInCharge2Name);
        END

        INSERT INTO dbo.EmployeeLeaveApplicationStatusHistory (AppNo, Status, Remarks, CreatedBy)
        VALUES (@AppNo, N'Scheduled', N'Leave application submitted', @CreatedBy);
    COMMIT TRANSACTION;

    SET @Status = 1;
    SET @Message = CONCAT(N'Leave has been scheduled successfully. App No: ', @AppNo);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetSelfStatus
    @EmpCode NVARCHAR(100),
    @GlobalSearch NVARCHAR(200) = NULL,
    @AppNo NVARCHAR(50) = NULL,
    @LeaveType NVARCHAR(100) = NULL,
    @Status NVARCHAR(50) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SET @PageNumber = CASE WHEN ISNULL(@PageNumber,0) <= 0 THEN 1 ELSE @PageNumber END;
    SET @PageSize = CASE WHEN ISNULL(@PageSize,0) <= 0 THEN 10 ELSE @PageSize END;

    SELECT a.AppNo, a.ApplicationDate, a.LeaveDays, a.FromDate, a.ToDate, a.LeaveTypeName, a.ReasonText,
           a.Status, a.StatusReason, a.ApprovedBy, a.ApprovedDate, a.HalfDayLeaveText,
           MAX(CASE WHEN p.PICOrder = 1 THEN p.EmpName END) AS PersonInCharge1Name,
           MAX(CASE WHEN p.PICOrder = 2 THEN p.EmpName END) AS PersonInCharge2Name
    INTO #FilteredLeaveStatus
    FROM dbo.EmployeeLeaveApplication a
    LEFT JOIN dbo.EmployeeLeaveApplicationPersonInCharge p ON p.AppNo = a.AppNo
    WHERE a.EmpCode = @EmpCode
      AND a.isActive = 1
      AND (@GlobalSearch IS NULL OR @GlobalSearch = '' OR
           CONVERT(NVARCHAR(50), a.AppNo) LIKE '%' + @GlobalSearch + '%' OR
           a.LeaveTypeName LIKE '%' + @GlobalSearch + '%' OR
           a.ReasonText LIKE '%' + @GlobalSearch + '%' OR
           a.Status LIKE '%' + @GlobalSearch + '%')
      AND (@AppNo IS NULL OR @AppNo = '' OR CONVERT(NVARCHAR(50), a.AppNo) LIKE '%' + @AppNo + '%')
      AND (@LeaveType IS NULL OR @LeaveType = '' OR a.LeaveTypeName LIKE '%' + @LeaveType + '%')
      AND (@Status IS NULL OR @Status = '' OR a.Status LIKE '%' + @Status + '%')
    GROUP BY a.AppNo, a.ApplicationDate, a.LeaveDays, a.FromDate, a.ToDate, a.LeaveTypeName, a.ReasonText,
             a.Status, a.StatusReason, a.ApprovedBy, a.ApprovedDate, a.HalfDayLeaveText;

    ;WITH Ordered AS
    (
        SELECT *, ROW_NUMBER() OVER (ORDER BY AppNo DESC) AS RowNum
        FROM #FilteredLeaveStatus
    )
    SELECT AppNo, ApplicationDate, LeaveDays, FromDate, ToDate, LeaveTypeName, ReasonText, Status,
           StatusReason, ApprovedBy, ApprovedDate, HalfDayLeaveText, PersonInCharge1Name, PersonInCharge2Name
    FROM Ordered
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize)
    ORDER BY RowNum;

    SELECT COUNT(1) AS TotalCount FROM #FilteredLeaveStatus;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_Cancel
    @AppNo INT,
    @EmpCode NVARCHAR(100),
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Status = 0;

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeLeaveApplication WHERE AppNo = @AppNo AND EmpCode = @EmpCode AND isActive = 1)
    BEGIN
        SET @Message = N'Leave application not found.';
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeLeaveApplication WHERE AppNo = @AppNo AND EmpCode = @EmpCode AND Status = N'Scheduled')
    BEGIN
        SET @Message = N'Only Scheduled leave can be cancelled.';
        RETURN;
    END

    UPDATE dbo.EmployeeLeaveApplication
       SET Status = N'CANCELLED', StatusReason = N'Cancelled by employee', ModifiedBy = @EmpCode, ModifiedOn = SYSUTCDATETIME()
     WHERE AppNo = @AppNo AND EmpCode = @EmpCode;

    INSERT INTO dbo.EmployeeLeaveApplicationStatusHistory (AppNo, Status, Remarks, CreatedBy)
    VALUES (@AppNo, N'CANCELLED', N'Cancelled by employee', @EmpCode);

    SET @Status = 1;
    SET @Message = N'Leave cancelled successfully.';
END
GO

/* =============================================================
   Leave Management requirement update - dynamic rules / edit flow
   ============================================================= */
IF OBJECT_ID('dbo.LeaveCompanyHolidayMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveCompanyHolidayMaster
    (
        HolidayID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveCompanyHolidayMaster PRIMARY KEY,
        HolidayDate DATE NOT NULL CONSTRAINT UQ_LeaveCompanyHolidayMaster_HolidayDate UNIQUE,
        HolidayName NVARCHAR(200) NOT NULL,
        isActive BIT NOT NULL CONSTRAINT DF_LeaveCompanyHolidayMaster_isActive DEFAULT 1,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_LeaveCompanyHolidayMaster_CreatedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

IF OBJECT_ID('dbo.LeaveShiftManagement', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveShiftManagement
    (
        ShiftCode NVARCHAR(50) NOT NULL CONSTRAINT PK_LeaveShiftManagement PRIMARY KEY,
        ShiftName NVARCHAR(150) NOT NULL,
        WorkOnMonday BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_Mon DEFAULT 1,
        WorkOnTuesday BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_Tue DEFAULT 1,
        WorkOnWednesday BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_Wed DEFAULT 1,
        WorkOnThursday BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_Thu DEFAULT 1,
        WorkOnFriday BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_Fri DEFAULT 1,
        WorkOnSaturday BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_Sat DEFAULT 1,
        WorkOnSunday BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_Sun DEFAULT 0,
        IsDefault BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_IsDefault DEFAULT 0,
        isActive BIT NOT NULL CONSTRAINT DF_LeaveShiftManagement_isActive DEFAULT 1,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_LeaveShiftManagement_CreatedOn DEFAULT SYSUTCDATETIME()
    );
END
GO

MERGE dbo.LeaveShiftManagement AS target
USING (VALUES
    (N'NORMAL', N'Normal Shift - Monday to Saturday', 1,1,1,1,1,1,0,1),
    (N'SUNDAY_WORK', N'Sunday Working Shift', 1,1,1,1,1,1,1,0),
    (N'ALL_DAYS', N'All Days Working Shift', 1,1,1,1,1,1,1,0)
) AS source (ShiftCode, ShiftName, WorkOnMonday, WorkOnTuesday, WorkOnWednesday, WorkOnThursday, WorkOnFriday, WorkOnSaturday, WorkOnSunday, IsDefault)
ON target.ShiftCode = source.ShiftCode
WHEN MATCHED THEN UPDATE SET
    ShiftName = source.ShiftName,
    WorkOnMonday = source.WorkOnMonday,
    WorkOnTuesday = source.WorkOnTuesday,
    WorkOnWednesday = source.WorkOnWednesday,
    WorkOnThursday = source.WorkOnThursday,
    WorkOnFriday = source.WorkOnFriday,
    WorkOnSaturday = source.WorkOnSaturday,
    WorkOnSunday = source.WorkOnSunday,
    IsDefault = source.IsDefault,
    isActive = 1
WHEN NOT MATCHED THEN INSERT
    (ShiftCode, ShiftName, WorkOnMonday, WorkOnTuesday, WorkOnWednesday, WorkOnThursday, WorkOnFriday, WorkOnSaturday, WorkOnSunday, IsDefault, isActive, CreatedBy, CreatedOn)
VALUES
    (source.ShiftCode, source.ShiftName, source.WorkOnMonday, source.WorkOnTuesday, source.WorkOnWednesday, source.WorkOnThursday, source.WorkOnFriday, source.WorkOnSaturday, source.WorkOnSunday, source.IsDefault, 1, N'SYSTEM', SYSUTCDATETIME());
GO


IF OBJECT_ID('dbo.LeaveEmployeeShiftSchedule', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveEmployeeShiftSchedule
    (
        ShiftScheduleID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LeaveEmployeeShiftSchedule PRIMARY KEY,
        EmpCode NVARCHAR(100) NOT NULL,
        ShiftDate DATE NOT NULL,
        ShiftCode NVARCHAR(50) NULL,
        IsWorkingDay BIT NOT NULL,
        CreatedBy NVARCHAR(50) NULL,
        CreatedOn DATETIME2(0) NOT NULL CONSTRAINT DF_LeaveEmployeeShiftSchedule_CreatedOn DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_LeaveEmployeeShiftSchedule_EmpDate UNIQUE(EmpCode, ShiftDate)
    );
END
GO

CREATE OR ALTER FUNCTION dbo.ufn_Leave_IsWorkingDay
(
    @EmpCode NVARCHAR(100),
    @WorkDate DATE
)
RETURNS BIT
AS
BEGIN
    DECLARE @isWorking BIT = NULL;
    DECLARE @scheduledShiftCode NVARCHAR(50) = NULL;

    -- Employee shift schedule has first priority. This supports employees who work on Sundays
    -- or employees who are off on an otherwise normal working day.
    SELECT TOP 1
           @isWorking = IsWorkingDay,
           @scheduledShiftCode = ShiftCode
    FROM dbo.LeaveEmployeeShiftSchedule
    WHERE EmpCode = @EmpCode AND ShiftDate = @WorkDate;

    IF @isWorking IS NOT NULL RETURN @isWorking;

    -- Company holidays are excluded unless the employee shift schedule explicitly marks the day as working.
    IF EXISTS (SELECT 1 FROM dbo.LeaveCompanyHolidayMaster WHERE HolidayDate = @WorkDate AND isActive = 1)
        RETURN 0;

    -- Deterministic day number: 1 = Monday, 7 = Sunday.
    DECLARE @dayNo INT = (DATEDIFF(DAY, '19000101', @WorkDate) % 7) + 1;

    -- Default Shift Management rule. NORMAL shift is seeded as Monday-Saturday working, Sunday off.
    SELECT TOP 1 @isWorking =
        CASE @dayNo
            WHEN 1 THEN WorkOnMonday
            WHEN 2 THEN WorkOnTuesday
            WHEN 3 THEN WorkOnWednesday
            WHEN 4 THEN WorkOnThursday
            WHEN 5 THEN WorkOnFriday
            WHEN 6 THEN WorkOnSaturday
            WHEN 7 THEN WorkOnSunday
        END
    FROM dbo.LeaveShiftManagement
    WHERE isActive = 1 AND IsDefault = 1
    ORDER BY ShiftCode;

    RETURN ISNULL(@isWorking, CASE WHEN @dayNo = 7 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END);
END
GO

CREATE OR ALTER FUNCTION dbo.ufn_Leave_CalculateWorkingDays
(
    @EmpCode NVARCHAR(100),
    @FromDate DATE,
    @ToDate DATE,
    @IsHalfDay BIT
)
RETURNS DECIMAL(5,2)
AS
BEGIN
    DECLARE @days DECIMAL(5,2) = 0;
    DECLARE @d DATE = @FromDate;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate > @ToDate RETURN 0;
    IF @IsHalfDay = 1 RETURN 0.5;

    WHILE @d <= @ToDate
    BEGIN
        IF dbo.ufn_Leave_IsWorkingDay(@EmpCode, @d) = 1
            SET @days = @days + 1;
        SET @d = DATEADD(DAY, 1, @d);
    END

    RETURN @days;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_CalculateDays
    @EmpCode NVARCHAR(100),
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @IsHalfDay BIT = 0,
    @LeaveDays DECIMAL(5,2) OUTPUT,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Status = 0;
    SET @LeaveDays = 0;
    SET @Message = N'';

    IF @FromDate IS NULL OR @ToDate IS NULL BEGIN SET @Message = N'Please select Leave Period.'; RETURN; END
    IF @FromDate > @ToDate BEGIN SET @Message = N'Please check the selected Leave Period dates.'; RETURN; END
    IF @IsHalfDay = 1 AND @FromDate <> @ToDate BEGIN SET @Message = N'Half Day Leave Date cannot be more than one day.'; RETURN; END

    SET @LeaveDays = dbo.ufn_Leave_CalculateWorkingDays(@EmpCode, @FromDate, @ToDate, @IsHalfDay);
    IF @LeaveDays <= 0 BEGIN SET @Message = N'Selected leave period contains no working day based on holiday/shift schedule.'; RETURN; END

    SET @Status = 1;
    SET @Message = N'Leave days calculated.';
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetEmployeeSummary
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @today DATE = CAST(GETDATE() AS DATE);
    DECLARE @yearStart DATE = DATEFROMPARTS(YEAR(@today), 1, 1);
    DECLARE @yearEnd DATE = DATEFROMPARTS(YEAR(@today), 12, 31);
    DECLARE @empName NVARCHAR(200), @designation NVARCHAR(200), @department NVARCHAR(200), @subDepartment NVARCHAR(200), @section NVARCHAR(200), @doj DATE;

    SELECT TOP 1
           @empName = empName,
           @designation = designation,
           @department = department,
           @subDepartment = subDepartment,
           @section = section,
           @doj = CAST(dateOfJoin AS DATE)
    FROM dbo.empMaster
    WHERE empCode = @EmpCode;

    DECLARE @serviceYears DECIMAL(10,2) = CASE WHEN @doj IS NULL THEN 0 ELSE DATEDIFF(DAY, @doj, @today) / 365.0 END;
    DECLARE @designationGroup NVARCHAR(150) = dbo.ufn_Leave_GetDesignationGroup(@designation);
    DECLARE @annualEntitlement DECIMAL(5,2) = 0;

    SELECT TOP 1 @annualEntitlement = AnnualDays
    FROM dbo.LeaveAnnualEntitlementMatrix
    WHERE isActive = 1
      AND DesignationGroup = @designationGroup
      AND @serviceYears >= MinYears
      AND (@serviceYears <= MaxYears OR MaxYears IS NULL)
    ORDER BY MinYears DESC;

    IF @doj IS NOT NULL AND YEAR(@doj) = YEAR(@today)
        SET @annualEntitlement = 0;

    DECLARE @medicalEntitlement DECIMAL(5,2) = 14;
    DECLARE @annualUtilised DECIMAL(5,2) = 0;
    DECLARE @medicalUtilised DECIMAL(5,2) = 0;
    DECLARE @carryForwardTotal DECIMAL(5,2) = 0;
    DECLARE @carryForwardUtilised DECIMAL(5,2) = 0;

    SELECT @annualUtilised = ISNULL(SUM(LeaveDays - ISNULL(Nocf,0)), 0),
           @carryForwardUtilised = ISNULL(SUM(ISNULL(Nocf,0)), 0)
    FROM dbo.EmployeeLeaveApplication
    WHERE EmpCode = @EmpCode
      AND FromDate BETWEEN @yearStart AND @yearEnd
      AND Status NOT IN (N'Rejected', N'CANCELLED')
      AND LeaveTypeName IN (N'Annual', N'Emergency- Annual', N'Company Holiday');

    SELECT @medicalUtilised = ISNULL(SUM(LeaveDays), 0)
    FROM dbo.EmployeeLeaveApplication
    WHERE EmpCode = @EmpCode
      AND FromDate BETWEEN @yearStart AND @yearEnd
      AND Status NOT IN (N'Rejected', N'CANCELLED')
      AND LeaveTypeName = N'Medical';

    SELECT @EmpCode AS EmpCode,
           ISNULL(@empName, '') AS EmpName,
           ISNULL(@designation, '') AS Designation,
           ISNULL(@department, '') AS Department,
           ISNULL(@subDepartment, '') AS SubDepartment,
           ISNULL(@section, '') AS Section,
           @doj AS DateOfJoin,
           CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(@designation,'')))) LIKE '%operator%' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsOperator,
           @carryForwardTotal AS CarryForwardTotal,
           @carryForwardUtilised AS CarryForwardUtilised,
           CASE WHEN @carryForwardTotal - @carryForwardUtilised < 0 THEN 0 ELSE @carryForwardTotal - @carryForwardUtilised END AS CarryForwardBalance,
           @annualEntitlement AS AnnualEntitlement,
           @annualUtilised AS AnnualUtilised,
           CASE WHEN @annualEntitlement - @annualUtilised < 0 THEN 0 ELSE @annualEntitlement - @annualUtilised END AS AnnualBalance,
           @medicalEntitlement AS MedicalEntitlement,
           @medicalUtilised AS MedicalUtilised,
           CASE WHEN @medicalEntitlement - @medicalUtilised < 0 THEN 0 ELSE @medicalEntitlement - @medicalUtilised END AS MedicalBalance,
           (CASE WHEN @carryForwardTotal - @carryForwardUtilised < 0 THEN 0 ELSE @carryForwardTotal - @carryForwardUtilised END) +
           (CASE WHEN @annualEntitlement - @annualUtilised < 0 THEN 0 ELSE @annualEntitlement - @annualUtilised END) AS TotalEntitlementBalance;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_Apply
    @EmpCode NVARCHAR(100),
    @LeaveTypeID NVARCHAR(50),
    @LeaveTypeName NVARCHAR(100),
    @FromDate DATE,
    @ToDate DATE,
    @LeaveDays DECIMAL(5,2),
    @IsHalfDay BIT,
    @HalfDayLeaveID INT = NULL,
    @HalfDayLeaveText NVARCHAR(100) = NULL,
    @ReasonID NVARCHAR(50) = NULL,
    @ReasonText NVARCHAR(500),
    @PersonInCharge1 NVARCHAR(100) = NULL,
    @PersonInCharge1Name NVARCHAR(200) = NULL,
    @PersonInCharge2 NVARCHAR(100) = NULL,
    @PersonInCharge2Name NVARCHAR(200) = NULL,
    @AnnualBalance DECIMAL(5,2) = 0,
    @MedicalBalance DECIMAL(5,2) = 0,
    @CarryForwardBalance DECIMAL(5,2) = 0,
    @CreatedBy NVARCHAR(100),
    @AppNoToUpdate INT = NULL,
    @AppNo INT OUTPUT,
    @Status INT OUTPUT,
    @Message NVARCHAR(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Status = 0;
    SET @Message = N'';
    SET @AppNo = ISNULL(@AppNoToUpdate, 0);

    DECLARE @designation NVARCHAR(200) = (SELECT TOP 1 designation FROM dbo.empMaster WHERE empCode = @EmpCode);
    DECLARE @isOperator BIT = CASE WHEN LOWER(LTRIM(RTRIM(ISNULL(@designation,'')))) LIKE '%operator%' THEN 1 ELSE 0 END;

    IF @LeaveTypeID IS NULL OR @LeaveTypeID = '' BEGIN SET @Message = N'Please select Leave Type.'; RETURN; END
    IF @FromDate IS NULL OR @ToDate IS NULL BEGIN SET @Message = N'Please select Leave Period.'; RETURN; END
    IF @FromDate > @ToDate BEGIN SET @Message = N'Please check the selected Leave Period dates.'; RETURN; END
    IF @LeaveDays < 0.5 BEGIN SET @Message = N'Leave should not apply below half day.'; RETURN; END
    IF @IsHalfDay = 1 AND ISNULL(@HalfDayLeaveID,0) <= 0 BEGIN SET @Message = N'Please select Half-Day Leave time.'; RETURN; END
    IF @IsHalfDay = 1 AND @FromDate <> @ToDate BEGIN SET @Message = N'Half Day Leave Date cannot be more than one day.'; RETURN; END
    IF @isOperator = 0 AND (@PersonInCharge1 IS NULL OR @PersonInCharge1 = '') BEGIN SET @Message = N'Please select Person in Charge 1.'; RETURN; END
    IF @isOperator = 0 AND @PersonInCharge1 = ISNULL(@PersonInCharge2, '') BEGIN SET @Message = N'Person in Charge 1 and Person in Charge 2 should not be same.'; RETURN; END

    IF @LeaveTypeName IS NULL OR @LeaveTypeName = ''
        SELECT @LeaveTypeName = LeaveType FROM dbo.LeaveTypeMaster WHERE LeaveID = @LeaveTypeID;

    DECLARE @isMedical BIT = CASE WHEN LOWER(ISNULL(@LeaveTypeName,'')) LIKE '%medical%' THEN 1 ELSE 0 END;
    IF @isMedical = 1 AND (@ReasonID IS NULL OR @ReasonID = '') BEGIN SET @Message = N'Please select Reason.'; RETURN; END
    IF @isMedical = 0 AND LTRIM(RTRIM(ISNULL(@ReasonText,''))) = '' BEGIN SET @Message = N'Please enter Reason.'; RETURN; END

    IF @isMedical = 1 AND (@ReasonText IS NULL OR @ReasonText = '')
        SELECT @ReasonText = ReasonDescription FROM dbo.ReasonTypeMaster WHERE ReasonID = @ReasonID;

    IF @IsHalfDay = 0 SET @HalfDayLeaveText = NULL;

    DECLARE @actualDays DECIMAL(5,2) = dbo.ufn_Leave_CalculateWorkingDays(@EmpCode, @FromDate, @ToDate, @IsHalfDay);
    IF @actualDays <= 0 BEGIN SET @Message = N'Selected leave period contains no working day based on holiday/shift schedule.'; RETURN; END
    IF @actualDays <> @LeaveDays BEGIN SET @Message = N'Please check Leave Days against selected dates, holidays and shift schedule.'; RETURN; END

    DECLARE @today DATE = CAST(GETDATE() AS DATE);
    DECLARE @timeline INT = DATEDIFF(DAY, @today, @FromDate);
    DECLARE @backDate BIT = CASE WHEN @FromDate < @today THEN 1 ELSE 0 END;

    IF @LeaveTypeName = N'Annual'
    BEGIN
        IF @LeaveDays > (@AnnualBalance + @CarryForwardBalance) BEGIN SET @Message = N'No.of Days applied is greater than total entitlement balance.'; RETURN; END
        IF @LeaveDays = 0.5 AND @timeline < 1 BEGIN SET @Message = N'Leave should apply before one day.'; RETURN; END
        IF @LeaveDays = 1 AND @timeline < 3 BEGIN SET @Message = N'Leave should apply before three day.'; RETURN; END
        IF @LeaveDays > 1 AND @timeline < 7 BEGIN SET @Message = N'Leave should apply before seven day.'; RETURN; END
    END

    IF @LeaveTypeName = N'Medical' AND @LeaveDays > @MedicalBalance
    BEGIN
        SET @Message = N'Cannot Apply!! Leave Applied is more than available Medical Leave.';
        RETURN;
    END

    DECLARE @nocf DECIMAL(5,2) = 0;
    DECLARE @carryForward BIT = 0;
    IF @LeaveTypeName IN (N'Annual', N'Emergency- Annual', N'Company Holiday') AND @CarryForwardBalance > 0
    BEGIN
        SET @carryForward = 1;
        SET @nocf = CASE WHEN @LeaveDays >= @CarryForwardBalance THEN @CarryForwardBalance ELSE @LeaveDays END;
    END

    BEGIN TRANSACTION;
        IF ISNULL(@AppNoToUpdate,0) > 0
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeLeaveApplication WHERE AppNo = @AppNoToUpdate AND EmpCode = @EmpCode AND Status = N'Scheduled' AND isActive = 1)
            BEGIN
                ROLLBACK TRANSACTION;
                SET @Message = N'Only Scheduled leave applications can be edited.';
                RETURN;
            END

            UPDATE dbo.EmployeeLeaveApplication
               SET FromDate = @FromDate,
                   ToDate = @ToDate,
                   LeaveDays = @LeaveDays,
                   LeaveTypeID = @LeaveTypeID,
                   LeaveTypeName = @LeaveTypeName,
                   ReasonID = @ReasonID,
                   ReasonText = @ReasonText,
                   IsHalfDay = @IsHalfDay,
                   HalfDayLeaveID = @HalfDayLeaveID,
                   HalfDayLeaveText = @HalfDayLeaveText,
                   BackDate = @backDate,
                   CarryForward = @carryForward,
                   Nocf = @nocf,
                   AnnualBalanceBefore = @AnnualBalance,
                   MedicalBalanceBefore = @MedicalBalance,
                   ModifiedBy = @CreatedBy,
                   ModifiedOn = SYSUTCDATETIME()
             WHERE AppNo = @AppNoToUpdate AND EmpCode = @EmpCode;

            DELETE FROM dbo.EmployeeLeaveApplicationPersonInCharge WHERE AppNo = @AppNoToUpdate;
            SET @AppNo = @AppNoToUpdate;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.EmployeeLeaveApplication
            (
                EmpCode, ApplicationDate, FromDate, ToDate, LeaveDays, LeaveTypeID, LeaveTypeName,
                ReasonID, ReasonText, IsHalfDay, HalfDayLeaveID, HalfDayLeaveText, BackDate,
                CarryForward, Nocf, AnnualBalanceBefore, MedicalBalanceBefore, Status, CreatedBy, CreatedOn, isActive
            )
            VALUES
            (
                @EmpCode, SYSUTCDATETIME(), @FromDate, @ToDate, @LeaveDays, @LeaveTypeID, @LeaveTypeName,
                @ReasonID, @ReasonText, @IsHalfDay, @HalfDayLeaveID, @HalfDayLeaveText, @backDate,
                @carryForward, @nocf, @AnnualBalance, @MedicalBalance, N'Scheduled', @CreatedBy, SYSUTCDATETIME(), 1
            );
            SET @AppNo = SCOPE_IDENTITY();
        END

        IF @isOperator = 0
        BEGIN
            INSERT INTO dbo.EmployeeLeaveApplicationPersonInCharge (AppNo, PICOrder, EmpCode, EmpName)
            VALUES (@AppNo, 1, @PersonInCharge1, @PersonInCharge1Name);

            IF @PersonInCharge2 IS NOT NULL AND @PersonInCharge2 <> ''
            BEGIN
                INSERT INTO dbo.EmployeeLeaveApplicationPersonInCharge (AppNo, PICOrder, EmpCode, EmpName)
                VALUES (@AppNo, 2, @PersonInCharge2, @PersonInCharge2Name);
            END
        END

        INSERT INTO dbo.EmployeeLeaveApplicationStatusHistory (AppNo, Status, Remarks, CreatedBy)
        VALUES (@AppNo, N'Scheduled', CASE WHEN ISNULL(@AppNoToUpdate,0) > 0 THEN N'Leave application updated' ELSE N'Leave application submitted' END, @CreatedBy);
    COMMIT TRANSACTION;

    SET @Status = 1;
    SET @Message = CASE WHEN ISNULL(@AppNoToUpdate,0) > 0 THEN CONCAT(N'Leave application updated successfully. App No: ', @AppNo) ELSE CONCAT(N'Leave has been scheduled successfully. App No: ', @AppNo) END;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetForEdit
    @AppNo INT,
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.AppNo, a.LeaveTypeID, a.LeaveTypeName, a.FromDate, a.ToDate, a.LeaveDays, a.IsHalfDay,
           a.HalfDayLeaveID, a.HalfDayLeaveText, a.ReasonID, a.ReasonText, a.Status,
           MAX(CASE WHEN p.PICOrder = 1 THEN p.EmpCode END) AS PersonInCharge1,
           MAX(CASE WHEN p.PICOrder = 1 THEN p.EmpName END) AS PersonInCharge1Name,
           MAX(CASE WHEN p.PICOrder = 2 THEN p.EmpCode END) AS PersonInCharge2,
           MAX(CASE WHEN p.PICOrder = 2 THEN p.EmpName END) AS PersonInCharge2Name
    FROM dbo.EmployeeLeaveApplication a
    LEFT JOIN dbo.EmployeeLeaveApplicationPersonInCharge p ON p.AppNo = a.AppNo
    WHERE a.AppNo = @AppNo AND a.EmpCode = @EmpCode AND a.Status = N'Scheduled' AND a.isActive = 1
    GROUP BY a.AppNo, a.LeaveTypeID, a.LeaveTypeName, a.FromDate, a.ToDate, a.LeaveDays, a.IsHalfDay,
             a.HalfDayLeaveID, a.HalfDayLeaveText, a.ReasonID, a.ReasonText, a.Status;
END
GO
