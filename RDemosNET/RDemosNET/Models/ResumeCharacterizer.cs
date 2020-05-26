using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using DocumentFormat.OpenXml.Packaging;

namespace RDemosNET.Models
{
    public class ResumeCharacterizer
    {
        public string Filename { get; set; }
        public string Contents { get; set; }

        public ResumeCharacterizer(IFormFile FileForUpload)
        {
            Filename = FileForUpload.FileName;
            if (FileForUpload.ContentType.Contains("pdf"))
            {
                Contents = ReadPdfDocument(FileForUpload);
            }
            else if (FileForUpload.ContentType.Contains("wordprocess"))
            {
                Contents = ReadWordDocument(FileForUpload);
            }
            else
            {
                Contents = ReadTextDocument(FileForUpload);
            }



        }

        public List<string> GetExperienceTokens()
        {
            List<string> tokens = new List<string>();

            TextNormalizer txtNormalizer = TextNormalizer.GetInstance();
            string strContents = txtNormalizer.RemovePunctuation(Contents, "-:");

            string[] contentTokens = Contents.Split();


            //foreach (string contentToken in contentTokens)
                //if (contentToken.Contains("19") || contentToken.Contains("20") && contentToken.Length > 3)


            return tokens;
        }

        #region FileFormatReading

        private string ReadTextDocument(IFormFile fileForUpload)
        {
            string contents = "";
            using (var reader = new StreamReader(fileForUpload.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    contents += reader.ReadLine() + " ";
            }
            return contents;
        }

        private string ReadPdfDocument(IFormFile fileForUpload)
        {
            string contents = "";

            //PdfDocument docToRead = PdfReader.Open(fileForUpload.OpenReadStream());

            //for (int pageNum = 0; pageNum < docToRead.PageCount; pageNum++)
            //{
            //    PdfPage pdfPage = docToRead.Pages[pageNum];
            //    contents += pdfPage.Contents.ToString() + " ";
            //}

            return contents.Trim();
        }

        private string ReadWordDocument(IFormFile fileForUpload)
        {
            string contents = "";

            WordprocessingDocument wordprocessingDocument = WordprocessingDocument.Open(fileForUpload.OpenReadStream(), false);
            DocumentFormat.OpenXml.Wordprocessing.Body body = wordprocessingDocument.MainDocumentPart.Document.Body;

            contents = body.InnerText;

            return contents.Trim();
        }

        #endregion
    }
}
