using System.Windows;

namespace QuickSetup.Logic.Infra
{
    /// <summary>
    /// http://stackoverflow.com/a/3329467/426315
    /// </summary>
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
            {
                // This will work when using ShowDialog():
                // window.DialogResult = e.NewValue as bool?;

                // we are using non-model window - Show()
                // so we are not actually using the DialogResult
                // and we will close the window no matter what is the actual value
                //var newValue = e.NewValue as bool?;
                window.Close();
            }
        }

        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}