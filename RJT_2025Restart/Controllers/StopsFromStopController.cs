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

    public class StopsFromStopController : Controller
    {
        // GET: StopsFromStopController
        public ActionResult<List<NaptanStop>> Index()
        {            
            string fromStop = Request.Query["fromCode"];
            string year = Request.Query["year"];

            using (DuckDBConnection DuckDB = new("Data Source=:memory:"))
            {
                DuckDB.Open();
                // Hard coded query for testing purposes (2025 June)
                string query = $"select ToStop as stop_id, NaptanCode as stop_code, CommonName as stop_name, Latitude as stop_lat, Longitude as stop_lon, StopType as vehicle_type from (select FromStop, ToStop, count(ToStop) as Count from (select a.NaptanId as FromStop, b.NaptanId as ToStop from (select NaptanId, Id, ScheduledArrival from 'Data/RJT.flatprediction.clean.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND NaptanId = '{fromStop}') a join (select NaptanId, Id, ScheduledArrival from 'Data/RJT.flatprediction.clean.parquet' where datepart('Year', ScheduledArrival) = '{year}') b on a.Id = b.Id where b.ScheduledArrival > a.ScheduledArrival) group by FromStop, ToStop having Count > 500 order by Count Desc) a join 'Data/RJT.naptan.parquet' b on a.ToStop = b._id;";
                return DuckDB.Query<NaptanStop>(query).ToList();
            }
        }
    }
}