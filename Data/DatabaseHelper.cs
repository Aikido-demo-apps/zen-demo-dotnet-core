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
            return await _context.Pets.ToListAsync();
        }

        public async Task<int> CreatePetByNameAsync(string name)
        {
            try
            {
                var pet = new Pet { Name = name };
                _context.Pets.Add(pet);
                return await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task ClearAllAsync()
        {
            _context.Pets.RemoveRange(_context.Pets);
            await _context.SaveChangesAsync();
        }
    }
}
