/* =============================================================
   Leave Application SQL Deployment Notes
   =============================================================
   1) Execute SQLScripts/LeaveApplication_HRMIS.sql in HRMIS database
      configured in appsettings.json as LiveHRMISConnection.

   2) Execute SQLScripts/LeaveApplication_EHRM_Lookups.sql in EHRM
      database configured in appsettings.json as EHRMConnection.

   The MVC code calls these databases separately:
   - HRMISConnection: legacy HRMIS procedures/tables for leave summary,
     carry forward, annual, medical, total leave, insert/update leave,
     leave self status, employee search, and PIC employee lookup.
   - EHRMConnection: LeaveTypeMaster, ReasonTypeMaster, and half-day
     lookup procedures used by the current project UI.
   ============================================================= */
GO
