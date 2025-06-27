namespace SnakeServer.Models
{
    public class Records
    {
        public int Id { get; set; }

        public required int AId { get; set; }
        public required int ASx { get; set; }
        public required int ASy { get; set; }

        public required int BId { get; set; }
        public required int BSx { get; set; }
        public required int BSy { get; set; }

        public string? ASteps { get; set; }  // 可空字段
        public string? BSteps { get; set; }  // 可空字段

        public required string Map { get; set; }
        public required string Loser { get; set; }

        public required DateTime CreateTime { get; set; }
    }

}
