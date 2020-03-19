using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

using DocumentFormat.OpenXml.Packaging;

using Microsoft.ML.Data;

namespace Demo.Models
{
    public class Document
    {
        public int ID { get; set; }
        public string Filename { get; set; }
        public string Contents { get; set; }

        [DataType(DataType.Date)]
        public DateTime LoadDate { get; set; }

        public string Type { get; set; }
        public string IssueDate { get; set; }
        public string NamedParts { get; set; }

        public string NotaryName { get; set; }

        public string TypeDescription { get { return ContentCharacterizer.GetInstance().GetTypeDescription(Type); } }

        public Document()
        {
        }

        public Document(IFormFile FileForUpload, DateTime uploadDate)
        {
            Filename = FileForUpload.FileName;
            LoadDate = uploadDate;

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

            ContentCharacterizer characterizer = ContentCharacterizer.GetInstance();

            Type = characterizer.GetDocumentType(Contents);

            IssueDate = characterizer.GetIssuingDate(Contents);

            NamedParts = characterizer.GetConcatenatedNames(characterizer.GetNames(Contents));

            NotaryName = characterizer.GetNotary(Contents);
        }

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

            PdfDocument docToRead = PdfReader.Open(fileForUpload.OpenReadStream());

            for (int pageNum = 0; pageNum < docToRead.PageCount; pageNum++)
            {
                PdfPage pdfPage = docToRead.Pages[pageNum];
                contents += pdfPage.Contents.ToString() + " ";
            }

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


        public override string ToString()
        {
            return ""; //  characterizer.GetDocumentDescription(Filename, contents.ToString());
        }
    }



    /// <summary>
    /// SimpleDocument is a lighter Document class used for Type Classification (ML)
    /// </summary>

    public class SimpleDocument
    {
        private string _contents = "";

        [LoadColumn(0)]
        public string ID { get; set; }
        [LoadColumn(1)]
        public string Contents { get { return _contents; } set { _contents = value.ToUpper(); } }
        [LoadColumn(2)]
        public string Type { get; set; }
    }


    public class TypePrediction
    {
        [ColumnName("PredictedLabel")]
        public string Type;
    }
}
