using NUnit.Framework;
using ProfanityFilter.Core;
using System.Collections.Generic;

namespace ProfanityFilter.UnitTest
{
    public class OriginalRussianTests
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
        public void TestUnicodeCensorship()
        {
            var badText = "соседский мальчик сказал хайль и я опешил.";
            var censoredText = "соседский мальчик сказал **** и я опешил.";
            profanity.LoadCensorWords(new List<string>() {"хайль"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestUnicodeCensorship2()
        {
            var badText = "Эффекти́вного противоя́дия от я́да фу́гу не существу́ет до сих пор";
            var censoredText = "Эффекти́вного **** от я́да фу́гу не существу́ет до сих пор";
            profanity.LoadCensorWords(new List<string>() {"противоя́дия"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestUnicodeCensorship3()
        {
            var badText = "Эффекти́вного противоя́дия от я́да фу́гу не существу́ет до сих пор. Но э́то не остана́вливает люде́й от употребле́ния блюд из ры́бы фу́гу.";
            var censoredText = "Эффекти́вного **** от я́да фу́гу не существу́ет до сих пор. Но э́то не остана́вливает люде́й от **** блюд из ры́бы фу́гу.";
            profanity.LoadCensorWords(new List<string>() {"противоя́дия", "употребле́ния"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestUnicodeCensorship4()
        {
            var badText = "...противоя́дия...hello_cat_употребле́ния,,,,qew";
            var censoredText = "...****...hello_cat_****,,,,qew";
            profanity.LoadCensorWords(new List<string>() {"противоя́дия", "употребле́ния"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }

        [Test]
        public void TestUnicodeCensorship5()
        {
            var badText = "Нидерла́ндах. В 18 (восемна́дцать) лет Маргаре́та вы́шла за́муж и перее́хала в Индоне́зию. Там она́ изуча́ла ме́стную культу́ру и та́нцы.";
            var censoredText = "****. В 18 (восемна́дцать) лет Маргаре́та вы́шла за́муж и **** в Индоне́зию. Там она́ изуча́ла ме́стную культу́ру и ****.";
            profanity.LoadCensorWords(new List<string>() {"шесто́м", "Нидерла́ндах", "перее́хала", "та́нцы"});
            Assert.AreEqual(censoredText, profanity.Censor(badText));
        }
    }
}