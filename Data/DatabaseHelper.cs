using Microsoft.EntityFrameworkCore;
using zen_demo_dotnet.Models;

namespace zen_demo_dotnet.Data
{
    public class DatabaseHelper
    {
        private readonly ApplicationDbContext _context;

        public DatabaseHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pet>> GetAllPetsAsync()
        {
            // Using raw SQL query without parameters to allow SQL injection
            // Explicitly include all columns that the Pet entity expects
            // Also include pet_id for display in the UI
            var pets = await _context.Pets.FromSqlRaw("SELECT \"Id\", \"Name\", \"Owner\", \"Id\" as pet_id FROM \"Pets\"").ToListAsync();
            return pets;
        }

        public async Task<int> CreatePetByNameAsync(string name)
        {
            try
            {
                // Using raw SQL query with direct string concatenation to allow SQL injection
                string sql = $"INSERT INTO \"Pets\" (\"Name\", \"Owner\") VALUES ('{name}', 'Aikido')";
                return await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                // Log the exception to see the error details
                Console.WriteLine($"SQL Error: {ex.Message}");

                // If there's an inner exception, log that too
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Log the stack trace for debugging
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Rethrow to make sure it's visible in application logs
                throw;
            }
        }

        public async Task ClearAllAsync()
        {
            _context.Pets.RemoveRange(_context.Pets);
            await _context.SaveChangesAsync();
        }
    }
}
