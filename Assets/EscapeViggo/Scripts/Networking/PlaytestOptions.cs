using System.Globalization;

namespace EscapeViggo.Networking
{
    public static class PlaytestOptions
    {
        public static bool TryParseDayMinutes(string text, out double minutes) =>
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out minutes) &&
            !double.IsNaN(minutes) && !double.IsInfinity(minutes) && minutes >= 0.1 && minutes <= 60;
    }
}
