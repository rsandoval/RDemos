using System;
using System.IO;
using System.Drawing;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Office.Interop.Word;

using Microsoft.ML.Data;


namespace Demo.Models
{
    public class Curriculum
    {
        public int ID { get; set; }
        public string Filename { get; set; }
        public string Contents { get; set; }

        [DataType(DataType.Date)]
        public DateTime LoadDate { get; set; }

        public string PersonalID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Profession { get; set; }

        public bool IsCV { get; set; }
        public bool SuccessfullyProcessed { get; set; }
        public string LastErrorMessage { get; set; }


        public Curriculum()
        {
            SuccessfullyProcessed = false;
        }

        public Curriculum(IFormFile FileForUpload, DateTime uploadDate)
        {
            Filename = FileForUpload.FileName;
            LoadDate = uploadDate;

            SuccessfullyProcessed = true;
            if (FileForUpload.ContentType.Contains("pdf"))
            {
                Contents = Document.ReadPdfDocument(FileForUpload);
            }
            else if (FileForUpload.ContentType.Contains("openxml"))
            {
                Contents = Document.ReadWordDocument(FileForUpload);
            }
            else if (FileForUpload.ContentType.Contains("image"))
            {
                Contents = Document.ReadImageDocument(FileForUpload);
            }
            else
            {
                Contents = Document.ReadTextDocument(FileForUpload);
            }

            if (String.IsNullOrEmpty(Contents) || Contents.Contains("[EXCEPTION]"))
            {
                SuccessfullyProcessed = false;
                LastErrorMessage = "Error al procesar archivo.";
                return;
            }


            // ContentCharacterizer characterizer = ContentCharacterizer.GetInstance();
            GetContents(Contents);
        }

        public void GetContents(string contents)
        {
            char[] separators = { ' ', '\t', '\n', '\r' };
            string[] tokens = contents.Split(separators);
            Name = GetName(tokens);
            Email = GetEmail(tokens);
            Phone = GetPhone(tokens);
            PersonalID = GetPersonalID(tokens);
            Profession = GetProfession(tokens);

            if ((String.IsNullOrEmpty(Email) || Email.Contains("(no detectado)")) &&
                (String.IsNullOrEmpty(Phone) || Phone.Contains("(no detectado)")))
                IsCV = false;
            else IsCV = true;
        }


        public bool InterpretContents(string contents)
        {
            char[] separators = { ' ', '\t', '\n', '\r' };
            string[] relevantTokens = { "teléfono", "celular", "email", "e-mail", "domicilio", "profesional" };
            string[] tokens = contents.Split(separators);
            int tokenIndex = 0;
            int currentStatus = 0; // Status -> 1: nombre, 2: email, 3: teléfono, 4: domicilio, 5: profesión

            string tempText = "";
            do
            {
                string currentToken = tokens[tokenIndex].ToLower();


            } while (++tokenIndex < tokens.Length);

            IsCV = true;

            return true;
        }

        private string GetName(string[] tokens)
        {
            EntityRecognizer nerMotor = new EntityRecognizer();
            string[] endOfNameTokens = { "TELÉFONO", "CALLE", "PERFIL", "ANTECEDENTES", "FECHA", "EMAIL", "E-MAIL", "CEDULA", "CÉDULA", "RUT" };
            int tokenIndex = 0;
            string fullname = "";

            bool beforeName = false;
            bool readingName = false;
            int nameItemsCounter = 0;
            while (tokenIndex < tokens.Length && nameItemsCounter <= 5)
            {
                string token = tokens[tokenIndex];
                if (String.IsNullOrEmpty(token)) { tokenIndex++; continue; }

                if (readingName && nameItemsCounter <= 5)
                {
                    bool endOfName = false;
                    if (token.Contains("@")) endOfName = true;
                    else
                    {
                        foreach (string endToken in endOfNameTokens)
                            if (endToken.Equals(token.ToUpper()))
                            {
                                endOfName = true;
                                nameItemsCounter = 1000;
                                break;
                            }
                    }
                    if (endOfName) break;

                    fullname += token + " ";
                    nameItemsCounter++;
                    beforeName = false;
                }
                else if (nerMotor.IsName(token) || beforeName)
                {
                    readingName = true; beforeName = false;
                    nameItemsCounter++;
                    fullname += token + " ";
                }
                else if (token.ToUpper().StartsWith("NOMBRE"))
                    beforeName = true;

                tokenIndex++;
            }

            if (String.IsNullOrEmpty(fullname)) return "(no detectado)"; 

            return fullname.Trim().ToUpper();
        }

        private string GetEmail(string[] tokens)
        {
            int tokenIndex = 0;

            while (tokenIndex < tokens.Length)
            {
                string token = tokens[tokenIndex];
                if (String.IsNullOrEmpty(token)) { tokenIndex++; continue; }

                if (token.Contains("@") && token.Contains("."))
                    return token;

                tokenIndex++;
            }

            return "(no detectado)";
        }

        private bool IsPhoneCandidate(string str)
        {
            if (str.Length > 10) return false;
            foreach (char c in str)
            {
                if ((c < '0' || c > '9') && c != '.' && c != '+' && c != '(' && c != ')')
                    return false;
            }

            return true;
        }

        private string GetPhone(string[] tokens)
        {
            EntityRecognizer nerMotor = new EntityRecognizer();
            string[] endOfNameTokens = { "CALLE", "PERFIL", "ANTECEDENTES", "FECHA", "EMAIL", "E-MAIL", "CEDULA", "CÉDULA", "RUT" };
            int tokenIndex = 0;
            string fullphone = "";

            int itemsCounter = 0;
            bool readingItem = false;
            bool beforeItem = false;
            while (tokenIndex < tokens.Length && itemsCounter <= 3)
            {
                string token = tokens[tokenIndex];
                if (String.IsNullOrEmpty(token)) { tokenIndex++; continue; }

                if (readingItem && itemsCounter <= 3)
                {
                    bool endOfItem = false;
                    if (token.Contains("@")) endOfItem = true;
                    if (!IsPhoneCandidate(token)) endOfItem = true;
                    else
                    {
                        foreach (string endToken in endOfNameTokens)
                            if (endToken.Equals(token.ToUpper()))
                            {
                                endOfItem = true;
                                itemsCounter = 1000;
                                break;
                            }
                    }
                    if (endOfItem) break;

                    fullphone += token;
                    itemsCounter++;
                    beforeItem = false;
                }
                else if (token.StartsWith("+") || beforeItem)
                {
                    if (token.ToUpper().StartsWith("CELULAR")) { tokenIndex++; continue; }
                    readingItem = true; beforeItem = false;
                    itemsCounter++;
                    fullphone += token + " ";
                }
                else if (token.ToUpper().StartsWith("TELÉFONO") || token.ToUpper().StartsWith("TELEFONO") || token.ToUpper().StartsWith("CELULAR"))
                    beforeItem = true;

                tokenIndex++;
            }

            if (fullphone.Length < 5) return "(no detectado)";

            return fullphone.Trim().ToUpper();
        }


        private bool IsRUTCandidate(string str)
        {
            if (str.Length < 8 || str.Length > 12) return false;
            if (!str.Contains("-")) return false;

            foreach (char c in str)
            {
                if ((c < '0' || c > '9') && c != '.' && c != '-')
                    return false;
            }

            if (str.IndexOf("-") < str.Length - 2) return false;

            return true;
        }

        private string GetPersonalID(string[] tokens)
        {
            int tokenIndex = 0;

            bool beforeItem = false;
            while (tokenIndex < tokens.Length)
            {
                string token = tokens[tokenIndex];
                if (String.IsNullOrEmpty(token)) { tokenIndex++; continue; }

                if (IsRUTCandidate(token))
                    return token;
                else if (token.ToUpper().StartsWith("RUT") || token.ToUpper().StartsWith("IDENTIDAD") || token.ToUpper().StartsWith("CEDULA") || token.ToUpper().StartsWith("CÉDULA"))
                    beforeItem = true;

                tokenIndex++;
            }

            return "(no detectado)";
        }

        public bool IsDateRange(string str)
        {
            if (str.Length < 4 || str.Length > 9) return false;

            foreach (char c in str)
            {
                if ((c < '0' || c > '9') && c != '-')
                    return false;
            }

            if (str.IndexOf("-") > str.Length - 3) return false;

            return true;

        }

        private string GetProfession(string[] tokens)
        {
            EntityRecognizer nerMotor = new EntityRecognizer();
            int tokenIndex = 0;
            string fulldescription = "";

            bool beforeItem = false;
            bool readingItem = false;
            int itemsCounter = 0;
            bool endOfItem = false;
            while (tokenIndex < tokens.Length && itemsCounter <= 50 && !endOfItem)
            {
                string token = nerMotor.ReplaceTildes(tokens[tokenIndex].ToUpper());
                if (String.IsNullOrEmpty(token)) { tokenIndex++; continue; }

                /*if ((readingItem || beforeItem) && (!token.Contains("INGENIER") || !token.Contains("MBA") || !token.Contains("TECNIC") ||
                    !token.Contains("UNIVERSIDAD") || !token.Contains("INSTITUTO")))
                {
                    readingItem = beforeItem = false;
                    tokenIndex++;
                    continue;
                }*/

                if (readingItem && itemsCounter <= 50)
                {
                    beforeItem = false;

                    if (IsDateRange(token)) { endOfItem = true; break; }

                    fulldescription += tokens[tokenIndex] + " ";
                    itemsCounter++;
                    if (token.Contains(",")) { endOfItem = true; break; }
                }
                else if (token.Contains("INGENIER") || token.Contains("MBA") || 
                    token.Contains("UNIVERSIDAD") || token.Contains("INSTITUTO") || beforeItem)
                {
                    readingItem = true; beforeItem = false;
                    itemsCounter++;
                    fulldescription += tokens[tokenIndex] + " ";
                }
                else if (token.ToUpper().StartsWith("PROFESIONAL") || token.ToUpper().StartsWith("EDUCACION") ||
                    token.ToUpper().StartsWith("CALIFICACION") || token.ToUpper().StartsWith("CARRERA") ||
                    token.ToUpper().StartsWith("ESTUDIOS") || token.ToUpper().StartsWith("TITULO"))
                    beforeItem = true;

                tokenIndex++;
            }

            if (String.IsNullOrEmpty(fulldescription)) return "(no detectado)";
            if (fulldescription.ToUpper().StartsWith("DE "))
                fulldescription = fulldescription.Substring(3);

            fulldescription = fulldescription.Replace("ANTECEDENTES", "");
            fulldescription = fulldescription.Replace("CALIFICACIONES", "");

            return fulldescription.Trim().ToUpper().Replace(",", "");
        }

    }
}
