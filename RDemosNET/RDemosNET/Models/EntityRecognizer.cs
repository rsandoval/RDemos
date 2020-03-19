using System;
using System.IO;
using System.Collections.Generic;


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
            result = result.Replace("*", " ").Replace("/", " ").Replace("-", " ");
            return result;
        }

        private string SimplifyString(string contents)
        {
            string result = contents.Replace(",", ", ");
            result = result.Replace("  ", " ");
            return result;
        }


        public List<string> FindItemsSuffixPrefix(string rawContents, bool considerBeginning = false)
        {
            string contents = " " + CleanString(rawContents) + " ";
            int nextItemIndex = -1;
            int lastItemIndex = -1;

            _foundItems.Clear();

            do
            {
                bool starting = considerBeginning;
                foreach (string prefix in _prefixes)
                {
                    if (!starting)
                    {
                        nextItemIndex = contents.IndexOf(" " + prefix + " ", lastItemIndex + 1);
                        if (nextItemIndex < 0) continue;
                    }

                    // Found one, now search the end. This is the nearest suffix found
                    int startIndex = (starting ? 0 : nextItemIndex + prefix.Length + 1);
                    starting = false;
                    int endIndex = contents.Length - 1;
                    foreach (string suffix in _suffixes)
                    {
                        int auxEndIndex = contents.IndexOf(suffix + " ", startIndex + 1);
                        if (auxEndIndex < 0) continue;

                        if (auxEndIndex < endIndex)
                            endIndex = auxEndIndex;
                    }
                    if (endIndex - startIndex > 50) endIndex = startIndex + 50;
                    lastItemIndex = endIndex;

                    string itemText = rawContents.Substring(startIndex, endIndex - startIndex).Trim();
                    if (startIndex >= 0 && endIndex > startIndex && !_foundItems.Contains(itemText))
                        _foundItems.Add(itemText);
                    break;
                }
            } while (nextItemIndex >= 0); 

            return _foundItems;
        }

        public List<string> FindItemsWith(string rawContents)
        {
            string contents = " " + CleanString(rawContents) + " ";
            int nextItemIndex = -1;
            int lastItemIndex = -1;

            _foundItems.Clear();

            do
            {
                foreach (string token in _endsWith)
                {
                    nextItemIndex = contents.IndexOf(" " + token + " ", lastItemIndex + 1);
                    if (nextItemIndex < 0) continue;

                    // Found one, now search backwards, to the start. 
                    int startIndex = nextItemIndex - (token.Length + 1);
                    int endIndex = contents.Length - 1;
                    foreach (string suffix in _suffixes)
                    {
                        int auxEndIndex = contents.IndexOf(suffix + " ", startIndex + 1);
                        if (auxEndIndex < 0) continue;

                        if (auxEndIndex < endIndex)
                            endIndex = auxEndIndex;
                    }
                    if (endIndex - startIndex > 50) endIndex = startIndex + 50;
                    lastItemIndex = endIndex;

                    string itemText = rawContents.Substring(startIndex, endIndex - startIndex).Trim();
                    if (startIndex >= 0 && endIndex > startIndex && !_foundItems.Contains(itemText))
                        _foundItems.Add(itemText);
                    break;
                }
            } while (nextItemIndex >= 0);

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
            for (int i = 0; i < names.Count; i++)
                names[i] = CleanNotaryName(_foundItems[i]);

            return names;
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
    }
}
