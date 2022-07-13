using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;



using RDemosNET.Models;

namespace RDemosNET
{
    public class RAlizeBatchModel : PageModel
    {
        private string _resultMessage = "No se han procesado datos";

        public IActionResult OnGet()
        {
            _resultMessage =
                "Esta funcionalidad interna de R:Solver se usa para procesar cierto tipo de archivos solamente.";

            return Page();
        }
        public IFormFile FileForUpload { get; set; }
        public string ResultMessage { get { return _resultMessage; } set { _resultMessage = value; } }
        public string Filename { get; set; }
        public int LinesProcessed { get; set; }

        public async Task OnPostAsync()
        {
            if (String.IsNullOrEmpty(FileForUpload.FileName))
            {
                ResultMessage = "Debe seleccionar un archivo de texto TXT con los comentarios separados por líneas del archivo.";
                return;
            }

            string processedComments = await GetResults();

            ResultMessage += ". Se generó el archivo " + Filename + " con el resultado de " + LinesProcessed + " líneas procesadas.";

            string newFilename = Filename.Substring(0, Filename.LastIndexOf('.')) + "-procesado.csv";
            Response.Headers.Add("Content-disposition", "attachment; filename=" + newFilename);
            Response.ContentType = "text/plain";
            await Response.WriteAsync(processedComments, System.Text.Encoding.UTF8);
            await Response.CompleteAsync();
        }

        public async Task<string> GetResults()
        {
            RAlizeBatchFile batchFile = new RAlizeBatchFile(FileForUpload);

            ResultMessage = "Documento procesado: " + batchFile.Filename;
            Filename = FileForUpload.FileName;

            if (!batchFile.SuccessfullyProcessed)
            {
                ResultMessage = "Error al procesar el documento " + FileForUpload.FileName;

                return ResultMessage;
            }
            else
            {
                ResultMessage = batchFile.Contents;
                LinesProcessed = batchFile.LinesProcessed;
                return ResultMessage;
            }
        }
    }
}
