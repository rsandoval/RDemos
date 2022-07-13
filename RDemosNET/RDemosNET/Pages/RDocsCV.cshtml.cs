using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

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

        public string DocumentDescription { get; set; }
        public string DocumentContents { get; set; }
        public string Filename { get; set; }


        public void OnPostAsync(string txtExperience)
        {
            Filename = FileForUpload.FileName;
            if (String.IsNullOrEmpty(Filename))
                DocumentDescription = "Debe seleccionar un documento para procesar. Los formatos son: PDF, DOC, DOCX, TXT, y TIF";
            else
                DocumentDescription = GetContentDetails();

            ResultMessage = "Documento con las siguientes características encontradas<br/>" + DocumentDescription;
        }


        public string GetContentDetails()
        {
            Curriculum document = new Curriculum(FileForUpload, DateTime.Today);

            ResultMessage = document.Contents;


            if (!document.SuccessfullyProcessed)
            {
                ResultMessage = "Error al procesar el documento " + FileForUpload.FileName + "<br/>" +
                    document.LastErrorMessage;

                return ResultMessage;
            }
            else
            {
                DocumentContents = document.Contents;

                string description = "<ul>";

                description += "<li>Archivo: <b>" + Filename + "</b></li>";
                if (!document.IsCV)
                {
                    description += "<li>Se determinó que el documento indicado NO es un CV válido.</li>";

                }
                else
                {
                    description += "<li>RUT: <b>" + document.PersonalID + "</b></li>";
                    description += "<li>Nombre: <b>" + document.Name + "</b></li>";
                    description += "<li>E-mail: <b>" + document.Email + "</b></li>";
                    description += "<li>Teléfono: <b>" + document.Phone + "</b></li>";
                    description += "<li>Profesión: <b>" + document.Profession + "</b></li>";

                }
                description += "<li><font color='gray'>Contenidos: <br />" + document.Contents.Substring(0, 500) + "</font></li>";
                description += "</ul>";
                return description;
            }


        }
    }
}