using NUnit.Framework;
using ProfanityFilter.Core;
using System.Collections.Generic;

namespace ProfanityFilter.UnitTest
{
    public class OriginalTests
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
        public void TestContainsProfanity()
        {
            Assert.True(profanity.ContainsProfanity("he is a m0th3rf*cker"));
        }

        [Test]
        public void TestLeavesParagraphsUntouched()
        {
            var innocentText = @"If you prick us do we not bleed?
                If you tickle us do we not laugh?
                If you poison us do we not die?
                And if you wrong us shall we not revenge?";
            var censoredText = profanity.Censor(innocentText);
            Assert.AreEqual(censoredText, innocentText);
        }

        [Test]
        public void TestEmptyString()
        {
            Assert.AreEqual("", profanity.Censor(""));
        }

        [Test]
        public void TestCensorship1()
        {
            var badText = "Dude, I hate shit. Fuck bullshit.";
            var censoredText = profanity.Censor(badText);
            // make sure it finds both instances
            Assert.False(censoredText.Contains("shit"));
            // make sure it's case sensitive
            Assert.False(censoredText.Contains("fuck"));
            // make sure some of the original text is still there
            Assert.True(censoredText.Contains("Dude"));
        }

        [Test]
        public void TestCensorship2()
        {
            var badText = "That wh0re gave m3 a very good H4nd j0b, dude. You gotta check.";
            var censoredText = "That **** gave m3 a very good ****, dude. You gotta check.";
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestCensorship3()
        {
            var badText = "Those 2 girls 1 cup. You gotta check. ";
            var censoredText = "Those ****. You gotta check. ";
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestCensorship4()
        {
            var badText = "2 girls 1 cup";
            var censoredText = "****";
            Assert.IsTrue(profanity.Censor(badText) == censoredText);
        }

        [Test]
        public void TestCensorship5()
        {
            var badText = "fuck 2 girls 1 cup";
            var censoredText = "**** ****";
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestCensorshipWithStartingSwearWord()
        {
            var badText = "  wh0re gave m3 a very good H@nD j0b.";
            var censoredText = "  **** gave m3 a very good ****.";
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestCensorshipWithEndingSwearWord()
        {
            var badText = "That wh0re gave m3 a very good H@nD j0b.";
            var censoredText = "That **** gave m3 a very good ****.";
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestCensorshipEmptyText()
        {
            var emptyText = "";
            Assert.AreEqual(emptyText, profanity.Censor(emptyText));
        }

        [Test]
        public void TestCensorshipFor2Words()
        {
            var badText = "That wh0re gave m3 a very good H4nd j0b";
            var censoredText = profanity.Censor(badText);
            Assert.False(censoredText.Contains("H4nd j0b"));
            Assert.True(censoredText.Contains("m3"));
        }

        [Test]
        public void TestCensorshipForCleanText()
        {
            var clean_text = "Hi there";
            Assert.AreEqual(clean_text, profanity.Censor(clean_text));
        }

        [Test]
        public void TestCustomWordList()
        {
            var customBadWords = new List<string>()
            {
                "happy", "jolly", "merry"
            };
            profanity.LoadCensorWords(customBadWords);
            
            // make sure it doesn't find real profanity anymore
            Assert.False(profanity.ContainsProfanity("Fuck you!"));
            // make sure it finds profanity in a sentence containing custom_badwords
            Assert.True(profanity.ContainsProfanity("Have a merry day! :)"));
        }

        [Test]
        public void TestCensorshipWithoutSpaces()
        {
            var badText = "...pen1s...hello_cat_vagina,,,,qew";
            var censoredText = "...****...hello_cat_****,,,,qew";
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestCustomWords()
        {
            var badText = "supremacia ariana";
            var censoredText = "****";
            profanity.AddCensorWords(new List<string>() { badText });
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestCustomWordsDoesNotRemoveInitialWords()
        {
            var badText = "fuck and heck";
            var censoredText = "**** and heck";
            profanity.AddCensorWords(new List<string>() {"supremacia ariana"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }
    }
}