using Avalonia.Controls;
using Avalonia;
using Avalonia.Markup.Xaml;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using static System.Net.WebRequestMethods;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;

namespace CardProject
{
    public partial class MainWindow : Window
    {
        public Uri clientUri;
        public HttpClient client;
        public CookieContainer cookies;
        public string loggedInUser;
        public MainWindow()
        {

            InitializeComponent();
            clientUri = new Uri("http://localhost:2137");
            client = new(){ BaseAddress = clientUri };
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
            RegisterButton.Click += RegisterButton_Click;
        }
        private async void RegisterButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string Username = UserNameTextBox.Text;
            string Password = PasswordTextBox.Text;


            using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                username = Username,
                password = Password,
            }),
            Encoding.UTF8,
            "application/json");

            using HttpResponseMessage responseMessage = await client.PostAsync("/auth/register", jsonContent);

            if (responseMessage != null && responseMessage.StatusCode == HttpStatusCode.OK)
            {
                loggedInUser = Username;
                GameSelectionPanel.IsVisible = true;
                LoginPanel.IsVisible = false;
            }
            else { Console.WriteLine("Username Already exists"); }
        }
        private async void LoginButton_Click(object sender,Avalonia.Interactivity.RoutedEventArgs e)
        {
            string Username = UserNameTextBox.Text;
            string Password = PasswordTextBox.Text;


            bool CheckIfAuthorized = await AuthorizeUser(Username, Password);

            if( CheckIfAuthorized )
            {
                GameSelectionPanel.IsVisible = true;
                LoginPanel.IsVisible = false;
            }
            else
            {
                Console.WriteLine("Invalid Username or Password");
            }
        }
        private async Task<bool> AuthorizeUser(string username,string password)
        {

            using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                username = username,
                password = password,
            }),
            Encoding.UTF8,
            "application/json");

            using HttpResponseMessage responseMessage = await client.PostAsync("/auth/login",jsonContent);
            
            if (responseMessage != null && responseMessage.StatusCode==HttpStatusCode.OK) { 
                return true;
                loggedInUser = username;
            }
            else { return false; }
        }

        private void WhiteJackButton_Click(object sender,Avalonia.Interactivity.RoutedEventArgs e)
        {
            var WhiteJackWindow = new WhiteJackWindow() { LoggedInUser = loggedInUser};
            WhiteJackWindow.Show();
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
            var PokerWindow = new PokerWindow();
            PokerWindow.Show();

            Console.WriteLine("Worky!");
        }

        private void HistoryButton_Click(object sender,Avalonia.Interactivity.RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow();
            historyWindow.Show();
        }
    }
}