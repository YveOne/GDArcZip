using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Utils
{
    internal class Funcs
    {
        public static string GetTempFileName()
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }

        public static IEnumerable<string> ReadFileLinesIter(string file)
        {
            foreach (var line in File.ReadAllLines(file))
            {
                yield return line;
            }
        }

        public static IEnumerable<string> ReadTextLinesIter(string text)
        {
            using (StringReader sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static IEnumerable<string> ReadStreamLinesIter(Stream stream, string encodingName = "UTF-8")
        {
            var encoding = Encoding.GetEncoding(encodingName);
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static bool GetKeyValueFromString(string line, out KeyValuePair<string, string> kvp, char splitBy = '=')
        {
            kvp = new KeyValuePair<string, string>();
            var split = line.Split(splitBy);
            if (split.Length < 2) return false;
            var key = split[0].Trim();
            if (key == "") return false;
            if (key.StartsWith("#")) return false;
            if (key.StartsWith("//")) return false;
            if (key.StartsWith("--")) return false;
            var val = String.Join("=", split.Skip(1).ToArray()).Trim();
            kvp = new KeyValuePair<string, string>(key, val);
            return true;
        }

        public delegate IEnumerable<string> ReadDictionaryIterDelegate();

        public static Dictionary<string, string> ReadDictionaryFromIter(ReadDictionaryIterDelegate iter, char splitby = '=')
        {
            var dict = new Dictionary<string, string>();
            foreach (var line in iter())
            {
                if (GetKeyValueFromString(line, out var kvp, splitby))
                    dict[kvp.Key] = kvp.Value;
            }
            return dict;
        }

        public static Dictionary<string, string> ReadDictionaryFromFile(string file, char splitby = '=')
        {
            return ReadDictionaryFromIter(() => ReadFileLinesIter(file), splitby);
        }

        public static Dictionary<string, string> ReadDictionaryFromText(string text, char splitby = '=')
        {
            return ReadDictionaryFromIter(() => ReadTextLinesIter(text), splitby);
        }

        public static Dictionary<string, string> ReadDictionaryFromStream(Stream stream, char splitby = '=')
        {
            return ReadDictionaryFromIter(() => ReadStreamLinesIter(stream), splitby);
        }

        public static string[] StringLines(string str)
            => str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

    }

}
