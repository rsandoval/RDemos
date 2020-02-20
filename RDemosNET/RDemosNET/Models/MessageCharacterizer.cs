using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace RDemosNET.Models
{
    public class MessageCharacterizer
    {
        const string _modelFolder = @"brain";
        private static string _appPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string _intentModelPath => Path.Combine(_modelFolder, "CustomerCommentModel.zip");
        private static string _emotionModelPath => Path.Combine(_modelFolder, "EmotionModel.zip");

        private MLContext _mlContext;
        private PredictionEngine<CustomerMessage, MessageTypePrediction> _messagePredEngine;
        private PredictionEngine<Comment, EmotionPrediction> _commentPredEngine;

        public string Intent { get; set; }
        public string MessageObject { get; set; }
        public string Emotion { get; set; }

        public string RawContents { get; set; }

        public MessageCharacterizer(string contents)
        {
            RawContents = contents;

            _mlContext = new MLContext(seed: 0);
            ITransformer intentLoadedModel = _mlContext.Model.Load(_intentModelPath, out var intentModelInputSchema);
            _messagePredEngine = _mlContext.Model.CreatePredictionEngine<CustomerMessage, MessageTypePrediction>(intentLoadedModel);

            Intent = GetIntent();
            MessageObject = GetMessageObject();

            ITransformer emotionLoadedModel = _mlContext.Model.Load(_emotionModelPath, out var emotionModelInputSchema);
            _commentPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, EmotionPrediction>(emotionLoadedModel);

            Emotion = GetEmotion();
        }

        public string GetIntent()
        {
            CustomerMessage comment = new CustomerMessage() { Contents = RawContents };
            var prediction = _messagePredEngine.Predict(comment);

            string strIntent = prediction.TypeCode.ToString().ToLower();

            if (strIntent.EndsWith("ar"))
                strIntent = strIntent.Replace("ar", "ación");

            return strIntent;
        }

        public string GetMessageObject()
        {
            TextNormalizer textNormalizer = TextNormalizer.GetInstance();
            string strContents = textNormalizer.CleanString(RawContents);
            
            string strObject = "";

            if (strContents.Contains("sim ") || strContents.Contains("simcard")) strObject += "simcard ";
            if (strContents.Contains("bolsa ") || strContents.Contains("agregar")) strObject += "bolsa internet ";
            if (strContents.Contains("buzon")) strObject += "buzón de voz ";
            if (strContents.Contains("costo") && strContents.Contains("salida")) strObject += "costo de salida ";
            if (strContents.Contains("sva")) strObject += "datos y SVA ";
            if (strContents.Contains("desvio")) strObject += "desvío de llamadas ";
            if (strContents.Contains("equipo")) strObject += "equipo ";
            if (strContents.Contains("privado")) strObject += "id privado ";
            if (strContents.Contains("gameloft") || strContents.Contains("game loft")) strObject += "gameloft ";
            if (strContents.Contains("game")) strObject += "juego ";
            if (strContents.Contains("conferencia")) strObject += "conferencia ";
            if (strContents.Contains("plan") && !strContents.Contains("plan de dato")) strObject += "plan ";
            if (strContents.Contains("plan de dato")) strObject += "plan de datos ";
            if (strContents.Contains("llamada")) strObject += "llamada ";
            if (strContents.Contains("razon social")) strObject += "razón social ";
            if (strContents.Contains("llamada espera")) strObject += "llamada en espera ";
            if (strContents.Contains(" sm ")) strObject += "SMS ";
            if (strContents.Contains("roaming")) strObject += "roaming ";
            if (strContents.Contains("trafico internacional") || strContents.Contains("distancia internacional") || strContents.Contains(" ldi ")) strObject += "tráfico internacional ";
            if (strContents.Contains("bam ")) strObject += "BAM ";

            if (String.IsNullOrEmpty(strObject) && (strContents.Contains("linea") || strContents.Contains("numeracion"))) strObject += "línea ";

            return strObject.Trim();
        }

        public string GetEmotion()
        {
            TextNormalizer textNormalizer = TextNormalizer.GetInstance();
            Comment comment = new Comment() { Contents = textNormalizer.CleanString(RawContents) };
            var prediction = _commentPredEngine.Predict(comment);

            switch (prediction.Emotion)
            {
                case "0": return "alegría";
                case "1": return "pena";
                case "2": return "miedo";
                case "3": return "disgusto";
                case "4": return "sorpresa";
                case "5": return "enojo";
            }

            return "-";
        }

        public string GetDescription()
        {
            string intent = Intent;
            string messageObject = MessageObject;
            string emotion = Emotion;

            Random randomGen = new Random(DateTime.Now.Millisecond);
            string[] startingPhrases = {
                "Ojo, que el cliente está presentando un",
                "Este es claramente un",
                "OK. En este caso el cliente está pidiendo un",
                "El cliente necesita un",
                "La petición del cliente es de un",
                "Aquí se está pidiendo un" };
            string[] connectingPhrases = {
                " de ",
                " y ojo con la "};
            
            string description = startingPhrases[randomGen.Next(startingPhrases.Length - 1) + 1]; // index = 0 es reclamos

            if (intent.Contains("reclamo"))
            {
                description = startingPhrases[randomGen.Next(2)] + " <b>" + intent + "</b>";
                if (!String.IsNullOrEmpty(messageObject))
                    description += " respecto a un" + (messageObject.EndsWith("a") ? "a " : " ") + " <b>" + messageObject + "</b>";
                description += ". Nótese que se siente " + emotion + " en el mensaje.";
            }
            else if (intent.EndsWith("a") || intent.EndsWith("ción") || intent.EndsWith("tud"))
            {
                description += "a <b>" + intent + "</b> de un" + (messageObject.EndsWith("a") ? "a " : " ") + "<b>" + messageObject + "</b>";
                if (randomGen.Next(2) == 1 && (emotion != "enojo" && !RawContents.ToLower().Contains("gracia") && !RawContents.ToLower().Contains("cordiale")))
                    description += " y lo pide con " + emotion + ".";
            }
            else if (!String.IsNullOrEmpty(messageObject))
            {
                description += " <b>" + intent + "</b> de un" + (messageObject.EndsWith("a") ? "a " : " ") + "<b>" + messageObject + "</b>";
            }
            else 
            {
                description += " <b>" + intent + "</b> de algo indeterminado";
            }

            return description;
        }

    }

    public class CustomerMessage
    {
        private string _cleanContents = "";

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
