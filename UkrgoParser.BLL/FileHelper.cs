using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace UkrgoParser.BLL
{
    public static class FileHelper
    {
        private const int DefaultBufferSize = 4096;

        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        public static Task<string[]> ReadAllLinesAsync(string path)
        {
            return ReadAllLinesAsync(path, Encoding.UTF8);
        }

        public static async Task<string[]> ReadAllLinesAsync(string path, Encoding encoding)
        {
            var lines = new List<string>();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }

        public static async Task WriteAllLinesAsync(string path, IList<string> lines)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            using (var stream = File.OpenWrite(path))
            using (var writer = new StreamWriter(stream))
            {
                if (lines.Count > 0)
                {
                    for (var i = 0; i < lines.Count - 1; i++)
                    {
                        await writer.WriteLineAsync(lines[i]);
                    }
                    await writer.WriteAsync(lines[lines.Count - 1]);
                }
            }
        }
    }
}
