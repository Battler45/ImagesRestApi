using System.Threading.Tasks;

namespace ImagesRestApi.Repositories
{
    public interface IUnitOfWork
    {
        public int SaveChanges();
        public Task<int> SaveChangesAsync();
    }
}
