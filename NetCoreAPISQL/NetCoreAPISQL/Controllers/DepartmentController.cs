using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetCoreAPISQL.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace NetCoreAPISQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string GetSPName (string spValue)
        {
            return _configuration.GetSection("StoreProcedures").GetValue<string>(spValue);
        }

        [HttpGet]
        public JsonResult Get()
        {
            string SPBuscarDepartment = GetSPName("BuscarDepartamento");
            return EjecutaSP(SPBuscarDepartment, null);
        }

        [HttpPost]
        public JsonResult Post(Department dep)
        {
            string SPInsertarDepartment = GetSPName("InsertarDepartamento");
            return EjecutaSP(SPInsertarDepartment, dep);
        }

        [HttpPut]
        public JsonResult Put(Department dep)
        {
            string SPUpdateDepartment = GetSPName("ActualizarDepartamento");
            return EjecutaSP(SPUpdateDepartment, dep);
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string SPBorrarDepartment = GetSPName("BorrarDepartamento");
            Department dep = new Department();
            dep.DepartmentId = id;
            return EjecutaSP(SPBorrarDepartment, dep);
        }

        public JsonResult EjecutaSP(string storeProcedure,Department dep)
        {
            try
            {
                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
                SqlDataReader read;
                object resultado=null;

                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(storeProcedure, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (dep == null){ resultado = 9; }
                        else{
                            cmd.Parameters.AddWithValue("@DepartmentId", dep.DepartmentId);
                            cmd.Parameters.AddWithValue("@DepartmentName", dep.DepartmentName);
                            resultado = "Operación realizada con exito";
                        }
                       
                        read = cmd.ExecuteReader();
                        table.Load(read);
                        read.Close();
                        con.Close();
                    }
                }

                if (resultado.GetType() != typeof(string)){
                    resultado = table;
                }

                return new JsonResult(resultado);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return new JsonResult("Error: " + ex.Message);
            }
            
        }
    }
}
