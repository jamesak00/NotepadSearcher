using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NotepadSearcher
{
    class SearchObj
    {
        public static List<string> SearchTextResults(string query, string path) 
        {
            string[] files = Directory.GetFiles(path, "*.txt");
            List<string> searchResults = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                using (StreamReader sr = new StreamReader(files.ElementAt(i)))
                {
                    string text = sr.ReadToEnd();

                    if (text.Contains(query))
                    {
                        searchResults.Add(files.ElementAt((i)));
                    }
                }   
            }

            return searchResults;
        }
    }
}
