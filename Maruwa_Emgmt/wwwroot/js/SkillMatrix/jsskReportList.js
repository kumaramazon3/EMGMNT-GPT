let skillMatrixData = []; // full dataset
let currentPage = 1;
let pageSize = 50;


$(document).ready(function () {
    BindDepartmentcodeDropdown();
});

function BindDepartmentcodeDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#department');
            $ddl.empty();
            $ddl.append($('<option></option>').val('').text('--- Select Department ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>')
                        .val(item.departmentCode) // keep code as value
                        .attr('data-name', item.departmentName) // store name separately
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
            '<option value="' + section.sectionCode + '">' + section.sectionCode + ' - ' + section.sectionName + '</option>'
        );
    });
}
// END

// Search Button
$(document).on('click', '#btnLoadSkillMatrix', function (e) {
    e.preventDefault();
    const departmentCode = $('#department option:selected').data('name');
    const sectionCode = $('#section').val();
    loadSkillMatrix(departmentCode, sectionCode); // ✔ Correct
});

async function loadSkillMatrix(departmentCode, sectionCode) {
    try {
        let url = `/SkillMatrix/GetSkillMatrixReportList?`;

        if (departmentCode) url += `departmentCode=${encodeURIComponent(departmentCode)}`;
        if (sectionCode) url += (url.endsWith('?') ? '' : '&') + `sectionCode=${encodeURIComponent(sectionCode)}`;
        console.log("Fetching URL:", url);
        const response = await fetch(url);
        const result = await response.json();
        // Check for success = true, otherwise display error from backend
        if (result.success) {
            generateSkillMatrix(result.data || []);

            skillMatrixData = result.data || [];
            currentPage = 1;
            $('#pageSizeSelect').val('50');
            pageSize = pageSize;

            renderPage(); // display first page
        } else {
            showErrorModal(result.message || result.error || "Unknown error from server");
        }
    } catch (error) {
        console.error("Error loading skill matrix:", error);
        showErrorModal(error.message);
    }
}

function generateSkillMatrix(data) {
    const $tbody = $('#tblSkillMatrixReport tbody');
    $tbody.empty(); // Clear existing rows

    if (!data || data.length === 0) {
        $tbody.append(`
            <tr>
                <td colspan="15" class="text-center text-danger">
                    No skill matrix data found.
                </td>
            </tr>
        `);
        return;
    }

    data.forEach((item, index) => {
        $tbody.append(`
            <tr>
                <td>${index + 1}</td>
                <td>${item.columnNames || ''}</td>
                <td title="${item.documentName || ''}">
                    ${item.documentName ? item.documentName.substring(0, 15) + (item.documentName.length > 15 ? '...' : '') : ''}
                </td>
                <td>${item.department || ''}</td>
               <td>
                    <a href="javascript:void(0);"
                       class="text-primary font-weight-bold"
                       onclick="onSectionClick('${item.columnNames}','${item.department}', '${item.sectionCode}')">
                       ${item.sectionCode || ''}
                    </a>
                </td>
                <td>${item.current_score || ''}</td>
                <td style="${item.old_score && item.old_score !== item.current_score ? 'color:red; font-weight:bold;' : ''}">
                    ${item.old_score || ''}
                </td>
                <td>${item.skillDate ? formatDate(item.skillDate) : ''}</td>
                
                  <td style="${
                                item.old_skilldate && item.skillDate && formatDate(item.old_skilldate) !== formatDate(item.skillDate)
                                    ? 'color:red; font-weight:bold;'
                                    : ''
                                }">
                        ${item.old_skilldate ? formatDate(item.old_skilldate) : ''}
                    </td>

                <td>${item.rev || ''}</td>
                <td>${item.empCode || ''}</td>

                <td title="${item.empName || ''}">
                    ${item.empName ? item.empName.substring(0, 10) + (item.empName.length > 10 ? '...' : '') : ''}
                </td>
                <td>${item.position || ''}</td>
                <td>${item.createdBy || ''}</td>
                <td>${item.createdDateTime ? formatDate(item.createdDateTime) : ''}</td>
                <td>${item.modifyedBy || ''}</td>
                <td>${item.maxLogDate ? formatDateOnly(item.maxLogDate) : ''}</td>
            </tr>
        `);
    });
}

// Helper: format date (yyyy-mm-dd)
function formatDate(dateStr) {
    const d = new Date(dateStr);
    if (isNaN(d)) return '';
    return d.toLocaleDateString('en-CA'); // yyyy-mm-dd
}

// Helper: format date and time (yyyy-mm-dd)
function formatDateOnly(dateStr) {
    const d = new Date(dateStr);
    if (isNaN(d)) return '';
    return d.toISOString().split('T')[0];
}

function showErrorModal(message) {
    $('#errorModalBody').text(message);
    $('#errorModal').modal('show'); // Bootstrap 4 way
}

//END


// Paging
$('#pageSizeSelect').on('change', function () {
    const value = $(this).val();
    if (value === 'All') {
        pageSize = skillMatrixData.length; // show all
    } else {
        pageSize = parseInt(value);
    }
    currentPage = 1; // reset to first page
    renderPage();
});
function nextPage() {
    const totalPages = Math.ceil(skillMatrixData.length / pageSize);
    if (currentPage < totalPages) {
        currentPage++;
        renderPage();
    }
}

function prevPage() {
    if (currentPage > 1) {
        currentPage--;
        renderPage();
    }
}
function renderPage() {
    const totalPages = Math.ceil(skillMatrixData.length / pageSize) || 1;
    if (currentPage > totalPages) currentPage = totalPages; // safety

    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const pageData = skillMatrixData.slice(startIndex, endIndex);

    generateSkillMatrix(pageData);

    // Update pagination info: Current / Total
    $('#paginationInfo').text(`${currentPage} / ${totalPages}`);

    // Optional: disable prev/next buttons when at first/last page
    $('button[onclick="prevPage()"]').prop('disabled', currentPage === 1);
    $('button[onclick="nextPage()"]').prop('disabled', currentPage === totalPages);
}
// Paging END


// Link Navigation
function onSectionClick(columnNames,department, sectionCode) {
    console.log("CDoc No: ", columnNames);
    console.log("Department: ", department);
    console.log("Section: ", sectionCode);
    //alert("departmentCode: " + department + ", sectionCode: " + sectionCode);

    // Call API to encrypt OR use base64 (simple option)
    var encdoc = btoa(columnNames);      // encode
    var encDept = btoa(department);      // encode
    var encSection = btoa(sectionCode);  // encode
    // Redirect to controller
    window.location.href = '/SkillMatrix/skmList?d=' + encDept + '&s=' + encSection + '&c=' + encdoc;
}
