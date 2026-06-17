// Initialize the modal once globally
document.addEventListener("DOMContentLoaded", function () {
    (async function () {

        await loadTrainingPrograms();
        await GetDepartmentDropdown();
        await GetTrainingTitleDropdown();

        // Initialize modal
        $('#editTrainingModal').modal({
            backdrop: 'static',
            keyboard: false,
            show: false   // Important: don't auto show
        });

        // CLEAR DROPDOWNS WHEN MODAL CLOSES
        $('#editTrainingModal').on('hidden.bs.modal', function () {
            $('#subDeptCode').empty().append('<option value="">--- Select sub-Dept ---</option>');
            $('#sectionCode').empty().append('<option value="">--- Select section ---</option>');

        });

    })();
});



// Async function to fetch data
async function loadTrainingPrograms() {
    try {
        const response = await fetch('/empTraining/getTrainingPrograms');
        if (!response.ok) {
            // Try to read JSON error message from server
            const errorData = await response.json();
            alert(errorData.message || 'Error fetching training programs');
            return;
        }
        const data = await response.json();
        populateTable(data);
    } catch (error) {
        // Network error or JS error
        alert('An unexpected error occurred: ' + error.message);
    }
}

// Function to populate HTML table
function populateTable(trainingPrograms) {
    const tbody = document.getElementById('tblbodytrainingprogram');
    tbody.innerHTML = ''; // clear previous data

    trainingPrograms.forEach(tp => {
        const tr = document.createElement('tr');

        tr.innerHTML = `
            <td>${tp.trainingCode || ''}</td>
            <td>${tp.program || ''}</td>
            <td>${tp.deptName || ''}</td>
            <td>${tp.subDeptName || ''}</td>
            <td>${tp.sectionName || ''}</td>
            <td>${tp.Method || ''}</td>
            <td>${tp.Type || ''}</td>
            <td class="text-center">
                <button class="btn btn-info btn-sm" onclick="editTraining(${tp.UID})">Edit</button>
            </td>
        `;

        tbody.appendChild(tr);
    });
}

// Example Edit button function
function editTraining(uid) {
    console.log('Edit UID:', uid);

    // Make an AJAX call to your API or controller to get the data
    $.ajax({
        url: '/empTraining/GetTrainingById',  // Replace with your actual endpoint
        type: 'GET',
        data: { uid: uid },
        success: function (data) {
            // Populate modal fields
            $('#empcode').val(data.empCode);
            $('#empname').val(data.empName);

            $('#editTrainingModal input[name="department"]').val(data.deptName);
            $('#editTrainingModal input[name="subDepartment"]').val(data.subDeptName);
            $('#editTrainingModal input[name="section"]').val(data.sectionName);

            $('#editTrainingModal input[name="dateOfJoin"]').val(data.dateOfJoin);
            $('#editTrainingModal select[name="trainingAttended"]').val(data.trainingCode);
            $('#editTrainingModal input[name="trainingProgram"]').val(data.trainingTitle);

            $('#method').val(data.method);
            $('#type').val(data.type);
            $('#trainerName').val(data.trainerName);

            $('#totalHours').val(data.totalHours);
            $('#markScored').val(data.markScored);
            $('#dateAttended').val(data.dateAttended?.split('T')[0]); // Format date for input

            $('#remarks').val(data.remarks);

            // Show the modal
            $('#editTrainingModal').modal('show');
        },
        error: function (xhr, status, error) {
            console.error('Error fetching training data:', error);
            alert('Unable to fetch training details.');
        }
    });
}

// Directly attach click event
$('#btnAddTraining').click(function () {
    $('#editTrainingModal input, #editTrainingModal select, #editTrainingModal textarea').val('');
    $('#attachCertificate').val('');
    $('#editTrainingModalLabel').text('Add Training Program');
    $('#editTrainingModal').modal('show');
});
function closeModal() {
    $('#editTrainingModal').modal('hide'); // This will hide the modal programmatically
}

function GetDepartmentDropdown() {
    $.ajax({
        url: '/empTraining/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#deptCode');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Department ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>')
                        .val(item.departmentCode)
                        .text(item.departmentCode + ' - ' + item.departmentName)
                );
                //$ddl.append(
                //    $('<option></option>').val(item.departmentCode).text(item.departmentName)
                //);
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
function GetTrainingTitleDropdown() {
    $.ajax({
        url: '/empTraining/Getmaster_TrainingTitle',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#trainingCode');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Training Title ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.code).text(item.titleName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading Training Title dropdown', xhr);
            }
        }
    });
}

function onDepartmentClick() {
    var departmentCode = $('#deptCode').val(); // get selected value
    if (departmentCode) {
        fetchSubDepartments(departmentCode); // call your existing function
    } else {
        $('#subDeptCode').empty().append('<option>--Select Section--</option>');
    }
}

function fetchSubDepartments(departmentCode) {
    $.ajax({
        url: '/empTraining/GetSubDepartments',  // Controller method to fetch sections
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
    var sectionDropdown = $('#subDeptCode'); /*subdepartmentcode*/
    sectionDropdown.empty();  // Clear existing options
    sectionDropdown.append('<option>--Select sub-Dept--</option>');  // Default option

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
$('#subDeptCode').on('change', function () {
    var SubDeptCode = $(this).val();  // Get the selected department code
    if (SubDeptCode) {
        fetchGetSubDeptSections(SubDeptCode);
    } else {
        $('#sectionCode').empty().append('<option>--Select Section--</option>');
    }
});
// Function to make AJAX call to fetch sections
function fetchGetSubDeptSections(SubDeptCode) {
    $.ajax({
        url: '/empTraining/GetSubDeptSections',  // Controller method to fetch sections
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
    var sectionDropdown = $('#sectionCode'); /*sectioncode*/
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