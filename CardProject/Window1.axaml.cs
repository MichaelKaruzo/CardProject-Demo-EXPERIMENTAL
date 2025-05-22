using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace CardProject
{
    public partial class PokerWindow : Window
    {
        private Button gameStartButton;
        private Button callButton;
        private Button foldButton;
        private Button raiseButton;

        private TextBlock? playerCard1;
        private TextBlock? playerCard2;

        private List<TextBlock> communityCards;

        private List<CardsPoker> Deck = new();
        private List<CardsPoker> PlayerHand = new();
        private List<CardsPoker> CommunityCards = new();

        private int gamePhase = 0;

        public PokerWindow()
        {
            InitializeComponent();

            gameStartButton = this.FindControl<Button>("game_start")!;
            callButton = this.FindControl<Button>("call_button")!;
            foldButton = this.FindControl<Button>("fold_button")!;
            raiseButton = this.FindControl<Button>("raise_button")!;

            playerCard1 = this.FindControl<TextBlock>("player_card1");
            playerCard2 = this.FindControl<TextBlock>("player_card2");

            communityCards = new List<TextBlock>
            {
                this.FindControl<TextBlock>("card_flip1")!,
                this.FindControl<TextBlock>("card_flip2")!,
                this.FindControl<TextBlock>("card_flip3")!,
                this.FindControl<TextBlock>("card_flip4")!,
                this.FindControl<TextBlock>("card_flip5")!
            };

            gameStartButton.Click += GameStartButton_Click;
            callButton.Click += CallButton_Click;
            foldButton.Click += FoldButton_Click;
            raiseButton.Click += RaiseButton_Click;

            MessageBox("Ready for action, partner!");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void GameStartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            AdvanceGamePhase("CALL");
        }

        private void RaiseButton_Click(object sender, RoutedEventArgs e)
        {
            var visibleCards = new List<CardsPoker>(PlayerHand);

            int revealed = 0;
            for (int i = 0; i < communityCards.Count; i++)
            {
                if (communityCards[i].Text != "🂠")
                {
                    visibleCards.Add(CommunityCards[i]);
                    revealed++;
                }
            }

            var logBox = this.FindControl<TextBlock>("logBox")!;

            if (revealed == 0)
            {
                logBox.Text = "Brak odkrytych kart wspólnych – nie można ocenić ręki.";
                return;
            }

            int rank = EvaluateBestPokerHand(visibleCards);
            string name = PokerHandName(rank);

            logBox.Text = $"RAISE: Układ z odkrytych kart: {name} ({rank})";
        }

        private void FoldButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox("You folded. New round starts.");
            StartGame();
        }

        private void StartGame()
        {
            gamePhase = 0;

            Deck = GenerateDeck();
            ShuffleDeck(Deck);

            PlayerHand = new List<CardsPoker> { DrawCard(), DrawCard() };
            CommunityCards = new List<CardsPoker>
            {
                DrawCard(), DrawCard(), DrawCard(),
                DrawCard(), DrawCard()
            };

            playerCard1!.Text = PlayerHand[0].ToString();
            playerCard2!.Text = PlayerHand[1].ToString();

            foreach (var cardSlot in communityCards)
                cardSlot.Text = "🂠";

            this.FindControl<TextBlock>("handInfo")!.Text = "(Układ pojawi się po riverze)";
            MessageBox("New game started. Your hand is dealt.");
        }

        private void AdvanceGamePhase(string action)
        {
            gamePhase++;

            switch (gamePhase)
            {
                case 1:
                    MessageBox($"{action}: Flop revealed!");
                    RevealCards(0, 3);
                    break;
                case 2:
                    MessageBox($"{action}: Turn revealed!");
                    RevealCards(3, 1);
                    break;
                case 3:
                    MessageBox($"{action}: River revealed!");
                    RevealCards(4, 1);
                    break;
                case 4:
                    EvaluateHand();
                    StartGame();
                    break;
            }
        }

        private void RevealCards(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                communityCards[i].Text = CommunityCards[i].ToString();
            }
        }

        private void EvaluateHand()
        {
            var allCards = new List<CardsPoker>();
            allCards.AddRange(PlayerHand);
            allCards.AddRange(CommunityCards);

            int rank = EvaluateBestPokerHand(allCards);
            string name = PokerHandName(rank);

            this.FindControl<TextBlock>("handInfo")!.Text = $"Układ: {name} ({rank})";
        }

        private void MessageBox(string msg)
        {
            this.FindControl<TextBlock>("logBox")!.Text = msg;
            Console.WriteLine(msg);
        }

        private List<CardsPoker> GenerateDeck()
        {
            var suits = new[] { "Hearts", "Diamonds", "Clubs", "Spades" };
            var values = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

            var deck = new List<CardsPoker>();
            foreach (var suit in suits)
            {
                foreach (var value in values)
                {
                    deck.Add(new CardsPoker(suit, value));
                }
            }

            return deck;
        }

        private void ShuffleDeck(List<CardsPoker> deck)
        {
            var rng = new Random();
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        private CardsPoker DrawCard()
        {
            if (Deck.Count == 0)
                throw new InvalidOperationException("Deck is empty!");

            var card = Deck[0];
            Deck.RemoveAt(0);
            return card;
        }

        private int EvaluateBestPokerHand(List<CardsPoker> cards)
        {
            int pairs = 0;
            int threes = 0;
            int fours = 0;
            bool flush = false;
            bool straight = false;

            var values = new Dictionary<string, int>();
            var suits = new Dictionary<string, int>();
            var nums = new List<int>();

            foreach (var card in cards)
            {
                if (!values.ContainsKey(card.Value))
                    values[card.Value] = 0;
                values[card.Value]++;

                if (!suits.ContainsKey(card.Suit))
                    suits[card.Suit] = 0;
                suits[card.Suit]++;

                nums.Add(GetCardNumericValue(card.Value));
            }

            foreach (var v in values.Values)
            {
                if (v == 4) fours++;
                else if (v == 3) threes++;
                else if (v == 2) pairs++;
            }

            flush = suits.ContainsValue(5) || suits.ContainsValue(6) || suits.ContainsValue(7);

            nums.Sort();
            nums = new List<int>(new HashSet<int>(nums));

            for (int i = 0; i <= nums.Count - 5; i++)
            {
                if (nums[i + 4] - nums[i] == 4)
                {
                    straight = true;
                    break;
                }
            }

            if (straight && flush) return 8;
            if (fours > 0) return 7;
            if (threes > 0 && pairs > 0) return 6;
            if (flush) return 5;
            if (straight) return 4;
            if (threes > 0) return 3;
            if (pairs >= 2) return 2;
            if (pairs == 1) return 1;
            return 0;
        }

        private string PokerHandName(int rank)
        {
            return rank switch
            {
                9 => "Royal Flush",
                8 => "Straight Flush",
                7 => "Four of a Kind",
                6 => "Full House",
                5 => "Flush",
                4 => "Straight",
                3 => "Three of a Kind",
                2 => "Two Pair",
                1 => "One Pair",
                _ => "High Card"
            };
        }

        private int GetCardNumericValue(string value)
        {
            return value switch
            {
                "2" => 2,
                "3" => 3,
                "4" => 4,
                "5" => 5,
                "6" => 6,
                "7" => 7,
                "8" => 8,
                "9" => 9,
                "10" => 10,
                "J" => 11,
                "Q" => 12,
                "K" => 13,
                "A" => 14,
                _ => 0
            };
        }
    }

    public class CardsPoker
    {
        public string Suit { get; set; }
        public string Value { get; set; }

        public CardsPoker(string suit, string value)
        {
            Suit = suit;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value} of {Suit}";
        }
    }
}