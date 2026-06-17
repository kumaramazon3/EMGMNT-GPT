let employees = [];        // Full employee data
let currentPage = 1;       // For pagination if used
let currentSort = {        // Track current sorting
    column: null,
    ascending: true
};
let sortColumn = "";
let sortAsc = true;

var addPopupModal;// Declare globally so all functions can access
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;// Email validation regex
let pageSize = 10;   // number of rows per page
const perms = window.userPermissions || {};// NEW: Extract permissions from session JSON

let canView = false;
let canEdit = true;
let canDelete = false;
// Fetch data once when the page loads
window.addEventListener('DOMContentLoaded', () => {
    // 1️ Fetch employees
    fetchEmployees();
    // 2️ Add dynamic filtering on input
    document.getElementById("txtSearch").addEventListener("input", () => {
        currentPage = 1; // reset to first page on search
        renderTable();
    });

    // 3 Disable Add button if user has no Addemp permission
    const perms = window.userPermissions || {};
    const canAdd = perms.Addemp === "Y";
    canView = perms.viewemp == null ||  perms.viewemp === "Y";
    canEdit = perms.editemp == null ||  perms.editemp === "Y" ;
    canDelete = perms.deleteemp === "Y";
    //debugger;

    // 4. Enable/Disable Add button with tooltip
    const addBtnWrapper = document.getElementById("addBtnWrapper");
    const addBtn = document.getElementById("btnAddEmployee");

    if (addBtn && addBtnWrapper) {
        if (canAdd) {
            addBtn.style.pointerEvents = "auto";
            addBtn.style.opacity = "1";
            addBtnWrapper.removeAttribute("title");
            addBtnWrapper.removeAttribute("data-bs-toggle");
            addBtnWrapper.removeAttribute("data-bs-placement");
        } else {
            addBtn.style.pointerEvents = "none";
            addBtn.style.opacity = "0.5";

            // Add tooltip to wrapper
            addBtnWrapper.setAttribute("title", "Add permission denied");
            addBtnWrapper.setAttribute("data-bs-toggle", "tooltip");
            addBtnWrapper.setAttribute("data-bs-placement", "top");

            // Initialize Bootstrap tooltip
            new bootstrap.Tooltip(addBtnWrapper);
        }
    }
});

function fetchEmployees() {
    fetch('/EmpMaster/GetEmployeeData')
        .then(response => {
            if (!response.ok) {
                if (response.status === 401) {
                    // Session expired → redirect to login
                    window.location.href = "/LoginUser/Login";
                }
                throw new Error("Network response was not ok");
            }
            return response.json();
        })
        .then(data => {
            employees = data || [];
            renderTable();
        })
        .catch(err => {
            console.error("Error:", err);
            showErrorModal(err);
            // Optional: do not redirect here, only redirect on 401
        });
}

function renderTable(filteredData = null) {
    const searchValue = (document.getElementById("txtSearch")?.value || "").toLowerCase();
    const data = (filteredData || employees).filter(emp =>
        (emp.empcode?.toLowerCase() || "").includes(searchValue) ||
        (emp.empname?.toLowerCase() || "").includes(searchValue) ||
        (emp.passportno?.toLowerCase() || "").includes(searchValue) ||
        (emp.newicno?.toLowerCase() || "").includes(searchValue) ||
        (emp.empname?.toLowerCase() || "").includes(searchValue) ||
        (emp.nationality?.toLowerCase() || "").includes(searchValue)
    );

    const pageSizeSelect = document.getElementById("pageSizeSelect");
    const btnCSV = document.getElementById("btnexposrtCSV");
    const btnExcel = document.getElementById("btnexposrtExcel");

    // --- Handle No Records Case ---
    if (data.length === 0) {
        document.getElementById("empBody").innerHTML = `
            <tr>
                <td colspan="12" class="text-center text-danger fw-bold">
                    No Records Found
                </td>
            </tr>
        `;

        document.getElementById("totalCount").innerText = "0";
        document.getElementById("currentPage").innerText = "0 / 0";

        // Disable controls
        pageSizeSelect.disabled = true;
        btnCSV.disabled = true;
        btnExcel.disabled = true;

        return; // Exit, no need to build table
    }

    // Re-enable controls when data exists
    pageSizeSelect.disabled = false;
    btnCSV.disabled = false;
    btnExcel.disabled = false;

    // Determine page size
    let pageSize = parseInt(pageSizeSelect?.value) || 10;
    if (pageSizeSelect?.value === "All") pageSize = data.length;

    const totalPages = Math.ceil(data.length / pageSize) || 1;
    if (currentPage > totalPages) currentPage = totalPages;

    const start = (currentPage - 1) * pageSize;
    const pageData = data.slice(start, start + pageSize);
    console.log(pageData);
    let body = "";
    pageData.forEach((emp, index) => {
        const sno = start + index + 1;

        const shortName = emp.empName && emp.empName.length > 20
            ? emp.empName.substring(0, 20) + "..."
            : emp.empName;

        // e.g., from userPermissions object in the outer scope
        body += `
        <tr>
            <td>${sno}</td>
            <td>${emp.empcode}</td>
            <td title="${emp.empName}">${shortName}</td>
            
            <td>${emp.department}</td>
            <td>${emp.subDepartment}</td>
            <td>${emp.section}</td>
            <td>${formatDate(emp.dateOfJoin)}</td>
            <td>${emp.designation}</td>
            <td>${emp.gender}</td>
            <td>${emp.nationality}</td>
            <td>
                <span title="${canView ? '' : 'View permission denied'}" style="display: inline-block;"
                      ${canView ? '' : 'data-bs-toggle="tooltip" data-bs-placement="top"'}>

                    <button class="btn btn-sm" onclick="viewEmployee('${emp.empcode}')" title="View" ${canView ? '' : 'disabled'}  >
                        <i class="fa fa-eye"></i>
                    </button>
                </span>

                <span  title="${canEdit ? '' : 'Edit permission denied'}" style="display: inline-block;"
                      ${canEdit ? '' : 'data-bs-toggle="tooltip" data-bs-placement="top"'}>

                    <button  class="btn btn-sm p-1 lh-1" onclick="editEmployee('${emp.empcode}')" title="Edit" ${canEdit ? '' : 'disabled'}>
                        <i class="fa fa-pencil"></i>
                    </button>
                </span>

                <span >
                    <button class="btn btn-sm" onclick="printEmployee('${emp.empcode}')" >
                        <i class="fa fa-print"></i>
                    </button>
                </span>
            </td>
        </tr>`;
    });

    document.getElementById("empBody").innerHTML = body;
    document.getElementById("totalCount").innerText = ` ${data.length}`;
    document.getElementById("currentPage").innerText = `${currentPage} / ${totalPages}`;

    // Enable Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[title]'));
    tooltipTriggerList.map(el => new bootstrap.Tooltip(el));
}


function renderTable_Old(filteredData = null) {
    const searchValue = (document.getElementById("txtSearch")?.value || "").toLowerCase();

    const data = (filteredData || employees).filter(emp =>
        (emp.empcode?.toLowerCase() || "").includes(searchValue) ||
        (emp.empname?.toLowerCase() || "").includes(searchValue) ||
        (emp.passportno?.toLowerCase() || "").includes(searchValue) ||
        (emp.newicno?.toLowerCase() || "").includes(searchValue) ||
        (emp.empname?.toLowerCase() || "").includes(searchValue) ||
        (emp.nationality?.toLowerCase() || "").includes(searchValue)
    );

    const pageSizeSelect = document.getElementById("pageSizeSelect");
    const btnCSV = document.getElementById("btnexposrtCSV");
    const btnExcel = document.getElementById("btnexposrtExcel");

    // --- Handle No Records Case ---
    if (data.length === 0) {
        document.getElementById("empBody").innerHTML = `
            <tr>
                <td colspan="12" class="text-center text-danger fw-bold">
                    No Records Found
                </td>
            </tr>
        `;

        document.getElementById("totalCount").innerText = "0";
        document.getElementById("currentPage").innerText = "0 / 0";

        // Disable controls
        pageSizeSelect.disabled = true;
        btnCSV.disabled = true;
        btnExcel.disabled = true;

        return; // Exit, no need to build table
    }

    // Re-enable controls when data exists
    pageSizeSelect.disabled = false;
    btnCSV.disabled = false;
    btnExcel.disabled = false;

    // Determine page size
    let pageSize = parseInt(pageSizeSelect?.value) || 10;
    if (pageSizeSelect?.value === "All") pageSize = data.length;

    const totalPages = Math.ceil(data.length / pageSize) || 1;
    if (currentPage > totalPages) currentPage = totalPages;

    const start = (currentPage - 1) * pageSize;
    const pageData = data.slice(start, start + pageSize);

    let body = "";
    pageData.forEach((emp, index) => {
        const sno = start + index + 1;

        const shortName = emp.empname && emp.empname.length > 20
            ? emp.empname.substring(0, 20) + "..."
            : emp.empname;

        // Assuming canView, canEdit, canDelete come from somewhere in your code,
        // e.g., from userPermissions object in the outer scope
        body += `
        <tr>
            <td>${sno}</td>
            <td>${emp.empcode}</td>
            <td title="${emp.empname}">${shortName}</td>
            
            <td>${emp.departmentcode}</td>
            <td>${emp.sectioncode}</td>
            <td>${formatDate(emp.dateofjoin)}</td>
            <td>${emp.designation}</td>
            <td>${emp.sex}</td>
            <td>${emp.nationality}</td>
            <td>
                <span title="${canView ? '' : 'View permission denied'}" style="display: inline-block;"
                      ${canView ? '' : 'data-bs-toggle="tooltip" data-bs-placement="top"'}>

                    <button class="btn btn-sm" onclick="viewEmployee('${emp.empcode}')" title="View" ${canView ? '' : 'disabled'}  >
                        <i class="fa fa-eye"></i>
                    </button>
                </span>

                <span  title="${canEdit ? '' : 'Edit permission denied'}" style="display: inline-block;"
                      ${canEdit ? '' : 'data-bs-toggle="tooltip" data-bs-placement="top"'}>

                    <button  class="btn btn-sm p-1 lh-1" onclick="editEmployee('${emp.empcode}')" title="Edit" ${canEdit ? '' : 'disabled'}>
                        <i class="fa fa-pencil"></i>
                    </button>
                </span>

                <span >
                    <button class="btn btn-sm" onclick="printEmployee('${emp.empcode}')" >
                        <i class="fa fa-print"></i>
                    </button>
                </span>
            </td>
        </tr>`;
    });

    document.getElementById("empBody").innerHTML = body;
    document.getElementById("totalCount").innerText = ` ${data.length}`;
    document.getElementById("currentPage").innerText = `${currentPage} / ${totalPages}`;

    // Enable Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[title]'));
    tooltipTriggerList.map(el => new bootstrap.Tooltip(el));
}

// Sorting function
function sortTable(column) {
    if (currentSort.column === column) {
        currentSort.ascending = !currentSort.ascending; // Toggle sort order
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

// Pagination functions
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

//Add an event listener for the dropdown
document.getElementById("pageSizeSelect").addEventListener("change", function () {
    const value = this.value;

    if (value === "All") {
        pageSize = employees.length; // show all records
    } else {
        pageSize = parseInt(value, 10); // convert string to integer
    }

    currentPage = 1; // reset to first page
    renderTable();   // re-render table with new page size
});

// Sorting
function sortTable(column) {
    if (sortColumn === column) {
        sortAsc = !sortAsc;
    } else {
        sortColumn = column;
        sortAsc = true;
    }

    employees.sort((a, b) => {
        if (a[column] < b[column]) return sortAsc ? -1 : 1;
        if (a[column] > b[column]) return sortAsc ? 1 : -1;
        return 0;
    });

    renderTable();
}

// CSV Export with date in filename
function exportTableToCSV() {
    const today = new Date();
    const day = String(today.getDate()).padStart(2, "0");
    const month = String(today.getMonth() + 1).padStart(2, "0");
    const year = today.getFullYear();
    const dateStr = `${day}${month}${year}`;
    const fileName = `${dateStr}_tblempmaster.csv`;

    // 🔍 VALIDATION: Check if table has data
    if (!employees || employees.length === 0) {
        alert("No data available to export!");
        return; // ❌ stop export
    }

    let csv = "Emp Code,Emp Name,Passport No,Blood Group,Department,Section,DOJ,Designation,Gender,Nationality\n";

    // 🚀 LOOP ALL EMPLOYEES (NOT ONLY PAGE RECORDS)
    employees.forEach(emp => {
        const doj = emp.dateOfJoin ? formatDate(emp.dateOfJoin) : "";

        csv += `=" ${emp.empcode} ",` +
            `"${emp.empName}",` +
            `"${emp.icNo}",` +
            `"${emp.bloodGroup}",` +
            `"${emp.department}",` +
            `"${emp.section}",` +
            `"${doj}",` +
            `"${emp.designation}",` +
            `"${emp.gender}",` +
            `"${emp.nationality}"\n`;
    });

    let blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    let link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
}

// Reuse date formatting function
function formatDate(dateStr) {
    if (!dateStr) return "";
    const date = new Date(dateStr);
    if (isNaN(date.getTime())) return dateStr;
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
}

// Excel Export with date in filename
function exportTableToExcel() {
    const today = new Date();
    const day = String(today.getDate()).padStart(2, "0");
    const month = String(today.getMonth() + 1).padStart(2, "0");
    const year = today.getFullYear();
    const dateStr = `${day}${month}${year}`;
    const fileName = `${dateStr}_tblempmaster.xls`;
    // 🔍 VALIDATION: Check if table has data
    if (!employees || employees.length === 0) {
        alert("No data available to export!");
        return; // ❌ stop export
    }
    let html = `
        <table border="1">
            <tr>
                <th>Emp Code</th>
                <th>Emp Name</th>
                <th>Passport No</th>
                <th>Blood Group</th>
                <th>Department</th>
                <th>Section</th>
                <th>DOJ</th>
                <th>Designation</th>
                <th>Sex</th>
                <th>Nationality</th>
            </tr>
    `;
    // 🚀 LOOP ALL EMPLOYEES (NOT ONLY PAGE RECORDS)
    employees.forEach(emp => {
        html += `
            <tr>
                <td style="mso-number-format:'\\@';">'${emp.empcode}</td>
                <td>${emp.empName}</td>
                <td>${emp.icNo}</td>
                <td>${emp.bloodGroup}</td>
                <td>${emp.department}</td>
                <td>${emp.section}</td>
                <td>${formatDate(emp.dateOfJoin)}</td>
                <td>${emp.designation}</td>
                <td>${emp.gender}</td>
                <td>${emp.nationality}</td>
            </tr>
        `;
    });

    html += "</table>";

    let blob = new Blob([html], { type: "application/vnd.ms-excel" });
    let link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    link.click();
}

// Function to open modal
function openAddPopup() {
    // 1. Reset all form controls
    clearForm();

    //if (addPopupModal) addPopupModal.show();
    // 2. Show the modal
    if (!addPopupModal) {
        // Initialize bootstrap modal if not already
        addPopupModal = new bootstrap.Modal(document.getElementById('addPopupModal'), {
            backdrop: 'static', // prevent click outside close
            keyboard: false      // prevent ESC key close
        });
    }
    addPopupModal.show();

    // 3. Set default / validation functions
    setTodayForUniformDate();// <--  Call the new function here
    setCurrentDateForInputs();
    restrictDOB();   // <-- APPLY THE DOB VALIDATION HERE
    setupEmailValidation(); // attach email validation now
    validateImageInput(); // Validate the image correct format or not.
    //validateEducationDoc();// Validate Correct format file uploading or not.
    //validateICDoc();// Validate IC doc Correct format file uploading or not.
    validateFiles();// Validate files (Allow only: .pdf,.doc,.docx,image)

    //preventNegativeNumbers(); // Not allow Negative Values.
    initializeDatePickers(); // Date format.
    setupForeignEmpRadioHandler();// // Foreign Radio button

    // 5. Find the new-empcode: Last Record + 1 = new-empcode
    loadNextEmpCode();

    // 6 Enable Enter key navigation between inputs/selects
    const formElements = document.querySelectorAll('input, select, textarea');
    formElements.forEach((el, index) => {
        el.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault(); // prevent form submission
                let nextIndex = index + 1;

                // Skip disabled or hidden fields
                while (
                    nextIndex < formElements.length &&
                    (formElements[nextIndex].disabled || formElements[nextIndex].offsetParent === null)
                ) {
                    nextIndex++;
                }

                if (nextIndex < formElements.length) {
                    formElements[nextIndex].focus();
                }
            }
        });
    });

    // 7. Display Current Date when CSS: .auto-date
    const today = new Date().toISOString().split("T")[0];
    document.querySelectorAll(".auto-date").forEach(input => input.value = today);

    // 8. Access Radio button
    setupRadioDisable("handphone", "hpdate");
    setupRadioDisable("laptop", "laptopdate");
    setupRadioDisable("tablet", "tabletdate");
    setupRadioDisable("companyphone", "companyphonedate");
    setupRadioDisable("walkietoki", "walkitokidate");
    setupRadioDisable("internet", "netaccessdate");
    setupRadioDisable("systemaccess", "systemaccessdate");
    setupRadioDisable("folderaccess", "folderaccessdate");

    // 9. CheckBox for Same as permanent address
    toggleAddressTextarea('sameaspermanent', 'address2');

}

// Function to fetch last emp code and set new emp code on popup
function loadNextEmpCode() {
    $.ajax({
        url: '/EmpMaster/GetLastEmpCode',
        type: 'GET',
        success: function (data) {
            if (data && data.lastEmpCode) {
                var last = data.lastEmpCode;               // e.g. "020755"
                var nextNum = parseInt(last, 10) + 1;      // integer 20756
                var length = last.length;                  // e.g. 6
                var nextCode = String(nextNum).padStart(length, '0');

                // Correct selector to match your HTML
                var $el = $('#empcode');  // <-- changed from #newempcode
                $el.text(nextCode);       // span, so use .text()
            } else {
                showErrorModal('GetLastEmpCode returned no data');
            }
        },
        error: function (err) {
            showErrorModal('Error fetching last emp code');
        }
    });
}

// Function to close modal
function closeAddPopup() {
    if (addPopupModal) addPopupModal.hide();
}
// Date Format: dd/mm/yyyy
function formatDate(dateStr) {
    if (!dateStr) return ""; // handle null/undefined

    const date = new Date(dateStr);

    // Check for invalid date
    if (isNaN(date.getTime())) return dateStr;

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0'); // months are 0-based
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
}

// show preview when user selects a photo
document.getElementById("img").addEventListener("change", function (event) {
    const file = event.target.files[0];

    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const img = document.getElementById("photoPreview");
            img.src = e.target.result;
            img.style.display = "block";
        }
        reader.readAsDataURL(file);
    }
});

// Display Current Date and prevent selecting past dates
function setTodayForUniformDate() {
    let today = new Date();
    let day = String(today.getDate()).padStart(2, '0');
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    // Format: yyyy-mm-dd (required for date input)
    let formattedDate = `${year}-${month}-${day}`;

    let uniformDateInput = document.getElementById("uniformdate");
    if (uniformDateInput) {
        uniformDateInput.value = formattedDate;  // Set default to today's date
        uniformDateInput.setAttribute("min", formattedDate);  // Prevent past dates
    }
}

// Display Past Date
function restrictDOB() {
    let today = new Date();

    // Calculate yesterday
    today.setDate(today.getDate() - 1);

    let day = String(today.getDate()).padStart(2, '0');
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    // Format required for input type="date": yyyy-mm-dd
    let maxDate = `${year}-${month}-${day}`;

    let dobInput = document.getElementById("dateofbirth");
    if (dobInput) {
        dobInput.setAttribute("max", maxDate);
    }
}

// Display Current Date
function setCurrentDateForInputs() {
    let today = new Date();
    let day = String(today.getDate()).padStart(2, '0');
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    let formattedDate = `${year}-${month}-${day}`; // yyyy-mm-dd

    let dateOfJoinInput = document.getElementById("dateofjoin");
    let resignedInput = document.getElementById("resigned");

    if (dateOfJoinInput) dateOfJoinInput.value = formattedDate;
    if (resignedInput) resignedInput.value = formattedDate;
}

// Email Listen to input event
function setupEmailValidation() {
    const emailInput = document.getElementById('email');
    if (!emailInput) return; // safety check
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    emailInput.addEventListener('input', () => {
        const emailValue = emailInput.value;

        if (emailValue === "" || emailRegex.test(emailValue)) {
            // Valid email or empty → remove highlight
            emailInput.style.borderColor = "";
            emailInput.style.backgroundColor = "";
        } else {
            // Invalid email → highlight input
            emailInput.style.borderColor = "red";
            emailInput.style.backgroundColor = "#ffe6e6"; // light red
        }
    });
}

// Highlight Invalid Input of imge
function validateImageInput() {
    const imgInput = document.getElementById('img');
    if (!imgInput) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg'];

    imgInput.addEventListener('change', () => {
        const file = imgInput.files[0];
        if (!file) {
            // No file selected → remove highlight
            imgInput.style.borderColor = "";
            imgInput.style.backgroundColor = "";
            return;
        }

        if (!allowedTypes.includes(file.type)) {
            // Invalid file → highlight input
            imgInput.style.borderColor = "red";
            imgInput.style.backgroundColor = "#ffe6e6"; // light red
            imgInput.value = ""; // clear the invalid file
        } else {
            // Valid file → remove highlight
            imgInput.style.borderColor = "";
            imgInput.style.backgroundColor = "";
        }
    });
}

// Validate the file type.
function validateEducationDoc() {
    const eduInput = document.getElementById('academicCertificateurl');
    if (!eduInput) return;

    const allowedExtensions = ['pdf', 'doc', 'docx'];

    eduInput.addEventListener('change', () => {
        const file = eduInput.files[0];
        if (!file) {
            // No file selected → remove highlight
            eduInput.style.borderColor = "";
            eduInput.style.backgroundColor = "";
            return;
        }

        // Extract file extension
        const fileExt = file.name.split('.').pop().toLowerCase();

        if (!allowedExtensions.includes(fileExt)) {
            // Invalid file → highlight input
            eduInput.style.borderColor = "red";
            eduInput.style.backgroundColor = "#ffe6e6"; // light red
            eduInput.value = ""; // clear the invalid file
        } else {
            // Valid file → remove highlight
            eduInput.style.borderColor = "";
            eduInput.style.backgroundColor = "";
        }
    });
}

// Validate IC doc
function validateICDoc() {
    const eduInput = document.getElementById('passportfileurl');
    if (!eduInput) return;

    const allowedExtensions = ['pdf'];

    eduInput.addEventListener('change', () => {
        const file = eduInput.files[0];
        if (!file) {
            // No file selected → remove highlight
            eduInput.style.borderColor = "";
            eduInput.style.backgroundColor = "";
            return;
        }

        // Extract file extension
        const fileExt = file.name.split('.').pop().toLowerCase();

        if (!allowedExtensions.includes(fileExt)) {
            // Invalid file → highlight input
            eduInput.style.borderColor = "red";
            eduInput.style.backgroundColor = "#ffe6e6"; // light red
            eduInput.value = ""; // clear the invalid file
        } else {
            // Valid file → remove highlight
            eduInput.style.borderColor = "";
            eduInput.style.backgroundColor = "";
        }
    });
}

function validateFiles() {
    const allowedTypes = [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif"
    ];

    document.querySelectorAll(".file-validate").forEach(input => {
        input.addEventListener("change", function () {
            const file = this.files[0];

            if (file && !allowedTypes.includes(file.type)) {
                alert("Invalid file type! Allowed types: PDF, DOC, DOCX, JPG, PNG, GIF.");
                this.value = "";  // Reset invalid file
            }
        });
    });
}


// Should not allow the Negative Values
function preventNegativeNumbers() {
    const numberInputs = ['jackno', 'tno', 'shino', 'capno', 'shoe'];

    numberInputs.forEach(id => {
        const input = document.getElementById(id);
        if (!input) return;

        // Prevent typing negative numbers
        input.addEventListener('input', () => {
            if (input.value < 0) {
                input.value = ''; // clear invalid negative value
                input.style.borderColor = 'red';
                input.style.backgroundColor = '#ffe6e6';
            } else {
                input.style.borderColor = '';
                input.style.backgroundColor = '';
            }
        });
    });
}

// Function to initialize Flatpickr on required fields

function initializeDatePickers() {
    // Check if Flatpickr is loaded
    if (typeof flatpickr === "undefined") {
        console.error("Flatpickr library is NOT loaded!");
        showErrorModal(err);
        return;
    }
    const dateFields = ["#dateofjoin", "#resigned", "#uniformDate"];
    dateFields.forEach(function (selector) {
        flatpickr(selector, {
            dateFormat: "d-m-Y",
            allowInput: true,
            maxDate: "today"
        });
    });
}

// Function to bind dropdowns based on the fetched data
function bindDropdowns(data) {
    // Iterate through the data and populate the respective dropdowns
    data.forEach(function (item) {
        switch (item.tableName) {

            case "tblarea":
                bindDropdown('#area', item);
                break;
            case "tblbanknames":
                bindDropdown('#bank', item);
                break;
            case "tblcategory":
                bindDropdown('#category', item);
                break;
            case "tbldepartment":
                bindDropdown('#departmentcode', item);
                break;
            case "tbldesignation":
                bindDropdown('#designation', item);
                break;
            case "tbleducation":
                bindDropdown('#edulevel', item);
                break;
            case "tblemployeetype":
                //bindDropdown('#emptype', item);
                bindEmpTypeDropdown('#emptype', item);
                break;
            case "tblhostelname":
                bindDropdown('#hostelname', item);
                break;
            case "tblrace":
                bindDropdown('#race', item);
                break;
            case "tblroute":
                bindDropdown('#route', item);
                break;
            case "tbnationality":
                bindDropdown('#nationality', item);
                break;
            case "tblmaritalstatus":
                bindDropdown('#maritalstatus', item);
                break;
            case "tbreligion":
                bindDropdown('#religion', item);
                break;
            case "tblhouseno":
                bindDropdown('#houseno', item);
                break;
            case "tbllicenses":
                bindDropdown('#licensetype', item);
                //loadCategories('#licensetype', item);
                break;
            case "tblrelation":
                bindDropdown('#relation', item);
                break;
            case "tbllanguage":
                bindDropdown('#languagespeaking', item);
                break;
            case "tblcap":
                bindDropdown('#capcolor', item);
                break;
            case "tblbloodgroup":
                bindDropdown('#bloodgroup', item);
                break;
            default:
                console.warn('Unknown TableName: ' + item.tableName);
        }
    });
}

// Helper function to bind data to a specific dropdown
function bindDropdown(dropdownSelector, item) {
    var dropdown = $(dropdownSelector);

    // Check if the dropdownSelector is "departmentcode"
    if (dropdownSelector === "#departmentcode") {
        // If it's departmentcode, display item.code - item.description
        dropdown.append('<option value="' + item.code + '">' + item.code + ' - ' + item.description + '</option>');
    } else {
        // For all other dropdowns, just display item.description
        dropdown.append('<option value="' + item.code + '">' + item.description + '</option>');
    }
}
function bindEmpTypeDropdown(dropdownSelector, item) {
    var dropdown = $(dropdownSelector);
    // Check if the dropdownSelector is "departmentcode"
    if (dropdownSelector === "#departmentcode") {
        // If it's departmentcode, display item.code - item.description
        dropdown.append('<option value="' + item.description + '">' + item.code + ' - ' + item.description + '</option>');
    } else {
        // For all other dropdowns, just display item.description
        dropdown.append('<option value="' + item.description + '">' + item.description + '</option>');
    }
}

// triggers when departmentcode is selected
$('#departmentcode').on('change', function () {
    var departmentCode = $(this).val();  // Get the selected department code

    if (departmentCode) {
        // Trigger the AJAX request to fetch sections based on department code
        fetchSections(departmentCode);
    } else {
        // If no department code is selected, clear the sections dropdown
        $('#sectioncode').empty().append('<option>--Select Section--</option>');
    }
});

// Function to make AJAX call to fetch sections
function fetchSections(departmentCode) {
    $.ajax({
        url: '/EmpMaster/GetSections',  // Controller method to fetch sections
        type: 'GET',
        data: { departmentCode: departmentCode },
        success: function (data) {
            // Call the function to bind sections to the dropdown
            bindSectionsToDropdown(data);
        },
        error: function (err) {
            console.error('Error fetching sections:', err);
            showErrorModal(err);
        }
    });
}

// Function to bind sections to the dropdown
function bindSectionsToDropdown(sections) {
    var sectionDropdown = $('#sectioncode');
    sectionDropdown.empty();  // Clear existing options
    sectionDropdown.append('<option>--Select Section--</option>');  // Default option

    // Sort sections in ascending order by section name
    sections.sort(function (a, b) {
        return a.sectionname.localeCompare(b.sectionname); // Sort by sectionname
    });

    // Append each section as an option
    sections.forEach(function (section) {
        sectionDropdown.append(
            '<option value="' + section.sectioncode + '">' + section.sectioncode + ' - ' + section.sectionname + '</option>'
        );
    });
}

document.getElementById("maritalstatus").addEventListener("change", function (e) {
    var val = this.value;

    var spouseName = document.getElementById("spousename");
    var noOfChildren = document.getElementById("noofchildren");
    var spouseWorkingRadios = document.querySelectorAll("input[name='isspouseworking']");

    if (val === "M") {
        // Enable when Married
        spouseName.disabled = false;
        noOfChildren.disabled = false;

        spouseWorkingRadios.forEach(function (r) {
            r.disabled = false;
        });
    } else {
        // Disable for other options
        spouseName.value = "";
        spouseName.disabled = true;

        noOfChildren.value = "";
        noOfChildren.disabled = true;

        spouseWorkingRadios.forEach(function (r) {
            r.checked = false;
            r.disabled = true;
        });
    }
});

// This function handles enabling/disabling the IC/Passport doc field
function setupForeignEmpRadioHandler() {
    // Get all radio buttons for 'foreignemp'
    var radios = document.querySelectorAll("input[name='foreignemp']");
    // Local Employee
    var newicno = document.getElementById("newicno");
    var docInput = document.getElementById("icfileurl");
    // Foregin Employee
    var passportno = document.getElementById("passportno");
    var passportfileurl = document.getElementById("passportfileurl");

    // If radios or docInput not found, do nothing
    if (!radios || !docInput) return;

    radios.forEach(function (radio) {
        radio.addEventListener("change", function (e) {
            var val = e.target.value;
            if (val === "N") {
                // If No → enable the file input
                docInput.disabled = false;
                newicno.disabled = false;
                passportno.disabled = true;
                passportfileurl.disabled = true;
                docInput.value = "";
                docInput.value = "";
                passportno.value = "";
            } else if(val === "Y") {
                // Otherwise → disable and clear
                passportno.disabled = false;
                passportfileurl.disabled = false;
                docInput.disabled = true;
                newicno.disabled = true;
                docInput.value = "";
                docInput.value = "";
                passportno.value = "";
            }
        });
    });

    // Trigger once on setup to reflect any pre-selected radio
    var checked = document.querySelector("input[name='foreignemp']:checked");
    if (checked) {
        checked.dispatchEvent(new Event("change"));
    }
}

// Error Popup Modal
function showErrorModal(message) {
    // Set the message into the modal body
    var msgElem = document.getElementById("errorModalMessage");
    msgElem.textContent = message;

    // Show the modal (Bootstrap 5 version)
    var errorModalEl = document.getElementById("errorModal");
    var modal = new bootstrap.Modal(errorModalEl);
    modal.show();
}

// 1. ALL CONTROLLERS LIST

// 34-Textboxs
const textboxControllers = ['empname', 'contract', 'newicno', 'passportno', 'email', 'pphone', 'spousename', 'noofchildren',
    'address1', 'address2', 'typehouseNo', 'vehicleno', 'vehicleno2', 'vehicleno3', 'accountno', 'epf', 'sosco', 'emergencycontactpersonnumber',
    'emergencycontactpersonname', 'taxno', 'EmergencyAddress', 'fieldstudy', 'others', 'jackqty', 'safetybeltqty', 'pantqty', 'earmuffqty', 'tshirtqty',
    'helmetqty', 'shoeqty', 'apronqty', 'capqty'];

// 23- Dropdowns
const dropdownControllers = ['departmentcode', 'sectioncode', 'designation', 'emptype', 'nationality', 'bloodgroup', 'religion', 'race', 'maritalstatus', 'hostelname',
    'houseno', 'route', 'licensetype', 'bank', 'edulevel', 'languagespeaking', 'proficiencylevel', 'jackno', 'tno', 'pantsize',
    'shoeno', 'capcolor', 'safetybeltsize'];

// 13- Radiobuttons
const radioControllers = ['foreignemp', 'sex', 'stayinhostel', 'transportneeded', 'drivinglicense', 'handphone', 'laptop', 'tablet', 'companyphone', 'walkietoki',
    'internet', 'systemaccess', 'folderaccess'];

// 13-Date Calenders
const dateInputs = ['dateofjoin', 'dateofbirth', 'uniformdate', 'hpdate', 'laptopdate', 'tabletdate', 'companyphonedate', 'walkitokidate', 'netaccessdate', 'systemaccessdate',
    'folderaccessdate', 'windowlogindate', 'companyemaildate'];

// 7-File Uploads
const uploadesInputs = ['img', 'icfileuploadurl', 'passportfileurl', 'academicCertificateurl', 'othercertificatesurl', 'resume', 'professionaldocurl'];

// 1-Checkbox
const checkboxInputs = ['sameaspermanent'];

// 2. CLEAR / RESET ALL CONTROLS
function clearForm() {
    // Textboxes
    textboxControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.value = "";
            el.classList.remove("highlight-error");
        }
    });

    // Dropdowns
    dropdownControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            // Set the dropdown value to the default option value, e.g. "0"
            el.value = "0";
            el.classList.remove("highlight-error");
        }
    });


    // Radio buttons
    radioControllers.forEach(name => {
        const radios = document.getElementsByName(name);
        radios.forEach(r => r.checked = false);
    });
    // Then set defaults
    resetAllRadios();

    // Dates
    dateInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.value = "";
            el.classList.remove("highlight-error");
        }
    });
    console.log("Form cleared.");
}

// Function to reset all radios, then set defaults for certain groups if you like
const radioDefaultValues = {
    sex: 'M',
    spouseworking: 'N',
    drivinglisence: 'N',
    emptransferring: 'N',
    wokitoki: 'N',
    tab: 'N',
    foreignemp: 'N',
    stayinhostel: 'N',
    transportneeded: 'N',
    hppass: 'N',
    carpass: 'N',
    computerpass: 'N',
    remotelogin: 'N'
};

function resetAllRadios() {
    radioControllers.forEach(function (groupName) {
        const radios = document.getElementsByName(groupName);
        if (!radios || radios.length === 0) {
            // No radio buttons with that name — skip
            return;
        }

        // First clear all
        for (let i = 0; i < radios.length; i++) {
            radios[i].checked = false;
        }

        // Then set default if specified
        const defaultVal = radioDefaultValues[groupName];
        if (defaultVal !== undefined) {
            for (let i = 0; i < radios.length; i++) {
                if (radios[i].value === defaultVal) {
                    radios[i].checked = true;
                    break;
                }
            }
        }
    });
}

// 3. VALIDATION FUNCTION BEFORE SAVE
function validateFormBeforeSave() {
    let isValid = true;

    // Reset old highlights
    document.querySelectorAll(".highlight-error").forEach(el => el.classList.remove("highlight-error"));

    // Validate textboxes
    textboxControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el && el.value.trim() === "") {
            el.classList.add("highlight-error");
            isValid = false;
        }
        else {
            console.log("have a data with controller ID: ", id);
        }
    });

    // Validate dropdowns
    dropdownControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el && (el.value === "" || el.value === "0" || el.value == null)) {
            el.classList.add("highlight-error");
            isValid = false;
        }
        else {
            console.log("have a data with controller ID: ", id);
        }
    });

    // Validate radio groups
    radioControllers.forEach(name => {
        const radios = document.getElementsByName(name);
        if (radios.length > 0) {
            const anyChecked = [...radios].some(r => r.checked);
            if (!anyChecked) {
                radios[0].parentElement.classList.add("highlight-error");
                isValid = false;
            }
        }
    });

    // Validate date inputs
    dateInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el && el.value.trim() === "") {
            el.classList.add("highlight-error");
            isValid = false;
        }
    });

    return isValid;
}

// 4. SAVE BUTTON CLICK EVENT
function saveEmployee() {

    if (!validateFormBeforeSave()) {
        alert("Please fill all required fields.");
        return;
    }

    // If validated successfully, Continue with your saving logic
    console.log("Saving employee...");
    // >>>> YOUR SAVE CODE GOES HERE <<<<
}

// Radio button function, if No then disable Date textbox
function setupRadioDisable(radioName, dateFieldId) {
    const radios = document.querySelectorAll(`input[name='${radioName}']`);
    const dateField = document.getElementById(dateFieldId);

    radios.forEach(radio => {
        radio.addEventListener("change", function () {
            dateField.disabled = (this.value === "no");
            if (this.value === "no") dateField.value = ""; // clear date
        });
    });
}

// Function to enable/disable textarea based on checkbox
function toggleAddressTextarea(checkboxId, textareaId) {
    const checkbox = document.getElementById(checkboxId);
    const textarea = document.getElementById(textareaId);

    if (!checkbox || !textarea) return; // safety check

    checkbox.addEventListener('change', function () {
        textarea.disabled = checkbox.checked;
    });
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






























