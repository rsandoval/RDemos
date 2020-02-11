using System;
using System.IO;
using System.Collections.Generic;


namespace Demo.Models
{
    public class EntityRecognizer
    {
        private List<string> _prefixes = new List<string>();
        private List<string> _suffixes = new List<string>();

        private List<string> _foundItems = new List<string>();

        protected static string _prefixesAndSuffixesFilename = "Names.txt";
        protected static string _prefixesAndSuffixesFilepath = "data";

        public List<string> Items { get { return _foundItems; } }
        public int Count {  get { return _foundItems.Count; } }

        public EntityRecognizer()
        {
            //LoadPrefixesAndSuffixes(Path.Combine(_prefixesAndSuffixesFilepath, _prefixesAndSuffixesFilename));
        }

        protected void LoadPrefixesAndSuffixes(string filepath)
        {
            bool readingPrefixesMode = true;

            StreamReader reader = new StreamReader(filepath);


            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line.Trim()) || line.Contains("#") || line.Contains("//")) continue;

                if (line.ToUpper().Contains("PREFIXES:")) { readingPrefixesMode = true; continue; }
                else if (line.ToUpper().Contains("SUFFIXES:")) { readingPrefixesMode = false; continue; }

                if (readingPrefixesMode)
                {
                    _prefixes.Add(line.Trim().ToLower());
                    _suffixes.Add(line.Trim().ToLower()); // Add to suffixes anyway
                }
                else _suffixes.Add(line.Trim().ToLower());
            }

            reader.Close();
        }

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


        public List<string> FindItems(string rawContents)
        {
            string contents = CleanString(rawContents);
            int nextItemIndex = -1;
            int lastItemIndex = -1;

            _foundItems.Clear();

            do
            {
                foreach (string prefix in _prefixes)
                {
                    nextItemIndex = contents.IndexOf(" " + prefix + " ", lastItemIndex + 1);
                    if (nextItemIndex < 0) continue;

                    // Found one, now search the end. This is the nearest suffix found
                    int startIndex = nextItemIndex + prefix.Length + 1;
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
    }
}
