using EscapeViggo.Networking;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class PlaytestOptionsTests
    {
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("-1")]
        [TestCase("0")]
        [TestCase("61")]
        [TestCase("2,5")]
        public void InvalidCommandLineDurationCannotCreateAnEndlessOrInstantDay(string text)
        {
            Assert.That(PlaytestOptions.TryParseDayMinutes(text, out _), Is.False);
        }

        [Test]
        public void FractionalMinutesParseIndependentlyOfMachineLocale()
        {
            var previous = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("nb-NO");
                Assert.That(PlaytestOptions.TryParseDayMinutes("2.5", out var minutes), Is.True);
                Assert.That(minutes, Is.EqualTo(2.5));
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = previous; }
        }
    }
}
