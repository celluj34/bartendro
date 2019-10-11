using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Database.Services
{
    public interface IReader
    {
        IQueryable<T> Query<T>() where T : class;
    }

    internal class Reader : IReader
    {
        private readonly IDatabaseContext _context;

        public Reader(IDatabaseContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return _context.Set<T>().AsNoTracking();
        }
    }
}