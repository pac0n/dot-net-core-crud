using System.Data.SqlClient;
using Unoamuchos.Models;

namespace Unoamuchos.Repository.Contract
{
    public interface IData
    {
        Task<bool> Save(BillDetail details);

        Task<bool> SaveItems(List<Items> items, SqlConnection conexion, int id);

        Task<List<BillDetail>> GetAllDetail();

        BillDetail View(int id);

        //(List<BillDetail> billDetails, List<Items> items) View(int id);

        Task<bool> Edit(BillDetail details);

        BillDetail Edit(int id);

        Task<bool> SaveitemsUpdate(List<Items> items, SqlConnection conexion, BillDetail details);

        //Task<List<Items>> GetItems(int id);
    }
}
