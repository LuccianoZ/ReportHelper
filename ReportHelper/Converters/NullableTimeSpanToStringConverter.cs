using System.Globalization;
using System.Windows.Data;

namespace ReportHelper.Converters
{
    // Bridges TimeSpan? (the ViewModel's natural type for a time-of-day value)
    // to string (what a TextBox.Text binding needs), and back.
    //
    // WHY THIS EXISTS: WPF has no built-in TimePicker control (only DatePicker).
    // Rather than pull in a third-party control library for one field shape, the
    // chosen approach (confirmed during BL-10 planning) is a plain TextBox with
    // a tolerant string parser — consistent with this app's broader voice-first
    // design, where free-text fields already accept both typed and dictated
    // input through the same TextBox.
    //
    // USAGE: declared once in App.xaml as a StaticResource (same scope rule as
    // BoolToVis — converters must live at the Application.Resources level so
    // every section view can reference {StaticResource TimeSpanConverter}
    // regardless of where that view sits in the visual tree), then applied to
    // each TimeSpan? property's binding:
    //   Text="{Binding ReportTime, Converter={StaticResource TimeSpanConverter}}"
    public class NullableTimeSpanToStringConverter : IValueConverter
    {
        // ViewModel -> UI. TimeSpan? -> string.
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not TimeSpan timeSpan)
            {
                // Covers both a true null (field never set) and any unexpected
                // non-TimeSpan value — in both cases, showing blank is correct:
                // a blank TextBox reads as "not entered yet", which is accurate.
                return string.Empty;
            }

            // Build a DateTime purely to borrow its well-tested 12-hour-clock
            // formatting (TimeSpan itself has no "h:mm tt" format specifier).
            // The date portion is irrelevant and discarded immediately.
            var asDateTime = DateTime.Today.Add(timeSpan);

            // "h:mm tt" -> e.g. "2:30 PM", "12:00 AM" for midnight. This is how
            // an officer would naturally read a time aloud, unlike 24-hour
            // "14:30", which is why this was chosen over TimeSpan.ToString().
            return asDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
        }

        // UI -> ViewModel. string -> TimeSpan?. This is the "tolerant parser":
        // it must accept whatever lands in the TextBox, whether the officer
        // typed it directly or it arrived via a Whisper.NET transcription of
        // spoken input (e.g. "two thirty PM").
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var text = value as string;

            if (string.IsNullOrWhiteSpace(text))
            {
                // Officer cleared the field entirely — that's a deliberate
                // "unset" action, not a parse failure, so it maps cleanly to
                // null rather than falling through to the DoNothing path below.
                return null;
            }

            // DateTime.TryParse (not TimeSpan.TryParse) is the right tool here:
            // TimeSpan.TryParse expects duration syntax like "14:30:00" and
            // rejects "2:30 PM" outright, since AM/PM isn't a duration concept.
            // DateTime.TryParse already understands both "14:30" (24-hour) and
            // "2:30 PM" / "2:30pm" (12-hour, with or without a space) out of the
            // box via its normal culture-aware parsing — exactly the two input
            // shapes this field needs to accept.
            if (DateTime.TryParse(text, culture, DateTimeStyles.None, out var parsed))
            {
                return parsed.TimeOfDay;
            }

            // Unparseable input (e.g. "banana", "25:99"). Binding.DoNothing is
            // the WPF-sanctioned way to say "leave the source property exactly
            // as it was" — it does NOT throw, and it does NOT write a bad value
            // over a good one. Pairing this binding with
            // ValidatesOnExceptions/NotifyOnValidationError in XAML (a View-layer
            // concern, added when this converter is wired up) is what surfaces
            // a visible validation error to the officer instead of silently
            // discarding their keystrokes.
            return Binding.DoNothing;
        }
    }
}
