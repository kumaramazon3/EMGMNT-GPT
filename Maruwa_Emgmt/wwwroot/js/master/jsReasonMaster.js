let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'ReasonID';
let sortDirection = 'ASC';
let deleteId = '';
let debounceTimer = null;
let inactiveEditConfirmed = false;

$(document).ready(function () {
    loadReasons();
    $('#pageSizeSelect').on('change', function () { pageSize = parseInt($(this).val()); currentPage = 1; loadReasons(); });
    $('#globalSearch').on('input', debounceSearch);
    $('.column-search').on('input', debounceSearch);
    $('#btnPrev').on('click', function () { if (currentPage > 1) { currentPage--; loadReasons(); } });
    $('#btnNext').on('click', function () { const totalPages = Math.ceil(totalCount / pageSize); if (currentPage < totalPages) { currentPage++; loadReasons(); } });
    $('#tblReason thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadReasons();
    });

    $('#reasonForm').on('keyup change', 'input[required],textarea[required],select[required]', function () {
        validateControl($(this));
    });

    $('#reasonModal').on('hidden.bs.modal', function () {
        resetReasonFormValidation();
        $('#reasonForm')[0].reset();
        $('#reasonID').prop('readonly', false);
    });
});

function debounceSearch() { clearTimeout(debounceTimer); debounceTimer = setTimeout(function () { currentPage = 1; loadReasons(); }, 300); }

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
    $('#reasonModal').modal('hide');
    resetReasonFormValidation();
    $('#reasonForm')[0].reset();
    $('#reasonID').prop('readonly', false);
}

function buildRequest() {
    const req = { globalSearch: $('#globalSearch').val(), reasonID: '', reasonType: '', reasonDescription: '', createdBy: '', editedBy: '', isActive: '', sortColumn, sortDirection, pageNumber: currentPage, pageSize };
    $('.column-search').each(function () { req[$(this).data('field')] = $(this).val(); });
    return req;
}

async function loadReasons() {
    try {
        const response = await $.ajax({ url: '/master/GetReasonList', type: 'POST', contentType: 'application/json', data: JSON.stringify(buildRequest()) });
        if (!response.success) { showFormMessage(response.message, false); return; }
        totalCount = response.totalCount;
        renderTable(response.data || []);
        updatePaging();
    } catch (e) { showFormMessage('Error loading Reason data', false); }
}

function renderTable(data) {
    let rows = '';
    data.forEach(item => {
        const activeText = isInactiveStatus(item.isActive) ? 'Inactive' : 'Active';
        const activeArg = isInactiveStatus(item.isActive) ? 'false' : 'true';
        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editReason('${escapeAttr(item.reasonID)}', ${activeArg})"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteReason('${escapeAttr(item.reasonID)}')"></i></td>
            <td>${escapeHtml(item.reasonID)}</td><td>${escapeHtml(item.reasonType)}</td><td>${escapeHtml(item.reasonDescription)}</td>
            <td>${escapeHtml(item.createdBy)}</td><td>${formatDate(item.createdOn)}</td><td>${escapeHtml(item.editedBy)}</td><td>${formatDate(item.editedOn)}</td><td>${activeText}</td>
        </tr>`;
    });
    $('#tblReason tbody').html(rows || '<tr><td colspan="10" class="text-center">No records found</td></tr>');
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

function openReasonModal() {
    $('#reasonModalTitle').text('Add Reason');
    $('#reasonForm')[0].reset();
    $('#reasonID').prop('readonly', false);
    resetReasonFormValidation();
    $('#reasonModal').modal('show');
}

async function editReason(id, activeStatusFromRow) {
    const response = await $.get('/master/GetReason', { id: id });
    if (!response.success) { alert(response.message); return; }
    const d = response.data;
    $('#reasonModalTitle').text('Edit Reason');
    $('#reasonID').val(d.reasonID).prop('readonly', true);
    $('#reasonType').val(d.reasonType);
    $('#reasonDescription').val(d.reasonDescription);
    resetReasonFormValidation();
    inactiveEditConfirmed = false;
    $('#reasonModal').modal('show');

    const statusToCheck = activeStatusFromRow !== undefined ? activeStatusFromRow : (d.isActive ?? d.IsActive ?? d.activeStatus ?? d.ActiveStatus);
    showInactiveEditConfirmIfNeeded(statusToCheck);
}

async function saveReason() {
    let valid = true;
    $('#reasonForm').find('input[required],textarea[required],select[required]').each(function () {
        if (!validateControl($(this))) valid = false;
    });
    if (!valid) { alert('Please enter/select all mandatory fields.'); return; }

    const form = $('#reasonForm')[0];
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const token = $('input[name="__RequestVerificationToken"]').val();
    const formData = $('#reasonForm').serialize();
    const response = await $.ajax({ url: '/master/SaveReason', type: 'POST', data: formData, headers: { 'RequestVerificationToken': token } });
    showFormMessage(response.message, response.success);
    if (response.success) { $('#reasonModal').modal('hide'); loadReasons(); }
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

function resetReasonFormValidation() {
    $('#reasonForm').find('input,textarea,select').removeClass('error-border valid-border');
    $('#formMessage').removeClass('alert-success alert-danger').addClass('d-none').text('');
}

function confirmDeleteReason(id) { deleteId = id; $('#deleteReasonModal').modal('show'); }
$('#btnConfirmReasonDelete').on('click', async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({ url: '/master/DeleteReason', type: 'POST', data: { id: deleteId }, headers: { 'RequestVerificationToken': token } });
    $('#deleteReasonModal').modal('hide');
    alert(response.message);
    if (response.success) loadReasons();
});

async function exportReason(format) {
    const response = await fetch('/master/ExportReasons?format=' + format, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(buildRequest()) });
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `ReasonMaster.${format}`; document.body.appendChild(a); a.click(); a.remove();
    window.URL.revokeObjectURL(url);
}

function showFormMessage(message, success) { const el = $('#formMessage'); el.removeClass('d-none alert-success alert-danger').addClass(success ? 'alert-success' : 'alert-danger').text(message); }
function escapeHtml(value) { return $('<div>').text(value || '').html(); }
function escapeAttr(value) { return String(value || '').replace(/'/g, '&#39;').replace(/"/g, '&quot;'); }
function formatDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toLocaleString(); }
