/* =============================================================
   Leave Application - EHRM lookup stored procedures
   -------------------------------------------------------------
   Execute this script in the EHRM database configured by
   appsettings.json -> ConnectionStrings:EHRMConnection.

   These lookup procedures read current project master tables only.
   Legacy leave calculation/save/status procedures are in HRMIS.
   ============================================================= */
GO

CREATE OR ALTER PROCEDURE dbo.usp_Leave_GetLeaveTypes
AS
BEGIN
    SET NOCOUNT ON;

    SELECT LeaveID,
           LeaveType,
           CASE
               WHEN REPLACE(LOWER(LeaveType), ' ', '') LIKE '%annual%' THEN N'Deduction from Annual entitlement'
               WHEN LOWER(LeaveType) LIKE '%medical%' THEN N'Deduct from Medical entitlement'
               ELSE N'As per existing HRMIS leave rule'
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
    FROM (VALUES
        (1, N'07.30 AM - 11.30 AM'),
        (2, N'08.00 AM - 12.00 PM'),
        (3, N'11.30 AM - 03.30 PM'),
        (4, N'12.00 PM - 05.00 PM'),
        (5, N'03.30 PM - 07.30 PM'),
        (6, N'07.30 PM - 11.30 PM'),
        (7, N'11.30 PM - 03.30 AM'),
        (8, N'03.30 AM - 07.30 AM'),
        (9, N'09.00 PM - 03.00 AM'),
        (10, N'03.00 AM - 09.00 AM')
    ) AS H(HalfDayLeaveID, TimeText)
    ORDER BY HalfDayLeaveID;
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

