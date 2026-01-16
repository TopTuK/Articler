using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Helpers
{
    public static class TextChunker
    {
        public static IEnumerable<string> ChunkText(string text, int chunkSize = 4000, int overlap = 200)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            if (chunkSize <= 0)
                throw new ArgumentException("Chunk size must be greater than zero.", nameof(chunkSize));

            if (overlap < 0 || overlap >= chunkSize)
                throw new ArgumentException("Overlap must be non-negative and less than chunk size.", nameof(overlap));

            int step = Math.Max(1, chunkSize - overlap);

            for (int i = 0; i < text.Length; i += step)
            {
                int end = Math.Min(i + chunkSize, text.Length);
                yield return text[i..end];
            }
        }
    }
}
