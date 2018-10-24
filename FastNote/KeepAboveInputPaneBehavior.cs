using Microsoft.Xaml.Interactivity;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace FastNote
{
    public class KeepAboveInputPaneBehavior : Behavior<FrameworkElement>
    {
        private Thickness _originalMargin;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectLoaded;
            _originalMargin = AssociatedObject.Margin;
        }

        private void AssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObjectLoaded;
            InputPane.GetForCurrentView().Hiding += InputPaneHiding;
            InputPane.GetForCurrentView().Showing += InputPaneShowing;
        }

        protected override void OnDetaching()
        {
            InputPane.GetForCurrentView().Hiding -= InputPaneHiding;
            InputPane.GetForCurrentView().Showing -= InputPaneShowing;
        }

        private void InputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            AssociatedObject.Margin =
              new Thickness(_originalMargin.Left, _originalMargin.Top,
              _originalMargin.Right, _originalMargin.Bottom + args.OccludedRect.Height - 18);
        }

        private void InputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            AssociatedObject.Margin = _originalMargin;
        }
    }
}
