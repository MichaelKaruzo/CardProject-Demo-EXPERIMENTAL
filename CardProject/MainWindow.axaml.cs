using Avalonia.Controls;
using Avalonia;
using Avalonia.Markup.Xaml;
using System;
using System.Threading.Tasks;

namespace CardProject
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            GameSelectionPanel.IsVisible = false;
            LoginButton.Click += LoginButton_Click;
            WhiteJackButton.Click += WhiteJackButton_Click;
            WarButton.Click += WarButton_Click;
            PokerButton.Click += PokerButton_Click;
            HistoryButton.Click += HistoryButton_Click;
        }

        private async void LoginButton_Click(object sender,Avalonia.Interactivity.RoutedEventArgs e)
        {
            string Username = UserNameTextBox.Text;
            string Password = PasswordTextBox.Text;

            //BackEnd

            bool CheckIfAuthorized = await AuthorizeUser(Username, Password);

            if( CheckIfAuthorized )
            {
                GameSelectionPanel.IsVisible = true;
            }
            else
            {
                Console.WriteLine("Invalid Username or Password");
            }
        }
        private async Task<bool> AuthorizeUser(string username,string password)
        {
            //Backend shit
            return true;
        }

        private void WhiteJackButton_Click(object sender,Avalonia.Interactivity.RoutedEventArgs e)
        {
            //var WhiteJackWindow = new WhiteJackWindow();
            //WhiteJackWindow.Show();

            Console.WriteLine("Worky!");
        }
        private void WarButton_Click(object sender,Avalonia.Interactivity.RoutedEventArgs e)
        {
            var WWindow = new WarWindow();
            //BackEnd hello
            WWindow.onGameEnded = (GameResult result) =>
            {
                GameHistory.AddResult(result);
                Console.WriteLine("worky");
            };
            WWindow.Show();
        }

        private void PokerButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            //var PokerWindow = new PokerWindow();
            //PokerWindow.Show();

            Console.WriteLine("Worky!");
        }

        private void HistoryButton_Click(object sender,Avalonia.Interactivity.RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow();
            historyWindow.Show();
        }
    }
}