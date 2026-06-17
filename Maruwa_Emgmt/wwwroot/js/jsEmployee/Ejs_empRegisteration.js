const perms = window.userPermissions || {};
let canView = false;
let canEdit = false;
let canDelete = false;

window.addEventListener('DOMContentLoaded', async () => {
    await DefaultPageLoadBind();   // ✅ wait until dropdowns exist
    await Edit_ViewBinding();      // ✅ then bind employee data


    const hostelName = document.getElementById("hostelName");
    hostelName.addEventListener("change", function () {
        console.log("Hostel changed:", this.value);
        fetchHouseNumbers(this.value);
    });

    document.getElementById("photo").addEventListener("change", function (event) {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                const img = document.getElementById("photoPreview");
                img.src = e.target.result;
                img.style.display = "block";
            }
            reader.readAsDataURL(file);
        }
    });

    const dobInput = document.getElementById("dateOfBirth");
    const today = new Date();

    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0'); // months are 0-11
    const dd = String(today.getDate()).padStart(2, '0');

    const maxDate = `${yyyy}-${mm}-${dd}`;
    dobInput.setAttribute("max", maxDate); // prevent future dates
});

async function DefaultPageLoadBind() {
    clearForm();
    setTodayForUniformDate();
    setCurrentDateForInputs();
    restrictDOB();
    setupEmailValidation();
    validateImageInput(); 
    validateFiles();
    setupForeignEmpRadioHandler();
    loadNextEmpCode();
    AccessDefaultRadibutton();

    hideHostelControlsByDefault();
    setupRadioDisableList();
    licensetypeDisable(); 

    DefaultMaridStatus();
    document.querySelector("input[name='isTransport'][value='N']").checked = true;
    document.getElementById("route").disabled = true;
    setupTransportNeededControls("isTransport", ["route"]);

    document.querySelector("input[name='gender'][value='M']").checked = true;

    const masterdropdowndata = await fetchDropdownData();
    bindDropdowns(masterdropdowndata);

    const formElements = document.querySelectorAll('input, select, textarea');
    formElements.forEach((el, index) => {
        el.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault(); // prevent form submission
                let nextIndex = index + 1;

                // Skip disabled or hidden fields
                while (
                    nextIndex < formElements.length &&
                    (formElements[nextIndex].disabled || formElements[nextIndex].offsetParent === null)
                ) {
                    nextIndex++;
                }

                if (nextIndex < formElements.length) {
                    formElements[nextIndex].focus();
                }
            }
        });
    });

    const today = new Date().toISOString().split("T")[0];
    document.querySelectorAll(".auto-date").forEach(input => input.value = today);

    document.querySelector("input[name='isStayingHostel'][value='N']").checked = true; /*stayinhostel*/

    document.querySelector("input[name='isDrivingLicense'][value='N']").checked = true;
    setupDrivingLicenseControls("isDrivingLicense", ["divlicensetype", "vehicleNo1", "vehicleNo2", "vehicleNo3"]);

    disableDownloadLinks();
    fetchCheckboxDropdownData();
    handleNationalityChange(); 

    await BindDepartmentcodeDropdown();
    await BindNationalityDropdown();
    await BindBloodGroupDropdown();
    await BindBankDropdown();
    await BindEducationDropdown();
    await BindHostalDropdown();
    await BindLanguageDropdown();
    await BindRaceDropdown();
    await GetmasterReligionDropdown();
    await GetmasterRouteDropdown();
    await GetmasterDesignationDropdown();
    await GetmasterEmployeType();
}

function BindNationalityDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Nationality',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#nationality');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Nationality ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.nationCOde).text(item.nationName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading nationality dropdown', xhr);
            }
        }
    });
}
function BindBloodGroupDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_BloodGroup',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#bloodGroup');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select bloodGroup ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.bloodGroupCode).text(item.bloodGroup)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading bloodGroup dropdown', xhr);
            }
        }
    });
}
function BindBankDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Bank',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#bankName'); /*bank*/
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select bankName ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.bankCode).text(item.bankName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading bankName dropdown', xhr);
            }
        }
    });
}
function BindEducationDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Education',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#educationLevel');/* edulevel*/
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select educationName ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.educationCode).text(item.educationName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading educationName dropdown', xhr);
            }
        }
    });
}
function BindHostalDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Hostal',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#hostelName'); /*hostelname*/
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Hostal ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.hostalCode).text(item.hostalName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading hostelname dropdown', xhr);
            }
        }
    });
}
function BindLanguageDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Language',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#languageProficiency'); /*languagespeaking*/
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select language ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.languageCode).text(item.languageName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading languageName dropdown', xhr);
            }
        }
    });
}
function BindDepartmentcodeDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#department');
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
function BindRaceDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Race',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#race');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Race ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.raceCode).text(item.raceName)
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
function GetmasterReligionDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Religion',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#religion');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Race ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.religionCode).text(item.religionName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login;
            } else {
                console.error('Error loading religionName dropdown', xhr);
            }
        }
    });
}
function GetmasterRouteDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Route',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#route');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Route ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.routeCode).text(item.routeName)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading religionName dropdown', xhr);
            }
        }
    });
}
function GetmasterDesignationDropdown() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Designation',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#designation'); 
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Designation ---'));
            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.designationname).text(item.designationname)
                    //$('<option></option>').val(item.desigcode).text(item.designationname)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading designationname dropdown', xhr);
            }
        }
    });
}
function Getmaster_Department() {
    $.ajax({
        url: '/EmpMaster/Getmaster_Department',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#department');
            $ddl.empty();
            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Department ---'));

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.departmentCode).text(item.departmentName)
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
function GetmasterDesignationDropdown(selectedDesignation) {
    $.ajax({
        url: '/EmpMaster/Getmaster_Designation',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#designation');
            $ddl.empty();

            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Designation ---')
            );

            // Bind data
            $.each(response, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.designationname.trim()).text(item.designationname.trim()));
                //$('<option></option>').val(item.desigcode.trim()).text(item.designationname.trim()));
            });

            // ✅ BIND EDIT VALUE HERE (AFTER DATA LOAD)
            if (selectedDesignation) {
                $ddl.val(selectedDesignation.trim());
            }
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading designation dropdown', xhr);
            }
        }
    });
}

function GetmasterEmployeType() {
    $.ajax({
        url: '/EmpMaster/Getmaster_EmployeeType',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            var $ddl = $('#empType');
            $ddl.empty();

            // Default option
            $ddl.append(
                $('<option></option>').val('').text('--- Select Department ---')
            );

            // Bind data
            $.each(response.records, function (index, item) {
                $ddl.append(
                    $('<option></option>').val(item.empType).text(item.empType)
                    //$('<option></option>').val(item.empcode).text(item.empType)
                );
            });
        },
        error: function (xhr) {
            if (xhr.status === 401) {
                window.location.href = '/LoginUser/Login';
            } else {
                console.error('Error loading EmpType dropdown', xhr);
            }
        }
    });
}

// Bind the nationality dropdown change event
$("#nationality").change(function () {
    handleNationalityChange(); // Run when nationality is selected
});

// This function runs when the nationality is changed
function handleNationalityChange() {
    const nationality = $("#nationality").val(); // Get the selected nationality
    const isMalay = nationality === "MALAY" || nationality === "MAL"; // Check if MALAY or MAL is selected
    // Enable or disable controls based on nationality
    if (isMalay) {
        // Enable IC-related controls for Malays
        $("#icNo").prop("disabled", false); /*newicno*/
        $("#icFilePath").prop("disabled", false);

        // Disable Passport-related controls for Malays
        $("#passportNo").prop("disabled", true);
        $("#passportfileurl").prop("disabled", true);

        // Select radio button N (No Foreign Worker) and disable it
        $("input[name='isForeign'][value='N']").prop("checked", true); /*foreignemp*/
        $("input[name='isForeign']").prop("disabled", true); // Disable radio buttons
    } else {
        // Disable IC-related controls for non-Malays
        $("#icNo").prop("disabled", true);
        $("#icFilePath").prop("disabled", true);

        // Enable Passport-related controls for non-Malays
        $("#passportNo").prop("disabled", false);
        $("#passportfileurl").prop("disabled", false);

        // Select radio button Y (Foreign Worker) and disable it
        $("input[name='isForeign'][value='Y']").prop("checked", true);
        $("input[name='isForeign']").prop("disabled", true); // Disable radio buttons
    }

    if (nationality === "") {
        $("#passportNo").prop("disabled", true);
        $("#passportfileurl").prop("disabled", true);
        $("#icNo").prop("disabled", true);
        $("#icFilePath").prop("disabled", true);
    }
}

function DefaultMaridStatus() {
    $("#spouseName").prop("disabled", true); /*spousename*/
    $("#noOfChildren").prop("disabled", true); /*noofchildren*/

    $("input[name='spouseWorking']").prop("disabled", true); /*isspouseworking*/

    $("#spousename").val(""); /*spousename*/
    $("#noOfChildren").val(0); /*noofchildren*/

    $("input[name='spouseWorking'][value='N']").prop("checked", true); /*isspouseworking*/
}

async function Edit_ViewBinding() {
    try {
        // Check if page is in Edit/View mode
        const mode = document.getElementById("hdnMode").value?.toLowerCase();
        const empDataHidden = document.getElementById("hdnEmployeeData").value;
        const empData = empDataHidden ? JSON.parse(empDataHidden) : null;
        if (empData && (mode === "edit" || mode === "view")) {
            controlActionButtons(mode);
            bindEmployee(empData);
            if (mode === "view") {
                disableAllEmployeeControls(); // disable all inputs for view
                document.getElementById("btnsave").style.display = "none";
                document.getElementById("btnClear").style.display = "none";
            }
            else if (mode === "edit") {
                document.getElementById("btnsave").textContent = "Update";
                document.getElementById("btnClear").textContent = "Close";
            }
        }
        else {
            const divtesting = document.getElementById("divtesting");
            if (mode === "view") {
                divtesting.classList.add("disabled-div");
            } else {
                divtesting.classList.remove("disabled-div");
            }
        }
    }
    catch (err) {
        console.error("Error initializing page:", err);
        //alert("Failed to load page data.");
    }
}

function disableDownloadLinks() {
    const downloadIds = [
        "iconicfiledownload",
        "iconpassportfiledownload",
        "iconprofessionaldocdownload",
        "iconresumedownload",
        "iconothercertificatesdownload",
        "iconacademicCertificatedownload"
    ];

    downloadIds.forEach(id => {
        const link = document.getElementById(id);
        if (link) {
            link.addEventListener("click", function (e) {
                e.preventDefault();
                return false;
            });

            // optional disabled styling
            link.style.pointerEvents = "none";
            link.style.opacity = "0.5";
            link.style.cursor = "not-allowed";
        }
    });
}

function licensetypeDisable() {
    document.getElementById("License").disabled = true; /*licensetype*/
    document.getElementById("vehicleNo1").disabled = true; /*vehicleno*/
    document.getElementById("vehicleNo2").disabled = true; /*vehicleno2*/
    document.getElementById("vehicleNo3").disabled = true; /*vehicleno3*/
}

function setupRadioDisableList() {
    setupRadioDisable("cpmHandPhone", "cpmHandphoneIssuedDate"); /*handphone hpdate*/
    setupRadioDisable("cpmLaptop", "cpmLaptopIssuedDate"); /*laptop laptopdate*/
    setupRadioDisable("cpmTablet", "cpmTabletIssuedDate"); /*tablet tabletdate*/
    setupRadioDisable("cpmSimcard", "cpmSimcardIssuedDate"); /*companyphone companyphonedate*/
    setupRadioDisable("cpmWalkieTalkie", "cpmWalkieTalkieIssuedDate"); /*walkietoki walkitokidate*/
    setupRadioDisable("cpmInternetAccess", "cpmInternetAccessIssuedDate");/* internet netaccessdate*/

    //setupRadioDisable("systemaccess", "systemaccessdate");
    //setupRadioDisable("folderaccess", "folderaccessdate");

}

function AccessDefaultRadibutton() {
    document.querySelector("input[name='cpmHandPhone'][value='N']").checked = true;/* handphone*/
    document.getElementById("cpmHandphoneIssuedDate").disabled = true; /*hpdate*/

    document.querySelector("input[name='cpmLaptop'][value='N']").checked = true; /*laptop*/
    document.getElementById("cpmLaptopIssuedDate").disabled = true;/* laptopdate*/

    document.querySelector("input[name='cpmTablet'][value='N']").checked = true; /*tablet*/
    document.getElementById("cpmTabletIssuedDate").disabled = true; /*tabletdate*/

    document.querySelector("input[name='cpmSimcard'][value='N']").checked = true; /*companyphone*/
    document.getElementById("cpmSimcardIssuedDate").disabled = true; /*companyphonedate*/

    document.querySelector("input[name='cpmWalkieTalkie'][value='N']").checked = true; /*walkietoki*/
    document.getElementById("cpmWalkieTalkieIssuedDate").disabled = true; /*walkitokidate*/

    document.querySelector("input[name='cpmInternetAccess'][value='N']").checked = true; /*internet*/
    document.getElementById("cpmInternetAccessIssuedDate").disabled = true; /*netaccessdate*/

    //document.querySelector("input[name='systemaccess'][value='N']").checked = true;
    //document.getElementById("systemaccessdate").disabled = true;

    //document.querySelector("input[name='folderaccess'][value='N']").checked = true;
    //document.getElementById("folderaccessdate").disabled = true;

    //document.querySelector("input[name='handphone'][value='N']").checked = true;
    //document.getElementById("hpdate").disabled = true;

}

function setupSpouseWorkingToggle() {
    try {
        var noOfChildren = document.getElementById("noofchildren");
        var spouseWorkingRadios = document.querySelectorAll("input[name='isspouseworking']");

        spouseWorkingRadios.forEach(function (radio) {
            radio.addEventListener("change", function () {
                if (this.value.toUpperCase() === "Y") {
                    noOfChildren.disabled = false; // Enable textbox
                } else {
                    noOfChildren.disabled = true; // Disable textbox
                }
                // Do NOT clear the value
            });
        });
    } catch (ex) {
        console.error("Error in setupSpouseWorkingToggle:", ex);
    }
}

// 34-Textboxs ---> ALL CONTROLLERS LIST
const textboxControllers = ['empname', 'contract', 'newicno',  'passportNo', 'email', 'pphone', 'spousename', 'noofchildren',
    'address1', 'address2', 'typehouseNo', 'vehicleno', 'vehicleno2', 'vehicleno3', 'accountno', 'epf', 'sosco', 'emergencycontactpersonnumber',
    'emergencycontactpersonname', 'taxno', 'EmergencyAddress', 'fieldstudy', 'others', 'jackqty', 'safetybeltqty', 'pantqty', 'earmuffqty', 'tshirtqty',
    'helmetqty', 'shoeqty', 'apronqty', 'capqty'];

// 23- Dropdowns
const dropdownControllers = ['departmentcode', 'sectioncode', 'designation', 'emptype', 'nationality', 'bloodgroup', 'religion', 'race', 'maritalstatus', 'hostelname',
    'houseno', 'route', 'License', 'bank', 'edulevel', 'languagespeaking', 'proficiencylevel', 'jackno', 'tno', 'pantsize',
    'shoeno', 'capcolor', 'safetybeltsize'];

// 13- Radiobuttons
const radioControllers = ['foreignemp', 'sex', 'stayinhostel', 'transportneeded', 'drivinglicense', 'handphone', 'laptop', 'tablet', 'companyphone', 'walkietoki',
    'internet', 'systemaccess', 'folderaccess'];

// 13-Date Calenders
const dateInputs = ['dateofjoin', 'dateofbirth', 'uniformdate', 'hpdate', 'laptopdate', 'tabletdate', 'companyphonedate', 'walkitokidate', 'netaccessdate', 'systemaccessdate',
    'folderaccessdate', 'windowlogindate', 'companyemaildate'];

// 7-File Uploads
const uploadesInputs = ['photo', 'icfileuploadurl', 'passportfileurl', 'academicCertificateurl', 'othercertificatesurl', 'resume', 'professionaldocurl'];

// 1-Checkbox
const checkboxInputs = ['sameaspermanent'];

// 2. CLEAR / RESET ALL CONTROLS
function clearForm() {
    // Textboxes
    textboxControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.value = "";
            el.classList.remove("highlight-error");
        }
    });

    // Dropdowns
    dropdownControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            // Set the dropdown value to the default option value, e.g. "0"
            el.value = "0";
            el.classList.remove("highlight-error");
        }
    });


    // Radio buttons
    radioControllers.forEach(name => {
        const radios = document.getElementsByName(name);
        radios.forEach(r => r.checked = false);
    });
    // Then set defaults
    resetAllRadios();

    // Dates
    dateInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.value = "";
            el.classList.remove("highlight-error");
        }
    });

    console.log("Form cleared.");
}

// e.g. for 'foreignemp' default to 'N', for 'sex' default to 'M'// Function to reset all radios, then set defaults for certain groups if you like
const radioDefaultValues = {
    sex: 'M',
    spouseworking: 'N',
    drivinglisence: 'N',
    emptransferring: 'N',
    wokitoki: 'N',
    tab: 'N',
    foreignemp: 'N',
    stayinhostel: 'N',
    transportneeded: 'N',
    hppass: 'N',
    carpass: 'N',
    computerpass: 'N',
    remotelogin: 'N'
};

function resetAllRadios() {
    radioControllers.forEach(function (groupName) {
        const radios = document.getElementsByName(groupName);
        if (!radios || radios.length === 0) {
            // No radio buttons with that name — skip
            return;
        }

        // First clear all
        for (let i = 0; i < radios.length; i++) {
            radios[i].checked = false;
        }

        // Then set default if specified
        const defaultVal = radioDefaultValues[groupName];
        if (defaultVal !== undefined) {
            for (let i = 0; i < radios.length; i++) {
                if (radios[i].value === defaultVal) {
                    radios[i].checked = true;
                    break;
                }
            }
        }
    });
}

// 3. VALIDATION FUNCTION BEFORE SAVE
function validateFormBeforeSave() {
    let isValid = true;

    // Reset old highlights
    document.querySelectorAll(".highlight-error").forEach(el => el.classList.remove("highlight-error"));

    // Validate textboxes
    textboxControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el && el.value.trim() === "") {
            el.classList.add("highlight-error");
            isValid = false;
        }
        else {
            console.log("have a data with controller ID: ", id);
        }
    });

    // Validate dropdowns
    dropdownControllers.forEach(id => {
        const el = document.getElementById(id);
        if (el && (el.value === "" || el.value === "0" || el.value == null)) {
            el.classList.add("highlight-error");
            isValid = false;
        }
        else {
            console.log("have a data with controller ID: ", id);
        }
    });

    // Validate radio groups
    radioControllers.forEach(name => {
        const radios = document.getElementsByName(name);
        if (radios.length > 0) {
            const anyChecked = [...radios].some(r => r.checked);
            if (!anyChecked) {
                radios[0].parentElement.classList.add("highlight-error");
                isValid = false;
            }
        }
    });

    // Validate date inputs
    dateInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el && el.value.trim() === "") {
            el.classList.add("highlight-error");
            isValid = false;
        }
    });

    return isValid;
}

// Display Current Date and prevent selecting past dates
function setTodayForUniformDate() {
    let today = new Date();
    let day = String(today.getDate()).padStart(2, '0');
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    // Format: yyyy-mm-dd (required for date input)
    let formattedDate = `${year}-${month}-${day}`;

    let uniformDateInput = document.getElementById("uniformdate");
    if (uniformDateInput) {
        uniformDateInput.value = formattedDate;  // Set default to today's date
        uniformDateInput.setAttribute("min", formattedDate);  // Prevent past dates
    }
}

// Display Current Date
function setCurrentDateForInputs() {
    let today = new Date();
    let day = String(today.getDate()).padStart(2, '0');
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    let formattedDate = `${year}-${month}-${day}`; // yyyy-mm-dd

    let dateOfJoinInput = document.getElementById("dateofjoin");
    let resignedInput = document.getElementById("resigned");

    if (dateOfJoinInput) dateOfJoinInput.value = formattedDate;
    if (resignedInput) resignedInput.value = formattedDate;
}

// Display Past Date
function restrictDOB() {
    let today = new Date();

    // Calculate yesterday
    today.setDate(today.getDate() - 1);

    let day = String(today.getDate()).padStart(2, '0');
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    // Format required for input type="date": yyyy-mm-dd
    let maxDate = `${year}-${month}-${day}`;

    let dobInput = document.getElementById("dateofbirth");
    if (dobInput) {
        dobInput.setAttribute("max", maxDate);
    }
}

// Email Listen to input event
function setupEmailValidation() {
    const emailInput = document.getElementById('email');
    if (!emailInput) return; // safety check
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    emailInput.addEventListener('input', () => {
        const emailValue = emailInput.value;

        if (emailValue === "" || emailRegex.test(emailValue)) {
            // Valid email or empty → remove highlight
            emailInput.style.borderColor = "";
            emailInput.style.backgroundColor = "";
        } else {
            // Invalid email → highlight input
            emailInput.style.borderColor = "red";
            emailInput.style.backgroundColor = "#ffe6e6"; // light red
        }
    });
}

// Highlight Invalid Input of imge
function validateImageInput() {
    const imgInput = document.getElementById('photo');
    if (!imgInput) return;

    const allowedTypes = ['image/jpeg', 'image/png', 'image/jpg'];

    imgInput.addEventListener('change', () => {
        const file = imgInput.files[0];
        if (!file) {
            // No file selected → remove highlight
            imgInput.style.borderColor = "";
            imgInput.style.backgroundColor = "";
            return;
        }

        if (!allowedTypes.includes(file.type)) {
            // Invalid file → highlight input
            imgInput.style.borderColor = "red";
            imgInput.style.backgroundColor = "#ffe6e6"; // light red
            imgInput.value = ""; // clear the invalid file
        } else {
            // Valid file → remove highlight
            imgInput.style.borderColor = "";
            imgInput.style.backgroundColor = "";
        }
    });
}

function validateFiles() {
    const allowedTypes = [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif"
    ];

    document.querySelectorAll(".file-validate").forEach(input => {
        input.addEventListener("change", function () {
            const file = this.files[0];

            if (file && !allowedTypes.includes(file.type)) {
                alert("Invalid file type! Allowed types: PDF, DOC, DOCX, JPG, PNG, GIF.");
                this.value = "";  // Reset invalid file
            }
        });
    });
}

document.getElementById("maritalStatus").addEventListener("change", function (e) {
    var val = this.value;

    var spouseName = document.getElementById("spouseName"); /*spousename*/
    var noOfChildren = document.getElementById("noOfChildren"); /*noofchildren*/
    var spouseWorkingRadios = document.querySelectorAll("input[name='spouseWorking']"); /*isspouseworking*/

    if (val === "M" || val ==="MARRIED") {
        // Enable when Married
        spouseName.disabled = false;
        noOfChildren.disabled = false;

        spouseWorkingRadios.forEach(function (r) {
            r.disabled = false;
        });
    } else {
        // Disable for other options
        spouseName.value = "";
        spouseName.disabled = true;

        noOfChildren.value = "";
        noOfChildren.disabled = true;

        spouseWorkingRadios.forEach(function (r) {
            r.checked = false;
            r.disabled = true;
        });
    }
});

async function fetchDropdownData() {
    try {
        const response = await fetch('/EmpMaster/GetAlldropdownData');

        // Check for unauthorized
        if (response.status === 401) {
            throw new Error("Unauthorized");
        }

        if (!response.ok) {
            throw new Error(response.statusText);
        }

        const data = await response.json();
        return data; // Return data to caller
    } catch (err) {
        console.error("Fetch error:", err);
        throw err; // Rethrow so DefaultPageLoadBind can catch
    }
}

// This function handles enabling/disabling the IC/Passport doc field
function setupForeignEmpRadioHandler() {
    // Get all radio buttons for 'foreignemp'
    var radios = document.querySelectorAll("input[name='foreignemp']");
    // Local Employee
    var newicno = document.getElementById("newicno");
    var docInput = document.getElementById("icfileuploadurl");
    // Foregin Employee
    var passportno = document.getElementById("passportNo");
    var passportfileurl = document.getElementById("passportfileurl");

    // If radios or docInput not found, do nothing
    if (!radios || !docInput) return;

    radios.forEach(function (radio) {
        radio.addEventListener("change", function (e) {
            var val = e.target.value;
            if (val === "N") {
                // If No → enable the file input
                docInput.disabled = false;
                newicno.disabled = false;
                passportno.disabled = true;
                passportfileurl.disabled = true;
                docInput.value = "";
                docInput.value = "";
                passportno.value = "";
            } else if (val === "Y") {
                // Otherwise → disable and clear
                passportno.disabled = false;
                passportfileurl.disabled = false;
                docInput.disabled = true;
                newicno.disabled = true;
                docInput.value = "";
                docInput.value = "";
                passportno.value = "";
            }
        });
    });

    // Trigger once on setup to reflect any pre-selected radio
    var checked = document.querySelector("input[name='foreignemp']:checked");
    if (checked) {
        checked.dispatchEvent(new Event("change"));
    }
}

// Function to bind dropdowns based on the fetched data
function bindDropdowns(data) {

    // Sort data by description in ascending order (alphabetical)
    data.sort(function (a, b) { return a.description.localeCompare(b.description); }); // Sorting by description

    // Iterate through the data and populate the respective dropdowns
    data.forEach(function (item) {
        switch (item.tableName) {
            case "tblarea":
                bindDropdown('#area', item);
                break;
            case "tbllanguageproficiency":
                bindDropdown('#proficiencyLevel', item); /*proficiencylevel*/
                break;
            case "tblmaritalstatus":
                bindmaritalstatusDropdown('#maritalStatus', item); /*maritalstatus*/
                break;
            case "tbreligion":
                bindDropdown('#religion', item);
                break;
            case "tblrelation":
                bindDropdown('#relation', item);
                break;
            case "tblcap":
                bindDropdown('#capColor', item); /*capcolor*/
                break;
            //case "tblemployeetype":
            //    bindEmpTypeDropdown('#empType', item);
            //    break;
            //case "tblclothsize":
            //    //bindDropdown('#jacketSize', item); /*jackno*/
            //    //bindDropdown('#tShirtSize', item); /*tno*/
            //    //bindDropdown('#pantSize', item); /*pantsize*/
            //    break;
            //case "tblbeltsize":
            //    bindDropdown('#safetyBeltSize', item); /*safetybeltsize*/
            //    break;
            //case "tblshoessize":
            //    bindDropdown('#safetyShoes', item); /*shoeno*/
            //    break;
            //case "tblbanknames":
            //    bindDropdown('#bank', item);
            //    break;
            //case "tblcategory":
            //    bindDropdown('#category', item);
            //    break;
            //case "tbldepartment":
            //    bindDropdown('#departmentcode', item);
            //    break;
            //case "tbldesignation":
            //    binddesignationDropdown('#designation', item);
            //    break;
            //case "tbleducation":
            //    bindDropdown('#edulevel', item);
            //    break;
            //case "tblhostelname":
            //    bindDropdown('#hostelname', item);
            //    break;
            //case "tblrace":
            //    bindDropdown('#race', item);
            //    break;
            //case "tblroute":
            //    bindrouteDropdown('#route', item);
            //    break;
            //case "tbnationality":
            //    bindDropdown('#nationality', item);
            //    break;
            //case "tbllanguage":
            //    bindDropdown('#languagespeaking', item);
            //    break;
            //case "tblbloodgroup":
            //    bindDropdown('#bloodgroup', item);
            //    break;
            default:
                console.warn('Unknown TableName: ' + item.tableName);
        }
    });
}

function bindrouteDropdown(dropdownSelector, item) {
    var dropdown = $(dropdownSelector);
    // Check if the dropdownSelector is "departmentcode"
    if (dropdownSelector === "#route") {
        // If it's departmentcode, display item.code - item.description
        dropdown.append('<option value="' + item.description + '">' + item.code + ' - ' + item.description + '</option>');
    } else {
        // For all other dropdowns, just display item.description
        dropdown.append('<option value="' + item.description + '">' + item.description + '</option>');
    }
}

function bindmaritalstatusDropdown(dropdownSelector, item) {
    var dropdown = $(dropdownSelector);
    // Check if the dropdownSelector is "departmentcode"
    if (dropdownSelector === "#maritalstatus") {
        // If it's departmentcode, display item.code - item.description
        dropdown.append('<option value="' + item.description + '">' + item.code + ' - ' + item.description + '</option>');
    } else {
        // For all other dropdowns, just display item.description
        dropdown.append('<option value="' + item.description + '">' + item.description + '</option>');
    }
}

function binddesignationDropdown(dropdownSelector, item) {
    var dropdown = $(dropdownSelector);
    // Check if the dropdownSelector is "departmentcode"
    if (dropdownSelector === "#designation") {
        // If it's departmentcode, display item.code - item.description
        dropdown.append('<option value="' + item.description + '">' + item.code + ' - ' + item.description + '</option>');
    } else {
        // For all other dropdowns, just display item.description
        dropdown.append('<option value="' + item.description + '">' + item.description + '</option>');
    }
}

function bindEmpTypeDropdown(dropdownSelector, item) {
    var dropdown = $(dropdownSelector);
    // Check if the dropdownSelector is "departmentcode"
    if (dropdownSelector === "#empType") {
        // If it's departmentcode, display item.code - item.description
        dropdown.append('<option value="' + item.description + '">' + item.code + ' - ' + item.description + '</option>');
    } else {
        // For all other dropdowns, just display item.description
        dropdown.append('<option value="' + item.description + '">' + item.description + '</option>');
    }
}

// Helper function to bind data to a specific dropdown
function bindDropdown(dropdownSelector, item) {
    var dropdown = $(dropdownSelector);

    // Check if the dropdownSelector is "departmentcode"
    if (dropdownSelector === "#departmentcode") {
        // If it's departmentcode, display item.code - item.description
        dropdown.append('<option value="' + item.code + '">' + item.code + ' - ' + item.description + '</option>');
    } else {
        // For all other dropdowns, just display item.description
        dropdown.append('<option value="' + item.code + '">' + item.description + '</option>');
    }
}

// New function to fetch checkbox dropdown data
async function fetchCheckboxDropdownData() {
    try {
        const response = await fetch('/EmpMaster/GetLicenseTypes');
        if (!response.ok) throw new Error('Network response was not ok');

        const data = await response.json();
        bindCheckboxDropdown('#License', data.records); /*licensetype*/
    } catch (err) {
        console.error(err);
        alert(err);
    }
}

function bindCheckboxDropdown_Old(containerId, records) {
    let html = '';
    records.forEach(item => {
        html += `
            <li class="dropdown-item">
                <input type="checkbox" class="cat-check me-2" value="${item.value}" />
                ${item.text}
            </li>`;
    });
    $(containerId).html(html);
}

function bindCheckboxDropdown(containerId, records) {
    let html = '';
    records.forEach(item => {
        html += `
            <li class="dropdown-item">
                <input type="checkbox"
                       class="cat-check me-2"
                       value="${item.value}"
                       data-text="${item.text}" />
                ${item.text}
            </li>`;
    });
    $(containerId).html(html);
}

$(document).on('change', '.cat-check', function () {
    updateSelectedLicenseText();
});

function updateSelectedLicenseText() {
    let selectedTexts = [];

    $('.cat-check:checked').each(function () {
        selectedTexts.push($(this).data('text'));
    });

    let displayText = selectedTexts.length > 0
        ? selectedTexts.join(', ')
        : '--- select License Type ---';

    $('#divlicensetype button').text(displayText);
}

//// triggers when departmentcode is selected
function onDepartmentClick() {
    var departmentCode = $('#department').val(); // get selected value
    if (departmentCode) {
        fetchSubDepartments(departmentCode); // call your existing function
    } else {
        $('#subDepartment').empty().append('<option>--Select Section--</option>');
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
    sectionDropdown.append('<option>--Select Sub Dept--</option>');  // Default option

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
$('#subDepartment').on('change', function () {
    var SubDeptCode = $(this).val();  // Get the selected department code
    if (SubDeptCode) {
        fetchGetSubDeptSections(SubDeptCode);
    } else {
        $('#section').empty().append('<option>--Select Section--</option>');
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

// Function to make AJAX call to fetch sections
function fetchSections(departmentCode) {
    $.ajax({
        url: '/EmpMaster/GetSections',  // Controller method to fetch sections
        type: 'GET',
        data: { departmentCode: departmentCode },
        success: function (data) {
            // Call the function to bind sections to the dropdown
            bindSectionsToDropdown(data);
        },
        error: function (err) {
            console.error('Error fetching sections:', err);
            showErrorModal(err);
        }
    });
}

// Function to bind sections to the dropdown
function bindSectionsToDropdown(sections) {
    var sectionDropdown = $('#sectioncode');
    sectionDropdown.empty();  // Clear existing options
    sectionDropdown.append('<option>--Select Section--</option>');  // Default option

    // Sort sections in ascending order by section name
    sections.sort(function (a, b) {
        return a.sectionname.localeCompare(b.sectionname); // Sort by sectionname
    });

    // Append each section as an option
    sections.forEach(function (section) {
        sectionDropdown.append(
            '<option value="' + section.sectioncode + '">' + section.sectioncode + ' - ' + section.sectionname + '</option>'
        );
    });
}

// Account Number Validation
function validateAccountNo(input) {
    // Remove non-digits
    let value = input.value.replace(/\D/g, '');

    // Limit max length to 12
    if (value.length > 12) {
        value = value.slice(0, 12);
    }
    input.value = value;
}

// Function to enable/disable textarea based on checkbox
const permanentAddress = document.getElementById("permanentAddress");
const mailingAddress = document.getElementById("currentAddress");
const sameAsCheckbox = document.getElementById("sameaspermanent");
sameAsCheckbox.addEventListener("change", function () {
    if (this.checked) {
        mailingAddress.value = permanentAddress.value;
        mailingAddress.setAttribute("readonly", true);
    } else {
        mailingAddress.value = "";
        mailingAddress.removeAttribute("readonly");
    }
});
// Keep syncing while typing
permanentAddress.addEventListener("input", function () {
    if (sameAsCheckbox.checked) {
        mailingAddress.value = this.value;
    }
});

function loadNextEmpCode() {
    $.ajax({
        url: '/EmpMaster/GetLastEmpCode',
        type: 'GET',
        success: function (data) {
            if (data && data.lastEmpCode != null) {

                var nextNum = parseInt(data.lastEmpCode, 10) + 1;

                // ALWAYS force 6 digits
                var nextCode = String(nextNum).padStart(6, '0');

                $('#empcode').text(nextCode);
            } else {
                showErrorModal('GetLastEmpCode returned no data');
            }
        },
        error: function () {
            showErrorModal('Error fetching last emp code');
        }
    });
}

// Radio button function, if No then disable Date textbox
function setupRadioDisable(radioName, dateFieldId) {
    const radios = document.querySelectorAll(`input[name='${radioName}']`);
    const dateField = document.getElementById(dateFieldId);

    radios.forEach(radio => {
        radio.addEventListener("change", function () {
            dateField.disabled = (this.value === "N");
            if (this.value === "N") dateField.value = ""; // clear date
        });
    });
}

// Staying Hostel  // Called when YES/NO radio button is clicked
function onHostelRadioClick(isYes) {
    const hostelname = document.getElementById("hostelName"); /*hostelname*/
    const houseNo = document.getElementById("houseNo"); /*houseno*/
    const typeHouseNo = document.getElementById("typehouseNo"); /*typehouseNo*/

    if (!hostelname || !houseNo || !typeHouseNo) return;

    if (isYes) {
        // Show inputs
        //hostelname.style.display = "block";
        //houseNo.style.display = "block";
        //typeHouseNo.style.display = "none";

        // Enable inputs
        hostelname.disabled = false;
        houseNo.disabled = false;
        typeHouseNo.disabled = true;

    } else {
        // Hide typeHouseNo
        //typeHouseNo.style.display = "none";

        // Disable all inputs
        hostelname.disabled = true;
        houseNo.disabled = true;
        typeHouseNo.disabled = true;

        // Reset dropdowns to default value
        hostelname.selectedIndex = 0; // or use: hostelname.value = "";
        houseNo.selectedIndex = 0;    // or use: houseNo.value = "";
        typeHouseNo.value = "";       // if it is a textbox
    }
}
function hideHostelControlsByDefault() {
    const hostelName = document.getElementById("hostelName"); /*hostelname*/
    const houseNo = document.getElementById("houseNo"); /*houseno*/
    const typeHouseNo = document.getElementById("typehouseNo");/* typehouseNo*/
    if (!hostelName || !houseNo || !typeHouseNo) return;
    typeHouseNo.style.display = "none";
    hostelName.disabled = true;
    houseNo.disabled = true;
    typeHouseNo.disabled = true;
}

//License Type YES/NO → Enable/Disable Vehicle Fields
function setupDrivingLicenseControls(radioName, fieldsToToggle) {
    try {
        const radios = document.querySelectorAll(`input[name='${radioName}']`);

        // Function to enable/disable fields
        const toggleFields = (enable) => {
            fieldsToToggle.forEach(fieldId => {
                const field = document.getElementById(fieldId);
                if (!field) return;

                // Check if it contains a dropdown
                const button = field.querySelector("button");
                if (button) {
                    button.disabled = !enable;       // Disable dropdown button
                    field.style.opacity = enable ? "1" : "1"; // Gray out---> "1","0.5"
                } else {
                    field.disabled = !enable;       // Normal input fields
                }
            });
        };

        // Disable all fields by default
        toggleFields(false);

        // Enable fields based on radio selection
        radios.forEach(radio => {
            radio.addEventListener("change", function () {
                toggleFields(this.value === "Y");
            });
        });
    } catch (ex) {
        console.error("Error in setupDrivingLicenseControls:", ex);
    }
}

function setupTransportNeededControls(radioName, fieldId) {
    const radios = document.querySelectorAll(`input[name='${radioName}']`);
    radios.forEach(radio => {
        radio.addEventListener("change", function () {
            const field = document.getElementById(fieldId);
            if (!field) return;

            const enable = (this.value === "Y");
            field.disabled = !enable;

            if (!enable) field.value = "";
        });
    });
}

// Function to fetch house numbers based on the selected hostel
function fetchHouseNumbers(housecode) {
    const houseNo = document.getElementById("houseNo"); /*houseno*/
    const typeHouseNo = document.getElementById("typehouseNo");
    if (!houseNo || !typeHouseNo) return;

    if (housecode === "TR") {
        houseNo.style.display = "none";
        houseNo.disabled = true;

        typeHouseNo.style.display = "block";
        typeHouseNo.disabled = false;
    }

    if (housecode === "PH") {
        typeHouseNo.style.display = "none";
        typeHouseNo.disabled = true;
        typeHouseNo.value = "";

        houseNo.style.display = "block";
        houseNo.disabled = false;

        if (houseNo.options.length > 1) {
            return;
        }
        // 🔹 Call API only if data NOT available
        fetch(`/EmpMaster/GetHouseNumbers?housecode=${housecode}`)
            .then(response => response.json())
            .then(data => {
                houseNo.innerHTML = "<option value=''>--- Select House No ---</option>";
                if (data && data.length > 0) {
                    data.forEach(item => {
                        const option = document.createElement("option");
                        option.value = item.housenumber;
                        option.textContent = item.housenumber;
                        houseNo.appendChild(option);
                    });
                }
            })
            .catch(error => {
                console.error("Error fetching house numbers:", error);
            });
    }
}

// Vehicle Number validation (Allow only string + number)
function validateVehicleNo(input) {
    input.value = input.value.replace(/[^a-zA-Z0-9]/g, "");

    const value = input.value;

    const hasLetter = /[a-zA-Z]/.test(value);
    const hasNumber = /[0-9]/.test(value);
    const validLength = value.length >= 3 && value.length <= 7;

    // Apply validation UI
    if (value.length > 0 && (!hasLetter || !hasNumber || !validLength)) {
        input.classList.add("is-invalid");
    } else {
        input.classList.remove("is-invalid");
    }
}
function validateTaxNo(input) {
    input.value = input.value.replace(/[^a-zA-Z0-9]/g, "");

    const value = input.value;

    const hasLetter = /[a-zA-Z]/.test(value);
    const hasNumber = /[0-9]/.test(value);
    const validLength = value.length >= 9 && value.length <= 14;

    // Apply validation UI
    if (value.length > 0 && (!hasLetter || !hasNumber || !validLength)) {
        input.classList.add("is-invalid");
    } else {
        input.classList.remove("is-invalid");
    }
}

// EPF number validation and allow only 8-digit
function validateEPFInput(input) {
    // Allow only numbers
    input.value = input.value.replace(/[^0-9]/g, "");

    // Enforce max length = 8
    if (input.value.length > 8) {
        input.value = input.value.slice(0, 8);
    }

    // Optional UI feedback
    if (input.value.length > 0 && input.value.length !== 8) {
        input.classList.add("is-invalid");
    } else {
        input.classList.remove("is-invalid");
    }
}

// Called on every key press in SOSCO
function handleSOSCOInput(input) {
    // Step 1: Format SOSCO
    formatSOSCO(input);

    // Step 2: Validate SOSCO if full length entered
    if (input.value.length === 14) {
        validateSOSCOOnSubmit(false); // false = no alert
        validateICAndSOSCO();         // check against newicno
    } else {
        input.classList.remove("is-invalid");
        document.getElementById("newicno").classList.remove("is-invalid");
    }
}

// Format SOSCO as 000000-00-0000 while typing
function formatSOSCO(input) {
    // Remove anything that is not a letter or number
    let alphanumeric = input.value.replace(/[^A-Za-z0-9]/g, "");
    // Limit to maximum 12 characters
    alphanumeric = alphanumeric.substring(0, 12);

    let formatted = "";
    if (alphanumeric.length >= 1) formatted = alphanumeric.substring(0, 6);
    if (alphanumeric.length > 6) formatted += "-" + alphanumeric.substring(6, 8);
    if (alphanumeric.length > 8) formatted += "-" + alphanumeric.substring(8, 12);

    input.value = formatted;
}

// Validate SOSCO format
function validateSOSCOOnSubmit(showAlert = true) {
    const sosco = document.getElementById("sosco");
    //const isValid = /^\d{6}-\d{2}-\d{4}$/.test(sosco.value);
    // Allow only letters (A-Z, a-z) and numbers (0-9) in the same format
    const isValid = /^[A-Za-z0-9]{6}-[A-Za-z0-9]{2}-[A-Za-z0-9]{4}$/.test(sosco.value);


    if (!isValid) {
        sosco.classList.add("is-invalid");
        if (showAlert) alert("SOSCO must be exactly 12 digits (000000-00-0000).");
        return false;
    }

    sosco.classList.remove("is-invalid");
    return true;
}

// Validate IC number and SOSCO are the same (ignoring hyphens)
function validateICAndSOSCO() {
    const ic = document.getElementById("newicno").value.replace(/[^0-9]/g, ""); // remove non-digits
    const sosco = document.getElementById("sosco").value.replace(/-/g, "");      // remove hyphens

    // Only validate if both fields have values
    if (!ic || !sosco) {
        // Remove any previous invalid state
        document.getElementById("sosco").classList.remove("is-invalid");
        document.getElementById("newicno").classList.remove("is-invalid");
        return true; // skip validation
    }

    if (ic !== sosco) {
        document.getElementById("sosco").classList.add("is-invalid");
        document.getElementById("newicno").classList.add("is-invalid");
        alert("SOSCO number must match IC number.");
        return false;
    }

    // If valid, remove invalid state
    document.getElementById("sosco").classList.remove("is-invalid");
    document.getElementById("newicno").classList.remove("is-invalid");
    return true;
}

// Called on typing in IC number
function handleICInput(input) {
    formatIC(input);
    if (input.value.length === 14) {
    if (document.getElementById("sosco").value.length === 14) {
        validateICAndSOSCO();
    } else {
        input.classList.remove("is-invalid");
        document.getElementById("sosco").classList.remove("is-invalid");
        }
    }
}

function formatIC(input) {
    let value = input.value.replace(/[^0-9A-Za-z]/g, "").toUpperCase(); // Keep numbers and letters only
    value = value.substring(0, 12);// Max length: 12 characters (YYMMDD + BP + ### + G)
    let formatted = "";
    if (value.length >= 1) formatted = value.substring(0, 6);// YYMMDD
    if (value.length > 6) formatted += "-" + value.substring(6, 8);// BP
    
    if (value.length > 8) formatted += "-" + value.substring(8, 11);// ###
    if (value.length > 11) formatted += value.substring(11, 12);// G
    input.value = formatted;
}

// Tax number validation
function handleTaxNoInput(input) {
    // Remove any special characters (allow only letters and numbers)
    input.value = input.value.replace(/[^a-zA-Z0-9]/g, "");

    // Optional: enforce max length (redundant with maxlength)
    if (input.value.length > 12) {
        input.value = input.value.substring(0, 12);
    }

    // Add validation feedback
    if (input.value.length < 9) {
        input.classList.add("is-invalid");
    } else {
        input.classList.remove("is-invalid");
    }
}

//  SAVE BUTTON CLICK EVENT
function saveEmployee() {
    let div5 = validateUniformAndPPEDetails();
    let div4 = validateEducationDetails();
    let div3 = validateHostelLicencesBankTaxDetails();
    let div2 = validateEmployeeParticular();
    let div1 = validateEmployeeBasic();
    if (!div1 || !div2 || !div3 || !div4 || !div5) {
        alert("Please correct highlighted fields.");
        return false;
    }
    const mode = document.getElementById("hdnMode").value;
    if (mode.toUpperCase() === "NEW" || mode === "") {
        submitEmployee("");
    } else if (mode === "edit") {
        submitEmployee("edit");
    }
}

function submitEmployee(mode) {
    // Create FormData
    let formData = new FormData();
    // ---------- LICENSE ----------
    let licenseValues = [];
    $("#License input[type='checkbox']:checked").each(function () {
        licenseValues.push($(this).val());
    });

    // ---------- EMP MASTER ---------- 49-Columns + 8 + 5 = 62
    //isResigned, isOperator //dateOfService ,leaveLevel ,dateOfTerimination //empCodeI // workingExp  // Type House No

    formData.append("empMaster.empcode", $("#empcode").text());
    formData.append("empMaster.empName", $("#empName").val());
    formData.append("empMaster.dateOfBirth", $("#dateOfBirth").val());
    formData.append("empMaster.gender", $("input[name='gender']:checked").val());
    formData.append("empMaster.dateOfJoin", $("#dateOfJoin").val());

    formData.append("empMaster.contractperiod", $("#contractperiod").val());
    formData.append("empMaster.empType", $("#empType").val());
    formData.append("empMaster.nationality", $("#nationality").val());
    formData.append("empMaster.isForeign", $("input[name='isForeign']:checked").val());
    formData.append("empMaster.bloodGroup", $("#bloodGroup").val());
    formData.append("empMaster.passportNo", $("#passportNo").val());

    formData.append("empMaster.icNo", $("#icNo").val());
    formData.append("empMaster.emailID", $("#emailID").val());
    formData.append("empMaster.mobileNo", $("#mobileNo").val());
    formData.append("empMaster.religion", $("#religion").val());
    formData.append("empMaster.race", $("#race").val());

    formData.append("empMaster.maritalStatus", $("#maritalStatus").val());
    formData.append("empMaster.spouseName", $("#spouseName").val());
    formData.append("empMaster.spouseWorking", $("input[name='spouseWorking']:checked").val());
    formData.append("empMaster.noOfChildren", $("#noOfChildren").val());
    formData.append("empMaster.permanentAddress", $("#permanentAddress").val());

    formData.append("empMaster.currentAddress", $("#currentAddress").val());
    formData.append("empMaster.epf", $("#epf").val());
    formData.append("empMaster.sosco", $("#sosco").val());
    formData.append("empMaster.taxNo", $("#taxNo").val());
    formData.append("empMaster.others", $("#others").val());

    formData.append("empMaster.emergencyContactPerson", $("#emergencyContactPerson").val());
    formData.append("empMaster.emergencyAddress", $("#emergencyAddress").val());
    formData.append("empMaster.emergencyContact", $("#emergencyContact").val());

    formData.append("empMaster.relation", $("#relation").val());
    formData.append("empMaster.department", $("#department").val());
    formData.append("empMaster.subDepartment", $("#subDepartment").val());
    formData.append("empMaster.section", $("#section").val());
    formData.append("empMaster.designation", $("#designation").val());

    formData.append("empMaster.isStayingHostel", $("input[name='isStayingHostel']:checked").val());
    formData.append("empMaster.hostelName", $("#hostelName").val());

    if ($("#hostelName").val() === "PH") {
        formData.append("empMaster.houseNo", $("#houseNo").val());
    }
    else if ($("#hostelName").val() === "TR") {
        formData.append("empMaster.houseNo", $("#typehouseNo").val());
    }

    formData.append("empMaster.isTransport", $("input[name='isTransport']:checked").val());
    formData.append("empMaster.route", $("#route").val());

    formData.append("empMaster.isDrivingLicense", $("input[name='isDrivingLicense']:checked").val());
    formData.append("empMaster.License", licenseValues.join(","));
    formData.append("empMaster.vehicleNo1", $("#vehicleNo1").val());
    formData.append("empMaster.vehicleNo2", $("#vehicleNo2").val());
    formData.append("empMaster.vehicleNo3", $("#vehicleNo3").val());

    formData.append("empMaster.bankName", $("#bankName").val());
    formData.append("empMaster.accountNumber", $("#accountNumber").val());
    formData.append("empMaster.educationLevel", $("#educationLevel").val());
    formData.append("empMaster.languageProficiency", $("#languageProficiency").val());
    formData.append("empMaster.proficiencyLevel", $("#proficiencyLevel").val());

    formData.append("empMaster.fieldOfStudy", $("#fieldOfStudy").val());

    // ---------- FILES ---------- 6-Columns
    if ($("#photo")[0].files.length > 0) {
        formData.append("photo", $("#photo")[0].files[0]);
    }

    if ($("#professionaldocurl")[0].files.length > 0)
        formData.append("professionaldocurl", $("#professionaldocurl")[0].files[0]);
    if ($("#resumeDocURL")[0].files.length > 0)
        formData.append("resumeDocURL", $("#resumeDocURL")[0].files[0]);
    if ($("#academicCertificateUrl")[0].files.length > 0)
        formData.append("academicCertificateUrl", $("#academicCertificateUrl")[0].files[0]);
    if ($("#otherCertificateUrl")[0].files.length > 0)
        formData.append("otherCertificateUrl", $("#otherCertificateUrl")[0].files[0]);
    if ($("#icFilePath")[0].files.length > 0)
        formData.append("icFilePath", $("#icFilePath")[0].files[0]);
    if ($("#passportfileurl")[0].files.length > 0)
        formData.append("passportfileurl", $("#passportfileurl")[0].files[0]);

    // ---------- UNIFORM ---------- 16-Colimns
    formData.append("uniformInfo.empcode", $("#empcode").text());

    let jacketQty = $("#jacketqty").val();
    let jacketSize = "";
    jacketQty = jacketQty ? parseInt(jacketQty, 10) : 0;// Normalize qty
    if (jacketQty > 0)  // If qty > 0, include size
        jacketSize = $("#jacketSize").val();
    formData.append("uniformInfo.jacketqty", jacketQty);
    formData.append("uniformInfo.jacketSize", jacketSize);

    let pantqty = $("#pantqty").val();
    let pantSize = "";
    pantqty = pantqty ? parseInt(pantqty, 10) : 0;
    if (pantqty > 0)
        pantSize = $("#pantSize").val();
    formData.append("uniformInfo.pantqty", pantqty);
    formData.append("uniformInfo.pantSize", pantSize);

    let tShirtqty = $("#tShirtqty").val();
    let tShirtSize = "";
    tShirtqty = tShirtqty ? parseInt(tShirtqty, 10) : 0;
    if (tShirtqty > 0)
        tShirtSize = $("#tShirtSize").val();
    formData.append("uniformInfo.tShirtqty", tShirtqty);
    formData.append("uniformInfo.tShirtSize", tShirtSize);

    let safetyShoesqty = $("#safetyShoesqty").val();
    let safetyShoes = "";
    safetyShoesqty = safetyShoesqty ? parseInt(safetyShoesqty, 10) : 0;
    if (safetyShoesqty > 0)
        safetyShoes = $("#safetyShoes").val();
    formData.append("uniformInfo.safetyShoesqty", safetyShoesqty);
    formData.append("uniformInfo.safetyShoes", safetyShoes);

    let capColorqty = $("#capColorqty").val();
    let capColor = "";
    capColorqty = capColorqty ? parseInt(capColorqty, 10) : 0;
    if (capColorqty > 0)
        capColor = $("#capColor").val();
    formData.append("uniformInfo.capColorqty", capColorqty);
    formData.append("uniformInfo.capColor", capColor);

    let safetyBeltqty = $("#safetyBeltqty").val();
    let safetyBeltSize = "";
    safetyBeltqty = safetyBeltqty ? parseInt(safetyBeltqty, 10) : 0;
    if (safetyBeltqty > 0)
        safetyBeltSize = $("#safetyBeltSize").val();
    formData.append("uniformInfo.safetyBeltqty", safetyBeltqty);
    formData.append("uniformInfo.safetyBeltSize", safetyBeltSize);

    formData.append("uniformInfo.earMuffqty", $("#earMuffqty").val());
    formData.append("uniformInfo.safetyHelmetqty", $("#safetyHelmetqty").val());
    formData.append("uniformInfo.apronQty", $("#apronQty").val());

    formData.append("uniformInfo.dateOfIssue", $("#dateOfIssue").val());

    // ---------- ACCESSORIES ---------- 17-Columns
    formData.append("accessoriesInfo.empcode", $("#empcode").text());

    formData.append("accessoriesInfo.cpmHandPhone", $("input[name='cpmHandPhone']:checked").val());
    formData.append("accessoriesInfo.cpmHandphoneIssuedDate", $("#cpmHandphoneIssuedDate").val());
    formData.append("accessoriesInfo.cpmLaptop", $("input[name='cpmLaptop']:checked").val());
    formData.append("accessoriesInfo.cpmLaptopIssuedDate", $("#cpmLaptopIssuedDate").val());
    formData.append("accessoriesInfo.cpmTablet", $("input[name='cpmTablet']:checked").val());

    formData.append("accessoriesInfo.cpmTabletIssuedDate", $("#cpmTabletIssuedDate").val());
    formData.append("accessoriesInfo.cpmSimcard", $("input[name='cpmSimcard']:checked").val());
    formData.append("accessoriesInfo.cpmSimcardIssuedDate", $("#cpmSimcardIssuedDate").val());
    formData.append("accessoriesInfo.cpmWalkieTalkie", $("input[name='cpmWalkieTalkie']:checked").val());
    formData.append("accessoriesInfo.cpmWalkieTalkieIssuedDate", $("#cpmWalkieTalkieIssuedDate").val());

    formData.append("accessoriesInfo.cpmInternetAccess", $("input[name='cpmInternetAccess']:checked").val());
    formData.append("accessoriesInfo.cpmInternetAccessIssuedDate", $("#cpmInternetAccessIssuedDate").val());
    formData.append("accessoriesInfo.windowsLoginID", $("#windowsLoginID").val());
    formData.append("accessoriesInfo.windowsLoginIDIssuedDate", $("#windowsLoginIDIssuedDate").val());

    formData.append("accessoriesInfo.companyEmail", $("#companyEmail").val());
    formData.append("accessoriesInfo.companyEmailIssuedDate", $("#companyEmailIssuedDate").val());

    // ---------- URL SWITCH ----------
    let url = mode === "edit"? "/EmpMaster/UpdateEmployee": "/EmpMaster/SaveEmployee";
    $.ajax({
        url: url,
        type: "POST",
        data: formData,
        processData: false, // Important!
        contentType: false, // Important!
        success: function (res) {
            if (res && res.success) {
                showModalMessage(res.message, true);// Show success message WITH empcode
                //setTimeout(() => { window.location.href = "/EmpMaster/empList"; }, 3000);// 2️. Redirect after 3 seconds
            } else if (res && res.message) {
                showModalMessage(res.message, false);

            } else {
                showModalMessage("Unexpected server response.", false);
            }
        },
        error: function (err) {
            let msg = "Failed to save employee.";
            if (err.responseJSON && err.responseJSON.message) {
                msg = err.responseJSON.message;
            }
            showModalMessage(msg, false); // red message
        }
    });
}

// Edit Bind the Emp Data
function bindEmployee(e) {
    if (!e) return;

    var em = e.empMaster;
    var eu = e.uniformInfo;
    var ea = e.accessoriesInfo;
    const mode = document.getElementById("hdnMode").value;
    document.getElementById("hdnEmpCode").value = em.empcode;// Hidden field
    document.getElementById("empcode").textContent = em.empcode;

    // ---------- DIV 1 ----------
    if (mode == 'edit' || mode == 'view') {
        empName.value = em.empName;
        document.getElementById("empName").disabled = true;
        const divtesting = document.getElementById("divtesting");
        if (mode === "view") {
            divtesting.classList.add("disabled-div");
        } else {
            divtesting.classList.remove("disabled-div");
        }
    }
    // 1. TEMPORARILY UNBIND THE CHANGE EVENT
    $('#department').off('change');
    if (em.department?.trim()) {
        bindDepartmentForEdit(em.department);
    }

    if (em.department) {
        bindSubDepartmentForEdit(em.department, em.subDepartment);
    } else {
        $('#subDepartment').empty().append('<option value="">--- Select SubDept ---</option>');
    }

    if (em.subDepartment) {
        GetSectionsBySubDeptCode(em.subDepartment, em.section);
    }
    else {
        $('#section').empty().append('<option value="">--- Select Section ---</option>');
    }

    if (em.designation?.trim())
        bindDesignationByText(em.designation);
    else
        designation.value = "";

    dateOfJoin.value = formatDate(em.dateOfJoin);
    //empType.value = em.empType?.trim() || "";
    if (em.empType)
        bindEmpTypeByText(em.empType);

    contractperiod.value = em.contractperiod;

    const defaultImg = (em.gender?.trim() === "M") ? "/img/male.jpg" : "/img/female.jpg";
    if (em.photo && em.photo.length > 0) {
        $("#photoPreview").attr("src", "data:image/png;base64," + em.photo);
    } else {
        $("#photoPreview").attr("src", defaultImg);
    }

    if(mode==="edit")
        document.getElementById("photo").disabled = true; //photo.
    // ---------- DIV 2 ----------
    if (em.nationality)
        bindNationalityByValue(em.nationality);
    else
        nationality.value = "";

    //checkRadio("isForeign", em.isForeign);
    isForeignRadio("isForeign", em.isForeign);

    toggleICPassportFields(em.isForeign);// Toggle fields based on foreign/local

    icNo.value = em.icNo;
    dateOfBirth.value = formatDate(em.dateOfBirth);

    if (em.bloodGroup)
        bindBloodGroupByValue(em.bloodGroup);
    else
        bloodGroup.value = "";

    passportNo.value = em.passportNo;
    GendercheckRadio("gender", em.gender?.trim() || "");
    emailID.value = em.emailID;
    mobileNo.value = (em.mobileNo || "").replace(/[\s-]/g, '');

    if (em.religion)
        bindReligionByValue(em.religion.trim());
    else
        religion.value = "";

    if (em.race)
        bindRaceByValue(em.race);
    else
        race.value = "";

    maritalStatus.value = em.maritalStatus.toUpperCase()?.trim() || "";
    setupmaritalstatus(em.maritalStatus, em.spouseWorking);// Toggle fields based on maritalstatus
    if (em.maritalStatus || "")
        spouseName.value = em.spouseName;
    checkRadio("spouseWorking", em.spouseWorking); /*isspouseworking*/
    noOfChildren.value = em.noOfChildren;

    permanentAddress.value = em.permanentAddress;

    sameaspermanent.checked = em.sameaspermanent;
    if (em.sameaspermanent) {
        sameaspermanent.checked === true;
        currentAddress.value = em.currentAddress;
        currentAddress.disabled = true;
    }
    else {
        sameaspermanent.checked === false;
        currentAddress.value = em.currentAddress;
        currentAddress.disabled = false;
    }


    if (em.icFilePath) {
        bindDownloadIcon("iconicfiledownload", em.icFilePath);
        iconicfiledownload.style.disabled = false;
    }
    else {
        iconicfiledownload.style.disabled = true;
    }
    if (em.passportfileurl) {
        bindDownloadIcon("iconpassportfiledownload", em.passportfileurl);
        iconpassportfiledownload.style.disabled = false;
    }
    else {
        iconpassportfiledownload.style.disabled = true;
    }

    // ---------- DIV 3 ----------
    setStayInHostelRadio(em.isStayingHostel);

    if (em.isStayingHostel==="Y") {
        hostelName.style.display = "";   // removes display:none
        hostelName.disabled = false;
        hostelName.value = em.hostelName || "";
        if (em.hostelName)
            bindHotelNameByValue(em.hostelName);
        else
            hostelName.value = "";


        if (em.hostelName === "PH") {
            houseNo.style.display = "";
            houseNo.disabled = false;
            fetchReBindHouseNumbers(em.hostelName, em.houseNo);
        }
        else if (em.hostelName === null || em.hostelName === undefined || em.hostelName.trim() === "-1") {
            hostelName.value = ""; // selects --- select Hostel ---
            houseNo.style.display = "none";
            houseNo.disabled = true;

            typehouseNo.style.display = "none";
            typehouseNo.disabled = true;
            typehouseNo.value = "";
        }
        else {
            houseNo.style.display = "none";
            houseNo.disabled = true;
            typehouseNo.style.display = "";   // removes display:none
            typehouseNo.disabled = false;
            typehouseNo.value = em.houseNo;
        }
    }
    else {
        hostelName.disabled = true;    // disable
    }
    
    isTransportcheckRadio("isTransport", em.isTransport);
    //TransRequiredRadio(em.isTransport ? em.isTransport.trim() : null);
    if (em.isTransport === "Y" && em.route) {
        bindRouteByValue(em.route);
        route.disabled = false;
    }
    else {
        route.value = "";
        route.disabled = true;
    }

    isDrivingcheckRadio("isDrivingLicense", em.isDrivingLicense);

    const hasLicense = em.isDrivingLicense === "Y";
    const dropdownButton = document.querySelector("#divlicensetype button");
    if (dropdownButton) {
        dropdownButton.disabled = !hasLicense;
        dropdownButton.style.opacity = hasLicense ? "1" : "0.5"; // Gray out if disabled
    }
    if (hasLicense) {
        EditLicenseBindingData(em.License); // e.licensetype = "B,C,D"
        updateDropdownText(em.License);
        //spnlicense.text = em.License;
        document.getElementById("vehicleNo1").disabled = false;
        document.getElementById("vehicleNo2").disabled = false;
        document.getElementById("vehicleNo3").disabled = false;

    } else {
        document.querySelector("#License").innerHTML = "";
    }
    vehicleNo1.value = em.vehicleNo1;
    vehicleNo2.value = em.vehicleNo2;
    vehicleNo3.value = em.vehicleNo3;

    if (em.bankName)
        binbankNameValue(em.bankName);
    else
        bankName.value = "";

    accountNumber.value = em.accountNumber;
    epf.value = em.epf;
    sosco.value = em.sosco;
    taxNo.value = em.taxNo;
    emergencyContactPerson.value = em.emergencyContactPerson;
    emergencyContact.value = em.emergencyContact;

    let relationValue = em.relation?.trim();
    if (!relationValue || relationValue === "-1" || relationValue === "-") {
        relation.value = "";
    } else {
        relation.value = relationValue;
    }

    emergencyAddress.value = em.emergencyAddress;

    // ---------- DIV 4 ----------
    if (em.educationLevel)
        bindEducationLevelyValue(em.educationLevel);
    else
        educationLevel.value = "";

    if (em.languageProficiency)
        bindlngValValue(em.languageProficiency);
    else
        languageProficiency.value = "";


    if (em.proficiencyLevel)
        bindProfiValValue(em.proficiencyLevel);
    else
        proficiencyLevel.value = "";

    fieldOfStudy.value = em.fieldOfStudy || "";
    others.value = em.others || "";

    if (em.professionalDocurl) {
        bindDownloadIcon("iconprofessionaldocdownload", em.professionalDocurl);
        iconprofessionaldocdownload.style.disabled = false;
    }
    else {
        iconprofessionaldocdownload.style.disabled = true;
    }
    if (em.resumeDocURL) {
        bindDownloadIcon("iconresumedownload", em.resumeDocURL);
        iconresumedownload.style.disabled = false;
    }
    else {
        iconresumedownload.style.disabled = true;
    }

    if (em.academicCertificateurl) {
        iconacademicCertificatedownload.style.disabled = false;
        bindDownloadIcon("iconacademicCertificatedownload", em.academicCertificateurl);
    }
    else {
        iconacademicCertificatedownload.style.disabled = true;
    }
    if (em.academicCertificateurl) {
        iconothercertificatesdownload.style.disabled = false;
        bindDownloadIcon("iconothercertificatesdownload", em.otherCertificateUrl);
    }
    else {
        iconothercertificatesdownload.style.disabled = true;
    }

    // ---------- DIV 5 ----------
    jacketSize.value = eu.jacketSize || "";
    jacketqty.value = eu.jacketqty || "";
    if (!eu.jacketqty || parseInt(eu.jacketqty, 10) === 0) { // Disable jacketSize if qty is 0 or empty
        jacketSize.disabled = true;
    } else {
        jacketSize.disabled = false;
    }


    safetyBeltSize.value = eu.safetyBeltSize || "";
    safetyBeltqty.value = eu.safetyBeltqty || "";
    if (!eu.safetyBeltqty || parseInt(eu.safetyBeltqty, 10) === 0) { // Disable jacketSize if qty is 0 or empty
        safetyBeltSize.disabled = true;
    } else {
        safetyBeltSize.disabled = false;
    }

    tShirtSize.value = eu.tShirtSize || "";
    tShirtqty.value = eu.tShirtqty || "";
    if (!eu.tShirtqty || parseInt(eu.tShirtqty, 10) === 0) { // Disable jacketSize if qty is 0 or empty
        tShirtSize.disabled = true;
    } else {
        tShirtSize.disabled = false;
    }



    earMuffqty.value = eu.earMuffqty || "";

    pantSize.value = eu.pantSize || "";
    pantqty.value = eu.pantqty || "";
    if (!eu.pantqty || parseInt(eu.pantqty, 10) === 0) { // Disable jacketSize if qty is 0 or empty
        pantSize.disabled = true;
    } else {
        pantSize.disabled = false;
    }

    safetyHelmetqty.value = eu.safetyHelmetqty || "";



    safetyShoes.value = eu.safetyShoes || "";
    safetyShoesqty.value = eu.safetyShoesqty || "";
    if (!eu.safetyShoesqty || parseInt(eu.safetyShoesqty, 10) === 0) { // Disable jacketSize if qty is 0 or empty
        safetyShoes.disabled = true;
    } else {
        safetyShoes.disabled = false;
    }


    apronQty.value = eu.apronQty || "";
    capColor.value = eu.capColor || "";
    capColorqty.value = eu.capColorqty || "";
    if (!eu.capColorqty || parseInt(eu.capColorqty, 10) === 0) { // Disable jacketSize if qty is 0 or empty
        capColor.disabled = true;
    } else {
        capColor.disabled = false;
    }


    dateOfIssue.value = formatDate(eu.dateOfIssue);

    // ---------- DIV 6 ----------

    bindRadioWithDate("cpmHandPhone", ea.cpmHandPhone, "cpmHandphoneIssuedDate", ea.cpmHandphoneIssuedDate);
    bindRadioWithDate("cpmLaptop", ea.cpmLaptop, "cpmLaptopIssuedDate", ea.cpmLaptopIssuedDate);
    bindRadioWithDate("cpmTablet", ea.cpmTablet, "cpmTabletIssuedDate", ea.cpmTabletIssuedDate);
    bindRadioWithDate("cpmSimcard", ea.cpmSimcard, "cpmSimcardIssuedDate", ea.cpmSimcardIssuedDate);
    bindRadioWithDate("cpmInternetAccess", ea.cpmInternetAccess, "cpmInternetAccessIssuedDate", ea.cpmInternetAccessIssuedDate);
    bindRadioWithDate("cpmWalkieTalkie", ea.cpmWalkieTalkie, "cpmWalkieTalkieIssuedDate", ea.cpmWalkieTalkieIssuedDate);

    //bindRadioWithDate("systemaccess", em.computerpass, "systemaccessdate", em.companyemailissuedate);
    //bindRadioWithDate("folderaccess", em.folderaccess, "systemaccessdate", em.folderaccessissuedate);

    windowsLoginID.value = ea.windowsLoginID || "";
    windowsLoginIDIssuedDate.value = formatDate(ea.windowsLoginIDIssuedDate);//windowsloginIDissuedate
    companyEmail.value = ea.companyEmail || "";
    companyEmailIssuedDate.value = formatDate(ea.companyEmailIssuedDate);//companyemailissuedate
}

function downloadfiles(anchorId) {
    const anchor = document.getElementById(anchorId);
    const filePath = anchor.getAttribute("href");
    if (!anchor) return;
    if (!filePath) return;

    // Create a temporary link to force download
    const tempLink = document.createElement("a");
    tempLink.href = filePath;
    tempLink.setAttribute("download", "");
    document.body.appendChild(tempLink);

    tempLink.click();
    document.body.removeChild(tempLink);
}

function ValidateSOSCO() {
    let isValid = false;

    const Nationality = $("#nationality").val() || "";
    const soscoInput = $("#sosco");

    if (Nationality.toUpperCase().trim() === "MAL") {
        const newicno = ($("#newicno").val() || "").replace(/[^a-zA-Z0-9]/g, "");
        const sosco = (soscoInput.val() || "").replace(/[^a-zA-Z0-9]/g, "");

        if (newicno === sosco) {
            isValid = true;
            // Remove highlight if previously added
            soscoInput.removeClass("error-border");
        } else {
            // Focus and highlight
            soscoInput.focus().addClass("error-border");
        }
    }
    return isValid;
}

function validateEmployeeBasic() {
    let isValid = true;
    let firstInvalidField = null;

    // Clear previous errors
    $("#divemployeedetails input, #divemployeedetails select")
        .removeClass("error-border");

    // Helper to mark invalid & capture first error
    function markInvalid(selector) {
        $(selector).addClass("error-border");
        if (!firstInvalidField) {
            firstInvalidField = selector;
        }
        isValid = false;
    }

    // Employee Name
    if ($("#empName").val().trim() === "") {
        markInvalid("#empName");
    }

    // Department
    if ($("#department").val() === "") {
        markInvalid("#department");
    }
    // SubDepartment
    if ($("#subDepartment").val() === "") {
        markInvalid("#subDepartment");
    }

    // Section
    var sectionVal = $("#section").val();
    if (sectionVal === null || sectionVal === "" || sectionVal === "--Select Section--") {
        markInvalid("#section");
    }

    // Designation
    if ($("#designation").val() === "") {
        markInvalid("#designation");
    }

    // Date of Join
    if ($("#dateOfJoin").val() === "") {
        markInvalid("#dateOfJoin");
    }

    // Employee Type
    if ($("#empType").val() === "") {
        markInvalid("#empType");
    }

    // Contract (numbers only + not empty)
    let contract = $("#contractperiod").val().trim();
    if (contract === "" || isNaN(contract)) {
        markInvalid("#contractperiod");
    }

    //// Image upload (only in add mode)
    //if (document.getElementById("hdnMode").value !== "edit") {
    //    if ($("#img")[0].files.length === 0) {
    //        markInvalid("#img");
    //    }
    //}

    if (document.getElementById("hdnMode").value !== "edit") {
        isValid = validateImage();
        if (!isValid)
            markInvalid("#photo");
    }


    // Focus first invalid field
    if (!isValid && firstInvalidField) {
        $(firstInvalidField).focus();
    }
    return isValid;
}

function validateImage() {
    const imgInput = $("#photo")[0];
    const imgPreview = $("#photoPreview").attr("src");
    const defaultImgMale = "/wwwroot/img/male.jpg";
    const defaultImgFemale = "/wwwroot/img/female.jpg";

    if (document.getElementById("hdnMode").value !== "edit") {
        if (imgInput.files.length === 0 || imgPreview === defaultImgMale || imgPreview === defaultImgFemale) {
            $("#photo").addClass("error-border");
            $("#photo").focus(); // set focus to image input
            return false;
        }
    }
    $("#photo").removeClass("error-border");
    return true;
}
// Div-Tag-2 (Input Controls Validation)
function validateEmployeeParticular() {
    let isValid = true;
    let firstInvalidField = null; // Track first invalid field
    const mode = document.getElementById("hdnMode").value;

    // Clear previous validation styles
    $("#divemployeeparticular input, #divemployeeparticular select")
        .removeClass("error-border");

    // Helper: mark invalid & track first field
    function markInvalid(ctrlId) {
        $(ctrlId).addClass("error-border");
        if (!firstInvalidField) {
            firstInvalidField = ctrlId;
        }
        isValid = false;
    }

    // Helper: Dropdown validation
    function validateDropdown(ctrlId) {
        let val = $(ctrlId).val();
        if (val === "" || val === "--- Select") {
            markInvalid(ctrlId);
        }
    }

    // Helper: Textbox empty check
    function validateTextbox(ctrlId) {
        if ($(ctrlId).val().trim() === "") {
            markInvalid(ctrlId);
        }
    }

    // Helper: File upload validation
    function validateFile(ctrlId) {
        if ($(ctrlId)[0].files.length === 0) {
            markInvalid(ctrlId);
        }
    }

    // Helper: Number validation
    function validateNumber(ctrlId) {
        let val = $(ctrlId).val().trim();
        if (val === "" || isNaN(val)) {
            markInvalid(ctrlId);
        }
    }

    // 1. Normal validations (always required)
    validateDropdown("#nationality");
    validateTextbox("#dateOfBirth");
    validateDropdown("#bloodGroup");
    validateTextbox("#emailID");
    validateNumber("#mobileNo");
    validateDropdown("#religion");
    validateDropdown("#race");
    validateDropdown("#maritalStatus");
    validateTextbox("#permanentAddress");

    // 2. Foreign Employee Radio Validation
    let foreignEmp = $("input[name='isForeign']:checked").val();
    if (!foreignEmp) {
        $("input[name='isForeign']").addClass("error-border");
        if (!firstInvalidField) firstInvalidField = "input[name='foreisForeignignemp']";
        isValid = false;
    } else if (foreignEmp === "Y") {
        validateTextbox("#passportNo");
        if (mode !== "edit") {
            validateFile("#passportfileurl");
        }
    } else if (foreignEmp === "N") {
        validateTextbox("#icNo");
        if (mode !== "edit") {
            validateFile("#icFilePath");
        }
    }

    // 3. Sex Radio Validation
    if ($("input[name='gender']:checked").length === 0) {
        $("input[name='gender']").addClass("error-border");
        if (!firstInvalidField) firstInvalidField = "input[name='gender']";
        isValid = false;
    }

    // 4. Marital Status Conditional Validation
    let maritalStatus = $("#maritalStatus").val();
    if (maritalStatus === "M") {
        validateTextbox("#spouseName");
    }
    // Focus first invalid field
    if (!isValid && firstInvalidField) {
        $(firstInvalidField).focus();
    }
    return isValid;
}

// Div-Tag-3 (Input Controls Validation)
function validateHostelLicencesBankTaxDetails() {
    let isValid = true;
    let firstInvalidField = null; // Track first invalid field

    // Clear previous error styles
    $("#divhostellicencesbanktaxdetails input, #divhostellicencesbanktaxdetails select")
        .removeClass("error-border");

    // Helper: mark invalid & track first field
    function markInvalid(ctrlId) {
        $(ctrlId).addClass("error-border");
        if (!firstInvalidField) firstInvalidField = ctrlId;
        isValid = false;
    }

    // Helper: Dropdown validation
    function validateDropdown(ctrlId) {
        let val = $(ctrlId).val();
        if (val === "" || val === "--- Select") {
            markInvalid(ctrlId);
        }
    }
    // Helper: Textbox validation
    function validateTextbox(ctrlId) {
        if ($(ctrlId).val().trim() === "") {
            markInvalid(ctrlId);
        }
    }

    // Helper: Number-only textbox validation
    function validateNumber(ctrlId) {
        let val = $(ctrlId).val().trim();
        if (val === "" || isNaN(val)) {
            markInvalid(ctrlId);
        }
    }
    function validateTaxNumber(ctrlId) {
        let val = $(ctrlId).val().trim();

        // Allow only letters and digits
        if (val === "" || !/^[a-zA-Z0-9]+$/.test(val)) {
            markInvalid(ctrlId);
        }
    }


    // Helper: SOSCO number validation
    //function validatesoscoNumber(ctrlId) {
    //    let val = $(ctrlId).val().trim();
    //    let numericVal = val.replace(/-/g, ''); // Remove hyphens
    //    if (numericVal === "" || !/^\d+$/.test(numericVal)) {
    //        markInvalid(ctrlId);
    //    }
    //}
    function validatesoscoNumber(ctrlId) {
        let val = $(ctrlId).val().trim();
        let cleanedVal = val.replace(/-/g, ''); // Remove hyphens if they are allowed
        // Allow only letters and numbers
        if (cleanedVal === "" || !/^[a-zA-Z0-9]+$/.test(cleanedVal)) {
            markInvalid(ctrlId);
        }
    }



    // Helper: Radio validation
    function validateRadio(name) {
        if ($("input[name='" + name + "']:checked").length === 0) {
            $("input[name='" + name + "']").addClass("error-border");
            if (!firstInvalidField) firstInvalidField = "input[name='" + name + "']";
            isValid = false;
            return null;
        }
        return $("input[name='" + name + "']:checked").val();
    }

    // 1. Stay In Hostel Validation
    let stayInHostel = validateRadio("isStayingHostel");

    if (stayInHostel === "Y") {
        // Hostel name mandatory
        validateDropdown("#hostelName");

        let hostelName = $("#hostelName").val();

        if (hostelName === "TR") {
            // Validate Type House No only
            validateTextbox("#typehouseNo");
        } else if (hostelName === "PH") {
            // Validate House No dropdown only
            validateDropdown("#houseNo");
        }
        else {
            //validateDropdown("houseNo");
        }
    }

    // 2. Transport Needed Validation
    let transportNeeded = validateRadio("isTransport");
    if (transportNeeded === "Y") {
        validateDropdown("#route");
    }

    // 3. Driving License Validation
    let drivingLicense = validateRadio("isDrivingLicense");
    if (drivingLicense === "Y") {
        validateDropdown("#License"); /*licensetype*/

        // At least ONE vehicle number must be entered
        let v1 = $("#vehicleNo1").val().trim();
        let v2 = $("#vehicleNo2").val().trim();
        let v3 = $("#vehicleNo3").val().trim();

        if (v1 === "" && v2 === "" && v3 === "") {
            markInvalid("#vehicleNo1");
            markInvalid("#vehicleNo2");
            markInvalid("#vehicleNo3");
        }
    }

    // 4. Bank & Statutory Details (Always Required)
    validateDropdown("#bankName");
    validateNumber("#accountNumber");
    validateNumber("#epf");
    validatesoscoNumber("#sosco");
    validateTaxNumber("#taxNo");
    //Nationality

    const Nationality = $("#nationality").val() || "";
    if (Nationality.toUpperCase().trim() === "MAL") {
        const soscoInput = $("#sosco");
        const newicno = ($("#icNo").val() || "").replace(/[^a-zA-Z0-9]/g, "");
        const sosco = (soscoInput.val() || "").replace(/[^a-zA-Z0-9]/g, "");
        if (newicno !== sosco) {
            markInvalid("#sosco");
        }
    }

    // 5. Emergency Contact Details (Always Required)
    validateTextbox("#emergencyContactPerson");
    validateNumber("#emergencyContact");
    validateDropdown("#relation");
    validateTextbox("#emergencyAddress");

    // Focus first invalid field
    if (!isValid && firstInvalidField) {
        $(firstInvalidField).focus();
    }

    return isValid;
}

// Div-Tag-4
function validateEducationDetails() {
    let isValid = true;
    let firstInvalidField = null; // Track first invalid field
    const mode = document.getElementById("hdnMode")?.value || ""; // safe read

    // Clear previous validation styles
    $("#diveducationdetails input, #diveducationdetails select")
        .removeClass("error-border");

    // Helper: mark invalid & track first field
    function markInvalid(ctrlId) {
        $(ctrlId).addClass("error-border");
        if (!firstInvalidField) firstInvalidField = ctrlId;
        isValid = false;
    }

    // Helper: Dropdown validation
    function validateDropdown(ctrlId) {
        let val = $(ctrlId).val();
        if (val === "" || val === "--- Select") {
            markInvalid(ctrlId);
        }
    }

    // Helper: Textbox validation
    function validateTextbox(ctrlId) {
        if ($(ctrlId).val().trim() === "") {
            markInvalid(ctrlId);
        }
    }

    // Helper: File upload validation
    function validateFile(ctrlId) {
        if ($(ctrlId)[0].files.length === 0) {
            markInvalid(ctrlId);
        }
    }

    // Validate files only if mode is empty or not "edit"
    if (mode.toLowerCase() !== "edit") {
        validateFile("#resumeDocURL");
        validateFile("#academicCertificateUrl");
        //validateFile("#professionaldocurl");
    } else {
        // Remove error styles for files in edit mode (just in case)
        //$("#resumeDocURL, #academicCertificateUrl, #professionaldocurl").removeClass("error-border");
        $("#resumeDocURL, #academicCertificateUrl").removeClass("error-border");
    }

    // Required dropdowns
    validateDropdown("#educationLevel");
    validateDropdown("#languageProficiency");
    validateDropdown("#proficiencyLevel");

    // Required textboxes
    validateTextbox("#fieldOfStudy");

    // Focus first invalid field
    if (!isValid && firstInvalidField) {
        $(firstInvalidField).focus();
    }

    return isValid;
}

function validateEducationFilesOnSubmit() {
    let isValid = true;
    let firstInvalid = null;

    function markInvalid(ctrlId) {
        $(ctrlId).addClass("error-border");
        if (!firstInvalid) firstInvalid = ctrlId;
        isValid = false;
    }

    // Professional Doc
    if ($("#professionaldocurl")[0].files.length === 0) {
        markInvalid("#professionaldocurl");
    }

    // Resume
    if ($("#resume")[0].files.length === 0) {
        markInvalid("#resume");
    }

    // Academic Certificate
    if ($("#academicCertificateurl")[0].files.length === 0) {
        markInvalid("#academicCertificateurl");
    }

    if (!isValid && firstInvalid) {
        $(firstInvalid).focus();
    }

    return isValid;
}

// Div-Tag-5
function validateUniformAndPPEDetails() {
    let isValid = true;
    let firstInvalidField = null;
    $("#divuniformandppedetails input, #divuniformandppedetails select").removeClass("error-border");

    function markInvalid(ctrlId) {
        $(ctrlId).addClass("error-border");
        if (!firstInvalidField) firstInvalidField = ctrlId;
        isValid = false;
    }
    function validateDate(ctrlId) {
        if ($(ctrlId).val().trim() === "") {
            markInvalid(ctrlId);
        }
    }

    function validateQtyWithDropdown(qtyCtrlId, dropdownCtrlId) {
        let qtyVal = $(qtyCtrlId).val().trim();

        if (qtyVal === "" || isNaN(qtyVal)) {
            markInvalid(qtyCtrlId);
            return;
        }

        qtyVal = parseInt(qtyVal, 10);

        if (qtyVal > 0 && dropdownCtrlId) {
            let ddlVal = $(dropdownCtrlId).val();
            if (ddlVal === "" || ddlVal === "--- Select") {
                markInvalid(dropdownCtrlId);
            }
        }
    }

    validateQtyWithDropdown("#jacketqty", "#jacketSize"); // Jacket
    validateQtyWithDropdown("#safetyBeltqty", "#safetyBeltSize");// Safety Belt
    validateQtyWithDropdown("#tShirtqty", "#tShirtSize"); // T-Shirt
    validateQtyWithDropdown("#earMuffqty", null);// Ear Muff (qty only)

    validateQtyWithDropdown("#pantqty", "#pantSize");// Pant
    validateQtyWithDropdown("#safetyHelmetqty", null); // Helmet (qty only)
    validateQtyWithDropdown("#safetyShoesqty", "#safetyShoes"); // Shoe
    validateQtyWithDropdown("#apronQty", null);// Apron (qty only)

    validateQtyWithDropdown("#capColorqty", "#capColor"); // Cap
    validateDate("#dateOfIssue"); // Uniform Issued Date

    if (!isValid && firstInvalidField) {
        $(firstInvalidField).focus();
    }

    return isValid;
}

function validateUniformAndPPEDetails_Old() {
    let isValid = true;
    let firstInvalidField = null; // Track first invalid field

    // Clear previous validation styles
    $("#divuniformandppedetails input, #divuniformandppedetails select").removeClass("error-border");

    // Helper: mark invalid & track first field
    function markInvalid(ctrlId) {
        $(ctrlId).addClass("error-border");
        if (!firstInvalidField) firstInvalidField = ctrlId;
        isValid = false;
    }

    // Helper: Dropdown validation
    function validateDropdown(ctrlId) {
        let val = $(ctrlId).val();
        if (val === "" || val === "--- Select") {
            markInvalid(ctrlId);
        }
    }

    // Helper: Number-only textbox validation
    function validateNumber(ctrlId) {
        let val = $(ctrlId).val().trim();
        if (val === "" || isNaN(val)) {
            markInvalid(ctrlId);
        }
    }

    // Helper: Date validation
    function validateDate(ctrlId) {
        if ($(ctrlId).val().trim() === "") {
            markInvalid(ctrlId);
        }
    }

    // Jacket
    validateDropdown("#jacketSize");
    validateNumber("#jacketqty");

    // Safety Belt
    validateDropdown("#safetyBeltSize");
    validateNumber("#safetyBeltqty");

    // T-Shirt
    validateDropdown("#tShirtSize");
    validateNumber("#tShirtqty");

    // Ear Muff (quantity only)
    validateNumber("#earMuffqty");

    // Pant
    validateDropdown("#pantSize");
    validateNumber("#pantqty");

    // Helmet (quantity only)
    validateNumber("#safetyHelmetqty");

    // Shoe
    validateDropdown("#safetyShoes");
    validateNumber("#safetyShoesqty");

    // Apron (quantity only)
    validateNumber("#apronQty");

    // Cap
    validateDropdown("#capColor");
    validateNumber("#capColorqty");

    // Uniform Issued Date
    validateDate("#dateOfIssue");

    // Focus first invalid field
    if (!isValid && firstInvalidField) {
        $(firstInvalidField).focus();
    }

    return isValid;
}

function showModalMessage(message, isSuccess) {
    const msgDiv = $("#modalMessage");
    msgDiv.removeClass("alert-success alert-danger").addClass(isSuccess ? "alert-success" : "alert-danger").text(message);
    $("#messageModal").modal("show");
}

function setNewMode() {
    document.getElementById("hdnMode").value = "NEW";
    document.getElementById("hdnEmpCode").value = "";
}

function resetSubDepartment() {
    loadedSubDeptForDepartment = null;
    $('#subDepartment').empty().append('<option value="">--- Select Sub Department ---</option>');
}

function binbankNameValue(bankVal) {
    if (!bankVal) return;
    const ddl = document.getElementById("bankName");
    const text = bankVal.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = bankVal.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindProfiValValue(profiVal) {
    if (!profiVal) return;
    const ddl = document.getElementById("proficiencyLevel");
    const text = profiVal.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = profiVal.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindlngValValue(lngVal) {
    if (!lngVal) return;
    const ddl = document.getElementById("languageProficiency");
    const text = lngVal.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = lngVal.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindEducationLevelyValue(eduVal) {
    if (!eduVal) return;
    const ddl = document.getElementById("educationLevel");
    const text = eduVal.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = eduVal.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindRouteByValue(RouteVal) {
    if (!RouteVal) return;
    const ddl = document.getElementById("route");
    const text = RouteVal.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = RouteVal.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindHotelNameByValue(HotelVal) {
    if (!HotelVal) return;
    const ddl = document.getElementById("hostelName");
    const text = HotelVal.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = HotelVal.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindReligionByValue(ReligionVal) {
    const ddl = document.getElementById("religion");
    if (!ReligionVal) return;

    const observer = new MutationObserver(() => {
        if (ddl.options.length > 1) {
            ddl.value = ReligionVal.trim();
            observer.disconnect();
        }
    });

    observer.observe(ddl, { childList: true });
}

function bindRaceByValue(RaceVal) {
    if (!RaceVal) return;
    const ddl = document.getElementById("race");
    const text = RaceVal.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = RaceVal.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindNationalityByValue(nationext) {
    if (!nationext) return;
    const ddl = document.getElementById("nationality");
    const text = nationext.trim().toLowerCase();
    const interval = setInterval(() => {
        if (ddl.options.length > 1) {
            ddl.value = nationext.trim();
            clearInterval(interval);
        }
    }, 100);
}

function bindDesignationByText(desigVal) {
    const ddl = document.getElementById("designation");
    if (!ddl) return;

    const code = (desigVal || "").trim();
    const interval = setInterval(() => {

        // Wait until dropdown has options loaded
        if (ddl.options.length <= 1) return;

        let matched = false;

        for (let i = 0; i < ddl.options.length; i++) {
            const opt = ddl.options[i];
            if (opt.value === code || opt.text === code) {
                ddl.selectedIndex = i; // ✅ select correct option
                matched = true;
                break;
            }
        }

        if (!matched) {
            ddl.selectedIndex = 0; // fallback to default option
        }

        clearInterval(interval);
    }, 100);
}

function bindEmpTypeByText(desigVal) {
    const ddl = document.getElementById("empType");
    if (!ddl) return;

    const code = (desigVal || "").trim();
    const interval = setInterval(() => {

        // Wait until dropdown has options loaded
        if (ddl.options.length <= 1) return;

        let matched = false;

        for (let i = 0; i < ddl.options.length; i++) {
            const opt = ddl.options[i];
            if (opt.value === code || opt.text === code) {
                ddl.selectedIndex = i; // ✅ select correct option
                matched = true;
                break;
            }
        }

        if (!matched) {
            ddl.selectedIndex = 0; // fallback to default option
        }

        clearInterval(interval);
    }, 100);
}

function bindBloodGroupByValue(designationText) {
    const ddl = document.getElementById("bloodGroup");
    if (!ddl) return;
    const code = (designationText || "").trim(); // ensure string
    const interval = setInterval(() => {
        // Wait until dropdown has options loaded
        if (ddl.options.length <= 1) return;

        // Check if the value exists in the options
        const exists = [...ddl.options].some(opt => opt.value === code);
        if (exists) {
            ddl.value = code; // select the value
        } else {
            ddl.value = ""; // fallback to default
        }
        clearInterval(interval); // stop the interval
    }, 100);
}

function bindDepartmentForEdit(departmentCode) {
    const ddl = document.getElementById("department");
    if (!ddl || !departmentCode) return;
    const code = departmentCode.trim();
    const interval = setInterval(() => {
        // Wait until dropdown is populated
        if (ddl.options.length <= 1) return;
        const exists = [...ddl.options].some(opt => opt.value === code);
        if (exists) {
            ddl.value = code;
            ddl.dispatchEvent(new Event("change")); // load sub-depts
        } else {
            // 🔴 INVALID EDIT VALUE → show default only ONCE
            ddl.value = "";
        }
        clearInterval(interval); // 🔥 IMPORTANT
    }, 100);
}

function setStayInHostelRadio(isHostel) {
    const isYes = isHostel === "Y";

    document.getElementsByName("isStayingHostel").forEach(radio => {
        radio.checked = (radio.value === (isYes ? "Y" : "N"));
    });
}

function TransRequiredRadio(Transareq) {
    const radios = document.getElementsByName("isTransport");
    // Safely handle null or undefined
    const transportValue = Transareq ? Transareq.toString().trim().toLowerCase() : "";
    for (const radio of radios) {
        if (transportValue === "true") {
            radio.checked = radio.value === "Y";
        } else {
            // Covers null, undefined, empty, or anything not "true"
            radio.checked = radio.value === "N";
        }
    }
}

function fetchReBindHouseNumbers(hostelname, selectedHouseNo) {
    const houseNo = document.getElementById("houseNo"); /*houseno*/
    if (!houseNo) return;
    fetch(`/EmpMaster/GetHouseNumbers?housecode=${hostelname}`)
        .then(response => response.json())
        .then(data => {
            // Reset dropdown
            houseNo.innerHTML = "<option value=''>--- Select House No ---</option>";

            if (data && data.length > 0) {
                data.forEach(item => {
                    const option = document.createElement("option");
                    option.value = item.housenumber;
                    option.textContent = item.housenumber;
                    houseNo.appendChild(option);
                });
            }

            // ✅ Select value AFTER binding
            if (selectedHouseNo) {
                houseNo.value = selectedHouseNo;
            }
        })
        .catch(error => {
            console.error("Error fetching house numbers:", error);
        });
}

function setSelectedCheckboxes(containerId, savedValues) {
    if (!savedValues) return;

    const valuesArray = savedValues.split(',').map(v => v.trim());
    // Iterate all checkboxes inside the dropdown
    const checkboxes = document.querySelectorAll(`${containerId} input.cat-check`);
    checkboxes.forEach(cb => {
        cb.checked = valuesArray.includes(cb.value);
    });
}

// Fetch and bind checkbox dropdown data
async function EditLicenseBindingData(savedValues = "") {
    try {
        const response = await fetch('/EmpMaster/GetLicenseTypes');
        if (!response.ok) throw new Error('Network response was not ok');

        const data = await response.json();
        bindCheckboxDropdown('#License', data.records); /*licensetype*/

        // After binding, pre-check saved values if any
        if (savedValues) {
            setSelectedCheckboxes('#License', savedValues); /*licensetype*/
        }
    } catch (err) {
        console.error(err);
        alert(err);
    }
}

function setupmaritalstatus(status, isworking) {
    try {
        var spouseName = document.getElementById("spouseName");
        var noOfChildren = document.getElementById("noOfChildren");
        var spouseWorkingRadios = document.querySelectorAll("input[name='spouseWorking']"); /*isspouseworking*/

        if (status && status === "M" || status.toUpperCase() ==="MARRIED") {
            // Enable spouse name
            spouseName.disabled = false;

            const workingValue = (isworking || "N").toUpperCase();
            spouseWorkingRadios.forEach(function (r) {
                r.disabled = false;
                r.checked = (r.value.toUpperCase() === workingValue);
            });
        } else {
            // Disable everything if not married
            spouseName.value = "";
            spouseName.disabled = true;

            noOfChildren.value = "";
            noOfChildren.disabled = true;

            spouseWorkingRadios.forEach(function (r) {
                r.checked = false;
                r.disabled = true;
            });
        }

    } catch (ex) {
        console.error("Error in setupmaritalstatus:", ex);
    }
}

function updateDropdownText(selectedTexts) {
    const button = $('#divlicensetype button');

    if (!button.length) return; // button not found, exit

    // Ensure it's an array
    if (!Array.isArray(selectedTexts)) {
        if (selectedTexts) {
            selectedTexts = [selectedTexts]; // wrap single value into array
        } else {
            selectedTexts = []; // fallback to empty array
        }
    }

    // Update button text
    button.text(
        selectedTexts.length > 0
            ? selectedTexts.join(', ')
            : '--- select License Type ---'
    );
}

function bindSubDepartmentForEdit(departmentCode, subDepartmentCode) {
    resetSubDepartment();
    if (!departmentCode) return;
    $.ajax({
        url: '/EmpMaster/GetsubDepartment',
        type: 'GET',
        data: { departmentCode },
        success: function (sections) {

            const $ddl = $('#subDepartment');

            // Bind options
            sections.forEach(sec => {
                $ddl.append(
                    `<option value="${sec.subDepartmentCode}">${sec.subDepartmentName}</option>`
                );
            });

            // 🔒 FORCE selection (after options exist)
            setTimeout(() => {$ddl.val(String(subDepartmentCode)).trigger('change');}, 0);
        },
        error: function (err) {
            console.error(err);
            showErrorModal(err);
        }
    });
}

function resetSubDepartment() {
    $('#subDepartment').empty().append('<option value="">--- Select Sub Department ---</option>');
}

function GetSubDeptByDeptvalue(departmentCode, subDepartmentCode = null) {
    $.ajax({
        url: '/EmpMaster/GetsubDepartment',
        type: 'GET',
        data: { departmentCode: departmentCode },
        success: function (data) {
            bind_SubDeptToDropdown(data, subDepartmentCode);
        },
        error: function (err) {
            console.error('Error fetching sections:', err);
            showErrorModal(err);
        }
    });
}

function bind_SubDeptToDropdown(departmentCode, selectedSectionCode = null) {
    const $sectionDropdown = $('#subDepartment');
    $sectionDropdown.empty();
    $sectionDropdown.append('<option value="">--- Select subDept ---</option>');
    if (!departmentCode || departmentCode.length === 0) return;
    const selected = selectedSectionCode? selectedSectionCode.trim().toUpperCase(): null;
    departmentCode.forEach(function (sec) {
        const secCode = sec.subDepartmentCode?.trim().toUpperCase();
        const isSelected = secCode && selected === selected;
        $sectionDropdown.append(
            `<option value="${sec.subDepartmentCode}" ${isSelected ? 'selected' : ''}>${sec.subDepartmentName}</option>`
        );
    });
}

// Get the all Sections based on Department Code
function GetSectionsBySubDeptCode(subDeptCode, selectedSectionCode = null) {
    $.ajax({
        url: '/EmpMaster/GetSectionsbySubDept',
        type: 'GET',
        data: { SubDeptCode: subDeptCode },
        success: function (data) {
            bind_SectionsToDropdown(data, selectedSectionCode);

        },
        error: function (err) {
            console.error('Error fetching sections:', err);
            showErrorModal(err);
        }
    });
}

// Bind Section Numbers where DepartmentCode
function bind_SectionsToDropdown(sections, selectedSectionCode = null) {
    const $sectionDropdown = $('#section');
    $sectionDropdown.empty();
    $sectionDropdown.append('<option value="">--- Select Section ---</option>');
    if (!sections || sections.length === 0) return;

    const selected = selectedSectionCode? selectedSectionCode.trim().toUpperCase(): null;
    sections.forEach(function (sec) {
        const secCode = sec.sectionCode?.trim().toUpperCase();
        const isSelected = selected && secCode === selected;

        $sectionDropdown.append(
            `<option value="${sec.sectionCode}" ${isSelected ? 'selected' : ''}>
                ${sec.sectionName}
            </option>`
        );
    });
}

function isForeignRadio(name, value) {
    if (!value) return;
    // Ensure value is 'Y' or 'N'
    const radioValue = value.toString().trim().toUpperCase();
    const el = document.querySelector(`input[name="${name}"][value="${radioValue}"]`);

    if (el) {
        el.checked = true;
    }
}

// Re-Bind Radio button
function GendercheckRadio(name, value) {
    // Determine actual radio value to select
    const radioValue = value && value.toString().trim().toLowerCase() === "true" ? "M" : "F";

    // Find the radio input with the matching value
    const el = document.querySelector(`input[name="${name}"][value="${radioValue}"]`);

    if (el) el.checked = true;
}

function isDrivingcheckRadio(name, value) {
    if (!value) return;
    const radioValue = value.trim().toUpperCase(); // "Y" or "N"
    const el = document.querySelector(`input[name="${name}"][value="${radioValue}"]`);

    if (el) {
        el.checked = true;
    }
}
function AccessoriescheckRadio(name, value) {
    if (!value) return;
    const radioValue = value.toString().trim().toUpperCase(); // "Y" or "N"
    const el = document.querySelector(`input[name="${name}"][value="${radioValue}"]`);

    if (el) {
        el.checked = true;
    }
}

function isTransportcheckRadio(name, value) {
    // Determine actual radio value to select
    const radioValue = (value || "").toString().trim().toUpperCase(); // Y or N

    // Find the radio input with the matching value
    const el = document.querySelector(`input[name="${name}"][value="${radioValue}"]`);

    if (el) el.checked = true;
}

function checkRadio(name, value) {
    // Determine actual radio value to select
    const radioValue = value && value.toString().trim().toLowerCase() === "true" ? "Y" : "N";

    // Find the radio input with the matching value
    const el = document.querySelector(`input[name="${name}"][value="${radioValue}"]`);

    if (el) el.checked = true;
}

function toggleICPassportFields(foreignemp) {
    const newicno = document.getElementById("icNo");
    const icfile = document.getElementById("icFilePath");
    const passportno = document.getElementById("passportNo");
    const passportfile = document.getElementById("passportfileurl");

    if (foreignemp === "Y") {
        newicno.disabled = true;
        icfile.disabled = true;
        passportno.disabled = false;
        passportfile.disabled = false;
    } else {
        newicno.disabled = false;
        icfile.disabled = false;
        passportno.disabled = true;
        passportfile.disabled = true;
    }
}

function bindRadioWithDate(radioName, radioValue, dateFieldId, dateValue) {
    AccessoriescheckRadio(radioName, radioValue);

    const dateField = document.getElementById(dateFieldId);
    if (!dateField) return;

    if (radioValue && radioValue.trim().toUpperCase() === "Y") {
        dateField.disabled = false;
        dateField.value = formatDate(dateValue);
    } else {
        dateField.disabled = true;
        dateField.value = "";
    }
}

// Check Date format
function formatDate(date) {
    return date ? date.substring(0, 10) : "";
}

// Function to enable/disable a download icon based on file availability
function bindDownloadIcon(iconId, fileUrl) {
    const icon = document.getElementById(iconId);
    if (!icon) return;

    if (fileUrl && fileUrl.trim() !== "") {
        // ENABLE
        icon.href = fileUrl;
        icon.style.pointerEvents = "auto";
        icon.style.opacity = "1";
        icon.style.cursor = "pointer";
    } else {
        // DISABLE
        icon.removeAttribute("href");
        icon.style.pointerEvents = "none";
        icon.style.opacity = "0.5";
        icon.style.cursor = "not-allowed";
    }
}

// View Function
function bindAllEmployeeControls() {
    try {
        // Get employee JSON from hidden field
        const empJson = $('#hdnEmployeeData').val();
        if (!empJson) return;

        const emp = JSON.parse(empJson);
        const mode = $('#hdnMode').val() || 'edit';

        // --- DIV-1: Basic Info ---

        document.getElementById("empcode").textContent = emp.empcode;
        $('#empname').val(emp.empname);
        $('#departmentcode').val(emp.departmentcode);
        $('#sectioncode').val(emp.sectioncode);
        $('#designation').val(emp.designation);
        $('#dateofjoin').val(emp.dateofjoin);
        $('#emptype').val(emp.emptype);
        $('#contract').val(emp.contract);
        if (e.img && e.img.length > 0) {
            // Assuming image is JPG or PNG
            $("#photoPreview").attr(
                "src",
                "data:image/jpeg;base64," + e.img
            );
        }

        // --- DIV-2: Employee Particular ---
        $('#nationality').val(emp.nationality);
        $("input[name='foreignemp'][value='" + emp.foreignemp + "']").prop('checked', true);
        $('#newicno').val(emp.newicno);
        if (emp.icfileurl) $('#icfileurl').after(`<a href="${emp.icfileurl}" target="_blank">View File</a>`);
        $('#dateofbirth').val(emp.dateofbirth);
        $('#bloodgroup').val(emp.bloodGroup);
        $('#passportNo').val(emp.passportNo);
        if (emp.passportfileurl) $('#passportfileurl').after(`<a href="${emp.passportfileurl}" target="_blank">View File</a>`);
        $("input[name='sex'][value='" + emp.sex + "']").prop('checked', true);
        $('#email').val(emp.email);
        $('#pphone').val(emp.pphone);
        $('#religion').val(emp.religion);
        $('#race').val(emp.race);
        $('#maritalstatus').val(emp.maritalstatus);
        $('#spousename').val(emp.spousename);
        $("input[name='isspouseworking'][value='" + emp.isspouseworking + "']").prop('checked', true);
        $('#noofchildren').val(emp.noofchildren);
        $('#address1').val(emp.address1);
        $('#sameaspermanent').prop('checked', emp.sameaspermanent);
        $('#address2').val(emp.address2);

        // --- DIV-3: Hostel / License / Bank / Tax ---
        $("input[name='stayinhostel'][value='" + emp.stayinhostel + "']").prop('checked', true);
        $('#hostelname').val(emp.hostelname);
        $('#houseno').val(emp.houseno);
        $('#typehouseNo').val(emp.typehouseNo);
        $("input[name='transportneeded'][value='" + emp.transportneeded + "']").prop('checked', true);
        $('#route').val(emp.route);
        $("input[name='drivinglicense'][value='" + emp.drivinglicense + "']").prop('checked', true);
        $('#licensetype').val(emp.licensetype);
        $('#vehicleno').val(emp.vehicleno);
        $('#vehicleno2').val(emp.vehicleno2);
        $('#vehicleno3').val(emp.vehicleno3);
        $('#bank').val(emp.bank);
        $('#accountno').val(emp.accountno);
        $('#epf').val(emp.epf);
        $('#sosco').val(emp.sosco);
        $('#taxno').val(emp.taxno);
        $('#emergencycontactpersonname').val(emp.emergencycontactpersonname);
        $('#emergencycontactpersonnumber').val(emp.emergencycontactpersonnumber);
        $('#relation').val(emp.relation);
        $('#EmergencyAddress').val(emp.emergencyAddress);

        // --- DIV-4: Education ---
        $('#edulevel').val(emp.edulevel);
        $('#languagespeaking').val(emp.languagespeaking);
        $('#proficiencylevel').val(emp.proficiencylevel);
        $('#fieldstudy').val(emp.fieldstudy);
        $('#others').val(emp.others);
        if (emp.professionaldocurl) $('#professionaldocurl').after(`<a href="${emp.professionaldocurl}" target="_blank">View File</a>`);
        if (emp.resume) $('#resume').after(`<a href="${emp.resume}" target="_blank">View File</a>`);
        if (emp.academicCertificateurl) $('#academicCertificateurl').after(`<a href="${emp.academicCertificateurl}" target="_blank">View File</a>`);
        if (emp.othercertificatesurl) $('#othercertificatesurl').after(`<a href="${emp.othercertificatesurl}" target="_blank">View File</a>`);

        // --- DIV-5: Uniform & PPE ---
        $('#jackno').val(emp.jackno);
        $('#jackqty').val(emp.jackqty);
        $('#safetybeltsize').val(emp.safetybeltsize);
        $('#safetybeltqty').val(emp.safetybeltqty);
        $('#tno').val(emp.tno);
        $('#tshirtqty').val(emp.tshirtqty);
        $('#earmuffqty').val(emp.earmuffqty);
        $('#pantsize').val(emp.pantsize);
        $('#pantqty').val(emp.pantqty);
        $('#helmetqty').val(emp.helmetqty);
        $('#shoeno').val(emp.shoeno);
        $('#shoeqty').val(emp.shoeqty);
        $('#apronqty').val(emp.apronqty);
        $('#capcolor').val(emp.capcolor);
        $('#capqty').val(emp.capqty);
        $('#uniformdate').val(emp.uniformdate);

        // --- DIV-6: Assets & Access ---
        $("input[name='handphone'][value='" + emp.handphone + "']").prop('checked', true);
        $('#hpdate').val(emp.hpdate);
        $("input[name='laptop'][value='" + emp.laptop + "']").prop('checked', true);
        $('#laptopdate').val(emp.laptopdate);
        $("input[name='tablet'][value='" + emp.tablet + "']").prop('checked', true);
        $('#tabletdate').val(emp.tabletdate);
        $("input[name='companyphone'][value='" + emp.companyphone + "']").prop('checked', true);
        $('#companyphonedate').val(emp.companyphonedate);
        $("input[name='walkietoki'][value='" + emp.walkietalkie + "']").prop('checked', true);
        $('#walkitokidate').val(emp.walkietalkieissuedate);
        $("input[name='internet'][value='" + emp.internet + "']").prop('checked', true);
        $('#netaccessdate').val(emp.netaccessdate);
        $("input[name='systemaccess'][value='" + emp.systemaccess + "']").prop('checked', true);
        $('#systemaccessdate').val(emp.systemaccessdate);
        $("input[name='folderaccess'][value='" + emp.folderaccess + "']").prop('checked', true);
        $('#folderaccessdate').val(emp.folderaccessdate);
        $('#windowloginid').val(emp.windowloginid);
        $('#windowlogindate').val(emp.windowlogindate);
        $('#companyemail').val(emp.companyemail);
        $('#companyemaildate').val(emp.companyemaildate);

        // --- DISABLE ALL CONTROLS IF VIEW MODE ---
        const divtesting = document.getElementById("divtesting");
        const licenseInput = document.getElementById("divlicensetype");
        if (mode === 'view') {
            $("input, select, textarea, button").prop("disabled", true);
            divtesting.disabled = true;  // disable
            licenseInput.disabled = true;  // disable
        }
        else {
            divtesting.disabled = true;  // disable
            licenseInput.disabled = false; // enable
        }

    } catch (ex) {
        console.error('Error in bindAllEmployeeControls:', ex);
        alert('Error loading employee details.');
    }
}
function disableAllEmployeeControls() {
    $("input,select, textarea").prop("disabled", true);
}

function controlActionButtons(mode) {
    const btnSave = document.getElementById("btnsave");
    const btnClear = document.getElementById("btnClear");

    if (mode === "view") {
        btnSave.disabled = true;
    }
    else if (mode === "edit") {
        btnSave.disabled = false;   // Update allowed
    }
}

function ClearFormData() {
    window.location.href = "/EmpMaster/Index";
}

// Function to close the modal
function closeModal() {
    // Using jQuery method
    $('#messageModal').modal('hide');
}

// Function to navigate to employee list page
function goToEmployeeList() {
    closeModal(); // First close the modal
    window.location.href = '/EmpMaster/empList'; // Then navigate to the employee list page
}


//function closeModal() {
//    var modalEl = document.getElementById('messageModal'); // Get the modal element
//    var modal = bootstrap.Modal.getOrCreateInstance(modalEl);// Create a new Modal instance
//    modal.hide();// Hide the modal
//}

// Listen for input / change / keyup events and remove the class.
$(document).on('input change keyup', 'input, select, textarea', function () {
    if ($(this).val().trim() !== '') {
        $(this).removeClass('error-border');
    }
});

// Uniform Validation
function validateuniformQty(type) {
    let qtyInput;
    let sizeSelect;
    switch (type) {
        case 'JACKET':
            qtyInput = document.getElementById('jacketqty');
            sizeSelect = document.getElementById('jacketSize');
            break;
        case 'TSHIRT':
            qtyInput = document.getElementById('tShirtqty');
            sizeSelect = document.getElementById('tShirtSize');
            break;
        case 'PANT':
            qtyInput = document.getElementById('pantqty');
            sizeSelect = document.getElementById('pantSize');
            break;
        case 'SAFETYSHOES':
            qtyInput = document.getElementById('safetyShoesqty');
            sizeSelect = document.getElementById('safetyShoes');
            break;
        case 'CAP':
            qtyInput = document.getElementById('capColorqty');
            sizeSelect = document.getElementById('capColor');
            break;
        case 'BELT':
            qtyInput = document.getElementById('safetyBeltqty');
            sizeSelect = document.getElementById('safetyBeltSize');
            break;
        default:
            console.error('Invalid uniform item type:', itemType);
            return;
    }

    // Safety check
    if (!qtyInput || !sizeSelect) return;

    let qtyValue = parseInt(qtyInput.value, 10);

    // Handle invalid or negative values
    if (isNaN(qtyValue) || qtyValue < 0) {
        qtyValue = 0;
        qtyInput.value = "0";
    }

    // Remove decimals (force integer)
    if (qtyInput.value.includes(".")) {
        qtyInput.value = Math.floor(qtyValue).toString();
        qtyValue = Math.floor(qtyValue);
    }

    // Disable / Enable size select
    if (qtyValue === 0) {
        sizeSelect.disabled = true;
        sizeSelect.value = ""; // reset dropdown
    } else {
        sizeSelect.disabled = false;
    }
}
