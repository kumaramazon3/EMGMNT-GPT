let fullData = [];
let currentPage = 1;
let pageSize = 50;

/* =========================
   INIT
========================= */
$(document).ready(function () {
    initializeDropdownsAndYears();
});

/* =========================
   INIT DROPDOWN
========================= */
function initializeDropdownsAndYears() {
    BindDepartmentcodeDropdown();
    loadSkillMatrix("", "", "", "0");
}

function BindDepartmentcodeDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#department');
            $ddl.empty();
            $ddl.append('<option value="">--- Select Department ---</option>');

            $.each(response, function (index, item) {
                $ddl.append(
                    `<option value="${item.departmentCode}" data-name="${item.departmentName}">
                        ${item.departmentCode} - ${item.departmentName}
                    </option>`
                );
            });
        },
        error: function (xhr) {
            console.error(xhr);
        }
    });
}

/* =========================
   SUB DEPARTMENT
========================= */
function onDepartmentClick() {
    var departmentCode = $('#department').val();

    if (departmentCode) {
        fetchSubDepartments(departmentCode);
    } else {
        $('#subDepartment').html('<option value="">--Select Sub Dept--</option>');
    }
}

function fetchSubDepartments(departmentCode) {
    $.ajax({
        url: '/EmpMaster/GetSubDepartments',
        type: 'GET',
        data: { departmentCode: departmentCode },
        success: function (data) {
            bindSubDepartmentsToDropdown(data);
        },
        error: function (err) {
            console.error(err);
        }
    });
}

function bindSubDepartmentsToDropdown(data) {
    var ddl = $('#subDepartment');
    ddl.empty();
    ddl.append('<option value="">--Select Sub Dept--</option>');

    data.sort((a, b) => a.subDepartmentName.localeCompare(b.subDepartmentName));

    data.forEach(item => {
        ddl.append(
            `<option value="${item.subDepartmentCode}">
                ${item.subDepartmentCode} - ${item.subDepartmentName}
            </option>`
        );
    });
}

/* =========================
   SECTION
========================= */
$('#subDepartment').on('change', function () {
    var subDept = $(this).val();

    if (subDept) {
        fetchSections(subDept);
    } else {
        $('#section').html('<option value="">--Select Section--</option>');
    }
});

function fetchSections(subDept) {
    $.ajax({
        url: '/SkillMatrix/GetSubDeptSections',
        type: 'GET',
        data: { SubDeptCode: subDept },
        success: function (data) {
            bindSections(data);
        },
        error: function (err) {
            console.error(err);
        }
    });
}

function bindSections(data) {
    var ddl = $('#section');
    ddl.empty();
    ddl.append('<option value="">--Select Section--</option>');

    data.sort((a, b) => a.sectionName.localeCompare(b.sectionName));

    data.forEach(item => {
        ddl.append(
            `<option value="${item.sectionCode}">
                ${item.sectionCode} - ${item.sectionName}
            </option>`
        );
    });
}

/*SEARCH BUTTON*/
$(document).on('click', '#btnLoadSkillMatrix', function (e) {
    e.preventDefault();

    const departmentCode = $('#department option:selected').data('name');
    const subDepartmentCode = $('#subDepartment').val();
    const sectionCode = $('#section').val();
    const type =0;

    loadSkillMatrix(departmentCode, subDepartmentCode, sectionCode, type);
});

/* LOAD DATA*/
async function loadSkillMatrix(departmentCode, subDepartmentCode, sectionCode, type) {
    try {
        let url = `/SkillMatrix/GetSkillMatrixReport?`;
        if (departmentCode) url += `departmentCode=${encodeURIComponent(departmentCode)}&`;
        if (subDepartmentCode) url += `subDepartmentCode=${encodeURIComponent(subDepartmentCode)}&`;
        if (sectionCode) url += `sectionCode=${encodeURIComponent(sectionCode)}&`;
        //if (type) url += `reportType=${encodeURIComponent(type)}&`;

        // FIXED PART 👇
        if (type !== undefined && type !== null)
            url += `reportType=${encodeURIComponent(type)}&`;


        const response = await fetch(url);
        const result = await response.json();

        if (response.ok && result.success) {
            generateSkillMatrix(result.data);
        } else {
            showError(result.message);
        }
    } catch (error) {
        showError(error.message);
    }
}

/* MAIN TABLE BIND */
function generateSkillMatrix(data) {

    if (!Array.isArray(data) || data.length === 0) {
        $("#tblSkillMatrixReport tbody").html(`
            <tr>
                <td colspan="11" class="text-center text-danger fw-bold">
                    No new Revision Data Found
                </td>
            </tr>
        `);
        $("#totalRecords").text("");
        return;
    }

    fullData = data;
    currentPage = 1;

    $("#totalRecords").text(`Total Rec: ${fullData.length}`);

    renderTable();
    updatePagination();
}

/*RENDER TABLE (PAGINATION) */
function renderTable() {
    let start = 0;
    let end = fullData.length;
    let size = pageSize;
    if (size !== "All") {
        size = parseInt(size);
        start = (currentPage - 1) * size;
        end = start + size;
    }
    let pageData = fullData.slice(start, end);
    let rows = "";

    pageData.forEach((item, index) => {
        rows += `
            <tr>
                <td>${start + index + 1}</td>
                <td>${item.columnNames ?? ''}</td>
                <td class="font-weight-bold">
                   <a href="/SkillMatrix/skmList?s=${encodeValue(item.sectionCode ?? '')}&d=${encodeValue(item.department ?? '')}"> ${item.sectionCode ?? ''}</a>
                  </td>
                <td>${item.documentName ?? ''}</td>

                <td class="font-weight-bold text-danger text-center">${item.rev ?? ''}</td>

                
                <td>${item.createdBy ?? ''}</td>
                <td>${formatDate(item.createdDateTime)}</td>
            </tr>
        `;
    });

    $("#tblSkillMatrixReport tbody").html(rows);
}

/* =========================
   PAGINATION INFO
========================= */
function updatePagination() {

    let size = pageSize;

    let totalPages = (size === "All")
        ? 1
        : Math.ceil(fullData.length / parseInt(size));

    $("#paginationInfo").text(`${currentPage} / ${totalPages}`);

    $("#prevBtn").prop("disabled", currentPage <= 1);
    $("#nextBtn").prop("disabled", currentPage >= totalPages);
}

/* =========================
   NEXT / PREV
========================= */
function nextPage() {

    let size = pageSize;

    let totalPages = (size === "All")
        ? 1
        : Math.ceil(fullData.length / parseInt(size));

    if (currentPage < totalPages) {
        currentPage++;
        renderTable();
        updatePagination();
    }
}

function prevPage() {

    if (currentPage > 1) {
        currentPage--;
        renderTable();
        updatePagination();
    }
}

/* =========================
   PAGE SIZE CHANGE
========================= */
$("#pageSizeSelect").on("change", function () {

    pageSize = $(this).val();
    currentPage = 1;

    renderTable();
    updatePagination();
});

/* =========================
   HELPERS
========================= */
function formatDate(dateStr) {
    if (!dateStr) return '';
    let d = new Date(dateStr);
    if (isNaN(d)) return '';
    return d.toLocaleDateString('en-CA');
}

function showError(msg) {
    console.error(msg);
    alert(msg);
}

function encodeValue(value) {
    return btoa(value); // base64
}

