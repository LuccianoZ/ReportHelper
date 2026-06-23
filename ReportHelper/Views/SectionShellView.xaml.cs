using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReportHelper.ViewModels;

namespace ReportHelper.Views
{
    public partial class SectionShellView : UserControl
    {
        // StrokeDashOffset value that hides the ring completely — equals StrokeDashArray in XAML.
        // Derived from the circle geometry: diameter 130, stroke thickness 7,
        // centre radius = (130-7)/2 = 61.5px, circumference = 2π×61.5 ≈ 386px,
        // in stroke-thickness units = 386 / 7 ≈ 55.5.
        private const double RingCircumference = 55.5;

        private Storyboard? _holdStoryboard;

        public SectionShellView()
        {
            InitializeComponent();
        }

        private void HoldCancelButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // CaptureMouse ensures we receive MouseUp even if the cursor moves off the element
            // before the officer releases — prevents the animation getting stuck mid-fill.
            ((UIElement)sender).CaptureMouse();

            var animation = new DoubleAnimation
            {
                From = RingCircumference,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(4)),
                // HoldEnd keeps the ring fully drawn after completion so the officer
                // sees it complete before the navigation fires.
                FillBehavior = FillBehavior.HoldEnd
            };

            animation.Completed += OnHoldCompleted;

            _holdStoryboard = new Storyboard();
            _holdStoryboard.Children.Add(animation);

            // Target the ProgressRing ellipse named in XAML.
            Storyboard.SetTarget(animation, ProgressRing);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Shape.StrokeDashOffsetProperty));

            _holdStoryboard.Begin();
        }

        private void HoldCancelButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            ResetRing();
        }

        private void ResetRing()
        {
            // Stop() releases the Storyboard's hold on StrokeDashOffset,
            // allowing us to set it back manually.
            _holdStoryboard?.Stop();
            _holdStoryboard = null;
            ProgressRing.StrokeDashOffset = RingCircumference;
        }

        private void OnHoldCompleted(object? sender, EventArgs e)
        {
            _holdStoryboard = null;

            // Animation ran to completion — tell the ViewModel to discard the report
            // and navigate home.
            if (DataContext is SectionShellViewModel vm)
                vm.CancelReport();
        }
    }
}
