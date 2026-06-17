// jscreateskill.js

// Global variables
let excelData = [];

// Initialize everything when document is ready
$(document).ready(function () {
    $("#btnSubmit").prop("disabled", true);
    initializeEventListeners();
});

// Function to initialize all event listeners
function initializeEventListeners() {
    // Clear button click event
    $('#btnClear').on('click', handleClearButtonClick);

    // Show Data button click event
    $('#btnShowData').on('click', handleShowDataButtonClick);

    // Submit button click event
    $('#btnSubmit').on('click', handleSubmitButtonClick);
}

// ==================== FILE HANDLERS FUNCTIONS (BUTTON CLICK EVENTS)====================
// Handler for Clear button click
function handleClearButtonClick() {
    clearTable();
    resetFileInput();
    clearExcelData();
    showSuccessMessage('Data cleared successfully!');
}

// Handler for Show Data button click
async function handleShowDataButtonClick() {
    const file = getSelectedFile();
    // Step 1: Validate if file is selected
    if (!file) {
        showModal("Error", "Please select a file to upload.");
        return;
    }

    // Validate if file is Excel
    if (!isExcelFile(file)) {
        showModal("Error", "Please upload a valid Excel file (.xlsx or .xls).");
        return;
    }

    try {
        // Show loading state
        setButtonLoadingState('#btnShowData', true, 'Loading...');

        // Read and process the Excel file
        await readAndProcessExcelFile(file);

    } catch (error) {
        console.error('Error reading Excel file:', error);
        showModal("Error",`Error reading Excel file. ${error.message || error}`);
    } finally {
        // Reset button state
        setButtonLoadingState('#btnShowData', false, 'Show Data');
    }
}

// Handler for Submit button click
function handleSubmitButtonClick() {
    if (!hasData()) {
        showModal("Error","No data to submit. Please load data first.");
        return;
    }
}

// Get selected file from input
function getSelectedFile() {
    const fileInput = $('#fleuploadskillmatrixfile')[0];
    return (fileInput.files && fileInput.files.length > 0) ? fileInput.files[0] : null;
}

// Reset file input
function resetFileInput() {
    $('#fleuploadskillmatrixfile').val('');
}

// Check if file is Excel
function isExcelFile(file) {
    const allowedExtensions = ['xlsx', 'xls'];
    const fileName = file.name;
    const fileExtension = fileName.split('.').pop().toLowerCase();
    return allowedExtensions.includes(fileExtension);
}

// Read and process Excel file
async function readAndProcessExcelFile(file) {
    const data = await readExcelFile(file);
    processExcelData(data);
}

// Read Excel file using Promise
function readExcelFile(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();

        reader.onload = function (e) {
            try {
                const data = new Uint8Array(e.target.result);
                const workbook = XLSX.read(data, { type: 'array' });

                // Get first sheet
                const firstSheetName = workbook.SheetNames[0];
                const worksheet = workbook.Sheets[firstSheetName];

                // Convert to JSON (array of arrays)
                const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });
                console.log("readExcelFile: ",jsonData);
                resolve(jsonData);
            } catch (error) {
                reject(error);
            }
        };

        reader.onerror = function (error) {
            reject(error);
        };

        reader.readAsArrayBuffer(file);
    });
}

// ==================== DATA PROCESSING FUNCTIONS ====================
// Process Excel data
function processExcelData_Old(data) {
    if (!data || data.length === 0) {
        showModal("Error", "No data found in the Excel file.");
        return;
    }

    console.log("processExcelData: ", data);
    // Find column indices
    const headers = data[0];
    const columnIndices = findColumnIndices(headers);

    if (!areRequiredColumnsFound(columnIndices)) {
        showModal("Error", "Some required columns (Doc No, Document Name, Rev, Standards, Department, Category, Prepared By, Reviewed By, Approved By, Approved Date) are missing or incorrectly named in your uploaded Excel file. Please check and upload again.");
        return;
    }

    // Clear previous data
    clearExcelData();
    //
    document.getElementById("spntotalrecordscount").textContent ="Total Records: "+ data.length;
    // Process each row (skip header row)
    for (let i = 1; i < data.length; i++) {
        const row = data[i];
        const processedRow = processRow(row, columnIndices);
        if (processedRow) {
            excelData.push(processedRow);
        }
    }

    if (excelData.length === 0) {
        showModal("Error", "No valid data rows found in the Excel file.");
        return;
    }

    // Display data in table
    displayTableData(excelData);
    showSuccessMessage(`Successfully loaded ${excelData.length} records!`);
}

function processExcelData(data) {

    // STEP 1: Check if Excel data exists
    if (!data || data.length === 0) {
        showModal("Error", "No data found in the Excel file.");
        return;
    }

    console.log("processExcelData: ", data);
    // STEP 2: Read header row
    const headers = data[0];
    // STEP 3: Find required column indexes from header
    const columnIndices = findColumnIndices(headers);
    // STEP 4: Validate required columns exist
    if (!areRequiredColumnsFound(columnIndices)) {
        showModal("Error", "Some required columns (Doc No, Document Name, Rev, Standards, Department, Category, Prepared By, Reviewed By, Approved By, Approved Date) are missing or incorrectly named in your uploaded Excel file. Please check and upload again.");
        return;
    }
    // STEP 5: Clear previously loaded Excel data
    clearExcelData();
    // STEP 6: Show total rows count from Excel
    document.getElementById("spntotalrecordscount").textContent ="Total Records: " + (data.length - 1);

    // STEP 7: Create object to store latest revision of each docNo
    const latestDocs = {};
    // STEP 8: Loop through Excel rows (skip header row)
    for (let i = 1; i < data.length; i++) {
        const row = data[i];
        const processedRow = processRow(row, columnIndices);// STEP 9: Process row into structured object
        if (processedRow) {
            const docNo = processedRow.docNo;
            const currentRev = parseInt(processedRow.Rev) || 0; // Convert revision to number
            if (!latestDocs[docNo]) { // STEP 10: If docNo not already stored → store it
                latestDocs[docNo] = processedRow;
            }
            else {
                const existingRev = parseInt(latestDocs[docNo].Rev) || 0; // STEP 11: Compare revisions if docNo already exists
                if (currentRev > existingRev) {// STEP 12: Replace only if current revision is higher
                    latestDocs[docNo] = processedRow;
                }
            }
        }
    }
    excelData = Object.values(latestDocs);// STEP 13: Convert object values to array
    
    if (excelData.length === 0) {// STEP 14: Check if valid rows exist
        showModal("Error", "No valid data rows found in the Excel file.");
        return;
    }
    displayTableData(excelData);// STEP 15: Display filtered data in table
    showSuccessMessage(`Successfully loaded ${excelData.length} records!`);// STEP 16: Show success message
}

// Function to process a single Excel row
function processRow(row, indices) {
    // STEP 17: Skip empty rows
    if (!row || row.length === 0 || !row[indices.docNo]) return null;
    // STEP 18: Read and sanitize Excel values

    const docNo = sanitizeInput(row[indices.docNo]);
    const docName = sanitizeInput(row[indices.docName]);
    const Rev = sanitizeInput(row[indices.Rev]);
    const standards = sanitizeInput(row[indices.standards]);
    const department = sanitizeInput(row[indices.department]);

    const category = sanitizeInput(row[indices.category]);
    const preparedby = sanitizeInput(row[indices.preparedby]);
    const reviewedby = sanitizeInput(row[indices.reviewedby]);
    const approvedby = sanitizeInput(row[indices.approvedby]);
    const approveddate = sanitizeInput(row[indices.approveddate]);

    // STEP 19: Skip row if docNo is empty
    if (!docNo) return null;
    // STEP 20: Extract section number from docNo
    const sectionNo = extractSectionNumber(docNo);
    // STEP 21: Return structured row object
    return {
        docNo: docNo,
        docName: docName,
        Rev: Rev,
        standards: standards,
        department: department,
        category: category,
        preparedby: preparedby,
        reviewedby: reviewedby,
        approvedby: approvedby,
        approveddate: approveddate,
        sectionNo: sectionNo
    };
}

// Find column indices for required fields
function findColumnIndices(headers) {
    return {
        docNo: findColumnIndex(headers, 'Doc No'),
        docName: findColumnIndex(headers, 'Document Name'),
        Rev: findColumnIndex(headers, 'Rev'),
        standards: findColumnIndex(headers, 'Standards'),
        department: findColumnIndex(headers, 'Department'),

        category: findColumnIndex(headers, 'Category'),
        preparedby: findColumnIndex(headers, 'Prepared By'),
        reviewedby: findColumnIndex(headers, 'Reviewed By'),
        approvedby: findColumnIndex(headers, 'Approved By'),
        approveddate: findColumnIndex(headers, 'Approved Date')
    };
}

// Find column index by name (case insensitive)
function findColumnIndex(headers, columnName) {
    if (!headers) return -1;

    for (let i = 0; i < headers.length; i++) {
        if (headers[i] && headers[i].toString().toLowerCase().trim() === columnName.toLowerCase()) {
            return i;
        }
    }
    return -1;
}

// Check if all required columns are found
function areRequiredColumnsFound(indices) {
    return indices.docNo !== -1 && indices.docName !== -1 && indices.department !== -1 && indices.Rev !== -1;
}

// Process a single row
function processRow_Old(row, indices) {
    // Skip empty rows
    if (!row || row.length === 0 || !row[indices.docNo]) return null;

    const docNo = sanitizeInput(row[indices.docNo]);
    const docName = sanitizeInput(row[indices.docName]);
    const department = sanitizeInput(row[indices.department]);
    const Rev = sanitizeInput(row[indices.Rev]);

    // Skip if docNo is empty
    if (!docNo) return null;

    // Extract section number
    const sectionNo = extractSectionNumber(docNo);

    return {
        docNo: docNo,
        docName: docName,
        department: department,
        sectionNo: sectionNo,
        Rev: Rev
    };
}

// Sanitize input
function sanitizeInput(value) {
    return value?.toString().trim() || '';
}

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

// ==================== UI DISPLAY FUNCTIONS ====================

// Display data in table

function displayTableData(data) {
    const tbody = $('#tblSkillMatrix tbody');
    tbody.empty();

    if (data.length === 0) {
        tbody.append('<tr><td colspan="4" class="text-center text-muted text-danger">No data to display</td></tr>');
        return;
    }
    else {
        document.getElementById("spntotalrecordscount").textContent = "Total Records: " + data.length;
    }

    $("#btnSubmit").prop("disabled", false);
    data.forEach((row, index) => {
        const sno = index + 1;
        const tr = $('<tr>');
        tr.append(`<td>${sno}</td>`);
        tr.append(`
        <td>
            ${escapeHtml(row.docNo)}
            <input type="hidden" name="docNo[]" value="${row.docNo}">
            <input type="hidden" name="standards[]" value="${row.standards}">
            <input type="hidden" name="category[]" value="${row.category}">
            <input type="hidden" name="preparedby[]" value="${row.preparedby}">
            <input type="hidden" name="reviewedby[]" value="${row.reviewedby}">
            <input type="hidden" name="approvedby[]" value="${row.approvedby}">
            <input type="hidden" name="approveddate[]" value="${row.approveddate}">
        </td>
    `);

        tr.append(`<td>${escapeHtml(row.docName)}</td>`);
        tr.append(`<td>${escapeHtml(row.department)}</td>`);
        tr.append(`<td><span class="badge badge-info">${escapeHtml(row.sectionNo)}</span></td>`);
        tr.append(`<td>${escapeHtml(row.Rev)}</td>`);

        tbody.append(tr);
    });
}


function displayTableData_Old(data) {
    const tbody = $('#tblSkillMatrix tbody');
    tbody.empty();

    if (data.length === 0) {
        tbody.append('<tr><td colspan="4" class="text-center text-muted">No data to display</td></tr>');
        return;
    }

    $("#btnSubmit").prop("disabled", false);

    data.forEach((row, index) => {
        const sno = index + 1; // dynamic serial number

        const tr = $('<tr>');
        tr.append(`<td>${sno}</td>`);
        tr.append(`<td>${escapeHtml(row.docNo)}</td>`);
        tr.append(`<td>${escapeHtml(row.docName)}</td>`);
        tr.append(`<td>${escapeHtml(row.department)}</td>`);
        tr.append(`<td><span class="badge badge-info">${escapeHtml(row.sectionNo)}</span></td>`);
        tr.append(`<td>${escapeHtml(row.Rev)}</td>`);

        tbody.append(tr);
    });
}

// Clear table
function clearTable() {
    $('#tblSkillMatrix tbody').html('<tr><td colspan="4" class="text-center text-muted"></td></tr>');
    document.getElementById("spntotalrecordscount").innerHTML = "";
}

// Clear excel data
function clearExcelData() {
    excelData = [];
}

// Check if data exists
function hasData() {
    return excelData.length > 0;
}

// ==================== UI HELPER FUNCTIONS ====================

// Set button loading state
function setButtonLoadingState(buttonSelector, isLoading, loadingText = 'Loading...') {
    const $button = $(buttonSelector);

    if (isLoading) {
        $button.html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> ${loadingText}`)
            .prop('disabled', true);
    } else {
        $button.html(loadingText).prop('disabled', false);
    }
}

// Show success message
function showSuccessMessage(message) {
    console.log('Success:', message);
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// ==================== SUBMISSION FUNCTIONS====================
$("#btnSubmit").click(async function () {
    let $btn = $(this);

    // Disable button + loading UI
    $btn.prop("disabled", true);
    $btn.html('<span class="spinner-border spinner-border-sm"></span> Inserting...');

    const deleted = true;

    if (deleted) {

        let rows = [];

        $("#tblSkillMatrix tbody tr").each(function () {

            let $tr = $(this);
            let tds = $tr.find("td");

            let docNo = $(tds[1]).text().trim();
            let docName = $(tds[2]).text().trim();
            let dept = $(tds[3]).text().trim();
            let revisionVal = $(tds[5]).text().trim();

            let standards = $tr.find('input[name="standards[]"]').val() || "";
            let category = $tr.find('input[name="category[]"]').val() || "";
            let preparedby = $tr.find('input[name="preparedby[]"]').val() || "";
            let reviewedby = $tr.find('input[name="reviewedby[]"]').val() || "";
            let approvedby = $tr.find('input[name="approvedby[]"]').val() || "";
            let approveddateRaw = $tr.find('input[name="approveddate[]"]').val();
            console.log(approveddateRaw);

            //let approvedDate = null;
            //if (approveddateRaw) {
            //    // expected format: dd/MM/yyyy
            //    let parts = approveddateRaw.split('/');
            //    if (parts.length === 3) {
            //        let day = parts[0];
            //        let month = parts[1];
            //        let year = parts[2];

            //        let parsed = new Date(`${year}-${month}-${day}`);

            //        if (!isNaN(parsed.getTime())) {
            //            approvedDate = parsed.toISOString();
            //        }
            //    }
            //}

            let approvedDate = null;
            if (approveddateRaw) {
                let parts = approveddateRaw.split('/');
                if (parts.length === 3) {
                    let parsed = new Date(parts[2], parts[1] - 1, parts[0]);

                    if (!isNaN(parsed)) {
                        approvedDate = parsed.toISOString();
                    }
                }
            }
            console.log(approvedDate);

            // PUSH ROW
            rows.push({
                columnNames: docNo,
                DocumentName: docName,
                Rev: revisionVal,
                standards: standards,
                Department: dept,

                category: category,
                preparedBy: preparedby,
                reviewedBy: reviewedby,
                approvedBy: approvedby,
                approvedDate: approvedDate,
                sectionCode: extractSectionNumber(docNo),
                CreatedBy: ""
            });
        });
        console.log("Payload Sent:", rows);
        try {
            let response = await fetch('/SkillMatrix/InsertSkillMatrixList', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(rows)
            });
            let result = await response.json();
            if (!result.success) {
                throw new Error(result.message);
            }
            showModal("Success", "All records inserted successfully.");
        } catch (error) {
            showModal("Error", error.message);

        } finally {
            $btn.prop("disabled", false);
            $btn.html("Submit");
        }

    } else {
        $btn.prop("disabled", false);
        $btn.html("Submit");
    }
});


// Clear button click event
$("#btnClear").click(function () {
    initializeEventListeners();
});

$("#btngoSKM").click(function () {
    //window.location.href = "/TNA/tnaList";
    window.location.href = "/SkillMatrix/skmList";
});

// ✅ Function to delete all records
async function deleteAllSkillMatrixRecords() {
    try {
        let response = await fetch('/SkillMatrix/DeleteAllSkillMatrix', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        let result = await response.json();
        if (!result.success) {
            {
                showModal("Error", "While Deleteting Old Data throwing Error: " + result.message);
                return false;
            }
        }
        console.log("All records deleted successfully.");
        return true;
    } catch (error) {
        showModal("Error", error.message);
        throw error; // Rethrow to handle in caller
    }
}

// Show modal with custom title and message
// Automatically changes header color based on title content
function showModal(title, message) {
    $("#modalTitle").text(title);
    $("#modalBody").text(message);

    // Get the modal header element
    const $modalHeader = $("#msgModal .modal-header");
    // Remove existing color classes
    $modalHeader.removeClass('bg-primary bg-success bg-danger bg-warning bg-info');

    // Add appropriate color class based on title
    const titleLower = title.toLowerCase();

    if (titleLower.includes('error') || titleLower.includes('failed') || titleLower.includes('fail')) {
        $modalHeader.addClass('bg-danger text-white'); // Red for errors
    }
    else if (titleLower.includes('success') || titleLower.includes('completed') || titleLower.includes('done')) {
        $modalHeader.addClass('bg-success text-white'); // Green for success
    }
    else if (titleLower.includes('warning') || titleLower.includes('caution')) {
        $modalHeader.addClass('bg-warning text-dark'); // Yellow for warnings
    }
    else {
        $modalHeader.addClass('bg-primary text-white'); // Default blue
    }

    // Show the modal
    $("#msgModal").modal("show");
}
