using System;
using System.Drawing;
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
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                ReadBytesBuffer(buffer);
            }
            catch (Exception e)
            {
                _textContents = e.Message;
                return;
            }

        }

        public ImageDocument(byte[] byteBuffer)
        {
            ReadBytesBuffer(byteBuffer);
        }

        public ImageDocument(Image rawImage)
        {
            ReadImage(rawImage);
        }
        public bool ReadBytesBuffer(byte[] byteBuffer)
        {
            try
            {
                using (var engine = new TesseractEngine(@"tessdata", "spa", EngineMode.Default))
                {
                    using (Pix img = Pix.LoadTiffFromMemory(byteBuffer))
                    {
                        using (Page page = engine.Process(img))
                        {
                            _textContents = page.GetText();
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _textContents = e.Message;
                return false;
            }

        }

        public bool ReadImage(System.Drawing.Image sourceImage)
        {
            try
            {
                using (var engine = new TesseractEngine(@"tessdata", "spa", EngineMode.Default))
                {
                    MemoryStream ms = new MemoryStream();
                    sourceImage.Save(ms, System.Drawing.Imaging.ImageFormat.Tiff);

                    using (Pix img = Pix.LoadTiffFromMemory(ms.ToArray()))
                    {
                        using (Page page = engine.Process(img))
                        {
                            _textContents = page.GetText();
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _textContents = e.Message;
                return false;
            }

        }

        public string GetContents()
        {
            return _textContents;
        }
    }
}
