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

    public class AllStopsController : Controller
    {
        public ActionResult<List<NaptanStop>> Index()
        {
            string year = Request.Query["year"];

            using (DuckDBConnection DuckDB = new("Data Source=:memory:"))
            {
                DuckDB.Open();
                // this query is massive overkill, but it does trim down stops to those that go places.
                // string query = $"select distinct NaptanId as stop_id, NaptanCode as stop_code, CommonName as stop_name, Latitude as stop_lat, Longitude as stop_lon, StopType as vehicle_type from (select distinct NaptanId, DestinationNaptanId, Count(DestinationNaptanId) as Count from 'Data/RJT.flatprediction.clean.parquet' group by NaptanId,DestinationNaptanId having Count > 10000 order by Count Desc) a join 'Data/RJT.naptan.parquet' b on a.NaptanId = b._id;";

                string query = $"select distinct NaptanId as stop_id, NaptanCode as stop_code, CommonName as stop_name, Latitude as stop_lat, Longitude as stop_lon, StopType as vehicle_type from (select distinct NaptanId, DestinationNaptanId, Count(DestinationNaptanId) as Count from 'Data/RJT.flatprediction.clean.parquet' where datepart('Year', ScheduledArrival) = '{year}' group by NaptanId,DestinationNaptanId having Count > 10000 order by Count Desc) a join 'Data/RJT.naptan.parquet' b on a.NaptanId = b._id;";
                return DuckDB.Query<NaptanStop>(query).ToList();
            }
        }        
    }
}