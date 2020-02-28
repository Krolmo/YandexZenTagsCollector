using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YandexZenTagsCollector
{
    public class StartingCharsGenerator
    {
        /// <summary>
        /// Generate starting symbols combinations for tags using brute force. Russian charset.
        /// </summary>
        /// <param name="startingFrom"></param>
        /// <param name="sw"></param>
        /// <param name="maxCount"></param>
        private static void makeRussianWords(string startingFrom, StreamWriter sw, int maxCount)
        {
            if (startingFrom.Length >= maxCount)
            {
                return;
            }

            for (char c = 'а'; c <= 'я'; c++)
            {
                sw.WriteLine(startingFrom + c);
                makeRussianWords(startingFrom + c, sw, maxCount);
            }
        }

        /// <summary>
        /// Generate starting symbols combinations for tags using brute force. English charset.
        /// </summary>
        /// <param name="startingFrom"></param>
        /// <param name="sw"></param>
        /// <param name="maxCount"></param>
        private static void makeEnglishWords(string startingFrom, StreamWriter sw, int maxCount)
        {
            if (startingFrom.Length >= maxCount)
            {
                return;
            }
            for (char c = 'a'; c <= 'z'; c++)
            {
                sw.WriteLine(startingFrom + c);
                makeEnglishWords(startingFrom + c, sw, maxCount);
            }

        }


        /// <summary>
        /// Generate file with all possible starting symbols for tags.
        /// </summary>
        /// <param name="maxCount">Max count of starting chars</param>
        public static void GenerateStartsFromCombinations(int maxCount, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                makeRussianWords(string.Empty, sw, maxCount);
                makeEnglishWords(string.Empty, sw, maxCount);
            }
        }
    }
}
