namespace TicTacToeOnline.Shared.Models
{
    public class User
    {
        public string Id { get; set; } = default!;
        public List<string> ConnectionsId { get; set; } = default!;
        public string Username { get; set; } = default!;
    }
}
