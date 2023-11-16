using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using Unoamuchos.Models;
using Unoamuchos.Repository.Contract;
using System.Reflection;

namespace Unoamuchos.Repository.Implementation
{
    public class Data : IData
    {
        private readonly string _cadenaSQL = "";
        public Data(IConfiguration configuracion) {

            _cadenaSQL = configuracion.GetConnectionString("cadenaSQL");
        }

        public async Task<List<BillDetail>> GetAllDetail()
        {
            List<BillDetail> _list = new List<BillDetail>();

            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    conexion.Open();
                    SqlCommand cmd = new SqlCommand("spt_getAllEBillDetails", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync()) 
                    {
                        _list.Add(new BillDetail
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CustomerName = reader["CustomerName"].ToString(),
                            MobileNumber = reader["MobileNumber"].ToString(),
                            Adress = reader["Adress"].ToString(),
                            TotalAmount = Convert.ToInt32(reader["TotalAmount"])
                        });
                        
                    }   
                }

                catch (SqlException ex)
                {
                    throw new Exception();
                }

                finally
                { 
                    conexion.Close();
                }

                return _list;
            }
        }

        /*
        public async Task<List<BillDetail>> View(int id)
        {
            List<BillDetail> _reg = new List<BillDetail>();

            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    conexion.Open();
                    SqlCommand cmd = new SqlCommand("spt_getEBillDetails", conexion);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    System.Console.WriteLine("El id del registro es: " + id);

                    while (await reader.ReadAsync())
                    {
                        _reg.Add(new BillDetail
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CustomerName = reader["CustomerName"].ToString(),
                            MobileNumber = reader["MobileNumber"].ToString(),
                            Adress = reader["Adress"].ToString(),
                            TotalAmount = Convert.ToInt32(reader["TotalAmount"])
                        });

                        System.Console.WriteLine("El nombre del cliente es: " + _reg[0].CustomerName);

                    }
                }

                catch (Exception ex)
                {
                    throw new Exception();
                }

                finally
                {
                    conexion.Close();
                }

                return _reg;
            }

        }
        */

        public BillDetail View(int id)
        {
            BillDetail billDetails = new BillDetail();

            using (SqlConnection connection = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spt_getEBillDetails";
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (SqlDataAdapter sqlDA = new SqlDataAdapter(cmd))
                        {
                            DataTable dtBills = new DataTable();
                            sqlDA.Fill(dtBills);

                            foreach (DataRow dr in dtBills.Rows)
                            {
                                billDetails.CustomerName = dr["CustomerName"].ToString();
                                billDetails.MobileNumber = dr["MobileNumber"].ToString();
                                billDetails.Adress = dr["Adress"].ToString();
                                billDetails.TotalAmount = Convert.ToInt32(dr["TotalAmount"]);

                                string product_name = dr["ProductName"].ToString(); 

                                if (product_name != "" && product_name != null)
                                {
                                    Items item = new Items();
                                    item.ProductName = dr["ProductName"].ToString();
                                    item.Price = Convert.ToInt32(dr["Price"]);
                                    item.Quantity = dr["Quantity"].ToString();
                                    item.Id = Convert.ToInt32(dr["BillId"]);
                                    billDetails.Items.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Manejo de la excepción aquí, se podría registrar o propagar el error original.
                    throw;
                }
            }

            return billDetails;
        }


        public async Task<bool> Save(BillDetail details)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    conexion.Open();
                    SqlCommand cmd = new SqlCommand("spt_saveEBillDetails", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("CustomerName", details.CustomerName);

                    cmd.Parameters.AddWithValue("MobileNumber", details.MobileNumber);
                    cmd.Parameters.AddWithValue("Adress", details.Adress);
                    cmd.Parameters.AddWithValue("TotalAmount", details.TotalAmount);

                    SqlParameter outputPara = new SqlParameter();
                    outputPara.DbType = DbType.Int32;
                    outputPara.Direction = ParameterDirection.Output;
                    outputPara.ParameterName = "@Id";
                    cmd.Parameters.Add(outputPara);

                    int filas_afectadas = await cmd.ExecuteNonQueryAsync();

                    //System.Console.WriteLine("El variable filas_afectadas es: " + filas_afectadas);

                    if (filas_afectadas > 0)
                    {

                        if (details.Items.Count > 0)
                        {
                            int id = int.Parse(outputPara.Value.ToString());

                            await SaveItems(details.Items, conexion, id);
                        }

                        else
                        {
                            int id = int.Parse(outputPara.Value.ToString());

                            List<Items> items = new List<Items>();

                            Items newItem = new Items();
                            newItem.ProductName = "Sin dato";
                            newItem.Price = 0;
                            newItem.Quantity = "0";
                            newItem.ItemIndex = id;

                            items.Add(newItem); // Agregar el nuevo elemento a la lista

                            await SaveItems(items, conexion, id);
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

        public async Task<bool> SaveItems(List<Items> items, SqlConnection conexion, int id)
        {
            try
            {
                string qry = "INSERT INTO tbl_BillItems(ProductName, Price, Quantity, BillId) VALUES";

                foreach (var item in items)
                {
                    qry += String.Format("('{0}', {1}, {2}, {3}),", item.ProductName, item.Price, item.Quantity, id);
                }

                qry = qry.Remove(qry.Length - 1);

                using (SqlCommand cmd = new SqlCommand(qry, conexion))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }

            catch (Exception)
            {
                throw;
            }

            finally
            {
                conexion.Close();
            }
        }

        public BillDetail Edit(int id)
        {
            BillDetail billDetails = new BillDetail();

            using (SqlConnection connection = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spt_getEBillDetails";
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (SqlDataAdapter sqlDA = new SqlDataAdapter(cmd))
                        {
                            DataTable dtBills = new DataTable();
                            sqlDA.Fill(dtBills);

                            foreach (DataRow dr in dtBills.Rows)
                            {
                                billDetails.CustomerName = dr["CustomerName"].ToString();
                                billDetails.MobileNumber = dr["MobileNumber"].ToString();
                                billDetails.Adress = dr["Adress"].ToString();
                                billDetails.TotalAmount = Convert.ToInt32(dr["TotalAmount"]);

                                string product_name = dr["ProductName"].ToString();

                                if (product_name != "" && product_name != null)
                                {
                                    Items item = new Items();
                                    item.ProductName = dr["ProductName"].ToString();
                                    item.Price = Convert.ToInt32(dr["Price"]);
                                    item.Quantity = dr["Quantity"].ToString();
                                    item.Id = Convert.ToInt32(dr["BillId"]);
                                    billDetails.Items.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Manejo de la excepción aquí, se podría registrar o propagar el error original.
                    throw;
                }
            }

            return billDetails;
        }

        /*
        public async Task<List<Items>> GetItems(int id)
        {
            List<Items> billItems = new List<Items>();

            using (SqlConnection connection = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spt_getItems";
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (SqlDataAdapter sqlDA = new SqlDataAdapter(cmd))
                        {
                            DataTable dtItems = new DataTable();
                            sqlDA.Fill(dtItems);

                            foreach (DataRow dr in dtItems.Rows)
                            {
                                Items item = new Items();
                                item.ProductName = dr["ProductName"].ToString();
                                item.Price = Convert.ToInt32(dr["Price"]);
                                item.Quantity = dr["Quantity"].ToString();
                                item.Id = Convert.ToInt32(dr["BillId"]);
                                billItems.Add(item);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Manejo de la excepción aquí, se podría registrar o propagar el error original.
                    throw;
                }
            }
            
                foreach(Items item in billItems)
                {
                    System.Console.WriteLine("Nombre del item es: " + item.ProductName);

                }
            

            return billItems;
        }
        */

        public async Task<bool> Edit(BillDetail details)
        {
            using (var conexion = new SqlConnection(_cadenaSQL))
            {
                try
                {
                    conexion.Open();
                    SqlCommand cmd = new SqlCommand("spt_updateEBill", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerName", details.CustomerName);
                    cmd.Parameters.AddWithValue("@MobileNumber", details.MobileNumber);
                    cmd.Parameters.AddWithValue("@Adress", details.Adress);
                    cmd.Parameters.AddWithValue("@TotalAmount", details.TotalAmount);
                    cmd.Parameters.AddWithValue("@Id", details.Id);

                    int filas_afectadas = await cmd.ExecuteNonQueryAsync();

                    //System.Console.WriteLine("El variable filas_afectadas es: " + filas_afectadas);

                    if (filas_afectadas > 0)
                    {

                        if (details.Items.Count > 0)
                        {
                            await SaveitemsUpdate(details.Items, conexion, details);
                        }

                        else
                        {
                            List<Items> items = new List<Items>();

                            Items newItem = new Items();
                            newItem.ProductName = "Sin dato";
                            newItem.Price = 0;
                            newItem.Quantity = "0";
                            newItem.ItemIndex = details.Id;

                            items.Add(newItem); // Agregar el nuevo elemento a la lista

                            await SaveitemsUpdate(items, conexion, details);
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

        public async Task<bool> SaveitemsUpdate(List<Items> items, SqlConnection conexion, BillDetail details)
        {
            try
            {
                string deleteqry = "DELETE tbl_BillItems WHERE BillId = " + details.Id;

                using (SqlCommand deleteCmd = new SqlCommand(deleteqry, conexion))
                {
                    deleteCmd.Parameters.AddWithValue("@BillId", details.Id);
                        
                    await deleteCmd.ExecuteNonQueryAsync();
                }

                string qry = "INSERT INTO tbl_BillItems(ProductName, Price, Quantity, BillId) VALUES";

                foreach (var item in items)
                {
                    qry += String.Format("('{0}', {1}, {2}, {3}),", item.ProductName, item.Price, item.Quantity, details.Id);
                }

                qry = qry.Remove(qry.Length - 1);

                using (SqlCommand cmd = new SqlCommand(qry, conexion))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }

            catch (Exception)
            {
                throw;
            }
        }
    }
}
