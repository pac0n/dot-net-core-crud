using Unoamuchos.Models;

namespace Unoamuchos.Repository.Contract
{
    public interface IBillRepository<T> where T : class
    {
        Task<bool> Save(BillDetail modelo);
    }
}
