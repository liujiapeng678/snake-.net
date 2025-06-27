namespace SnakeServer.Models
{
    public class Bots
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required string BotName { get; set; }
        public required string BotDescription { get; set; }
        public required string Code {  get; set; }
        public required int Rating { get; set; }
    }
}
