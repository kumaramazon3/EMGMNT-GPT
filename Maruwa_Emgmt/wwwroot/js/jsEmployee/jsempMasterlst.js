/************************************
 * GLOBAL VARIABLES
 ************************************/
let employees = [];          // Full employee data
let currentPage = 1;
let pageSize = 10;

let currentSort = {
    column: null,
    ascending: true
};

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const perms = window.userPermissions || {};

let canView = false;
let canEdit = false;
let canDelete = false;

/************************************
 * PAGE LOAD
 ************************************/
window.addEventListener('DOMContentLoaded', () => {

    // Permissions
    canView = perms.viewemp == null || perms.viewemp === "Y";
    canEdit = perms.editemp == null || perms.editemp === "Y";
    canDelete = perms.deleteemp === "Y";
    const canAdd = perms.Addemp === "Y";

    // Fetch data
    fetchEmployees();

    // Search
    document.getElementById("txtSearch")?.addEventListener("input", () => {
        currentPage = 1;
        renderTable();
    });

    // Page size
    document.getElementById("pageSizeSelect")?.addEventListener("change", function () {
        pageSize = this.value === "All" ? employees.length : parseInt(this.value, 10);
        currentPage = 1;
        renderTable();
    });

    // Add button permissions
    const addBtnWrapper = document.getElementById("addBtnWrapper");
    const addBtn = document.getElementById("btnAddEmployee");

    if (addBtn && addBtnWrapper) {
        if (!canAdd) {
            addBtn.style.pointerEvents = "none";
            addBtn.style.opacity = "0.5";
            addBtnWrapper.setAttribute("title", "Add permission denied");
            addBtnWrapper.setAttribute("data-bs-toggle", "tooltip");
            new bootstrap.Tooltip(addBtnWrapper);
        }
    }
});

/************************************
 * FETCH EMPLOYEES
 ************************************/
function fetchEmployees() {
    fetch('/EmpMaster/GetAllEmployeeList')
        .then(res => {
            if (res.status === 401) {
                window.location.href = "/LoginUser/Login";
                return;
            }
            if (!res.ok) throw new Error("Failed to load data");
            return res.json();
        })
        .then(data => {
            employees = data || [];
            renderTable();
        })
        .catch(err => {
            console.error(err);
            alert("Error loading employee data");
        });
}

//RENDER TABLE
function renderTable() {
    const searchValue = (document.getElementById("txtSearch")?.value || "").toLowerCase();
    let filteredData = employees.filter(emp =>
        (emp.empcode || "").toLowerCase().includes(searchValue) ||
        (emp.empName || "").toLowerCase().includes(searchValue) ||
        (emp.department || "").toLowerCase().includes(searchValue) ||
        (emp.nationality || "").toLowerCase().includes(searchValue)
    );

    const tbody = document.getElementById("empBody");
    const pageSizeSelect = document.getElementById("pageSizeSelect");
    const btnCSV = document.getElementById("btnexposrtCSV");
    const btnExcel = document.getElementById("btnexposrtExcel");

    if (filteredData.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="12" class="text-center text-danger fw-bold">
                    No Records Found
                </td>
            </tr>`;
        document.getElementById("totalCount").innerText = "0";
        document.getElementById("currentPage").innerText = "0 / 0";
        pageSizeSelect.disabled = btnCSV.disabled = btnExcel.disabled = true;
        return;
    }

    pageSizeSelect.disabled = btnCSV.disabled = btnExcel.disabled = false;

    let size = pageSizeSelect.value === "All"
        ? filteredData.length
        : parseInt(pageSizeSelect.value, 10) || 10;

    const totalPages = Math.ceil(filteredData.length / size);
    if (currentPage > totalPages) currentPage = totalPages;

    const start = (currentPage - 1) * size;
    const pageData = filteredData.slice(start, start + size);

    /*  <td>${emp.createdDate || ""}</td>*/
    console.log("Page Data: ",pageData);
    let html = "";
    pageData.forEach((emp, index) => {

        const sno = start + index + 1;
        const shortName = emp.empName && emp.empName.length > 20? emp.empName.substring(0, 20) + "...": emp.empName;
        const shortdesig = emp.designation && emp.designation.length > 15 ? emp.designation.substring(0, 15) + "..." : emp.designation;

        html += `
        <tr>
            <td>${sno}</td>
            <td>${emp.empcode}</td>
            <td title="${emp.empName}">${shortName}</td>
            <td>${emp.department || ""}</td>
            <td>${emp.subDepartment || ""}</td>
            <td>${emp.section || ""}</td>
            <td>${formatDate(emp.dateOfJoin)}</td>
            <td title="${emp.designation}">${shortdesig}</td>
           
            <td>${emp.gender || ""}</td>
            <td>${emp.nationality || ""}</td>
            <td>${emp.educationLevel || ""}</td>
        
            <td>
                <button class="btn btn-sm" onclick="viewEmployee('${emp.empcode}')" ${canView ? "" : "disabled"}>
                    <i class="fa fa-eye"></i>
                </button>
                <button class="btn btn-sm" onclick="editEmployee('${emp.empcode}')" ${canEdit ? "" : "disabled"}>
                    <i class="fa fa-pencil"></i>
                </button>
                <button class="btn btn-sm" onclick="printEmployee('${emp.empcode}')">
                    <i class="fa fa-print"></i>
                </button>
            </td>
        </tr>`;
    });

    tbody.innerHTML = html;
    document.getElementById("totalCount").innerText = filteredData.length;
    document.getElementById("currentPage").innerText = `${currentPage} / ${totalPages}`;
}

// SORTING
function sortTable(column) {

    if (currentSort.column === column) {
        currentSort.ascending = !currentSort.ascending;
    } else {
        currentSort.column = column;
        currentSort.ascending = true;
    }

    employees.sort((a, b) => {
        let valA = (a[column] || "").toString().toLowerCase();
        let valB = (b[column] || "").toString().toLowerCase();

        if (valA < valB) return currentSort.ascending ? -1 : 1;
        if (valA > valB) return currentSort.ascending ? 1 : -1;
        return 0;
    });

    renderTable();
}

//PAGINATION
function nextPage() {
    const totalPages = Math.ceil(employees.length / pageSize);
    if (currentPage < totalPages) {
        currentPage++;
        renderTable();
    }
}

function prevPage() {
    if (currentPage > 1) {
        currentPage--;
        renderTable();
    }
}

//EXPORT CSV
function exportTableToCSV() {

    if (!employees.length) {
        alert("No data available");
        return;
    }

    let csv = "Emp Code,Emp Name,Department,Section,DOJ,Designation,Gender,Nationality,Education\n";

    employees.forEach(emp => {
        csv += `"${emp.empcode}","${emp.empName}","${emp.department}","${emp.section}","${formatDate(emp.dateOfJoin)}","${emp.designation}","${emp.gender}","${emp.nationality}","${emp.educationLevel}"\n`;
    });

    downloadFile(csv, "tblempmaster.csv", "text/csv");
}

/************************************
 * EXPORT EXCEL
 ************************************/
function exportTableToExcel() {

    if (!employees.length) {
        alert("No data available");
        return;
    }

    let html = `<table border="1">
        <tr>
            <th>Emp Code</th><th>Emp Name</th><th>Department</th><th>Section</th>
            <th>DOJ</th><th>Designation</th><th>Gender</th><th>Nationality</th><th>Education</th>
        </tr>`;

    employees.forEach(emp => {
        html += `<tr>
            <td style="mso-number-format:'\\@';">${emp.empcode}</td>
            <td>${emp.empName}</td>
            <td>${emp.department}</td>
            <td>${emp.section}</td>
            <td>${formatDate(emp.dateOfJoin)}</td>
            <td>${emp.designation}</td>
            <td>${emp.gender}</td>
            <td>${emp.nationality}</td>
            <td>${emp.educationLevel}</td>
        </tr>`;
    });

    html += "</table>";
    downloadFile(html, "tblempmaster.xls", "application/vnd.ms-excel");
}

/************************************
 * HELPERS
 ************************************/
function formatDate(dateStr) {
    if (!dateStr) return "";
    const d = new Date(dateStr);
    if (isNaN(d)) return "";
    return `${String(d.getDate()).padStart(2, '0')}-${String(d.getMonth() + 1).padStart(2, '0')}-${d.getFullYear()}`;
}

function downloadFile(content, filename, type) {
    const blob = new Blob([content], { type });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = filename;
    link.click();
}


// Edit button function
function editEmployee(empcode) {
    if (!empcode) {
        alert("Invalid employee code");
        return;
    }
    const encodedEmpCode = encodeURIComponent(btoa(empcode));
    window.location.href = `/EmpMaster/EmpRegisteration/${encodedEmpCode}?mode=edit`;
}


// View Button function
function viewEmployee(empCode) {
    if (!empCode) {   // ✅ fixed variable name
        alert("Invalid employee code");
        return;
    }
    const encoded = encodeURIComponent(btoa(empCode));
    window.location.href = `/EmpMaster/EmpRegisteration/${encoded}?mode=view`;
}

// Print
async function printEmployee(empCode) {

    try {
        await $.ajax({
            url: `/EmpMaster/PrintEmployeePdf/${encodeURIComponent(empCode)}`,
            type: 'GET',
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data, status, xhr) {

                const contentType = xhr.getResponseHeader("content-type");

                if (!contentType || !contentType.includes("application/pdf")) {
                    const reader = new FileReader();
                    reader.onload = function () {
                        alert(reader.result || "Unknown error from server");
                    };
                    reader.readAsText(data);
                    return;
                }

                const blob = new Blob([data], { type: 'application/pdf' });
                const url = window.URL.createObjectURL(blob);

                const a = document.createElement('a');
                a.href = url;
                a.download = 'Employee_' + empCode + '.pdf';
                document.body.appendChild(a);
                a.click();

                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
            },
            error: function (xhr) {

                if (xhr.response) {
                    const reader = new FileReader();
                    reader.onload = function () {
                        alert(reader.result || "Server error occurred");
                    };
                    reader.readAsText(xhr.response);
                } else {
                    alert("Request failed. Status: " + xhr.status);
                }

                console.error("Status:", xhr.status);
            }
        });
    }
    catch (ex) {
        console.error("JS Error:", ex);
        alert("Unexpected client error occurred");
    }
}
