let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'RecordNo';
let sortDirection = 'DESC';
let deleteId = 0;
let debounceTimer = null;

const inactiveEditConfirmMessage = 'you are going to edit the Inactive record, please confirm if you want to proceed?';

function isInactiveStatus(value) {
    if (value === undefined || value === null) return false;
    if (typeof value === 'boolean') return value === false;
    if (typeof value === 'number') return value === 0;

    const status = String(value).trim().toLowerCase();
    return status === 'false' ||
        status === '0' ||
        status === 'inactive' ||
        status === 'n' ||
        status === 'no';
}

function getFirstDefined() {
    for (let i = 0; i < arguments.length; i++) {
        if (arguments[i] !== undefined && arguments[i] !== null && String(arguments[i]).trim() !== '') {
            return arguments[i];
        }
    }
    return undefined;
}

function showInactiveEditConfirmModal(editModalSelector) {
    return new Promise(function (resolve) {
        const modalId = 'inactiveEditConfirmModal';
        let modal = $('#' + modalId);

        if (modal.length === 0) {
            $('body').append(`
                <div class="modal fade" id="${modalId}" tabindex="-1" role="dialog" data-backdrop="static" data-keyboard="false">
                    <div class="modal-dialog modal-dialog-centered" role="document">
                        <div class="modal-content">
                            <div class="modal-header bg-danger text-white">
                                <h5 class="modal-title">Confirm Edit</h5>
                                <button type="button" class="close text-white" id="btnInactiveEditClose" aria-label="Close">
                                    <span aria-hidden="true">&times;</span>
                                </button>
                            </div>
                            <div class="modal-body">
                                ${inactiveEditConfirmMessage}
                            </div>
                            <div class="modal-footer justify-content-end">
                                <button type="button" class="btn btn-secondary" id="btnInactiveEditCancel">Cancel</button>
                                <button type="button" class="btn btn-danger" id="btnInactiveEditProceed">Proceed</button>
                            </div>
                        </div>
                    </div>
                </div>`);
            modal = $('#' + modalId);
        }

        modal.css('z-index', 1065);
        setTimeout(function () { $('.modal-backdrop').last().css('z-index', 1060); }, 10);

        modal.off('click.inactiveEdit');
        modal.on('click.inactiveEdit', '#btnInactiveEditProceed', function () {
            modal.modal('hide');
            resolve(true);
        });
        modal.on('click.inactiveEdit', '#btnInactiveEditCancel, #btnInactiveEditClose', function () {
            modal.modal('hide');
            if (editModalSelector) {
                $(editModalSelector).modal('hide');
            }
            resolve(false);
        });

        modal.modal('show');
    });
}

async function showInactiveEditWarningAfterModal(activeStatus, editModalSelector) {
    if (isInactiveStatus(activeStatus)) {
        return await showInactiveEditConfirmModal(editModalSelector);
    }
    return true;
}

$(document).ready(function () {
    loadDepartments();
    $('#pageSizeSelect').on('change', function () { pageSize = parseInt($(this).val()); currentPage = 1; loadDepartments(); });
    $('#globalSearch').on('input', debounceSearch);
    $('.column-search').on('input', debounceSearch);
    $('#btnPrev').on('click', function () { if (currentPage > 1) { currentPage--; loadDepartments(); } });
    $('#btnNext').on('click', function () { const totalPages = Math.ceil(totalCount / pageSize); if (currentPage < totalPages) { currentPage++; loadDepartments(); } });
    $('#tblDepartment thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadDepartments();
    });

    $('#departmentForm').on('keyup change', 'input,select', function () {
        if ($.trim($(this).val()) === '') $(this).removeClass('valid-border').addClass('error-border');
        else $(this).removeClass('error-border').addClass('valid-border');
    });
    $('#departmentModal').on('hidden.bs.modal', function () {
        $('#departmentForm')[0].reset();
        $('#departmentForm').find('input,select').removeClass('error-border valid-border');
        $('#formMessage').addClass('d-none').text('');
    });

});

function debounceSearch() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(function () { currentPage = 1; loadDepartments(); }, 300);
}

function buildRequest() {
    const req = {
        globalSearch: $('#globalSearch').val(), departmentCode: '', departmentName: '', japanHead: '', office: '', gotSection: '', prefix: '',
        sortColumn: sortColumn, sortDirection: sortDirection, pageNumber: currentPage, pageSize: pageSize
    };
    $('.column-search').each(function () { req[$(this).data('field')] = $(this).val(); });
    return req;
}

async function loadDepartments() {
    try {
        const response = await $.ajax({ url: '/master/GetDepartmentList', type: 'POST', contentType: 'application/json', data: JSON.stringify(buildRequest()) });
        if (!response.success) { showFormMessage(response.message, false); return; }
        totalCount = response.totalCount;
        renderTable(response.data || []);
        updatePaging();
    } catch (e) { showFormMessage('Error loading department data', false); }
}

const departmentActiveStatusMap = {};

function renderTable(data) {
    let rows = '';

    data.forEach(item => {
        const activeStatus = getFirstDefined(item.activeStatus, item.ActiveStatus, item.isActive, item.IsActive);
        departmentActiveStatusMap[item.recordNo] = activeStatus;

        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editDepartment(${item.recordNo})"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteDepartment(${item.recordNo})"></i></td>
            <td>${escapeHtml(item.departmentCode)}</td>
            <td>${escapeHtml(item.departmentName)}</td>
            <td>${escapeHtml(item.japanHead)}</td>
            <td>${escapeHtml(item.office)}</td>
            <td>${escapeHtml(item.gotSection)}</td>
            <td>${escapeHtml(item.prefix)}</td>
            <td>${escapeHtml(item.createdBy)}</td>
            <td>${formatDate(item.createdOn)}</td>
            <td>${escapeHtml(item.editedBy)}</td>
            <td>${formatDate(item.editedOn)}</td>
            <td>${isInactiveStatus(activeStatus) ? 'Inactive' : 'Active'}</td>
        </tr>`;
    });

    $('#tblDepartment tbody').html(rows || '<tr><td colspan="13" class="text-center">No records found</td></tr>');
}


function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

function openDepartmentModal() {
    $('#departmentModalTitle').text('Add Department');
    $('#departmentForm')[0].reset();
    $('#recordNo').val(0);
    $('#departmentCode').prop('readonly', false);
    $('#formMessage').addClass('d-none').text('');
    $('#departmentForm').find('input,select').removeClass('error-border valid-border');
    $('#departmentModal').modal('show');
}

async function editDepartment(id) {
    const response = await $.get('/master/GetDepartment', { id: id });
    if (!response.success) { alert(response.message); return; }

    const d = response.data;
    const activeStatus = getFirstDefined(
        departmentActiveStatusMap[id],
        d.activeStatus,
        d.ActiveStatus,
        d.isActive,
        d.IsActive
    );

    $('#departmentModalTitle').text('Edit Department');
    $('#recordNo').val(d.recordNo);
    $('#departmentCode').val(d.departmentCode).prop('readonly', true);
    $('#departmentName').val(d.departmentName);
    $('#japanHead').val(d.japanHead);
    $('#office').val(d.office);
    $('#gotSection').val(d.gotSection);
    $('#prefix').val(d.prefix);
    $('#formMessage').addClass('d-none').text('');
    $('#departmentForm').find('input,select').removeClass('error-border valid-border');
    $('#departmentModal').modal('show');

    await showInactiveEditWarningAfterModal(activeStatus, '#departmentModal');
}


async function saveDepartmentbkp() {
    const form = $('#departmentForm')[0];
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const token = $('input[name="__RequestVerificationToken"]').val();
    const formData = $('#departmentForm').serialize();
    const response = await $.ajax({ url: '/master/SaveDepartment', type: 'POST', data: formData, headers: { 'RequestVerificationToken': token } });
    showFormMessage(response.message, response.success);
    if (response.success) { $('#departmentModal').modal('hide'); loadDepartments(); }
}

async function saveDepartment() {

    var valid = true;

    $("#departmentForm")
        .find("input[required],select[required]")
        .each(function () {

            var value = $(this).val();

            if (value == null || value.trim() == "") {

                $(this)
                    .removeClass("valid-border")
                    .addClass("error-border");

                valid = false;
            }
            else {

                $(this)
                    .removeClass("error-border")
                    .addClass("valid-border");
            }
        });

    if (!valid) {
        alert("Please enter/select all mandatory fields.");
        return;
    }

    // Existing AJAX ...
    const form = $('#departmentForm')[0];
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const token = $('input[name="__RequestVerificationToken"]').val();
    const formData = $('#departmentForm').serialize();
    const response = await $.ajax({ url: '/master/SaveDepartment', type: 'POST', data: formData, headers: { 'RequestVerificationToken': token } });
    showFormMessage(response.message, response.success);
    if (response.success) { $('#departmentModal').modal('hide'); loadDepartments(); }
}
function confirmDeleteDepartment(id) { deleteId = id; $('#deleteDepartmentModal').modal('show'); }
$('#btnConfirmDepartmentDelete').on('click', async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({ url: '/master/DeleteDepartment', type: 'POST', data: { id: deleteId }, headers: { 'RequestVerificationToken': token } });
    $('#deleteDepartmentModal').modal('hide');
    alert(response.message);
    if (response.success) loadDepartments();
});

async function exportDepartment(format) {
    const response = await fetch('/master/ExportDepartments?format=' + format, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(buildRequest()) });
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `DepartmentMaster.${format}`; document.body.appendChild(a); a.click(); a.remove();
    window.URL.revokeObjectURL(url);
}

function showFormMessage(message, success) {
    const el = $('#formMessage');
    el.removeClass('d-none alert-success alert-danger').addClass(success ? 'alert-success' : 'alert-danger').text(message);
}
function escapeHtml(value) { return $('<div>').text(value || '').html(); }
function formatDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toLocaleString(); }
