using System.Linq;
using Bartendro.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Data.Services
{
    public interface IReader
    {
        IQueryable<T> Query<T>() where T : Entity;
    }

    internal class Reader : IReader
    {
        private readonly IDatabaseContext _context;

        public Reader(IDatabaseContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query<T>() where T : Entity
        {
            return _context.Set<T>().AsNoTracking().Where(x => x.Deleted == false);
        }
    }
}