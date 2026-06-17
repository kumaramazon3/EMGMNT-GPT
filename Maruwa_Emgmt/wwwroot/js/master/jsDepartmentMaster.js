let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'RecordNo';
let sortDirection = 'DESC';
let deleteId = 0;
let debounceTimer = null;

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

function renderTable(data) {
    let rows = '';
    data.forEach(item => {
        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editDepartment(${item.recordNo})"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteDepartment(${item.recordNo})"></i></td>
            <td>${escapeHtml(item.departmentCode)}</td><td>${escapeHtml(item.departmentName)}</td><td>${escapeHtml(item.japanHead)}</td><td>${escapeHtml(item.office)}</td><td>${escapeHtml(item.gotSection)}</td><td>${escapeHtml(item.prefix)}</td>
            <td>${escapeHtml(item.createdBy)}</td><td>${formatDate(item.createdOn)}</td><td>${escapeHtml(item.editedBy)}</td><td>${formatDate(item.editedOn)}</td><td>${item.activeStatus ? 'Active' : 'Inactive'}</td>
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
    $('#departmentModal').modal('show');
}

async function editDepartment(id) {
    const response = await $.get('/master/GetDepartment', { id: id });
    if (!response.success) { alert(response.message); return; }
    const d = response.data;
    $('#departmentModalTitle').text('Edit Department');
    $('#recordNo').val(d.recordNo); $('#departmentCode').val(d.departmentCode).prop('readonly', true); $('#departmentName').val(d.departmentName);
    $('#japanHead').val(d.japanHead); $('#office').val(d.office); $('#gotSection').val(d.gotSection); $('#prefix').val(d.prefix);
    $('#formMessage').addClass('d-none').text('');
    $('#departmentModal').modal('show');
}

async function saveDepartment() {
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
