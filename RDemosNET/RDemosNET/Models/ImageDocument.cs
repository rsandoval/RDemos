using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Tesseract.Interop;
using Tesseract;


namespace Demo.Models
{
    public class ImageDocument
    {
        private string _textContents = "";

        public ImageDocument(Stream fileStream)
        {
            try
            {
                using (var engine = new TesseractEngine(@"tessdata", "spa" , EngineMode.Default))
                {
                    byte[] buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, (int) fileStream.Length);
                    using (var img = Pix.LoadTiffFromMemory(buffer))
                    {
                        using (var page = engine.Process(img))
                        {
                            _textContents = page.GetText();
                        }
                    }
                    // have to load Pix via a bitmap since Pix doesn't support loading a stream.
                    //using (var image = Pix.LoadTiffFromMemory(reader.))
                    //{
                    //    //using (var pix = PixConverter.ToPix(image))
                    //    //{
                    //    //    using (var page = engine.Process(pix))
                    //    //    {
                    //    //        meanConfidenceLabel.InnerText = String.Format("{0:P}", page.GetMeanConfidence());
                    //    //        resultText.InnerText = page.GetText();
                    //    //    }
                    //    //}
                    //}
                }

            }
            catch (Exception e)
            {
                _textContents = e.Message;
                return;
            }

        }

        public string GetContents()
        {
            return _textContents;
        }
    }
}
