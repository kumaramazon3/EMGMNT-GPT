let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'LeaveID';
let sortDirection = 'ASC';
let deleteId = '';
let debounceTimer = null;
let inactiveEditConfirmed = false;


$(document).ready(function () {
    loadLeaveTypes();
    $('#pageSizeSelect').on('change', function () { pageSize = parseInt($(this).val()); currentPage = 1; loadLeaveTypes(); });
    $('#globalSearch').on('input', debounceSearch);
    $('.column-search').on('input', debounceSearch);
    $('#btnPrev').on('click', function () { if (currentPage > 1) { currentPage--; loadLeaveTypes(); } });
    $('#btnNext').on('click', function () { const totalPages = Math.ceil(totalCount / pageSize); if (currentPage < totalPages) { currentPage++; loadLeaveTypes(); } });
    $('#tblLeaveType thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadLeaveTypes();
    });

    $('#leaveTypeForm').on('keyup change', 'input[required],textarea[required],select[required]', function () {
        validateControl($(this));
    });

    $('#leaveTypeModal').on('hidden.bs.modal', function () {
        resetLeaveTypeFormValidation();
        $('#leaveTypeForm')[0].reset();
        $('#leaveID').prop('readonly', false);
    });
});

function debounceSearch() { clearTimeout(debounceTimer); debounceTimer = setTimeout(function () { currentPage = 1; loadLeaveTypes(); }, 300); }

function isInactiveStatus(value) {
    if (value === undefined || value === null) return false;
    if (typeof value === 'boolean') return value === false;
    if (typeof value === 'number') return value === 0;
    const status = String(value).trim().toLowerCase();
    return status === 'false' || status === '0' || status === 'inactive' || status === 'n' || status === 'no';
}

function showInactiveEditConfirmIfNeeded(value) {
    if (isInactiveStatus(value)) {
        setTimeout(function () {
            $('#inactiveEditConfirmModal').modal('show');
        }, 300);
    }
}

function proceedInactiveEdit() {
    inactiveEditConfirmed = true;
    $('#inactiveEditConfirmModal').modal('hide');
}

function cancelInactiveEdit() {
    inactiveEditConfirmed = false;
    $('#inactiveEditConfirmModal').modal('hide');
    $('#leaveTypeModal').modal('hide');
    resetLeaveTypeFormValidation();
    $('#leaveTypeForm')[0].reset();
    $('#leaveID').prop('readonly', false);
}

function buildRequest() {
    const req = { globalSearch: $('#globalSearch').val(), leaveID: '', leaveType: '', createdBy: '', editedBy: '', isActive: '', sortColumn, sortDirection, pageNumber: currentPage, pageSize };
    $('.column-search').each(function () { req[$(this).data('field')] = $(this).val(); });
    return req;
}

async function loadLeaveTypes() {
    try {
        const response = await $.ajax({ url: '/master/GetLeaveTypeList', type: 'POST', contentType: 'application/json', data: JSON.stringify(buildRequest()) });
        if (!response.success) { showFormMessage(response.message, false); return; }
        totalCount = response.totalCount;
        renderTable(response.data || []);
        updatePaging();
    } catch (e) { showFormMessage('Error loading LeaveType data', false); }
}

function renderTable(data) {
    let rows = '';
    data.forEach(item => {
        const activeText = isInactiveStatus(item.isActive) ? 'Inactive' : 'Active';
        const activeArg = isInactiveStatus(item.isActive) ? 'false' : 'true';
        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editLeaveType('${escapeAttr(item.leaveID)}', ${activeArg})"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteLeaveType('${escapeAttr(item.leaveID)}')"></i></td>
            <td>${escapeHtml(item.leaveID)}</td><td>${escapeHtml(item.leaveType)}</td>
            <td>${escapeHtml(item.createdBy)}</td><td>${formatDate(item.createdOn)}</td><td>${escapeHtml(item.editedBy)}</td><td>${formatDate(item.editedOn)}</td><td>${activeText}</td>
        </tr>`;
    });
    $('#tblLeaveType tbody').html(rows || '<tr><td colspan="9" class="text-center">No records found</td></tr>');
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

function openLeaveTypeModal() {
    $('#leaveTypeModalTitle').text('Add LeaveType');
    $('#leaveTypeForm')[0].reset();
    $('#leaveID').prop('readonly', false);
    resetLeaveTypeFormValidation();
    $('#leaveTypeModal').modal('show');
}

async function editLeaveType(id, activeStatusFromRow) {
    const response = await $.get('/master/GetLeaveType', { id: id });
    if (!response.success) { alert(response.message); return; }
    const d = response.data;
    $('#leaveTypeModalTitle').text('Edit LeaveType');
    $('#leaveID').val(d.leaveID).prop('readonly', true);
    $('#leaveType').val(d.leaveType);
    resetLeaveTypeFormValidation();
    inactiveEditConfirmed = false;
    $('#leaveTypeModal').modal('show');

    const statusToCheck = activeStatusFromRow !== undefined ? activeStatusFromRow : (d.isActive ?? d.IsActive ?? d.activeStatus ?? d.ActiveStatus);
    showInactiveEditConfirmIfNeeded(statusToCheck);
}

async function saveLeaveType() {
    let valid = true;
    $('#leaveTypeForm').find('input[required],textarea[required],select[required]').each(function () {
        if (!validateControl($(this))) valid = false;
    });
    if (!valid) { alert('Please enter/select all mandatory fields.'); return; }

    const form = $('#leaveTypeForm')[0];
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const token = $('input[name="__RequestVerificationToken"]').val();
    const formData = $('#leaveTypeForm').serialize();
    const response = await $.ajax({ url: '/master/SaveLeaveType', type: 'POST', data: formData, headers: { 'RequestVerificationToken': token } });
    showFormMessage(response.message, response.success);
    if (response.success) { $('#leaveTypeModal').modal('hide'); loadLeaveTypes(); }
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

function resetLeaveTypeFormValidation() {
    $('#leaveTypeForm').find('input,textarea,select').removeClass('error-border valid-border');
    $('#formMessage').removeClass('alert-success alert-danger').addClass('d-none').text('');
}

function confirmDeleteLeaveType(id) { deleteId = id; $('#deleteLeaveTypeModal').modal('show'); }
$('#btnConfirmLeaveTypeDelete').on('click', async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({ url: '/master/DeleteLeaveType', type: 'POST', data: { id: deleteId }, headers: { 'RequestVerificationToken': token } });
    $('#deleteLeaveTypeModal').modal('hide');
    alert(response.message);
    if (response.success) loadLeaveTypes();
});

async function exportLeaveType(format) {
    const response = await fetch('/master/ExportLeaveTypes?format=' + format, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(buildRequest()) });
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `LeaveTypeMaster.${format}`; document.body.appendChild(a); a.click(); a.remove();
    window.URL.revokeObjectURL(url);
}

function showFormMessage(message, success) { const el = $('#formMessage'); el.removeClass('d-none alert-success alert-danger').addClass(success ? 'alert-success' : 'alert-danger').text(message); }
function escapeHtml(value) { return $('<div>').text(value || '').html(); }
function escapeAttr(value) { return String(value || '').replace(/'/g, '&#39;').replace(/"/g, '&quot;'); }
function formatDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toLocaleString(); }
