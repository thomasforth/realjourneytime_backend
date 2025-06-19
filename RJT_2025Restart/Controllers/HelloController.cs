using Dapper;
using DuckDB.NET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static RJT_ASPNET.Models.Models;

namespace RJT_2025Restart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class HelloController : Controller
    {
        public List<DayType> Index()
        {
            using (DuckDBConnection DuckDB = new("Data Source=:memory:"))
            {
                DuckDB.Open();
                string query = $"select * from 'Data/RJT.holidays.clean.parquet';";
                List<DayType> DayTypes = DuckDB.Query<DayType>(query).ToList();
                return DayTypes;
            }
        }        
    }
}