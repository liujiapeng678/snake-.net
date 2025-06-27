namespace SnakeServer.Models
{
    public class Users
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }

        public required string Photo { get; set; }
    }
}
