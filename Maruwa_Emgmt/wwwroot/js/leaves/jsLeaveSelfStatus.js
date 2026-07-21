let currentPage = 1;
let pageSize = 10;
let totalCount = 0;
let debounceTimer = null;

$(document).ready(function () {
    loadSelfStatus();
    $('#pageSizeSelect').on('change', function () { pageSize = parseInt($(this).val()); currentPage = 1; loadSelfStatus(); });
    $('#globalSearch').on('input', debounceSearch);
    $('.column-search').on('input', debounceSearch);
    $('#btnPrev').on('click', function () { if (currentPage > 1) { currentPage--; loadSelfStatus(); } });
    $('#btnNext').on('click', function () { const totalPages = Math.max(1, Math.ceil(totalCount / pageSize)); if (currentPage < totalPages) { currentPage++; loadSelfStatus(); } });
});

function debounceSearch() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(function () { currentPage = 1; loadSelfStatus(); }, 300);
}

function buildRequest() {
    const req = { globalSearch: $('#globalSearch').val(), appNo: '', leaveType: '', status: '', pageNumber: currentPage, pageSize: pageSize };
    $('.column-search').each(function () { req[$(this).data('field')] = $(this).val(); });
    return req;
}

async function loadSelfStatus() {
    try {
        const response = await $.ajax({ url: '/leave/GetSelfStatus', type: 'POST', contentType: 'application/json', data: JSON.stringify(buildRequest()) });
        if (!response.success) { alert(response.message || 'Unable to load leave status.'); return; }
        totalCount = response.totalCount || 0;
        renderTable(response.data || []);
        updatePaging();
    } catch (e) { alert('Error loading Leave Self Status.'); }
}

function renderTable(data) {
    let rows = '';
    data.forEach(item => {
        const status = item.status || '';
        const lowerStatus = String(status).toLowerCase();
        const statusClass = 'status-' + lowerStatus.replace(/\s+/g, '-');
        const canEditOrCancel = lowerStatus === 'scheduled';
        const appNoHtml = canEditOrCancel
            ? `<a href="/leave/LeaveApplication?appNo=${item.appNo}" title="Edit scheduled leave">${item.appNo || ''}</a>`
            : `${item.appNo || ''}`;
        const actionHtml = canEditOrCancel
            ? `<a class="btn btn-sm btn-primary mr-1" href="/leave/LeaveApplication?appNo=${item.appNo}">Edit</a><button class="btn btn-sm btn-danger" onclick="cancelLeave(${item.appNo})">Cancel</button>`
            : '';
        rows += `<tr>
            <td>${appNoHtml}</td>
            <td>${formatDate(item.applicationDate)}</td>
            <td>${formatNumber(item.leaveDays)}</td>
            <td>${formatDate(item.fromDate)} - ${formatDate(item.toDate)}${item.halfDayLeaveText ? '<br/><small>' + escapeHtml(item.halfDayLeaveText) + '</small>' : ''}</td>
            <td>${escapeHtml(item.leaveTypeName)}</td>
            <td>${escapeHtml(item.reasonText)}</td>
            <td>${escapeHtml(item.statusReason)}</td>
            <td>${escapeHtml(item.approvedBy)}</td>
            <td>${formatDate(item.approvedDate)}</td>
            <td class="${statusClass}">${escapeHtml(status)}</td>
            <td>${actionHtml}</td>
        </tr>`;
    });
    $('#tblLeaveSelfStatus tbody').html(rows || '<tr><td colspan="11" class="text-center">No records found</td></tr>');
}

async function cancelLeave(appNo) {
    if (!confirm('Are you sure you want to cancel the Leave?')) return;
    try {
        const response = await $.post('/leave/CancelLeave', { appNo: appNo });
        alert(response.message || 'Leave cancelled.');
        if (response.success) loadSelfStatus();
    } catch (e) { alert('Error while cancelling leave.'); }
}

function updatePaging() {
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);
    $('#recordInfo').text(`Total Records: ${totalCount}`);
    $('#btnPrev').prop('disabled', currentPage <= 1);
    $('#btnNext').prop('disabled', currentPage >= totalPages);
}

function formatDate(value) {
    if (!value) return '';
    const d = new Date(value);
    return isNaN(d) ? '' : d.toLocaleDateString();
}
function formatNumber(value) { const n = Number(value || 0); return Number.isInteger(n) ? n.toString() : n.toFixed(1); }
function escapeHtml(value) { return $('<div>').text(value || '').html(); }
