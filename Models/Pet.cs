using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zen_demo_dotnet.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = "Aikido Security";

        [NotMapped]
        public int pet_id { get; set; }
    }
}
