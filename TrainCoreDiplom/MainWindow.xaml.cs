using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TrainCoreDiplom.Pages;

namespace TrainCoreDiplom
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Открываем страницу авторизации при запуске
            MainFrame.Navigate(new LoginPage());
        }

        // Для навигации из других мест можно использовать:
        public Frame GetMainFrame()
        {
            return MainFrame;
        }
    }
}