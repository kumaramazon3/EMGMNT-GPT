let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'CreatedOn';
let sortDirection = 'DESC';
let debounceTimer = null;
let employeeLookupList = [];
let partyLookupList = [];
let involvedParties = [];
let isViewMode = false;

$(document).ready(function () {
    loadGrievances();

    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadGrievances();
    });

    $('#globalSearch').on('input', debounceSearch);
    $('.column-search').on('input', debounceSearch);

    $('#btnPrev').on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadGrievances();
        }
    });

    $('#btnNext').on('click', function () {
        const totalPages = Math.ceil(totalCount / pageSize);
        if (currentPage < totalPages) {
            currentPage++;
            loadGrievances();
        }
    });

    $('#tblGrievance thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadGrievances();
    });

    $('#complainantEmpSearch').on('input', function () {
        debounceLookup(() => searchEmployees($('#complainantEmpSearch').val(), 'employeeOptions', 'complainant'));
    });

    $('#complainantEmpSearch').on('change', function () {
        const empCode = extractEmployeeCode($(this).val());
        if (empCode) fillComplainantByCode(empCode);
    });

    $('#partyEmpSearch').on('input', function () {
        debounceLookup(() => searchEmployees($('#partyEmpSearch').val(), 'partyEmployeeOptions', 'party'));
    });

    $('#partyEmpSearch').on('change', function () {
        const empCode = extractEmployeeCode($(this).val());
        if (empCode) fillPartyByCode(empCode);
    });

    $('#supportDocsYes').on('change', function () {
        $('#supportingFiles').show();
    });

    $('#supportDocsNo').on('change', function () {
        $('#supportingFiles').hide().val('');
        $('#supportingFilePreview').empty();
    });

    $('#supportingFiles').on('change', function () {
        previewSupportingFiles(this.files);
    });

    $('#employeeSignatureFile').on('change', function () {
        previewSignature(this.files && this.files.length ? this.files[0] : null);
    });

    $('#grievanceForm').on('keyup change', 'input[required],textarea[required],select[required]', function () {
        validateControl($(this));
    });

    $('#grievanceModal').on('hidden.bs.modal', resetGrievanceForm);
});

function debounceSearch() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(function () {
        currentPage = 1;
        loadGrievances();
    }, 300);
}

function debounceLookup(callback) {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(callback, 250);
}

function buildRequest() {
    const req = {
        globalSearch: $('#globalSearch').val(),
        referenceNo: '',
        status: '',
        sortColumn: sortColumn,
        sortDirection: sortDirection,
        pageNumber: currentPage,
        pageSize: pageSize
    };

    $('.column-search').each(function () {
        req[$(this).data('field')] = $(this).val();
    });

    return req;
}

async function loadGrievances() {
    try {
        const response = await $.ajax({
            url: '/ERHRLetters/GetMyGrievances',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(buildRequest())
        });

        if (!response.success) {
            alert(response.message || 'Error loading employee grievance forms');
            return;
        }

        totalCount = response.totalCount || 0;
        renderGrievanceTable(response.data || []);
        updatePaging();
    } catch (e) {
        alert('Error loading employee grievance forms');
    }
}

function renderGrievanceTable(data) {
    let rows = '';
    data.forEach(item => {
        const id = getFirstDefined(item.grievanceID, item.GrievanceID);
        rows += `<tr>
            <td><i class="bi bi-eye text-primary" style="cursor:pointer" title="View" onclick="viewGrievance(${id})"></i></td>
            <td>${escapeHtml(getFirstDefined(item.referenceNo, item.ReferenceNo))}</td>
            <td>${formatDate(getFirstDefined(item.dateOfReport, item.DateOfReport))}</td>
            <td>${escapeHtml(getFirstDefined(item.complainantEmpId, item.ComplainantEmpId))}</td>
            <td>${escapeHtml(getFirstDefined(item.complainantName, item.ComplainantName))}</td>
            <td>${escapeHtml(getFirstDefined(item.department, item.Department))}</td>
            <td>${escapeHtml(getFirstDefined(item.grievanceSummary, item.GrievanceSummary))}</td>
            <td>${escapeHtml(getFirstDefined(item.status, item.Status))}</td>
            <td>${formatDate(getFirstDefined(item.createdOn, item.CreatedOn))}</td>
        </tr>`;
    });

    $('#tblGrievance tbody').html(rows || '<tr><td colspan="9" class="text-center">No records found</td></tr>');
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

async function openNewGrievanceForm() {
    resetGrievanceForm();
    isViewMode = false;
    $('#grievanceModal').removeClass('modal-view-mode');
    $('#grievanceModalTitle').text('Raise a New Complaint');
    setReadonlyMode(false);
    setTodayFields();
    $('#referenceNo').val('Auto-generated after submit');
    await loadLoggedInEmployee();
    $('#grievanceModal').modal('show');
}

async function viewGrievance(id) {
    resetGrievanceForm();
    isViewMode = true;
    $('#grievanceModal').addClass('modal-view-mode');
    $('#grievanceModalTitle').text('View Employee Grievance Form');

    const response = await $.get('/ERHRLetters/GetGrievanceForm', { id: id });
    if (!response.success) {
        alert(response.message || 'Unable to load grievance form');
        return;
    }

    fillForm(response.data);
    setReadonlyMode(true);
    $('#grievanceModal').modal('show');
}

async function loadLoggedInEmployee() {
    const response = await $.get('/ERHRLetters/GetLoggedInEmployee');
    if (response.success && response.data) {
        fillComplainant(response.data);
    }
}

async function searchEmployees(searchText, datalistId, target) {
    const response = await $.get('/ERHRLetters/SearchEmployeeLookup', { searchText: searchText || '' });
    const data = response.success ? (response.data || []) : [];
    if (target === 'party') partyLookupList = data;
    else employeeLookupList = data;

    const list = $('#' + datalistId);
    list.empty();
    data.forEach(e => {
        const empCode = getFirstDefined(e.empCode, e.EmpCode);
        const empName = getFirstDefined(e.empName, e.EmpName);
        list.append(`<option value="${escapeAttr(empCode + ' - ' + empName)}"></option>`);
    });
}

function extractEmployeeCode(displayText) {
    const text = String(displayText || '').trim();
    if (!text) return '';
    return text.split(' - ')[0].trim();
}

async function fillComplainantByCode(empCode) {
    const response = await $.get('/ERHRLetters/GetEmployeeByCode', { empCode });
    if (response.success) fillComplainant(response.data);
    else alert(response.message || 'Employee not found');
}

function fillComplainant(emp) {
    const empCode = getFirstDefined(emp.empCode, emp.EmpCode);
    const empName = getFirstDefined(emp.empName, emp.EmpName);
    $('#complainantEmpSearch').val(empCode + (empName ? ' - ' + empName : ''));
    $('#complainantEmpId').val(empCode);
    $('#complainantName').val(empName);
    $('#department').val(getFirstDefined(emp.department, emp.Department));
    $('#positionTitle').val(getFirstDefined(emp.positionTitle, emp.PositionTitle));
    $('#declarationEmployee').val(empName + (empCode ? ' - ' + empCode : ''));
}

async function fillPartyByCode(empCode) {
    const response = await $.get('/ERHRLetters/GetEmployeeByCode', { empCode });
    if (!response.success) {
        alert(response.message || 'Employee not found');
        return;
    }
    const emp = response.data;
    const code = getFirstDefined(emp.empCode, emp.EmpCode);
    const name = getFirstDefined(emp.empName, emp.EmpName);
    $('#partyEmpSearch').val(code + (name ? ' - ' + name : ''));
    $('#partyEmployeeName').val(name);
    $('#partyPositionTitle').val(getFirstDefined(emp.positionTitle, emp.PositionTitle));
    $('#partyDepartment').val(getFirstDefined(emp.department, emp.Department));
}

function addInvolvedParty() {
    const employeeID = extractEmployeeCode($('#partyEmpSearch').val());
    if (!employeeID) {
        alert('Please select/key in employee ID.');
        return;
    }
    if (involvedParties.some(x => String(x.employeeID).toLowerCase() === employeeID.toLowerCase())) {
        alert('This employee is already added.');
        return;
    }

    involvedParties.push({
        employeeID: employeeID,
        employeeName: $('#partyEmployeeName').val(),
        positionTitle: $('#partyPositionTitle').val(),
        department: $('#partyDepartment').val()
    });
    renderInvolvedParties();
    $('#partyEmpSearch,#partyEmployeeName,#partyPositionTitle,#partyDepartment').val('');
}

function removeInvolvedParty(index) {
    involvedParties.splice(index, 1);
    renderInvolvedParties();
}

function renderInvolvedParties() {
    let rows = '';
    involvedParties.forEach((p, index) => {
        rows += `<tr>
            <td>${escapeHtml(getFirstDefined(p.employeeID, p.EmployeeID))}</td>
            <td>${escapeHtml(getFirstDefined(p.employeeName, p.EmployeeName))}</td>
            <td>${escapeHtml(getFirstDefined(p.positionTitle, p.PositionTitle))}</td>
            <td>${escapeHtml(getFirstDefined(p.department, p.Department))}</td>
            <td class="save-only"><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="removeInvolvedParty(${index})"></i></td>
        </tr>`;
    });
    $('#tblInvolvedParties tbody').html(rows || '<tr><td colspan="5" class="text-center">No involved persons added</td></tr>');
}

function collectFormData() {
    const formData = new FormData();
    formData.append('GrievanceID', $('#grievanceID').val() || 0);
    formData.append('ComplainantEmpId', $('#complainantEmpId').val());
    formData.append('ComplainantName', $('#complainantName').val());
    formData.append('Department', $('#department').val());
    formData.append('PositionTitle', $('#positionTitle').val());
    formData.append('ReferenceNo', $('#referenceNo').val() === 'Auto-generated after submit' ? '' : $('#referenceNo').val());
    formData.append('UnfairTreatment', $('#unfairTreatment').is(':checked'));
    formData.append('HarassmentBullying', $('#harassmentBullying').is(':checked'));
    formData.append('WorkLapses', $('#workLapses').is(':checked'));
    formData.append('PolicySopBreach', $('#policySopBreach').is(':checked'));
    formData.append('OshaConcern', $('#oshaConcern').is(':checked'));
    formData.append('SupervisorMisconduct', $('#supervisorMisconduct').is(':checked'));
    formData.append('AbuseOfAuthority', $('#abuseOfAuthority').is(':checked'));
    formData.append('WorkingHoursIssue', $('#workingHoursIssue').is(':checked'));
    formData.append('OtherComplaint', $('#otherComplaint').is(':checked'));
    formData.append('OtherComplaintText', $('#otherComplaintText').val());
    formData.append('ConductDate', $('#conductDate').val());
    formData.append('ConductTime', $('#conductTime').val());
    formData.append('Location', $('#location').val());
    formData.append('IncidentDescription', $('#incidentDescription').val());
    formData.append('Witnesses', $('#witnesses').val());
    formData.append('SupportingDocumentsAttached', $('#supportDocsYes').is(':checked'));
    formData.append('DesiredOutcome', $('#desiredOutcome').val());
    formData.append('involvedPartiesJson', JSON.stringify(involvedParties));

    const signatureFile = $('#employeeSignatureFile')[0].files[0];
    if (signatureFile) formData.append('employeeSignatureFile', signatureFile);

    const files = $('#supportingFiles')[0].files;
    for (let i = 0; i < files.length; i++) formData.append('supportingFiles', files[i]);

    return formData;
}

async function saveEmployeeGrievance() {
    if (!validateGrievanceForm()) return;

    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({
        url: '/ERHRLetters/SaveEmployeeGrievance',
        type: 'POST',
        data: collectFormData(),
        processData: false,
        contentType: false,
        headers: { 'RequestVerificationToken': token }
    });

    if (response.success) {
        alert(response.message + (response.referenceNo ? '\nReference No: ' + response.referenceNo : ''));
        $('#grievanceModal').modal('hide');
        loadGrievances();
    } else {
        showFormMessage(response.message || 'Unable to submit Employee Grievance Form', false);
    }
}

function validateGrievanceForm() {
    let valid = true;

    if (!$('#complainantEmpId').val()) {
        $('#complainantEmpSearch').removeClass('valid-border').addClass('error-border');
        valid = false;
    } else {
        $('#complainantEmpSearch').removeClass('error-border').addClass('valid-border');
    }

    const hasNature = $('#unfairTreatment,#harassmentBullying,#workLapses,#policySopBreach,#oshaConcern,#supervisorMisconduct,#abuseOfAuthority,#workingHoursIssue,#otherComplaint').filter(':checked').length > 0;
    if (!hasNature) {
        alert('Please select at least one Nature of Grievance / Complaint.');
        return false;
    }

    $('#grievanceForm').find('textarea[required],input[required]').each(function () {
        if (!validateControl($(this))) valid = false;
    });

    if (!valid) {
        alert('Please enter/select all mandatory fields.');
        return false;
    }
    return true;
}

function fillForm(data) {
    $('#grievanceID').val(getFirstDefined(data.grievanceID, data.GrievanceID));
    $('#referenceNo').val(getFirstDefined(data.referenceNo, data.ReferenceNo));

    const empCode = getFirstDefined(data.complainantEmpId, data.ComplainantEmpId);
    const empName = getFirstDefined(data.complainantName, data.ComplainantName);
    $('#complainantEmpSearch').val(empCode + (empName ? ' - ' + empName : ''));
    $('#complainantEmpId').val(empCode);
    $('#complainantName').val(empName);
    $('#department').val(getFirstDefined(data.department, data.Department));
    $('#positionTitle').val(getFirstDefined(data.positionTitle, data.PositionTitle));
    $('#dateOfReportDisplay').val(formatDateOnly(getFirstDefined(data.dateOfReport, data.DateOfReport)));

    setChecked('unfairTreatment', getFirstDefined(data.unfairTreatment, data.UnfairTreatment));
    setChecked('harassmentBullying', getFirstDefined(data.harassmentBullying, data.HarassmentBullying));
    setChecked('workLapses', getFirstDefined(data.workLapses, data.WorkLapses));
    setChecked('policySopBreach', getFirstDefined(data.policySopBreach, data.PolicySopBreach));
    setChecked('oshaConcern', getFirstDefined(data.oshaConcern, data.OshaConcern));
    setChecked('supervisorMisconduct', getFirstDefined(data.supervisorMisconduct, data.SupervisorMisconduct));
    setChecked('abuseOfAuthority', getFirstDefined(data.abuseOfAuthority, data.AbuseOfAuthority));
    setChecked('workingHoursIssue', getFirstDefined(data.workingHoursIssue, data.WorkingHoursIssue));
    setChecked('otherComplaint', getFirstDefined(data.otherComplaint, data.OtherComplaint));
    $('#otherComplaintText').val(getFirstDefined(data.otherComplaintText, data.OtherComplaintText));

    $('#conductDate').val(formatInputDate(getFirstDefined(data.conductDate, data.ConductDate)));
    $('#conductTime').val(formatInputTime(getFirstDefined(data.conductTime, data.ConductTime)));
    $('#location').val(getFirstDefined(data.location, data.Location));
    $('#incidentDescription').val(getFirstDefined(data.incidentDescription, data.IncidentDescription));
    $('#witnesses').val(getFirstDefined(data.witnesses, data.Witnesses));
    $('#desiredOutcome').val(getFirstDefined(data.desiredOutcome, data.DesiredOutcome));

    const supporting = toBool(getFirstDefined(data.supportingDocumentsAttached, data.SupportingDocumentsAttached));
    $('#supportDocsYes').prop('checked', supporting);
    $('#supportDocsNo').prop('checked', !supporting);
    $('#supportingFiles').toggle(!isViewMode && supporting);

    involvedParties = (data.involvedParties || data.InvolvedParties || []).map(p => ({
        employeeID: getFirstDefined(p.employeeID, p.EmployeeID),
        employeeName: getFirstDefined(p.employeeName, p.EmployeeName),
        positionTitle: getFirstDefined(p.positionTitle, p.PositionTitle),
        department: getFirstDefined(p.department, p.Department)
    }));
    renderInvolvedParties();

    const signaturePath = getFirstDefined(data.employeeSignaturePath, data.EmployeeSignaturePath);
    if (signaturePath) $('#signaturePreview').html(`<a href="${escapeAttr(signaturePath)}" target="_blank"><img src="${escapeAttr(signaturePath)}" class="signature-preview" /></a>`);

    const declarationName = getFirstDefined(data.declarationEmployeeName, data.DeclarationEmployeeName, empName);
    const declarationEmpId = getFirstDefined(data.declarationEmployeeId, data.DeclarationEmployeeId, empCode);
    $('#declarationEmployee').val(declarationName + (declarationEmpId ? ' - ' + declarationEmpId : ''));
    $('#declarationDateDisplay').val(formatDate(getFirstDefined(data.declarationDate, data.DeclarationDate)));

    renderAttachments(data.attachments || data.Attachments || []);
}

function renderAttachments(attachments) {
    if (!attachments || attachments.length === 0) {
        $('#supportingFilePreview').html('');
        return;
    }
    const html = attachments.map(a => {
        const path = getFirstDefined(a.filePath, a.FilePath);
        const name = getFirstDefined(a.originalFileName, a.OriginalFileName);
        return `<div><a href="${escapeAttr(path)}" target="_blank">${escapeHtml(name)}</a></div>`;
    }).join('');
    $('#supportingFilePreview').html(html);
}

function previewSupportingFiles(files) {
    if (!files || files.length === 0) {
        $('#supportingFilePreview').empty();
        return;
    }
    let html = '';
    for (let i = 0; i < files.length; i++) html += `<div>${escapeHtml(files[i].name)}</div>`;
    $('#supportingFilePreview').html(html);
}

function previewSignature(file) {
    $('#signaturePreview').empty();
    if (!file) return;
    if (!file.type.startsWith('image/')) {
        $('#signaturePreview').text(file.name);
        return;
    }
    const reader = new FileReader();
    reader.onload = function (e) {
        $('#signaturePreview').html(`<img src="${e.target.result}" class="signature-preview" />`);
    };
    reader.readAsDataURL(file);
}

function setReadonlyMode(readonly) {
    $('#grievanceForm input, #grievanceForm textarea, #grievanceForm select').prop('disabled', readonly);
    $('#grievanceForm input[type="hidden"]').prop('disabled', false);
    $('#grievanceForm input[readonly]').prop('disabled', false).prop('readonly', true);
    $('#complainantName,#department,#positionTitle,#dateOfReportDisplay,#referenceNo,#declarationEmployee,#declarationDateDisplay').prop('disabled', false).prop('readonly', true);
}

function resetGrievanceForm() {
    $('#grievanceForm')[0].reset();
    $('#grievanceID').val(0);
    $('#referenceNo').val('');
    $('#complainantEmpId').val('');
    $('#formMessage').removeClass('alert-success alert-danger').addClass('d-none').text('');
    $('#grievanceForm').find('input,textarea,select').removeClass('error-border valid-border').prop('disabled', false);
    $('#complainantName,#department,#positionTitle,#dateOfReportDisplay,#referenceNo,#declarationEmployee,#declarationDateDisplay').prop('readonly', true);
    involvedParties = [];
    renderInvolvedParties();
    $('#signaturePreview,#supportingFilePreview').empty();
    $('#supportingFiles').hide().val('');
    $('#grievanceModal').removeClass('modal-view-mode');
    isViewMode = false;
}

function setTodayFields() {
    const now = new Date();
    $('#dateOfReportDisplay').val(now.toLocaleDateString());
    $('#declarationDateDisplay').val(now.toLocaleString());
}

function validateControl(control) {
    const value = control.val();
    if (value == null || String(value).trim() === '') {
        control.removeClass('valid-border').addClass('error-border');
        return false;
    }
    control.removeClass('error-border').addClass('valid-border');
    return true;
}

function showFormMessage(message, success) {
    $('#formMessage').removeClass('d-none alert-success alert-danger').addClass(success ? 'alert-success' : 'alert-danger').text(message);
}

function getFirstDefined() {
    for (let i = 0; i < arguments.length; i++) {
        if (arguments[i] !== undefined && arguments[i] !== null && String(arguments[i]).trim() !== '') return arguments[i];
    }
    return '';
}

function setChecked(id, value) { $('#' + id).prop('checked', toBool(value)); }
function toBool(value) { return value === true || value === 1 || String(value).toLowerCase() === 'true' || String(value) === '1'; }
function escapeHtml(value) { return $('<div>').text(value || '').html(); }
function escapeAttr(value) { return String(value || '').replace(/'/g, '&#39;').replace(/"/g, '&quot;'); }
function formatDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? String(value) : d.toLocaleString(); }
function formatDateOnly(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? String(value) : d.toLocaleDateString(); }
function formatInputDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toISOString().substring(0, 10); }
function formatInputTime(value) {
    if (!value) return '';
    const text = String(value);
    if (/^\d{2}:\d{2}/.test(text)) return text.substring(0, 8);
    const d = new Date(value);
    return isNaN(d) ? '' : d.toTimeString().substring(0, 8);
}
