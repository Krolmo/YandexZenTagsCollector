using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace YandexZenTagsCollector
{
    class Program
    {
        private static int _theadsCount = 4;
        private static int _delay = 50;
        private static int _wordsLength = 6;
        private static string[] _vocabNames = new string[] { "russian.txt", "russian_surnames.txt" };

        private static Random _rnd = new Random();
        private static HashSet<string> _results = new HashSet<string>();
        private static HashSet<string> _errors = new HashSet<string>();
        private static readonly object _lock = new object();

        private static List<string> _vocabulary = new List<string>();

        private static void getTags(int threadIndex)
        {


            Regex _extractor = new Regex("https:\\/\\/zen.yandex.ru\\/t\\/([^\"]*?)\\?clid=", RegexOptions.Compiled);
            using (var client = new WebClient())
            {
                for (int idx = threadIndex; idx < _vocabulary.Count; idx = idx + _theadsCount)
                {
                    string resultFromYandex;
                    string escapedChars = Uri.EscapeUriString(_vocabulary[idx]);
                    //Sorry for ugly coding it is 3AM now
                    Exception currentException = new Exception("Dummy exception");
                    int tryNumber = 0;
                    bool hasError = true;
                    while (currentException != null && tryNumber < 5)
                    {
                        try
                        {
                            using (var stream = client.OpenRead("https://zen.yandex.ru/api/v3/launcher/suggest?from_person=&clid=10000&country_code=ru&lang=ru&page_type=catalog_suggest&search_text=" + escapedChars + "&rnd=" + _rnd.Next(1000000000, 1999999999)))
                            using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
                            {
                                resultFromYandex = textReader.ReadToEnd();
                                Match match = _extractor.Match(resultFromYandex);
                                while (match.Success)
                                {
                                    string tag = Uri.UnescapeDataString(match.Groups[1].Value);
                                    lock (_lock)
                                    {
                                        _results.Add(tag);
                                    }
                                    match = match.NextMatch();
                                }
                            }
                            currentException = null;
                            hasError = false;
                        }
                        catch (Exception ex)
                        {
                            currentException = ex;
                            tryNumber++;
                        }

                    }

                    if (hasError)
                    {
                        _errors.Add(_vocabulary[idx]);
                    }


                    if (_delay > 0)
                    {
                        System.Threading.Thread.Sleep(_delay);
                    }
                }

            }
        }

        #region Filling words beginings for generating requests to Zen

        private static string filterLine(string line)
        {
            StringBuilder result = new StringBuilder(line.Length);
            string lowerCaseLine = line.ToLower();
            foreach (var chr in lowerCaseLine)
            {
                if (char.IsLetter(chr))
                {
                    result.Append(chr);
                }
            }
            return result.ToString();
        }
        
        private static void readFromVocabs()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            HashSet<string> startingSymbols = new HashSet<string>();

            foreach (var item in _vocabNames)
            {
                using (StreamReader sr = new StreamReader("Vocabs\\"+item, Encoding.GetEncoding(1251)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = filterLine(line);
                        string substr;
                        if (line.Length <= _wordsLength)
                        {
                            substr = line;
                            startingSymbols.Add(substr);
                        } else
                        {
                            substr = line.Substring(0, _wordsLength);
                            startingSymbols.Add(substr);
                        }
                        //Adding also all starting symbols of this word, expecting presence some new words out of dictionary
                        if (substr.Length > 1)
                        {
                            for (int i = 1; i< substr.Length; i++)
                            {
                                startingSymbols.Add(substr.Substring(0,i));
                            }
                        }
                    }
                }
            }

            _vocabulary = startingSymbols.OrderBy(m => m).ToList();
        }

        #endregion

        static void Main(string[] args)
        {
            ///Filling words beginings
            readFromVocabs();

            //Starting threads to milk Zen
            Task[] tasks = new Task[_theadsCount];
            for (int i = 0; i < _theadsCount; i++)
            {
                //We need to create separate parameter, otherwise the loop will change parameter passsed into thread run function
                int parameter = i;
                tasks[i] = Task.Run(() => getTags(parameter));
            }

            Task.WaitAll(tasks);
            using (StreamWriter sw = new StreamWriter("results.txt"))
            {
                foreach (var item in _results)
                {
                    sw.WriteLine(item);
                }
            }

            using (StreamWriter sw = new StreamWriter("errors.txt"))
            {
                foreach (var item in _errors)
                {
                    sw.WriteLine(item);
                }
            }

        }
    }
}
