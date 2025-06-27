using SnakeServer.Models;
using System.Text;

namespace SnakeServer.Models
{
    public class Player
    {
        public int Id { get; set; }
        public int? BotId { get; set; }  // -1是人
        public int Rating { get; set; }
        public string? BotCode { get; set; }
        public int? Sx { get; set; }
        public int? Sy { get; set; }
        public List<int> Steps { get; set; }

        public Player()
        {
            Steps = new List<int>();
        }

        public Player(int? id, int? botId, int rating, string botCode, int? sx, int? sy, List<int> steps)
        {
            Id = id.Value;
            BotId = botId;
            Rating = rating;
            BotCode = botCode;
            Sx = sx;
            Sy = sy;
            Steps = steps ?? new List<int>();
        }

        private bool CheckTailIncreasing(int step)
        {
            return step % 3 == 1;
        }

        public List<Cell> GetCells()
        {
            List<Cell> res = new List<Cell>();
            int[] dx = { -1, 0, 1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            int step = 0;
            int x = Sx ?? 0;
            int y = Sy ?? 0;
            res.Add(new Cell(x, y));

            foreach (int d in Steps)
            {
                step++;
                x += dx[d];
                y += dy[d];
                res.Add(new Cell(x, y));
                if (!CheckTailIncreasing(step))
                {
                    res.RemoveAt(0);
                }
            }
            return res;
        }

        public string StepsToString()
        {
            StringBuilder res = new StringBuilder();
            foreach (int d in Steps)
            {
                res.Append(d);
            }
            return res.ToString();
        }
    }
}