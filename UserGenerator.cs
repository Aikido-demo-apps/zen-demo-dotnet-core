using System.Text;

namespace zen_demo_dotnet;

public class UserGenerator
{
    private readonly string[] _firstNames = new[]
    {
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
        "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica",
        "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa",
        "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley"
    };

    private readonly string[] _lastNames = new[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzales", "Wilson", "Anderson",
        "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson",
        "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker"
    };

    private int GetHashCode(string str)
    {
        unchecked
        {
            int hash = 0;
            for (int i = 0; i < str.Length; i++)
            {
                hash = ((hash << 5) - hash) + str[i];
                hash = hash & hash;
            }
            return Math.Abs(hash);
        }
    }

    private string ToBase36(int value)
    {
        const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
        var result = new StringBuilder();

        // Handle 0 explicitly
        if (value == 0)
        {
            return "0";
        }

        value = Math.Abs(value); // Ensure positive number

        while (value > 0)
        {
            result.Insert(0, chars[value % 36]);
            value /= 36;
        }

        return result.ToString();
    }

    public (string Id, string Name) GenerateUserFromIP(string ip)
    {
        var hash = GetHashCode(ip);

        var firstName = _firstNames[hash % _firstNames.Length];
        var lastName = _lastNames[(hash >> 4) % _lastNames.Length];

        var userId = ToBase36(hash);
        if (userId.Length > 8)
        {
            userId = userId.Substring(0, 8);
        }

        return (userId, $"{firstName} {lastName}");
    }
}