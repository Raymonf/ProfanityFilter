using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ProfanityFilter.Core
{
    public class Profanity
    {
        /// <summary>
        /// The max number of additional words forming a swear word. For example:
        /// hand job = 1
        /// this is a fish = 3
        /// </summary>
        private int maxNumberCombinations = 1;
        
        /// <summary>
        /// The hash set to store the censored words in
        /// </summary>
        private HashSet<string> CensorWordSet = null;
        
        // Characters for creating variants
        private readonly Dictionary<char, List<char>> _charsMapping =
            new Dictionary<char, List<char>>()
            {
                { 'a', new List<char>() { 'a', '@', '*', '4' } },
                { 'i', new List<char>() { 'i', '*', 'l', '1' } },
                { 'o', new List<char>() { 'o', '*', '0', '@' } },
                { 'u', new List<char>() { 'u', '*', 'v' } },
                { 'v', new List<char>() { 'v', '*', 'u' } },
                { 'l', new List<char>() { 'l', '1' } },
                { 'e', new List<char>() { 'e', '*', '3' } },
                { 's', new List<char>() { 's', '$', '5' } }
            };

        // Data directory, in case the user wants to override the data path
        private string dataDir = null;
        
        /// <summary>
        /// The profanity filter
        /// </summary>
        /// <param name="dataDir">Optional directory to search for alphabetic_unicode.json and profanity_wordlist.txt in</param>
        public Profanity(string dataDir = null)
        {
            this.dataDir = dataDir;
            Utils.LoadUnicodeSymbols(dataDir);
        }

        /// <summary>
        /// This flag is used mostly for unit test compatibility.
        /// If you need exact compatibility with the Python better_profanity,
        /// you should set this to true. 
        /// </summary>
        private bool originalBehaviorMode = false;
        
        /// <summary>
        /// Sets the original behavior mode flag 
        /// </summary>
        /// <param name="value">originalBehaviorMode value</param>
        public void SetOriginalBehaviorMode(bool value)
        {
            originalBehaviorMode = value;
        }

        private int CountNonAllowedCharacters(string word)
        {
            var count = 0;
            
            foreach (var c in word)
            {
                if (!Utils.AllowedCharacters.Contains(c.ToString()))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Adds words to the censored word set.
        /// </summary>
        /// <param name="newWords">An IEnumerable<string> of words to add</param>
        public void AddCensorWords(IEnumerable<string> newWords)
        {
            foreach (var word in newWords)
            {
                CensorWordSet.Add(word);
            }
        }
        
        /// <summary>
        /// Sets the censored word list.
        /// If `customWords` is null, we'll just read the word list from profanity_wordlist.txt. 
        /// </summary>
        /// <param name="customWords">An IEnumerable<string> of words to add</param>
        public void LoadCensorWords(IEnumerable<string> customWords = null)
        {
            var tempWords = customWords ?? ReadWordList();

            var allCensorWords = new List<string>();
            foreach (var word in tempWords)
            {
                var tempWord = word.ToLowerInvariant();
                var nonAllowedCharacterCount = CountNonAllowedCharacters(word);
                if (nonAllowedCharacterCount > maxNumberCombinations)
                {
                    maxNumberCombinations = nonAllowedCharacterCount;
                }
                
                allCensorWords.AddRange(GeneratePatternsFromWord(tempWord));
            }

            CensorWordSet = allCensorWords.ToHashSet();
        }

        /// <summary>
        /// Return words from file `profanity_wordlist.txt`.
        /// </summary>
        /// <returns>A list of words</returns>
        private List<string> ReadWordList()
        {
            var path = "profanity_wordlist.txt";
            if (dataDir != null)
            {
                path = Path.Combine(dataDir + "/", path);
            }
            
            return File.ReadAllLines(path, Encoding.UTF8)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        private List<string> GeneratePatternsFromWord(string word)
        {
            var combos = new List<List<char>>();
            foreach (var c in word)
            {
                combos.Add(!_charsMapping.ContainsKey(c) ? new List<char>() { c } : _charsMapping[c]);
            }

            var product = combos.CartesianProduct();
            return product.Select(combo => string.Join("", combo)).ToList();
        }

        private string GetReplacementForSwearWord(char censorChar, int count = 4)
        {
            // 4 is hardcoded for original behavior mode
            if (originalBehaviorMode)
            {
                return new string(censorChar, 4);                
            }
            
            return new string(censorChar, count);
        }

        /// <summary>
        /// Return true if the input text has any swear words.
        /// </summary>
        /// <param name="s">Input text</param>
        /// <returns>If the input text has any swear words</returns>
        public bool ContainsProfanity(string s)
        {
            return s != Censor(s);
        }

        /// <summary>
        /// Return a list of next wordsIndices after the input index.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="wordsIndices">Original wordsIndices</param>
        /// <param name="startIndex">The index to start searching at</param>
        /// <returns></returns>
        private List<(string, int)> UpdateNextWordsIndices(string text, List<(string, int)> wordsIndices, int startIndex)
        {
            // Python: not set() = true
            if (wordsIndices == null || wordsIndices.Count < 1)
            {
                wordsIndices = Utils.GetNextWords(text, startIndex, maxNumberCombinations);
            }
            else
            {
                wordsIndices.RemoveRange(0, 2);
                if (wordsIndices.Count > 0 && wordsIndices[^1].Item1 != "")
                {
                    wordsIndices.AddRange(Utils.GetNextWords(text, wordsIndices[^1].Item2, 1));
                }
            }

            return wordsIndices;
        }

        /// <summary>
        /// Replace the swear words with censor characters.
        /// </summary>
        /// <param name="s">The input text</param>
        /// <param name="censorChar">The character to censor text with</param>
        /// <returns></returns>
        private string HideSwearWords(string s, char censorChar)
        {
            var nextWordStartIndex = Utils.GetStartIndexOfNextWord(s, 0);

            // If there are no words in the text, return the raw text without parsing
            if (nextWordStartIndex >= s.Length - 1)
            {
                return s;
            }
            
            var censoredText = "";
            var curWord = "";
            int skipIndex = -1;
            var nextWordsIndices = new List<(string, int)>();
            
            // Left strip the text, to avoid inaccurate parsing
            if (nextWordStartIndex > 0)
            {
                censoredText = s[..nextWordStartIndex];
                s = s[nextWordStartIndex..];
            }
            
            // Splitting each word in the text to compare with censored words
            for (var i = 0; i < s.Length; i++)
            {
                if (i < skipIndex)
                    continue;
                
                char? c = s[i];
                if (Utils.AllowedCharacters.Contains(c.ToString()))
                {
                    curWord += c;
                    continue;
                }
                
                // Skip continuous non-allowed characters
                if (curWord.Trim() == "")
                {
                    censoredText += c;
                    curWord = "";
                    continue;
                }
                
                // Iterate the next words combined with the current one
                // to check if it forms a swear word
                nextWordsIndices = UpdateNextWordsIndices(s, nextWordsIndices, i);
                var (containsSwearWord, endIndex) = Utils.AnyNextWordsFormSwearWord(
                    curWord, s, nextWordsIndices, CensorWordSet
                );

                if (containsSwearWord)
                {
                    curWord = GetReplacementForSwearWord(censorChar, curWord.Length);
                    skipIndex = endIndex;
                    c = null;
                    nextWordsIndices.Clear();
                }
                
                // If the current word is a swear word
                if (CensorWordSet.Contains(curWord.ToLowerInvariant()))
                {
                    curWord = GetReplacementForSwearWord(censorChar, curWord.Length);
                }

                censoredText += curWord;
                
                if (c.HasValue)
                {
                    censoredText += c;
                }

                curWord = "";
            }
            
            // Final check
            if (curWord != "" && skipIndex < s.Length - 1)
            {
                if (CensorWordSet.Contains(curWord.ToLowerInvariant()))
                {
                    curWord = GetReplacementForSwearWord(censorChar, curWord.Length);
                }

                censoredText += curWord;
            }

            return censoredText;
        }
        
        /// <summary>
        /// Replace the swear words in the text with `censorChar`.
        /// </summary>
        /// <param name="s">The input text</param>
        /// <param name="censorChar">The character to censor text with</param>
        /// <returns></returns>
        public string Censor(string s, char censorChar = '*')
        {
            if (CensorWordSet == null)
            {
                LoadCensorWords();
            }

            return HideSwearWords(s, censorChar);
        }
    }
}
