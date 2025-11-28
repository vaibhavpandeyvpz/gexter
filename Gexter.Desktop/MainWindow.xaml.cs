using FontAwesome.WPF;
using Gexter.Desktop.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Gexter.Desktop;

public partial class MainWindow : Window
{
    private MainViewModel ViewModel => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();

        // Handle command line arguments (file path)
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && System.IO.File.Exists(args[1]))
        {
            Loaded += (s, e) => ViewModel.LoadFile(args[1]);
        }

        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        // Update maximize/restore icon
        MaximizeIcon.Icon = WindowState == WindowState.Maximized
            ? FontAwesomeIcon.WindowRestore
            : FontAwesomeIcon.WindowMaximize;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            MaximizeButton_Click(sender, new RoutedEventArgs());
        }
        else if (e.LeftButton == MouseButtonState.Pressed)
        {
            // If maximized, restore and then drag
            if (WindowState == WindowState.Maximized)
            {
                var point = e.GetPosition(this);
                WindowState = WindowState.Normal;
                Left = point.X - (Width / 2);
                Top = point.Y - 16;
            }
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0)
            {
                var file = files[0];
                if (file.EndsWith(".gxt", StringComparison.OrdinalIgnoreCase))
                {
                    ViewModel.LoadFile(file);
                }
                else
                {
                    MessageDialog.Show("Please drop a .gxt file.", "Invalid File",
                        MessageDialogType.Warning, MessageDialogButtons.Ok, this);
                }
            }
        }
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0 && files[0].EndsWith(".gxt", StringComparison.OrdinalIgnoreCase))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Focus search box on Ctrl+F
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
        }
    }
}
