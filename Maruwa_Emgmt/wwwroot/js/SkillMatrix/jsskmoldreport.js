let fullData = []; // Original data from DB
let currentPage = 1;
let pageSize = 100;
let filteredData = [];  // Data after filtering
let sectionList = [];


/* INIT*/
$(document).ready(function () {
    initializeDropdownsAndYears();
});

/*INIT DROPDOWN*/
function initializeDropdownsAndYears() {
    BindDepartmentcodeDropdown();
    FindAllSectionList();
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

function FindAllSectionList() {
    $.ajax({
        url: '/SkillMatrix/FindAllSectionList',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            sectionList = response.map(x => x.sectionCode);
            console.log(sectionList);
        }
    });
}

/* SUB DEPARTMENT */
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

/* SECTION */
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
    const selectedSection = $('#section').val();
    if (!selectedSection) {
        filteredData = [...fullData];
    }
    else {
        filteredData = fullData.filter(item => {
            const sectionlst = getGroupValues(item.sectionCode);
            if (!sectionlst)
                return false;

            const sections = sectionlst.split(',').map(x => x.trim());
            return sections.includes(selectedSection);
        });
    }
    currentPage = 1;

    $("#totalRecords").text(`Total Rec: ${filteredData.length}`);
    renderTable();
    updatePagination();
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
    filteredData = [...fullData];

    currentPage = 1;
    $("#totalRecords").text(`Total Rec: ${filteredData.length}`);

    renderTable();
    updatePagination();
}

/*RENDER TABLE (PAGINATION) */
function renderTable() {
    let allRows = [];
    filteredData.forEach(item => {
        const sectionlst = getGroupValues(item.sectionCode);
        const sections = sectionlst? sectionlst.split(",").map(x => x.trim()): [item.sectionCode];

        sections.forEach(section => {
            // ✅ If sectionList is empty → skip validation (allow all)
            let isValidSection = true;
            if (sectionList && sectionList.length > 0) {
                isValidSection = sectionList.some(x =>
                    (x || "").trim().toUpperCase() === (section || "").trim().toUpperCase()
                );
            }

            if (!isValidSection) {
                return;
            }

            allRows.push({
                item,
                section
            });
        });
    });

    let start = (currentPage - 1) * pageSize;
    let end = start + pageSize;
    let pageRows = allRows.slice(start, end);

    let html = "";
    let sno = start + 1;

    pageRows.forEach(({ item, section }) => {
        html += `
            <tr>
                <td>${sno++}</td>
                <td>${item.columnNames ?? ''}</td>
                <td class="font-weight-bold">
                    <a href="/SkillMatrix/skmList?s=${encodeValue(section)}&d=${encodeValue(item.department ?? '')}">
                        ${section}
                    </a>
                </td>
                <td>${item.documentName ?? ''}</td>
                <td class="font-weight-bold text-danger text-center">${item.rev ?? ''}</td>
                <td>${item.createdBy ?? ''}</td>
                <td>${formatDate(item.createdDateTime)}</td>
            </tr>
        `;
    });

    $("#tblSkillMatrixReport tbody").html(html);
    $("#totalRecords").text(`Total Rec: ${allRows.length}`);
}



function renderTable_MultipleSectionRecords() {
    let allRows = [];
    filteredData.forEach(item => {
        const sectionlst = getGroupValues(item.sectionCode);
        const sections = sectionlst? sectionlst.split(",").map(x => x.trim()): [item.sectionCode];
        sections.forEach(section => {
            allRows.push({
                item,
                section
            });
        });
    });

    let start = (currentPage - 1) * pageSize;
    let end = start + pageSize;
    let pageRows = allRows.slice(start, end);

    let html = "";
    let sno = start + 1;

    pageRows.forEach(({ item, section }) => {
        html += `
            <tr>
                <td>${sno++}</td>
                <td>${item.columnNames ?? ''}</td>
                <td class="font-weight-bold">
                    <a href="/SkillMatrix/skmList?s=${encodeValue(section)}&d=${encodeValue(item.department ?? '')}">
                        ${section}
                    </a>
                </td>
                <td>${item.documentName ?? ''}</td>
                <td class="font-weight-bold text-danger text-center">${item.rev ?? ''}</td>
                <td>${item.createdBy ?? ''}</td>
                <td>${formatDate(item.createdDateTime)}</td>
            </tr>
        `;
    });

    $("#tblSkillMatrixReport tbody").html(html);
    $("#totalRecords").text(`Total Rec: ${allRows.length}`);
}

function renderTable_OldSingleSection() {
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
        const sectionlst = getGroupValues(item.sectionCode);
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

/* PAGINATION INFO */
function updatePagination() {
    let size = pageSize;
    let totalPages = (size === "All") ? 1 : Math.ceil(filteredData.length / parseInt(size));

    $("#paginationInfo").text(`${currentPage} / ${totalPages}`);
    $("#prevBtn").prop("disabled", currentPage <= 1);
    $("#nextBtn").prop("disabled", currentPage >= totalPages);
}

/* NEXT / PREV */
function nextPage() {
    let size = pageSize;
    let totalPages = (size === "All") ? 1: Math.ceil(fullData.length / parseInt(size));
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

/* PAGE SIZE CHANGE */
$("#pageSizeSelect").on("change", function () {

    pageSize = $(this).val();
    currentPage = 1;

    renderTable();
    updatePagination();
});

/* HELPERS */
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

function getGroupValues(selectedText) {
    const normalizedSelected = selectedText.trim().toLowerCase();
    for (const groupName in processGroups) {
        const group = processGroups[groupName];
        const matched = group.items.some(item =>
            item.trim().toLowerCase() === normalizedSelected
        );
        if (matched) {
            return group.values;
        }
    }
    return "";
}

const processGroups = {
    // TPH
    Press: {
        items: ["7120", "7420", "7520", "7620"],
        values: "7120,7620,7520,7420"
    },
    Casting: {
        items: ["7510B", "7510-20", "7110B", "7410B", "7410-20", "7110-20"],
        values: "7110B,7110-20,7510B,7710-20,7410B,7410-20"
    },
    Honing: {
        items: ["7140", "7440", "7540"],
        values: "7140,7640,7540,7440"//
    },
    Laser: {
        items: ["T7150", "7450", "7550"],
        values: "7150,7550,7450"
    },
    Packing: {
        items: ["7190", "7590", "7490"],
        values: "7190,7590,7490"//
    },
    MainFiring: {
        items: ["7130", "7430", "7530"],
        values: "7130,7530,7430"//
    },
    SecondFiring: {
        items: ["7160", "7460", "7560"],
        values: "7160,7560,740"
    },
    Inspection: {
        items: ["7170", "7470", "7670"],
        values: "7170,7670,7570,7470"
    },
    Engineering: {
        items: ["7601-12A", "7500-20", "7100-12A", "7400-20", "7401-12A", "7100-20"],
        values: "7100-12A,7100-20,7601-12A,7500-20,7400-20,7401-12A"
    },
    BallMill: {
        items: ["7510A", "7510-10", "7110A", "7410A", "7410-10", "7110-10"],
        values: "7110A,7110-10,7510-10,7410A,7410-10"
    },
    Arraying: {
        items: ["2130A", "7130A", "7730A"],
        values: "7120"
    },
    General: {
        items: ["7100", "7601", "7101", "7601", "7501", "7401"],
        values: "7100,7101,7601,7501,7401"
    },
    QA: {
        items: ["7100-10", "7500-10", "7400-10", "7110B-10"],
        values: "9010,7100-10,7110B-10,7500-10,7400-10"
    },
    Catron: {
        items: ["7170A", "7470A"],
        values: "7120,7170A,7470A"
    },
    Maintenance: {
        items: ["7102", "7103", "7403"],
        values: "7102,7103,7403"
    },
    Safety: {
        items: ["7105", "7405"],
        values: "9005,7105,7405"//
    },

    //CV
    cvFiring: {
        items: ["3030", "3130", "3140", "3230", "3430"],
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
        items: ["4200", "4200-10", "4201-10"],
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
        items: ["9010", "9011", "9100D"],
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


$(document).on('click', '#btnClear', function (e) {
    e.preventDefault();
    // Clear dropdown selections if needed
    $('#department').val('');
    $('#subDepartment').val('');
    $('#section').val('');

    // Restore all data
    filteredData = [...fullData];

    currentPage = 1;

    $("#totalRecords").text(`Total Rec: ${filteredData.length}`);

    renderTable();
    updatePagination();
});