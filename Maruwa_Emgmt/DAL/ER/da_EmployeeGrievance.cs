using Maruwa_Emgmt.InterFace.ER;
using Maruwa_Emgmt.Models.ER;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

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
            cmd.Parameters.AddWithValue("@SearchText", DbValue(searchText));
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

            await using (var cmd = new SqlCommand("usp_EGF_GetGrievanceNatures", con) { CommandType = CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) model.SelectedNatures.Add(MapGrievanceNature(reader));
            }

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

            if (model.HrAction != null)
            {
                await using var empCmd = new SqlCommand("usp_EGF_GetHrActionEmployees", con) { CommandType = CommandType.StoredProcedure };
                empCmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                await using var empReader = await empCmd.ExecuteReaderAsync();
                while (await empReader.ReadAsync()) model.HrAction.ActionEmployees.Add(MapHrActionEmployee(empReader));
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

                await using (var deleteNatureCmd = new SqlCommand("usp_EGF_DeleteGrievanceNatures", con, (SqlTransaction)tran) { CommandType = CommandType.StoredProcedure })
                {
                    deleteNatureCmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                    await deleteNatureCmd.ExecuteNonQueryAsync();
                }

                foreach (var nature in model.SelectedNatures)
                {
                    await using var natureCmd = new SqlCommand("usp_EGF_SaveGrievanceNature", con, (SqlTransaction)tran) { CommandType = CommandType.StoredProcedure };
                    natureCmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                    natureCmd.Parameters.AddWithValue("@NatureID", nature.NatureID);
                    natureCmd.Parameters.AddWithValue("@NatureName", DbValue(nature.NatureName));
                    natureCmd.Parameters.AddWithValue("@IsOther", nature.IsOther);
                    natureCmd.Parameters.AddWithValue("@OtherComplaintText", DbValue(model.OtherComplaintText ?? nature.OtherComplaintText));
                    natureCmd.Parameters.AddWithValue("@CreatedBy", DbValue(employeeCode));
                    await natureCmd.ExecuteNonQueryAsync();
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

        public async Task<(bool Success, string Message)> MarkViewedByHrAsync(int grievanceId, string employeeCode)
        {
            return await ExecuteStatusProcedureAsync("usp_EGF_MarkViewedByHr", grievanceId, employeeCode, null);
        }

        public async Task<(bool Success, string Message)> UpdateHrRemarksAsync(int grievanceId, string remarks, string employeeCode)
        {
            return await ExecuteStatusProcedureAsync("usp_EGF_UpdateHrRemarks", grievanceId, employeeCode, remarks);
        }

        private async Task<(bool Success, string Message)> ExecuteStatusProcedureAsync(string procedureName, int grievanceId, string employeeCode, string? remarks)
        {
            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand(procedureName, con) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@GrievanceID", grievanceId);
                if (remarks != null) cmd.Parameters.AddWithValue("@HRRemarks", DbValue(remarks));
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                var status = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var message = new SqlParameter("@Message", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(status);
                cmd.Parameters.Add(message);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return (Convert.ToInt32(status.Value) == 1, Convert.ToString(message.Value) ?? "Updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing {ProcedureName} for grievance", procedureName);
                return (false, ex.Message);
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
                cmd.Parameters.AddWithValue("@ActionEmployeeId", DbValue(model.ActionEmployeeId));
                cmd.Parameters.AddWithValue("@ActionEmployeeName", DbValue(model.ActionEmployeeName));
                var hrEmployeesJson = JsonSerializer.Serialize(model.ActionEmployees ?? new List<EmployeeGrievanceHrActionEmployeeVm>(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                cmd.Parameters.AddWithValue("@HrActionEmployeesJson", DbValue(hrEmployeesJson));
                cmd.Parameters.AddWithValue("@InvestigationSummary", DbValue(model.InvestigationSummary));
                cmd.Parameters.AddWithValue("@EmployeeExplanation", DbValue(model.EmployeeExplanation));
                cmd.Parameters.AddWithValue("@Remarks", DbValue(model.Remarks));
                cmd.Parameters.AddWithValue("@OutcomeResolved", model.OutcomeResolved);
                cmd.Parameters.AddWithValue("@OutcomeReferredToER", model.OutcomeReferredToER);
                cmd.Parameters.AddWithValue("@OutcomeReferredToDomesticInquiry", model.OutcomeReferredToDomesticInquiry);
                cmd.Parameters.AddWithValue("@MinorMisconduct", model.MinorMisconduct);
                cmd.Parameters.AddWithValue("@MajorMisconduct", model.MajorMisconduct);
                cmd.Parameters.AddWithValue("@MajorMisconductText", DbValue(model.MajorMisconductText));
                cmd.Parameters.AddWithValue("@HRSignaturePath", DbValue(model.HRSignaturePath));
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
            cmd.Parameters.AddWithValue("@SortColumn", string.IsNullOrWhiteSpace(request.SortColumn) ? "CreatedOn" : request.SortColumn);
            cmd.Parameters.AddWithValue("@SortDirection", string.Equals(request.SortDirection, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC");
            cmd.Parameters.AddWithValue("@PageNumber", request.PageNumber <= 0 ? 1 : request.PageNumber);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize <= 0 ? 10 : request.PageSize);
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

        private static string Value(IDataRecord reader, string name) => reader[name] == DBNull.Value ? string.Empty : Convert.ToString(reader[name]) ?? string.Empty;
        private static DateTime? NullableDate(IDataRecord reader, string name) => reader[name] == DBNull.Value ? null : Convert.ToDateTime(reader[name]);

        private static EmployeeLookupVm MapEmployee(IDataRecord reader) => new()
        {
            EmpCode = Value(reader, "EmpCode"),
            EmpName = Value(reader, "EmpName"),
            Department = Value(reader, "Department"),
            PositionTitle = Value(reader, "PositionTitle"),
            EmailID = Value(reader, "EmailID")
        };

        private static EmployeeGrievanceListItemVm MapListItem(IDataRecord reader) => new()
        {
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            ReferenceNo = Value(reader, "ReferenceNo"),
            ComplainantEmpId = Value(reader, "ComplainantEmpId"),
            ComplainantName = Value(reader, "ComplainantName"),
            Department = Value(reader, "Department"),
            DateOfReport = NullableDate(reader, "DateOfReport"),
            GrievanceSummary = Value(reader, "GrievanceSummary"),
            Status = Value(reader, "Status"),
            HRRemarks = Value(reader, "HRRemarks"),
            CreatedOn = NullableDate(reader, "CreatedOn")
        };

        private static EmployeeGrievanceFormVm MapForm(IDataRecord reader) => new()
        {
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            ReferenceNo = Value(reader, "ReferenceNo"),
            ComplainantEmpId = Value(reader, "ComplainantEmpId"),
            ComplainantName = Value(reader, "ComplainantName"),
            Department = Value(reader, "Department"),
            PositionTitle = Value(reader, "PositionTitle"),
            DateOfReport = NullableDate(reader, "DateOfReport"),
            UnfairTreatment = ToBool(reader["UnfairTreatment"]),
            HarassmentBullying = ToBool(reader["HarassmentBullying"]),
            WorkLapses = ToBool(reader["WorkLapses"]),
            PolicySopBreach = ToBool(reader["PolicySopBreach"]),
            OshaConcern = ToBool(reader["OshaConcern"]),
            SupervisorMisconduct = ToBool(reader["SupervisorMisconduct"]),
            AbuseOfAuthority = ToBool(reader["AbuseOfAuthority"]),
            WorkingHoursIssue = ToBool(reader["WorkingHoursIssue"]),
            OtherComplaint = ToBool(reader["OtherComplaint"]),
            OtherComplaintText = Value(reader, "OtherComplaintText"),
            ConductDate = NullableDate(reader, "ConductDate"),
            ConductTime = reader["ConductTime"] == DBNull.Value ? (TimeSpan?)null : (TimeSpan)reader["ConductTime"],
            Location = Value(reader, "Location"),
            IncidentDescription = Value(reader, "IncidentDescription"),
            Witnesses = Value(reader, "Witnesses"),
            SupportingDocumentsAttached = ToBool(reader["SupportingDocumentsAttached"]),
            DesiredOutcome = Value(reader, "DesiredOutcome"),
            EmployeeSignaturePath = Value(reader, "EmployeeSignaturePath"),
            DeclarationEmployeeName = Value(reader, "DeclarationEmployeeName"),
            DeclarationEmployeeId = Value(reader, "DeclarationEmployeeId"),
            DeclarationDate = NullableDate(reader, "DeclarationDate"),
            Status = Value(reader, "Status"),
            HRRemarks = Value(reader, "HRRemarks")
        };

        private static EmployeeGrievanceNatureSelectionVm MapGrievanceNature(IDataRecord reader) => new()
        {
            GrievanceNatureID = Convert.ToInt32(reader["GrievanceNatureID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            NatureID = reader["NatureID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NatureID"]),
            NatureName = Value(reader, "NatureName"),
            IsOther = ToBool(reader["IsOther"]),
            OtherComplaintText = Value(reader, "OtherComplaintText")
        };

        private static EmployeeGrievanceInvolvedPartyVm MapParty(IDataRecord reader) => new()
        {
            PartyID = Convert.ToInt32(reader["PartyID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            EmployeeID = Value(reader, "EmployeeID"),
            EmployeeName = Value(reader, "EmployeeName"),
            PositionTitle = Value(reader, "PositionTitle"),
            Department = Value(reader, "Department")
        };

        private static EmployeeGrievanceAttachmentVm MapAttachment(IDataRecord reader) => new()
        {
            AttachmentID = Convert.ToInt32(reader["AttachmentID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            OriginalFileName = Value(reader, "OriginalFileName"),
            StoredFileName = Value(reader, "StoredFileName"),
            FilePath = Value(reader, "FilePath"),
            ContentType = Value(reader, "ContentType"),
            SizeBytes = reader["SizeBytes"] == DBNull.Value ? 0 : Convert.ToInt64(reader["SizeBytes"]),
            UploadedOn = NullableDate(reader, "UploadedOn")
        };

        private static EmployeeGrievanceHrActionVm MapHrAction(IDataRecord reader) => new()
        {
            HRActionID = Convert.ToInt32(reader["HRActionID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            HREmpId = Value(reader, "HREmpId"),
            HRName = Value(reader, "HRName"),
            ActionDate = NullableDate(reader, "ActionDate"),
            ActionEmployeeId = Value(reader, "ActionEmployeeId"),
            ActionEmployeeName = Value(reader, "ActionEmployeeName"),
            InvestigationSummary = Value(reader, "InvestigationSummary"),
            EmployeeExplanation = Value(reader, "EmployeeExplanation"),
            Remarks = Value(reader, "Remarks"),
            OutcomeResolved = ToBool(reader["OutcomeResolved"]),
            OutcomeReferredToER = ToBool(reader["OutcomeReferredToER"]),
            OutcomeReferredToDomesticInquiry = ToBool(reader["OutcomeReferredToDomesticInquiry"]),
            MinorMisconduct = ToBool(reader["MinorMisconduct"]),
            MajorMisconduct = ToBool(reader["MajorMisconduct"]),
            MajorMisconductText = Value(reader, "MajorMisconductText"),
            HRSignaturePath = Value(reader, "HRSignaturePath"),
            Department = Value(reader, "Department")
        };

        private static EmployeeGrievanceHrActionEmployeeVm MapHrActionEmployee(IDataRecord reader) => new()
        {
            HRActionEmployeeID = Convert.ToInt32(reader["HRActionEmployeeID"]),
            HRActionID = Convert.ToInt32(reader["HRActionID"]),
            GrievanceID = Convert.ToInt32(reader["GrievanceID"]),
            EmployeeID = Value(reader, "EmployeeID"),
            EmployeeName = Value(reader, "EmployeeName"),
            PositionTitle = Value(reader, "PositionTitle"),
            Department = Value(reader, "Department"),
            EmployeeSignaturePath = Value(reader, "EmployeeSignaturePath")
        };

        private static bool ToBool(object value) => value != DBNull.Value && Convert.ToBoolean(value);
    }
}
