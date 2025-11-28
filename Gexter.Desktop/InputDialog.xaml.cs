using System.Windows;
using System.Windows.Input;

namespace Gexter.Desktop;

public partial class InputDialog : Window
{
    public string? InputValue { get; private set; }
    public bool Confirmed { get; private set; }

    public InputDialog()
    {
        InitializeComponent();
        InputTextBox.Focus();
    }

    public static (bool confirmed, string? value) Show(string prompt, string title = "Input",
        string defaultValue = "", Window? owner = null)
    {
        var dialog = new InputDialog();
        dialog.Owner = owner ?? Application.Current.MainWindow;
        dialog.TitleText.Text = title;
        dialog.PromptText.Text = prompt;
        dialog.InputTextBox.Text = defaultValue;
        dialog.InputTextBox.SelectAll();

        dialog.ShowDialog();
        return (dialog.Confirmed, dialog.InputValue);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        InputValue = InputTextBox.Text;
        Confirmed = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = false;
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Enter)
        {
            OkButton_Click(this, new RoutedEventArgs());
        }
        else if (e.Key == Key.Escape)
        {
            CancelButton_Click(this, new RoutedEventArgs());
        }
    }
}

