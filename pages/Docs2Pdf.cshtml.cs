using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GemBox.Document;

namespace web_docs2pdf.Pages
{
    public class Docs2PdfModel : PageModel
    {
        [BindProperty]
        public IFormFile? UploadedFile { get; set; } 

        public async Task<IActionResult> OnPostAsync()
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a DOCX file.");
                return Page();
            }

            if (!UploadedFile.FileName.EndsWith(".docx"))
            {
                ModelState.AddModelError("", "Only DOCX files are supported.");
                return Page();
            }

            if (UploadedFile.Length > 50 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File too large.");
                return Page();
            }

            // Set GemBox license (FREE version)
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            // Prepare folder for temp storage
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(uploadsPath);

            // Save uploaded DOCX
            var docxPath = Path.Combine(uploadsPath, Guid.NewGuid() + ".docx");
            using (var stream = new FileStream(docxPath, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(stream);
            }

            // Convert DOCX → PDF
            var document = DocumentModel.Load(docxPath);
            var pdfPath = Path.Combine(uploadsPath, Guid.NewGuid() + ".pdf");
            document.Save(pdfPath);

            // Read PDF bytes
            var pdfBytes = await System.IO.File.ReadAllBytesAsync(pdfPath);

            // Clean up temporary files
            System.IO.File.Delete(docxPath);
            System.IO.File.Delete(pdfPath);

            // Return PDF for download
            return File(pdfBytes, "application/pdf", "converted.pdf");
        }
    }
}
