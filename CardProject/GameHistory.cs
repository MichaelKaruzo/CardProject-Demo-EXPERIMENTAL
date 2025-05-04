using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardProject
{
    public static class GameHistory
    {
        private static List<GameResult> GRes = new List<GameResult>();

        public static void AddResult(GameResult result)
        {
            GRes.Add(result);
        }
        public static IReadOnlyList<GameResult> GetAllResults()
        {
            return GRes.ToList();
        }
    }
}
