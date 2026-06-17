let deleteCode = 0;
let trainingData = [];
let currentPage = 1;
let pageSize = 20;

$(document).ready(() => {
    loadNextTrainingCode();
    loadTraining();

    $('#btnSave').click(() => {
        const btn = document.getElementById('btnSave');
        if (btn.textContent === 'ADD') saveTraining();
        else updateTraining();
    });

    $('#btnClear').click(clearForm);
    $('#btnConfirmDelete').click(() => deleteTraining(deleteCode));

    $('#btnNext').click(nextPage);
    $('#btnPrev').click(prevPage);
});

// Get next training code
function loadNextTrainingCode() {
    $.ajax({
        url: '/TM/GetNextTrainingCode',
        type: 'GET',
        dataType: 'json',
        success: function (res) {
            if (res && res.nextCode) {
                $('#txtTrainingCode').val(res.nextCode);
            }
        },
        error: function (xhr) {
            console.error('Error loading next training code', xhr.responseText);
        }
    });
}

function loadTraining() {
    $.ajax({
        url: '/TM/GetTrainingList',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            trainingData = response || [];
            currentPage = 1;
            renderTable();
        },
        error: function (xhr) {
            console.error('Error loading trainings:', xhr.responseText);
        }
    });
}

function renderTable() {
    const $tbody = $('#tblTraining tbody');
    $tbody.empty();

    // Filter data by search query
    let filteredData = trainingData.filter(item =>
        item.titleName.toLowerCase().includes(searchQuery)
    );

    if (filteredData.length === 0) {
        $tbody.append('<tr><td colspan="3" class="text-center text-danger">No training found.</td></tr>');
        $('#pageInfo').text('');
        return;
    }

    let start = (currentPage - 1) * pageSize;
    let end = start + pageSize;

    // If pageSize = 0, show all rows
    if (pageSize === 0) {
        start = 0;
        end = filteredData.length;
    }

    const pageData = filteredData.slice(start, end);

    $.each(pageData, (index, item) => {
        const row = `
            <tr>
                <td>${item.code}</td>
                <td>${item.titleName}</td>
                <td class="text-center">
                    <i class="bi bi-pencil-square text-primary me-2" style="cursor:pointer" onclick="editTraining('${item.code}')"></i>
                    <i class="bi bi-trash text-danger" style="cursor:pointer" onclick="confirmDelete('${item.code}')"></i>
                </td>
            </tr>
        `;
        $tbody.append(row);
    });

    updatePaginationInfo(filteredData.length);
}

function updatePaginationInfo(filteredLength) {
    const totalItems = filteredLength !== undefined ? filteredLength : trainingData.length;
    let totalPages = pageSize === 0 ? 1 : Math.ceil(totalItems / pageSize);

    $('#pageInfo').text(`Page ${currentPage} of ${totalPages}`);

    $('#btnPrev').prop('disabled', currentPage === 1);
    $('#btnNext').prop('disabled', currentPage === totalPages);
}

// Update next/prev functions to handle filtered data
function nextPage() {
    const filteredLength = trainingData.filter(item =>
        item.titleName.toLowerCase().includes(searchQuery)
    ).length;

    if (currentPage * pageSize < filteredLength) {
        currentPage++;
        renderTable();
    }
}

function prevPage() {
    if (currentPage > 1) {
        currentPage--;
        renderTable();
    }
}
// Save Training
async function saveTraining() {
    const $titleInput = $('#txtTrainingTitle');
    const titleName = $titleInput.val().trim();
    const trainingCode = $('#txtTrainingCode').val();

    // ❌ Highlight if empty
    if (!titleName) {
        $titleInput.addClass('border-danger'); // red border
        return; // stop save
    }

    // Remove highlight if valid
    $titleInput.removeClass('border-danger');

    const payload = { titleName };
    if (trainingCode) payload.code = trainingCode;

    try {
        const response = await fetch('/TM/SaveTraining', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const result = await response.json();

        if (!response.ok) throw new Error(result.message || 'Something went wrong');

        showSuccessModal(result.message || 'Training saved successfully');
        clearForm();
        loadTraining();

    } catch (err) {
        showErrorModal(err.message);
    }
}

$('#txtTrainingTitle').on('input', function () {
    if ($(this).val().trim() !== '') {
        $(this).removeClass('border-danger');
    }
});

// Update Training
async function updateTraining() {
    const $titleInput = $('#txtTrainingTitle');
    const titleName = $titleInput.val().trim();
    const trainingCode = $('#txtTrainingCode').val();

    // ❌ Highlight if empty
    if (!titleName) {
        $titleInput.addClass('border-danger'); // red border
        return; // stop update
    }

    // Remove highlight if valid
    $titleInput.removeClass('border-danger');

    const model = {
        code: trainingCode,
        titleName: titleName
    };
    try {
        const response = await fetch('/TM/UpdateTraining', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(model)
        });

        const result = await response.json();

        if (!response.ok) {
            throw new Error(result.message || 'Update failed');
        }

        showSuccessModal(result.message || 'Training updated successfully');
        loadTraining();
        clearForm(); // optional: reset form and header to "Add Training Master"

    } catch (err) {
        showErrorModal(err.message || 'Update failed');
    }
}

// Edit Training
async function editTraining(code) {
    try {
        const response = await fetch(`/TM/GetTrainingById?code=${code}`);

        if (!response.ok) {
            throw new Error('Unable to load training details');
        }

        const data = await response.json();

        $('#txtTrainingCode').val(data.code);
        $('#txtTrainingTitle').val(data.titleName);

        const btn = $('#btnSave');
        btn.text('UPDATE').removeClass('btn-primary').addClass('btn-warning');

        // ✅ CHANGE HEADER TITLE
        $('#trainingHeaderTitle').text('Update Training Master');

    } catch (err) {
        showErrorModal(err.message || 'Error loading training');
    }
}

// Delete Training
async function deleteTraining(code) {
    try {
        DeleteConfirmcloseModal();
        const response = await fetch('/TM/DeleteTraining', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ code })
        });

        const result = await response.json();

        if (!response.ok) {
            throw new Error(result.message || 'Delete failed');
        }

        // ✅ SUCCESS MESSAGE
        showSuccessModal(result.message || 'Training deleted successfully');

        loadTraining();
        clearForm();

    } catch (err) {
        showErrorModal(err.message);
    }
}

// Clear Form
function clearForm() {
    // Clear input values
    $('#txtTrainingCode').val('');
    $('#txtTrainingTitle').val('');

    // Remove any highlight/red border
    $('#txtTrainingTitle').removeClass('border-danger');

    // Load next training code
    loadNextTrainingCode();

    // Reset Save button to ADD
    const btn = $('#btnSave');
    btn.text('ADD');
    btn.removeClass('btn-warning').addClass('btn-primary');

    // Reset header title
    $('#trainingHeaderTitle').text('Add Training Master');
}

// Confirm Delete Modal
function confirmDelete(code) {
    deleteCode = code;
    $('#deleteModal').modal('show');
}

function DeleteConfirmcloseModal() {
    $('#deleteModal').modal('hide');  // Bootstrap 4 way
}

function showErrorModal(message) {
    const header = document.getElementById('messageHeader');
    const title = document.getElementById('messageTitle');
    const body = document.getElementById('messageBody');

    // Remove existing custom classes
    header.classList.remove('modal-header-success', 'modal-header-error');

    // Add error class
    header.classList.add('modal-header-error');
    title.innerText = 'Error';
    body.innerText = message;

    messageModalInstance.show();
}
function showSuccessModal(message) {
    const header = document.getElementById('messageHeader');
    const title = document.getElementById('messageTitle');
    const body = document.getElementById('messageBody');

    // Remove existing custom classes
    header.classList.remove('modal-header-success', 'modal-header-error');

    // Add success class
    header.classList.add('modal-header-success');
    title.innerText = 'Success';
    body.innerText = message;

    messageModalInstance.show();
}


$('#pageSizeSelect').change(function () {
    const value = parseInt($(this).val());
    pageSize = value; // 0 = All
    currentPage = 1;
    renderTable();
});

let searchQuery = '';
$('#searchTraining').on('input', function () {
    searchQuery = $(this).val().trim().toLowerCase();
    currentPage = 1; // reset to first page on search
    renderTable();
});

function closeModal() {
    messageModalInstance.hide(); // Hides the modal reliably
}

// Create this once, globally
const messageModalInstance = new bootstrap.Modal(
    document.getElementById('messageModal')
);
