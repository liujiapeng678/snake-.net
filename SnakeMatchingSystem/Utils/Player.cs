namespace SnakeMatchingSystem.Utils
{
    public class Player
    {
        public int? UserId { get; set; }
        public int? Rating { get; set; }
        public int? BotId { get; set; }
        public int? WaitingTime { get; set; }

        public Player()
        {
        }

        public Player(int? userId, int? rating, int? botId, int? waitingTime)
        {
            UserId = userId;
            Rating = rating;
            BotId = botId;
            WaitingTime = waitingTime;
        }
    }
}