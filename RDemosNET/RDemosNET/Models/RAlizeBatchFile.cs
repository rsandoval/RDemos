using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace RDemosNET.Models
{
    public class RAlizeBatchFile
    {
        public string Filename { get; set; }
        public string Contents { get; set; }
        public bool SuccessfullyProcessed { get; set; }
        public int LinesProcessed { get; set; }

        public RAlizeBatchFile()
        {
            SuccessfullyProcessed = false;
            LinesProcessed = 0;
        }

        public RAlizeBatchFile(IFormFile FileForUpload)
        {
            Filename = FileForUpload.FileName;

            //if (!FileForUpload.ContentType.Contains("txt")) return;

            Contents = ReadCSVFile(FileForUpload);

            SuccessfullyProcessed = !String.IsNullOrEmpty(Contents);
        }

        public string ReadCSVFile(IFormFile fileForUpload)
        {
            string contents = "";
            int lines = 0;
            using (var reader = new StreamReader(fileForUpload.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    string comment = reader.ReadLine().Trim().ToLower();
                    CommentCharacterizer characterizer = new CommentCharacterizer(comment);
                    contents += comment + ";" + characterizer.GetSentiment() + ";" + characterizer.GetEmotion() + ";" + characterizer.GetIntention() + ";" + characterizer.GetDescription() + "\n";
                    lines++;
                }
            }
            LinesProcessed = lines;
            return contents;

        }

    }
}
