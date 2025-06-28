using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NotepadSearcher
{
    class FillListBox
    {
        public static string[] PathList(string userpath)
        {
            string dirName = Path.GetDirectoryName(userpath);

            string[] files = Directory.GetFiles(dirName, "*.txt");

            return files;
        }

        public static List<string> FileNameList(string[] files)
        {
            List<string> fileNames = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                string path = files.ElementAt(i);
                fileNames.Add(Path.GetFileNameWithoutExtension(path).ToString());
            }

            return fileNames;
        }
    }
}
