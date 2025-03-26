namespace zen_demo_dotnet.Models
{
    public class CreateRequest
    {
        public string? Name { get; set; }
    }

    public class CommandRequest
    {
        public string? UserCommand { get; set; }
    }

    public class RequestRequest
    {
        public string? Url { get; set; }
    }
}
