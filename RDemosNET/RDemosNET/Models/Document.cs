using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using DocumentFormat.OpenXml.Packaging;
using Tesseract.Interop;
using Tesseract;
using System.Drawing;

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
        public string NamedCompanies { get; set; }
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
            else if (FileForUpload.ContentType.Contains("word"))
            {
                Contents = ReadWordDocument(FileForUpload);
            }
            else if (FileForUpload.ContentType.Contains("jpeg"))
            {
                Contents = ReadImageDocument(FileForUpload);
            }
            else
            {
                Contents = ReadTextDocument(FileForUpload);
            }

            ContentCharacterizer characterizer = ContentCharacterizer.GetInstance();

            Type = characterizer.GetDocumentType(Contents);

            IssueDate = characterizer.GetIssuingDate(Contents);

            List<string> companyNames = characterizer.GetCompanyNames(Contents);
            NamedCompanies = characterizer.GetConcatenatedNames(companyNames);

            List<string> namedParts = characterizer.GetNames(Contents);
            foreach (string companyName in companyNames)
                namedParts.Remove(companyName);

            NamedParts = characterizer.GetConcatenatedNames(namedParts);

            NotaryName = characterizer.GetNotary(Contents);
        }

        protected HtmlGenericControl meanConfidenceLabel;

        private string ReadImageDocument(IFormFile fileForUpload)
        {
            string contents = "";

            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                var reader = new StreamReader(fileForUpload.OpenReadStream());
                // have to load Pix via a bitmap since Pix doesn't support loading a stream.
                using (var image = Pix.LoadTiffFromMemory(reader.))
                {
                    using (var pix = PixConverter.ToPix(image))
                    {
                        using (var page = engine.Process(pix))
                        {
                            meanConfidenceLabel.InnerText = String.Format("{0:P}", page.GetMeanConfidence());
                            resultText.InnerText = page.GetText();
                        }
                    }
                }
            }
            inputPanel.Visible = false;
            resultPanel.Visible = true;
            
            return contents;
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

            PdfReader docReader = new PdfReader(fileForUpload.OpenReadStream());
            PdfDocument docToRead = new PdfDocument(docReader);
            int numPages = docToRead.GetNumberOfPages();
            //docToRead.GetFirstPage

            for (int pageNum = 1; pageNum <= numPages; pageNum++)
            {
                /*ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(
                    Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));*/

                PdfPage pdfPage = docToRead.GetPage(pageNum);
                PdfStream pageStream = pdfPage.GetContentStream(0);
                ICollection<PdfName> keys = pageStream.KeySet();
                string content = PdfTextExtractor.GetTextFromPage(pdfPage);
                contents += content.Replace("\n", " ").Replace("\r","") + " ";
                
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
