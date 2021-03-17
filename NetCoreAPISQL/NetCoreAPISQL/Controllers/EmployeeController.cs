using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetCoreAPISQL.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace NetCoreAPISQL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

       public EmployeeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }
        private string GetSPName(string spValue)
        {
            return _configuration.GetSection("StoreProcedures").GetValue<string>(spValue);
        }

        [HttpGet]
        public JsonResult Get()
        {
            string SPBuscarEmployee = GetSPName("BuscarEmpleado");
            return EjecutaSP(SPBuscarEmployee, null);
        }

        [Route("GetAllDepartmentNames")]
        public JsonResult GetAllDepartmentNames()
        {
            string SPBuscarDepartamentosEmp = GetSPName("BuscarDepartamentosEmp");
            return EjecutaSP(SPBuscarDepartamentosEmp, null);
        }

        [HttpPost]
        public JsonResult Post(Employee emp)
        {
            string SPInsertarEmployee = GetSPName("InsertarEmpleado");
            return EjecutaSP(SPInsertarEmployee, emp);
        }

        [HttpPut]
        public JsonResult Put(Employee emp)
        {
            string SPUpdateEmployee = GetSPName("ActualizarEmpleado");
            return EjecutaSP(SPUpdateEmployee, emp);
        }

        [HttpDelete("{id}")]
        public JsonResult Delete(int id)
        {
            string SPBorrarEmployee = GetSPName("BorrarEmpleado");
            Employee emp = new Employee();
            emp.EmployeeId = id;
            return EjecutaSP(SPBorrarEmployee, emp);
        }

        public JsonResult EjecutaSP(string storeProcedure, Employee emp)
        {
            try
            {
                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("EmployeeAppCon");
                SqlDataReader read;
                object resultado = null;

                using (SqlConnection con = new SqlConnection(sqlDataSource))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(storeProcedure, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (emp == null) { resultado = 9; }
                        else
                        {
                            cmd.Parameters.AddWithValue("@EmployeeId", emp.EmployeeId);
                            cmd.Parameters.AddWithValue("@EmployeeName", emp.EmployeeName);
                            cmd.Parameters.AddWithValue("@Department", emp.Department);
                            cmd.Parameters.AddWithValue("@DateOfJoining", emp.DateOfJoining);
                            cmd.Parameters.AddWithValue("@PhotoFileName", emp.PhotoFileName);
                            resultado = "Operación realizada con exito";
                        }

                        read = cmd.ExecuteReader();
                        table.Load(read);
                        read.Close();
                        con.Close();
                    }
                }

                if (resultado.GetType() != typeof(string))
                {
                    resultado = table;
                }

                return new JsonResult(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return new JsonResult("Error: " + ex.Message);
            }

        }

        [Route("SaveFile")]
        [HttpPost]
        public JsonResult SaveFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = postedFile.FileName;
                var physicalPath = _env.ContentRootPath+"/Photos/"+filename;

                using(var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    postedFile.CopyTo(stream);
                }

                return new JsonResult(filename);
            }
            catch(Exception ex)
            {
                return new JsonResult("anonymous.png");
            }
        }

    }
}
