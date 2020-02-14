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
        public string Emotion { get; set; }

        public string RawContents { get; set; }

        public MessageCharacterizer(string contents)
        {
            RawContents = contents;

            _mlContext = new MLContext(seed: 0);
            ITransformer intentLoadedModel = _mlContext.Model.Load(_intentModelPath, out var intentModelInputSchema);
            _messagePredEngine = _mlContext.Model.CreatePredictionEngine<CustomerMessage, MessageTypePrediction>(intentLoadedModel);

            ITransformer emotionLoadedModel = _mlContext.Model.Load(_emotionModelPath, out var emotionModelInputSchema);
            _commentPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, EmotionPrediction>(emotionLoadedModel);
        }

        public string GetIntention()
        {
            CustomerMessage comment = new CustomerMessage() { Contents = RawContents };
            var prediction = _messagePredEngine.Predict(comment);

            return prediction.TypeCode.ToLower();
        }

        public string GetEmotion()
        {
            Comment comment = new Comment() { Contents = RawContents };
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
            string intention = GetIntention();
            string emotion = GetEmotion();

            Random randomGen = new Random(DateTime.Now.Millisecond);
            string[] startingPhrases = { "OK. Clarísimo. Veo que es un comentario ", "Este comentario se siente como ", "De todo lo que leo, interpreto que hay un sentimiento ", "Este comentario se siente " };
            string[] connectingPhrases = { ", particularmente con una sensación de ", ". Aquí interpreto un tono de ", ", sintiendo que genera una emoción de ", ", asociándolo más bien a " };
            
            string description = startingPhrases[randomGen.Next(startingPhrases.Length)];

            if (RawContents.Length > 200)
                description = "Veo varios elementos en este comentario. Primero entiendo el comentario relacionado con ";
            else if (RawContents.Length > 320)
                description = "Bien completo el comentario. Entiendo que se se trata de ";

            return description + intention + " de un cliente con " + emotion;
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
