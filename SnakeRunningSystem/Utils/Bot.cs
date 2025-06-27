using System.Reflection.Emit;

namespace SnakeRunningSystem.Utils
{
    public class Bot
    {
        public int userId { get; set; }
        public int botId { get; set; }
        public string botCode { get; set; }
        public string input { get; set; }

        public Bot(int userId, int botId, string botCode, string input)
        {
            this.userId = userId;
            this.botId = botId;
            this.botCode = botCode;
            this.input = input;
        }
    }

}
