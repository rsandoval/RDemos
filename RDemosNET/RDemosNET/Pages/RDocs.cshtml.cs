using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

using Demo.Models;

namespace RDemosNET
{
    public class RDocsModel : PageModel
    {
        private string _resultMessage = "Documento a mostrar";

        public IActionResult OnGet()
        {
            _resultMessage =
                "En esta demostración Ud. puede subir un documento en formato docx, txt, o pdf y dejar que R:Docs lo analice.\n\r"
                + "Algunas restricciones de estas capacidades.\n\r"
                + "- Por ahora, sólo documentos de texto legible - no imágenes\n\r"
                + "- Sólo puede intentar con 10 documentos por día.";

            return Page();
        }

        public RDocsModel() 
        {
        }

        [BindProperty]
        public IFormFile FileForUpload { get; set; }
        public string ResultMessage { get { return _resultMessage; } set { _resultMessage = value; } }
        public string Filename { get; set; }
        public string TypeDescription { get; set; }
        public string DateDescription { get; set; }
        public string NamesDescription { get; set; }
        public string NotaryDescription { get; set; }

        //public async Task OnPostAsync()
        public async Task OnPostAsync()
        {
            Document document = new Document(FileForUpload, DateTime.Today);

            ResultMessage = "Documento con las siguientes características encontradas";
            Filename = FileForUpload.FileName;
            TypeDescription = document.TypeDescription;
            DateDescription = document.IssueDate.ToString("dd MMM yyyy");
            NamesDescription = document.NamedParts;
            NotaryDescription = document.NotaryName;
        }
    }
}