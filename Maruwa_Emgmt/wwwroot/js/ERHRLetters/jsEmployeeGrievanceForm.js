let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let sortColumn = 'CreatedOn';
let sortDirection = 'DESC';
let debounceTimer = null;
let employeeLookupList = [];
let partyLookupList = [];
let hrActionLookupList = [];
let involvedParties = [];
let hrActionEmployeeRows = [];
let hrEmployeeSignatureDrawing = {};
let isViewMode = false;
let isHrUserLogin = false;
let isHrActionEditable = false;
let loginInfo = {};
let departmentLookupMap = {};
let natureLookupList = [];
let selectedNatureRows = [];
let natureLookupLoaded = false;

const signaturePads = {
    employee: {
        canvasId: 'employeeSignatureCanvas',
        boxId: 'employeeSignatureBox',
        dataId: 'employeeSignatureData',
        pathId: 'employeeSignaturePath',
        validationId: 'employeeSignatureValidationMessage',
        previewContainerId: 'employeeSignaturePreviewContainer',
        previewImgId: 'employeeSignaturePreviewImg',
        saveMessageId: 'employeeSignatureSaveMessage',
        hasSignature: false,
        isDrawing: false,
        isSaved: false,
        pendingPath: '',
        readOnly: false
    },
    hr: {
        canvasId: 'hrSignatureCanvas',
        boxId: 'hrSignatureBox',
        dataId: 'hrSignatureData',
        pathId: 'hrSignaturePath',
        validationId: 'hrSignatureValidationMessage',
        previewContainerId: 'hrSignaturePreviewContainer',
        previewImgId: 'hrSignaturePreviewImg',
        saveMessageId: 'hrSignatureSaveMessage',
        hasSignature: false,
        isDrawing: false,
        isSaved: false,
        pendingPath: '',
        readOnly: false
    }
};

$(document).ready(function () {
    initialiseScreen();

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

    $('#hrActionEmpSearch').on('input', function () {
        debounceLookup(() => searchEmployees($('#hrActionEmpSearch').val(), 'hrActionEmployeeOptions', 'hrAction'));
    });

    $('#hrActionEmpSearch').on('change', function () {
        const empCode = extractEmployeeCode($(this).val());
        if (empCode) fillHrActionEmployeeByCode(empCode);
    });

    $('#natureSearchInput').on('focus click', function () {
        if (isViewMode) return;
        loadNatureOptions($(this).val(), true);
        $('#natureDropdownPanel').removeClass('d-none');
    });

    $('#natureSearchInput').on('input', function () {
        if (isViewMode) return;
        debounceLookup(() => loadNatureOptions($('#natureSearchInput').val(), true));
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.nature-selector-wrapper').length) {
            $('#natureDropdownPanel').addClass('d-none');
        }
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

    $('#otherComplaintText').on('input', function () {
        refreshNatureHiddenJson();
        validateControl($(this));
    });

    $('#grievanceForm').on('keyup change', 'input[required],textarea[required],select[required]', function () {
        validateControl($(this));
    });

    $('#grievanceModal').on('shown.bs.modal', function () {
        initSignaturePad('employee');
        initSignaturePad('hr');
        resizeSignatureCanvas('employee', false);
        resizeSignatureCanvas('hr', false);
        redrawSavedSignatureIfAvailable('employee');
        redrawSavedSignatureIfAvailable('hr');
        initialiseHrEmployeeSignatureCanvases();
    });

    $(window).on('resize', function () {
        if ($('#grievanceModal').hasClass('show')) {
            resizeSignatureCanvas('employee', true);
            resizeSignatureCanvas('hr', true);
        }
    });

    $('#grievanceModal').on('hidden.bs.modal', resetGrievanceForm);
});

async function initialiseScreen() {
    try {
        await loadGrievanceLoginInfo();
    } catch (e) { }

    loadDepartmentLookupMap().always(function () {
        loadGrievances();
    });
    loadNatureOptions('', false);
}

async function loadGrievanceLoginInfo() {
    const response = await $.get('/ERHRLetters/GetGrievanceLoginInfo');
    if (response && response.success) {
        isHrUserLogin = toBool(response.isHrUser);
        loginInfo = response;
        $('#isHrUser').val(isHrUserLogin ? 'true' : 'false');
        $('body').toggleClass('egf-hr-login', isHrUserLogin);
        $('#btnRaiseComplaint').toggle(!isHrUserLogin);
        if (isHrUserLogin) $('#tblGrievance').addClass('hr-list');
        else $('#tblGrievance').removeClass('hr-list');
    }
}

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

function loadDepartmentLookupMap() {
    departmentLookupMap = {};

    const fromEmployeeMaster = $.get('/EmpMaster/Getmaster_Department')
        .done(function (data) {
            registerDepartmentList(Array.isArray(data) ? data : (data.data || []));
        })
        .fail(function () { });

    const fromDepartmentMaster = $.get('/master/GetSectionDepartmentLookup', { searchText: '' })
        .done(function (response) {
            const data = response && response.success ? (response.data || []) : (Array.isArray(response) ? response : []);
            registerDepartmentList(data);
        })
        .fail(function () { });

    return $.when(fromEmployeeMaster, fromDepartmentMaster).always(function () { });
}

function registerDepartmentList(data) {
    (data || []).forEach(function (d) {
        const code = getFirstDefined(d.departmentCode, d.DepartmentCode, d.code, d.Code);
        const name = getFirstDefined(d.departmentName, d.DepartmentName, d.name, d.Name);
        registerDepartment(code, name);
    });
}

function registerDepartment(code, name) {
    const cleanCode = String(code || '').trim();
    let cleanName = String(name || '').trim();

    if (!cleanCode) return;

    if (cleanCode.indexOf(' - ') > -1) {
        const parts = cleanCode.split(' - ');
        const parsedCode = parts[0].trim();
        const parsedName = parts.slice(1).join(' - ').trim();
        if (parsedCode) {
            departmentLookupMap[parsedCode.toLowerCase()] = parsedName ? (parsedCode + ' - ' + parsedName) : cleanCode;
        }
        return;
    }

    departmentLookupMap[cleanCode.toLowerCase()] = cleanName ? (cleanCode + ' - ' + cleanName) : cleanCode;
}

function formatDepartmentCodeName(value, fallbackName) {
    const raw = String(value || '').trim();
    const name = String(fallbackName || '').trim();

    if (!raw && !name) return '';
    if (raw.indexOf(' - ') > -1) return raw;

    const mapped = departmentLookupMap[raw.toLowerCase()];
    if (mapped && mapped.indexOf(' - ') > -1) return mapped;

    if (raw && name) return raw + ' - ' + name;
    return mapped || raw || name;
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

        if (response.isHrUser !== undefined) {
            isHrUserLogin = toBool(response.isHrUser);
            $('#btnRaiseComplaint').toggle(!isHrUserLogin);
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
        const remarks = getFirstDefined(item.hrRemarks, item.HRRemarks);
        const remarksHtml = isHrUserLogin
            ? `<textarea id="hrRemarks_${id}" class="form-control form-control-sm hr-remarks-textarea">${escapeHtml(remarks)}</textarea><button type="button" class="btn btn-primary btn-sm mt-1" onclick="updateHrRemarks(${id})">Update</button>`
            : escapeHtml(remarks);

        rows += `<tr>
            <td><i class="bi bi-eye text-primary" style="cursor:pointer" title="View" onclick="viewGrievance(${id})"></i></td>
            <td>${escapeHtml(getFirstDefined(item.referenceNo, item.ReferenceNo))}</td>
            <td>${formatDate(getFirstDefined(item.dateOfReport, item.DateOfReport))}</td>
            <td>${escapeHtml(getFirstDefined(item.complainantEmpId, item.ComplainantEmpId))}</td>
            <td>${escapeHtml(getFirstDefined(item.complainantName, item.ComplainantName))}</td>
            <td>${escapeHtml(getDepartmentDisplayValue(item))}</td>
            <td>${escapeHtml(getFirstDefined(item.grievanceSummary, item.GrievanceSummary))}</td>
            <td>${escapeHtml(getFirstDefined(item.status, item.Status))}</td>
            <td>${remarksHtml}</td>
            <td>${formatDate(getFirstDefined(item.createdOn, item.CreatedOn))}</td>
        </tr>`;
    });

    $('#tblGrievance tbody').html(rows || '<tr><td colspan="10" class="text-center">No records found</td></tr>');
}

async function updateHrRemarks(id) {
    if (!isHrUserLogin) return;

    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({
        url: '/ERHRLetters/UpdateHrRemarks',
        type: 'POST',
        data: { grievanceId: id, remarks: $('#hrRemarks_' + id).val() },
        headers: { 'RequestVerificationToken': token }
    });

    alert(response.message || (response.success ? 'Remarks updated successfully.' : 'Unable to update remarks.'));
    if (response.success) loadGrievances();
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

async function openNewGrievanceForm() {
    if (isHrUserLogin) {
        alert('HR login can view and update employee grievance complaints only.');
        return;
    }

    resetGrievanceForm();
    isViewMode = false;
    $('#grievanceModal').removeClass('modal-view-mode');
    $('#grievanceModalTitle').text('Raise a New Complaint');
    setEmployeeFormReadonly(false);
    setHrActionVisibility(false, false);
    setTodayFields();
    $('#referenceNo').val('Auto-generated after submit');
    await loadLoggedInEmployee();
    $('#grievanceModal').modal('show');
}

async function viewGrievance(id) {
    resetGrievanceForm();
    isViewMode = true;
    $('#grievanceModal').addClass('modal-view-mode');
    $('#grievanceModalTitle').text(isHrUserLogin ? 'View / HR Action - Employee Grievance Form' : 'View Employee Grievance Form');

    const response = await $.get('/ERHRLetters/GetGrievanceForm', { id: id });
    if (!response.success) {
        alert(response.message || 'Unable to load grievance form');
        return;
    }

    fillForm(response.data);
    setEmployeeFormReadonly(true);

    const status = String(getFirstDefined(response.data.status, response.data.Status)).toLowerCase();
    const hasHrAction = !!(response.data.hrAction || response.data.HrAction);
    const showHrForm = isHrUserLogin || hasHrAction || status === 'completed';
    setHrActionVisibility(showHrForm, isHrUserLogin);

    $('#grievanceModal').modal('show');

    if (isHrUserLogin) {
        loadGrievances();
    }
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
    else if (target === 'hrAction') hrActionLookupList = data;
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
    $('#department').val(getDepartmentDisplayValue(emp));
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
    $('#partyDepartment').val(getDepartmentDisplayValue(emp));
}

async function fillHrActionEmployeeByCode(empCode, addToGrid) {
    const response = await $.get('/ERHRLetters/GetEmployeeByCode', { empCode });
    if (!response.success) {
        alert(response.message || 'Employee not found');
        return false;
    }

    const emp = response.data;
    const code = getFirstDefined(emp.empCode, emp.EmpCode);
    const name = getFirstDefined(emp.empName, emp.EmpName);
    const position = getFirstDefined(emp.positionTitle, emp.PositionTitle);
    const department = getDepartmentDisplayValue(emp);

    $('#hrActionEmpSearch').val(code + (name ? ' - ' + name : ''));
    $('#hrActionEmployeeId').val(code);
    $('#hrActionEmployeeName').val(name);
    $('#hrActionPositionTitle').val(position);
    $('#hrActionDepartment').val(department);

    if (addToGrid === true) {
        addHrActionEmployeeRow(code, name, position, department, true);
    }

    return true;
}

async function addHrActionEmployee() {
    let employeeID = extractEmployeeCode($('#hrActionEmpSearch').val()) || $('#hrActionEmployeeId').val();

    if (!employeeID) {
        alert('Please select/key in employee ID.');
        $('#hrActionEmpSearch').removeClass('valid-border').addClass('error-border');
        return;
    }

    if (!$('#hrActionEmployeeName').val()) {
        const loaded = await fillHrActionEmployeeByCode(employeeID, false);
        if (!loaded) {
            $('#hrActionEmpSearch').removeClass('valid-border').addClass('error-border');
            return;
        }
        employeeID = $('#hrActionEmployeeId').val() || employeeID;
    }

    const employeeName = $('#hrActionEmployeeName').val();
    const positionTitle = $('#hrActionPositionTitle').val();
    const department = $('#hrActionDepartment').val();

    if (!employeeName) {
        alert('Please select a valid employee ID from the search list.');
        $('#hrActionEmpSearch').removeClass('valid-border').addClass('error-border');
        return;
    }

    addHrActionEmployeeRow(employeeID, employeeName, positionTitle, department, true);
}

function addHrActionEmployeeRow(employeeID, employeeName, positionTitle, department, clearAfterAdd, employeeSignaturePath, employeeSignatureData) {
    const cleanId = String(employeeID || '').trim();
    const cleanName = String(employeeName || '').trim();
    if (!cleanId) return false;

    if (hrActionEmployeeRows.some(x => String(getFirstDefined(x.employeeID, x.EmployeeID)).toLowerCase() === cleanId.toLowerCase())) {
        alert('This employee is already added.');
        clearHrActionEmployeeSearchFields();
        return false;
    }

    const signaturePath = getFirstDefined(employeeSignaturePath, '');
    const signatureData = getFirstDefined(employeeSignatureData, '');

    hrActionEmployeeRows.push({
        employeeID: cleanId,
        employeeName: cleanName,
        positionTitle: positionTitle || '',
        department: department || '',
        employeeSignaturePath: signaturePath || '',
        employeeSignatureData: signatureData || '',
        employeeSignatureHasSignature: !!(signaturePath || signatureData),
        employeeSignatureSaved: !!(signaturePath || signatureData)
    });

    renderHrActionEmployeeTable();

    if (clearAfterAdd !== false) {
        clearHrActionEmployeeSearchFields();
    }

    return true;
}
function setHrActionEmployeeRow(employeeID, employeeName, positionTitle, department, employeeSignaturePath) {
    // Kept for backward compatibility with older code paths; now supports multiple rows.
    return addHrActionEmployeeRow(employeeID, employeeName, positionTitle, department, false, employeeSignaturePath);
}

function clearHrActionEmployeeSearchFields() {
    $('#hrActionEmpSearch,#hrActionEmployeeId,#hrActionEmployeeName,#hrActionPositionTitle,#hrActionDepartment').val('');
    $('#hrActionEmpSearch').removeClass('error-border valid-border');
}

function removeHrActionEmployee(index) {
    if (!isHrUserLogin) return;
    if (index === undefined || index === null) {
        hrActionEmployeeRows = [];
    } else {
        hrActionEmployeeRows.splice(index, 1);
    }
    clearHrActionEmployeeSearchFields();
    renderHrActionEmployeeTable();
}

function canEditHrActionSignature() {
    return isHrUserLogin && isHrActionEditable;
}

function renderHrActionEmployeeTable() {
    let rows = '';
    const canEditSignatures = canEditHrActionSignature();

    hrActionEmployeeRows.forEach((p, index) => {
        const employeeId = getFirstDefined(p.employeeID, p.EmployeeID);
        const employeeName = getFirstDefined(p.employeeName, p.EmployeeName);
        const positionTitle = getFirstDefined(p.positionTitle, p.PositionTitle);
        const department = getDepartmentDisplayValue(p);
        const hasSavedSignature = !!getHrEmployeeSignatureSource(index);
        const signatureBoxClass = hasSavedSignature ? 'valid-border' : '';
        const disabledClass = canEditSignatures ? '' : 'signature-disabled';

        rows += `<tr>
            <td>${escapeHtml(employeeId)}</td>
            <td>${escapeHtml(employeeName)}</td>
            <td>${escapeHtml(positionTitle)}</td>
            <td>${escapeHtml(department)}</td>
            <td class="hr-employee-signature-cell">
                <div id="hrEmployeeSignatureBox_${index}" class="hr-employee-signature-box ${signatureBoxClass}">
                    <canvas id="hrEmployeeSignatureCanvas_${index}" class="hr-employee-signature-canvas ${disabledClass}" data-index="${index}"></canvas>
                </div>
                <div id="hrEmployeeSignatureValidation_${index}" class="text-danger mt-1 d-none">Employee signature is required.</div>
                <div class="hr-employee-signature-actions hr-action-edit-only">
                    <button type="button" class="btn btn-success btn-sm" onclick="saveHrEmployeeSignature(${index}, true)"><i class="bi bi-check2-circle"></i> Save Signature</button>
                    <button type="button" class="btn btn-outline-secondary btn-sm" onclick="clearHrEmployeeSignature(${index})"><i class="bi bi-eraser"></i> Clear</button>
                </div>
                <div id="hrEmployeeSignatureMessage_${index}" class="text-success mt-1 d-none hr-employee-signature-message"></div>
            </td>
            <td class="hr-action-edit-only"><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="removeHrActionEmployee(${index})"></i></td>
        </tr>`;
    });

    $('#tblHrActionEmployee tbody').html(rows || '<tr><td colspan="6" class="text-center">No employees selected</td></tr>');
    setTimeout(initialiseHrEmployeeSignatureCanvases, 0);
}

function getHrEmployeeSignatureSource(index) {
    const row = hrActionEmployeeRows[index] || {};
    return getFirstDefined(row.employeeSignatureData, row.EmployeeSignatureData, row.employeeSignaturePath, row.EmployeeSignaturePath, '');
}

function initialiseHrEmployeeSignatureCanvases() {
    $('#tblHrActionEmployee canvas.hr-employee-signature-canvas').each(function () {
        const canvas = this;
        const index = parseInt($(canvas).data('index'), 10);
        prepareHrEmployeeSignatureCanvas(canvas, index);
    });
}

function prepareHrEmployeeSignatureCanvas(canvas, index) {
    if (!canvas || Number.isNaN(index)) return;
    const row = hrActionEmployeeRows[index];
    if (!row) return;

    const rect = canvas.getBoundingClientRect();
    canvas.width = Math.max(280, Math.floor(rect.width || 320));
    canvas.height = Math.max(82, Math.floor(rect.height || 88));
    const ctx = canvas.getContext('2d');
    configureSignatureContext(ctx);

    const source = getHrEmployeeSignatureSource(index);
    if (source) {
        drawImageOnCanvas(canvas, source, function () {
            configureSignatureContext(ctx);
        });
    }

    if (canvas.dataset.initialized === 'true') return;
    canvas.dataset.initialized = 'true';

    const startDraw = function (event) {
        if (!canEditHrActionSignature()) return;
        event.preventDefault();
        hrEmployeeSignatureDrawing[index] = true;
        const point = getCanvasPointFromEvent(event, canvas);
        const context = canvas.getContext('2d');
        context.beginPath();
        context.moveTo(point.x, point.y);
    };

    const draw = function (event) {
        if (!hrEmployeeSignatureDrawing[index] || !canEditHrActionSignature()) return;
        event.preventDefault();
        const point = getCanvasPointFromEvent(event, canvas);
        const context = canvas.getContext('2d');
        context.lineTo(point.x, point.y);
        context.stroke();
        markHrEmployeeSignatureChanged(index, canvas);
    };

    const stopDraw = function () {
        if (!hrEmployeeSignatureDrawing[index]) return;
        hrEmployeeSignatureDrawing[index] = false;
        markHrEmployeeSignatureChanged(index, canvas);
    };

    canvas.addEventListener('mousedown', startDraw);
    canvas.addEventListener('mousemove', draw);
    window.addEventListener('mouseup', stopDraw);
    canvas.addEventListener('touchstart', startDraw, { passive: false });
    canvas.addEventListener('touchmove', draw, { passive: false });
    canvas.addEventListener('touchend', stopDraw);
    canvas.addEventListener('touchcancel', stopDraw);
}

function getCanvasPointFromEvent(event, canvas) {
    const pointer = getPointerEvent(event);
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    return {
        x: (pointer.clientX - rect.left) * scaleX,
        y: (pointer.clientY - rect.top) * scaleY
    };
}

function markHrEmployeeSignatureChanged(index, canvas) {
    const row = hrActionEmployeeRows[index];
    if (!row || !canvas) return;
    row.employeeSignatureData = canvas.toDataURL('image/png');
    row.employeeSignaturePath = '';
    row.employeeSignatureHasSignature = true;
    row.employeeSignatureSaved = false;
    $('#hrEmployeeSignatureBox_' + index).removeClass('error-border').addClass('valid-border');
    $('#hrEmployeeSignatureValidation_' + index).addClass('d-none');
    $('#hrEmployeeSignatureMessage_' + index).addClass('d-none').text('');
}

function saveHrEmployeeSignature(index, showMessage) {
    const row = hrActionEmployeeRows[index];
    const canvas = document.getElementById('hrEmployeeSignatureCanvas_' + index);
    if (!row || !canvas) return false;

    if (!row.employeeSignatureData && !row.employeeSignaturePath && !row.employeeSignatureHasSignature) {
        $('#hrEmployeeSignatureBox_' + index).removeClass('valid-border').addClass('error-border');
        $('#hrEmployeeSignatureValidation_' + index).removeClass('d-none');
        $('#hrEmployeeSignatureMessage_' + index)
            .removeClass('d-none text-success')
            .addClass('text-danger')
            .text('Please draw and save employee signature.');
        return false;
    }

    if (!row.employeeSignatureData && !row.employeeSignaturePath) {
        row.employeeSignatureData = canvas.toDataURL('image/png');
    }

    row.employeeSignatureSaved = true;
    row.employeeSignatureHasSignature = true;
    $('#hrEmployeeSignatureBox_' + index).removeClass('error-border').addClass('valid-border');
    $('#hrEmployeeSignatureValidation_' + index).addClass('d-none');

    if (showMessage) {
        $('#hrEmployeeSignatureMessage_' + index)
            .removeClass('d-none text-danger')
            .addClass('text-success')
            .text('Signature saved.');
    }

    return true;
}

function clearHrEmployeeSignature(index) {
    if (!canEditHrActionSignature()) return;
    const row = hrActionEmployeeRows[index];
    const canvas = document.getElementById('hrEmployeeSignatureCanvas_' + index);
    if (!row || !canvas) return;
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    configureSignatureContext(ctx);
    row.employeeSignaturePath = '';
    row.employeeSignatureData = '';
    row.employeeSignatureHasSignature = false;
    row.employeeSignatureSaved = false;
    $('#hrEmployeeSignatureBox_' + index).removeClass('valid-border').addClass('error-border');
    $('#hrEmployeeSignatureValidation_' + index).removeClass('d-none');
    $('#hrEmployeeSignatureMessage_' + index).addClass('d-none').text('');
}

function validateHrActionEmployeeSignatures() {
    let valid = true;
    hrActionEmployeeRows.forEach(function (row, index) {
        const hasSource = !!(row.employeeSignatureData || row.employeeSignaturePath || row.EmployeeSignatureData || row.EmployeeSignaturePath);
        if (hasSource && !row.employeeSignatureSaved) {
            saveHrEmployeeSignature(index, false);
        }

        if (!(row.employeeSignatureData || row.employeeSignaturePath || row.EmployeeSignatureData || row.EmployeeSignaturePath) || !row.employeeSignatureSaved) {
            $('#hrEmployeeSignatureBox_' + index).removeClass('valid-border').addClass('error-border');
            $('#hrEmployeeSignatureValidation_' + index).removeClass('d-none');
            valid = false;
        }
    });

    if (!valid) {
        alert('Please draw and save employee signature for each HR action employee row.');
    }

    return valid;
}

function drawImageOnCanvas(canvas, src, callback) {
    if (!canvas || !src) return;
    const ctx = canvas.getContext('2d');
    const image = new Image();
    image.onload = function () {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        const scale = Math.min(canvas.width / image.width, canvas.height / image.height);
        const drawWidth = image.width * scale;
        const drawHeight = image.height * scale;
        const x = (canvas.width - drawWidth) / 2;
        const y = (canvas.height - drawHeight) / 2;
        ctx.drawImage(image, x, y, drawWidth, drawHeight);
        if (typeof callback === 'function') callback();
    };
    image.src = src;
}

async function loadNatureOptions(searchText, showDropdown) {
    try {
        const response = await $.get('/ERHRLetters/GetNatureOfGrievanceLookup', { searchText: searchText || '' });
        const data = response.success ? (response.data || []) : [];
        natureLookupList = uniqueNatureList(data);
        natureLookupLoaded = true;
        renderNatureDropdown();
        if (showDropdown && !isViewMode) $('#natureDropdownPanel').removeClass('d-none');
    } catch (e) {
        natureLookupList = [];
        renderNatureDropdown();
    }
}

function uniqueNatureList(data) {
    const map = {};
    const list = [];
    (data || []).forEach(function (item) {
        const natureId = parseInt(getFirstDefined(item.natureID, item.NatureID), 10) || 0;
        const natureName = getFirstDefined(item.natureName, item.NatureName);
        const key = natureId > 0 ? String(natureId) : natureName.toLowerCase();
        if (!natureName || map[key]) return;
        map[key] = true;
        list.push({
            natureID: natureId,
            natureName: natureName,
            isOther: toBool(getFirstDefined(item.isOther, item.IsOther)) || isOtherNatureName(natureName)
        });
    });
    return list;
}

function renderNatureDropdown() {
    const panel = $('#natureDropdownPanel');
    const searchText = String($('#natureSearchInput').val() || '').trim().toLowerCase();
    const filtered = natureLookupList.filter(function (n) {
        return !searchText || n.natureName.toLowerCase().indexOf(searchText) >= 0 || String(n.natureID).indexOf(searchText) >= 0;
    });

    if (isViewMode) {
        panel.addClass('d-none').empty();
        return;
    }

    if (filtered.length === 0) {
        panel.html('<div class="px-3 py-2 text-muted">No Nature of Grievance found</div>');
        return;
    }

    const html = filtered.map(function (n) {
        const checked = selectedNatureRows.some(x => Number(getFirstDefined(x.natureID, x.NatureID)) === Number(n.natureID)) ? 'checked' : '';
        return `<label class="nature-dropdown-item">
            <input type="checkbox" ${checked} onchange="toggleNatureSelection(${n.natureID}, this.checked)" />
            <span>${escapeHtml(n.natureName)}</span>
        </label>`;
    }).join('');
    panel.html(html);
}

function toggleNatureSelection(natureId, checked) {
    if (isViewMode) return;
    const nature = natureLookupList.find(x => Number(x.natureID) === Number(natureId));
    if (!nature) return;

    if (checked) {
        if (!selectedNatureRows.some(x => Number(getFirstDefined(x.natureID, x.NatureID)) === Number(natureId))) {
            selectedNatureRows.push({
                natureID: nature.natureID,
                natureName: nature.natureName,
                isOther: nature.isOther,
                otherComplaintText: ''
            });
        }
    } else {
        selectedNatureRows = selectedNatureRows.filter(x => Number(getFirstDefined(x.natureID, x.NatureID)) !== Number(natureId));
    }

    $('#natureSearchInput').val('');
    renderSelectedNatures();
    renderNatureDropdown();
}

function removeSelectedNature(index) {
    if (isViewMode) return;
    selectedNatureRows.splice(index, 1);
    renderSelectedNatures();
    renderNatureDropdown();
}

function renderSelectedNatures() {
    const container = $('#selectedNatureContainer');
    if (selectedNatureRows.length === 0) {
        container.html('<span class="selected-nature-empty">No Nature of Grievance selected</span>');
        $('#natureOfGrievanceJson').val('');
        $('#otherComplaintTextRow').addClass('d-none');
        $('#otherComplaintText').val('');
        return;
    }

    const html = selectedNatureRows.map(function (n, index) {
        const name = getFirstDefined(n.natureName, n.NatureName);
        const removeIcon = isViewMode ? '' : `<span class="remove-nature" title="Remove" onclick="removeSelectedNature(${index})">&times;</span>`;
        return `<span class="selected-nature-chip">${escapeHtml(name)}${removeIcon}</span>`;
    }).join('');
    container.html(html);

    const otherSelected = isOtherNatureSelected();
    $('#otherComplaintTextRow').toggleClass('d-none', !otherSelected);
    if (!otherSelected) $('#otherComplaintText').val('');
    refreshNatureHiddenJson();
}

function refreshNatureHiddenJson() {
    const otherText = $('#otherComplaintText').val();
    const data = selectedNatureRows.map(function (n) {
        return {
            natureID: parseInt(getFirstDefined(n.natureID, n.NatureID), 10) || 0,
            natureName: getFirstDefined(n.natureName, n.NatureName),
            isOther: toBool(getFirstDefined(n.isOther, n.IsOther)) || isOtherNatureName(getFirstDefined(n.natureName, n.NatureName)),
            otherComplaintText: otherText
        };
    });
    $('#natureOfGrievanceJson').val(JSON.stringify(data));
}

function isOtherNatureSelected() {
    return selectedNatureRows.some(function (n) {
        return toBool(getFirstDefined(n.isOther, n.IsOther)) || isOtherNatureName(getFirstDefined(n.natureName, n.NatureName));
    });
}

function isOtherNatureName(natureName) {
    return String(natureName || '').trim().toLowerCase().startsWith('other');
}

function setSelectedNaturesFromData(data) {
    const fromDb = getFirstDefined(data.selectedNatures, data.SelectedNatures);
    selectedNatureRows = [];

    if (Array.isArray(fromDb) && fromDb.length > 0) {
        selectedNatureRows = fromDb.map(function (n) {
            return {
                grievanceNatureID: parseInt(getFirstDefined(n.grievanceNatureID, n.GrievanceNatureID), 10) || 0,
                natureID: parseInt(getFirstDefined(n.natureID, n.NatureID), 10) || 0,
                natureName: getFirstDefined(n.natureName, n.NatureName),
                isOther: toBool(getFirstDefined(n.isOther, n.IsOther)),
                otherComplaintText: getFirstDefined(n.otherComplaintText, n.OtherComplaintText)
            };
        });
    } else {
        selectedNatureRows = buildNatureRowsFromLegacyFlags(data);
    }

    const otherText = getFirstDefined(data.otherComplaintText, data.OtherComplaintText);
    $('#otherComplaintText').val(otherText);
    renderSelectedNatures();
}

function buildNatureRowsFromLegacyFlags(data) {
    const rows = [];
    const map = [
        ['unfairTreatment', 'UnfairTreatment', 'Unfair treatment / discrimination'],
        ['harassmentBullying', 'HarassmentBullying', 'Harassment / bullying'],
        ['workLapses', 'WorkLapses', 'Work lapses'],
        ['policySopBreach', 'PolicySopBreach', 'Breach of company policy / SOP'],
        ['oshaConcern', 'OshaConcern', 'Safety, health & environment (OSH) concern'],
        ['supervisorMisconduct', 'SupervisorMisconduct', 'Misconduct by supervisor / colleague'],
        ['abuseOfAuthority', 'AbuseOfAuthority', 'Abuse of authority / power'],
        ['workingHoursIssue', 'WorkingHoursIssue', 'Working hours / shift scheduling issue'],
        ['otherComplaint', 'OtherComplaint', 'Other (please specify in the below text box)']
    ];

    map.forEach(function (m) {
        if (toBool(getFirstDefined(data[m[0]], data[m[1]]))) {
            const master = natureLookupList.find(x => x.natureName.toLowerCase() === m[2].toLowerCase());
            rows.push({
                natureID: master ? master.natureID : 0,
                natureName: master ? master.natureName : m[2],
                isOther: m[2].toLowerCase().startsWith('other')
            });
        }
    });
    return rows;
}

function getLegacyNatureFlagsFromSelected() {
    const flags = {
        unfairTreatment: false,
        harassmentBullying: false,
        workLapses: false,
        policySopBreach: false,
        oshaConcern: false,
        supervisorMisconduct: false,
        abuseOfAuthority: false,
        workingHoursIssue: false,
        otherComplaint: false
    };

    selectedNatureRows.forEach(function (n) {
        const text = getFirstDefined(n.natureName, n.NatureName).toUpperCase();
        if (text.indexOf('UNFAIR') >= 0 || text.indexOf('DISCRIMINATION') >= 0) flags.unfairTreatment = true;
        if (text.indexOf('HARASSMENT') >= 0 || text.indexOf('BULLYING') >= 0) flags.harassmentBullying = true;
        if (text.indexOf('WORK LAPSES') >= 0) flags.workLapses = true;
        if (text.indexOf('BREACH') >= 0 || text.indexOf('SOP') >= 0) flags.policySopBreach = true;
        if (text.indexOf('SAFETY') >= 0 || text.indexOf('OSH') >= 0 || text.indexOf('HEALTH') >= 0 || text.indexOf('ENVIRONMENT') >= 0) flags.oshaConcern = true;
        if (text.indexOf('MISCONDUCT BY SUPERVISOR') >= 0 || text.indexOf('MISCONDUCT BY') >= 0 || text.indexOf('COLLEAGUE') >= 0) flags.supervisorMisconduct = true;
        if (text.indexOf('ABUSE OF AUTHORITY') >= 0 || text.indexOf('ABUSE OF') >= 0 || text.indexOf('POWER') >= 0) flags.abuseOfAuthority = true;
        if (text.indexOf('WORKING HOURS') >= 0 || text.indexOf('SHIFT SCHEDULING') >= 0) flags.workingHoursIssue = true;
        if (toBool(getFirstDefined(n.isOther, n.IsOther)) || isOtherNatureName(text)) flags.otherComplaint = true;
    });

    return flags;
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
            <td>${escapeHtml(getDepartmentDisplayValue(p))}</td>
            <td class="employee-save-only"><i class="bi bi-trash text-danger" style="cursor:pointer" onclick="removeInvolvedParty(${index})"></i></td>
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
    refreshNatureHiddenJson();
    const natureFlags = getLegacyNatureFlagsFromSelected();
    formData.append('natureOfGrievanceJson', $('#natureOfGrievanceJson').val());
    formData.append('UnfairTreatment', natureFlags.unfairTreatment);
    formData.append('HarassmentBullying', natureFlags.harassmentBullying);
    formData.append('WorkLapses', natureFlags.workLapses);
    formData.append('PolicySopBreach', natureFlags.policySopBreach);
    formData.append('OshaConcern', natureFlags.oshaConcern);
    formData.append('SupervisorMisconduct', natureFlags.supervisorMisconduct);
    formData.append('AbuseOfAuthority', natureFlags.abuseOfAuthority);
    formData.append('WorkingHoursIssue', natureFlags.workingHoursIssue);
    formData.append('OtherComplaint', natureFlags.otherComplaint);
    formData.append('OtherComplaintText', $('#otherComplaintText').val());
    formData.append('ConductDate', $('#conductDate').val());
    formData.append('ConductTime', $('#conductTime').val());
    formData.append('Location', $('#location').val());
    formData.append('IncidentDescription', $('#incidentDescription').val());
    formData.append('Witnesses', $('#witnesses').val());
    formData.append('SupportingDocumentsAttached', $('#supportDocsYes').is(':checked'));
    formData.append('DesiredOutcome', $('#desiredOutcome').val());
    formData.append('involvedPartiesJson', JSON.stringify(involvedParties));
    formData.append('EmployeeSignaturePath', $('#employeeSignaturePath').val());
    formData.append('employeeSignatureData', $('#employeeSignatureData').val());

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

    if (selectedNatureRows.length === 0) {
        $('#natureSearchInput').removeClass('valid-border').addClass('error-border');
        alert('Please select at least one Nature of Grievance / Complaint.');
        return false;
    }

    if (isOtherNatureSelected() && String($('#otherComplaintText').val() || '').trim() === '') {
        $('#otherComplaintText').removeClass('valid-border').addClass('error-border');
        alert('Please enter complaint details for Other nature of grievance.');
        return false;
    }

    $('#grievanceForm').find('textarea[required],input[required]').each(function () {
        if (!validateControl($(this))) valid = false;
    });

    if (!validateSignature('employee', 'Please draw and save the employee signature.')) {
        valid = false;
    }

    if (!valid) {
        alert('Please enter/select all mandatory fields.');
        return false;
    }
    return true;
}

function fillForm(data) {
    $('#grievanceID').val(getFirstDefined(data.grievanceID, data.GrievanceID));
    $('#hrActionGrievanceID').val(getFirstDefined(data.grievanceID, data.GrievanceID));
    $('#referenceNo').val(getFirstDefined(data.referenceNo, data.ReferenceNo));

    const empCode = getFirstDefined(data.complainantEmpId, data.ComplainantEmpId);
    const empName = getFirstDefined(data.complainantName, data.ComplainantName);
    $('#complainantEmpSearch').val(empCode + (empName ? ' - ' + empName : ''));
    $('#complainantEmpId').val(empCode);
    $('#complainantName').val(empName);
    $('#department').val(getDepartmentDisplayValue(data));
    $('#positionTitle').val(getFirstDefined(data.positionTitle, data.PositionTitle));
    $('#dateOfReportDisplay').val(formatDateOnly(getFirstDefined(data.dateOfReport, data.DateOfReport)));

    setSelectedNaturesFromData(data);

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
        department: getDepartmentDisplayValue(p)
    }));
    renderInvolvedParties();

    const signaturePath = getFirstDefined(data.employeeSignaturePath, data.EmployeeSignaturePath);
    setSignatureExistingPath('employee', signaturePath);

    const declarationName = getFirstDefined(data.declarationEmployeeName, data.DeclarationEmployeeName, empName);
    const declarationEmpId = getFirstDefined(data.declarationEmployeeId, data.DeclarationEmployeeId, empCode);
    $('#declarationEmployee').val(declarationName + (declarationEmpId ? ' - ' + declarationEmpId : ''));
    $('#declarationDateDisplay').val(formatDate(getFirstDefined(data.declarationDate, data.DeclarationDate)));

    renderAttachments(data.attachments || data.Attachments || []);
    fillHrAction(data);
}

function fillHrAction(data) {
    const hrAction = data.hrAction || data.HrAction || {};
    const grievanceId = getFirstDefined(data.grievanceID, data.GrievanceID);
    const referenceNo = getFirstDefined(data.referenceNo, data.ReferenceNo);
    const complainantEmpId = getFirstDefined(data.complainantEmpId, data.ComplainantEmpId);
    const complainantName = getFirstDefined(data.complainantName, data.ComplainantName);

    $('#hrActionID').val(getFirstDefined(hrAction.hrActionID, hrAction.HRActionID));
    $('#hrActionGrievanceID').val(grievanceId);
    $('#hrCaseReferenceNo').val(referenceNo);
    $('#hrActionDate').val(formatDate(getFirstDefined(hrAction.actionDate, hrAction.ActionDate)) || new Date().toLocaleString());

    $('#hrEmpId').val(getFirstDefined(hrAction.hrEmpId, hrAction.HREmpId, loginInfo.empCode));
    $('#hrName').val(getFirstDefined(hrAction.hrName, hrAction.HRName, loginInfo.empName));

    const actionEmployees = getFirstDefined(hrAction.actionEmployees, hrAction.ActionEmployees) || [];
    hrActionEmployeeRows = [];

    if (Array.isArray(actionEmployees) && actionEmployees.length > 0) {
        actionEmployees.forEach(function (empRow) {
            addHrActionEmployeeRow(
                getFirstDefined(empRow.employeeID, empRow.EmployeeID),
                getFirstDefined(empRow.employeeName, empRow.EmployeeName),
                getFirstDefined(empRow.positionTitle, empRow.PositionTitle),
                getDepartmentDisplayValue(empRow),
                false,
                getFirstDefined(empRow.employeeSignaturePath, empRow.EmployeeSignaturePath),
                getFirstDefined(empRow.employeeSignatureData, empRow.EmployeeSignatureData)
            );
        });
    } else {
        const actionEmployeeId = getFirstDefined(hrAction.actionEmployeeId, hrAction.ActionEmployeeId);
        const actionEmployeeName = getFirstDefined(hrAction.actionEmployeeName, hrAction.ActionEmployeeName);
        const actionEmployeePosition = getFirstDefined(hrAction.actionEmployeePositionTitle, hrAction.ActionEmployeePositionTitle);
        const actionEmployeeDepartment = getFirstDefined(hrAction.actionEmployeeDepartment, hrAction.ActionEmployeeDepartment);

        if (actionEmployeeId) {
            addHrActionEmployeeRow(actionEmployeeId, actionEmployeeName, actionEmployeePosition, actionEmployeeDepartment, false);
        }
    }

    clearHrActionEmployeeSearchFields();
    renderHrActionEmployeeTable();

    $('#hrInvestigationSummary').val(getFirstDefined(hrAction.investigationSummary, hrAction.InvestigationSummary));
    $('#hrEmployeeExplanation').val(getFirstDefined(hrAction.employeeExplanation, hrAction.EmployeeExplanation));
    $('#hrRemarks').val(getFirstDefined(hrAction.remarks, hrAction.Remarks, data.hrRemarks, data.HRRemarks));

    setChecked('hrOutcomeResolved', getFirstDefined(hrAction.outcomeResolved, hrAction.OutcomeResolved));
    setChecked('hrOutcomeReferredToER', getFirstDefined(hrAction.outcomeReferredToER, hrAction.OutcomeReferredToER));
    setChecked('hrOutcomeReferredToDomesticInquiry', getFirstDefined(hrAction.outcomeReferredToDomesticInquiry, hrAction.OutcomeReferredToDomesticInquiry));
    setChecked('hrMinorMisconduct', getFirstDefined(hrAction.minorMisconduct, hrAction.MinorMisconduct));
    setChecked('hrMajorMisconduct', getFirstDefined(hrAction.majorMisconduct, hrAction.MajorMisconduct));
    $('#hrMajorMisconductText').val(getFirstDefined(hrAction.majorMisconductText, hrAction.MajorMisconductText));

    const hrSignaturePath = getFirstDefined(hrAction.hrSignaturePath, hrAction.HRSignaturePath);
    setSignatureExistingPath('hr', hrSignaturePath);

    if (isViewMode) {
        hideSignaturePreview('employee');
        hideSignaturePreview('hr');
    }

    $('#hrFooterEmpId').val(getFirstDefined(hrAction.hrEmpId, hrAction.HREmpId, loginInfo.empCode));
    $('#hrFooterEmpName').val(getFirstDefined(hrAction.hrName, hrAction.HRName, loginInfo.empName));
    $('#hrFooterDepartment').val(getFirstDefined(hrAction.department, hrAction.Department, loginInfo.department, 'HUMAN RESOURCE'));
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
    const html = Array.from(files || []).map(file => `<div>${escapeHtml(file.name)} (${Math.ceil(file.size / 1024)} KB)</div>`).join('');
    $('#supportingFilePreview').html(html);
}

function collectHrActionData() {
    const formData = new FormData();
    formData.append('GrievanceID', $('#hrActionGrievanceID').val() || $('#grievanceID').val());
    formData.append('HREmpId', $('#hrEmpId').val());
    formData.append('HRName', $('#hrName').val());
    const selectedHrEmployee = hrActionEmployeeRows.length > 0 ? hrActionEmployeeRows[0] : null;
    formData.append('ActionEmployeeId', (selectedHrEmployee ? selectedHrEmployee.employeeID : '') || $('#hrActionEmployeeId').val() || extractEmployeeCode($('#hrActionEmpSearch').val()));
    formData.append('ActionEmployeeName', (selectedHrEmployee ? selectedHrEmployee.employeeName : '') || $('#hrActionEmployeeName').val());
    formData.append('hrActionEmployeesJson', JSON.stringify(hrActionEmployeeRows.map(function (row) {
        return {
            employeeID: getFirstDefined(row.employeeID, row.EmployeeID),
            employeeName: getFirstDefined(row.employeeName, row.EmployeeName),
            positionTitle: getFirstDefined(row.positionTitle, row.PositionTitle),
            department: getDepartmentDisplayValue(row),
            employeeSignaturePath: getFirstDefined(row.employeeSignaturePath, row.EmployeeSignaturePath),
            employeeSignatureData: getFirstDefined(row.employeeSignatureData, row.EmployeeSignatureData)
        };
    })));
    formData.append('InvestigationSummary', $('#hrInvestigationSummary').val());
    formData.append('EmployeeExplanation', $('#hrEmployeeExplanation').val());
    formData.append('Remarks', $('#hrRemarks').val());
    formData.append('OutcomeResolved', $('#hrOutcomeResolved').is(':checked'));
    formData.append('OutcomeReferredToER', $('#hrOutcomeReferredToER').is(':checked'));
    formData.append('OutcomeReferredToDomesticInquiry', $('#hrOutcomeReferredToDomesticInquiry').is(':checked'));
    formData.append('MinorMisconduct', $('#hrMinorMisconduct').is(':checked'));
    formData.append('MajorMisconduct', $('#hrMajorMisconduct').is(':checked'));
    formData.append('MajorMisconductText', $('#hrMajorMisconductText').val());
    formData.append('HRSignaturePath', $('#hrSignaturePath').val());
    formData.append('hrSignatureData', $('#hrSignatureData').val());
    formData.append('Department', $('#hrFooterDepartment').val() || 'HUMAN RESOURCE');
    return formData;
}

async function saveHrAction() {
    if (!isHrUserLogin) return;

    const searchedEmployeeCode = extractEmployeeCode($('#hrActionEmpSearch').val());
    if (searchedEmployeeCode && hrActionEmployeeRows.length === 0) {
        await addHrActionEmployee();
    }

    if (hrActionEmployeeRows.length === 0) {
        $('#hrActionEmpSearch').removeClass('valid-border').addClass('error-border');
        alert('Please add at least one employee in HR action section.');
        return;
    }

    if (!validateHrActionEmployeeSignatures()) return;

    if (!validateSignature('hr', 'Please draw and save the HR signature.')) return;

    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await $.ajax({
        url: '/ERHRLetters/SaveHrAction',
        type: 'POST',
        data: collectHrActionData(),
        processData: false,
        contentType: false,
        headers: { 'RequestVerificationToken': token }
    });

    if (response.success) {
        alert(response.message || 'HR action submitted successfully.');
        $('#grievanceModal').modal('hide');
        loadGrievances();
    } else {
        showFormMessage(response.message || 'Unable to submit HR action.', false);
    }
}

function setEmployeeFormReadonly(readonly) {
    $('#grievanceForm').find('input, textarea, select').not('#hrActionSection input, #hrActionSection textarea, #hrActionSection select').prop('disabled', readonly);
    $('#grievanceForm input[type="hidden"]').prop('disabled', false);
    $('#complainantName,#department,#positionTitle,#dateOfReportDisplay,#referenceNo,#declarationEmployee,#declarationDateDisplay').prop('disabled', false).prop('readonly', true);
    $('#employeeSignatureCanvas').toggleClass('signature-disabled', readonly);
    $('#btnClearEmployeeSignature,#btnSaveEmployeeSignature').prop('disabled', readonly);
    signaturePads.employee.readOnly = readonly;
}

function setHrActionVisibility(visible, canEdit) {
    isHrActionEditable = !!(visible && canEdit);
    $('#grievanceModal').toggleClass('hr-action-edit-mode', isHrActionEditable);
    $('#hrActionSection').toggleClass('hr-action-visible', visible);
    $('#btnHrSubmit').toggle(visible && canEdit);
    $('.hr-action-edit-only').toggle(visible && canEdit);
    $('.hr-required-star').toggle(canEdit);

    $('#hrActionSection').find('input, textarea, select').prop('disabled', !canEdit);
    $('#hrActionSection input[readonly]').prop('disabled', false).prop('readonly', true);
    $('#hrEmpId,#hrName,#hrCaseReferenceNo,#hrActionDate,#hrActionEmployeeName,#hrActionPositionTitle,#hrActionDepartment,#hrFooterEmpId,#hrFooterEmpName,#hrFooterDepartment').prop('disabled', false).prop('readonly', true);
    $('#hrSignatureCanvas').toggleClass('signature-disabled', !canEdit);
    signaturePads.hr.readOnly = !canEdit;

    renderHrActionEmployeeTable();

    if (!visible) {
        $('#btnHrSubmit').hide();
    }
}

function initSignaturePad(type) {
    const pad = signaturePads[type];
    const canvas = document.getElementById(pad.canvasId);
    if (!canvas || canvas.dataset.initialized === 'true') return;

    const startDraw = function (event) {
        if (pad.readOnly) return;
        event.preventDefault();
        pad.isDrawing = true;
        const point = getSignatureCanvasPoint(getPointerEvent(event), canvas);
        const ctx = canvas.getContext('2d');
        ctx.beginPath();
        ctx.moveTo(point.x, point.y);
    };

    const draw = function (event) {
        if (!pad.isDrawing || pad.readOnly) return;
        event.preventDefault();
        const point = getSignatureCanvasPoint(getPointerEvent(event), canvas);
        const ctx = canvas.getContext('2d');
        ctx.lineTo(point.x, point.y);
        ctx.stroke();
        pad.hasSignature = true;
        updateSignatureData(type);
        $('#' + pad.boxId).removeClass('error-border').addClass('valid-border');
        $('#' + pad.validationId).addClass('d-none');
    };

    const stopDraw = function () {
        if (!pad.isDrawing) return;
        pad.isDrawing = false;
        if (pad.hasSignature) updateSignatureData(type);
    };

    canvas.addEventListener('mousedown', startDraw);
    canvas.addEventListener('mousemove', draw);
    document.addEventListener('mouseup', stopDraw);
    canvas.addEventListener('touchstart', startDraw, { passive: false });
    canvas.addEventListener('touchmove', draw, { passive: false });
    document.addEventListener('touchend', stopDraw);
    canvas.dataset.initialized = 'true';
}

function getPointerEvent(event) {
    return event.touches && event.touches.length > 0 ? event.touches[0] : event;
}

function resizeSignatureCanvas(type, preserveExisting) {
    const pad = signaturePads[type];
    const canvas = document.getElementById(pad.canvasId);
    if (!canvas) return;

    const oldData = preserveExisting && pad.hasSignature ? canvas.toDataURL('image/png') : '';
    const rect = canvas.getBoundingClientRect();
    canvas.width = Math.max(350, Math.floor(rect.width || canvas.parentElement.clientWidth || 500));
    canvas.height = Math.max(120, Math.floor(rect.height || 130));
    configureSignatureContext(canvas.getContext('2d'));

    if (oldData) drawSignatureImage(type, oldData);
}

function configureSignatureContext(ctx) {
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.strokeStyle = '#111';
}

function getSignatureCanvasPoint(event, canvas) {
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
    return {
        x: (event.clientX - rect.left) * scaleX,
        y: (event.clientY - rect.top) * scaleY
    };
}

function updateSignatureData(type) {
    const pad = signaturePads[type];
    const canvas = document.getElementById(pad.canvasId);
    if (!canvas || !pad.hasSignature) {
        $('#' + pad.dataId).val('');
        return;
    }

    $('#' + pad.dataId).val(canvas.toDataURL('image/png'));
    $('#' + pad.pathId).val('');
    pad.isSaved = false;
}

function saveEmployeeSignaturePreview(showMessage) {
    return saveSignaturePreview('employee', showMessage, 'Signature saved. Preview shown below.');
}

function saveHrSignaturePreview(showMessage) {
    return saveSignaturePreview('hr', showMessage, 'HR signature saved. Preview shown below.');
}

function saveSignaturePreview(type, showMessage, successText) {
    const pad = signaturePads[type];
    if (pad.readOnly) return false;

    if (!pad.hasSignature && !$('#' + pad.dataId).val() && !$('#' + pad.pathId).val()) {
        validateSignature(type, type === 'hr' ? 'Please draw and save the HR signature.' : 'Please draw and save the employee signature.');
        return false;
    }

    if (pad.hasSignature && !$('#' + pad.dataId).val() && !$('#' + pad.pathId).val()) {
        updateSignatureData(type);
    }

    const signatureSource = $('#' + pad.dataId).val() || $('#' + pad.pathId).val() || pad.pendingPath;
    if (!signatureSource) {
        validateSignature(type, type === 'hr' ? 'Please draw and save the HR signature.' : 'Please draw and save the employee signature.');
        return false;
    }

    showSignaturePreview(type, signatureSource, type === 'hr' && canEditHrActionSignature());

    if (type === 'hr' && canEditHrActionSignature()) {
        $('#' + pad.previewImgId).attr('src', signatureSource);
        $('#' + pad.previewContainerId).removeClass('d-none').show();
    }

    pad.isSaved = true;
    setSignatureValid(type);

    $('#' + pad.saveMessageId)
        .removeClass('d-none text-danger text-success')
        .addClass('text-success')
        .text(successText || 'Signature saved. Preview shown below.');

    if (type === 'hr' && canEditHrActionSignature()) {
        $('#' + pad.saveMessageId).removeClass('d-none').show();
    }

    return true;
}

function showSignaturePreview(type, src, forceShow) {
    const pad = signaturePads[type];
    if (!src) {
        hideSignaturePreview(type);
        return;
    }

    $('#' + pad.previewImgId).attr('src', src);
    if (isViewMode && !forceShow) {
        $('#' + pad.previewContainerId).addClass('d-none');
        return;
    }

    $('#' + pad.previewContainerId).removeClass('d-none');
}

function hideSignaturePreview(type) {
    const pad = signaturePads[type];
    $('#' + pad.previewImgId).attr('src', '');
    $('#' + pad.previewContainerId).addClass('d-none');
}

function setSignatureExistingPath(type, path) {
    const pad = signaturePads[type];
    $('#' + pad.pathId).val(path || '');
    $('#' + pad.dataId).val('');
    pad.pendingPath = path || '';
    pad.hasSignature = !!path;
    pad.isSaved = !!path;
    if (path) showSignaturePreview(type, path);
    else hideSignaturePreview(type);
}

function redrawSavedSignatureIfAvailable(type) {
    const pad = signaturePads[type];
    if (pad.pendingPath) drawSavedSignature(type, pad.pendingPath);
    else clearSignatureCanvasOnly(type);
}

function drawSavedSignature(type, path) {
    if (!path) return;
    setSignatureExistingPath(type, path);
    drawSignatureImage(type, path, function () {
        const pad = signaturePads[type];
        pad.hasSignature = true;
        pad.isSaved = true;
        showSignaturePreview(type, path);
        setSignatureValid(type);
    });
}

function drawSignatureImage(type, src, callback) {
    const pad = signaturePads[type];
    const canvas = document.getElementById(pad.canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    const image = new Image();
    image.onload = function () {
        clearSignatureCanvasOnly(type, false);
        const boxWidth = canvas.width;
        const boxHeight = canvas.height;
        const scale = Math.min(boxWidth / image.width, boxHeight / image.height);
        const drawWidth = image.width * scale;
        const drawHeight = image.height * scale;
        const x = (boxWidth - drawWidth) / 2;
        const y = (boxHeight - drawHeight) / 2;
        ctx.drawImage(image, x, y, drawWidth, drawHeight);
        configureSignatureContext(ctx);
        if (typeof callback === 'function') callback();
    };
    image.src = src;
}

function clearSignatureCanvasOnly(type, resetState) {
    const pad = signaturePads[type];
    const canvas = document.getElementById(pad.canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width || 500, canvas.height || 130);
    configureSignatureContext(ctx);

    if (resetState !== false) {
        pad.hasSignature = false;
        $('#' + pad.dataId).val('');
    }
}

function clearEmployeeSignature() {
    clearSignature('employee');
}

function clearHrSignature() {
    clearSignature('hr');
}

function clearSignature(type) {
    const pad = signaturePads[type];
    if (pad.readOnly) return;
    clearSignatureCanvasOnly(type);
    pad.pendingPath = '';
    pad.isSaved = false;
    $('#' + pad.pathId).val('');
    hideSignaturePreview(type);
    $('#' + pad.saveMessageId).addClass('d-none').text('');
    $('#' + pad.boxId).removeClass('valid-border').addClass('error-border');
    $('#' + pad.validationId).removeClass('d-none');
}

function validateSignature(type, message) {
    const pad = signaturePads[type];
    if (pad.hasSignature || $('#' + pad.dataId).val() || $('#' + pad.pathId).val()) {
        if (!pad.readOnly && !pad.isSaved) {
            saveSignaturePreview(type, false, type === 'hr' ? 'HR signature saved. Preview shown below.' : 'Signature saved. Preview shown below.');
        }
        setSignatureValid(type);
        return true;
    }

    $('#' + pad.boxId).removeClass('valid-border').addClass('error-border');
    $('#' + pad.validationId).removeClass('d-none');
    $('#' + pad.saveMessageId)
        .removeClass('d-none text-success')
        .addClass('text-danger')
        .text(message || 'Signature is required.');
    return false;
}

function setSignatureValid(type) {
    const pad = signaturePads[type];
    $('#' + pad.boxId).removeClass('error-border').addClass('valid-border');
    $('#' + pad.validationId).addClass('d-none');
}

function resetGrievanceForm() {
    $('#grievanceModal').removeClass('hr-action-edit-mode');
    $('#grievanceForm')[0].reset();
    $('#grievanceID').val(0);
    $('#hrActionID').val('');
    $('#hrActionGrievanceID').val('');
    $('#referenceNo').val('');
    $('#complainantEmpId').val('');
    $('#hrActionEmployeeId').val('');
    $('#hrActionEmployeeName,#hrActionPositionTitle,#hrActionDepartment,#hrActionEmpSearch').val('');
    hrActionEmployeeRows = [];
    renderHrActionEmployeeTable();
    $('#formMessage').removeClass('alert-success alert-danger').addClass('d-none').text('');
    $('#grievanceForm').find('input,textarea,select').removeClass('error-border valid-border').prop('disabled', false);
    $('#complainantName,#department,#positionTitle,#dateOfReportDisplay,#referenceNo,#declarationEmployee,#declarationDateDisplay').prop('readonly', true);
    involvedParties = [];
    renderInvolvedParties();
    selectedNatureRows = [];
    $('#natureSearchInput').val('');
    $('#natureDropdownPanel').addClass('d-none').empty();
    renderSelectedNatures();
    $('#supportingFilePreview').empty();
    $('#supportingFiles').hide().val('');
    resetSignature('employee');
    resetSignature('hr');
    setHrActionVisibility(false, false);
    isHrActionEditable = false;
    $('#grievanceModal').removeClass('modal-view-mode');
    isViewMode = false;
}

function resetSignature(type) {
    const pad = signaturePads[type];
    $('#' + pad.dataId + ',#' + pad.pathId).val('');
    $('#' + pad.validationId).addClass('d-none');
    $('#' + pad.saveMessageId).addClass('d-none').removeClass('text-danger').addClass('text-success').text('');
    $('#' + pad.boxId).removeClass('error-border valid-border');
    hideSignaturePreview(type);
    pad.hasSignature = false;
    pad.pendingPath = '';
    pad.isSaved = false;
    pad.readOnly = false;
    clearSignatureCanvasOnly(type);
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

function getDepartmentDisplayValue(data) {
    if (!data) return '';

    const directDepartment = getFirstDefined(data.department, data.Department);
    const code = getFirstDefined(data.departmentCode, data.DepartmentCode);
    const name = getFirstDefined(data.departmentName, data.DepartmentName);

    if (directDepartment) return formatDepartmentCodeName(directDepartment, name);
    return formatDepartmentCodeName(code, name);
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
