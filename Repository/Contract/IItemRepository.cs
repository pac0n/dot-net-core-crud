using System.Data.SqlClient;
using Unoamuchos.Models;

namespace Unoamuchos.Repository.Contract
{
    public interface IItemRepository<T> where T : class
    {
        Task<bool> SaveItems(List<Items> items, SqlConnection conexion, int id);
    }
}
