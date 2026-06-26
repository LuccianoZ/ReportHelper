using System.Globalization;
using ReportHelper.Converters;

namespace ReportHelper.Tests.Unit.Converters
{
    // BL-10: the 6 TimeSpan? fields on ReportHeaderViewModel (ReportTime,
    // IncidentTimeStart, IncidentTimeEnd, DispatchTime, ArrivalTime) need to bind
    // to a TextBox.Text (string), since WPF has no built-in TimePicker control.
    // This converter is the bridge. Convert = ViewModel -> UI (TimeSpan? -> string).
    // ConvertBack = UI -> ViewModel (string -> TimeSpan?), and is the "tolerant
    // parser" — it accepts both "14:30" and "2:30 PM" style input, since the
    // officer may type OR dictate via Whisper (which could transcribe either form).
    public class NullableTimeSpanToStringConverterTests
    {
        private readonly NullableTimeSpanToStringConverter _converter = new();

        // ── Convert: TimeSpan? -> string (ViewModel -> UI) ──────────────────

        [Fact]
        public void Convert_NullTimeSpan_ReturnsEmptyString()
        {
            // A field the officer hasn't touched yet must show as blank, not
            // "00:00" — that would look like a real entered midnight value.
            var result = _converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void Convert_PopulatedTimeSpan_ReturnsFormattedTimeString()
        {
            var time = new TimeSpan(14, 30, 0);

            var result = _converter.Convert(time, typeof(string), null, CultureInfo.InvariantCulture);

            // 12-hour display with AM/PM — matches how an officer would naturally
            // read a time back, rather than 24-hour "14:30".
            Assert.Equal("2:30 PM", result);
        }

        [Fact]
        public void Convert_Midnight_ReturnsTwelveAM()
        {
            // Edge case: TimeSpan.Zero must not render as "0:00 AM" — a real
            // clock reads 12:00 AM at midnight.
            var result = _converter.Convert(TimeSpan.Zero, typeof(string), null, CultureInfo.InvariantCulture);

            Assert.Equal("12:00 AM", result);
        }

        // ── ConvertBack: string -> TimeSpan? (UI -> ViewModel) ──────────────
        // This is the "tolerant parser" — it must accept multiple formats since
        // input may come from manual typing OR a Whisper transcription landing
        // in the same TextBox.

        [Theory]
        [InlineData("14:30", 14, 30)]      // 24-hour, manually typed
        [InlineData("2:30 PM", 14, 30)]     // 12-hour with AM/PM, manually typed
        [InlineData("2:30pm", 14, 30)]      // no space, lowercase — plausible transcription
        [InlineData("02:30", 2, 30)]        // leading zero, 24-hour
        public void ConvertBack_ValidFormats_ParsesToCorrectTimeSpan(string input, int expectedHour, int expectedMinute)
        {
            var result = _converter.ConvertBack(input, typeof(TimeSpan?), null, CultureInfo.InvariantCulture);

            Assert.Equal(new TimeSpan(expectedHour, expectedMinute, 0), result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ConvertBack_EmptyOrWhitespace_ReturnsNull(string input)
        {
            // Officer cleared the field (e.g. backspaced everything) — this must
            // map back to "unset", not throw and not silently keep a stale value.
            var result = _converter.ConvertBack(input, typeof(TimeSpan?), null, CultureInfo.InvariantCulture);

            Assert.Null(result);
        }

        [Fact]
        public void ConvertBack_NullInput_ReturnsNull()
        {
            // A binding can legitimately hand the converter a null string
            // (e.g. before the TextBox has any text at all) — must not throw.
            var result = _converter.ConvertBack(null, typeof(TimeSpan?), null, CultureInfo.InvariantCulture);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("not a time")]
        [InlineData("25:99")]
        [InlineData("banana")]
        public void ConvertBack_UnparseableInput_ReturnsDoNothing(string input)
        {
            // WPF's binding convention: returning Binding.DoNothing from
            // ConvertBack tells the binding engine to leave the source property
            // untouched and (with ValidatesOnExceptions/NotifyOnValidationError
            // wiring, added when this is bound in XAML) surface a validation
            // error instead of corrupting good data with a bad parse.
            var result = _converter.ConvertBack(input, typeof(TimeSpan?), null, CultureInfo.InvariantCulture);

            Assert.Equal(System.Windows.Data.Binding.DoNothing, result);
        }

        [Fact]
        public void RoundTrip_ConvertThenConvertBack_PreservesOriginalValue()
        {
            // Confidence check that the two directions are actually inverses for
            // a normal value — guards against the format string drifting out of
            // sync with the parse patterns in a future edit.
            var original = new TimeSpan(9, 5, 0);

            var asString = _converter.Convert(original, typeof(string), null, CultureInfo.InvariantCulture);
            var roundTripped = _converter.ConvertBack(asString, typeof(TimeSpan?), null, CultureInfo.InvariantCulture);

            Assert.Equal(original, roundTripped);
        }
    }
}
