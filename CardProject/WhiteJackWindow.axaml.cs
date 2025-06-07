using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardProject { 
public partial class WhiteJackWindow : Window
{
    ClientWebSocket socket;
        List<string> playerHand;
        private string guid;
        public string LoggedInUser;
        public string LastDiscarded { get; set; }
        public string KnockedPlayer { get; set; }
        public WhiteJackWindow()
        {
            InitializeComponent();
            ResultBox = this.FindControl<TextBlock>("ResultBox");
            KnockOrStandButton = this.FindControl<Button>("KnockOrStandButton");
            TakeButton = this.FindControl<Button>("TakeButton");
            DiscardButton = this.FindControl<Button>("DiscardButton");
            DrawButton = this.FindControl<Button>("DrawButton");
            BttPanel = this.FindControl<StackPanel>("BttPanel"); ;
            TakeButton.Click += ActionBttClick;
            KnockOrStandButton.Click += ActionBttClick;
            DrawButton.Click += ActionBttClick;
            DiscardButton.Click += DiscardBttClick;
            HandPanel = this.FindControl<StackPanel>("HandPanel");
            DiscardPile = this.FindControl<TextBlock>("DiscardPile");
            KnockInfo = this.FindControl<TextBlock>("KnockInfo");
            DataContext = this;
            InitializeSocket();
        }
    private async void ActionBttClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = (Button)sender;
        await sendAction(button.Content.ToString().ToLower());
        if(button.Name!= "KnockOrStandButton")
            {
                DiscardButton.IsEnabled = true;
            }
        }
        private async void DiscardBttClick(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var card = this.FindControl<TextBox>("DiscardTextBox").Text;
            playerHand.Remove(card);
            await sendAction(card);
            HandUpdate();
            DiscardButton.IsEnabled = false;
            BttPanel.IsVisible = true;
        }
        private async Task sendAction(string message)
     {
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
     }
        private async Task GetTurnActions()
        {
            var buffer = new byte[1024];
            while(true) { 
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                
                if (message.StartsWith("discard"))
                    {
                        LastDiscarded=message.Substring(8);
                    DiscardPile.Text = "Discarded : "+LastDiscarded;
                }
                else if (message.StartsWith("knock"))
                {
                        KnockedPlayer=message.Substring(6);
                    KnockInfo.Text = "Knocked : " + KnockedPlayer;
                        ResultBox.Text = "Player " + KnockedPlayer + " has knocked!";
                    KnockOrStandButton.Content = "Stand";
                }else if (message.StartsWith("winner"))
                    {
                        var winner = message.Substring(7);
                        ResultBox.Text = "Player " + winner + " has won!";
                        DiscardButton.IsVisible = false;
                    BttPanel.IsVisible = false;
                }else if (message.StartsWith("urturn"))
                {
                    ResultBox.Text = "Your turn!";
                }
                else if (message.Length == 2) {
                    if(!playerHand.Contains(message))
                    {
                        playerHand.Add(message);
                        HandUpdate();
                    }
                }
            }
        }
        private void HandUpdate() {
            HandPanel.Children.RemoveAll(HandPanel.Children);
            foreach (var card in playerHand)
            {
                HandPanel.Children.Add(new TextBlock { Text = card });
            }
        }
        private async Task waitForGame() {
            var buffer = new byte[1024];
            string starter = string.Empty;
            await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(LoggedInUser)), WebSocketMessageType.Text, true, CancellationToken.None);
            while (true)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer);
                if (message.StartsWith("guid"))
                {
                    guid = message.Substring(5);
                }
                else if (message.Contains(','))
                {
                    message = message.Substring(0,8);
                    var cards = message.Split(',');
                    playerHand = new List<string>(cards);
                    HandUpdate();
                    await GetTurnActions();
                    return;
                }
                else if (message.StartsWith("starter"))
                {
                    playerHand = new List<string>();
                    starter = message.Substring(8);
                    ResultBox.Text = "Game started! Starting player is" + message.Substring(8);
                    
                }
                else
                {
                    ResultBox.Text = message;
                }
            }
        }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    private async void InitializeSocket()
        {
            CookieContainer cookies = new CookieContainer();
            cookies.GetCookies(new Uri("http://localhost:2137"));
            socket = new ClientWebSocket();
            socket.Options.Cookies = cookies;
            await socket.ConnectAsync(new("ws://localhost:2137/wj/fetch"), CancellationToken.None);
            await waitForGame();
        }
}}