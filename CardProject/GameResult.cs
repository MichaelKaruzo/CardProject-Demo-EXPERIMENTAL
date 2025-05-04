using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardProject
{
    public class GameResult
    {
        public string GameType { get; set; }
        public string Winner { get; set; }

        public string Summary { get; set; }

        public override string ToString()
        {
            return $"{GameType} - Winner: {Winner} | {Summary}";
        }
    }
}
