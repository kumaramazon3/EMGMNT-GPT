let allData = [];       // Full data from AJAX
let filteredData = [];  // Filtered by search
let currentPage = 1;
let pageSize = 50;

$(document).ready(function () {
    loadDesignation();
});

// Load data from AJAX
async function loadDesignation() {
    try {
        const response = await $.ajax({
            url: '/master/GetDesignationList',
            type: 'GET'
        });

        if (response.success) {
            allData = response.data;
            currentPage = 1;
            filteredData = allData; // Initially no filter
            renderTable();
        } else {
            alert(response.message);
        }
    } catch (error) {
        alert("Error loading data");
    }
}

// Render Table with pagination
function renderTable() {
    pageSize = parseInt($("#pageSizeSelect").val());

    const totalData = filteredData.length;
    let start = 0;
    let end = totalData;

    if (pageSize !== 0) {
        start = (currentPage - 1) * pageSize;
        end = start + pageSize;
    }

    const pageData = (pageSize === 0) ? filteredData : filteredData.slice(start, end);

    let rows = "";
    pageData.forEach(item => {
        rows += `
            <tr>
                <td>${item.positionid}</td>
                <td>${item.designationname}</td>
                <td>${item.dlevel ?? ''}</td>
                <td>${item.probation}</td>
                <td>${item.inscategory ?? ''}</td>
                <td>${item.insamount ?? ''}</td>
                <td>${item.ctQlevel ?? ''}</td>
                <td>
                <i class="bi bi-pencil-square text-primary me-2" style="cursor:pointer; pointer-events:none;" onclick="editTraining('${item.sno}')" disable></i>
                <i class="bi bi-trash text-danger" style="cursor:pointer; pointer-events:none;" onclick="confirmDelete('${item.sno}')"></i>
                </td>
            </tr>
        `;
    });

    $("#tblDesignation tbody").html(rows);
    updatePageInfo();
}
// Update Page Info and enable/disable buttons
function updatePageInfo() {
    if (pageSize === 0) {
        $("#pageInfo").text(`Showing All ${filteredData.length} records`);
        $("#btnPrev, #btnNext").prop("disabled", true);
        return;
    }

    const totalPages = Math.ceil(filteredData.length / pageSize);
    $("#pageInfo").text(`Page ${currentPage} of ${totalPages}`);

    $("#btnPrev").prop("disabled", currentPage === 1);
    $("#btnNext").prop("disabled", currentPage === totalPages);
}
// Dropdown change
$("#pageSizeSelect").on("change", function () {
    currentPage = 1;
    renderTable();
});

// Prev button
$("#btnPrev").on("click", function () {
    if (currentPage > 1) {
        currentPage--;
        renderTable();
    }
});

// Next button
$("#btnNext").on("click", function () {
    const totalPages = Math.ceil(filteredData.length / pageSize);
    if (currentPage < totalPages) {
        currentPage++;
        renderTable();
    }
});

// Search box
$("#searchBox").on("input", function () {
    const searchTerm = $(this).val().toLowerCase();
    filteredData = allData.filter(item =>
        item.designationname.toLowerCase().includes(searchTerm)
    );
    currentPage = 1; // Reset to first page after search
    renderTable();
});

function showMessage(title, message, isSuccess) {

    $("#messageTitle").text(title);
    $("#messageBody").text(message);

    if (isSuccess) {
        $("#messageHeader").removeClass().addClass("modal-header bg-success text-white");
    } else {
        $("#messageHeader").removeClass().addClass("modal-header bg-danger text-white");
    }

    $("#messageModal").modal('show');
}

function closeModal() {
    $("#messageModal").modal('hide');
}

