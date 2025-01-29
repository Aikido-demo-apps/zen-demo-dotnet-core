public class UserGenerator
{
    private readonly string[] FirstNames = new[]
    {
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda",
        "William", "Elizabeth", "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica",
        "Thomas", "Sarah", "Charles", "Karen", "Christopher", "Nancy", "Daniel", "Lisa",
        "Matthew", "Betty", "Anthony", "Margaret", "Mark", "Sandra", "Donald", "Ashley"
    };

    private readonly string[] LastNames = new[]
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

    public (string Id, string Name) GenerateUserFromIP(string ip)
    {
        var hash = GetHashCode(ip);

        var firstName = FirstNames[hash % FirstNames.Length];
        var lastName = LastNames[(hash >> 4) % LastNames.Length];

        var userId = Convert.ToString(hash, 36).Substring(0, Math.Min(8, Convert.ToString(hash, 36).Length));

        return (userId, $"{firstName} {lastName}");
    }
}