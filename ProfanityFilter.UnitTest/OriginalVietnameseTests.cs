using NUnit.Framework;
using ProfanityFilter.Core;
using System.Collections.Generic;

namespace ProfanityFilter.UnitTest
{
    public class OriginalVietnameseTests
    {
        private Profanity profanity = null;
        
        [SetUp]
        public void Setup()
        {
            profanity = new Profanity();
            
            // These tests assume **** for every single swear filter
            // Useful function for original library compatibility too
            profanity.SetOriginalBehaviorMode(true);
            
            // Preload censored word list
            profanity.LoadCensorWords();
        }

        [Test]
        public void TestUnicodeVietnamese()
        {
            var badText = "Đây là 1 câu nói bậy.";
            var censoredText = "Đây là 1 **** nói ****.";
            profanity.LoadCensorWords(new List<string>() {"câu", "bậy"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestUnicodeVietnamese2()
        {
            var badText = "Con chó sủa gâu gâu!";
            var censoredText = "Con chó sủa **** ****!";
            profanity.LoadCensorWords(new List<string>() {"gâu"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }
    }
}