using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;

namespace Demo.Models
{
    public class ContentCharacterizer
    {
        private static ContentCharacterizer _single = null;

        const string _modelFolder = @"brain";
        private static string _appPath => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        private static string _modelPath => Path.Combine(_modelFolder, "DocsModel.zip");

        private MLContext _mlContext;
        private PredictionEngine<SimpleDocument, TypePrediction> _predEngine;
        private List<Tuple<string, string>> _documentTypes = new List<Tuple<string, string>>();
        private Dictionary<string, int> _months = new Dictionary<string, int>();

        private ContentCharacterizer()
        {
            _mlContext = new MLContext(seed: 0);
            ITransformer loadedModel = _mlContext.Model.Load(_modelPath, out var modelInputSchema);
            _predEngine = _mlContext.Model.CreatePredictionEngine<SimpleDocument, TypePrediction>(loadedModel);

            _documentTypes = LoadTypes();
            InitializeMonthWords();
        }

        public static ContentCharacterizer GetInstance()
        {
            if (_single == null) _single = new ContentCharacterizer();
            return _single;
        }

        private List<Tuple<string, string>> LoadTypes()
        {
            List<Tuple<string, string>> types = new List<Tuple<string, string>>();

            StreamReader reader = new StreamReader(Path.Combine("data", "Types.txt"));

            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                char[] separators = { '\t' };
                string[] tokens = line.Split(separators);
                types.Add(new Tuple<string, string>(tokens[0], tokens[1]));
            }
            reader.Close();

            return types;
        }


        public string GetDocumentType(string contents)
        {
            SimpleDocument singleDoc = new SimpleDocument() { ID = "-", Contents = contents };
            var prediction = _predEngine.Predict(singleDoc);

            return prediction.Type;
        }

        public string GetIssuingDate(string contents)
        {
            char[] separators = { ' ', '\t', '\n', '\r' };
            string[] words = contents.Split(separators);

            DateRecognizer recognizer = DateRecognizer.GetInstance();
            List<string> foundDates = recognizer.FindItems(contents);

            int firstValidDateIndex = 0;
            string foundDate = DateTime.Today.ToString("dd MMM yyyyy");
            while (firstValidDateIndex < foundDates.Count && !ContainsDate(foundDates[firstValidDateIndex]))
                firstValidDateIndex++;

            if (foundDates.Count >= 0 && firstValidDateIndex < foundDates.Count)
                foundDate = foundDates[firstValidDateIndex].Replace(",", "").Trim();

            return foundDate;
        }


        public List<string> GetNames(string contents)
        {
            string notaryName = GetNotary(contents);
            NameRecognizer recognizer = NameRecognizer.GetInstance();
            List<string> foundNames = recognizer.FindItems(contents);

            foundNames.Remove(notaryName);

            return foundNames;
        }

        public List<string> GetCompanyNames(string contents)
        {
            CompanyRecognizer recognizer = CompanyRecognizer.GetInstance();
            List<string> foundNames = recognizer.FindItems(contents);

            List<string> cleanFoundNames = new List<string>();
            foreach (string item in foundNames)
                cleanFoundNames.Add(item.Replace("POR UNA PARTE", "").Replace("Y EL", "").Replace("EL USO DE CUALQUIERA DE ESTAS EXPRESIONES EN EL TEXTO DE PRESENTE CONTRATO SE ENTENDERA REFERIDA A", ""));

            return cleanFoundNames;
        }

        public string GetNotary(string contents)
        {
            NotaryRecognizer recognizer = NotaryRecognizer.GetInstance();

            int notaryIndex = contents.ToLower().IndexOf("NOTAR");
            bool considerBeginning = notaryIndex >= 0 && notaryIndex < 100;
            List<string> foundNames = recognizer.FindItems(contents, considerBeginning);
            foundNames = recognizer.CleanNotaryNames(foundNames);

            return (foundNames.Count > 0 ? foundNames[0] : "");
        }


        public string GetConcatenatedNames(List<string> names)
        {
            string concatNames = "";

            if (names.Count == 0) return concatNames;

            foreach (string name in names)
                concatNames += name.Replace(",", "") + ", ";

            int lastCommaIndex = concatNames.LastIndexOf(",");

            return concatNames.Substring(0, lastCommaIndex).Trim();
        }

        public string GetTypeDescription(string typeID)
        {
            foreach (Tuple<string, string> typeTuple in _documentTypes)
                if (typeTuple.Item1.Equals(typeID))
                    return typeTuple.Item2;

            return "-";
        }

        public string GetDocumentDescription(string filename, string contents)
        {
            string docCharacterization = "";
            string typeId = GetDocumentType(contents);
            string typeDesc = GetTypeDescription(typeId);
            string docDate = GetIssuingDate(contents);
            string docNames = GetConcatenatedNames(GetNames(contents));

            docCharacterization = "El documento \"" + filename + "\" es de tipo " + typeDesc
                + ", emitido el " + docDate + (!String.IsNullOrEmpty(docNames) ? " y menciona a " + docNames + "." : ".");

            return docCharacterization;
        }

        private bool ContainsDate(string token)
        {
            foreach (string month in _months.Keys)
                if (token.Contains(month)) return true;

            return ContainsDigits(token);
        }
        private bool ContainsDigits(string token)
        {
            string[] digits = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

            foreach (string digit in digits)
                if (token.Contains(digit))
                    return true;

            return false;
        }



        private void InitializeMonthWords()
        {
            _months = new Dictionary<string, int>();
            _months.Add("enero", 1);
            _months.Add("ene", 1);
            _months.Add("january", 1);
            _months.Add("jan", 1);
            _months.Add("febrero", 2);
            _months.Add("feb", 2);
            _months.Add("february", 2);
            _months.Add("marzo", 3);
            _months.Add("mar", 3);
            _months.Add("march", 3);
            _months.Add("abril", 4);
            _months.Add("abr", 4);
            _months.Add("april", 4);
            _months.Add("apr", 4);
            _months.Add("mayo", 5);
            _months.Add("may", 5);
            _months.Add("junio", 6);
            _months.Add("jun", 6);
            _months.Add("june", 6);
            _months.Add("julio", 7);
            _months.Add("jul", 7);
            _months.Add("july", 7);
            _months.Add("agosto", 8);
            _months.Add("ago", 8);
            _months.Add("august", 8);
            _months.Add("aug", 8);
            _months.Add("septiembre", 9);
            _months.Add("setiembre", 9);
            _months.Add("sep", 9);
            _months.Add("set", 9);
            _months.Add("september", 9);
            _months.Add("octubre", 10);
            _months.Add("oct", 10);
            _months.Add("october", 10);
            _months.Add("noviembre", 11);
            _months.Add("nov", 11);
            _months.Add("november", 11);
            _months.Add("diciembre", 12);
            _months.Add("dic", 12);
            _months.Add("december", 12);
            _months.Add("dec", 12);
        }
    }
}
