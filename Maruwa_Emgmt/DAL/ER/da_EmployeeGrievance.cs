using Maruwa_Emgmt.InterFace.ER;
using Maruwa_Emgmt.Models.ER;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Maruwa_Emgmt.DAL.ER
{
    public class da_EmployeeGrievance : i_EmployeeGrievance
    {
        private readonly string _connectionString;
        private readonly ILogger<da_EmployeeGrievance> _logger;

        public da_EmployeeGrievance(IConfiguration configuration, ILogger<da_EmployeeGrievance> logger)
        {
            _connectionString = configuration.GetConnectionString("EHRMConnection") ?? throw new InvalidOperationException("EHRMConnection is missing in appsettings.json");
            _logger = logger;
        }

        public async Task<List<EmployeeLookupVm>> SearchEmployeeLookupAsync(string? searchText)
        {
            var list = new List<EmployeeLookupVm>();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_EGF_SearchEmployee", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@SearchText", string.IsNullOrWhiteSpace(searchText) ? (object)DBNull.Value : searchText.Trim());
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) list.Add(MapEmployee(reader));
            return list;
        }

        public async Task<EmployeeLookupVm?> GetEmployeeByCodeAsync(string empCode)
        {
            if (string.IsNullOrWhiteSpace(empCode)) return null;
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_EGF_GetEmployeeByCode", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@EmpCode", empCode.Trim());
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapEmployee(reader) : null;
        }

        public async Task<EmployeeGrievanceListResult> GetMyGrievancesAsync(EmployeeGrievanceSearchRequest request, string loggedInEmpCode, bool isHrUser = false)
        {
            var result = new EmployeeGrievanceListResult();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("usp_EGF_GetMyGrievanceList", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@LoggedInEmpCode", loggedInEmpCode);
            cmd.Parameters.AddWithValue("@IsHrUser", isHrUser);
            AddSearchParameters(cmd, request);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) result.Data.Add(MapListItem(reader));
            if (await reader.NextResultAsync() && await reader.ReadAsync()) result.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            return result;
        }

        public async Task<EmployeeGrievanceFormVm?> GetGrievanceByIdAsync(int grievanceId, string loggedInEmpCode, bool isHrUser = false)
        {
            EmployeeGrievanceFormVm? model = null;
            await using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            await using (var cmd = new SqlCommand("usp_EGF_GetComplaintById", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                cmd.Parameters.AddWithValue("@LoggedInEmpCode", loggedInEmpCode);
                cmd.Parameters.AddWithValue("@IsHrUser", isHrUser);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync()) model = MapForm(reader);
            }

            if (model == null) return null;

            await using (var cmd = new SqlCommand("usp_EGF_GetInvolvedParties", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) model.InvolvedParties.Add(MapParty(reader));
            }

            await using (var cmd = new SqlCommand("usp_EGF_GetAttachments", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) model.Attachments.Add(MapAttachment(reader));
            }

            await using (var cmd = new SqlCommand("usp_EGF_GetHrAction", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync()) model.HrAction = MapHrAction(reader);
            }

            return model;
        }

        public async Task<(bool Success, string Message, int GrievanceID, string ReferenceNo)> SaveComplaintAsync(EmployeeGrievanceFormVm model, string employeeCode)
        {
            await using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            await using var tran = await con.BeginTransactionAsync();
            try
            {
                await using var cmd = new SqlCommand("usp_EGF_SaveComplaint", con, (SqlTransaction)tran) { CommandType = CommandType.StoredProcedure };
                AddComplaintParameters(cmd, model, employeeCode);
                var grievanceIdParam = new SqlParameter("@GrievanceIDOut", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var referenceNoParam = new SqlParameter("@ReferenceNoOut", SqlDbType.NVarChar, 30) { Direction = ParameterDirection.Output };
                var statusParam = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(grievanceIdParam);
                cmd.Parameters.Add(referenceNoParam);
                cmd.Parameters.Add(statusParam);
                cmd.Parameters.Add(messageParam);

                await cmd.ExecuteNonQueryAsync();
                var success = Convert.ToInt32(statusParam.Value) == 1;
                var message = Convert.ToString(messageParam.Value) ?? "Employee grievance saved successfully";
                var grievanceId = grievanceIdParam.Value == DBNull.Value ? 0 : Convert.ToInt32(grievanceIdParam.Value);
                var referenceNo = Convert.ToString(referenceNoParam.Value) ?? string.Empty;

                if (!success)
                {
                    await tran.RollbackAsync();
                    return (false, message, grievanceId, referenceNo);
                }

                foreach (var party in model.InvolvedParties)
                {
                    await using var partyCmd = new SqlCommand("usp_EGF_SaveInvolvedParty", con, (SqlTransaction)tran) { CommandType = CommandType.StoredProcedure };
                    partyCmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                    partyCmd.Parameters.AddWithValue("@EmployeeID", DbValue(party.EmployeeID));
                    partyCmd.Parameters.AddWithValue("@EmployeeName", DbValue(party.EmployeeName));
                    partyCmd.Parameters.AddWithValue("@PositionTitle", DbValue(party.PositionTitle));
                    partyCmd.Parameters.AddWithValue("@Department", DbValue(party.Department));
                    await partyCmd.ExecuteNonQueryAsync();
                }

                foreach (var attachment in model.Attachments)
                {
                    await using var attachCmd = new SqlCommand("usp_EGF_SaveAttachment", con, (SqlTransaction)tran) { CommandType = CommandType.StoredProcedure };
                    attachCmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                    attachCmd.Parameters.AddWithValue("@OriginalFileName", DbValue(attachment.OriginalFileName));
                    attachCmd.Parameters.AddWithValue("@StoredFileName", DbValue(attachment.StoredFileName));
                    attachCmd.Parameters.AddWithValue("@FilePath", DbValue(attachment.FilePath));
                    attachCmd.Parameters.AddWithValue("@ContentType", DbValue(attachment.ContentType));
                    attachCmd.Parameters.AddWithValue("@SizeBytes", attachment.SizeBytes);
                    attachCmd.Parameters.AddWithValue("@UploadedBy", employeeCode);
                    await attachCmd.ExecuteNonQueryAsync();
                }

                await tran.CommitAsync();
                return (true, message, grievanceId, referenceNo);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                _logger.LogError(ex, "Error while saving employee grievance form");
                return (false, ex.Message, 0, string.Empty);
            }
        }

        public async Task<(bool Success, string Message)> SaveHrActionAsync(EmployeeGrievanceHrActionVm model, string employeeCode)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand("usp_EGF_SaveHrAction", con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@GrievanceID", model.GrievanceID);
                cmd.Parameters.AddWithValue("@HREmpId", DbValue(model.HREmpId ?? employeeCode));
                cmd.Parameters.AddWithValue("@HRName", DbValue(model.HRName));
                cmd.Parameters.AddWithValue("@InvestigationSummary", DbValue(model.InvestigationSummary));
                cmd.Parameters.AddWithValue("@EmployeeExplanation", DbValue(model.EmployeeExplanation));
                cmd.Parameters.AddWithValue("@OutcomeResolved", model.OutcomeResolved);
                cmd.Parameters.AddWithValue("@OutcomeReferredToER", model.OutcomeReferredToER);
                cmd.Parameters.AddWithValue("@OutcomeReferredToDomesticInquiry", model.OutcomeReferredToDomesticInquiry);
                cmd.Parameters.AddWithValue("@MinorMisconduct", model.MinorMisconduct);
                cmd.Parameters.AddWithValue("@MajorMisconduct", model.MajorMisconduct);
                cmd.Parameters.AddWithValue("@MajorMisconductText", DbValue(model.MajorMisconductText));
                cmd.Parameters.AddWithValue("@HRSignaturePath", DbValue(model.HRSignaturePath));
                cmd.Parameters.AddWithValue("@EmployeeSignaturePath", DbValue(model.EmployeeSignaturePath));
                cmd.Parameters.AddWithValue("@Department", DbValue(model.Department ?? "HUMAN RESOURCE"));
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "HR action saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving HR action for grievance");
                return (false, ex.Message);
            }
        }

        private static void AddSearchParameters(SqlCommand cmd, EmployeeGrievanceSearchRequest request)
        {
            cmd.Parameters.AddWithValue("@GlobalSearch", DbValue(request.GlobalSearch));
            cmd.Parameters.AddWithValue("@ReferenceNo", DbValue(request.ReferenceNo));
            cmd.Parameters.AddWithValue("@StatusFilter", DbValue(request.Status));
            cmd.Parameters.AddWithValue("@SortColumn", request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", request.SortDirection);
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
        }

        private static void AddComplaintParameters(SqlCommand cmd, EmployeeGrievanceFormVm model, string employeeCode)
        {
            cmd.Parameters.AddWithValue("@GrievanceID", model.GrievanceID);
            cmd.Parameters.AddWithValue("@ComplainantEmpId", DbValue(model.ComplainantEmpId));
            cmd.Parameters.AddWithValue("@ComplainantName", DbValue(model.ComplainantName));
            cmd.Parameters.AddWithValue("@Department", DbValue(model.Department));
            cmd.Parameters.AddWithValue("@PositionTitle", DbValue(model.PositionTitle));
            cmd.Parameters.AddWithValue("@DateOfReport", model.DateOfReport ?? DateTime.Now);
            cmd.Parameters.AddWithValue("@UnfairTreatment", model.UnfairTreatment);
            cmd.Parameters.AddWithValue("@HarassmentBullying", model.HarassmentBullying);
            cmd.Parameters.AddWithValue("@WorkLapses", model.WorkLapses);
            cmd.Parameters.AddWithValue("@PolicySopBreach", model.PolicySopBreach);
            cmd.Parameters.AddWithValue("@OshaConcern", model.OshaConcern);
            cmd.Parameters.AddWithValue("@SupervisorMisconduct", model.SupervisorMisconduct);
            cmd.Parameters.AddWithValue("@AbuseOfAuthority", model.AbuseOfAuthority);
            cmd.Parameters.AddWithValue("@WorkingHoursIssue", model.WorkingHoursIssue);
            cmd.Parameters.AddWithValue("@OtherComplaint", model.OtherComplaint);
            cmd.Parameters.AddWithValue("@OtherComplaintText", DbValue(model.OtherComplaintText));
            cmd.Parameters.AddWithValue("@ConductDate", model.ConductDate.HasValue ? (object)model.ConductDate.Value.Date : DBNull.Value);
            cmd.Parameters.AddWithValue("@ConductTime", model.ConductTime.HasValue ? (object)model.ConductTime.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Location", DbValue(model.Location));
            cmd.Parameters.AddWithValue("@IncidentDescription", DbValue(model.IncidentDescription));
            cmd.Parameters.AddWithValue("@Witnesses", DbValue(model.Witnesses));
            cmd.Parameters.AddWithValue("@SupportingDocumentsAttached", model.SupportingDocumentsAttached);
            cmd.Parameters.AddWithValue("@DesiredOutcome", DbValue(model.DesiredOutcome));
            cmd.Parameters.AddWithValue("@EmployeeSignaturePath", DbValue(model.EmployeeSignaturePath));
            cmd.Parameters.AddWithValue("@DeclarationEmployeeName", DbValue(model.DeclarationEmployeeName));
            cmd.Parameters.AddWithValue("@DeclarationEmployeeId", DbValue(model.DeclarationEmployeeId));
            cmd.Parameters.AddWithValue("@DeclarationDate", model.DeclarationDate ?? DateTime.Now);
            cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
        }

        private static object DbValue(string? value) => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

        private static EmployeeLookupVm MapEmployee(IDataRecord reader) => new()
        {
            EmpCode = Convert.ToString(reader["EmpCode"]) ?? string.Empty,
            EmpName = Convert.ToString(reader["EmpName"]) ?? string.Empty,
            Department = Convert.ToString(reader["Department"]) ?? string.Empty,
            PositionTitle = Convert.ToString(reader["PositionTitle"]) ?? string.Empty,
            EmailID = Convert.ToString(reader["EmailID"]) ?? string.Empty
        };

        private static EmployeeGrievanceListItemVm MapListItem(IDataRecord reader) => new()
        {
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            ReferenceNo = Convert.ToString(reader["ReferenceNo"]) ?? string.Empty,
            ComplainantEmpId = Convert.ToString(reader["ComplainantEmpId"]) ?? string.Empty,
            ComplainantName = Convert.ToString(reader["ComplainantName"]) ?? string.Empty,
            Department = Convert.ToString(reader["Department"]) ?? string.Empty,
            DateOfReport = reader["DateOfReport"] == DBNull.Value ? null : Convert.ToDateTime(reader["DateOfReport"]),
            GrievanceSummary = Convert.ToString(reader["GrievanceSummary"]) ?? string.Empty,
            Status = Convert.ToString(reader["Status"]) ?? string.Empty,
            CreatedOn = reader["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["CreatedOn"])
        };

        private static EmployeeGrievanceFormVm MapForm(IDataRecord reader) => new()
        {
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            ReferenceNo = Convert.ToString(reader["ReferenceNo"]),
            ComplainantEmpId = Convert.ToString(reader["ComplainantEmpId"]) ?? string.Empty,
            ComplainantName = Convert.ToString(reader["ComplainantName"]) ?? string.Empty,
            Department = Convert.ToString(reader["Department"]) ?? string.Empty,
            PositionTitle = Convert.ToString(reader["PositionTitle"]) ?? string.Empty,
            DateOfReport = reader["DateOfReport"] == DBNull.Value ? null : Convert.ToDateTime(reader["DateOfReport"]),
            UnfairTreatment = ToBool(reader["UnfairTreatment"]),
            HarassmentBullying = ToBool(reader["HarassmentBullying"]),
            WorkLapses = ToBool(reader["WorkLapses"]),
            PolicySopBreach = ToBool(reader["PolicySopBreach"]),
            OshaConcern = ToBool(reader["OshaConcern"]),
            SupervisorMisconduct = ToBool(reader["SupervisorMisconduct"]),
            AbuseOfAuthority = ToBool(reader["AbuseOfAuthority"]),
            WorkingHoursIssue = ToBool(reader["WorkingHoursIssue"]),
            OtherComplaint = ToBool(reader["OtherComplaint"]),
            OtherComplaintText = Convert.ToString(reader["OtherComplaintText"]),
            ConductDate = reader["ConductDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ConductDate"]),
            ConductTime = reader["ConductTime"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)reader["ConductTime"],
            Location = Convert.ToString(reader["Location"]),
            IncidentDescription = Convert.ToString(reader["IncidentDescription"]),
            Witnesses = Convert.ToString(reader["Witnesses"]),
            SupportingDocumentsAttached = ToBool(reader["SupportingDocumentsAttached"]),
            DesiredOutcome = Convert.ToString(reader["DesiredOutcome"]),
            EmployeeSignaturePath = Convert.ToString(reader["EmployeeSignaturePath"]),
            DeclarationEmployeeName = Convert.ToString(reader["DeclarationEmployeeName"]),
            DeclarationEmployeeId = Convert.ToString(reader["DeclarationEmployeeId"]),
            DeclarationDate = reader["DeclarationDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["DeclarationDate"]),
            Status = Convert.ToString(reader["Status"]) ?? string.Empty
        };

        private static EmployeeGrievanceInvolvedPartyVm MapParty(IDataRecord reader) => new()
        {
            PartyID = Convert.ToInt32(reader["PartyID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            EmployeeID = Convert.ToString(reader["EmployeeID"]) ?? string.Empty,
            EmployeeName = Convert.ToString(reader["EmployeeName"]) ?? string.Empty,
            PositionTitle = Convert.ToString(reader["PositionTitle"]) ?? string.Empty,
            Department = Convert.ToString(reader["Department"]) ?? string.Empty
        };

        private static EmployeeGrievanceAttachmentVm MapAttachment(IDataRecord reader) => new()
        {
            AttachmentID = Convert.ToInt32(reader["AttachmentID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            OriginalFileName = Convert.ToString(reader["OriginalFileName"]) ?? string.Empty,
            StoredFileName = Convert.ToString(reader["StoredFileName"]) ?? string.Empty,
            FilePath = Convert.ToString(reader["FilePath"]) ?? string.Empty,
            ContentType = Convert.ToString(reader["ContentType"]) ?? string.Empty,
            SizeBytes = reader["SizeBytes"] == DBNull.Value ? 0 : Convert.ToInt64(reader["SizeBytes"]),
            UploadedOn = reader["UploadedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["UploadedOn"])
        };

        private static EmployeeGrievanceHrActionVm MapHrAction(IDataRecord reader) => new()
        {
            HRActionID = Convert.ToInt32(reader["HRActionID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            HREmpId = Convert.ToString(reader["HREmpId"]),
            HRName = Convert.ToString(reader["HRName"]),
            ActionDate = reader["ActionDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["ActionDate"]),
            InvestigationSummary = Convert.ToString(reader["InvestigationSummary"]),
            EmployeeExplanation = Convert.ToString(reader["EmployeeExplanation"]),
            OutcomeResolved = ToBool(reader["OutcomeResolved"]),
            OutcomeReferredToER = ToBool(reader["OutcomeReferredToER"]),
            OutcomeReferredToDomesticInquiry = ToBool(reader["OutcomeReferredToDomesticInquiry"]),
            MinorMisconduct = ToBool(reader["MinorMisconduct"]),
            MajorMisconduct = ToBool(reader["MajorMisconduct"]),
            MajorMisconductText = Convert.ToString(reader["MajorMisconductText"]),
            HRSignaturePath = Convert.ToString(reader["HRSignaturePath"]),
            EmployeeSignaturePath = Convert.ToString(reader["EmployeeSignaturePath"]),
            Department = Convert.ToString(reader["Department"])
        };

        private static bool ToBool(object value) => value != DBNull.Value && Convert.ToBoolean(value);
    }
}
