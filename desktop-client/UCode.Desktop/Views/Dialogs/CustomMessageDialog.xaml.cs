using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace UCode.Desktop.Views.Dialogs
{
    /// <summary>
    /// Custom message dialog with modern design
    /// </summary>
    public partial class CustomMessageDialog : BaseMetroDialog
    {
        public CustomMessageDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region Dependency Properties

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(CustomMessageDialog), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty AffirmativeButtonTextProperty =
            DependencyProperty.Register(nameof(AffirmativeButtonText), typeof(string), typeof(CustomMessageDialog), new PropertyMetadata("OK"));

        public static readonly DependencyProperty NegativeButtonTextProperty =
            DependencyProperty.Register(nameof(NegativeButtonText), typeof(string), typeof(CustomMessageDialog), new PropertyMetadata("Hủy"));

        public static readonly DependencyProperty ShowNegativeButtonProperty =
            DependencyProperty.Register(nameof(ShowNegativeButton), typeof(bool), typeof(CustomMessageDialog), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(nameof(ShowIcon), typeof(bool), typeof(CustomMessageDialog), new PropertyMetadata(true));

        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.Register(nameof(DialogResult), typeof(MessageDialogResult), typeof(CustomMessageDialog), new PropertyMetadata(MessageDialogResult.Canceled));

        #endregion

        #region Properties

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public string AffirmativeButtonText
        {
            get => (string)GetValue(AffirmativeButtonTextProperty);
            set => SetValue(AffirmativeButtonTextProperty, value);
        }

        public string NegativeButtonText
        {
            get => (string)GetValue(NegativeButtonTextProperty);
            set => SetValue(NegativeButtonTextProperty, value);
        }

        public bool ShowNegativeButton
        {
            get => (bool)GetValue(ShowNegativeButtonProperty);
            set => SetValue(ShowNegativeButtonProperty, value);
        }

        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public MessageDialogResult DialogResult
        {
            get => (MessageDialogResult)GetValue(DialogResultProperty);
            set => SetValue(DialogResultProperty, value);
        }

        #endregion

        #region Event Handlers

        private void AffirmativeButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageDialogResult.Affirmative;
        }

        private void NegativeButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageDialogResult.Negative;
        }

        #endregion
    }
}
