using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NotepadSearcher
{
    class PrintFile
    {
        public static string Generate(string path)
        {
            path = $@"{path}";

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line = sr.ReadToEnd();
                    return line;
                }
            }

            catch (IOException e)
            {
                return e.ToString();
            }
        }
    }
}
