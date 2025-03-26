using System.ComponentModel.DataAnnotations;

namespace zen_demo_dotnet.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
