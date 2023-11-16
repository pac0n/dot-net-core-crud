using System.Data.SqlClient;
using Unoamuchos.Models;
using Unoamuchos.Repository.Contract;

namespace Unoamuchos.Repository.Implementation
{
    public class ItemRepository : IItemRepository<Items>
    {
        public Task<bool> SaveItems(List<Items> items, SqlConnection conexion, int id)
        {
            try
            {
                string qry = "insert into tbl_BillItems(ProductName, Price, Quantity) values";

                foreach (var item in items)
                {
                    qry += String.Format("('{0}',{1},{2},{3}),", item.ProductName, item.Price, item.Quantity, id);
                }

                qry = qry.Remove(qry.Length - 1);
                SqlCommand cmd = new SqlCommand(qry, conexion);
                cmd.ExecuteNonQuery();
            }

            catch (Exception)
            {
                throw;
            }

            return Task.FromResult(true);
        }
    }
}
