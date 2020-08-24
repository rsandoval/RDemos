////////////////////////////////////////////////////////////////////////////////////////////////////
// CONFIDENCIAL:
// Este es un archivo de código fuente propiedad de RSolver SpA y está protegido bajo derechos
// de autor y leyes de protección a la propiedad intelectual. Ud. no debe ver, revisar, copiar,
// utilizar, compartir, o distribuir este archivo, excepto que cuente con la expresa licencia y/o
// autorización de RSolver SpA.
// CONFIDENTIAL:
// This is a source code file property of RSolver SpA and is protected by copyright laws.
// You may not see, read, check, copy, share, and/or distribute this file without the written
// authorization from RSolver SpA.
////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright © 2020 R:Solver (RSolver SpA)
//  Todos los derechos reservados sobre este código fuente / All rights reserved over this source code
//
//  TextNormalizer.cs
//  This class implements methods to clean text in order to simplify it for NLP purposes.
//
//  .NET Project:       Demos
//  Company Project:    Demos
//  Creado/Created:     feb 2020 - Rodrigo Sandoval (rodrigo.sandoval@rsolver.com)
//  
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace RDemosNET.Models
{
    public class TextNormalizer
    {
        private static TextNormalizer _single = null;
        const string _modelFolder = @"data";
        private static string BasePath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        List<string> _stopWords = new List<string>();
        List<string> _irrelevantExpressions = new List<string>();

        private TextNormalizer()
        {
            _stopWords = LoadDataFile(Path.Combine(BasePath, _modelFolder, "StopWords.txt"));
            _irrelevantExpressions = LoadDataFile(Path.Combine(BasePath, _modelFolder, "IrrelevantText.txt"));
        }

        public static TextNormalizer GetInstance()
        {
            if (_single == null) _single = new TextNormalizer();
            return _single;
        }

        public string CleanString(string text)
        {
            string resultString = " " + RemovePunctuation(text.ToLower());

            foreach (string irrelevant in _irrelevantExpressions)
                resultString = resultString.Replace(irrelevant, " ");

            foreach (string word in _stopWords)
                resultString = resultString.Replace(" " + word + " ", " ");

            resultString = StemWords(resultString);
            resultString = RemoveDummies(resultString);

            resultString = resultString.Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Trim();

            return resultString;
        }

        public string RemovePunctuation(string text, string symbolsToKeep = "")
        {
            string[] punctuations = { ".", ",", ";", ":", "!", "?", "¿", "¡", "\"", "'", "-", "/", "+", "*", "\n", "\r", "\t", "\\", "&", "@", "#", "$", "%", "°", "~", "}", "{", "(", ")", "[", "]", "_", "<", ">", "“", "”" };

            string resultString = text;
            resultString = " " + RemoveTildes(text) + "   ";

            foreach (string punctuation in punctuations)
            {
                if (!String.IsNullOrEmpty(symbolsToKeep) && symbolsToKeep.Contains(punctuation))
                    continue;

                resultString = resultString.Replace(punctuation, " ");
            }

            return resultString;
        }

        public string RemoveTildes(string text)
        {
            return text.ToLower().Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ü", "u").Replace("ñ", "n");
        }

        public string StemWords(string text)
        {
            // First simple and basic stemming
            return text.Replace("s ", " ").Replace("cion ", " ").Replace("erla ", "er ").Replace("erlo ", "er ").Replace("erse ", "er "); ;
        }

        public string RemoveDummies(string text)
        {
            string[] tokens = text.Split();
            string newString = "";
            foreach (string token in tokens)
                if (token.Contains("99") || token.Contains("98") || token.Contains("90"))
                    continue;
                else
                    newString += token + " ";

            return newString;
        }

        public string FirstLetterUpper(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "";

            string fixedName = rawName.ToLower();

            int index = -1;
            while (index < fixedName.Length)
            {
                string prevString = "";
                if (index > 0 && index < fixedName.Length) prevString = fixedName.Substring(0, index) + " ";
                fixedName = prevString + fixedName.Substring(index + 1, 1).ToUpper() + fixedName.Substring(index + 2);
                index = fixedName.IndexOf(" ", index + 1);
                if (index < 1) break;
            }

            return fixedName;
        }


        private List<string> LoadDataFile(string filename)
        {
            List<string> dataLines = new List<string>();
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(filename);
                string dataLine = reader.ReadLine();
                while (!string.IsNullOrEmpty(dataLine))
                {
                    dataLines.Add(dataLine);
                    dataLine = reader.ReadLine();
                }
            }
            catch (Exception)
            {
            }

            return dataLines;
        }
    }
}
