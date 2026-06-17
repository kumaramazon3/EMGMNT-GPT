window.addEventListener('DOMContentLoaded', async () => {
    const bitval = true; //await UserAccessibility();
    if (bitval) {
        await DefaultFunctions();
    }
    else {
        $('#errorModal').modal('show');
    }
});

async function DefaultFunctions() {
    await BindLeaveTypesDropdown();
    await GetLeaveTimings();
    await GetallEmployesAsync();
    await GetEmployeeLeaveSummary("");
    initLeaveCalculation();
    // Ensure disabled on load
    $('#halfdayleave').prop('disabled', true);
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
                if (item.ltCode === "AL") {
                    $ddl.append(
                        //$('<option></option>').val(item.ltCode).text(item.ltCode + ' - ' + item.leaveType)
                        $('<option></option>').val(item.ltCode).text(item.leaveType)
                    );
                }
            });
        },
        error: function (xhr) {
            console.error('Error loading departmentName dropdown', xhr);
        }
    });
}

async function GetEmployeeLeaveSummary(empCode) {
    //if (!empCode) return;
    try {
        const data = await $.ajax({
            url: `/leave/GetEmployeeLeaveSummary?empCode=${empCode}`,
            type: 'GET',
            dataType: 'json'
        });
        // ✅ Bind Current Year --> data.currentYear -> first SP result
        $("#empName").text(data.currentYear?.empName ?? 0);
        $("#designation").text(data.currentYear?.designation ?? 0);

        $("#department").text(data.currentYear?.department + "-" + data.currentYear?.subDepartment + "-" + data.currentYear?.section);

        $("#CurrentYearAnnLeaves").text(data.currentYear?.annual ?? 0);// Total Ann Leaves
        $("#CurrentYearUtilisedAnnualLeaves").text(data.currentYear?.usedLeaves ?? 0); // Used  Leaves
        $("#CurrentyearBalanceAnnLeaves").text(data.currentYear?.remainingAnnualLeaves ?? 0);// Balance Leaves

        $("#CurrentYearAMedicalLeaves").text(data.currentYear?.med ?? 0);

        $("#CurrentYearUtilisedMedicalLeaves").text(data.currentYear?.mcUsed ?? 0);
        $("#CurrentyearMedicalBalanceLeaves").text(data.currentYear?.remainingMCLeaves ?? 0);

        // ✅ Bind Last Year --> data.lastYear -> second SP result
        $("#lstYearalb").text(data.lastYear?.actualAnnualLeaves ?? 0);// actualAnnualLeaves (Last Year)
        $("#divutilised").text(data.lastYear?.lastYear_Used_AnnualLeave ?? 0);// Used Leaves
        $("#divbalance").text(data.lastYear?.lastYear_Remaining_AnnualLeaves ?? 0);// Remaning Leaves

        $("#divprorate").text((Number(data.lastYear?.lastYear_Remaining_AnnualLeaves) || 0) + (Number(data.currentYear?.remainingAnnualLeaves) || 0));

    } catch (error) {
        console.error("Error fetching balances:", error);
        alert("Failed to load leave balances.");
    }
}

function GetLeaveTimings() {
    $.ajax({
        url: '/leave/GetLeaveTimings',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#halfdayleave');
            $ddl.empty();

            // Default option
            $ddl.append($('<option></option>').val('').text('--- Select Half-Day Leave ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.uid).text(item.fromTime + ' - ' + item.toTime)
                );
            });
        },
        error: function (xhr) {
            console.error('Error loading leave timings dropdown', xhr);
        }
    });
}

function GetallEmployesAsync() {
    $.ajax({
        url: '/leave/GetallEmployes',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#personincharge1');
            var $ddl2 = $('#personincharge2');
            $ddl.empty();
            $ddl2.empty();

            // Default option
            $ddl.append($('<option></option>').val('').text('--- Select Person In-Charge-1 ---'));
            $ddl2.append($('<option></option>').val('').text('--- Select Person In-Charge-2 ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append($('<option></option>').val(item.empCode).text(item.empName));
                $ddl2.append($('<option></option>').val(item.empCode).text(item.empName));
            });
        },
        error: function (xhr) {
            console.error('Error loading leave timings dropdown', xhr);
        }
    });
}

function toggleHalfDayLeave() {
    var isChecked = $('#chkhalfdayleave').is(':checked');

    if (isChecked) {
        $('#halfdayleave').prop('disabled', false);
    } else {
        $('#halfdayleave').prop('disabled', true);
        $('#halfdayleave').val('');
        $('#halfdayleave').removeClass('is-invalid');
    }
}

// STEP 2: Main initializer function
function initLeaveCalculation() {
    const fromInput = document.getElementById("leavePeriodFrom");
    const toInput = document.getElementById("leavePeriodTo");
    const daysInput = document.getElementById("leavedays");

    // STEP 3: Attach event listeners to date fields
    fromInput.addEventListener("change", handleDateChange);
    toInput.addEventListener("change", handleDateChange);

    // STEP 4: Attach event listener to leave days input
    daysInput.addEventListener("input", handleLeaveDaysInput);
}

// STEP 4: Main handler function
function handleDateChange() {
    const fromDate = getDateValue("leavePeriodFrom");
    const toDate = getDateValue("leavePeriodTo");

    if (!fromDate || !toDate) {
        setLeaveDays("");
        return;
    }

    if (!isValidDateRange(fromDate, toDate)) {
        //alert("End date cannot be before start date");
        $('#leavePeriodTo').addClass('is-invalid');
        setLeaveDays("");
        return;
    }
    else {
        $('#leavePeriodTo').removeClass('is-invalid');
    }

    const days = calculateDays(fromDate, toDate);
    setLeaveDays(days);

    // ✅ STEP: Call here (BEST place)
    validateHalfDayWithDates();
}

// Leave Day: 0.5 Validation
function validateHalfDayWithDates() {
    const days = document.getElementById("leavedays").value;
    const fromInput = document.getElementById("leavePeriodFrom");
    const toInput = document.getElementById("leavePeriodTo");

    const fromDate = getDateValue("leavePeriodFrom");
    const toDate = getDateValue("leavePeriodTo");

    if (days === "0.5" && fromDate && toDate) {
        if (fromDate.getTime() !== toDate.getTime()) {
            markInvalid(fromInput);
            markInvalid(toInput);
        } else {
            clearInvalid(fromInput);
            clearInvalid(toInput);
        }
    }
}

function handleLeaveDaysInput() {
    const daysInput = document.getElementById("leavedays");
    const fromInput = document.getElementById("leavePeriodFrom");
    const toInput = document.getElementById("leavePeriodTo");
    const errorSpan = document.getElementById("spnerror"); // your error span

    let value = daysInput.value;

    // STEP 1: Reset error first
    errorSpan.innerText = "";
    clearInvalid(daysInput);

    // STEP 2: Allow only numbers and decimal
    if (!/^\d*\.?\d*$/.test(value)) {
        markInvalid(daysInput);
        errorSpan.innerText = "Please enter a valid number";
        return;
    }

    // STEP 3: Validate decimal (only .5 allowed)
    if (value.includes(".")) {
        let parts = value.split(".");

        // Only allow .5
        if (parts[1] !== "5") {
            markInvalid(daysInput);
            errorSpan.innerText = "Only 0.5 is allowed for half-day leave";
            return;
        }
    }

    // STEP 4: Remove invalid if correct
    clearInvalid(daysInput);
    errorSpan.innerText = "";

    // STEP 5: If 0.5 → validate same date
    if (value === "0.5") {
        const fromDate = getDateValue("leavePeriodFrom");
        const toDate = getDateValue("leavePeriodTo");

        if (fromDate && toDate) {
            if (fromDate.getTime() !== toDate.getTime()) {
                markInvalid(fromInput);
                markInvalid(toInput);
                errorSpan.innerText = "For half-day leave, From and To dates must be the same";
            } else {
                clearInvalid(fromInput);
                clearInvalid(toInput);
            }
        }
    } else {
        // Remove date highlight if leave is not 0.5
        clearInvalid(fromInput);
        clearInvalid(toInput);
    }
}
// ---------------- HELPER FUNCTIONS ---------------- //

// STEP 5: Get date value
function getDateValue(id) {
    const value = document.getElementById(id).value;
    return value ? new Date(value) : null;
}

// STEP 6: Validate date range
function isValidDateRange(fromDate, toDate) {
    return fromDate <= toDate;
}

// STEP 7: Calculate days (inclusive)
function calculateDays(fromDate, toDate) {
    const timeDiff = toDate - fromDate;
    return (timeDiff / (1000 * 60 * 60 * 24)) + 1;
}

// STEP 8: Set value to input
function setLeaveDays(days) {
    document.getElementById("leavedays").value = days;
}

function validateForm() {
    let isValid = true;
    // List of required fields
    const fields = [
        '#leavetype',
        '#leavePeriodFrom',
        '#leavePeriodTo',
        '#leavedays',
        '#reason',
        '#personincharge1',
        '#personincharge2'
    ];

    // Loop and validate
    fields.forEach(function (selector) {
        let value = $(selector).val();

        if (!value) {
            $(selector).addClass('is-invalid');
            isValid = false;
        } else {
            $(selector).removeClass('is-invalid');
        }
    });

    // ✅ Half-day validation
    if ($('#chkhalfdayleave').is(':checked')) {
        if ($('#halfdayleave').val() === '') {
            $('#halfdayleave').addClass('is-invalid');
            isValid = false;
        } else {
            $('#halfdayleave').removeClass('is-invalid');
        }
    }
    else {
        $('#halfdayleave').removeClass('is-invalid');
    }
    return isValid;
}

$('input, select, textarea').on('change keyup', function () {
    if ($(this).val()) {
        $(this).removeClass('is-invalid');
    }
});

function validateForm_Calculation() {
    const fromInput = document.getElementById("leavePeriodFrom");
    const toInput = document.getElementById("leavePeriodTo");
    const daysInput = document.getElementById("leavedays");
    const chkHalf = document.getElementById("chkhalfdayleave");
    const halfSelect = document.getElementById("halfdayleave");
    const errorSpan = document.getElementById("spnerror");

    // Reset error messages
    errorSpan.innerText = "";
    clearInvalid(fromInput);
    clearInvalid(toInput);
    clearInvalid(daysInput);

    const daysValue = parseFloat(daysInput.value);

    // 1️⃣ Validate leave days is a number
    if (isNaN(daysValue) || daysInput.value === "") {
        markInvalid(daysInput);
        errorSpan.innerText = "Please enter a valid leave days number";
        return false;
    }

    // 2️⃣ Validate .5 for half-day only
    if (daysInput.value.includes(".")) {
        const parts = daysInput.value.split(".");
        if (parts[1] !== "5") {
            markInvalid(daysInput);
            errorSpan.innerText = "Only 0.5 is allowed for half-day leave";
            return false;
        }
    }

    // 3️⃣ Get date values
    const fromDate = getDateValue("leavePeriodFrom");
    const toDate = getDateValue("leavePeriodTo");

    if (!fromDate || !toDate) {
        markInvalid(fromInput);
        markInvalid(toInput);
        errorSpan.innerText = "Please select both From and To dates";
        return false;
    }

    // 4️⃣ Half-day rule: From and To must be same
    const isHalfDay = daysValue % 1 === 0.5;
    if (isHalfDay && fromDate.getTime() === toDate.getTime() && daysValue === 0.5) {
        chkHalf.checked = true;
    }

    if (isHalfDay && daysValue === 0.5 && fromDate.getTime() !== toDate.getTime()) {
        markInvalid(fromInput);
        markInvalid(toInput);
        errorSpan.innerText = "For half-day leave, From and To must be the same";
        return false;
    }

    // 5️⃣ Total leave calculation
    const msPerDay = 1000 * 60 * 60 * 24;
    let diffDays = Math.round((toDate - fromDate) / msPerDay) + 1; // inclusive

    // Adjust for .5
    if (isHalfDay) {
        diffDays = Math.floor(diffDays);
        chkHalf.checked = true;
        $('#halfdayleave').prop('disabled', false);
        if (!halfSelect.value) { // empty string means nothing selected
            $('#halfdayleave').addClass('is-invalid'); // highlight
        } else {
            $('#halfdayleave').removeClass('is-invalid'); // remove highlight
        }

    }
    return true; // validation passed
}

function validate_LeaveType() {
    const leavetype = document.getElementById("leavetype").value;
    const applieddays = parseFloat(document.getElementById("leavedays").value);
    const prorate = parseFloat(getDateValue("divprorate"));
    const bmedical = parseFloat(getDateValue("CurrentyearMedicalBalanceLeaves"));

    // Optional: validate number input
    if (isNaN(applieddays)) {
        alert("Please enter a valid number for leave days.");
        return false;
    }

    switch (leavetype) {
        case "AL":
            // Negative check
            if (applieddays < 0) {
                alert("Leave days cannot be negative.");
                return false;
            }
            // Minimum 0.5 day
            if (applieddays < 0.5) {
                alert("Minimum leave is half day.");
                return false;
            }
            // Exceed prorate
            if (applieddays > prorate) {
                alert("Applied days cannot be greater than prorated leave.");
                return false;
            }
            break;

        case "MC":
            // Negative check (good to keep consistent)
            if (applieddays < 0) {
                alert("Leave days cannot be negative.");
                return false;
            }

            // Exceed medical balance
            if (applieddays > bmedical) {
                alert("Exceeds medical leave balance.");
                return false;
            }
            break;

        default:
            break;
    }

    return true;
}


// Helpers
function markInvalid(el) { el.classList.add("is-invalid"); }
function clearInvalid(el) { el.classList.remove("is-invalid"); }
function getDateValue(id) {
    const val = document.getElementById(id).value;
    if (!val) return null;
    return new Date(val);
}
function validateLeaveDaysMatch() {
    const fromInput = document.getElementById("leavePeriodFrom");
    const toInput = document.getElementById("leavePeriodTo");
    const daysInput = document.getElementById("leavedays");
    const errorSpan = document.getElementById("spnerror");

    // Reset previous error highlights
    errorSpan.innerText = "";
    clearInvalid(daysInput);

    const fromDate = getDateValue("leavePeriodFrom");
    const toDate = getDateValue("leavePeriodTo");
    const enteredDays = parseFloat(daysInput.value);

    if (!fromDate || !toDate || isNaN(enteredDays)) {
        return true; // skip if dates or days not filled
    }

    // Calculate total days between dates (inclusive)
    const msPerDay = 1000 * 60 * 60 * 24;
    const diffDays = Math.round((toDate - fromDate) / msPerDay) + 1;

    // Only allow .5 increments for half-days
    if (enteredDays % 1 !== 0 && enteredDays % 1 !== 0.5) {
        markInvalid(daysInput);
        errorSpan.innerText = "Only full day or half-day (.5) increments are allowed";
        return false;
    }

    // Determine max allowed days
    const maxAllowed = diffDays % 1 === 0 ? diffDays : diffDays - 0.5;

    if (enteredDays < diffDays - 0.5 || enteredDays > diffDays) {
        markInvalid(daysInput);
        errorSpan.innerText = `Leave days do not match the selected date range. Allowed range: ${diffDays - 0.5} to ${diffDays}`;
        return false;
    }
    return true; // validation passed
}

function getDateValue(id) {
    const val = document.getElementById(id).value;
    if (!val) return null;
    return new Date(val);
}


function getDateValue(id) {
    const val = document.getElementById(id).value;
    if (!val) return null;
    return new Date(val);
}

function checkBackdate(fromDateStr) {
    var fromDate = new Date(fromDateStr);
    var today = new Date();
    fromDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);
    return fromDate < today ? 'Y' : 'N';
}

$('#btnsubmit').click(async function (e) {
    e.preventDefault(); // Prevent default form submission

    // Run validations
    if (!validateForm() || !validateForm_Calculation() || !validateLeaveDaysMatch() || !validate_LeaveType()) {
        return false;
    }

    // Prepare data
    var data = {
        EmpCode: "",
        days: parseFloat($('#leavedays').val()) || 0,
        workfor: parseFloat($('#leavedays').val()) || 0,
        fromdate: $('#leavePeriodFrom').val(),
        todate: $('#leavePeriodTo').val(),

        leavetype: $('#leavetype option:selected').text(),
        leavetype1: $('#leavetype').val(),
        reason: $('#reason').val().trim(),
        leavetime: $('#chkhalfdayleave').is(':checked') ? $('#halfdayleave').val() : '',
        backdate: checkBackdate($('#leavePeriodFrom').val()),

        AnnualBal: parseInt($('#CurrentyearBalanceAnnLeaves').text().trim()) || 0,
        MedicalBal: parseInt($('#CurrentyearMedicalBalanceLeaves').text().trim()) || 0,
        pic1: $('#personincharge1').val(),
        pic11: $('#personincharge1 option:selected').text(),
        pic2: $('#personincharge2').val(),
        pic22: $('#personincharge2 option:selected').text(),

        createdby: ""
    };

    console.log(data);

    try {
        // Use async/await instead of callbacks
        const response = await $.ajax({
            url: '/Leave/SubmitLeave',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        });

        // Success response// success
        // alert(response.message);
        if (response.success) {
            // Success styling
            $('#leaveResultModal .modal-header')
                .removeClass('bg-danger text-white')
                .addClass('bg-success text-white');

            $('#leaveResultMessage')
                .removeClass('text-danger')
                .addClass('text-success')
                .text("Success: New Leave Request created ID: (" + response.message + ")");

            $('#leaveResultModal').modal('show');
            clearLeaveForm();
        }
        else {
            // Fail styling
            $('#leaveResultModal .modal-header')
                .removeClass('bg-success text-white')
                .addClass('bg-danger text-white');

            $('#leaveResultMessage')
                .removeClass('text-success')
                .addClass('text-danger')
                .text("Failed: " + message);

            $('#leaveResultModal').modal('show');
        }
        console.log(response.message);

    } catch (xhr) {
        var response = xhr.responseJSON;
        var message = (response && response.message) ? response.message : 'Error submitting leave.';
        // Fail styling
        $('#leaveResultModal .modal-header')
            .removeClass('bg-success text-white')
            .addClass('bg-danger text-white');

        $('#leaveResultMessage')
            .removeClass('text-success')
            .addClass('text-danger')
            .text("Failed: " + message);

        $('#leaveResultModal').modal('show');
    }
});


function clearLeaveForm() {
    // Reset dropdowns
    document.getElementById("leavetype").value = "";
    document.getElementById("halfdayleave").value = "";
    document.getElementById("personincharge1").value = "";
    document.getElementById("personincharge2").value = "";

    // Reset date inputs
    document.getElementById("leavePeriodFrom").value = "";
    document.getElementById("leavePeriodTo").value = "";

    // Reset checkbox
    document.getElementById("chkhalfdayleave").checked = false;

    // Reset number input
    document.getElementById("leavedays").value = "";

    // Reset textarea
    document.getElementById("reason").value = "";

    // Clear error message
    document.getElementById("spnerror").innerText = "";

    // Optional: disable half-day dropdown if you use toggle logic
    document.getElementById("halfdayleave").disabled = true;
}



