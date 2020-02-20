using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Demo.Models;

namespace RDemosNET
{
    public class RDocsCVModel : PageModel
    {
        private string _resultMessage = "Documento a mostrar";

        public void OnGet()
        {
            _resultMessage =
                "En esta demostración Ud. puede subir un CV en formato docx, pdf, o txt y definir elementos clave a buscar dentro del CV. R:Docs procesará el contenido, según los elementos buscados.";

        }
        [BindProperty]
        public IFormFile FileForUpload { get; set; }
        public string ResultMessage { get { return _resultMessage; } set { _resultMessage = value; } }
        public string Filename { get; set; }
        public void OnPostAsync(string txtExperience)
        {
            Document document = new Document(FileForUpload, DateTime.Today);

            ResultMessage = "Documento con las siguientes características encontradas";
            Filename = FileForUpload.FileName;
        }
    }
}