using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace RDemosNET.Models
{
    public class BankMessageCharacterizer
    {
        const string _modelFolder = @"brain";
        private static string _appPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string _intentModelPath => Path.Combine(_modelFolder, "BankCommentTypeModel.zip");
        private static string _subintentModelPath => Path.Combine(_modelFolder, "BankCommentSubtypeModel.zip");
        private static string _emotionModelPath => Path.Combine(_modelFolder, "EmotionModel.zip");
        private static string _intentionModelPath => Path.Combine(_modelFolder, "IntentionModel.zip");

        private MLContext _mlContext;
        private PredictionEngine<BankCustomerMessage, BankMessageTypePrediction> _messagePredEngine;
        private PredictionEngine<BankCustomerMessage, BankMessageSubtypePrediction> _messageSubtypePredEngine;
        private PredictionEngine<Comment, EmotionPrediction> _commentPredEngine;
        private PredictionEngine<Comment, IntentionPrediction> _commentIntentionPredEngine;

        public string Intent { get; set; }
        public string Subintent { get; set; }
        public string Emotion { get; set; }
        public string Intention { get; set; }

        public string RawContents { get; set; }

        public BankMessageCharacterizer(string contents)
        {
            RawContents = contents;

            _mlContext = new MLContext(seed: 0);
            ITransformer intentLoadedModel = _mlContext.Model.Load(_intentModelPath, out var intentModelInputSchema);
            _messagePredEngine = _mlContext.Model.CreatePredictionEngine<BankCustomerMessage, BankMessageTypePrediction>(intentLoadedModel);
            ITransformer subintentLoadedModel = _mlContext.Model.Load(_subintentModelPath, out var subintentModelInputSchema);
            _messageSubtypePredEngine = _mlContext.Model.CreatePredictionEngine<BankCustomerMessage, BankMessageSubtypePrediction>(subintentLoadedModel);

            Intent = GetIntent();
            Subintent = GetSubintent();

            ITransformer emotionLoadedModel = _mlContext.Model.Load(_emotionModelPath, out var emotionModelInputSchema);
            _commentPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, EmotionPrediction>(emotionLoadedModel);
            ITransformer intentionLoadedModel = _mlContext.Model.Load(_emotionModelPath, out var intentionModelInputSchema);
            _commentIntentionPredEngine = _mlContext.Model.CreatePredictionEngine<Comment, IntentionPrediction>(intentionLoadedModel);

            Emotion = GetEmotion();
            Intention = GetIntention();
        }

        public string GetIntent()
        {
            BankCustomerMessage comment = new BankCustomerMessage() { Contents = RawContents };
            var prediction = _messagePredEngine.Predict(comment);

            string strIntent = prediction.TypeCode.ToString().ToLower();

            return DescriptionsManager.GetInstance().GetDescription(strIntent);
        }

        public string GetSubintent()
        {
            BankCustomerMessage comment = new BankCustomerMessage() { Contents = RawContents };
            var prediction = _messageSubtypePredEngine.Predict(comment);

            string strSubintent = prediction.SubTypeCode.ToString().ToLower();

            return strSubintent;
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

        public string GetIntention()
        {
            //Comment comment = new Comment() { ID = "0", Contents = RawContents };
            //var prediction = _commentIntentionPredEngine.Predict(comment);

            //if (prediction.Intention.Contains("-"))
            //    return "que va a ocurrir";
            //else return "que ya ocurrió";
            string strContents = TextNormalizer.GetInstance().CleanString(RawContents);
            if (strContents.Contains("ar ") || strContents.Contains("er ") || strContents.Contains("rias ") || strContents.Contains("ras "))
                return "que pide que ocurra";
            else return "que ya ocurrió";
        }
        public string GetDescription()
        {
            string intent = Intent.ToLower();
            string subintent = Subintent;
            string emotion = Emotion;
            string intention = Intention;

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

            if (intent.StartsWith("consulta") || intent.StartsWith("queja"))
            {
                description += "a <b>" + intent + "</b>, subcódigo <b>" + subintent + "</b>, " + intention;
                if (randomGen.Next(2) == 1 && (emotion != "enojo" && !RawContents.ToLower().Contains("gracia") && !RawContents.ToLower().Contains("cordiale")))
                    description += " y lo pide con " + emotion + ".";
            }
            else if (intent.Contains("reclamo") || intent.Contains("queja"))
            {
                description = startingPhrases[randomGen.Next(2)] + " <b>" + intent + "</b>";
                if (!String.IsNullOrEmpty(subintent))
                    description += " respecto a un código <b>" + subintent + "</b>, " + intention;
                description += ". Nótese que se siente " + emotion + " en el mensaje.";
            }
            else if (!String.IsNullOrEmpty(subintent))
            {
                description += " <b>" + intent + "</b> de un" + (subintent.EndsWith("a") ? "a " : " ") + "<b>" + subintent + "</b>";
            }
            else
            {
                description += " <b>" + intent + "</b> de algo indeterminado, " + intention;
            }

            return description;
        }

        public string GetSimpleDescription()
        {
            string intent = Intent;
            string subintent = Subintent;
            string emotion = Emotion;
            string intention = Intention;

            string description = "Solicitud: <b>" + intent + "</b><br />"
                + "Subcódigo: <b>" + subintent + "</b><br />"
                + "Emoción: <b>" + emotion + "</b><br />"
                + "Intención: <b>" + intention + "</b>"; 
            return description;
        }
    }


    public class DescriptionsManager
    {
        const string _dataFolder = @"data";
        private static string _appPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string _intentDescriptions => Path.Combine(_dataFolder, "BankIntentDescription.txt");

        private static DescriptionsManager _single = null;
        List<Tuple<int, string>> _codesAndDescriptions = new List<Tuple<int, string>>();

        private DescriptionsManager()
        {
            _codesAndDescriptions = LoadDataFile(_intentDescriptions);
        }

        public static DescriptionsManager GetInstance()
        {
            if (_single == null) _single = new DescriptionsManager();
            return _single;
        }

        private List<Tuple<int, string>> LoadDataFile(string filename)
        {
            List<Tuple<int, string>> pairs = new List<Tuple<int, string>>();
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(filename);
                string dataLine = reader.ReadLine();
                while (!string.IsNullOrEmpty(dataLine))
                {
                    char[] separator = { ';' };
                    string[] tokens = dataLine.Split(separator);
                    int code = 0; int.TryParse(tokens[0], out code);
                    Tuple<int, string> pair = new Tuple<int, string>(code, tokens[1]);
                    pairs.Add(pair);
                    dataLine = reader.ReadLine();
                }
            }
            catch (Exception)
            {
            }

            return pairs;
        }

        public string GetDescription(int code)
        {
            foreach (Tuple<int, string> pair in _codesAndDescriptions)
                if (code == pair.Item1)
                    return pair.Item2;

            return "-";
        }

        public string GetDescription(string strCode)
        {

            int code = 0; int.TryParse(strCode, out code);
            if (code >= 0)
                return GetDescription(code);

            return "-";
        }
    }

    public class BankCustomerMessage
    {
        private string _cleanContents = "";
        // public string OriginalContents = "";

        [LoadColumn(0)]
        public string Contents { get { return _cleanContents; } set { _cleanContents = CleanContent(value); } }
        [LoadColumn(1)]
        public string TypeCode { get; set; }
        [LoadColumn(2)]
        public string SubTypeCode { get; set; }

        public string CleanContent(string contents)
        {
            TextNormalizer normalizer = TextNormalizer.GetInstance();
            contents = normalizer.CleanString(contents);
            return contents;
        }
    }

    public class BankMessageTypePrediction
    {
        [ColumnName("PredictedLabel")]
        public string TypeCode;
    }

    public class BankMessageSubtypePrediction
    {
        [ColumnName("PredictedLabel")]
        public string SubTypeCode;
    }
}
