// ================== GLOBAL VARIABLES ==================
let leaveData = [];        // all records from API
let filteredData = [];     // filtered records after column search
let currentPage = 1;       // current page
let pageSize = 50;         // default page size
let data = []; // will store your full dataset

// ================== DOM CONTENT LOADED ==================
window.addEventListener('DOMContentLoaded', async () => {
    const bitval = true; //await UserAccessibility();
    if (bitval) {
        await DefaultFunctions();
    }
    else {
        $('#errorModal').modal('show');
    }
});

async function UserAccessibility() {
    if (!employeeSession) return false;

    try {
        // Replace &quot; with actual quotes and parse JSON
        const data = employeeSession.replaceAll("&quot;", '"');
        const emp = JSON.parse(data);

        const designation = emp.designation;
        console.log("Designation:", designation);

        // Case-insensitive check
        const allowedDesignations = [
            "director",
            "general manager (coo)",
            "chief executive officer (ceo)"
        ];

        // Convert designation to lowercase and trim spaces
        const userDesignation = designation.toLowerCase().trim();

        // Return true if matches, false otherwise
        return allowedDesignations.includes(userDesignation);

    } catch (error) {
        console.error("Error parsing employee session:", error);
        return false;
    }
}

async function DefaultFunctions() {

    // Load data from API
    await loadLeaveData();
    await GetmasterDesignationDropdown();
    await BindDepartmentcodeDropdown();
    await BindLeaveTypesDropdown();
    // Page size change
    document.getElementById("pageSizeSelect").addEventListener("change", function () {
        pageSize = parseInt(this.value);
        currentPage = 1;
        renderTable(filteredData);
    });

    // Next / Prev buttons
    document.getElementById("btnNext").addEventListener("click", function () {
        const totalPages = getTotalPages();
        if (currentPage < totalPages) {
            currentPage++;
            renderTable(filteredData);
        }
    });

    document.getElementById("btnPrev").addEventListener("click", function () {
        if (currentPage > 1) {
            currentPage--;
            renderTable(filteredData);
        }
    });

    // Column search with debounce
    let debounceTimer;
    $(document).on('input', '.column-search', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(applyColumnSearch, 250);
    });

}

document.getElementById("okBtn").addEventListener("click", function () {
    $('#errorModal').modal('hide');
    window.location.href = "/EmpMaster/empList";
});

// ================== LOAD DATA ==================
async function loadLeaveData() {
    try {
        const response = await fetch('/leave/GetScheduledLeave');
        if (!response.ok) throw new Error("Failed to fetch data");

        leaveData = await response.json();
        filteredData = leaveData; // initially show all
        renderTable(filteredData);

    } catch (error) {
        console.error("Error loading leave data:", error);
    }
}

// ================== RENDER TABLE ==================
function renderTable(data) {
    const tbody = document.getElementById("tblbodyleaveappovalData");
    tbody.innerHTML = "";

    if (!data || data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="12" class="text-center text-danger">No records found</td></tr>`;
        updatePageInfo(data);
        return;
    }

    let start = (currentPage - 1) * pageSize;
    let end = pageSize === 0 ? data.length : start + pageSize;

    const pageData = pageSize === 0 ? data : data.slice(start, end);
    console.log(pageData);

    pageData.forEach((item, index) => {
        const sno = index + 1;
        let empNameText = item.empName || "";
        let displayempName = empNameText.length > 15 ? empNameText.substring(0, 15) + ".." : empNameText;
        let reasonText = item.reason || "";
        let displayreason = reasonText.length > 25 ? reasonText.substring(0, 25) + ".." : reasonText;
        const leaveClass = getLeaveColorClass(item.leavetype, item.days);

        // *** NEW / FIXED CODE ***
        // Create a unique radio group name combining appno and index
        // This prevents multiple rows sharing the same radio button group name
        // which causes only one radio button to be selected across those rows.
        const radioName = `approval_${item.appno}_${index}`;
        // Check if the current item status is 'SCHEDULED' (case-insensitive)
        const isScheduled = item.status && item.status.trim().toUpperCase() === "SCHEDULED";
        const row = `
            <tr>
                <td>
                    ${sno || ''}
                    <input type="hidden" id="hdappno_${index}" name="appno" value="${item.appno || ''}">
                     <input type="hidden" id="hdleavetype_${index}" value="${item.leavetype || ''}">
                     <input type="hidden" id="hdnocf_${index}" value="${item.nocf || ''}">
                     <input type="hidden" id="hdcarryfwd_${index}" value="${item.carryfwd || ''}">
                     <input type="hidden" id="hdworkfor_${index}" value="${item.workfor || ''}">
                </td>

                <td>${formatDate(item.applicationdate)}</td>
                <td>${item.empCode}</td>
                <td title="${empNameText}">${displayempName}</td>
                <td class="text-end">${item.days}</td>
                <td style="width:100px">${formatDate(item.fromdate)} - ${formatDate(item.todate)}</td>

               <td class="${leaveClass} font-weight-bold">${item.leavetype}</td>
                <td>${item.designation}</td>
                <td>${item.department}</td>
                <td>${item.sectioncode}</td>
                <td title="${reasonText}">${displayreason}</td>

                <td>
                    <div class="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="scheduled_${radioName}" name="${radioName}" class="custom-control-input" value="Scheduled" ${isScheduled ? "checked" : ""}>
                        <label class="custom-control-label text-primary" for="scheduled_${radioName}">Scheduled</label>
                    </div>
                    <br>
                    <div class="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="approved_${radioName}" name="${radioName}" class="custom-control-input" value="Approved">
                        <label class="custom-control-label green" for="approved_${radioName}">Approved</label>
                    </div>
                    <br>
                    <div class="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="rejected_${radioName}" name="${radioName}" class="custom-control-input" value="Rejected">
                        <label class="custom-control-label red" for="rejected_${radioName}">Rejected</label>
                    </div>
                    <textarea class="form-control form-control-sm" id="remarks_${index}" placeholder="Enter remarks" rows="1"> </textarea>
                </td>
                <td>
                    <button class="btn btn-sm btn-danger"
                        title="History"
                        onclick="GetHistoryInfo(
                            '${item.empCode}',
                            '${empNameText.replace(/'/g, "\\'")}',
                            '${item.designation?.replace(/'/g, "\\'") || ""}',
                            '${item.department?.replace(/'/g, "\\'") || ""}',
                            '${item.sectioncode?.replace(/'/g, "\\'") || ""}'
                        )">

                        <i class="fas fa-history"></i>
                    </button>
                </td>
            </tr>
        `;
        tbody.insertAdjacentHTML("beforeend", row);
    });
    updatePageInfo(data);
}

function getLeaveColorClass(leaveType, days) {

    if (!leaveType) return "leave-other";

    const type = leaveType.toUpperCase().trim();

    switch (type) {

        // Case 2: Always RED
        case "EMERGENCY - ANNUAL":
        case "EMERGENCY UNPAID":
            return "leave-red";

        // Case 3: Always ORANGE
        case "MATERNITY":
        case "MEDICAL":
        case "PATERNITY":
        case "HOSPITALIZATION":
            return "leave-medical";

        // Case 1: Days based logic
        case "ANNUAL":
        case "CALAMITY":
        case "COMPANY HOLIDAY":
        case "COMPASSIONATE":
        case "REPLACEMENT LEAVE":
        case "UNPAID":
        case "MARRIAGE CHILDREN":
        case "MARRIAGE-SELF":
            return days > 2 ? "leave-red" : "leave-black";

        default:
            return "leave-other";
    }
}

function getLeaveColorClass_Old(leaveType) {

    if (!leaveType) return "leave-other";

    const type = leaveType.toUpperCase().trim();

    if (["ANNUAL", "EMERGENCY - ANNUAL", "PLAN EMERGENCY - ANNUAL"].includes(type))
        return "leave-annual";

    if (["UNPAID", "PLAN EMERGENCY - UNPAID"].includes(type))
        return "leave-unpaid";

    if (["MEDICAL", "MATERNITY", "HOSPITALIZATION"].includes(type))
        return "leave-medical";

    if (["MARRIAGE-SELF", "MARRIAGE CHILDREN"].includes(type))
        return "leave-marriage";

    if (["REPLACEMENT LEAVE"].includes(type))
        return "leave-replacement";

    if (type.includes("EMERGENCY"))
        return "leave-emergency";

    return "leave-other";
}

// ================== GET TOTAL PAGES ==================
function getTotalPages() {
    if (pageSize === 0) return 1;
    return Math.ceil(filteredData.length / pageSize);
}

// ================== UPDATE PAGE INFO ==================
function updatePageInfo(data) {
    const totalPages = pageSize === 0 ? 1 : Math.ceil(data.length / pageSize);
    document.getElementById("pageInfo").innerText =
        `Page ${currentPage} of ${totalPages}`;
}

// ================== FORMAT DATE ==================
function formatDate(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return date.toLocaleDateString();
}

// ================== COLUMN SEARCH ==================
function applyColumnSearch() {
    filteredData = leaveData.filter(row => {
        let isMatch = true;

        $('.column-search').each(function () {
            const field = $(this).data('field');
            const searchValue = $(this).val().toLowerCase().trim();

            if (searchValue) {
                const cellValue = (row[field] ?? '').toString().toLowerCase();
                if (!cellValue.includes(searchValue)) {
                    isMatch = false;
                    return false; // break .each()
                }
            }
        });

        return isMatch;
    });

    currentPage = 1;
    renderTable(filteredData);
}

async function GetHistoryInfo(empCode, empName, designation, department, sectioncode) {
    if (event) event.preventDefault();
    if (!empCode) return;
    try {
        const data = await $.ajax({
            url: `/leave/GetLeaveHistory?empCode=${empCode}`,
            type: 'GET',
            dataType: 'json'  // expects JSON from controller
        });

        // Populate modal content
        showHistoryModal(data);
        GetEmployeeLeaveFullSummary(empCode);
        // Set employee info in modal immediately
        $("#empName").text(empName);
        $("#designation").text(designation);
        $("#department").text(department + " - " + sectioncode);

        $("#CurrentyearBalanceAnnLeaves").text('...'); // You can fill if you have ALB info
        //$("#mlb").text('...'); // You can fill if you have MLB info


        // Show modal with static backdrop & disable ESC
        $('#historyModal').modal({
            show: true,         // show modal
            backdrop: 'static', // clicking outside does NOT close
            keyboard: false     // pressing ESC does NOT close
        });

    } catch (error) {
        console.error("Error fetching leave history:", error);
        alert("An error occurred while fetching leave history.");
    }
}

function showHistoryModal(data) {
    const modalBody = $("#historyModalBody");

    if (!data || data.length === 0) {
        modalBody.html("<p class='text-center text-muted my-3'>No leave history found.</p>");
        return; // DO NOT call $("#historyModal").modal("show") here!
    }

    let html = `
        <div class="table-responsive">
            <table class="table table-hover table-sm table-bordered mb-0" style="font-size:14px;">
                <thead class="thead-light">
                    <tr>
                        <th>Sno</th>
                        <th>Status</th>
                        <th>Application Date</th>
                        <th>Work For</th>
                        <th>From Date</th>
                        <th>To Date</th>
                        <th>Leave Type</th>
                        <th>Reason</th>
                        <th>Carry Fwd</th>
                        <th>Back Date</th>
                    </tr>
                </thead>
                <tbody>`;

        data.forEach((item, index) => {
            const sno = index + 1;
            html += `
                <tr>
                    <td>${sno || ''}</td>
                    <td>${item.status || ''}</td>
                    <td>${formatDate(item.applicationdate)}</td>
                    <td>${item.workfor || ''}</td>
                    <td>${formatDate(item.fromdate)}</td>
                    <td>${formatDate(item.todate)}</td>
                    <td>${item.leavetype || ''}</td>
                    <td title="${item.reason || ''}">${truncateText(item.reason, 30)}</td>
                    <td>${item.carryfwd || ''}</td>
                    <td>${item.backdate || ''}</td>
                </tr>`;
        });
    html += `
                </tbody>
            </table>
        </div>`;

    modalBody.html(html);
}

// Helper to format date as "dd/mm/yyyy"
function formatDate(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return `${d.getDate().toString().padStart(2, '0')}/${(d.getMonth() + 1).toString().padStart(2, '0')}/${d.getFullYear()}`;
}

// Helper to truncate long text
function truncateText(text, maxLength) {
    if (!text) return '';
    return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
}

function GetApprovalInfo(empCode, days, appno, workfor, nocf, carryfwd, empName, designation, department, sectioncode, fromdate, todate, leavetype) {

    // Bind values to labels
    $("#apprEmpCode").text(empCode);
    $("#apprEmpName").text(empName);
    $("#apprLeaveDates").text(formatDate(fromdate) + " - " + formatDate(todate));
    $("#apprLeaveType").text(leavetype);
    $("#apprnumberofdaysLeave").text(days);

    $("#hdappnoPopUpwindow").val(appno);
    $("#hdworkfor").val(workfor);
    $("#hdnocf").val(nocf);
    $("#hdcarryfwd").val(carryfwd);

    // Clear previous selection
    $("input[name='approvalType']").prop("checked", false);
    $("#approvalRemarks").val("");

    // Open modal (cannot close outside)
    $('#approvalModal').modal({
        show: true,
        backdrop: 'static',
        keyboard: false
    });
}

function clearApprovalForm() {

    $("input[name='approvalType']").prop("checked", false);
    $("#approvalRemarks").val("");
}

// Drop Down List Binding
function GetmasterDesignationDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Designation',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddldesignation');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Desig ---'));
            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.designationname).text(item.designationname)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading designationname dropdown', xhr);
            }
        }
    });
}
function BindDepartmentcodeDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#ddldepartment');
            $ddl.empty();
            // Default option
            $ddl.append($('<option></option>').val('').text('--- Select Department ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>')
                        .val(item.departmentCode)
                        .text(item.departmentCode + ' - ' + item.departmentName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading departmentName dropdown', xhr);
            }
        }
    });
}
function BindLeaveTypesDropdown() {
    $.ajax({
        url: '/leave/Getmaster_LeaveTypes',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            //console.log(response);
            var $ddl = $('#leavetype');
            $ddl.empty();
            // Default option
            $ddl.append($('<option></option>').val('').text('--- Select LeaveType ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append($('<option></option>').val(item.leaveType).text(item.ltCode + ' - ' + item.leaveType));
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading departmentName dropdown', xhr);
            }
        }
    });
}

// Drop Down Events
function onDepartmentClick() {
    var departmentCode = $('#ddldepartment').val(); // get selected value
    if (departmentCode) {
        fetchSubDepartments(departmentCode); // call your existing function
    } else {
        $('#subDepartment').empty().append('<option>--Select Section--</option>');
    }
}

function fetchSubDepartments(departmentCode) {
    $.ajax({
        url: '/EmpMaster/GetSubDepartments',  // Controller method to fetch sections
        type: 'GET',
        data: { departmentCode: departmentCode },
        success: function (data) {
            // Call the function to bind sections to the dropdown
            bindSubDepartmentsToDropdown(data);
        },
        error: function (err) {
            console.error('Error fetching sections:', err);
            showErrorModal(err);
        }
    });
}
function bindSubDepartmentsToDropdown(SubDepartments) {
    var sectionDropdown = $('#subDepartment'); /*subdepartmentcode*/
    sectionDropdown.empty();  // Clear existing options
    sectionDropdown.append('<option>--Select Sub Dept--</option>');  // Default option

    SubDepartments.sort(function (a, b) {
        return a.subDepartmentName.localeCompare(b.subDepartmentName); // Sort by sectionname
    });
    // Append each section as an option
    SubDepartments.forEach(function (section) {
        sectionDropdown.append(
            '<option value="' + section.subDepartmentCode + '">' + section.subDepartmentCode + ' - ' + section.subDepartmentName + '</option>'
        );
    });
}

// triggers when Sub-departmentcode is selected
$('#subDepartment').on('change', function () {
    var SubDeptCode = $(this).val();  // Get the selected department code
    if (SubDeptCode) {
        fetchGetSubDeptSections(SubDeptCode);
    } else {
        $('#section').empty().append('<option>--Select Section--</option>');
    }
});
// Function to make AJAX call to fetch sections
function fetchGetSubDeptSections(SubDeptCode) {
    $.ajax({
        url: '/EmpMaster/GetSubDeptSections',  // Controller method to fetch sections
        type: 'GET',
        data: { SubDeptCode: SubDeptCode },
        success: function (data) {
            // Call the function to bind sections to the dropdown
            bindSubDeptSectionsToDropdown(data);
        },
        error: function (err) {
            console.error('Error fetching sections:', err);
            showErrorModal(err);
        }
    });
}

function bindSubDeptSectionsToDropdown(sections) {
    var sectionDropdown = $('#section'); /*sectioncode*/
    sectionDropdown.empty();  // Clear existing options
    sectionDropdown.append('<option>--Select Section--</option>');  // Default option

    // Sort sections in ascending order by section name
    sections.sort(function (a, b) {
        return a.sectionName.localeCompare(b.sectionName); // Sort by sectionname
    });

    // Append each section as an option
    sections.forEach(function (section) {
        sectionDropdown.append(
            '<option value="' + section.sectionCode + '">' + section.sectionCode + ' - ' + section.sectionName + '</option>'
        );
    });
}

// Filter Table Data based on user selection:
function filterTable() {

    const selectedDesig = document.getElementById("ddldesignation").value.trim();
    const selectedDept = document.getElementById("ddldepartment").value.trim();
    const selectedSection = document.getElementById("section").value.trim();
    const selectedleavetype = document.getElementById("leavetype").value.trim();
    function isValid(val) {
        return val && !val.startsWith("--");
    }

    //filteredData = leaveData.filter(item =>
    //    (!isValid(selectedDesig) || item.designation?.toLowerCase().trim() === selectedDesig.toLowerCase()) &&
    //    (!isValid(selectedDept) || String(item.department).trim() === selectedDept.split("-")[0].trim()) &&
    //    (!isValid(selectedSection) || item.sectioncode?.toLowerCase().trim() === selectedSection.toLowerCase()) &&
    //    (!isValid(selectedleavetype) || item.leavetype?.toLowerCase().trim() === selectedleavetype.toLowerCase())
    //);

    filteredData = leaveData.filter(item =>
        (!isValid(selectedDesig) || item.designation?.toLowerCase().trim() === selectedDesig.toLowerCase()) &&
        (!isValid(selectedDept) || String(item.department).trim() === selectedDept.split("-")[0].trim()) &&
        (!isValid(selectedSection) || item.sectioncode?.toLowerCase().trim() === selectedSection.toLowerCase()) &&
        (!isValid(selectedleavetype) || (
            selectedleavetype.toLowerCase().includes("emergency") ? item.leavetype?.toLowerCase().includes("emergency")
                : item.leavetype?.toLowerCase().trim() === selectedleavetype.toLowerCase()
        ))
    );



    currentPage = 1;
    renderTable(filteredData);
}

function isValid(val) {
    return val && !val.startsWith("--");
}

// Submit button
document.getElementById("btnsubmit").addEventListener("click", async function () {
    const rows = document.querySelectorAll("#tblbodyleaveappovalData tr");
    let updates = [];

    rows.forEach((row, index) => {
        const appNo = row.querySelector(`#hdappno_${index}`)?.value || "";
        const empCode = row.children[2]?.innerText.trim();
        const selectedRadio = row.querySelector(`input[type="radio"]:checked`);

        if (!selectedRadio) return;                   // nothing selected
        if (selectedRadio.value === "Scheduled") return; // skip Scheduled

        const nocf = parseFloat(row.querySelector(`#hdnocf_${index}`)?.value || 0);
        const workfor = parseFloat(row.querySelector(`#hdworkfor_${index}`)?.value || 0);
        const leaveType = row.querySelector(`#hdleavetype_${index}`)?.value || "";
        const leavetypeCode = getLeaveTypeCode(leaveType);
        const carryfwd = row.querySelector(`#hdcarryfwd_${index}`)?.value || "";
        const remarks = row.querySelector(`#remarks_${index}`)?.value || "";

        const grantedLeaves = carryfwd === "Y" ? workfor + nocf : workfor;

        updates.push({
            AppNo: appNo.toString(),
            LeaveempCode: empCode,
            Status: selectedRadio.value,
            WorkFor: workfor.toString(),
            Nocf: nocf.toString(),
            Granted: grantedLeaves.toString(),
            LeavetypeID: leavetypeCode,
            carryfwd: carryfwd,
            Remarks: remarks
        });
    });

    if (updates.length === 0) {
        showMessageModal("Please select at least one record (Approved or Rejected) to submit.");
        return;
    }

    const confirmed = await showConfirmModal("Are you sure you want to submit the selected records?");
    if (!confirmed) return;

    // ✅ Send all updates at once
    await updateLeaveStatus(updates);

    loadLeaveData(); // reload table data after submit
});

async function updateLeaveStatus(data) {
    try {
        console.log("Sending to backend:", data);

        const response = await fetch("/leave/SubmitApproval", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data) // send flat array
        });

        if (!response.ok) throw new Error("Update failed");

        const result = await response.json();
        showMessageModal(result.message || "Leave approvals updated successfully.");
    } catch (error) {
        console.error(error);
        showMessageModal("Error updating leave approvals.");
    }
}


async function updateLeaveStatus(data) {
    try {
        console.log(data);
        const response = await fetch("/leave/SubmitApproval", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)  // always send array
        });

        if (!response.ok) throw new Error("Update failed");
        const result = await response.json();
        showMessageModal(result.message || "Leave approvals updated successfully.");

    } catch (error) {
        console.error(error);
        showMessageModal("Error updating leave approvals.");
    }
}


function showMessageModal(message) {
    document.getElementById("messageText").innerText = message;
    $("#messageModal").modal("show");
}

function showConfirmModal(message) {
    return new Promise((resolve) => {
        document.getElementById("confirmText").innerText = message;
        $("#confirmModal").modal("show");
        document.getElementById("btnYes").onclick = function () {
            $("#confirmModal").modal("hide");
            resolve(true);
        };
        $("#confirmModal").on("hidden.bs.modal", function () {
            resolve(false);
        });
    });
}


function showManagerModal(message) {
    return new Promise((resolve) => {
        // Set the modal message
        document.getElementById("managerModalText").innerText = message;

        // Show the modal
        $("#managerModal").modal("show");

        // Handle OK button click
        document.getElementById("managerOkBtn").onclick = function () {
            $("#managerModal").modal("hide");
            resolve(true);
        };

        // If modal is hidden by other means (just in case)
        $("#managerModal").on("hidden.bs.modal", function () {
            resolve(false);
        });
    });
}

async function GetEmployeeLeaveFullSummary(empCode) {
    if (!empCode) return;
    try {
        const data = await $.ajax({
            url: `/leave/GetEmployeeLeaveFullSummary?empCode=${empCode}`,
            type: 'GET',
            dataType: 'json'
        });

        // ✅ Bind Current Year --> data.currentYear -> first SP result
        $("#CurrentYearAnnLeaves").text((data.currentYear?.balAleave ?? 0) + (data.currentYear?.annual ?? 0));
        $("#divCurrentYearutilised").text(data.currentYear?.annual ?? 0);
        $("#CurrentyearBalanceAnnLeaves").text(data.currentYear?.balAleave ?? 0);
        //$("#mlb").text(data.currentYear?.balMleave ?? 0);

        // ✅ Bind Last Year --> data.lastYear -> second SP result
        $("#lstYearalb").text(data.lastYear?.actualAnnualLeaves ?? 0);// actualAnnualLeaves (Last Year)
        $("#divbalance").text(data.lastYear?.baL_LastYear_AnnualLeave ?? 0);// Balance Leaves

        $("#divutilised").text(
            (data.lastYear?.actualAnnualLeaves ?? 0) -
            (data.lastYear?.baL_LastYear_AnnualLeave ?? 0)
        );



        //$("#lstYearmlb").text(data.lastYear?.baL_LastYear_MedicalLeave ?? 0);

    } catch (error) {
        console.error("Error fetching balances:", error);
        alert("Failed to load leave balances.");
    }
}

function getLeaveTypeCode(ltype) {
    const map = {
        "ANNUAL": "AL",
        "Calamity": "CAL",
        "CompanyHoliday": "CH",
        "Compassionate": "CL",
        "Marriage-Children": "MAC",
        "Maternity": "ML",
        "Paternity": "PL",
        "PlanEmergency": "PEAL",
        "PlanEmergencyUP": "PLUP",
        "Hospitalization": "HL",
        "marriage-self": "MS",
        "Emergency": "AL",
        "EmergencyUP": "EUP",
        "Unpaid": "UP",
        "Medical": "MC"
    };

    return map[ltype] || ""; // return empty if not found
}
