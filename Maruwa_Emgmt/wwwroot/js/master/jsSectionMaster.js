let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'SectionId';
let sortDirection = 'DESC';
let deleteId = 0;
let debounceTimer = null;

$(document).ready(function () {
    loadDepartmentsForDropdown('');
    loadSections();
    $('#pageSizeSelect').on('change', function () { pageSize = parseInt($(this).val()); currentPage = 1; loadSections(); });
    $('#globalSearch').on('input', debounceSearch);
    $('#departmentManualSearch').on('input', function () { debounceDepartmentSearch(); currentPage = 1; loadSections(); });
    $('.column-search').on('input', debounceSearch);
    $('#btnPrev').on('click', function () { if (currentPage > 1) { currentPage--; loadSections(); } });
    $('#btnNext').on('click', function () { const totalPages = Math.ceil(totalCount / pageSize); if (currentPage < totalPages) { currentPage++; loadSections(); } });
    $('#tblSection thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadSections();
    });
});

function debounceSearch() { clearTimeout(debounceTimer); debounceTimer = setTimeout(function () { currentPage = 1; loadSections(); }, 300); }
function debounceDepartmentSearch() { clearTimeout(debounceTimer); debounceTimer = setTimeout(function () { loadDepartmentsForDropdown($('#departmentManualSearch').val()); }, 300); }

function buildRequest() {
    const req = {
        globalSearch: $('#globalSearch').val(), sectionCode: '', sectionname: '', sectionId: '', departmentcode: $('#departmentManualSearch').val(), subDepartmentName: '', issectionActive: '', createdBy: '', editedBy: '',
        sortColumn: sortColumn, sortDirection: sortDirection, pageNumber: currentPage, pageSize: pageSize
    };
    $('.column-search').each(function () { req[$(this).data('field')] = $(this).val(); });
    return req;
}

async function loadSections() {
    try {
        const response = await $.ajax({ url: '/master/GetSectionList', type: 'POST', contentType: 'application/json', data: JSON.stringify(buildRequest()) });
        if (!response.success) { showFormMessage(response.message, false); return; }
        totalCount = response.totalCount;
        renderTable(response.data || []);
        updatePaging();
    } catch (e) { showFormMessage('Error loading section data', false); }
}

async function loadDepartmentsForDropdown(searchText, selectedValue) {
    const response = await $.get('/master/GetSectionDepartmentLookup', { searchText: searchText || '' });
    const ddl = $('#departmentcode');
    ddl.empty().append('<option value="">--Select Department--</option>');
    if (response.success) {
        (response.data || []).forEach(d => ddl.append(`<option value="${escapeHtml(d.departmentCode)}">${escapeHtml(d.departmentCode)} - ${escapeHtml(d.departmentName)}</option>`));
    }
    if (selectedValue) ddl.val(selectedValue);
}

function renderTable(data) {
    let rows = '';
    data.forEach(item => {
        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editSection(${item.sectionId})"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteSection(${item.sectionId})"></i></td>
            <td>${escapeHtml(item.sectionCode)}</td><td>${escapeHtml(item.sectionname)}</td><td>${item.sectionId}</td><td>${escapeHtml(item.departmentcode)}</td><td>${escapeHtml(item.subDepartmentName)}</td><td>${item.issectionActive ? 'Active' : 'Inactive'}</td>
            <td>${escapeHtml(item.createdBy)}</td><td>${formatDate(item.createdOn)}</td><td>${escapeHtml(item.editedBy)}</td><td>${formatDate(item.editedOn)}</td>
        </tr>`;
    });
    $('#tblSection tbody').html(rows || '<tr><td colspan="12" class="text-center">No records found</td></tr>');
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

function openSectionModal() {
    $('#sectionModalTitle').text('Add Section');
    $('#sectionForm')[0].reset();
    $('#sectionId').val(0);
    $('#sectionCode').prop('readonly', false);
    $('#formMessage').addClass('d-none').text('');
    loadDepartmentsForDropdown($('#departmentManualSearch').val());
    $('#sectionModal').modal('show');
}

async function editSection(id) {
    const response = await $.get('/master/GetSection', { id: id });
    if (!response.success) { alert(response.message); return; }
    const d = response.data;
    $('#sectionModalTitle').text('Edit Section');
    $('#sectionId').val(d.sectionId); $('#sectionCode').val(d.sectionCode).prop('readonly', true); $('#sectionname').val(d.sectionname);
    $('#subDepartmentName').val(d.subDepartmentName);
    await loadDepartmentsForDropdown(d.departmentcode, d.departmentcode);
    $('#formMessage').addClass('d-none').text('');
    $('#sectionModal').modal('show');
}

async function saveSection() {
    const form = $('#sectionForm')[0];
    if (!form.checkValidity()) { form.reportValidity(); return; }
    const token = $('input[name="__RequestVerificationToken"]').val();
    const formData = $('#sectionForm').serialize();
    const response = await $.ajax({ url: '/master/SaveSection', type: 'POST', data: formData, headers: { 'RequestVerificationToken': token } });
    showFormMessage(response.message, response.success);
    if (response.success) { $('#sectionModal').modal('hide'); loadSections(); }
}

function confirmDeleteSection(id) { deleteId = id; $('#deleteSectionModal').modal('show'); }
$('#btnConfirmSectionDelete').on('click', async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({ url: '/master/DeleteSection', type: 'POST', data: { id: deleteId }, headers: { 'RequestVerificationToken': token } });
    $('#deleteSectionModal').modal('hide');
    alert(response.message);
    if (response.success) loadSections();
});

async function exportSection(format) {
    const response = await fetch('/master/ExportSections?format=' + format, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(buildRequest()) });
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = `SectionMaster.${format}`; document.body.appendChild(a); a.click(); a.remove();
    window.URL.revokeObjectURL(url);
}

function showFormMessage(message, success) { const el = $('#formMessage'); el.removeClass('d-none alert-success alert-danger').addClass(success ? 'alert-success' : 'alert-danger').text(message); }
function escapeHtml(value) { return $('<div>').text(value || '').html(); }
function formatDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toLocaleString(); }
