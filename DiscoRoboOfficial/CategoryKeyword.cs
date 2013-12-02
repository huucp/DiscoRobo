using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace DiscoRoboOfficial
{
    public class Keyword
    {
        public string Word { get; set; }
        public string Category { get; set; }
        public Keyword(string word, string category)
        {
            Word = word;
            Category = category;
        }

        public static List<Keyword> LoadKeywordsFromFile(string filename)
        {
            var listKeys = new List<Keyword>();
            var resrouceStream = Application.GetResourceStream(new Uri(filename, UriKind.Relative));
            if (resrouceStream != null)
            {
                Stream myFileStream = resrouceStream.Stream;
                if (myFileStream.CanRead)
                {
                    var myStreamReader = new StreamReader(myFileStream);

                    while (!myStreamReader.EndOfStream)
                    {
                        string line = myStreamReader.ReadLine(); // category name line
                        string category = ExtractCategoryName(line);

                        line = myStreamReader.ReadLine(); // keywords line
                        string[] keywords = Regex.Split(line, " / ");
                        foreach (var keyword in keywords)
                        {
                            string word = keyword.Trim().ToLower();
                            listKeys.Add(new Keyword(word, category));
                        }

                        myStreamReader.ReadLine(); // blank space line
                    }
                }
            }

            // Sort list descending base on key word length
            var lengths = from element in listKeys
                          orderby element.Word.Length descending
                          select element;
            return lengths.ToList();
        }

        private static string ExtractCategoryName(string line)
        {
            return line.Remove(0, 3).Trim().ToLower();
        }
    }
}
