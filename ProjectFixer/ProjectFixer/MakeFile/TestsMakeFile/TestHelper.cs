using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProjectFixer.MakeFile.TestsMakeFile
{
    internal static class TestHelper
    {
        internal static IList<string> GetLinesFromResource(string resourceName)
        {
            var lines = ReadLines(() => Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream(resourceName),
                    Encoding.UTF8);
                
            return lines.ToList();
        }

        private static IEnumerable<string> ReadLines(Func<Stream> streamProvider, Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
