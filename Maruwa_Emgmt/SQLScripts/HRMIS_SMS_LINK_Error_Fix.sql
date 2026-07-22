/* =============================================================
   HRMIS Leave SMS_LINK Error Fix
   -------------------------------------------------------------
   Execute this script in the HRMIS database configured as
   LiveHRMISConnection.

   It fixes Apply Leave error:
   Invalid object name 'sms.dbo.SMS_LINK'

   Reason: some old HRMIS databases contain leaveform triggers or older
   stored procedure versions that try to write to the external SMS database.
   This script creates SMS-safe versions of the HRMIS leave insert/update
   stored procedures and disables only leaveform triggers that reference
   sms.dbo.SMS_LINK.
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

if @ltype='Marriage-self'
begin
        set @tms='MS'   
end 

if @ltype='Emergency-Annual'
begin
        set @tms='AL'
end
    if @ltype='Emergency-Unpaid'
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

if @ltype='Marriage-self'
begin
        set @tms='MS'   
end 

if @ltype='Emergency-Annual'
begin
        set @tms='AL'
end
if @ltype='Emergency-Unpaid'
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


update Leaveform 
set empno = @ecode,applicationdate=@adate,days=@totdays,workfor=@days,
fromdate=@fdate,todate=@tdate,leavetype=@ltype,reason=@reason,leavetime=@ltime,
carryfwd=@carry,nocf=@nocf,backdate=@bk,status=@stat,leavetype1=@tms,modifiedby=@ecode,
modifiedtime=getdate(),grantedleave=@days where appno=@appno


GO

GO


