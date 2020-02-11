using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace RDemosNET.Models
{
    public class CommentCharacterizer
    {
        const string _modelFolder = @"brain";
        private static string _appPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string _sentimentModelPath => Path.Combine(_modelFolder, "SentimentModel.zip");
        private static string _emotionModelPath => Path.Combine(_modelFolder, "EmotionModel.zip");
        private static string _intentionModelPath => Path.Combine(_modelFolder, "IntentionModel.zip");
        private static string _ironyModelPath => Path.Combine(_modelFolder, "IronyModel.zip");

        private MLContext _mlContext;
        private PredictionEngine<Comment, SentimentPrediction> _sentimentPredEngine;
        private PredictionEngine<Comment, EmotionPrediction> _emotionPredEngine;
        private PredictionEngine<Comment, IntentionPrediction> _intentionPredEngine;
        private PredictionEngine<Comment, IronyPrediction> _ironyPredEngine;

        public string RawContents { get; set; }

        public CommentCharacterizer(string contents)
        {
            RawContents = contents;

            _mlContext = new MLContext(seed: 0);
            ITransformer loadedSentimentModel = _mlContext.Model.Load(_sentimentModelPath, out var sentimentModelInputSchema);
            _sentimentPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, SentimentPrediction>(loadedSentimentModel);
            ITransformer loadedEmotionModel = _mlContext.Model.Load(_emotionModelPath, out var emotionModelInputSchema);
            _emotionPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, EmotionPrediction>(loadedEmotionModel);
            ITransformer loadedIntentionModel = _mlContext.Model.Load(_intentionModelPath, out var intentionModelInputSchema);
            _intentionPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, IntentionPrediction>(loadedIntentionModel);
            ITransformer loadedIronyModel = _mlContext.Model.Load(_ironyModelPath, out var ironyModelInputSchema);
            _ironyPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, IronyPrediction>(loadedIronyModel);
        }


        public string GetSentiment()
        {
            Comment comment = new Comment() { ID = "0", Contents = RawContents };
            var prediction = _sentimentPredEngine.Predict(comment);

            if (prediction.Sentiment.Contains("-"))
                return "negativo";
            else if (prediction.Sentiment.Equals("0"))
                return "neutro";
            else return "positivo";
        }

        public string GetEmotion()
        {
            Comment comment = new Comment() { ID = "0", Contents = RawContents };
            var prediction = _emotionPredEngine.Predict(comment);

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

        public string GetIntention()
        {
            Comment comment = new Comment() { ID = "0", Contents = RawContents };
            var prediction = _intentionPredEngine.Predict(comment);

            if (prediction.Intention.Contains("-"))
                return "que va a ocurrir";
            else return "que ya ocurrió";
        }

        public string GetIrony()
        {
            Comment comment = new Comment() { ID = "0", Contents = RawContents };
            var prediction = _ironyPredEngine.Predict(comment);

            if (prediction.Irony.Contains("-"))
                return "literal";
            else return "irónico";
        }


        public string GetCommentDescription()
        {
            string sentiment = GetSentiment();
            string emotion = GetEmotion();
            string intention = GetIntention();
            string irony = GetIrony();
            Random randomGen = new Random(DateTime.Now.Millisecond);
            string[] startingPhrases = { "OK. Clarísimo. Veo que es un comentario ", "Este comentario se siente como ", "De todo lo que leo, interpreto que hay un sentimiento ", "Este comentario se siente " };
            string[] connectingPhrases = { ", particularmente con una sensación de ", ". Aquí interpreto un tono de ", ", sintiendo que genera una emoción de ", ", asociándolo más bien a " };
            string description = startingPhrases[randomGen.Next(startingPhrases.Length)];

            if (emotion.Equals("enojo") || emotion.Equals("disgusto") || emotion.Equals("miedo"))
                sentiment = "negativo";
            else if (emotion.Equals("alegría"))
                sentiment = "positivo";

            if (RawContents.Length > 200)
                description = "Veo varios elementos en este comentario. Primero entiendo el comentario como ";
            else if (RawContents.Length > 320)
                description = "Bien completo el comentario. Aquí interpreto un tono ";

            description += sentiment + connectingPhrases[randomGen.Next(connectingPhrases.Length)] + emotion + " en relación a algo " + intention + ".";

            if (irony.Contains("irónico"))
                description += " Pero ojo, que es " + irony + ".";
            else if (randomGen.Next(2) == 1)
                description += " Y no veo que sea irónico.";

            return description;
        }
    }


    public class Comment
    {
        private string _contents = "";

        [LoadColumn(0)]
        public string ID { get; set; }
        [LoadColumn(1)]
        public string Contents { get { return _contents; } set { _contents = value.ToLower(); } }
        [LoadColumn(2)]
        public string Source { get; set; }
        [LoadColumn(3)]
        public string Sentiment { get; set; }
        [LoadColumn(4)]
        public string Emotion { get; set; }
        [LoadColumn(5)]
        public string Intention { get; set; }
        [LoadColumn(6)]
        public string Irony { get; set; }
    }

    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Sentiment;
    }
    public class EmotionPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Emotion;
    }
    public class IntentionPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Intention;
    }
    public class IronyPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Irony;
    }
}
