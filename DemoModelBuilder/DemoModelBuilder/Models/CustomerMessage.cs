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
//  CustomerMessage.cs
//  This class handles the logic of customer messages in natural language to build classification models using ML.NET.
//
//  .NET Project:       Demos
//  Company Project:    Demos
//  Creado/Created:     feb 2020 - Rodrigo Sandoval (rodrigo.sandoval@rsolver.com)
//  
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ML.Data;

namespace DemoModelBuilder.Models
{
    public class CustomerMessage
    {
        private string _cleanContents = "";
        // public string OriginalContents = "";

        [LoadColumn(0)]
        public string Contents { get { return _cleanContents; } set { _cleanContents = CleanContent(value); } }
        [LoadColumn(1)]
        public string Type { get; set; }
        [LoadColumn(2)]
        public string TypeCode { get; set; }
        [LoadColumn(3)]
        public string Object { get; set; }
        [LoadColumn(4)]
        public string ObjectCode { get; set; }

        public string CleanContent(string contents)
        {
            TextNormalizer normalizer = TextNormalizer.GetInstance();
            contents = normalizer.CleanString(contents);
            return contents;
        }


    }

    public class MessageTypePrediction
    {
        [ColumnName("PredictedLabel")]
        public string TypeCode;
    }

    public class MessageObjectPrediction
    {
        [ColumnName("PredictedLabel")]
        public string ObjectCode;
    }


}
