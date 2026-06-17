let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'Sno';
let sortDirection = 'DESC';
let deleteId = 0;
let debounceTimer = null;

$(document).ready(function () {
    loadInsuranceCategories('');
    loadProbations('');
    loadDesignations();
    $('#pageSizeSelect').on('change', function () { pageSize = parseInt($(this).val()); currentPage = 1; loadDesignations(); });
    $('#globalSearch').on('input', debounceSearch);
    $('#insuranceCategorySearch').on('input', function () { debounceLookup(() => loadInsuranceCategories($('#insuranceCategorySearch').val())); });
    $('#probationSearch').on('input', function () { debounceLookup(() => loadProbations($('#probationSearch').val())); });
    $('.column-search').on('input', debounceSearch);
    $('#btnPrev').on('click', function () { if (currentPage > 1) { currentPage--; loadDesignations(); } });
    $('#btnNext').on('click', function () { const totalPages = Math.ceil(totalCount / pageSize); if (currentPage < totalPages) { currentPage++; loadDesignations(); } });
    $('#tblDesignation thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadDesignations();
    });
});

function debounceSearch() { clearTimeout(debounceTimer); debounceTimer = setTimeout(function () { currentPage = 1; loadDesignations(); }, 300); }
function debounceLookup(callback) { clearTimeout(debounceTimer); debounceTimer = setTimeout(callback, 300); }

function buildRequest() {
    const req = { globalSearch: $('#globalSearch').val(), designationcode: '', designationName: '', probation: '', insCatergory: '', insamount: '', createdBy: '', editedBy: '', isActive: '', sortColumn, sortDirection, pageNumber: currentPage, pageSize };
    $('.column-search').each(function () { req[$(this).data('field')] = $(this).val(); });
    return req;
}

async function loadDesignations() {
    try {
        const response = await $.ajax({ url: '/master/GetDesignationList', type: 'POST', contentType: 'application/json', data: JSON.stringify(buildRequest()) });
        if (!response.success) { showFormMessage(response.message, false); return; }
        totalCount = response.totalCount;
        renderTable(response.data || []);
        updatePaging();
    } catch (e) { showFormMessage('Error loading designation data', false); }
}

async function loadInsuranceCategories(searchText, selectedValue) {
    const response = await $.get('/master/GetInsuranceCategoryLookup', { searchText: searchText || '' });
    const ddl = $('#insCatergory');
    ddl.empty().append('<option value="">--Select Insurance Category--</option>');
    if (response.success) (response.data || []).forEach(d => ddl.append(`<option value="${escapeHtml(d.categoryCode)}">${escapeHtml(d.categoryCode)} - ${escapeHtml(d.categoryName)}</option>`));
    if (selectedValue) ddl.val(selectedValue);
}

async function loadProbations(searchText, selectedValue) {
    const response = await $.get('/master/GetProbationLookup', { searchText: searchText || '' });
    const ddl = $('#probation');
    ddl.empty().append('<option value="">--Select Probation--</option>');
    if (response.success) (response.data || []).forEach(d => ddl.append(`<option value="${escapeHtml(d.probationCode)}">${escapeHtml(d.probationCode)} - ${escapeHtml(d.probationName)}</option>`));
    if (selectedValue) ddl.val(selectedValue);
}

function renderTable(data) {
    let rows = '';
    data.forEach(item => {
        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editDesignation(${item.sno})"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteDesignation(${item.sno})"></i></td>
            <td>${escapeHtml(item.designationcode)}</td><td>${escapeHtml(item.designationName)}</td><td>${escapeHtml(item.probation)}</td><td>${escapeHtml(item.insCatergory)}</td><td>${escapeHtml(item.insamount)}</td>
            <td>${escapeHtml(item.createdBy)}</td><td>${formatDate(item.createdOn)}</td><td>${escapeHtml(item.editedBy)}</td><td>${formatDate(item.editedOn)}</td><td>${item.isActive ? 'Active' : 'Inactive'}</td>
        </tr>`;
    });
    $('#tblDesignation tbody').html(rows || '<tr><td colspan="12" class="text-center">No records found</td></tr>');
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

function openDesignationModal() {
    $('#designationModalTitle').text('Add Designation');
    $('#designationForm')[0].reset();
    $('#sno').val(0);
    $('#designationcode').prop('readonly', false);
    $('#formMessage').addClass('d-none').text('');
    loadInsuranceCategories('');
    loadProbations('');
    $('#designationModal').modal('show');
}

async function editDesignation(id) {
    const response = await $.get('/master/GetDesignation', { id });
    if (!response.success) { alert(response.message); return; }
    const d = response.data;
    $('#designationModalTitle').text('Edit Designation');
    $('#sno').val(d.sno); $('#designationcode').val(d.designationcode).prop('readonly', true); $('#designationName').val(d.designationName);
    $('#insamount').val(d.insamount);
    await loadInsuranceCategories(d.insCatergory, d.insCatergory);
    await loadProbations(d.probation, d.probation);
    $('#formMessage').addClass('d-none').text('');
    $('#designationModal').modal('show');
}

async function saveDesignation() {
    const form = $('#designationForm')[0];
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({ url: '/master/SaveDesignation', type: 'POST', data: $('#designationForm').serialize(), headers: { 'RequestVerificationToken': token } });
    showFormMessage(response.message, response.success);
    if (response.success) { $('#designationModal').modal('hide'); loadDesignations(); }
}

function confirmDeleteDesignation(id) { deleteId = id; $('#deleteDesignationModal').modal('show'); }
$('#btnConfirmDesignationDelete').on('click', async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({ url: '/master/DeleteDesignation', type: 'POST', data: { id: deleteId }, headers: { 'RequestVerificationToken': token } });
    $('#deleteDesignationModal').modal('hide');
    alert(response.message);
    if (response.success) loadDesignations();
});

async function exportDesignation(format) {
    const response = await fetch('/master/ExportDesignations?format=' + format, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(buildRequest()) });
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `DesignationMaster.${format}`; document.body.appendChild(a); a.click(); a.remove();
    window.URL.revokeObjectURL(url);
}

function showFormMessage(message, success) { const el = $('#formMessage'); el.removeClass('d-none alert-success alert-danger').addClass(success ? 'alert-success' : 'alert-danger').text(message); }
function escapeHtml(value) { return $('<div>').text(value ?? '').html(); }
function formatDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toLocaleString(); }
