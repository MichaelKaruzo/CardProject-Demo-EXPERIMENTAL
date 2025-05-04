using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;


namespace CardProject
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void LoadHistory()
        {
            var results = GameHistory.GetAllResults();
            

            if(results == null || results.Count == 0)
            {
                Console.WriteLine("NO_HISTORY_FOUND");
                return;
            }

            if(HistoryListBox != null)
            {
                HistoryListBox.ItemsSource = results;
            }
            
        }
    }
}
