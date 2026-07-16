let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'SectionId';
let sortDirection = 'DESC';
let deleteId = 0;
let debounceTimer = null;
let allDepartments = [];

let inactiveEditConfirmed = false;
let currentEditingInactiveRecord = false;
let showConfirmModalAfterSectionShown = false;

function isInactiveStatus(value) {
    if (value === undefined || value === null) return false;
    if (typeof value === 'boolean') return value === false;
    if (typeof value === 'number') return value === 0;
    const status = String(value).trim().toLowerCase();
    return status === 'false' || status === '0' || status === 'inactive' || status === 'n' || status === 'no';
}

function getSectionStatusValue(item) {
    if (!item) return null;
    return item.issectionActive ?? item.IssectionActive ?? item.isSectionActive ?? item.IsSectionActive ?? 
           item.activeStatus ?? item.ActiveStatus ?? item.isActive ?? item.IsActive;
}

function getSectionStatusText(item) {
    return isInactiveStatus(getSectionStatusValue(item)) ? 'Inactive' : 'Active';
}

function getStatusFromEditIcon(editIcon) {
    try {
        const row = $(editIcon).closest('tr');
        const statusText = row.find('td').eq(6).text();
        return statusText;
    } catch (e) {
        return '';
    }
}

function showInactiveEditConfirm() {
    const modal = $('#inactiveEditConfirmModal');
    
    if (!modal.length) {
        console.error('Modal #inactiveEditConfirmModal not found!');
        alert('You are going to edit the Inactive record, please confirm if you want to proceed?');
        return;
    }

    console.log('Showing inactive confirmation modal');
    
    // Remove any existing backdrop
    $('.modal-backdrop').remove();
    
    // Show modal with Bootstrap 4 method
    modal.modal({
        backdrop: 'static',
        keyboard: false,
        show: true
    });
}

function proceedInactiveEdit() {
    console.log('User clicked Proceed');
    inactiveEditConfirmed = true;
    $('#inactiveEditConfirmModal').modal('hide');
}

function cancelInactiveEdit() {
    console.log('User clicked Cancel');
    inactiveEditConfirmed = false;
    currentEditingInactiveRecord = false;
    
    // Hide both modals
    $('#inactiveEditConfirmModal').modal('hide');

    $('#sectionModal').modal('hide');
    $('#sectionForm')[0].reset();
    $('#sectionForm').find('input,select').removeClass('error-border valid-border');
    $('#formMessage').addClass('d-none').text('');
    
    setTimeout(() => {
        $('#sectionModal').modal('hide');
        resetSectionForm();
    }, 300);
}

function resetSectionForm() {
    const form = document.getElementById('sectionForm');
    if (form) form.reset();
    
    document.getElementById('sectionId').value = '0';
    document.getElementById('subDepartmentName').value = '-';
    document.getElementById('departmentLookupSearch').value = '';
    document.getElementById('departmentcode').value = '';
    resetSectionFormValidation();
}

$(document).ready(function () {
    console.log('SectionMaster JS loaded');
    
    loadDepartmentsForDropdown('');
    loadSections();

    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadSections();
    });

    $('#globalSearch').on('input', debounceSearch);
    $('.column-search').on('input', debounceSearch);

    $('#departmentLookupSearch').on('input change', function () {
        setDepartmentCodeFromLookup();
        validateDepartmentLookup();
    });

    $('#sectionForm').on('keyup change', 'input[required],select[required]', function () {
        if ($(this).attr('id') === 'departmentLookupSearch') {
            validateDepartmentLookup();
        } else {
            validateControl($(this));
        }
    });

    // When section modal is shown, show confirmation if needed
    $('#sectionModal').on('shown.bs.modal', function () {
        console.log('Section modal shown, showConfirmModalAfterSectionShown:', showConfirmModalAfterSectionShown);
        if (showConfirmModalAfterSectionShown) {
            showConfirmModalAfterSectionShown = false;
            // Small delay to ensure proper z-index handling
            setTimeout(() => {
                showInactiveEditConfirm();
            }, 100);
        }
    });

    // When section modal is hidden, reset flags
    $('#sectionModal').on('hidden.bs.modal', function () {
        resetSectionForm();
        currentEditingInactiveRecord = false;
        inactiveEditConfirmed = false;
        showConfirmModalAfterSectionShown = false;
    });

    $('#btnPrev').on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadSections();
        }
    });

    $('#btnNext').on('click', function () {
        const totalPages = Math.ceil(totalCount / pageSize);
        if (currentPage < totalPages) {
            currentPage++;
            loadSections();
        }
    });

    $('#tblSection thead th[data-sort]').on('click', function () {
        const selected = $(this).data('sort');
        sortDirection = sortColumn === selected && sortDirection === 'ASC' ? 'DESC' : 'ASC';
        sortColumn = selected;
        loadSections();
    });
});

function debounceSearch() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(function () {
        currentPage = 1;
        loadSections();
    }, 300);
}

function buildRequest() {
    const req = {
        globalSearch: $('#globalSearch').val(),
        sectionCode: '',
        sectionname: '',
        sectionId: '',
        departmentcode: '',
        subDepartmentName: '',
        issectionActive: '',
        createdBy: '',
        editedBy: '',
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

async function loadSections() {
    try {
        const response = await $.ajax({
            url: '/master/GetSectionList',
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
        showFormMessage('Error loading section data', false);
    }
}

async function loadDepartmentsForDropdown(searchText, selectedValue) {
    try {
        const response = await $.get('/master/GetSectionDepartmentLookup', { searchText: searchText || '' });
        if (response.success) {
            allDepartments = response.data || [];
        }
    } catch (e) {
        allDepartments = [];
    }

    renderDepartmentOptions(allDepartments);

    if (selectedValue) {
        const selectedDept = allDepartments.find(dept => String(dept.departmentCode) === String(selectedValue));
        if (selectedDept) {
            const displayText = getDepartmentDisplayText(selectedDept);
            $('#departmentLookupSearch').val(displayText);
            $('#departmentcode').val(selectedDept.departmentCode);
        } else {
            $('#departmentLookupSearch').val(selectedValue);
            $('#departmentcode').val(selectedValue);
        }
    }
}

function renderDepartmentOptions(departments) {
    const list = $('#departmentOptions');
    list.empty();
    (departments || []).forEach(d => {
        list.append(`<option value="${escapeHtml(getDepartmentDisplayText(d))}"></option>`);
    });
}

function getDepartmentDisplayText(dept) {
    const code = String(dept.departmentCode || '').trim();
    const name = String(dept.departmentName || '').trim();
    return name ? `${code} - ${name}` : code;
}

function setDepartmentCodeFromLookup() {
    const text = String($('#departmentLookupSearch').val() || '').trim().toLowerCase();
    const selectedDept = allDepartments.find(dept => {
        const code = String(dept.departmentCode || '').trim().toLowerCase();
        const name = String(dept.departmentName || '').trim().toLowerCase();
        const display = getDepartmentDisplayText(dept).toLowerCase();
        return text === code || text === name || text === display;
    });
    $('#departmentcode').val(selectedDept ? selectedDept.departmentCode : '');
}

function validateDepartmentLookup() {
    const lookup = $('#departmentLookupSearch');
    setDepartmentCodeFromLookup();
    if (!$('#departmentcode').val()) {
        lookup.removeClass('valid-border').addClass('error-border');
        return false;
    }
    lookup.removeClass('error-border').addClass('valid-border');
    return true;
}

function renderTable(data) {
    let rows = '';
    data.forEach(item => {
        const statusText = getSectionStatusText(item);
        rows += `<tr>
            <td><i class="bi bi-pencil-square text-primary" style="cursor:pointer" onclick="editSection(${item.sectionId}, this)"></i></td>
            <td><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDeleteSection(${item.sectionId})"></i></td>
            <td>${escapeHtml(item.sectionCode)}</td>
            <td>${escapeHtml(item.sectionname)}</td>
            <td>${item.sectionId}</td>
            <td>${escapeHtml(item.departmentcode)}</td>
            <td>${statusText}</td>
            <td>${escapeHtml(item.createdBy)}</td>
            <td>${formatDate(item.createdOn)}</td>
            <td>${escapeHtml(item.editedBy)}</td>
            <td>${formatDate(item.editedOn)}</td>
        </tr>`;
    });
    $('#tblSection tbody').html(rows || '<tr><td colspan="11" class="text-center">No records found</td></tr>');
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
    resetSectionForm();
    $('#sectionCode').prop('readonly', false);
    loadDepartmentsForDropdown('');
    $('#sectionModal').modal('show');
}

async function editSection(id, editIcon) {
    console.log('editSection called for id:', id);
    
    const rowStatusText = getStatusFromEditIcon(editIcon);
    console.log('Row status:', rowStatusText);

    const response = await $.get('/master/GetSection', { id: id });

    if (!response.success) {
        alert(response.message);
        return;
    }

    const d = response.data;
    console.log('Section data:', d);

    $('#sectionModalTitle').text('Edit Section');
    $('#sectionId').val(d.sectionId);
    $('#sectionCode').val(d.sectionCode).prop('readonly', true);
    $('#sectionname').val(d.sectionname);
    $('#subDepartmentName').val(d.subDepartmentName || '-');

    resetSectionFormValidation();
    await loadDepartmentsForDropdown('', d.departmentcode);

    // Check if the record is inactive
    const inactiveRecord = isInactiveStatus(rowStatusText) || isInactiveStatus(getSectionStatusValue(d));
    
    console.log('Is inactive record:', inactiveRecord);

    if (inactiveRecord) {
        // Mark that we're editing an inactive record
        currentEditingInactiveRecord = true;
        inactiveEditConfirmed = false;
        showConfirmModalAfterSectionShown = true;
    }

    // Show the section modal (confirmation will be shown in 'shown.bs.modal' event if needed)
    $('#sectionModal').modal('show');
}

async function saveSection() {
    if (currentEditingInactiveRecord && !inactiveEditConfirmed) {
        alert('Please confirm if you want to proceed with editing the inactive record.');
        return;
    }

    let valid = true;
    $('#sectionForm').find('input[required],select[required]').each(function () {
        if ($(this).attr('id') === 'departmentLookupSearch') {
            if (!validateDepartmentLookup()) valid = false;
        } else if (!validateControl($(this))) {
            valid = false;
        }
    });

    if (!valid) {
        alert('Please enter/select all mandatory fields.');
        return;
    }

    const form = document.getElementById('sectionForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const formData = $('#sectionForm').serialize();

    const response = await $.ajax({
        url: '/master/SaveSection',
        type: 'POST',
        data: formData,
        headers: { 'RequestVerificationToken': token }
    });

    showFormMessage(response.message, response.success);

    if (response.success) {
        $('#sectionModal').modal('hide');
        loadSections();
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

function resetSectionFormValidation() {
    $('#sectionForm').find('input,select').removeClass('error-border valid-border');
    $('#formMessage').removeClass('alert-success alert-danger').addClass('d-none').text('');
}

function confirmDeleteSection(id) {
    deleteId = id;
    $('#deleteSectionModal').modal('show');
}

$('#btnConfirmSectionDelete').on('click', async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({
        url: '/master/DeleteSection',
        type: 'POST',
        data: { id: deleteId },
        headers: { 'RequestVerificationToken': token }
    });

    $('#deleteSectionModal').modal('hide');
    alert(response.message);

    if (response.success) {
        loadSections();
    }
});

async function exportSection(format) {
    const response = await fetch('/master/ExportSections?format=' + format, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(buildRequest())
    });

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `SectionMaster.${format}`;
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

function escapeHtml(value) {
    return $('<div>').text(value || '').html();
}

function formatDate(value) {
    if (!value) return '';
    const d = new Date(value);
    return isNaN(d) ? '' : d.toLocaleString();
}