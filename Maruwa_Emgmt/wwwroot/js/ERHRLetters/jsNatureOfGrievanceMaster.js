let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'NatureName';
let sortDirection = 'ASC';
let deleteId = 0;
let debounceTimer = null;
let inactiveEditConfirmed = false;

$(document).ready(function () {
    loadNatures();

    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadNatures();
    });

    $('#globalSearch').on('input', debounceSearch);
    $('.column-search').on('input', debounceSearch);

    $('#btnPrev').on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadNatures();
        }
    });

    $('#btnNext').on('click', function () {
        const totalPages = Math.ceil(totalCount / pageSize);
        if (currentPage < totalPages) {
            currentPage++;
            loadNatures();
        }
    });

    $('#tblNature thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadNatures();
    });

    $('#natureForm').on('keyup change', 'input[required],textarea[required],select[required]', function () {
        validateControl($(this));
    });

    $('#natureModal').on('hidden.bs.modal', function () {
        resetNatureFormValidation();
        $('#natureForm')[0].reset();
        $('#natureID').val(0);
    });
});

function debounceSearch() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(function () {
        currentPage = 1;
        loadNatures();
    }, 300);
}

function getFirstDefined() {
    for (let i = 0; i < arguments.length; i++) {
        if (arguments[i] !== undefined && arguments[i] !== null && String(arguments[i]).trim() !== '') return arguments[i];
    }
    return '';
}

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
    $('#natureModal').modal('hide');
    resetNatureFormValidation();
    $('#natureForm')[0].reset();
    $('#natureID').val(0);
}

function buildRequest() {
    const req = {
        globalSearch: $('#globalSearch').val(),
        natureName: '',
        isOther: '',
        createdBy: '',
        editedBy: '',
        isActive: '',
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

async function loadNatures() {
    try {
        const response = await $.ajax({
            url: '/ERHRLetters/GetNatureOfGrievanceList',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(buildRequest())
        });

        if (!response.success) {
            showFormMessage(response.message, false);
            return;
        }

        totalCount = response.totalCount;
        renderTable(response.data || []);
        updatePaging();
    } catch (e) {
        showFormMessage('Error loading Nature of Grievance data', false);
    }
}

function renderTable(data) {
    let rows = '';

    data.forEach(item => {
        const natureId = getFirstDefined(item.natureID, item.NatureID);
        const natureName = getFirstDefined(item.natureName, item.NatureName);
        const isOther = getFirstDefined(item.isOther, item.IsOther);
        const createdBy = getFirstDefined(item.createdBy, item.CreatedBy);
        const createdOn = getFirstDefined(item.createdOn, item.CreatedOn);
        const editedBy = getFirstDefined(item.editedBy, item.EditedBy);
        const editedOn = getFirstDefined(item.editedOn, item.EditedOn);
        const isActive = getFirstDefined(item.isActive, item.IsActive);
        const activeText = isInactiveStatus(isActive) ? 'Inactive' : 'Active';
        const activeArg = isInactiveStatus(isActive) ? 'false' : 'true';

        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editNature(${natureId}, ${activeArg})"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteNature(${natureId})"></i></td>
            <td>${escapeHtml(natureName)}</td>
            <td>${toBool(isOther) ? 'Yes' : 'No'}</td>
            <td>${escapeHtml(createdBy)}</td>
            <td>${formatDate(createdOn)}</td>
            <td>${escapeHtml(editedBy)}</td>
            <td>${formatDate(editedOn)}</td>
            <td>${activeText}</td>
        </tr>`;
    });

    $('#tblNature tbody').html(rows || '<tr><td colspan="9" class="text-center">No records found</td></tr>');
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

function openNatureModal() {
    $('#natureModalTitle').text('Add Nature of Grievance');
    $('#natureForm')[0].reset();
    $('#natureID').val(0);
    resetNatureFormValidation();
    $('#natureModal').modal('show');
}

async function editNature(id, activeStatusFromRow) {
    const response = await $.get('/ERHRLetters/GetNatureOfGrievance', { id: id });
    if (!response.success) {
        alert(response.message);
        return;
    }

    const d = response.data;
    $('#natureModalTitle').text('Edit Nature of Grievance');
    $('#natureID').val(getFirstDefined(d.natureID, d.NatureID, id));
    $('#natureName').val(getFirstDefined(d.natureName, d.NatureName));
    $('#isOther').prop('checked', toBool(getFirstDefined(d.isOther, d.IsOther)));

    resetNatureFormValidation();
    inactiveEditConfirmed = false;
    $('#natureModal').modal('show');

    const statusToCheck = activeStatusFromRow !== undefined
        ? activeStatusFromRow
        : getFirstDefined(d.isActive, d.IsActive, d.activeStatus, d.ActiveStatus);

    showInactiveEditConfirmIfNeeded(statusToCheck);
}

async function saveNature() {
    let valid = true;

    $('#natureForm').find('input[required],textarea[required],select[required]').each(function () {
        if (!validateControl($(this))) valid = false;
    });

    if (!valid) {
        alert('Please enter/select all mandatory fields.');
        return;
    }

    const form = $('#natureForm')[0];
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const formData = $('#natureForm').serialize();

    const response = await $.ajax({
        url: '/ERHRLetters/SaveNatureOfGrievance',
        type: 'POST',
        data: formData,
        headers: { 'RequestVerificationToken': token }
    });

    showFormMessage(response.message, response.success);

    if (response.success) {
        $('#natureModal').modal('hide');
        loadNatures();
    }
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

function resetNatureFormValidation() {
    $('#natureForm').find('input,textarea,select').removeClass('error-border valid-border');
    $('#formMessage').removeClass('alert-success alert-danger').addClass('d-none').text('');
}

function confirmDeleteNature(id) {
    deleteId = id;
    $('#deleteNatureModal').modal('show');
}

$('#btnConfirmNatureDelete').on('click', async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();

    const response = await $.ajax({
        url: '/ERHRLetters/DeleteNatureOfGrievance',
        type: 'POST',
        data: { id: deleteId },
        headers: { 'RequestVerificationToken': token }
    });

    $('#deleteNatureModal').modal('hide');
    alert(response.message);

    if (response.success) loadNatures();
});

async function exportNature(format) {
    const response = await fetch('/ERHRLetters/ExportNatureOfGrievances?format=' + format, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(buildRequest())
    });

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `NatureOfGrievanceMaster.${format}`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
}

function showFormMessage(message, success) {
    const el = $('#formMessage');
    el.removeClass('d-none alert-success alert-danger')
        .addClass(success ? 'alert-success' : 'alert-danger')
        .text(message);
}

function toBool(value) { return value === true || value === 1 || String(value).toLowerCase() === 'true' || String(value) === '1'; }
function escapeHtml(value) { return $('<div>').text(value ?? '').html(); }
function escapeAttr(value) { return String(value ?? '').replace(/'/g, '&#39;').replace(/"/g, '&quot;'); }
function formatDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toLocaleString(); }
