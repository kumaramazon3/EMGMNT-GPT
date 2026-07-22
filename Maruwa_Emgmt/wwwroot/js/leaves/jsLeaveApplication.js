let leavePageData = null;
let summaryData = null;
let debounceTimer = null;
let picSearchTimer = null;
let allLeaveTypes = [];
let allPersonsInCharge = [];
let redirectAfterSuccess = false;

$(document).ready(function () {
    loadLeaveApplicationPageData();

    $('#isHalfDay').on('change', function () {
        const checked = $(this).is(':checked');
        $('#halfDayLeave').prop('disabled', !checked);
        if (checked) {
            const today = new Date().toISOString().slice(0, 10);
            $('#fromDate,#toDate').val(today);
        } else {
            $('#halfDayLeave').val('');
        }
        calculateLeaveDays();
    });

    $('#fromDate,#toDate').on('change', calculateLeaveDays);
    $('#leaveType').on('change', function () {
        setLeaveTypeSearchFromDropdown();
        applyReasonLogic();
        validateControl($(this));
        calculateLeaveDays();
    });
    $('#leaveTypeSearch').on('input', function () {
        filterLeaveTypes();
        syncLeaveTypeFromSearch(false);
        validateLeaveTypeSearch();
    });
    $('#leaveTypeSearch').on('change blur', function () {
        syncLeaveTypeFromSearch(true);
        validateLeaveTypeSearch();
    });
    $('#reason,#reasonText,#halfDayLeave,#leaveDays').on('change keyup', function () { validateControl($(this)); });

    $('#personInCharge1Search,#personInCharge2Search').on('focus', function () {
        if (!allPersonsInCharge || allPersonsInCharge.length === 0) {
            loadPersonsInCharge('');
        }
    });

    $('#personInCharge1Search,#personInCharge2Search').on('input', function () {
        const target = this.id === 'personInCharge1Search' ? '#personInCharge1' : '#personInCharge2';
        setPicHiddenValue('#' + this.id, target);
        clearTimeout(picSearchTimer);
        const searchText = $(this).val();
        picSearchTimer = setTimeout(function () { loadPersonsInCharge(searchText); }, 300);
    });

    $('#personInCharge1Search,#personInCharge2Search').on('change', function () {
        const target = this.id === 'personInCharge1Search' ? '#personInCharge1' : '#personInCharge2';
        setPicHiddenValue('#' + this.id, target);
        validateControl($(this));
    });

    $('#btnApplyLeave').on('click', applyLeave);

    $('#leaveResultModal').on('hidden.bs.modal', function () {
        if (redirectAfterSuccess) window.location.href = '/leave/LeaveSelfStatus';
    });
});

async function loadLeaveApplicationPageData() {
    try {
        const response = await $.get('/leave/GetLeaveApplicationPageData');
        if (!response.success) { showLeaveMessage(response.message || 'Unable to load Leave Application page.', false); return; }
        leavePageData = response.data;
        summaryData = leavePageData.summary;
        bindSummary(summaryData);
        bindDropdowns(leavePageData);
        const appNo = new URLSearchParams(window.location.search).get('appNo');
        if (appNo) await loadLeaveForEdit(appNo);
    } catch (e) {
        showLeaveMessage('Error loading Leave Application page.', false);
    }
}

function bindSummary(summary) {
    const emp = summary.employee || {};
    $('#empName').text(emp.empName || '');
    $('#designation').text(emp.designation || '');
    $('#department').text([emp.department, emp.subDepartment, emp.section].filter(Boolean).join(' - '));

    if (isLoggedInEmployeeOperator()) {
        $('#picBlock').addClass('d-none-force');
        $('#personInCharge1,#personInCharge2,#personInCharge1Search,#personInCharge2Search').val('');
    } else {
        $('#picBlock').removeClass('d-none-force');
    }

    $('#carryForwardTotal').text(formatNumber(summary.carryForwardTotal));
    $('#carryForwardUtilised').text(formatNumber(summary.carryForwardUtilised));
    $('#carryForwardBalance').text(formatNumber(summary.carryForwardBalance));
    $('#annualEntitlement').text(formatNumber(summary.annualEntitlement));
    $('#annualUtilised').text(formatNumber(summary.annualUtilised));
    $('#annualBalance').text(formatNumber(summary.annualBalance));
    $('#medicalEntitlement').text(formatNumber(summary.medicalEntitlement));
    $('#medicalUtilised').text(formatNumber(summary.medicalUtilised));
    $('#medicalBalance').text(formatNumber(summary.medicalBalance));
    $('#totalEntitlementBalance').text(formatNumber(summary.totalEntitlementBalance));
}

function isLoggedInEmployeeOperator() {
    const emp = summaryData?.employee || {};
    const designation = String(emp.designation || '').trim().toLowerCase();
    return !!emp.isOperator || designation === 'operator' || designation.includes('operator');
}

function bindDropdowns(data) {
    allLeaveTypes = data.leaveTypes || [];
    allPersonsInCharge = data.personsInCharge || [];
    fillSelect('#leaveType', allLeaveTypes, '- Select Leave type -');
    renderLeaveTypeOptions(allLeaveTypes);
    fillSelect('#halfDayLeave', data.halfDayLeaves, '-Select Half Day Leave Time -');
    fillSelect('#reason', data.reasons, '-Select-');
    renderPersonOptions(allPersonsInCharge);
    applyReasonLogic();
}

function fillSelect(selector, rows, firstText) {
    const ddl = $(selector);
    ddl.empty().append(`<option value="">${firstText}</option>`);
    (rows || []).forEach(item => {
        ddl.append(`<option value="${escapeAttr(item.id)}" data-text="${escapeAttr(item.text)}">${escapeHtml(item.text)}</option>`);
    });
}

function renderLeaveTypeOptions(rows) {
    const list = $('#leaveTypeOptions');
    if (!list.length) return;
    list.empty();

    const used = {};
    (rows || []).forEach(function (item) {
        const display = getLeaveTypeDisplayText(item);
        const key = String(item?.id || display || '').trim().toLowerCase();

        if (!display || used[key]) return;
        used[key] = true;

        list.append(`<option value="${escapeAttr(display)}"></option>`);
    });
}

function getLeaveTypeDisplayText(item) {
    const code = String(item?.id || '').trim();
    const text = String(item?.text || '').trim();

    if (code && text) {
        const lowerText = text.toLowerCase();
        const lowerCode = code.toLowerCase();

        if (lowerText === lowerCode) return code;
        if (lowerText.startsWith(lowerCode + ' - ')) return text;

        return `${code} - ${text}`;
    }

    return text || code;
}

function filterLeaveTypes() {
    const text = normalizeLookupText($('#leaveTypeSearch').val());
    const filtered = !text ? allLeaveTypes : allLeaveTypes.filter(function (x) {
        return normalizeLookupText(x.text).includes(text) ||
            normalizeLookupText(x.id).includes(text) ||
            normalizeLookupText(getLeaveTypeDisplayText(x)).includes(text);
    });
    renderLeaveTypeOptions(filtered.length ? filtered : allLeaveTypes);
}

function setLeaveTypeSearchFromDropdown() {
    const id = $('#leaveType').val();
    const item = (allLeaveTypes || []).find(function (x) { return String(x.id || '') === String(id || ''); });
    $('#leaveTypeSearch').val(id && item ? getLeaveTypeDisplayText(item) : '');
}

function syncLeaveTypeFromSearch(allowSingleMatch) {
    const text = normalizeLookupText($('#leaveTypeSearch').val());
    if (!text) {
        $('#leaveType').val('');
        applyReasonLogic();
        return;
    }

    const exactMatches = (allLeaveTypes || []).filter(function (x) {
        return normalizeLookupText(x.text) === text ||
            normalizeLookupText(x.id) === text ||
            normalizeLookupText(getLeaveTypeDisplayText(x)) === text;
    });

    const matches = exactMatches.length ? exactMatches : (allowSingleMatch ? (allLeaveTypes || []).filter(function (x) {
        return normalizeLookupText(x.text).includes(text) ||
            normalizeLookupText(x.id).includes(text) ||
            normalizeLookupText(getLeaveTypeDisplayText(x)).includes(text);
    }) : []);

    if (matches.length === 1) {
        $('#leaveType').val(matches[0].id);
        $('#leaveTypeSearch').val(getLeaveTypeDisplayText(matches[0]));
        applyReasonLogic();
        calculateLeaveDays();
    } else {
        $('#leaveType').val('');
        applyReasonLogic();
    }
}

function validateLeaveTypeSearch() {
    if ($('#leaveType').val()) {
        $('#leaveTypeSearch').removeClass('error-border').addClass('valid-border');
        return true;
    }
    if ($.trim($('#leaveTypeSearch').val() || '') !== '') {
        $('#leaveTypeSearch').removeClass('valid-border').addClass('error-border');
    } else {
        $('#leaveTypeSearch').removeClass('valid-border error-border');
    }
    return false;
}


function normalizeLookupText(value) {
    return String(value || '').trim().toLowerCase();
}

async function loadPersonsInCharge(searchText) {
    try {
        const response = await $.get('/leave/SearchPersonsInCharge', { searchText: searchText || '' });
        if (response.success) {
            allPersonsInCharge = response.data || [];
            renderPersonOptions(allPersonsInCharge);
        }
    } catch (e) { }
}

function renderPersonOptions(rows) {
    const list = $('#personInChargeOptions');
    list.empty();
    (rows || []).forEach(item => list.append(`<option value="${escapeAttr(item.text)}"></option>`));
}

function setPicHiddenValue(searchSelector, hiddenSelector) {
    const text = normalizeLookupText($(searchSelector).val());
    const match = (allPersonsInCharge || []).find(function (x) {
        return normalizeLookupText(x.text) === text ||
            normalizeLookupText(x.id) === text ||
            normalizeLookupText(`${x.id || ''} - ${x.text || ''}`) === text ||
            normalizeLookupText(x.text).startsWith(text + ' - ');
    });
    $(hiddenSelector).val(match ? match.id : '');
}

function setPicValue(hiddenSelector, searchSelector, id, text) {
    $(hiddenSelector).val(id || '');
    $(searchSelector).val(text || '');
    if (id && text && !(allPersonsInCharge || []).some(x => String(x.id) === String(id))) {
        allPersonsInCharge.push({ id: id, text: text });
        renderPersonOptions(allPersonsInCharge);
    }
}

function getLookupTextById(rows, id, fallback) {
    const match = (rows || []).find(x => String(x.id || '') === String(id || ''));
    return match ? (match.text || fallback || '') : (fallback || '');
}

function applyReasonLogic() {
    const type = String(getSelectedText('#leaveType') || '').toLowerCase();
    const isMedical = type.includes('medical');
    if (isMedical) {
        $('#reason').prop('disabled', false).removeClass('d-none').show();
        $('#reasonText').prop('disabled', true).addClass('d-none').hide().val('');
        $('#reasonLogicNote').text('Medical Leave: select Reason from dropdown.');
    } else {
        $('#reason').prop('disabled', true).addClass('d-none').hide().val('');
        $('#reasonText').prop('disabled', false).removeClass('d-none').show();
        $('#reasonLogicNote').text('Other Leave Types: enter Reason in text box.');
    }
}

async function calculateLeaveDays() {
    const from = $('#fromDate').val();
    const to = $('#toDate').val();
    const isHalfDay = $('#isHalfDay').is(':checked');
    $('#leaveDaysError').text('');
    $('#leaveDays').val('');
    if (!from || !to) return;
    try {
        const response = await $.ajax({
            url: '/leave/CalculateLeaveDays',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ fromDate: from, toDate: to, isHalfDay: isHalfDay })
        });
        if (!response.success) {
            $('#leaveDaysError').text(response.message || 'Please check the selected date.');
            return;
        }
        $('#leaveDays').val(response.leaveDays);
    } catch (e) {
        $('#leaveDaysError').text('Unable to calculate leave days.');
    }
}

function getSelectedText(selector) {
    const opt = $(selector).find('option:selected');
    return opt.data('text') || opt.text() || '';
}

function buildLeaveRequest() {
    const isMedical = String(getSelectedText('#leaveType') || '').toLowerCase().includes('medical');
    const reasonSelection = isMedical ? getSelectedText('#reason') : '';
    const additionalReason = String($('#reasonText').val() || '').trim();
    const isOperator = isLoggedInEmployeeOperator();
    return {
        appNo: $('#editAppNo').val() ? parseInt($('#editAppNo').val()) : null,
        leaveTypeId: $('#leaveType').val(),
        leaveTypeName: getSelectedText('#leaveType'),
        fromDate: $('#fromDate').val(),
        toDate: $('#toDate').val(),
        leaveDays: parseFloat($('#leaveDays').val() || '0'),
        isHalfDay: $('#isHalfDay').is(':checked'),
        halfDayLeaveId: $('#halfDayLeave').val() ? parseInt($('#halfDayLeave').val()) : null,
        halfDayLeaveText: $('#halfDayLeave').val() ? getSelectedText('#halfDayLeave') : '',
        reasonId: isMedical ? $('#reason').val() : null,
        reasonText: isMedical ? reasonSelection : additionalReason,
        personInCharge1: isOperator ? null : $('#personInCharge1').val(),
        personInCharge1Name: isOperator ? 'OPERATOR_SKIP' : getLookupTextById(allPersonsInCharge, $('#personInCharge1').val(), $('#personInCharge1Search').val()),
        personInCharge2: isOperator ? null : $('#personInCharge2').val(),
        personInCharge2Name: isOperator ? '' : getLookupTextById(allPersonsInCharge, $('#personInCharge2').val(), $('#personInCharge2Search').val()),
        annualBalance: parseFloat(summaryData?.annualBalance || 0),
        medicalBalance: parseFloat(summaryData?.medicalBalance || 0),
        carryForwardBalance: parseFloat(summaryData?.carryForwardBalance || 0)
    };
}

function validateBeforeSubmit(req) {
    let valid = true;
    $('.error-border').removeClass('error-border');

    const isMedical = String(req.leaveTypeName || '').toLowerCase().includes('medical');
    const isOperator = isLoggedInEmployeeOperator();

    if (!req.leaveTypeId) { markInvalid('#leaveTypeSearch'); valid = false; }
    if (!req.fromDate) { markInvalid('#fromDate'); valid = false; }
    if (!req.toDate) { markInvalid('#toDate'); valid = false; }
    if (!req.leaveDays || req.leaveDays < 0.5) { markInvalid('#leaveDays'); valid = false; }
    if (req.isHalfDay && !req.halfDayLeaveId) { markInvalid('#halfDayLeave'); valid = false; }
    if (isMedical && !req.reasonId) { markInvalid('#reason'); valid = false; }
    if (!isMedical && !req.reasonText) { markInvalid('#reasonText'); valid = false; }
    if (!isOperator && !req.personInCharge1) { markInvalid('#personInCharge1Search'); valid = false; }
    if (!isOperator && req.personInCharge1 && req.personInCharge1 === req.personInCharge2) { markInvalid('#personInCharge2Search'); showLeaveMessage('Person in Charge 1 and Person in Charge 2 should not be same.', false); return false; }

    const type = String(req.leaveTypeName || '').toLowerCase();
    const days = Number(req.leaveDays || 0);
    const from = new Date(req.fromDate + 'T00:00:00');
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const timeline = Math.floor((from - today) / (1000 * 60 * 60 * 24));

    if (type.includes('annual') && !type.includes('emergency')) {
        if (days > (Number(summaryData?.totalEntitlementBalance || 0))) { showLeaveMessage('No.of Days applied is greater than total entitlement balance.', false); return false; }
        if (days === 0.5 && timeline < 1) { showLeaveMessage('Leave should apply before one day.', false); return false; }
        if (days === 1 && timeline < 3) { showLeaveMessage('Leave should apply before three day.', false); return false; }
        if (days > 1 && timeline < 7) { showLeaveMessage('Leave should apply before seven day.', false); return false; }
    }
    if (type.includes('medical') && days > Number(summaryData?.medicalBalance || 0)) {
        showLeaveMessage('Cannot Apply!! Leave Applied is more than available Medical Leave.', false); return false;
    }

    if (!valid) showLeaveMessage('Please enter/select all mandatory fields.', false);
    return valid;
}

async function applyLeave() {
    await calculateLeaveDays();
    const req = buildLeaveRequest();
    if (!validateBeforeSubmit(req)) return;
    try {
        $('#btnApplyLeave').prop('disabled', true).text('Submitting...');
        const response = await $.ajax({ url: '/leave/ApplyLeave', type: 'POST', contentType: 'application/json', data: JSON.stringify(req) });
        showLeaveMessage(response.message || (response.success ? 'Leave has been scheduled.' : 'Leave not saved.'), response.success);
        if (response.success) redirectAfterSuccess = true;
    } catch (e) {
        showLeaveMessage('Error while applying leave.', false);
    } finally {
        $('#btnApplyLeave').prop('disabled', false).text($('#editAppNo').val() ? 'UPDATE LEAVE' : 'APPLY LEAVE');
    }
}

async function loadLeaveForEdit(appNo) {
    try {
        const response = await $.get('/leave/GetLeaveApplicationForEdit', { appNo: appNo });
        if (!response.success) { showLeaveMessage(response.message || 'Unable to edit selected leave.', false); return; }
        const d = response.data;
        $('#editAppNo').val(d.appNo);
        $('#leaveAppNo').val(d.appNo);
        $('#leaveType').val(d.leaveTypeId);
        setLeaveTypeSearchFromDropdown();
        $('#fromDate').val(formatInputDate(d.fromDate));
        $('#toDate').val(formatInputDate(d.toDate));
        $('#leaveDays').val(formatNumber(d.leaveDays));
        $('#isHalfDay').prop('checked', !!d.isHalfDay);
        $('#halfDayLeave').prop('disabled', !d.isHalfDay).val(d.halfDayLeaveId || '');
        applyReasonLogic();
        if (String(d.leaveTypeName || '').toLowerCase().includes('medical')) {
            $('#reason').val(d.reasonId || '');
        } else {
            $('#reasonText').val(d.reasonText || '');
        }
        setPicValue('#personInCharge1', '#personInCharge1Search', d.personInCharge1, d.personInCharge1Name);
        setPicValue('#personInCharge2', '#personInCharge2Search', d.personInCharge2, d.personInCharge2Name);
        $('#btnApplyLeave').text('UPDATE LEAVE');
        $('.leave-section-title').text('Edit Leave Details');
    } catch (e) {
        showLeaveMessage('Error loading leave application for edit.', false);
    }
}

function resetForm() {
    $('#editAppNo,#leaveTypeSearch,#leaveType,#fromDate,#toDate,#halfDayLeave,#leaveDays,#reason,#reasonText,#personInCharge1,#personInCharge2,#personInCharge1Search,#personInCharge2Search').val('');
    $('#leaveAppNo').val('Auto Generated');
    $('#isHalfDay').prop('checked', false);
    $('#halfDayLeave').prop('disabled', true);
    $('.error-border,.valid-border').removeClass('error-border valid-border');
    applyReasonLogic();
}

function markInvalid(selector) { $(selector).removeClass('valid-border').addClass('error-border'); }
function validateControl(control) {
    if (control.prop('disabled') || !control.is(':visible')) return;
    if ($.trim(control.val() || '') === '') control.removeClass('valid-border').addClass('error-border');
    else control.removeClass('error-border').addClass('valid-border');
}
function showLeaveMessage(message, success) {
    $('#leaveResultModalLabel').text(success ? 'Leave Submission' : 'Leave Validation');
    $('#leaveResultMessage').html(String(message || '').replace(/\n/g, '<br/>'));
    $('#leaveResultModal .modal-header').removeClass('bg-primary bg-danger bg-success').addClass(success ? 'bg-success' : 'bg-danger');
    $('#leaveResultModal').modal('show');
}
function formatNumber(value) { const n = Number(value || 0); return Number.isInteger(n) ? n.toString() : n.toFixed(1); }
function formatInputDate(value) { if (!value) return ''; const d = new Date(value); return isNaN(d) ? '' : d.toISOString().slice(0, 10); }
function escapeHtml(value) { return $('<div>').text(value || '').html(); }
function escapeAttr(value) { return String(value || '').replace(/'/g, '&#39;').replace(/"/g, '&quot;'); }
