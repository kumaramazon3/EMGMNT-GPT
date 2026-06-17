/**********************************************************
   3️ OCR IMAGE READING (Tesseract.js)
**********************************************************/
async function readImageText(file) {
    const result = await Tesseract.recognize(file, "eng", {
        logger: m => console.log(m)
    });

    return result.data.text;
}


/**********************************************************
   4️ PDF TEXT READING (PDF.js)
**********************************************************/
async function readPdfText(file) {

    const arrayBuffer = await file.arrayBuffer();
    const pdf = await pdfjsLib.getDocument({ data: arrayBuffer }).promise;

    let fullText = "";

    for (let i = 1; i <= pdf.numPages; i++) {
        const page = await pdf.getPage(i);
        const content = await page.getTextContent();
        const strings = content.items.map(item => item.str).join(" ");
        fullText += strings + "\n";
    }

    return fullText;
}


/**********************************************************
   5️ AUTOMATIC FIELD FILL — PASSPORT (IMAGE OCR)
**********************************************************/
function fillPassportFromText(text) {

    console.log("OCR Passport Text:", text);

    // Passport number example match
    const passportNo = text.match(/[A-Z][0-9]{7,8}/);
    if (passportNo) document.getElementById("passportno").value = passportNo[0];

    // DOB dd/mm/yyyy
    const dob = text.match(/\d{2}\/\d{2}\/\d{4}/);
    if (dob) document.getElementById("dateofbirth").value = dob[0];

    // Nationality
    if (text.includes("IND")) document.getElementById("nationality").value = "INDIAN";

    // Add more field parsing as needed...
}



/**********************************************************
   6️ AUTOMATIC FIELD FILL — VISA PDF
**********************************************************/
function fillVisaFromText(text) {

    console.log("Visa PDF Text:", text);

    let m;

    m = text.match(/Ref\s*No[: ]+([A-Z0-9]+)/i);
    if (m) document.getElementById("v_refno").value = m[1];

    m = text.match(/Passport\s*No[: ]+([A-Z0-9]+)/i);
    if (m) document.getElementById("v_passportno").value = m[1];

    m = text.match(/\d{2}\s?[A-Za-z]{3}\s?\d{4}/);
    if (m) document.getElementById("v_validuntil").value = m[0];

    // Add more field parsing as needed...
}



/**********************************************************
   7️ FILE INPUT LISTENERS
**********************************************************/

// Passport Image Upload (JPG/PNG)
document.querySelector("input[name='passportFile']")
    .addEventListener("change", async function () {

        let file = this.files[0];
        if (!file) return;

        let text = await readImageText(file);
        fillPassportFromText(text);
    });


// Visa PDF Upload
document.querySelector("input[name='visaFile']")
    .addEventListener("change", async function () {

        let file = this.files[0];
        if (!file) return;

        let text = await readPdfText(file);
        fillVisaFromText(text);
    });