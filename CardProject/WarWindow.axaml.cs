using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CardProject
{

    public partial class WarWindow : Window
    {
        private Button drawButton;
        private TextBlock playerCardTBox;
        private TextBlock computerCardTBox;
        private TextBlock ResultTBox;
        private TextBlock playerDeckTBox;
        private TextBlock computerDeckTBox;
        private List<Card> PlayerDeck;
        private List<Card> ComputerDeck;
        public Action<GameResult> onGameEnded;

        private enum WarEvent_Phases { None, Start, Reveal, End };
        private WarEvent_Phases CurrentPhase = WarEvent_Phases.None;
        private List<Card> WarPile = new List<Card>();

        private bool GameOver = false;
        private int RoundsPlayed = 0;
        public WarWindow()
        {
            InitilalizeComponent();

            drawButton = this.FindControl<Button>("DrawButton");
            playerCardTBox = this.FindControl<TextBlock>("PlayerCard");
            computerCardTBox = this.FindControl<TextBlock>("ComputerCard");
            ResultTBox = this.FindControl<TextBlock>("ResultBox");
            playerDeckTBox = this.FindControl<TextBlock>("PlayerDeckCount");
            computerDeckTBox = this.FindControl<TextBlock>("ComputerDeckCount");

            drawButton.Click += DrawButton_Click;

            InitializeGame();

        }
        private void InitilalizeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void InitializeGame()
        {
            List<Card> deck = await RequestCardsFromServer();
            PlayerDeck = new List<Card>();
            ComputerDeck = new List<Card>();
            WarPile = new List<Card>();
            CurrentPhase = WarEvent_Phases.None;
            RoundsPlayed = 0;
            GameOver = false;

            playerCardTBox.Text = "";
            computerCardTBox.Text = "";
            ResultTBox.Text = "";

            for (int i = 0; i < deck.Count; i++)
            {
                if (i % 2 == 0)
                {
                    PlayerDeck.Add(deck[i]);
                }
                else
                {
                    ComputerDeck.Add(deck[i]);
                }
            }
            UpdateGameUI();
        }

        private async Task<List<Card>> RequestCardsFromServer()
        {
            //BAAAAAACKEND RAH
            //For sake of testin 'er
            List<Card> deck = new List<Card>();
            string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            foreach (string suit in suits)
            {
                foreach (string value in values)
                {
                    deck.Add(new Card { Suit = suit, Value = value });
                }
            }

            Random Ran = new Random();

            for(int i = deck.Count - 1; i > 0; i--)
            {
                int j = Ran.Next(i + 1);
                Card temp = deck[j];
                deck[j] = deck[i];
                deck[i] = temp; 
            }
            return deck;
        }

        private void UpdateGameUI()
        {
            playerDeckTBox.Text = $"Player cards: {PlayerDeck.Count}";
            computerDeckTBox.Text = $"Computer cards: {ComputerDeck.Count}";
        }

        private async void DrawButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (GameOver)
            {
                drawButton.Click -= DrawButton_Click;
                ResetGame();
                return;
            }
            if (PlayerDeck.Count == 0 || ComputerDeck.Count == 0)
            {
                CheckWhoWon();
                return;
            }

            if (CurrentPhase != WarEvent_Phases.None)
            {
                await WarEvent();
                return;
            }


            Card playerCard = PlayerDeck[0];
            Card computerCard = ComputerDeck[0];

            playerCardTBox.Text = $"{playerCard.Value} of {playerCard.Suit}";
            computerCardTBox.Text = $"{computerCard.Value} of {computerCard.Suit}";

            PlayerDeck.RemoveAt(0);
            ComputerDeck.RemoveAt(0);
            RoundsPlayed++;

            int Compare = CompareCards(playerCard, computerCard);

            WarPile = new List<Card> { playerCard, computerCard }; 

            if(Compare > 0)
            {
                ResultTBox.Text = "Player Won the Round!";
                PlayerDeck.AddRange(WarPile);
            }
            else if(Compare < 0)
            {
                ResultTBox.Text = "Computer Won the Round!";
                ComputerDeck.AddRange(WarPile);
            }
            else
            {
                ResultTBox.Text = "Tie... War Press Draw to Continue!";
                CurrentPhase = WarEvent_Phases.Start;
            }
            UpdateGameUI();


            if (PlayerDeck.Count == 0 || ComputerDeck.Count == 0)
            {
                CheckWhoWon();
                return;
            }
        }

        private async Task WarEvent()
        {
            switch (CurrentPhase)
            {
                case WarEvent_Phases.Start:
                    if(PlayerDeck.Count < 4 || ComputerDeck.Count < 4)
                    {
                        ResultTBox.Text = "Not enough Cards for full war.";
                        if(PlayerDeck.Count < ComputerDeck.Count)
                        {
                            ComputerDeck.AddRange(WarPile);
                            ComputerDeck.AddRange(PlayerDeck);
                            PlayerDeck.Clear();
                        }
                        else
                        {
                            PlayerDeck.AddRange(WarPile);
                            PlayerDeck.AddRange(ComputerDeck);
                            ComputerDeck.Clear();
                        }
                        CurrentPhase = WarEvent_Phases.None;
                        UpdateGameUI();
                        CheckWhoWon();
                        return;
                    }
                    for(int i = 0; i < 3; i++)
                    {
                        WarPile.Add(PlayerDeck[0]);
                        PlayerDeck.RemoveAt(0);
                        WarPile.Add(ComputerDeck[0]);
                        ComputerDeck.RemoveAt(0);
                    }

                    ResultTBox.Text = "Each side place 3 face-down cards. Click to reveal war cards.";
                    CurrentPhase = WarEvent_Phases.Reveal;
                    break;

                case WarEvent_Phases.Reveal:

                    if (PlayerDeck.Count == 0 || ComputerDeck.Count == 0)
                    {
                        ResultTBox.Text = "Not enough Cards to complete War.";
                        if(PlayerDeck.Count == 0)
                        {
                            ComputerDeck.AddRange(WarPile);
                        }
                        else
                        {
                            PlayerDeck.AddRange(WarPile);
                        }
                        CurrentPhase = WarEvent_Phases.None;
                        WarPile.Clear();
                        UpdateGameUI();
                        CheckWhoWon();
                        return;
                    }
                    
                    Card PlayerWarCard = PlayerDeck[0];
                    Card ComputerWarCard = ComputerDeck[0];
                    PlayerDeck.RemoveAt(0);
                    ComputerDeck.RemoveAt(0);
                    WarPile.Add(PlayerWarCard);
                    WarPile.Add(ComputerWarCard);

                    playerCardTBox.Text = $"WAR EVENT: {PlayerWarCard.Value} of {PlayerWarCard.Suit}";
                    computerCardTBox.Text = $"WAR EVENT: {ComputerWarCard.Value} of {ComputerWarCard.Suit}";

                    CurrentPhase = WarEvent_Phases.End;
                    ResultTBox.Text = "Click to see who won the war.";
                    break;


                 case WarEvent_Phases.End:

                    if(WarPile.Count < 2)
                    {
                        ResultTBox.Text = "Error : WAR_EVENT_ERROR. GAME_RESET_REQUIRED.";
                        CurrentPhase = WarEvent_Phases.End;
                        WarPile.Clear();
                        UpdateGameUI();
                        return;
                    }

                    Card LastCardP = WarPile[WarPile.Count - 2];
                    Card LastCardC = WarPile[WarPile.Count - 1];
                    int Compare = CompareCards(LastCardP, LastCardC);

                    if (Compare > 0)
                    {
                        ResultTBox.Text = "Player Won the War.";
                        PlayerDeck.AddRange(WarPile);
                    }
                    else if (Compare < 0)
                    {
                        ResultTBox.Text = "Computer won the War.";
                        ComputerDeck.AddRange(WarPile);
                    }
                    else
                    {
                        ResultTBox.Text = "Tie again! Let the war continue then!";
                        CurrentPhase = WarEvent_Phases.Start;
                        return;
                    }

                    WarPile.Clear();
                    CurrentPhase = WarEvent_Phases.None;
                    UpdateGameUI();
                    if (PlayerDeck.Count == 0 || ComputerDeck.Count == 0)
                    {
                        CheckWhoWon();
                    }
                    break;

            }
        }

        private int CompareCards(Card card1,Card card2)
        {
            int value1 = GetCardValue(card1.Value);
            int value2 = GetCardValue(card2.Value);

            return value1.CompareTo(value2);
        }

        private int GetCardValue(string value)
        {
            switch (value.Trim())
            {
                case "2": return 2;
                case "3": return 3;
                case "4": return 4;
                case "5": return 5;
                case "6": return 6;
                case "7": return 7;
                case "8": return 8;
                case "9": return 9;
                case "10": return 10;
                case "J": return 11;
                case "Q": return 12;
                case "K": return 13;
                case "A": return 14;
                default: return 0;
            }
        }

        private void CheckWhoWon()
        {
            CurrentPhase = WarEvent_Phases.None;
            WarPile.Clear();
            GameOver = true;
            string Winner;
            if (PlayerDeck.Count > ComputerDeck.Count)
            {
                ResultTBox.Text = "Game Over. Player Won.";
                Winner = "Player";
            }
            else
            {
                ResultTBox.Text = "Game Over. Computer Won.";
                Winner = "Computer";
            }

            string MemoryInfo = $"Rounds Played: {RoundsPlayed}, Final Score - Player: {PlayerDeck.Count},Computer: {ComputerDeck.Count}";


            GameHistory.AddResult(new GameResult
            {
                GameType = "War",
                Winner = Winner,
                Summary = MemoryInfo
            });

            drawButton.Content = "New Game?";
            drawButton.Click -= DrawButton_Click;
            drawButton.Click += (s, e) =>
            {
                ResetGame();
            };


        }

        private void ResetGame()
        {
            WarWindow newGame = new WarWindow
            {
                onGameEnded = this.onGameEnded
            };
            newGame.Show();
            this.Close();
        }
    }
    public class Card
    {
        public string Suit { get; set; }
        public string Value { get; set; }
    }
}
