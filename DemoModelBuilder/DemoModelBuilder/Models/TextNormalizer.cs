﻿////////////////////////////////////////////////////////////////////////////////////////////////////
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
using System.Collections.Generic;
using System.Text;

namespace DemoModelBuilder.Models
{
    public class TextNormalizer
    {
        private static TextNormalizer _single = null;
        public static string BasePath = @"C:\Users\rodri\Documents\GitHub\RDemos\DemoModelBuilder\DemoModelBuilder";

        List<string> _stopWords = new List<string>();
        List<string> _irrelevantExpressions = new List<string>();

        private TextNormalizer()
        {
            _stopWords = LoadDataFile(BasePath + @"\data\StopWords.txt");
            _irrelevantExpressions = LoadDataFile(BasePath + @"\data\IrrelevantText.txt");
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

        public string RemovePunctuation(string text)
        {
            string[] punctuations = { ".", ",", ";", ":", "!", "?", "¿", "¡", "\"", "'", "-", "/", "+", "*", "\n", "\r", "\t", "\\", "&", "@", "#", "$", "%", "°", "~", "}", "{", "(", ")", "[", "]", "_", "<", ">", "“", "”" };

            string resultString = text;
            resultString = " " + RemoveTildes(text) + "   ";

            foreach (string punctuation in punctuations)
                resultString = resultString.Replace(punctuation, " ");

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
                if (token.Contains("99") || token.Contains("98"))
                    continue;
                else
                    newString += token + " ";

            return newString;
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
            catch (Exception e)
            {
            }

            return dataLines;
        }
    }
}
