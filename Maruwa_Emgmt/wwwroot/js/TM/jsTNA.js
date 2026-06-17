$(document).ready(function () {
    BindDepartmentcodeDropdown();

    $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable

    // Set max attribute for start date to prevent future dates
    const today = new Date().toISOString().split('T')[0];
    $('#startDate').attr('max', today);

    // Optional: prevent manual input of future date
    $('#startDate').on('change', function () {
        if (this.value > today) {
            this.value = today;
        }
    });

    // 🔹 Keyboard navigation for skill table inputs
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
            e.preventDefault(); // Prevent form submission

            if (currentIndex < $inputsInCell.length - 1) {
                // Move to next input in same cell
                $inputsInCell.eq(currentIndex + 1).focus();
            } else {
                // Move to first input of same column in the next row
                const nextRow = $table.rows[rowIndex + 1];
                if (nextRow) {
                    const nextCell = nextRow.cells[colIndex];
                    if (nextCell) {
                        const $nextInputs = $(nextCell).find('input');
                        if ($nextInputs.length) $nextInputs.eq(0).focus();
                    }
                }
            }
        }
        else if (e.key === "Tab") {
            e.preventDefault(); // override default Tab
            // Move to first input in the next cell
            const $nextCell = $td.next('td'); // jQuery next td
            if ($nextCell.length) {
                const $nextInputs = $nextCell.find('input');
                if ($nextInputs.length) {
                    $nextInputs.eq(0).focus();
                }
            }
        }
    });



    // 🔹 NEW: Update background color dynamically on score input
    window.isSkillMatrixDirty = false; // Initialize dirty flag
    $('#skillTable').on('keyup change', '.score-input', function () {
        const value = $(this).val();
        let bgColor = "white"; // default

        if (value === "0") bgColor = "#FFFF00";      // yellow
        else if (value === "1") bgColor = "#41fbfee3"; // light blue
        else if (value === "2") bgColor = "#fea521"; // orange
        else if (value === "3") bgColor = "#a1e964"; // green

        $(this).css('background-color', bgColor);

        // mark the table as dirty
        window.isSkillMatrixDirty = true;
    });



});

$(document).on('click', '#btnLoadSkillMatrix', function (e) {
    e.preventDefault();
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
    const departmentCode = $('#department').val();
    const subDepartmentCode = $('#subDepartment').val();
    const sectionCode = $('#section').val();
    // Optional date
    let logDate = null;
    const dateVal = $('#startDate').val();
    //LogDateControl();
    if (dateVal) {
        logDate = new Date(dateVal).toISOString();
        LogDateControl();
    }
    else {
        LogDateControl();
    }
    loadSkillMatrix(departmentCode, subDepartmentCode, sectionCode, logDate);
});

$(document).on('change', '#department, #subDepartment, #section', function () {
    if ($(this).val()) {
        $(this).removeClass('is-invalid');
    }
});

/* ================================ LOAD DATA ================================*/
async function loadSkillMatrix(departmentCode, subDepartmentCode, sectionCode, logDate) {
    try {
        let url = `/TNA/GetSkillMatrix?`;

        if (departmentCode) url += `departmentCode=${encodeURIComponent(departmentCode)}&`;
        if (subDepartmentCode) url += `subDepartmentCode=${encodeURIComponent(subDepartmentCode)}&`;
        if (sectionCode) url += `sectionCode=${encodeURIComponent(sectionCode)}&`;
        if (logDate) url += `logDate=${encodeURIComponent(logDate)}&`;

        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP Error: ${response.status}`);
        }
        const result = await response.json();
        let data = [];
        if (result.success === true && Array.isArray(result.data)) {
            if (result.data.length > 0) {
                $('#btnSubmitSkillMatrix').prop('disabled', false);  // Enable
            } else {
                $('#btnSubmitSkillMatrix').prop('disabled', true);   // Disable
            }

            data = result.data;
        } else if (Array.isArray(result)) {
            data = result;
        } else {
            throw new Error(result.message || "Invalid data format");
        }
        generateSkillMatrix(data);
        //LogDateControl();
    }
    catch (error) {
        console.error("Error loading skill matrix:", error);
        $("#skillHead").html("");
        $("#skillBody").html(`
            <tr>
                <td colspan="10" class="text-center text-danger">
                    ${error.message}
                </td>
            </tr>
        `);
    }
}

function LogDateControl() {

    let logDate = null;
    const dateVal = $('#startDate').val();

    if (dateVal) {
        logDate = new Date(dateVal).toISOString();
        $("#spndataType").text("(Log Table Info)");
        $('#btnSubmitSkillMatrix').prop('disabled', true); // Disable
    }
    else {
        $("#spndataType").text("");
        $('#btnSubmitSkillMatrix').prop('disabled', false); // Disable
    }
}

/* ================================ GENERATE TABLE ================================ */
function generateSkillMatrix(data) {
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
    // 🔹 STEP 1: Get distinct dynamic columns
    const columns = [...new Set(
        data.map(x => x.columnNames).filter(x => x)
    )];

    if (Array.isArray(data) && data.length > 0) {
        const firstRow = data[0];
        const combinedText = `
        ${firstRow.departmentCode ?? ''} - 
        ${firstRow.subDepartmentCode ?? ''} - 
        ${firstRow.sectionCode ?? ''}
    `;
    $("#span").text("DEPARTMENT: "+combinedText.trim());
    }

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
            const skill = emp.skills[col];
            if (skill) {
                // 🔹 Determine background color based on score
                const score = skill.score;
                let bgColor = "white"; // default for empty or other values
                if (score === "0") bgColor = "#FFFF00";      // light yellow
                else if (score === "1") bgColor = "#41fbfee3"; // light orange
                else if (score === "2") bgColor = "#fea521"; // light orange
                else if (score === "3") bgColor = "#a1e964"; // light green

                bodyHtml += `
                <td style="min-width:150px">
                   <input type="number" class="form-control form-control-sm mb-1 text-center score-input" min="0" max="3" step="1" value="${score ?? ''}" style="background-color: ${bgColor} !important;" />
                  <input type="date" class="form-control form-control-sm mb-1"  max="${today}" value="${skill.skillDate ? skill.skillDate.split('T')[0] : ''}" />
                </td>
            `;
            } else {
                bodyHtml += `<td></td>`;
            }
        });
        bodyHtml += "</tr>";
    });
    $("#skillBody").html(bodyHtml);
}

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

    $("#skillBody tr").each(function () {
        const $tr = $(this);

        const empCode = $tr.find("td:eq(0)").text().trim();
        const empName = $tr.find("td:eq(1)").text().trim();
        const position = $tr.find("td:eq(2)").text().trim();

        $tr.find("td").slice(3).each(function () {
            const $td = $(this);

            const score = $td.find('input[type="number"]').val();
            const skillDate = $td.find('input[type="date"]').val();
            const columnName = $("#skillHead th").eq($td.index()).text().trim();

            if (columnName) {
                skillData.push({
                    empCode: empCode,
                    empName: empName,
                    position: position,
                    columnNames: columnName,
                    score: score || null,
                    skillDate: skillDate || null
                });
            }
        });
    });

    try {
        const response = await fetch('/TNA/SaveSkillMatrix', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(skillData)
        });

        const result = await response.json();

        if (result.success) {
            loadSkillMatrix();
            showModal("Success", result.message, true);
        } else {
            showModal("Error", result.message, false);
        }

    } catch (err) {
        showModal("Error", "Something went wrong while saving.", false);
    }
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
function BindDepartmentcodeDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#department');
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
    sectionDropdown.append('<option value="">--Select Section--</option>');  // Default option

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