using System;
using System.IO;
using System.Collections.Generic;
using RDemosNET.Models;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace Demo.Models
{
    public class EntityRecognizer
    {
        private List<string> _prefixes = new List<string>();
        private List<string> _suffixes = new List<string>();
        private List<string> _startsWith = new List<string>();
        private List<string> _endsWith = new List<string>();

        protected List<string> _foundItems = new List<string>();

        protected static string _prefixesAndSuffixesFilename = "Names.txt";
        protected static string _prefixesAndSuffixesFilepath = "data";

        public List<string> Items { get { return _foundItems; } }
        public int Count {  get { return _foundItems.Count; } }


        #region Constructor and Initiation
        public EntityRecognizer()
        {
            //LoadPrefixesAndSuffixes(Path.Combine(_prefixesAndSuffixesFilepath, _prefixesAndSuffixesFilename));
        }

        protected void LoadPrefixesAndSuffixes(string filepath)
        {
            int readingBlock = 1; // 1: PREFIXES  2:SUFFIXES   3: STARSWITH    4: ENDSWITH

            StreamReader reader = new StreamReader(filepath);

            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()) || line.Contains("#") || line.Contains("//")) continue;

                if (line.ToUpper().Contains("PREFIXES:")) { readingBlock = 1; continue; }
                else if (line.ToUpper().Contains("SUFFIXES:")) { readingBlock = 2; continue; }
                else if (line.ToUpper().Contains("STARTSWITH:")) { readingBlock = 3; continue; }
                else if (line.ToUpper().Contains("ENDSWITH:")) { readingBlock = 4; continue; }

                if (readingBlock == 1)
                {
                    _prefixes.Add(line.Trim().ToLower());
                    _suffixes.Add(line.Trim().ToLower()); // Add to suffixes anyway
                }
                else if (readingBlock == 2) _suffixes.Add(line.Trim().ToLower());
                else if (readingBlock == 3) _startsWith.Add(line.Trim().ToLower());
                else if (readingBlock == 4) _endsWith.Add(line.Trim().ToLower());
            }

            reader.Close();
        }

        #endregion

        private string CleanString(string contents)
        {
            string result = contents.ToLower().Trim().Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
            result = result.Replace("*", " ").Replace("/", " ").Replace(". ", " ").Replace("  ", " ").Trim();
            return result;
        }

        private string SimplifyString(string contents)
        {
            string result = contents.Replace(",", ", ");
            result = result.Replace("  ", " ");
            return result;
        }

        private List<int> IndicesOf(string contents, string search)
        {
            List<int> indicesFound = new List<int>();

            int lastFoundIndex = -1;
            do
            {
                lastFoundIndex = contents.IndexOf(search, lastFoundIndex + 1);
                if (lastFoundIndex >= 0)
                    indicesFound.Add(lastFoundIndex);
            } while (lastFoundIndex >= 0);

            return indicesFound;
        }


        public List<string> FindItemsSuffixPrefix(string rawContents, bool considerBeginning = false)
        {
            string contents = " " + CleanString(rawContents) + " ";
            int nextItemIndex = -1;
            int lastItemIndex = -1;

            _foundItems.Clear();

            do
            {
                foreach (string prefix in _prefixes)
                {
                    lastItemIndex = 0;
                    do
                    {
                        nextItemIndex = contents.IndexOf(" " + prefix + " ", lastItemIndex + 1);
                        if (nextItemIndex < 0) break;

                        // Found one, now search the end. This is the nearest suffix found
                        int startIndex = nextItemIndex + prefix.Length + 1;
                        int endIndex = contents.Length - 1;
                        foreach (string suffix in _suffixes)
                        {
                            int auxEndIndex = contents.IndexOf(suffix + " ", startIndex + 1);
                            if (auxEndIndex < 0) continue;

                            if (auxEndIndex < endIndex && auxEndIndex > startIndex)
                                endIndex = auxEndIndex;
                        }
                        if (endIndex - startIndex > 50) endIndex = startIndex + 50;
                        lastItemIndex = endIndex;

                        string itemText = contents.Substring(startIndex, endIndex - startIndex).Trim();
                        if (startIndex >= 0 && endIndex > startIndex && !_foundItems.Contains(itemText))
                            _foundItems.Add(itemText);
                    } while (nextItemIndex >= 0);
                }
            } while (nextItemIndex >= 0); 

            return _foundItems;
        }



        public List<string> FindItemsWith(string rawContents)
        {
            string contents = CleanString(rawContents) + " ";

            _foundItems.Clear();

            // Design note: since this method was first implemented for finding company names
            // it made sense to start with the ending tokens, such as ("s.a.", "ltda.") and then
            // move backwards to find the beginning, since these names are somewhate harder to find.
            foreach (string endToken in _endsWith)
            {
                List<int> indicesFound = IndicesOf(contents, endToken);
                foreach (int index in indicesFound)
                {
                    int endIndex = index + endToken.Length;
                    int startIndex = endIndex - 150; // Items to find shouldn't be longer than 150 characters (or could they?)
                    if (startIndex < 0) startIndex = 0;

                    bool foundOne = false;
                    foreach (string startToken in _startsWith)
                    {
                        int auxStartIndex = contents.IndexOf(startToken, startIndex + 1);
                        if (auxStartIndex < 0 || auxStartIndex > index) continue;

                        foundOne = true;
                        string itemText = contents.Substring(auxStartIndex, endIndex - auxStartIndex).Trim();
                        if (!_foundItems.Contains(itemText.ToUpper()))
                            _foundItems.Add(itemText.ToUpper());
                        break;
                    }

                    if (foundOne) continue;

                    // Didn't find any _startsWith, now let's use prefixes to find the beginning
                    foreach (string prefix in _prefixes)
                    {
                        int auxStartIndex = contents.IndexOf(" " + prefix, startIndex + 1);
                        if (auxStartIndex < 0 || auxStartIndex > index) continue;

                        startIndex = auxStartIndex + prefix.Length + 1;

                        foundOne = true;
                        string itemText = contents.Substring(startIndex, endIndex - startIndex).Trim();
                        if (!_foundItems.Contains(itemText.ToUpper()))
                            _foundItems.Add(itemText.ToUpper());
                        break;
                    }
                } // foreach (int index in indicesFound)
            } // foreach (string endToken in _endsWith)

            return _foundItems;
        }

        public List<string> FindItems(string rawContents, bool considerBeginning = false)
        {
            if (_endsWith.Count > 0 || _startsWith.Count > 0)
                return FindItemsWith(rawContents);
            else
                return FindItemsSuffixPrefix(rawContents, considerBeginning);
        }
    }

    public class NameRecognizer : EntityRecognizer
    {
        private static NameRecognizer _single = null;
        public static NameRecognizer GetInstance()
        {
            if (_single == null) _single = new NameRecognizer();
            return _single;
        }

        private NameRecognizer()
        {
            _prefixesAndSuffixesFilename = "Names.txt";
            LoadPrefixesAndSuffixes(Path.Combine(_prefixesAndSuffixesFilepath, _prefixesAndSuffixesFilename));
        }
        public List<string> CleanNames(List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
                names[i] = CleanName(_foundItems[i]);

            return names;
        }

        private string CleanName(string name)
        {
            name = name.ToLower();
            name = name.Replace(" en adelante ", "");
            name = name.Replace(" entre ", "");

            if (name.Contains(" en santiago "))
                name = name.Substring(0, name.IndexOf(" en santiago "));

            return TextNormalizer.GetInstance().FirstLetterUpper(name.Trim());
        }
    }

    public class NotaryRecognizer : EntityRecognizer
    {
        private static NotaryRecognizer _single = null;
        public static NotaryRecognizer GetInstance()
        {
            if (_single == null) _single = new NotaryRecognizer();
            return _single;
        }

        private NotaryRecognizer()
        {
            _prefixesAndSuffixesFilename = "NotaryNames.txt";
            LoadPrefixesAndSuffixes(Path.Combine(_prefixesAndSuffixesFilepath, _prefixesAndSuffixesFilename));
        }

        public List<string> CleanNotaryNames(List<string> names)
        {
            List<string> finalNames = new List<string>();

            for (int i = 0; i < names.Count; i++)
                if (IsValidNotaryName(names[i]))
                    finalNames.Add(TextNormalizer.GetInstance().FirstLetterUpper(CleanNotaryName(names[i])));

            return finalNames;
        }

        private bool IsValidNotaryName(string name)
        {
            string lowerName = name.ToLower();

            if (lowerName.Contains("de santiago") || lowerName.Contains("de stgo") || lowerName.Contains("notari"))
                return false;

            return true;
        }

        private string CleanNotaryName(string rawNotaryName)
        {
            string cleanNotaryName = "";
            string[] tokens = rawNotaryName.Split();

            if (tokens.Length > 0) cleanNotaryName += tokens[0] + " ";
            if (tokens.Length > 1) cleanNotaryName += tokens[1] + " ";
            if (tokens.Length > 2) cleanNotaryName += tokens[2];

            return cleanNotaryName.Trim();
        }

    }
    public class DateRecognizer : EntityRecognizer
    {
        private static DateRecognizer _single = null;
        public static DateRecognizer GetInstance()
        {
            if (_single == null) _single = new DateRecognizer();
            return _single;
        }

        private DateRecognizer()
        {
            _prefixesAndSuffixesFilename = "Dates.txt";
            LoadPrefixesAndSuffixes(Path.Combine(_prefixesAndSuffixesFilepath, _prefixesAndSuffixesFilename));
        }

        public List<string> RemoveNonValidDates(List<string> dates)
        {
            List<string> validDates = new List<string>();

            foreach (string date in dates)
                if (IsValidDate(date))
                    validDates.Add(CleanDateString(date));

            return validDates;
        }

        private bool IsValidDate(string textDate)
        {
            string[] validMonths = { "enero", "ene", "febrero", "feb", "marzo", "mar", "abril", "abr", "mayo", "may", "junio", "jun", "julio", "jul", "agosto", "ago", "septiembre", "setiembre", "sep", "octubre", "oct", "noviembre", "nov", "diciembre", "dic" };
            List<string> validDays = new List<string>();

            foreach (string validMonth in validMonths)
                if (textDate.ToLower().Contains(" " + validMonth + " "))
                    return true;

            return false;
        }

        private string CleanDateString(string dateString)
        {
            dateString = " " + dateString + " ";
            dateString = dateString.Replace(", ", " ");
            dateString = dateString.Replace("santiago", "");
            dateString = dateString.Replace(" ano ", " ");
            dateString = dateString.Replace(" de ", " ");
            dateString = dateString.Replace(" dias ", " ");
            dateString = dateString.Replace(" del ", " ");
            dateString = dateString.Replace(" mes ", " ");

            dateString = dateString.Replace("mil novecientos", "19");
            dateString = dateString.Replace("dos mil ", "20");

            dateString = dateString.Replace("treinta y uno ", "31 ");
            dateString = dateString.Replace("treinta ", "30 ");
            dateString = dateString.Replace("veintinueve ", "29 ");
            dateString = dateString.Replace("veintiocho ", "28 ");
            dateString = dateString.Replace("veintisiete ", "27 ");
            dateString = dateString.Replace("veintiseis ", "26 ");
            dateString = dateString.Replace("veinticinco ", "25 ");
            dateString = dateString.Replace("veintiseis ", "26 ");
            dateString = dateString.Replace("veinticinco ", "25 ");
            dateString = dateString.Replace("veinticuatro ", "24 ");
            dateString = dateString.Replace("veintitres ", "23 ");
            dateString = dateString.Replace("veintidos ", "22 ");
            dateString = dateString.Replace("veintiuno ", "21 ");
            dateString = dateString.Replace("veinte ", "20 ");
            dateString = dateString.Replace("diecinueve ", "19 ");
            dateString = dateString.Replace("dieciocho ", "18 ");
            dateString = dateString.Replace("diecisiete ", "17 ");
            dateString = dateString.Replace("dieciseis ", "16 ");
            dateString = dateString.Replace("quince ", "15 ");
            dateString = dateString.Replace("catorce ", "14 ");
            dateString = dateString.Replace("trece ", "13 ");
            dateString = dateString.Replace("doce ", "12 ");
            dateString = dateString.Replace("once ", "11 ");
            dateString = dateString.Replace("diez ", "10 ");
            dateString = dateString.Replace("nueve ", "09 ");
            dateString = dateString.Replace("ocho ", "08 ");
            dateString = dateString.Replace("siete ", "07 ");
            dateString = dateString.Replace("seis ", "06 ");
            dateString = dateString.Replace("cinco ", "05 ");
            dateString = dateString.Replace("cuatro ", "04 ");
            dateString = dateString.Replace("tres ", "03 ");
            dateString = dateString.Replace("dos ", "02 ");
            dateString = dateString.Replace("uno ", "01 ");

            int lastDigitIndex = LastDigitIndex(dateString);
            if (lastDigitIndex > 10)
                dateString = dateString.Substring(0, lastDigitIndex + 1).Trim();

            return dateString.Trim();
        }

        private int LastDigitIndex(string date)
        {
            string[] digits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            int lastIndex = -1;

            foreach(string digit in digits)
            {
                int index = date.LastIndexOf(digit);
                if (index > lastIndex)
                    lastIndex = index;
            }

            return lastIndex;
        }
    }


    public class CompanyRecognizer : EntityRecognizer
    {
        private static CompanyRecognizer _single = null;
        public static CompanyRecognizer GetInstance()
        {
            if (_single == null) _single = new CompanyRecognizer();
            return _single;
        }

        private CompanyRecognizer()
        {
            _prefixesAndSuffixesFilename = "CompanyNames.txt";
            LoadPrefixesAndSuffixes(Path.Combine(_prefixesAndSuffixesFilepath, _prefixesAndSuffixesFilename));
        }

        public List<string> CleanNames(List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
                names[i] = CleanName(_foundItems[i]);

            return names;
        }

        private string CleanName(string name)
        {
            name = name.ToUpper();
            name = name.Replace("EN ADELANTE ", "").Replace(" ENTRE ", " ").Replace("POR UNA PARTE", "").Replace("Y EL", "").Replace("EL USO DE CUALQUIERA DE ESTAS EXPRESIONES EN EL TEXTO DE PRESENTE CONTRATO SE ENTENDERA REFERIDA A", "");

            if (name.Contains(" EN SANTIAGO "))
                name = name.Substring(0, name.IndexOf(" EN SANTIAGO "));


            return name;
        }
    }


    public class RUTRecognizer : EntityRecognizer
    {
        private static RUTRecognizer _single = null;
        public static RUTRecognizer GetInstance()
        {
            if (_single == null) _single = new RUTRecognizer();
            return _single;
        }

        private RUTRecognizer()
        {
            _prefixesAndSuffixesFilename = "RUT.txt";
            LoadPrefixesAndSuffixes(Path.Combine(_prefixesAndSuffixesFilepath, _prefixesAndSuffixesFilename));
        }

        public List<string> CleanElements(List<string> elements)
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i] = Clean(_foundItems[i]);

            return elements;
        }

        private string Clean(string element)
        {
            element = element.Replace("numero", "").Replace("N°", "").Replace("num ", "").Trim();
            return element;
        }
    }
}
