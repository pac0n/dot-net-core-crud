using System.Data.SqlClient;
using System.Data;
using Unoamuchos.Models;
using Unoamuchos.Repository.Contract;

namespace Unoamuchos.Repository.Implementation
{
    public class BillRepository : IBillRepository<BillDetail>
    {
        private readonly string _cadenaSQL = "";
        private readonly ItemRepository _itemRepository;

        public BillRepository(IConfiguration configuracion)
        {
            _cadenaSQL = configuracion.GetConnectionString("cadenaSQL");
            _itemRepository = new ItemRepository();
        }

        public async Task<bool> Save(BillDetail modelo)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    conexion.Open();
                    SqlCommand cmd = new SqlCommand("spt_saveEBillDetails", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("CustomerName", modelo.CustomerName);

                    //System.Console.WriteLine("El nombre del cliente es: " + modelo.CustomerName);

                    cmd.Parameters.AddWithValue("MobileNumber", modelo.MobileNumber);
                    cmd.Parameters.AddWithValue("Adress", modelo.Adress);
                    cmd.Parameters.AddWithValue("TotalAmount", modelo.TotalAmount);

                    SqlParameter outputPara = new SqlParameter();
                    outputPara.DbType = DbType.Int32;
                    outputPara.Direction = ParameterDirection.Output;
                    outputPara.ParameterName = "@Id";
                    cmd.Parameters.Add(outputPara);

                    int filas_afectadas = await cmd.ExecuteNonQueryAsync();

                    //System.Console.WriteLine("El variable filas_afectadas es: " + filas_afectadas);

                    if (filas_afectadas > 0)
                    {
                        //System.Console.WriteLine("El nombre del item es: " + modelo.Items[1].ProductName);

                        if (modelo.Items.Count > 0)
                        {
                            int id = int.Parse(outputPara.Value.ToString());

                            //System.Console.WriteLine("El id entero es: " + id);

                            await _itemRepository.SaveItems(modelo.Items, conexion, id);
                        }

                        return true;
                    }

                    return false;

                }

                catch (Exception)
                {
                    throw new Exception();
                }

                finally
                {
                    conexion.Close();
                }
            }
        }
    }
}
