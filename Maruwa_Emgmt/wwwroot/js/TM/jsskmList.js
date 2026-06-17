
$(document).ready(function () {
    initializeDropdownsAndYears();
    disableButtons();
    setupStartDateValidation();
    setupSkillTableKeyboardNavigation();
    setupScoreInputColoring();
    handleNavigationByDocNo();  // 👈 call here
});

// Initialize dropdowns and log years
function initializeDropdownsAndYears() {
    BindDepartmentcodeDropdown();
    loadLogYears();
    var logyear = document.getElementById("logyear");
    logyear.disabled = true;
}
// Disable buttons initially
function disableButtons() {
    $('#btnSubmitSkillMatrix').prop('disabled', true);
    $('#empCodeSearch').prop('disabled', true);
}

// Set max date and prevent future date input
function setupStartDateValidation() {
    const today = new Date().toISOString().split('T')[0];
    $('#startDate').attr('max', today);

    $('#startDate').on('change', function () {
        if (this.value > today) {
            this.value = today;
        }
    });
}

// Keyboard navigation for skill table
function setupSkillTableKeyboardNavigation() {
    $('#skillTable').on('keydown', 'input', function (e) {
        const $input = $(this);
        const $td = $input.closest('td');
        const $tr = $input.closest('tr');
        const colIndex = $td[0].cellIndex;
        const rowIndex = $tr[0].rowIndex;
        const $table = $('#skillTable')[0];

        const $inputsInCell = $td.find('input');
        const currentIndex = $inputsInCell.index($input);

        if (e.key === "Enter") {
            e.preventDefault();

            if (currentIndex < $inputsInCell.length - 1) {
                $inputsInCell.eq(currentIndex + 1).focus();
            } else {
                const nextRow = $table.rows[rowIndex + 1];
                if (nextRow) {
                    const nextCell = nextRow.cells[colIndex];
                    if (nextCell) {
                        const $nextInputs = $(nextCell).find('input');
                        if ($nextInputs.length) $nextInputs.eq(0).focus();
                    }
                }
            }
        } else if (e.key === "Tab") {
            e.preventDefault();
            const $nextCell = $td.next('td');
            if ($nextCell.length) {
                const $nextInputs = $nextCell.find('input');
                if ($nextInputs.length) $nextInputs.eq(0).focus();
            }
        }
    });
}

// Update background color dynamically for scores
function setupScoreInputColoring() {
    window.isSkillMatrixDirty = false;

    $('#skillTable').on('keyup change', '.score-input', function () {
        const value = $(this).val();
        let bgColor = "white";

        if (value === "0") bgColor = "#FFFF00";      // yellow
        else if (value === "1") bgColor = "#41fbfee3"; // light blue
        else if (value === "2") bgColor = "#fea521"; // orange
        else if (value === "3") bgColor = "#a1e964"; // green

        $(this).css('background-color', bgColor);
        window.isSkillMatrixDirty = true;
    });
}

// Navigation function from New Revistion Page
function handleNavigationByDocNo() {
    const params = new URLSearchParams(window.location.search);
    //const encDocNo = params.get('c');
    const encDept = params.get('d');
    const encSection = params.get('s');

    if (encDept && encSection) {
        const DocumentNo = "";//atob(encDocNo);
        const subDept = "";
        const department = atob(encDept);// TPH
        const sectionCode = atob(encSection);// 7160

        const sectionlist = getGroupValues(sectionCode);
        console.log(sectionlist);
        loadSkillMatrix(department, "newREV", sectionCode, sectionlist);

        // Call your main function---> 06-May-26
        //loadSkillMatrix(department, subDept, sectionCode);
        $("#span").html(`${department} - ${sectionCode}`);
    }
    //labelBind();
}

$(document).on('click', '#btnLoadSkillMatrix', function (e) {
    e.preventDefault();

    // Remove query string from URL
    const cleanUrl = window.location.origin + window.location.pathname;
    window.history.replaceState({}, document.title, cleanUrl);

    let $searchBtn = $('#btnLoadSkillMatrix');
    let $createBtn = $('#btskillmatrix');

    let isValid = true;
    const controls = ['#department', '#subDepartment', '#section'];
    controls.forEach(function (id) {
        const val = $(id).val();
        if (!val) {
            isValid = false;
            $(id).addClass('is-invalid');
        } else {
            $(id).removeClass('is-invalid');
        }
    });

    if (!isValid) return;
    // 🔹 Show loading and disable buttons
    $searchBtn.prop('disabled', true);
    $createBtn.prop('disabled', true);
    loadSkillMatrixFromControls();

    $searchBtn.prop('disabled', false);
    $createBtn.prop('disabled', false);

    //const departmentCode = $('#department option:selected').data('name');
    const departmentCode = $('#department option:selected').data('name');

    var sectionCode = $('#section').val();

    if (departmentCode && sectionCode && sectionCode.trim() !== '') {
        let combinedText = `${departmentCode} - ${sectionCode}`;
        $("#span").html(combinedText);
    }
});

function labelBind(departmentCode, sectionCode) {
    if (departmentCode && sectionCode && sectionCode.trim() !== '') {
        let combinedText = `${departmentCode} - ${sectionCode}`;
        $("#span").html(combinedText);
    }
}

function loadSkillMatrixFromControls() {
    //const departmentCode = $('#department option:selected').data('name');

    const fullText = $('#department option:selected').text();
    const deptCode = $('#department').val();
    var departmentCode = fullText.split('-')[1]?.trim();
    console.log(departmentCode);
    console.log(deptCode);
    if (deptCode === "3140" || deptCode === "3490") {
        departmentCode = "Ceramic Valve";
    }

    const subDepartmentCode = $('#subDepartment').val();
    const sectionCode = $('#section').val();
    const sectionFullName = $('#section option:selected').text();
    const sectioncod = $('#section').val();
    const sectionText = $('#section option:selected').data('name');
    const sectionlist = getGroupValues(sectionCode);


    console.log(sectionlist);
    let logDate = $('#logyear').val() || null;
    //loadSkillMatrix(departmentCode, subDepartmentCode, sectionCode, aliasName.sectionCode, logDate);
    loadSkillMatrix(departmentCode, subDepartmentCode, sectioncod, sectionlist, logDate);
}

/* ================================ LOAD DATA ================================*/
async function loadSkillMatrix(departmentCode, subDepartmentCode, sectionCode, aliasName, logDate) {
    try {
        let url = `/SkillMatrix/new_GetSkillMatrix?`;

        if (departmentCode) url += `departmentCode=${encodeURIComponent(departmentCode)}&`;
        if (subDepartmentCode) url += `subDepartmentCode=${encodeURIComponent(subDepartmentCode)}&`;
        if (aliasName) url += `aliasName=${encodeURIComponent(aliasName)}&`;
        if (sectionCode) url += `sectionCode=${encodeURIComponent(sectionCode)}&`;
        if (logDate) url += `logDate=${encodeURIComponent(logDate)}&`;

        const response = await fetch(url);

        // Parse JSON regardless of HTTP status
        const result = await response.json();

        if (response.ok) {
            // Success response (status 200-299)
            if (result.success === true && Array.isArray(result.data)) {
                generateSkillMatrix(result.data);
            } else {
                // Unexpected format even though HTTP status is OK
                showErrorModal(result.message);
            }
        } else {
            // Non-OK status (e.g. 500), show backend error message if available
            throw new Error(result.message || result.error || `HTTP Error: ${response.status}`);
        }
    } catch (error) {
        console.error("Error loading skill matrix:", error);
        showErrorModal(error.message);
        $("#tblSkillMatrixReport tbody").html(`
            <tr>
                <td colspan="16" class="text-center text-danger">
                    ${error.message}
                </td>
            </tr>
        `);
    }
}

/* ================================ GENERATE TABLE ================================ */

//bodyHtml += `<td class="fixed-col small-font font-weight-bold" style="font-size:0.6rem">${emp.empName ?? ''}</td>`;

function generateSkillMatrix(data) {
    $("#spndataType").html("");
    if (!Array.isArray(data) || data.length === 0) {
        $("#skillHead").html("");
        $("#skillBody").html(`
            <tr>
                <td colspan="5" class="text-center" style="color:red;">
                    No Data Found
                </td>
            </tr>
        `);

        $("#span").text("");
        $('#btnSubmitSkillMatrix').prop('disabled', true);
        return;
    }

    let logDate = $('#logyear').val() || null;

    // STEP 1: Get distinct columns
    const columns = [...new Set(
        data.map(x => x.columnNames).filter(x => x)
    )];

    // Emp validation
    const hasInvalidEmpCode = data.some(item =>
        !item.empCode || item.empCode === "undefined"
    );

    if (hasInvalidEmpCode) {
        $('#btnSubmitSkillMatrix').prop('disabled', true);

        if (logDate) {
            showModal(
                "Error",
                "Multiple skill matrix records are available for Section Code (" + $('#section').val() + "), but fewer employees exist in empMaster, causing the transaction to fail.",
                false
            );
        }
    }

    $('#empCodeSearch').prop('disabled', false);

    // STEP 2: Group Employees
    const employees = {};

    data.forEach(item => {
        if (!employees[item.empCode]) {
            employees[item.empCode] = {
                empName: item.empName,
                position: item.position,
                skills: {}
            };
        }
        employees[item.empCode].skills[item.columnNames] = item;
    });

    /* ================= HEADER ================= */

    let headHtml = "<tr>";
    headHtml += `<th class="emp-code-col font-weight-bold">Emp Code</th>`;
    headHtml += `<th class="emp-name-col font-weight-bold">Emp Name</th>`;
    headHtml += `<th class="position-col font-weight-bold">Position</th>`;

    columns.forEach(col => {

        const docInfo = data.find(x => x.columnNames === col);
        const documentName = docInfo?.documentName || '';

        headHtml += `
            <th class="text-center font-weight-bold"
                style="min-width:180px;font-size:0.7rem;
                white-space:normal;word-break:break-word;
                position:sticky; top:0; background:#f8f9fa; z-index:20;">

                <div>${col}</div>

                ${documentName
                ? `<div class="font-weight-bold text-primary"
                        style="font-size:0.65rem;line-height:1.2;margin-top:2px;">
                        ${documentName}
                       </div>`
                : ''
            }
            </th>`;
    });

    headHtml += "</tr>";

    $("#skillHead").html(headHtml);

    /* ================= BODY ================= */

    const today = new Date().toISOString().split('T')[0];

    let bodyHtml = "";

    $("#spndataType").html(`Emp Count: ${Object.keys(employees).length}`);

    Object.keys(employees).forEach(empCode => {

        const emp = employees[empCode];

        bodyHtml += "<tr style='font-size:0.6rem'>";
        bodyHtml += `<td class="emp-code-col font-weight-bold" style="font-size:0.6rem"> ${empCode ?? ''}</td>`;
        bodyHtml += `<td class="emp-name-col font-weight-bold" style="font-size:0.6rem;white-space:normal;word-break:break-word;"> ${emp.empName ?? ''}</td>`;
        bodyHtml += `<td class="position-col font-weight-bold" style="font-size:0.6rem"> ${emp.position ?? ''}</td>`;


        columns.forEach(col => {

            const skill = emp.skills[col] || {};

            const score = skill.score ?? '';
            const skillDate = skill.skillDate ? skill.skillDate.split('T')[0] : '';

            const currentYear = new Date().getFullYear();
            const minDate = `${currentYear}-01-01`;
            const maxDate = today;

            let bgColor = "white";

            if (score === "0") bgColor = "#FFFF00";
            else if (score === "1") bgColor = "#41fbfee3";
            else if (score === "2") bgColor = "#fea521";
            else if (score === "3") bgColor = "#a1e964";

            bodyHtml += `
                <td style="min-width:150px">
                    <input type="number"
                        class="form-control form-control-sm mb-1 text-center score-input"
                        min="0" max="3" step="1"
                        value="${score}"
                        style="background-color:${bgColor} !important;" />

                    <input type="date"
                        class="form-control form-control-sm mb-1 skill-date"
                        min="${minDate}"
                        max="${maxDate}"
                        value="${skillDate}" />
                </td>
            `;
        });

        bodyHtml += "</tr>";
    });

    $("#skillBody").html(bodyHtml);

    /* ================= BUTTON LOGIC ================= */

    if ($('#skillBody').find('tr').length > 0) {
        if (!logDate) {
            $('#btnSubmitSkillMatrix').prop('disabled', false);
        }
    } else {
        $('#btnSubmitSkillMatrix').prop('disabled', true);
    }

    if (columns.length === 0) {
        showErrorModal("No matching document information found.");
        $('#btnSubmitSkillMatrix').prop('disabled', true);
    }
}


function generateSkillMatrix_NoStikyHeader(data) {
    $("#spndataType").html("");
    if (!Array.isArray(data) || data.length === 0) {
        $("#skillHead").html("");
        $("#skillBody").html(`
            <tr>
                <td colspan="5" class="text-center" style="color:red;">
                    No Data Found
                </td>
            </tr>
        `);

        $("#span").text("");
        $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable
        return;
    }
    let logDate = $('#logyear').val() || null;
    // 🔹 STEP 1: Get distinct dynamic columns
    const columns = [...new Set(
        data.map(x => x.columnNames).filter(x => x)
    )];


    if (Array.isArray(data) && data.length > 0) {
        const firstRow = data[0];
        let combinedText = `${$('#department').val()} - ${$('#subDepartment').val()} - ${$('#section').val()}`;
        if (logDate) {
            combinedText += " <span class='badge bg-warning text-dark'>Log Table Data</span>";
        }
        /*$("#span").html(`Dept : ${combinedText}`);*/
    }

    // Check if any empCode is null, undefined, or empty
    const hasInvalidEmpCode = data.some(item =>
        !item.empCode || item.empCode === "undefined"
    );

    if (hasInvalidEmpCode) {

        $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable button
        if (logDate) {
            showModal("Error", "Multiple skill matrix records are available for Section Code (" + $('#section').val() + "), but fewer employees exist in empMaster, causing the transaction to fail.", false);
        }
        else {
            //showModal("Error", "Multiple skill matrix records exist for Section Code (" + $('#section').val() + "), but fewer employees are available in the empMaster table. Due to this mismatch, the transaction cannot be performed.", false);
        }
    } else {
        //$('#btnSubmitSkillMatrix').prop('disabled', false); // Enable button
    }


    $('#empCodeSearch').prop('disabled', false); // Disable

    // 🔹 STEP 2: Group Employees
    const employees = {};
    data.forEach(item => {
        if (!employees[item.empCode]) {
            employees[item.empCode] = {
                empName: item.empName,
                position: item.position,
                skills: {}
            };
        }
        employees[item.empCode].skills[item.columnNames] = item;
    });

    /* ===== HEADER ===== */
    let headHtml = "<tr>";
    headHtml += `<th class="fixed-col small font-weight-bold" style="font-size:0.9rem">Emp Code</th>`;
    headHtml += `<th class="fixed-col small font-weight-bold" style="font-size:0.9rem">Emp Name</th>`;
    headHtml += `<th class="fixed-col small font-weight-bold" style="font-size:0.9rem">Position</th>`;
    //columns.forEach(col => {
    //    headHtml += `<th class="text-center small font-weight-bold" style="min-width:95px;font-size:0.7rem">${col}</th>`;
    //});

    columns.forEach(col => {
        const docInfo = data.find(x => x.columnNames === col);
        const documentName = docInfo?.documentName || '';
        headHtml += `
                <th class="text-center small font-weight-bold"
                    data-column="${col}"
                    style="min-width:180px;font-size:0.7rem;white-space:normal;word-break:break-word;">

                    <div>${col}</div>

                    ${documentName
                                ? `<div class="font-weight-bold text-primary"
                               style="font-size:0.65rem;line-height:1.2;margin-top:2px;">
                               ${documentName}
                           </div>`
                                : ''
                            }
                </th>`;
             });

    headHtml += "</tr>";
    $("#skillHead").html(headHtml);

    /* ===== BODY ===== */
    const today = new Date().toISOString().split('T')[0];
    let bodyHtml = "";
    $("#spndataType").html(`Emp Count: ${Object.keys(employees).length}`);

    Object.keys(employees).forEach(empCode => {
        const emp = employees[empCode];
        bodyHtml += "<tr style='font-size:0.6rem'>";
        bodyHtml += `
<td class="emp-code-col font-weight-bold"
    style="font-size:0.6rem">
    ${empCode ?? ''}
</td>`;


        bodyHtml += `
<td class="fixed-col font-weight-bold">
    ${emp.empName ?? ''}
</td>`;

        bodyHtml += `
<td class="position-col font-weight-bold"
    style="font-size:0.6rem">
    ${emp.position ?? ''}
</td>`;

        // Dynamic skill columns
        columns.forEach(col => {
            const skill = emp.skills[col] || {};

            const score = skill.score ?? '';
            const skillDate = skill.skillDate ? skill.skillDate.split('T')[0] : '';

            // Restrict to current year only, max today
            const currentYear = new Date().getFullYear();
            const minDate = `${currentYear}-01-01`; // Jan 1 of current year
            const maxDate = today; // today

            let bgColor = "white";

            if (score === "0") bgColor = "#FFFF00";
            else if (score === "1") bgColor = "#41fbfee3";
            else if (score === "2") bgColor = "#fea521";
            else if (score === "3") bgColor = "#a1e964";

            bodyHtml += `
                <td style="min-width:150px">
                    <input type="number" class="form-control form-control-sm mb-1 text-center score-input" min="0" max="3" step="1"
                           value="${score}" style="background-color: ${bgColor} !important;" />
                 <input type="date" class="form-control form-control-sm mb-1 skill-date"
                   min="${minDate}" max="${maxDate}" value="${skillDate}" />
                </td>
            `;
        });
        bodyHtml += "</tr>";
    });

    $("#skillBody").html(bodyHtml);

    if ($('#skillBody').find('tr').length > 0) {
        let logDate = $('#logyear').val() || null;
        if (!logDate) {
            $('#btnSubmitSkillMatrix').prop('disabled', false); // Enable button
        }
    } else {
        $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable button
    }
    if (columns.length == 0) {
        showErrorModal("No matching document information found.");
        $('#btnSubmitSkillMatrix').prop('disabled', columns.length === 0);
    }
}

$('#logyear').on('change', function () {
    let logYear = parseInt($(this).val());
    const currentYear = new Date().getFullYear();
    if (logYear && logYear !== currentYear) {
        $('#btnSubmitSkillMatrix').prop('disabled', true);
    }
});

$(document).on('change', '#department, #subDepartment, #section', function () {
    if ($(this).val()) {
        $(this).removeClass('is-invalid');
    }
});

$(document).on("input", ".score-input", function () {
    let value = $(this).val();
    // Remove non-digits
    value = value.replace(/[^0-9]/g, '');
    if (value !== '') {
        const num = parseInt(value);
        if (num < 0) value = 0;
        if (num > 3) value = 3;
        // Allow only single digit 0-3
        if (!/^[0-3]$/.test(value)) {
            value = '';
        }
    }
    $(this).val(value);
});

//// Submit Button Click
$("#btnSubmitSkillMatrix").click(async function () {
    const skillData = [];
    var departmentCode = $('#department option:selected').data('name');
    const subDepartmentCode = $('#subDepartment').val();
    var edit;
    var sectionCode = $('#section').val();
    if (!sectionCode) {
        const params = new URLSearchParams(window.location.search);
        const encDept = params.get('d');
        const encSection = params.get('s');
        departmentCode = atob(encDept);// TPH
        sectionCode = atob(encSection);// 7160
        edit = "YES";
    }

    $("#skillBody tr").each(function () {
        const $tr = $(this);
        const empCode = $tr.find("td:eq(0)").text().trim();
        const empName = $tr.find("td:eq(1)").text().trim();
        const position = $tr.find("td:eq(2)").text().trim();

        $tr.find("td").slice(3).each(function () {
            const $td = $(this);
            const score = $td.find('input[type="number"]').val();
            const skillDate = $td.find('input[type="date"]').val();
            //const columnName = $("#skillHead th").eq($td.index()).text().trim();
            const columnName = $("#skillHead th")
                .eq($td.index())
                .find("div:first")
                .text()
                .trim();

            if (columnName) {
                skillData.push({
                    columnNames: columnName,
                    empCode: empCode,
                    empName: empName,
                    position: position,
                    departmentCode: departmentCode,
                    subDepartmentCode: subDepartmentCode,
                    sectionCode: extractSectionNumber(columnName),
                    score: score || null,
                    skillDate: skillDate || null
                });
            }
        });
    });
    try {
        const response = await fetch('/SkillMatrix/SaveSkillMatrix', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(skillData)
        });
        const result = await response.json();
        if (result.success) {
            if (edit=="YES") {
            handleNavigationByDocNo()
            }
            else {
                loadSkillMatrix();
            }
            document.getElementById("empCodeSearch").value = "";
            showModal("Success", result.message, true);

            // Clear old data
            $("#skillHead").empty();
            $("#skillBody").empty();

            loadSkillMatrixFromControls();
        } else {
            showModal("Error", result.message, false);
        }

    } catch (err) {
        showModal("Error", "Something went wrong while saving.", false);
    }
    setTimeout(() => {
        labelBind(departmentCode, sectionCode);
    }, 100);
});
// END

$(document).on('change', 'input[type="date"]', function () {
    const today = new Date().toISOString().split('T')[0];
    if (this.value > today) {
        alert("Future dates are not allowed.");
        this.value = today;
    }
});

// Function to show modal popup
function showModal(title, message, isSuccess = true) {
    var $modal = $("#modalPopup");

    $("#modalTitle").text(title);
    $("#modalBody").text(message);

    // Remove previous classes
    $modal.find(".modal-content").removeClass("modal-success modal-error");

    // Add class based on success or error
    if (isSuccess) {
        $modal.find(".modal-content").addClass("modal-success");
    } else {
        $modal.find(".modal-content").addClass("modal-error");
    }

    // Show modal (Bootstrap 4)
    $modal.modal({ backdrop: 'static', keyboard: false });
    $modal.modal('show');
}

function showErrorModal(message) {
    $('#errorModalBody').text(message);
    $('#errorModal').modal('show'); // Bootstrap 4 way
}

function closeModal() {
    $('#modalPopup').modal('hide'); // This will hide the modal programmatically
}

// Export into Excel Sheet
$("#btnExportExcel").click(function () {
    exportSkillMatrixToExcel();
});

function exportSkillMatrixToExcel() {
    var wb = XLSX.utils.book_new();

    // Create data array first
    var worksheetData = [];

    // ========== HEADER ROW ==========
    var headerRow = [];
    $("#skillHead th").each(function () {
        headerRow.push($(this).text().trim());
    });
    worksheetData.push(headerRow);

    // ========== DATA ROWS ==========
    $("#skillBody tr").each(function () {
        var dataRow = [];
        var $tds = $(this).find("td");

        $tds.each(function (colIndex) {
            if (colIndex < 3) {
                // Fixed columns
                dataRow.push($(this).text().trim());
            } else {
                // Dynamic skill columns - combine with CHAR(10) for line breaks
                var $td = $(this);

                // Get values
                var $scoreInput = $td.find('input[type="number"]');
                var score = $scoreInput.val() || "N/A";

                var $dateInput = $td.find('input[type="date"]');
                var dateValue = $dateInput.val() || "N/A";

                var $checkbox = $td.find('input[type="checkbox"]');
                var isChecked = $checkbox.is(':checked');
                var checkboxValue = isChecked ? "Yes" : "No";

                // Format date
                if (dateValue !== "N/A") {
                    var dateParts = dateValue.split('-');
                    if (dateParts.length === 3) {
                        dateValue = `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`;
                    }
                }

                // Create formula with CHAR(10) for line breaks (Excel's line break character)
                // This ensures proper line breaks in Excel
                var formattedValue = `Score: ${score}\nDate: ${dateValue}\nActive: ${checkboxValue}`;
                dataRow.push(formattedValue);
            }
        });

        worksheetData.push(dataRow);
    });

    // Create worksheet from data
    var ws = XLSX.utils.aoa_to_sheet(worksheetData);

    // ========== APPLY STYLES ==========
    var range = XLSX.utils.decode_range(ws['!ref']);

    // Style header row (row 0)
    for (var col = 0; col <= range.e.c; col++) {
        var headerCell = XLSX.utils.encode_cell({ r: 0, c: col });
        if (!ws[headerCell]) continue;

        // Make headers bold with proper styling
        ws[headerCell].s = {
            font: {
                bold: true,
                sz: 10
            },
            fill: {
                patternType: "solid",
                fgColor: { rgb: "F2F2F2" }
            },
            alignment: {
                horizontal: "center",
                vertical: "center",
                wrapText: true
            }
        };
    }

    // Style data rows
    for (var row = 1; row <= range.e.r; row++) {
        for (var col = 0; col <= range.e.c; col++) {
            var cellRef = XLSX.utils.encode_cell({ r: row, c: col });
            if (!ws[cellRef]) continue;

            if (col < 3) {
                // Fixed columns - left aligned
                ws[cellRef].s = {
                    font: { sz: 9 },
                    alignment: {
                        horizontal: "left",
                        vertical: "center"
                    }
                };
            } else {
                // Get score from the cell value to determine color
                var cellValue = ws[cellRef].v || "";
                var score = "N/A";

                // Extract score from the formatted text
                var scoreMatch = cellValue.match(/Score:\s*(\d+|N\/A)/);
                if (scoreMatch && scoreMatch[1] !== "N/A") {
                    score = parseInt(scoreMatch[1]);
                }

                // Get color based on score
                var bgColor = "FFFFFF";
                if (typeof score === 'number') {
                    bgColor = getExcelColorByScore(score);
                }

                // Apply style with proper formatting for line breaks
                ws[cellRef].s = {
                    font: { sz: 9 },
                    fill: {
                        patternType: "solid",
                        fgColor: { rgb: bgColor }
                    },
                    alignment: {
                        horizontal: "center",
                        vertical: "center",
                        wrapText: true,  // This enables line breaks
                        shrinkToFit: false
                    }
                };
            }
        }
    }

    // ========== SET COLUMN WIDTHS ==========
    var cols = [];

    // Fixed columns
    cols[0] = { wch: 12 }; // Emp Code
    cols[1] = { wch: 20 }; // Emp Name
    cols[2] = { wch: 15 }; // Position

    // Dynamic columns - wider for multi-line content
    for (var col = 3; col <= range.e.c; col++) {
        // Get header text length for dynamic width
        var headerText = worksheetData[0][col] || "";
        var width = Math.min(30, Math.max(18, headerText.length + 5));
        cols[col] = { wch: width };
    }

    ws['!cols'] = cols;

    // ========== SET ROW HEIGHTS ==========
    var rows = [];

    // Header row height
    rows[0] = { hpt: 30 };

    // Data rows - taller for multi-line content
    for (var row = 1; row <= range.e.r; row++) {
        rows[row] = { hpt: 65 }; // Tall enough for 3 lines
    }

    ws['!rows'] = rows;

    // Add the worksheet to workbook
    XLSX.utils.book_append_sheet(wb, ws, "Skill Matrix");

    // Save the file
    XLSX.writeFile(wb, "Skill_Matrix.xlsx");
}

function getExcelColorByScore(score) {
    switch (parseInt(score)) {
        case 0: return "FFFF00";  // Yellow
        case 1: return "41FBFE";  // Light Blue/Cyan
        case 2: return "FEA521";  // Orange
        case 3: return "A1E964";  // Light Green
        default: return "FFFFFF"; // White
    }


}
// END

// Dropdown List Binding
async function loadLogYears() {
    try {
        const response = await fetch('/SkillMatrix/GetLogYears');
        if (!response.ok) throw new Error("Network response was not ok");

        const data = await response.json();
        const dropdown = document.getElementById('logyear');
        dropdown.innerHTML = `<option value="">--- Select Year ---</option>`;
        console.log(data);

        data.forEach(item => {
            const option = document.createElement('option');
            option.value = item.logYear; // use lowercase as returned from server
            option.text = item.logYear;
            dropdown.appendChild(option);
        });

    } catch (error) {
        console.error("Error loading log years:", error);
    }
}
function BindDepartmentcodeDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#department');
            $ddl.empty();
            $ddl.append($('<option></option>').val('').text('--- Select Department ---'));
            //var allowedCodes = [7000, 3000, 4201];
            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>')
                        .val(item.departmentCode)
                        .attr('data-name', item.departmentName)
                        .text(item.departmentCode + ' - ' + item.departmentName)
                );

                //// show only allowed department codes
                //if (allowedCodes.includes(Number(item.departmentCode))) {

                //    $ddl.append(
                //        $('<option></option>')
                //            .val(item.departmentCode)
                //            .attr('data-name', item.departmentName)
                //            .text(item.departmentCode + ' - ' + item.departmentName)
                //    );
                //}
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

function onDepartmentClick() {
    var departmentCode = $('#department').val(); // get selected value
    if (departmentCode) {
        fetchSubDepartments(departmentCode); // call your existing function
    } else {
        $('#subDepartment').empty().append('<option value="">--Select Section--</option>');
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
    sectionDropdown.append('<option value="">--Select Sub Dept--</option>');  // Default option

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

$('#subDepartment').on('change', function () {
    var SubDeptCode = $(this).val();  // Get the selected department code
    if (SubDeptCode) {
        fetchGetSubDeptSections(SubDeptCode);
    } else {
        $('#section').empty().append('<option value="">--Select Section--</option>');
    }
});
// Function to make AJAX call to fetch sections
function fetchGetSubDeptSections(SubDeptCode) {
    $.ajax({
        url: '/SkillMatrix/GetSubDeptSections',  // Controller method to fetch sections
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
    sectionDropdown.append('<option value="">--Select Section--</option>');  // Default option

    // Sort sections in ascending order by section name
    sections.sort(function (a, b) {
        return a.sectionName.localeCompare(b.sectionName); // Sort by sectionname
    });

    // Append each section as an option
    sections.forEach(function (section) {
        sectionDropdown.append(
            '<option value="' + section.sectionCode + '" data-name="' + section.sectionName + '">'
            + section.sectionCode + ' - ' + section.sectionName +
            '</option>'
        );
    });
}

function handleSectionChange() {
    var section = document.getElementById("section");
    var logyear = document.getElementById("logyear");

    if (section.value && section.value !== "") {
        logyear.disabled = false;
    } else {
        logyear.disabled = true;
        logyear.value = ""; // reset
    }
}
// END



//Case A: Close browser tab
//Case B: Click browser back button
//Case C: Click menu / navigation inside your app
window.isSkillMatrixDirty = false;

// Track unsaved changes
// Mark dirty when any score or date changes
$('#skillTable').on('change keyup', 'input', function () {
    window.isSkillMatrixDirty = true;
});
//Handle browser/tab close or reload
window.addEventListener('beforeunload', function (e) {
    if (window.isSkillMatrixDirty) {
        e.preventDefault();
        e.returnValue = ''; // Chrome requires returnValue to be set
    }
});
//Handle internal navigation (menu clicks, back buttons)
// Example: intercept menu/navigation links
$('a.internal-link').on('click', function (e) {
    if (window.isSkillMatrixDirty) {
        e.preventDefault();
        const href = $(this).attr('href'); // store target link
        $('#unsavedChangesModal').data('target', href).modal('show');
    }
});

//Modal Button Actions
$('#leavePageBtn').on('click', function () {
    const target = $('#unsavedChangesModal').data('target');
    window.isSkillMatrixDirty = false; // reset flag
    $('#unsavedChangesModal').modal('hide');

    if (target) {
        window.location.href = target; // go to clicked page
    } else {
        window.close(); // fallback for tab close
    }
});

//Optional: Reset dirty flag on submit
$('#btnSubmitSkillMatrix').on('click', function () {
    // after successful submit
    window.isSkillMatrixDirty = false;
});

$("#btskillmatrix").click(function () {
    window.location.href = "/SkillMatrix/createSkill";
});

// empCode Search
$("#empCodeSearch").on("keyup", function () {
    let value = $(this).val().toLowerCase();
    $("#skillBody tr").filter(function () {
        let empCode = $(this).find("td:first").text().toLowerCase();
        $(this).toggle(empCode.indexOf(value) > -1);
    });
});


// Extract section number from Doc No
function extractSectionNumber(docNo) {
    if (!docNo || typeof docNo !== 'string') return '';

    // Pattern 1: MM-PS-OP-XXXXX-PXX-XX format
    const regex1 = /MM-PS-OP-([A-Z0-9]+?)(?:-P\d+|$)/i;
    const match1 = docNo.match(regex1);

    if (match1 && match1[1]) {
        return match1[1];
    }

    // Pattern 2: Alternative format
    const regex2 = /OP-([A-Z0-9]+)-/i;
    const match2 = docNo.match(regex2);

    if (match2 && match2[1]) {
        return match2[1];
    }

    // Pattern 3: Extract numbers after last hyphen or pattern
    const regex3 = /-(\d+[A-Z]?)(?:-|$)/;
    const match3 = docNo.match(regex3);

    if (match3 && match3[1]) {
        return match3[1];
    }
    return '';
}

// Featch Report Data
async function loadSkillMatrixReportByDocNo(DocNo, DeptCode, secCode) {
    try {
        let url = `/SkillMatrix/GetSkillMatrixReportByDocNo?`;
        if (DocNo) url += `DocNo=${encodeURIComponent(DocNo)}&`;
        if (DeptCode) url += `Dept=${encodeURIComponent(DeptCode)}&`;
        if (secCode) url += `secCode=${encodeURIComponent(secCode)}&`;
        const response = await fetch(url);
        const text = await response.text(); // 👈 read as text first

        if (!text) {
            throw new Error("Empty response from server");
        }
        const result = JSON.parse(text); // 👈 safely parse
        if (response.ok) {
            if (result.success === true && Array.isArray(result.data)) {
                DisplaySkillMatrixReportByDocNo(result.data);
            } else {
                showErrorModal(result.message || "Invalid response format");
            }
        } else {
            throw new Error(result.message || `HTTP Error: ${response.status}`);
        }

    } catch (error) {
        console.error("Error loading skill matrix:", error);
        showErrorModal(error.message);

        $("#tblSkillMatrixReport tbody").html(`
            <tr>
                <td colspan="16" class="text-center text-danger">
                    ${error.message}
                </td>
            </tr>
        `);
    }
}

function DisplaySkillMatrixReportByDocNo(data) {
    if (!Array.isArray(data) || data.length === 0) {
        $("#skillHead").html("");
        $("#skillBody").html(`
            <tr>
                <td colspan="5" class="text-center" style="color:red;">
                    No Data Found
                </td>
            </tr>
        `);

        $("#span").text("");
        $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable
        return;
    }
    let logDate = $('#logyear').val() || null;
    // 🔹 STEP 1: Get distinct dynamic columns
    const columns = [...new Set(
        data.map(x => x.columnNames).filter(x => x)
    )];


    if (Array.isArray(data) && data.length > 0) {
        const firstRow = data[0];
        let combinedText = `${$('#department').val()} - ${$('#subDepartment').val()} - ${$('#section').val()}`;
        if (logDate) {
            combinedText += " <span class='badge bg-warning text-dark'>Log Table Data</span>";
        }
    }

    // Check if any empCode is null, undefined, or empty
    const hasInvalidEmpCode = data.some(item =>
        !item.empCode || item.empCode === "undefined"
    );

    if (hasInvalidEmpCode) {
        $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable button
        if (logDate) {
            showModal("Error", "Multiple skill matrix records are available for Section Code (" + $('#section').val() + "), but fewer employees exist in empMaster, causing the transaction to fail.", false);
        }
        else {
            //showModal("Error", "Multiple skill matrix records exist for Section Code (" + $('#section').val() + "), but fewer employees are available in the empMaster table. Due to this mismatch, the transaction cannot be performed.", false);
        }
    } else {
        //$('#btnSubmitSkillMatrix').prop('disabled', false); // Enable button
    }


    $('#empCodeSearch').prop('disabled', false); // Disable

    // 🔹 STEP 2: Group Employees
    const employees = {};
    data.forEach(item => {
        if (!employees[item.empCode]) {
            employees[item.empCode] = {
                empName: item.empName,
                position: item.position,
                skills: {}
            };
        }
        employees[item.empCode].skills[item.columnNames] = item;
    });

    /* ===== HEADER ===== */
    let headHtml = "<tr>";
    headHtml += `<th class="fixed-col small font-weight-bold" style="font-size:0.9rem">Emp Code</th>`;
    headHtml += `<th class="fixed-col small font-weight-bold" style="font-size:0.9rem">Emp Name</th>`;
    headHtml += `<th class="fixed-col small font-weight-bold" style="font-size:0.9rem">Position</th>`;
    columns.forEach(col => {
        headHtml += `<th class="text-center small font-weight-bold" style="min-width:90px;font-size:0.7rem">${col}</th>`;
    });

    headHtml += "</tr>";
    $("#skillHead").html(headHtml);

    /* ===== BODY ===== */
    const today = new Date().toISOString().split('T')[0];
    let bodyHtml = "";
    Object.keys(employees).forEach(empCode => {
        const emp = employees[empCode];
        bodyHtml += "<tr style='font-size:0.6rem'>";

        // Fixed columns
        bodyHtml += `<td class="fixed-col small-font font-weight-bold" style="font-size:0.6rem">${empCode ?? ''}</td>`;
        bodyHtml += `<td class="fixed-col small-font font-weight-bold" style="font-size:0.6rem">${emp.empName ?? ''}</td>`;
        bodyHtml += `<td class="fixed-col small-font font-weight-bold" style="font-size:0.6rem">${emp.position ?? ''}</td>`;

        // Dynamic skill columns
        columns.forEach(col => {
            const skill = emp.skills[col] || {};

            const score = skill.score ?? '';
            const old_score = skill.old_score ?? '';
            const skillDate = skill.skillDate ? skill.skillDate.split('T')[0] : '';

            // Restrict to current year only, max today
            const currentYear = new Date().getFullYear();
            const minDate = `${currentYear}-01-01`; // Jan 1 of current year
            const maxDate = today; // today

            let bgColor = "white";
            let old_bgColor = "white";

            if (score === "0") bgColor = "#FFFF00";
            else if (score === "1") bgColor = "#41fbfee3";
            else if (score === "2") bgColor = "#fea521";
            else if (score === "3") bgColor = "#a1e964";

            if (old_score === "0") old_bgColor = "#FFFF00";
            else if (old_score === "1") old_bgColor = "#41fbfee3";
            else if (old_score === "2") old_bgColor = "#fea521";
            else if (old_score === "3") old_bgColor = "#a1e964";

            bodyHtml += `
                <td style="min-width:60px">
                     <div class="mb-1">
                    <b>Current Score: </b>
                        <input type="number" class="form-control form-control-sm mb-1 text-center score-input"
                               value="${score}" style="background-color: ${bgColor} !important;" readonly/>
                    </div>

                     <div class="mb-1">
                    <b>Old Score: </b>
                     <input type="number" class="form-control form-control-sm mb-1 text-center score-input" 
                     value="${old_score}" style="background-color: ${old_bgColor} !important;" readonly/>
                  </div>
                </td>
            `;
        });
        bodyHtml += "</tr>";
    });
    $("#skillBody").html(bodyHtml);

    if ($('#skillBody').find('tr').length > 0) {
        let logDate = $('#logyear').val() || null;
        if (!logDate) {
            $('#btnSubmitSkillMatrix').prop('disabled', false); // Enable button
        }
    } else {
        $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable button
    }
}

function getGroupValues(selectedText) {
    const normalizedSelected = selectedText.trim().toLowerCase();
    for (const groupName in processGroups) {
        const group = processGroups[groupName];
        const matched = group.items.some(item =>
            item.trim().toLowerCase() === normalizedSelected
        );
        if (matched) {
            console.log("Matched Group:", groupName);
            return group.values;
        }
    }

    return "";
}

const processGroups = {
    // TPH
    Press: {
        //items: ["TPH 1 - Pressing", "TPH 3 PPRESS", "Fax Pressing", "Zirconia Pressing"],
        items: ["7120", "7420", "7520", "7620"],
        values: "7120,7620,7520,7420"
    },
    Casting: {
        //items: ["TPH 1 - Casting", "TPH Casting", "Fax Casting", "FAX - Casting", "Zirconia Casting", "ZIRCONIA-Casting"],
        items: ["7510B", "7510-20", "7110B", "7410B", "7410-20", "7110-20"],
        values: "7110B,7110-20,7510B,7710-20,7410B,7410-20"
    },
    Honing: {
        //items: ["TPH 1 - Honing", "TPH 3 - Honing", "Fax Honing", "Zirconia Honing"],
        items: ["7140", "7440", "7540"],
        values: "7140,7640,7540,7440"//
    },
    Laser: {
        //items: ["TPH 1 - Laser", "Fax Laser", "Zirconia Laser"],
        items: ["T7150", "7450", "7550"],
        values: "7150,7550,7450"
    },
    Packing: {
        //items: ["TPH 1 - Packing", "Fax Packing", "Zirconia Packing"],
        items: ["7190", "7590", "7490"],
        values: "7190,7590,7490"//
    },
    MainFiring: {
        //items: ["Fax Mainfiring", "Zirconia Mainfiring","TPH 1 - Main Firing"],
        items: ["7130", "7430","7530"],
        values: "7130,7530,7430"//
    },
    SecondFiring: {
        //items: ["Fax Secondfiring", "Zirconia Secondfiring", "TPH 1 - 2nd Firing"],
        items: ["7160", "7460", "7560"],
        values: "7160,7560,740"
    },
    Inspection: {
        //items: ["TPH 1 - Inspection", "TPH 3 -Inspection", "Zirconia Inspection"],
        items: ["7170", "7470", "7670"],
        values: "7170,7670,7570,7470"
    },
    Engineering: {
        //items: ["TPH Engineering", "TPH 1 - Engineering", "TPH 3 -ENGINEERING", "FAX-Engineering", "ZIRCONIA-Engineering", "TPH Zirconia Engineering"],
        items: ["7601-12A", "7500-20", "7100-12A", "7400-20", "7401-12A", "7100-20"],
        values: "7100-12A,7100-20,7601-12A,7500-20,7400-20,7401-12A"
    },
    BallMill: {
        items: ["7510A", "7510-10", "7110A", "7410A", "7410-10","7110-10"],
        values: "7110A,7110-10,7510-10,7410A,7410-10"
    },
    Arraying: {
        items: ["2130A", "7130A","7730A"],
        values: "7120"
    },
    General: {
        //items: ["General", "TPH 1 - General", "TPH 3 General", "Fax General", "Zirconia General"],
        items: ["7601", "7101", "7601", "7501", "7401"],
        values: "7100,7101,7601,7501,7401"
    },
    QA: {
        //items: ["TPH 1 - QAID", "TPH RAW MATERIAL QA", "Fax QAID", "Zirconia QA", "", ""],
        items: ["7100-10", "7500-10", "7400-10", "7110B-10"],
        values: "9010,7100-10,7110B-10,7500-10,7400-10"
    },
    Catron: {
        //items: ["TPH Master Carton", "MASTER CARTON ZIRCONIA"],
        items: ["7170A", "7470A"],
        values: "7120,7170A,7470A"
    },
    Maintenance: {
        items: ["7102", "7103", "7403"],
        values: "7102,7103,7403"
    },
    Safety: {
        //items: ["TPH Safety", "TPH Zirconia Safety"],
        items: ["7105", "7405"],
        values: "9005,7105,7405"//
    },

    //CV
    cvFiring: {
        items: ["3030", "3130","3140", "3230", "3430"],
        values: "3030,3130,3140,3230,3430"
    },
    cvEngineering: {
        items: ["3100-12A", "3100-20", "3200-20", "3400-20"],
        values: "3100-12A,3100-20,3200-20,3400-20"
    },
    cvGrinding: {
        items: ["3140", "3151", "3240", "3440"],
        values: "3140,3151,3240,3440"
    },
    cvWashing: {
        items: ["Washing", "CV-Washing", "CV-WASHING", "MS-Washing"],
        values: "3153,3160,3260,3460"
    },
    cvGeneral: {
        items: ["3100", "3101", "3201", "3401"],
        values: "3100,3101,3201,3401"
    },
    cvMaintenance: {
        items: ["3102", "3103"],
        values: "3102,3103,3120,3420"
    },
    cvLapping: {
        items: ["3150", "3152", "3250", "3450"],
        values: "3150,3152,3250,3450"
    },
    cvInspection: {
        items: ["3170", "3170A", "3270", "3470"],
        values: "3120,3170,3170A,3270,3420,3470"
    },
    cvPacking: {
        items: ["3190", "3290", "3490"],
        values: "3190,3290,3490"
    },
    cvPressing: {
        items: ["3120", "3220", "3420",],
        values: "3120,3220,3420"
    },
    cvCV: {
        items: ["3100-5S", "3200-30", "3105", "3400-30"],
        values: "3100,3100-5S,3200-30,3105,3400-30"
    },
    cvMaterial: {
        items: ["3110", "3210", "3410"],
        values: "3110,3210,3410"
    },
    cvQAID: {
        items: ["3100-10", "3200-10", "3400-10"],
        values: "3100-10,3200-10,3400-10"
    },

    // Magnet
    cvArrange: {
        items: ["4270C", "4270D", "4270E"],
        values: "4270,4270C,4270D,4270E"
    },
    mgPressing: {
        items: ["4220", "4420",],
        values: "4220,4420"
    },
    mgFiring: {
        items: ["4230"],
        values: "4230"
    },
    mgPacking: {
        items: ["4290"],
        values: "4290"
    },
    mgGrinding: {
        items: ["4240"],
        values: "4240"
    },
    mgSlicing: {
        items: ["4250"],
        values: "4250"
    },
    mgInspection: {
        items: ["4270A"],
        values: "4270"
    },
    mgMaintenance: {
        items: ["4202", "4203"],
        values: "4202,4203,"
    },
    mgFerrite: {
        items: ["4205"],
        values: "4205"
    },
    mgRawmaterial: {
        items: ["4210"],
        values: "4210"
    },
    mgEngineering: {
        items: ["4200-20", "4201-12A"],
        values: "4200-20,4201-12A"
    },
    mgQAUD: {
        items: ["4200","4200-10", "4201-10"],
        values: "4200,4200-10,4201-10"
    },

    fmdMaintenance: {
        items: ["9013", "1234"],
        values: "9013,1234"
    },
    oshid: {
        items: ["9005"],
        values: "9005"
    },
    vcid: {
        items: ["9004"],
        values: "9004"
    },
    qaid: {
        items: ["9010", "9011","9100D"],
        values: "9010,9011,9100D"
    },
    rd: {
        items: ["9001"],
        values: "9001"
    },
    tfc: {
        items: ["7700"],
        values: "7700,7790"
    }
};


