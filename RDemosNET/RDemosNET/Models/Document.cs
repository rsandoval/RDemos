using System;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

//using PdfSharp.Pdf;
//using PdfSharp.Pdf.IO;

using iText.Kernel.Pdf;

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

        private string ReadPdfDocumentOld(IFormFile fileForUpload)
        {
            string contents = "";

            PdfReader docReader = new PdfReader(fileForUpload.OpenReadStream());
            PdfDocument docToRead = new PdfDocument(docReader);
            docToRead.GetFirstPage

            for (int pageNum = 0; pageNum < docToRead.GetNumberOfPages(); pageNum++)
            {
                PdfPage pdfPage = docToRead.GetPage(pageNum);
                PdfStream pageStream = pdfPage.GetContentStream(0);
                ICollection<PdfName> keys = pageStream.KeySet();
                contents += pageStream.ToString() + " ";
            }

            return contents.Trim();
        }

        private string ReadPdfDocument(IFormFile fileForUpload)
        {
            string contents = "";

            PdfReader docReader = new PdfReader(fileForUpload.OpenReadStream());
            PdfDocument docToRead = new PdfDocument(docReader);
            PDfText

            Pdf

            Rectangle rect = new Rectangle(36, 750, 523, 56);
            CustomFontFilter fontFilter = new CustomFontFilter(rect);
            FilteredEventListener listener = new FilteredEventListener();

            // Create a text extraction renderer
            LocationTextExtractionStrategy extractionStrategy = listener
                    .attachEventListener(new LocationTextExtractionStrategy(), fontFilter);

            // Note: If you want to re-use the PdfCanvasProcessor, you must call PdfCanvasProcessor.reset()
            PdfCanvasProcessor parser = new PdfCanvasProcessor(listener);
            parser.processPageContent(pdfDoc.getFirstPage());

            // Get the resultant text after applying the custom filter
            contents = extractionStrategy.getResultantText();

            docToRead.Close();

            return contents.Trim();
        }
        public string ExtractText(this PdfPage page, Rectangle rect)
        {
            var filter = new IEventFilter[1];
            filter[0] = new TextRegionEventFilter(rect);
            var filteredTextEventListener = new FilteredTextEventListener(new LocationTextExtractionStrategy(), filter);
            var str = PdfTextExtractor.GetTextFromPage(page, filteredTextEventListener);
            return str;
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
