namespace zen_demo_dotnet.Helpers
{
    public static class UserHelper
    {
        public static string GetName(int number)
        {
            string[] names = {
                "Hans",
                "Pablo",
                "Samuel",
                "Timo",
                "Tudor",
                "Willem",
                "Wout",
                "Yannis",
            };

            // Use absolute value to handle negative numbers
            // Use modulo to wrap around the list
            int index = Math.Abs(number) % names.Length;
            return names[index];
        }
    }
}
