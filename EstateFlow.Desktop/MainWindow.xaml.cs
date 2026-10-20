using System.Windows;
using EstateFlow.Desktop.Mvvm;

namespace EstateFlow.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}