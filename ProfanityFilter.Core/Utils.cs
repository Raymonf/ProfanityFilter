using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ProfanityFilter.Core
{
    public static class Utils
    {
        public static readonly HashSet<string> AllowedCharacters = new HashSet<string>();

        /// <summary>
        /// Add ascii_letters, digits, and @$*"' to the allowed character list (AllowedCharacters).
        /// Then, load the unicode characters from categories Ll, Lu, Mc, Mn into AllowedCharacters.
        /// 
        /// More about Unicode categories can be found at
        /// https://en.wikipedia.org/wiki/Template:General_Category_(Unicode)
        /// </summary>
        /// <param name="dataDir">Optional directory to search for alphabetic_unicode.json in</param>
        public static void LoadUnicodeSymbols(string dataDir = null)
        {
            // We only want this to run once, ever.
            if (AllowedCharacters.Count > 0)
            {
                return;
            }
            
            // ascii_letters, digits, @$*"'
            foreach (char c in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@$*\"'")
            {
                AllowedCharacters.Add(c.ToString());
            }

            // alphabetic_unicode.json
            var path = "alphabetic_unicode.json";
            if (dataDir != null)
            {
                path = Path.Combine(dataDir + "/", path);
            }
            var unicodeData = File.ReadAllText(path);
            var unicodeStrings = JsonSerializer.Deserialize<List<string>>(unicodeData);
            foreach (var s in unicodeStrings)
            {
                AllowedCharacters.Add(s);
            }
        }
        
        /// <summary>
        /// Return the index of the first character of the next word in the given text.
        /// </summary>
        /// <param name="s">The text to work with</param>
        /// <param name="startIndex">The index to start searching at</param>
        /// <returns></returns>
        public static int GetStartIndexOfNextWord(string s, int startIndex)
        {
            var result = s.Length;

            for (var i = startIndex; i < s.Length; i++)
            {
                if (! AllowedCharacters.Contains(s[i].ToString()))
                {
                    continue;
                }

                result = i;
                break;
            }

            return result;
        }

        /// <summary>
        /// Return the next word in the given text, and the index of its last character.
        /// </summary>
        /// <param name="s">The text to work with</param>
        /// <param name="startIndex"></param>
        /// <returns>The index to start searching at</returns>
        private static (string, int) GetNextWordAndEndIndex(string s, int startIndex)
        {
            var nextWord = "";

            var i = startIndex;
            for (; i < s.Length; i++)
            {
                var c = s[i];
                if (AllowedCharacters.Contains(c.ToString()))
                {
                    nextWord += c;
                    continue;
                }

                break;
            }

            return (nextWord, i);
        }

        /// <summary>
        /// Return true and the end index of the word in the text, if any word formed in wordsIndices is in `censorWordSet`.
        /// </summary>
        /// <param name="curWord"></param>
        /// <param name="text"></param>
        /// <param name="wordsIndices"></param>
        /// <param name="censorWordSet"></param>
        /// <returns></returns>
        public static (bool, int) AnyNextWordsFormSwearWord(string curWord, string text, List<(string, int)> wordsIndices,
            HashSet<string> censorWordSet)
        {
            var fullWord = curWord.ToLowerInvariant();
            var fullWordWithSeparators = curWord.ToLowerInvariant();

            // Check both words in the pairs
            for (var i = 0; i < wordsIndices.Count; i += 2)
            {
                var (singleWord, endIndex) = wordsIndices[i];

                if (singleWord == "")
                {
                    continue;
                }
                
                var (wordWithSeparators, _) = wordsIndices[i + 1];

                fullWord = fullWord + singleWord.ToLowerInvariant();
                fullWordWithSeparators = fullWordWithSeparators + wordWithSeparators.ToLowerInvariant();

                if (censorWordSet.Contains(fullWord) || censorWordSet.Contains(fullWordWithSeparators))
                {
                    return (true, endIndex);
                }
            }

            return (false, -1);
        }

        /// <summary>
        /// Return a list of pairs of next words and next words included with separators, combined with their end indices.
        /// For example: Word `hand_job` has next words pairs: `job`, `_job`.
        /// </summary>
        /// <param name="s">Input text</param>
        /// <param name="startIndex">Index to start getting words at</param>
        /// <param name="numOfNextWords">The number of next words to get</param>
        /// <returns>A list of pairs of next words and next words included with separators</returns>
        public static List<(string, int)> GetNextWords(string s, int startIndex, int numOfNextWords = 1)
        {
            // Find the starting index of the next word
            var nextWordStartIndex = GetStartIndexOfNextWord(s, startIndex);

            // Return an empty string if there are no other words
            if (nextWordStartIndex >= s.Length - 1)
            {
                return new List<(string, int)>()
                {
                    ("", nextWordStartIndex),
                    ("", nextWordStartIndex)
                };
            }
            
            // Combine the words into a list
            var (nextWord, endIndex) = GetNextWordAndEndIndex(s, nextWordStartIndex);

            var words = new List<(string, int)>()
            {
                (nextWord, endIndex),
                (s[startIndex .. nextWordStartIndex] + nextWord, endIndex)
            };

            if (numOfNextWords > 1)
            {
                words.AddRange(GetNextWords(s, endIndex, numOfNextWords - 1));
            }

            return words;
        }
        
        /// <summary>
        /// Gets the Cartesian product of an IEnumerable 
        /// </summary>
        /// <param name="sequences">The sequences to use to get the Cartesian product</param>
        /// <typeparam name="T">The inner type of the IEnumerable</typeparam>
        /// <returns>Cartesian product</returns>
        // https://stackoverflow.com/a/3098381
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>()};
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) => 
                    from accseq in accumulator 
                    from item in sequence 
                    select accseq.Concat(new[] {item})                          
            );
        }
    }
}
