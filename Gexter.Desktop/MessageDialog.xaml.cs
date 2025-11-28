using FontAwesome.WPF;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Gexter.Desktop;

public enum MessageDialogType
{
    Info,
    Warning,
    Error,
    Question
}

public enum MessageDialogButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

public enum MessageDialogResult
{
    None,
    Ok,
    Cancel,
    Yes,
    No
}

public partial class MessageDialog : Window
{
    public MessageDialogResult Result { get; private set; } = MessageDialogResult.None;

    public MessageDialog()
    {
        InitializeComponent();
    }

    public static MessageDialogResult Show(string message, string title = "Message",
        MessageDialogType type = MessageDialogType.Info,
        MessageDialogButtons buttons = MessageDialogButtons.Ok,
        Window? owner = null)
    {
        var dialog = new MessageDialog();
        dialog.Owner = owner ?? Application.Current.MainWindow;
        dialog.TitleText.Text = title;
        dialog.MessageText.Text = message;

        // Set icon based on type
        switch (type)
        {
            case MessageDialogType.Info:
                dialog.IconImage.Icon = FontAwesomeIcon.InfoCircle;
                dialog.IconImage.Foreground = (Brush)Application.Current.Resources["InfoBrush"];
                break;
            case MessageDialogType.Warning:
                dialog.IconImage.Icon = FontAwesomeIcon.ExclamationTriangle;
                dialog.IconImage.Foreground = (Brush)Application.Current.Resources["WarningBrush"];
                break;
            case MessageDialogType.Error:
                dialog.IconImage.Icon = FontAwesomeIcon.TimesCircle;
                dialog.IconImage.Foreground = (Brush)Application.Current.Resources["ErrorBrush"];
                break;
            case MessageDialogType.Question:
                dialog.IconImage.Icon = FontAwesomeIcon.QuestionCircle;
                dialog.IconImage.Foreground = (Brush)Application.Current.Resources["InfoBrush"];
                break;
        }

        // Set buttons based on type
        switch (buttons)
        {
            case MessageDialogButtons.Ok:
                dialog.OkButton.Visibility = Visibility.Visible;
                dialog.CancelButton.Visibility = Visibility.Collapsed;
                dialog.YesButton.Visibility = Visibility.Collapsed;
                dialog.NoButton.Visibility = Visibility.Collapsed;
                break;
            case MessageDialogButtons.OkCancel:
                dialog.OkButton.Visibility = Visibility.Visible;
                dialog.CancelButton.Visibility = Visibility.Visible;
                dialog.YesButton.Visibility = Visibility.Collapsed;
                dialog.NoButton.Visibility = Visibility.Collapsed;
                break;
            case MessageDialogButtons.YesNo:
                dialog.OkButton.Visibility = Visibility.Collapsed;
                dialog.CancelButton.Visibility = Visibility.Collapsed;
                dialog.YesButton.Visibility = Visibility.Visible;
                dialog.NoButton.Visibility = Visibility.Visible;
                break;
            case MessageDialogButtons.YesNoCancel:
                dialog.OkButton.Visibility = Visibility.Collapsed;
                dialog.CancelButton.Visibility = Visibility.Visible;
                dialog.YesButton.Visibility = Visibility.Visible;
                dialog.NoButton.Visibility = Visibility.Visible;
                break;
        }

        dialog.ShowDialog();
        return dialog.Result;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageDialogResult.Cancel;
        Close();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageDialogResult.Ok;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageDialogResult.Cancel;
        Close();
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageDialogResult.Yes;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageDialogResult.No;
        Close();
    }
}

