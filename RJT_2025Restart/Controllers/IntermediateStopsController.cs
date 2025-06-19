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

    public class IntermediateStopsController : Controller
    {
        // GET: IntermediateStopsController
        public List<string> Index()
        {

            string fromCode1 = Request.Query["fromCode"];
            string toCode1 = Request.Query["toCode"];
            string year = Request.Query["year"];

            // 43000828302 --> 43000831902 for testing.

            using (DuckDBConnection DuckDB = new("Data Source=:memory:"))
            {
                DuckDB.Open();
                string MostCommonJourneyIdsConnectingTheTwoPoints = DuckDB.Query<string>($"SELECT Id FROM 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND (NaptanId = '{fromCode1}' OR NaptanId = '{toCode1}') GROUP BY Id ORDER BY COUNT(*) DESC LIMIT 1").FirstOrDefault();

                List<FlatPrediction> stopOnTheMostCommonJourneyId = DuckDB.Query<FlatPrediction>($"SELECT * FROM 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND Id = '{MostCommonJourneyIdsConnectingTheTwoPoints}' order by ScheduledArrival Desc").ToList();
                DateTime fromStopTime = stopOnTheMostCommonJourneyId.Where(x => x.NaptanId == fromCode1).First().ScheduledArrival;

                DateTime toStopTime = stopOnTheMostCommonJourneyId.Where(x => x.NaptanId == toCode1).First().ScheduledArrival;
                List<string> IntermediateStops = stopOnTheMostCommonJourneyId.Where(x => x.ScheduledArrival >= fromStopTime && x.ScheduledArrival <= toStopTime).OrderBy(x => x.ScheduledArrival).Select(x => x.NaptanId).ToList();

                if (fromStopTime > toStopTime)
                {
                    return new List<string> { "Buses seem to arrive at the fromStop before they leave at the toStop." };
                }
                else
                {
                    return IntermediateStops;
                }
            }
        }
    }
}
