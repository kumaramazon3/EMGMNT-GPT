// DOM Ready: Bind Events
$(document).ready(function () {

    // Initialize dropdowns or other bindings
    bindTrainingDropdown(); // attach the onchange event
    // Remove red border on input/select/textarea change
    $('#content').on('input change', 'input, select, textarea', function () {
        $(this).removeClass('border-danger');
    });

    // Clear button: reset all fields
    $('#clear').on('click', function () {
        clearAllFields();
    });

    // Enter key navigation: move focus to next input/select/textarea/button
    $('#content').on('keydown', 'input, select, textarea', function (e) {
        if (e.key === "Enter") {
            e.preventDefault(); // Prevent default form submission

            const $this = $(this);

            // Optional: validate current field before moving
            if (!$this.val()) {
                $this.addClass('border-danger'); // Highlight empty field
            } else {
                $this.removeClass('border-danger'); // Remove red if valid
            }

            // Get all focusable elements inside #content
            const focusable = $('#content').find('input, select, textarea, button')
                .filter(':visible')
                .not(':disabled');

            const index = focusable.index(this);

            // Move focus to next element if exists
            if (index > -1 && index + 1 < focusable.length) {
                focusable.eq(index + 1).focus();
            }
        }
    });

});

function bindTrainingDropdown() {
    $.ajax({
        url: '/empTraining/GetTrainingList',
        type: 'GET',
        success: function (data) {
            var $dropdown = $('#trainingAttended');
            $dropdown.empty();
            $dropdown.append('<option value="">--Select Training--</option>');

            if (data && data.length > 0) {
                data.forEach(function (item) {
                    $dropdown.append('<option value="' + item.code + '">' + item.titleName + '</option>');
                });
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading training list:", error);
            alert("Unable to load training list.");
        }
    });
}

// When empcode changes, call the fetch function
$('#empcode').on('change', function () {
    var empcode = $(this).val().trim();

    // If empty, clear all fields
    if (empcode === "") {
        clearEmployeeFields();
        return;
    }

    // Call AJAX function
    fetchEmployeeData(empcode);
});

$('#trainingAttended').on('change', function () {
    var trainingText = $(this).find('option:selected').text().trim();
    if (trainingText === "--Select Training--") {
        clearTrainingFields();
        return;
    }
    fetchTrainingDetails(trainingText);
});

// Fetch training details when user selects a training
function fetchTrainingDetails(trainingText) {
    var titleName = trainingText;// $('#trainingAttended option:selected').text().trim();
    var type = $('#type').val();   // INTERNAL / EXTERNAL

    if (!titleName || titleName === '-- Select Training --') {
        clearTrainingFields();
        return;
    }

    $.ajax({
        url: '/empTraining/GetTrainingDetails',
        type: 'GET',
        data: {
            trainingCode: titleName,
            type: type
        },
        success: function (data) {

            if (!data) {
                clearTrainingFields();
                return;
            }

            $('#trainingProgramme').val(data.programme || '');
            $('#method').val(data.method || '');
            $('#type').val(data.type || '');

            // ===== Bind Trainer Dropdown =====
            var $trainer = $('#trainerName');
            $trainer.empty();
            $trainer.append('<option value="">--- Select Trainer ---</option>');

            if (data.trainers && data.trainers.length > 0) {
                $.each(data.trainers, function (i, t) {
                    $trainer.append(
                        `<option value="${t.empcode}">${t.empName}</option>`
                    );
                });
            }
        },
        error: function () {
            clearTrainingFields();
        }
    });
}

// Clear fields if no training selected
function clearTrainingFields() {
    $('#trainingProgramme').val('');
    $('#method').val('');
    $('#type').val('');
}

// Function: Fetch Employee Data
function fetchEmployeeData(empcode) {
    var empcodeBase64 = btoa(empcode);
    $.ajax({
        url: '/empTraining/EmpRegisteration/' + encodeURIComponent(empcodeBase64), // Correct route
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            populateEmployeeFields(response.employeeData);
        },
        error: function (xhr, status, error) {
            console.error("Error fetching employee data:", error, "Status:", xhr.status);
            alert("Invalid employee code or server error.");
            clearEmployeeFields();
        }
    });
}

// Populate Fields
function populateEmployeeFields(employeeJson) {
    if (!employeeJson || employeeJson === "{}") {
        clearEmployeeFields();
        return;
    }

    var empData = JSON.parse(employeeJson);

    $('#empname').val(empData.empName || '');
    $('#department').val(empData.department || '');
    $('#subDepartment').val(empData.subDepartment || '');
    $('#section').val(empData.section || '');
    $('#dateOfJoin').val(formatDateToDDMMMYYYY(empData.dateOfJoin));
}
function formatDateToDDMMMYYYY(dateString) {
    if (!dateString) return '';

    // Create a Date object from the string
    var date = new Date(dateString);

    // Check if date is valid
    if (isNaN(date)) return '';

    // Array of month abbreviations
    var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    var day = date.getDate();
    var month = monthNames[date.getMonth()];
    var year = date.getFullYear();

    return day + '-' + month + '-' + year;
}

// Clear Fields
function clearEmployeeFields() {
    $('#empname').val('');
    $('#department').val('');
    $('#subDepartment').val('');
    $('#section').val('');
    $('#dateOfJoin').val('');
}

$('#clear').on('click', function () {
    clearAllFields();
});

function clearAllFields() {
    // Clear all text/number/date/file inputs
    $('#content').find('input').each(function () {
        const type = $(this).attr('type');
        if (type === 'text' || type === 'number' || type === 'date' || type === 'file') {
            $(this).val('');
        } else if (type === 'checkbox' || type === 'radio') {
            $(this).prop('checked', false);
        }
    });

    // Reset all select dropdowns
    $('#content').find('select').each(function () {
        $(this).prop('selectedIndex', 0);
    });

    // Clear all textareas
    $('#content').find('textarea').val('');

    // Clear trainerName dropdown specifically
    $('#trainerName').empty().append('<option value="">--- Select Trainer ---</option>');
}

function showModal(title, message, type = 'success') {
    // Remove any previous classes
    $('#resultModal .modal-header').removeClass('bg-success bg-danger text-white');

    // Add color based on type
    if (type.toLowerCase() === 'success') {
        $('#resultModal .modal-header').addClass('bg-success text-white');
    } else if (type.toLowerCase() === 'error') {
        $('#resultModal .modal-header').addClass('bg-danger text-white');
    }

    // Set title and message
    $('#modalTitle').text(title);
    $('#modalBody').text(message);

    // Show the modal
    $('#resultModal').modal('show');
}

// Save button click
$('#save').on('click', function (e) {
    e.preventDefault(); // prevent default form submission
    saveTrainingForm(); // call the reusable function
});

// Prepare form data function
function getEmpTrainingModel() {
    return {
        empCode: $('#empcode').val(),
        empName: $('#empname').val(),
        department: $('#department').val(),
        subDepartment: $('#subDepartment').val(),
        sectionCode: $('#section').val(),
        trainingAttended: $('#trainingAttended option:selected').text(),
        programme: $('#trainingProgramme').val(),
        method: $('#method').val(),
        type: $('#type').val(),
        trainerCode: $('#trainerName').val(),
        trainerName: $('#trainerName option:selected').text(),
        hours: $('#totalHours').val(),
        markScored: $('#markScored').val(),
        dateAttended: $('#dateAttended').val(),
        trainingAttachment: $('#attachCertificate').val(),
        remarks: $('#remarks').val()
    };
}

// Full form validation function
function validateTrainingForm() {
    let isValid = true;

    // List of required fields
    const requiredIds = [
        'empcode', 'empname', 'trainingAttended',
        'trainerName', 'totalHours', 'markScored', 'dateAttended'
    ];

    requiredIds.forEach(function (id) {
        const $field = $('#' + id);
        if (!$field.val()) {
            $field.addClass('border-danger'); // Highlight red
            isValid = false;
        } else {
            $field.removeClass('border-danger'); // Remove red if filled
        }
    });

    return isValid;
}

// Save form via AJAX
function saveTrainingForm() {
    if (!validateTrainingForm()) {
        showModal("Validation Error", "Please fill all required fields!", 'error');
        return;
    }
    let model = getEmpTrainingModel();
    $.ajax({
        url: '/empTraining/SaveEmpTraining',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(model),
        success: function (response) {
            if (response.success) {
                showModal("Success", response.message || "Training saved successfully!", 'success');
                clearAllFields();
                $('#dateAttended').val(new Date().toISOString().split('T')[0]);
            } else {
                showModal("Error", response.message || "Failed to save training.", 'error');
            }
        },
        error: function (xhr, status, error) {
            showModal("Exception", xhr.responseText || error || "Something went wrong!", 'error');
        }
    });
}

// Clear all fields
function clearAllFields() {
    $('#content').find('input').each(function () {
        const type = $(this).attr('type');
        if (type === 'text' || type === 'number' || type === 'date' || type === 'file') {
            $(this).val('');
        } else if (type === 'checkbox' || type === 'radio') {
            $(this).prop('checked', false);
        }
    });

    $('#content').find('select').each(function () {
        $(this).prop('selectedIndex', 0); // reset to first option
    });

    $('#content').find('textarea').val('');
}

$('#btnCloseHeader, #btnCloseFooter').on('click', function () {
    $('#resultModal').modal('hide');  // Hide the modal manually
});