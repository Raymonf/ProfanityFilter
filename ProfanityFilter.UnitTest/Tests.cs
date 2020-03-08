using NUnit.Framework;
using ProfanityFilter.Core;

namespace ProfanityFilter.UnitTest
{
    public class Tests
    {
        private Profanity profanity = null;
        
        [SetUp]
        public void Setup()
        {
            profanity = new Profanity();
            // Preload censored word list
            profanity.LoadCensorWords();
        }

        [Test]
        public void TestContainsProfanity_WithProfanity()
        {
            Assert.True(profanity.ContainsProfanity("fuck"));
            Assert.True(profanity.ContainsProfanity("shit"));
            Assert.IsTrue(profanity.ContainsProfanity("fuck off twat"));
        }

        [Test]
        public void TestContainsProfanity_NoProfanity()
        {
            Assert.False(profanity.ContainsProfanity("darn"));
            Assert.False(profanity.ContainsProfanity("HECK OFF"));
            Assert.False(profanity.ContainsProfanity("heck off you poophead"));
        }

        [Test]
        public void TestCensor()
        {
            Assert.True(profanity.Censor("Censor me harder daddy") == "Censor me harder daddy");
            Assert.True(profanity.Censor("Eat shit, twat!") == "Eat ****, ****!");
        }

        [Test]
        public void TestCensorWithCustomCharacter()
        {
            Assert.True(profanity.Censor("Censor me harder daddy", '#') == "Censor me harder daddy");
            Assert.True(profanity.Censor("Eat shit, twat!", '#') == "Eat ####, ####!");
            Assert.True(profanity.Censor("no u", '#') == "no u");
        }
    }
}