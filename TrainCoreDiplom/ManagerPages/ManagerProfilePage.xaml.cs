using System;
using System.Windows;
using System.Windows.Controls;

namespace TrainCoreDiplom.ManagerPages
{
    public partial class ManagerProfilePage : Page
    {
        public ManagerProfilePage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (App.CurrentUser != null)
            {
                LoginText.Text = App.CurrentUser.Login;
                EmailText.Text = App.CurrentUser.Email;
                RoleText.Text = "Менеджер";
                LastLoginText.Text = App.CurrentUser.LastLogin?.ToString("dd.MM.yyyy HH:mm") ?? "никогда";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}