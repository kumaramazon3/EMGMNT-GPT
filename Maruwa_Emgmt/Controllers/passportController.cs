using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;


namespace Maruwa_Emgmt.Controllers
{
    public class passportController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReadPdf(IFormFile file)
        {
            // 1) Read raw text from PDF pages
            string text = "";
            using (var stream = file.OpenReadStream())
            {
                var reader = new PdfReader(stream);
                using (var pdf = new PdfDocument(reader))
                {
                    for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                    {
                        text += PdfTextExtractor.GetTextFromPage(pdf.GetPage(i)) + "\n";
                    }
                }
            }

            // 2) Remove everything before the holder section to reduce noise/repetition (if present)
            //    This keeps the part starting from "ePass Holder Information" if it exists,
            //    otherwise it leaves text intact.
            string cleanText = text;
            var holderIdx = Regex.Match(text, @"ePass Holder Information", RegexOptions.IgnoreCase);
            if (holderIdx.Success)
            {
                // keep from the first occurrence of holder section
                cleanText = text.Substring(holderIdx.Index);
            }

            // 3) Helper: try several patterns, return first match
            string GetFirstMatch(string[] patterns, RegexOptions opts = RegexOptions.IgnoreCase | RegexOptions.Singleline)
            {
                foreach (var p in patterns)
                {
                    var m = Regex.Match(cleanText, p, opts);
                    if (m.Success && m.Groups.Count >= 2)
                    {
                        var s = m.Groups[1].Value.Trim();
                        // Normalize whitespace and collapse multi-line into single line
                        s = Regex.Replace(s, @"\s+", " ");
                        return s;
                    }
                }
                return "";
            }

            // 4) Field patterns (multiple fallbacks)

                var vpNo = GetFirstMatch(new[] {
        @"VP\s*No\s*[:]\s*([^\r\n]+)",
        @"VP\s*No\s*\r?\n\s*[:]\s*([^\r\n]+)"
    }, RegexOptions.IgnoreCase);

            var name = GetFirstMatch(new[] {
        @"Name\s*[:]\s*([^\r\n]+)",
        @"Name\s*\r?\n\s*[:]\s*([^\r\n]+)",
        @"ePass Holder Information.*?Name\s*[:]\s*([^\r\n]+)"
    });

            var gender = GetFirstMatch(new[] {
        @"Gender\s*[:]\s*([^\r\n]+)",
        @"Gender\s*\r?\n\s*[:]\s*([^\r\n]+)"
    });

            var nationality = GetFirstMatch(new[] {
        @"Nationality\s*[:]\s*([^\r\n]+)",
        @"Nationality\s*\r?\n\s*[:]\s*([^\r\n]+)"
    });

            var passportNo = GetFirstMatch(new[] {
        @"Passport\s*No\s*[:]\s*([^\r\n]+)",
        @"Passport\s*No\s*\r?\n\s*[:]\s*([^\r\n]+)"
    });

            var refNo = GetFirstMatch(new[] {
        @"Ref\s*No\s*[:]\s*([^\r\n]+)",
        @"Ref\s*No\s*\r?\n\s*[:]\s*([^\r\n]+)"
    });

            // Date of Issue: try "date BEFORE label" first, then "value after label"
            var issueDate = GetFirstMatch(new[] {
        // date immediately before "Date Of Issue"
        @"([0-9]{2}\s+[A-Za-z]{3}\s+[0-9]{4})\s*Date\s+Of\s+Issue",
        // or date after the label
        @"Date\s+Of\s+Issue\s*[:\r\n]*\s*([^\r\n]+)"
    });

            // Place of Issue: value is often after a ":" on same line or after label; try both
            var placeOfIssue = GetFirstMatch(new[] {
        @"Place\s+Of\s+Issue\s*[:\r\n]*\s*([^\r\n]+)",
        // sometimes appears as ": SOMEVALUE \n Date Of Issue" etc - try to pick token after a colon above Date Of Issue
        @"[:]\s*([A-Z][A-Z0-9 \-\,\/]+)\s*Date\s+Of\s+Issue"
    });

            // Valid Until: inside Condition block -> find UNTIL <date>
            var validUntil = GetFirstMatch(new[] {
        @"Condition.*?UNTIL\s*([0-9]{2}\s+[A-Za-z]{3}\s+[0-9]{4})",
        // fallback: a standalone date labeled "Valid Until" or near "Good for any number"
        @"Valid\s+Until\s*[:\r\n]*\s*([^\r\n]+)",
        @"UNTIL\s*([0-9]{2}\s+[A-Za-z]{3}\s+[0-9]{4})"
    });

            // Position: capture multi-line phrase up to DEVELOPER (or end of line block)
            var position = GetFirstMatch(new[] {
        @"FOR\s+EMPLOYMENT\s+AS\s*([\s\S]*?DEVELOPER)",
        @"FOR\s+EMPLOYMENT\s+AS\s*([^\r\n]+)"
    }, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Company name (With :)
            var company = GetFirstMatch(new[] {
        @"With\s*[:]\s*([^\r\n]+)",
        @"With\s*\r?\n\s*[:]\s*([^\r\n]+)"
    });

            // Company Address: grab everything between "At :" and next "Ref No" (non-greedy)
            var companyAddress = "";
            {
                var m = Regex.Match(cleanText, @"At\s*[:]\s*(.*?)\s*Ref\s*No", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (m.Success && m.Groups.Count >= 2)
                {
                    companyAddress = Regex.Replace(m.Groups[1].Value.Trim(), @"\s+", " ");
                }
                else
                {
                    // fallback: try to find line that starts with "At :"
                    var m2 = Regex.Match(cleanText, @"At\s*[:]\s*([^\r\n]+)", RegexOptions.IgnoreCase);
                    if (m2.Success) companyAddress = Regex.Replace(m2.Groups[1].Value.Trim(), @"\s+", " ");
                }
            }

            // Final clean-up: ensure "position" single-line and trimmed
            position = Regex.Replace(position, @"\s+", " ").Trim();

            // Build result
            var result = new
            {
                vpNo,
                name,
                gender,
                nationality,
                passportNo,
                refNo,
                issueDate,
                placeOfIssue,
                validUntil,
                position,
                company,
                companyAddress
            };

            return Json(result);
        }

    }
}
