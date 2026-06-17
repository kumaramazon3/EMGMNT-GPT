document.addEventListener("DOMContentLoaded", function () {

    //document.getElementById("btnLogin").addEventListener("click", function (event) {
    //    event.preventDefault(); // Stop form refresh
    //    //$('#loadingOverlay').show();// Show overlay + spinner
    //    var userID = document.getElementById("txtUserID").value.trim();
    //    var password = document.getElementById("txtPassword").value.trim();

    //    // Reset UI
    //    document.getElementById("txtUserID").classList.remove("input-error");
    //    document.getElementById("txtPassword").classList.remove("input-error");
    //    document.getElementById("errUserID").innerText = "";
    //    document.getElementById("errPassword").innerText = "";

    //    let isValid = true;

    //    if (userID === "") {
    //        document.getElementById("txtUserID").classList.add("input-error");
    //        document.getElementById("errUserID").innerText = "User ID required";
    //        isValid = false;
    //    }

    //    if (password === "") {
    //        document.getElementById("txtPassword").classList.add("input-error");
    //        document.getElementById("errPassword").innerText = "Password required";
    //        isValid = false;
    //    }

    //    if (!isValid) return;

    //    // Debug
    //    console.log("Sending:", userID, password);

    //    // AJAX POST
    //    fetch("/LoginUser/ValidateLogin", {
    //        method: "POST",
    //        headers: {
    //            "Content-Type": "application/json"
    //        },
    //        body: JSON.stringify({
    //            userID: userID,
    //            password: password
    //        })
    //    })
    //        .then(response => response.json())
    //        .then(data => {

    //            if (data.success === true) {
    //                //window.location.href = "/Home/Index";
    //                // Controller will redirect → JS stays idle
    //                window.location.href = data.redirectUrl;
    //                $('#loadingOverlay').hide();// hide overlay + spinner

    //            } else {
    //                // Display error message on screen
    //                document.getElementById("errLoginMsg").innerText = data.message;
    //                document.getElementById("errLoginMsg").innerText = data.message;
    //            }
    //        })
    //        .catch(err => console.error("Login Error:", err));
    //    //document.getElementById("errLoginMsg").innerText = "Login Error: " + err;
    //});


    // Call the login function when button is clicked
    const btnLogin = document.getElementById("btnLogin");
    btnLogin.addEventListener("click", function (event) {
        event.preventDefault();
        handleLogin();
    });
});

async function handleLogin() {
    const btnLogin = document.getElementById("btnLogin");
    const txtUserID = document.getElementById("txtUserID");
    const txtPassword = document.getElementById("txtPassword");
    const errUserID = document.getElementById("errUserID");
    const errPassword = document.getElementById("errPassword");
    const errLoginMsg = document.getElementById("errLoginMsg");

    // Reset error messages and styles
    txtUserID.classList.remove("input-error");
    txtPassword.classList.remove("input-error");
    errUserID.innerText = "";
    errPassword.innerText = "";
    errLoginMsg.innerText = "";

    const userID = txtUserID.value.trim();
    const password = txtPassword.value.trim();

    // Client-side validation
    let isValid = true;
    if (!userID) {
        txtUserID.classList.add("input-error");
        errUserID.innerText = "User ID required";
        isValid = false;
    }
    if (!password) {
        txtPassword.classList.add("input-error");
        errPassword.innerText = "Password required";
        isValid = false;
    }
    if (!isValid) return;

    // Disable controls while login request is in progress
    btnLogin.disabled = true;
    txtUserID.disabled = true;
    txtPassword.disabled = true;
    btnLogin.innerText = "Logging in..."; // optional

    try {
        const response = await fetch("/LoginUser/ValidateLogin", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ userID, password })
        });

        const data = await response.json();

        if (data.success) {
            // Successful login → redirect
            window.location.href = data.redirectUrl;
        } else {
            // Server returned login error → enable controls
            errLoginMsg.innerText = data.message;
            enableLoginControls();
        }
    } catch (err) {
        // Network or unexpected error → enable controls
        console.error("Login error:", err);
        errLoginMsg.innerText = "Login error, please try again.";
        enableLoginControls();
    }
}

/**
 * Re-enable login controls
 */
function enableLoginControls() {
    const btnLogin = document.getElementById("btnLogin");
    const txtUserID = document.getElementById("txtUserID");
    const txtPassword = document.getElementById("txtPassword");

    btnLogin.disabled = false;
    txtUserID.disabled = false;
    txtPassword.disabled = false;
    btnLogin.innerText = "Login"; // reset button text
}