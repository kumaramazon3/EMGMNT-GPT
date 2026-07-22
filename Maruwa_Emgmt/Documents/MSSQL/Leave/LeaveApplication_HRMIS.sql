/* =============================================================
   Leave Application - HRMIS database script
   -------------------------------------------------------------
   Execute this script in the HRMIS database configured by
   appsettings.json -> ConnectionStrings:LiveHRMISConnection.

   This script creates the legacy HRMIS stored procedures and
   helper stored procedures used by the MVC Leave Application DAL.
   ============================================================= */
GO


/* =============================================================
   SMS_LINK dependency safety fix
   -------------------------------------------------------------
   Some legacy HRMIS databases have INSERT/UPDATE triggers on dbo.leaveform
   that write SMS rows into sms.dbo.SMS_LINK. If the SMS database/object is
   not available in the current environment, Apply Leave fails with:
   Invalid object name 'sms.dbo.SMS_LINK'.

   This helper disables only leaveform triggers whose definition references
   sms.dbo.SMS_LINK. It does not disable other leaveform triggers.
   ============================================================= */
CREATE OR ALTER PROCEDURE dbo.usp_Leave_DisableSmsLinkTriggers
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX) = N'';

    SELECT @sql = @sql + N'DISABLE TRIGGER ' + QUOTENAME(t.name) + N' ON dbo.leaveform;' + CHAR(13) + CHAR(10)
    FROM sys.triggers t
    INNER JOIN sys.sql_modules m ON t.object_id = m.object_id
    WHERE t.parent_id = OBJECT_ID(N'dbo.leaveform')
      AND t.is_disabled = 0
      AND (
            m.definition LIKE N'%sms.dbo.SMS_LINK%'
         OR m.definition LIKE N'%[sms].[dbo].[SMS_LINK]%'
         OR m.definition LIKE N'%SMS_LINK%'
      );

    IF (@sql <> N'')
        EXEC sys.sp_executesql @sql;
END
GO

/* Source: hrmis_getleavelevel.sql */
/****** Object:  StoredProcedure [dbo].[HRMIS_getleavelevel]    Script Date: 07/21/2026 11:29:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[HRMIS_getleavelevel] 
@ecode varchar(10),
@exp INT
as
IF EXISTS (SELECT * FROM empmaster WHERE empcode=@ecode)
Begin
	if Year(GetDate())=2018
	Begin
		SELECT e.leavelevel,Convert(decimal(4,0),CEILING((l.annual*9/12))) as annual,l.probation,Convert(decimal(4,0),CEILING((l.medical*9/12))) as medical,l.experience, Convert(decimal(4,0),CEILING((l.annual*9/12))) as annual1 FROM empmaster e, leavelevel l 
		WHERE l.leavelevel=e.leavelevel and l.experience=@exp and e.empcode=@ecode
	End
	Else if Year(GetDate())=2019
	Begin
		SELECT e.leavelevel,Convert(decimal(4,0),CEILING((l.annual*9/12))) as annual,l.probation,Convert(decimal(4,0),CEILING((l.medical*9/12))) as medical,l.experience, Convert(decimal(4,0),CEILING((l.annual*9/12))) as annual1 FROM empmaster e, leavelevel l 
		WHERE l.leavelevel=e.leavelevel and l.experience=@exp and e.empcode=@ecode
	End
	Else if Year(GetDate())=2020
	Begin
		SELECT e.leavelevel,l.annual,l.probation,l.medical,l.experience, Convert(decimal(4,0),CEILING((l.annual*9/12))) as annual1 FROM empmaster e, leavelevel l 
		WHERE l.leavelevel=e.leavelevel and l.experience=@exp and e.empcode=@ecode
	End
	Else
	Begin
		SELECT e.leavelevel,l.annual,l.probation,l.medical,l.experience, l.annual as annual1 FROM empmaster e, leavelevel l 
		WHERE l.leavelevel=e.leavelevel and l.experience=@exp and e.empcode=@ecode
	End

End
ELSE
RAISERROR('No Record Found' ,16,1)
GO

GO

/* Source: hrmis_getannual.sql */
/****** Object:  StoredProcedure [dbo].[HRMIS_GetAnnual]    Script Date: 07/21/2026 11:30:38 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[HRMIS_GetAnnual]
@from datetime,
@to datetime,
@ecode varchar(6)
AS
-- 04/07/2012
--SELECT sum(workfor) as annual  from leaveform
--where leavetype in ( 'annual', 'emergency', 'planemergency' )  
--and (status='APPROVED' or status='CAPPROVED' or status='CSCHEDULED' or
-- status='APPROVEDDEPT' or status='APPROVEDSECT' or status='SCHEDULED' or status = 'S' )
-- and empno =@ecode and (fromdate between @from and @to)

SELECT (sum(days) - sum (nocf)) as annual  from leaveform
where leavetype in ( 'annual', 'emergency', 'planemergency' )  
and (status='APPROVED' or status='CAPPROVED' or status='CSCHEDULED' or
 status='APPROVEDDEPT' or status='APPROVEDSECT' or status='SCHEDULED' or status = 'S' )
and empno =@ecode and (fromdate between @from and @to)
GO

GO

/* Source: hrmis_getmedical.sql */
/****** Object:  StoredProcedure [dbo].[HRMIS_GetMedical]    Script Date: 07/21/2026 11:31:20 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[HRMIS_GetMedical]
@from datetime,
@to datetime,
@ecode varchar(6)
AS
SELECT sum(workfor) as medical  from leaveform
where (leavetype = 'Medical')  and (status='APPROVED'  or status='APPROVEDDEPT' or status='APPROVEDSECT' or status='SCHEDULED') and empno =@ecode and (fromdate between @from and @to) 


GO

GO

/* Source: hrmis_GetTotalLeave.sql */
/****** Object:  StoredProcedure [dbo].[HRMIS_GetTotalLeave]    Script Date: 07/21/2026 11:32:03 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[HRMIS_GetTotalLeave]
@ecode varchar(10),
@from datetime,
@to datetime

as
--SELECT sum(workfor) as Leavetaken  from leaveform
--where leavetype in ( 'annual', 'emergency', 'planemergency' ) 
--and (status='APPROVED'  or status='APPROVEDDEPT' or status='APPROVEDSECT' or status='SCHEDULED')
-- and empno =@ecode and (fromdate between @from and @to)
-- group by empno

if year(getdate())=2020 
Begin
SELECT (sum(days) - sum (nocf)) as Leavetaken   from leaveform
where leavetype in ( 'annual', 'emergency', 'planemergency' )  
and (status='APPROVED' or status='CAPPROVED' or status='CSCHEDULED' or
 status='APPROVEDDEPT' or status='APPROVEDSECT' or status='SCHEDULED' or status = 'S' )
and empno =@ecode and (fromdate between '04/01/2019' and '12/31/2019')
group by empno
End
Else
Begin
SELECT (sum(days) - sum (nocf)) as Leavetaken   from leaveform
where leavetype in ( 'annual', 'emergency', 'planemergency' )  
and (status='APPROVED' or status='CAPPROVED' or status='CSCHEDULED' or
 status='APPROVEDDEPT' or status='APPROVEDSECT' or status='SCHEDULED' or status = 'S' )
and empno =@ecode and (fromdate between @from and @to)
group by empno
End
GO

GO

/* Source: HRMIS_InsLeavenew.sql */
/****** Object:  StoredProcedure [dbo].[HRMIS_InsLeavenew]    Script Date: 07/21/2026 11:32:49 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[HRMIS_InsLeavenew]
@ecode varchar(6),
@adate datetime,
@totdays float,
@days float,
@fdate datetime,
@tdate datetime,
@ltype varchar(20),
@reason varchar(200),
@ltime varchar(30),
@carry char(1),
@nocf float,
@backdate char(1),
@appno int,

@sts varchar(15),
@PIC1 Varchar(20) = null,
@PIC11 Varchar(100)=null,
@PIC2 Varchar(20) = null,
@PIC22 Varchar(100)=null,

@AnnualBal varchar(10)=null,
@MedicalBal Varchar(10)=null

as

EXEC dbo.usp_Leave_DisableSmsLinkTriggers;

DECLARE @category VARCHAR(30)
DECLARE @dpt VARCHAR(30)
DECLARE @sct VARCHAR(30)
DECLARE @tms varchar(10)



if @ltype ='Annual'
begin
	set @tms='AL'
end


if @ltype='Calamity' 
begin
	    set @tms='CAL'  
end	
if @ltype='CompanyHoliday'
begin
	    set @tms='CH'
end
if @ltype='Compassionate'
begin
	    set @tms='CL'
end
if @ltype='Marriage-Children'
begin
	    set @tms='MAC'
end
if @ltype='Maternity'
begin
	    set @tms='ML'
end
if @ltype='Paternity'
begin
	    set @tms='PL'
end
if @ltype='PlanEmergency'
begin
        set @tms='PEAL' 
end 
  if @ltype='PlanEmergencyUP'
begin
        set @tms='PLUP' 
end
    if @ltype='Hospitalization'
begin
        set @tms='HL'  
end

if @ltype='marriage-self'
begin
        set @tms='MS'   
end 

if @ltype='Emergency'
begin
        set @tms='AL'
end
    if @ltype='EmergencyUnpaid'
begin
        set @tms='EUP'
end

if @ltype='EmergencyUP'
begin
        set @tms='EUP'
end

     if @ltype='Unpaid'
begin
        set @tms='UP'
end
     if @ltype='Medical'
begin
        set @tms='MC'
    end

select @category=category,@dpt=departmentcode,@sct=sectioncode from empmaster where empcode=@ecode

insert into leaveform(empno,applicationdate,days,workfor,fromdate,todate,leavetype,
reason,leavetime,carryfwd,nocf,backdate,appno,designation,department,sectioncode,leavetype1,status,
createdby,createdtime,modifiedby,modifiedtime,grantedleave,pic1, pic11, pic2, pic22, AnnualBal, MedicalBal)values
(@ecode,@adate,@totdays,@days,@fdate,@tdate,@ltype,@reason,@ltime,
@carry,@nocf,@backdate,@appno,@category,@dpt,@sct,@tms,@sts,@ecode,getdate(),@ecode,getdate(),@days, @pic1, @pic11, @pic2, @pic22, @AnnualBal, @MedicalBal)

GO

GO

/* Source: HRMIS_updleave_new.sql */
/****** Object:  StoredProcedure [dbo].[HRMIS_UpdLeave_new]    Script Date: 07/21/2026 11:33:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[HRMIS_UpdLeave_new]
@appno int,
@ecode varchar(6),
@adate datetime,
@totdays float,
@days float,
@fdate datetime,
@tdate datetime,
@ltype varchar(20),
@reason varchar(200),
@ltime varchar(30),
@carry char(1),
@nocf float,
@stat char(15),
@bk char(1)

as

EXEC dbo.usp_Leave_DisableSmsLinkTriggers;

DECLARE @tms varchar(10)



if @ltype ='Annual'
begin
	set @tms='AL'
end


if @ltype='Calamity' 
begin
	    set @tms='CAL'  
end	
if @ltype='CompanyHoliday'
begin
	    set @tms='CH'
end
if @ltype='Compassionate'
begin
	    set @tms='CL'
end
if @ltype='Marriage-Children'
begin
	    set @tms='MAC'
end
if @ltype='Maternity'
begin
	    set @tms='ML'
end
if @ltype='Paternity'
begin
	    set @tms='PL'
end
if @ltype='PlanEmergency'
begin
        set @tms='PEAL' 
end 
  if @ltype='PlanEmergencyUP'
begin
        set @tms='PLUP' 
end
    if @ltype='Hospitalization'
begin
        set @tms='HL'  
end

if @ltype='marriage-self'
begin
        set @tms='MS'   
end 

if @ltype='Emergency'
begin
        set @tms='AL'
end
    if @ltype='EmergencyUP'
begin
        set @tms='EUP'
end
     if @ltype='Unpaid'
begin
        set @tms='UP'
end
     if @ltype='Medical'
begin
        set @tms='MC'
    end


update Leaveform 
set empno = @ecode,applicationdate=@adate,days=@totdays,workfor=@days,
fromdate=@fdate,todate=@tdate,leavetype=@ltype,reason=@reason,leavetime=@ltime,
carryfwd=@carry,nocf=@nocf,backdate=@bk,status=@stat,leavetype1=@tms,modifiedby=@ecode,
modifiedtime=getdate(),grantedleave=@days where appno=@appno


GO

GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetEmployeeBasicLegacy
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @designationCol NVARCHAR(300) = CASE WHEN COL_LENGTH('dbo.empMaster', 'designation') IS NOT NULL THEN N'designation' ELSE N'NULL' END;
    DECLARE @departmentCol NVARCHAR(300) = CASE
        WHEN COL_LENGTH('dbo.empMaster', 'department') IS NOT NULL THEN N'department'
        WHEN COL_LENGTH('dbo.empMaster', 'departmentcode') IS NOT NULL THEN N'departmentcode'
        ELSE N'NULL' END;
    DECLARE @subDepartmentCol NVARCHAR(300) = CASE
        WHEN COL_LENGTH('dbo.empMaster', 'subDepartment') IS NOT NULL THEN N'subDepartment'
        WHEN COL_LENGTH('dbo.empMaster', 'subdepartmentcode') IS NOT NULL THEN N'subdepartmentcode'
        ELSE N'NULL' END;
    DECLARE @sectionCol NVARCHAR(300) = CASE
        WHEN COL_LENGTH('dbo.empMaster', 'section') IS NOT NULL THEN N'section'
        WHEN COL_LENGTH('dbo.empMaster', 'sectioncode') IS NOT NULL THEN N'sectioncode'
        ELSE N'NULL' END;
    DECLARE @dateOfServiceCol NVARCHAR(300) = CASE WHEN COL_LENGTH('dbo.empMaster', 'dateOfService') IS NOT NULL THEN N'dateOfService' ELSE N'NULL' END;
    DECLARE @dateOfJoinCol NVARCHAR(300) = CASE WHEN COL_LENGTH('dbo.empMaster', 'dateOfJoin') IS NOT NULL THEN N'dateOfJoin' ELSE N'NULL' END;
    DECLARE @isOperatorCol NVARCHAR(300) = CASE WHEN COL_LENGTH('dbo.empMaster', 'isOperator') IS NOT NULL THEN N'isOperator' ELSE N'NULL' END;

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT TOP 1
               empCode AS EmpCode,
               empName AS EmpName,
               ' + @designationCol + N' AS Designation,
               ' + @departmentCol + N' AS Department,
               ' + @subDepartmentCol + N' AS SubDepartment,
               ' + @sectionCol + N' AS Section,
               ' + @dateOfServiceCol + N' AS DateOfService,
               ' + @dateOfJoinCol + N' AS DateOfJoin,
               ' + @isOperatorCol + N' AS IsOperator
        FROM dbo.empMaster
        WHERE empCode = @pEmpCode';

    EXEC sp_executesql @sql, N'@pEmpCode NVARCHAR(100)', @pEmpCode = @EmpCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_SearchEmployees
    @SearchText NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @activeFilter NVARCHAR(MAX) = N'';
    IF COL_LENGTH('dbo.empMaster', 'isResigned') IS NOT NULL
        SET @activeFilter = N' AND ISNULL(isResigned, '''') NOT IN (''Y'', ''Yes'', ''YES'', ''1'')';
    ELSE IF COL_LENGTH('dbo.empMaster', 'resigned') IS NOT NULL
        SET @activeFilter = N' AND ISNULL(resigned, '''') NOT IN (''Y'', ''Yes'', ''YES'', ''1'')';

    DECLARE @sql NVARCHAR(MAX) = N'
        SELECT TOP (100)
               empCode AS EmpCode,
               CONCAT(empCode, N'' - '', empName) AS EmpDisplay
        FROM dbo.empMaster
        WHERE (@pSearch IS NULL OR @pSearch = '''' OR empCode LIKE N''%'' + @pSearch + N''%'' OR empName LIKE N''%'' + @pSearch + N''%'')
        ' + @activeFilter + N'
        ORDER BY empName, empCode';

    EXEC sp_executesql @sql, N'@pSearch NVARCHAR(200)', @pSearch = @SearchText;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetLeaveLevelLegacy
    @EmpCode NVARCHAR(100),
    @ExpLevel INT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.HRMIS_getleavelevel @ecode = @EmpCode, @exp = @ExpLevel;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetAnnualTakenLegacy
    @FyStart DATE,
    @FyEnd DATE,
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.HRMIS_GetAnnual @from = @FyStart, @to = @FyEnd, @ecode = @EmpCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetMedicalTakenLegacy
    @FyStart DATE,
    @FyEnd DATE,
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.HRMIS_GetMedical @from = @FyStart, @to = @FyEnd, @ecode = @EmpCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetTotalLeaveLegacy
    @EmpCode NVARCHAR(100),
    @FyStart DATE,
    @FyEnd DATE
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.HRMIS_GetTotalLeave @ecode = @EmpCode, @from = @FyStart, @to = @FyEnd;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetCarryForwardLegacy
    @EmpCode NVARCHAR(100),
    @ForTheYear INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 *
    FROM dbo.leavecf
    WHERE empcode = @EmpCode
      AND fortheyear = @ForTheYear;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_InsertCarryForwardLegacy
    @EmpCode NVARCHAR(100),
    @LeaveRemain DECIMAL(10,2),
    @ForTheYear INT,
    @Remain DECIMAL(10,2),
    @Cfwd DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.leavecf WHERE empcode = @EmpCode AND fortheyear = @ForTheYear)
    BEGIN
        INSERT INTO dbo.leavecf (empcode, leaveremain, fortheyear, remain, cfwd)
        VALUES (@EmpCode, @LeaveRemain, @ForTheYear, @Remain, @Cfwd);
    END
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_UpdateCarryForwardLegacy
    @EmpCode NVARCHAR(100),
    @ForTheYear INT,
    @Remain DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.leavecf
       SET remain = @Remain
     WHERE empcode = @EmpCode
       AND fortheyear = @ForTheYear;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetNextAppNoLegacy
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ISNULL(MAX(appno), 0) + 1 AS NextAppNo
    FROM dbo.leaveform;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_CalculateDaysLegacy
    @EmpCode NVARCHAR(100),
    @FromDate DATE,
    @ToDate DATE,
    @IsHalfDay BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @FromDate IS NULL OR @ToDate IS NULL OR @FromDate > @ToDate
    BEGIN
        SELECT CAST(0 AS DECIMAL(5,2)) AS LeaveDays;
        RETURN;
    END

    IF @IsHalfDay = 1
    BEGIN
        SELECT CAST(0.5 AS DECIMAL(5,2)) AS LeaveDays;
        RETURN;
    END

    -- Existing HRMIS Leaveform accepted manually-entered days; for MVC auto-fill,
    -- default to inclusive calendar days without creating any new shift/holiday tables.
    SELECT CAST(DATEDIFF(DAY, @FromDate, @ToDate) + 1 AS DECIMAL(5,2)) AS LeaveDays;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_ApplyUsingLegacyTables
    @IsUpdate BIT,
    @AppNo INT,
    @EmpCode NVARCHAR(100),
    @ApplicationDate DATETIME,
    @AppliedDays DECIMAL(10,2),
    @WorkFor DECIMAL(10,2),
    @FromDate DATE,
    @ToDate DATE,
    @LeaveType NVARCHAR(100),
    @Reason NVARCHAR(500),
    @LeaveTime NVARCHAR(100) = NULL,
    @CarryForward CHAR(1),
    @NoCarryForward DECIMAL(10,2),
    @BackDate CHAR(1),
    @Pic1 NVARCHAR(100) = NULL,
    @Pic1Name NVARCHAR(200) = NULL,
    @Pic2 NVARCHAR(100) = NULL,
    @Pic2Name NVARCHAR(200) = NULL,
    @AnnualBalance NVARCHAR(50) = NULL,
    @MedicalBalance NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @IsUpdate = 1
    BEGIN
        EXEC dbo.HRMIS_UpdLeave_new
            @AppNo, @EmpCode, @ApplicationDate, @AppliedDays, @WorkFor, @FromDate, @ToDate,
            @LeaveType, @Reason, @LeaveTime, @CarryForward, @NoCarryForward, N'SCHEDULED', @BackDate;
    END
    ELSE
    BEGIN
        EXEC dbo.HRMIS_InsLeavenew
            @EmpCode, @ApplicationDate, @AppliedDays, @WorkFor, @FromDate, @ToDate,
            @LeaveType, @Reason, @LeaveTime, @CarryForward, @NoCarryForward, @BackDate,
            @AppNo, N'SCHEDULED', @Pic1, @Pic1Name, @Pic2, @Pic2Name, @AnnualBalance, @MedicalBalance;
    END
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

    SELECT appno AS AppNo,
           applicationdate AS ApplicationDate,
           days AS LeaveDays,
           fromdate AS FromDate,
           todate AS ToDate,
           leavetype AS LeaveTypeName,
           reason AS ReasonText,
           LTRIM(RTRIM(status)) AS Status,
           statusreason AS StatusReason,
           approvedby AS ApprovedBy,
           approveddate AS ApprovedDate,
           leavetime AS HalfDayLeaveText,
           pic1 AS PersonInCharge1,
           pic11 AS PersonInCharge1Name,
           pic2 AS PersonInCharge2,
           pic22 AS PersonInCharge2Name
    INTO #FilteredLegacySelfStatus
    FROM dbo.leaveform
    WHERE empno = @EmpCode
      AND fromdate >= DATEFROMPARTS(YEAR(GETDATE()), 1, 1)
      AND (@GlobalSearch IS NULL OR @GlobalSearch = '' OR
           CONVERT(NVARCHAR(50), appno) LIKE '%' + @GlobalSearch + '%' OR
           leavetype LIKE '%' + @GlobalSearch + '%' OR
           reason LIKE '%' + @GlobalSearch + '%' OR
           status LIKE '%' + @GlobalSearch + '%')
      AND (@AppNo IS NULL OR @AppNo = '' OR CONVERT(NVARCHAR(50), appno) LIKE '%' + @AppNo + '%')
      AND (@LeaveType IS NULL OR @LeaveType = '' OR leavetype LIKE '%' + @LeaveType + '%')
      AND (@Status IS NULL OR @Status = '' OR status LIKE '%' + @Status + '%');

    ;WITH Ordered AS
    (
        SELECT *, ROW_NUMBER() OVER (ORDER BY AppNo DESC) AS RowNum
        FROM #FilteredLegacySelfStatus
    )
    SELECT AppNo, ApplicationDate, LeaveDays, FromDate, ToDate, LeaveTypeName, ReasonText, Status,
           StatusReason, ApprovedBy, ApprovedDate, HalfDayLeaveText, PersonInCharge1Name, PersonInCharge2Name
    FROM Ordered
    WHERE RowNum BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize)
    ORDER BY RowNum;

    SELECT COUNT(1) AS TotalCount FROM #FilteredLegacySelfStatus;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetForEdit
    @AppNo INT,
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
           appno AS AppNo,
           leavetype AS LeaveTypeName,
           fromdate AS FromDate,
           todate AS ToDate,
           days AS LeaveDays,
           leavetime AS HalfDayLeaveText,
           reason AS ReasonText,
           pic1 AS PersonInCharge1,
           pic11 AS PersonInCharge1Name,
           pic2 AS PersonInCharge2,
           pic22 AS PersonInCharge2Name,
           LTRIM(RTRIM(status)) AS Status
    FROM dbo.leaveform
    WHERE appno = @AppNo
      AND empno = @EmpCode
      AND LTRIM(RTRIM(status)) = N'SCHEDULED';
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_Cancel
    @AppNo INT,
    @EmpCode NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.leaveform WHERE appno = @AppNo AND empno = @EmpCode)
        THROW 51000, 'Leave application not found.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.leaveform WHERE appno = @AppNo AND empno = @EmpCode AND LTRIM(RTRIM(status)) = N'SCHEDULED')
        THROW 51001, 'Only Scheduled leave can be cancelled.', 1;

    UPDATE dbo.leaveform
       SET status = N'CANCELLED'
     WHERE appno = @AppNo
       AND empno = @EmpCode;
END
GO
