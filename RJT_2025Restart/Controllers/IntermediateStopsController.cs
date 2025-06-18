using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static RJT_ASPNET.Models.Models;

namespace RJT_2025Restart.Controllers
{
    [Produces("application/json")]

    public class IntermediateStopsController : Controller
    {
        // GET: IntermediateStopsController
        public List<NaptanStop> Index()
        {
            string fromCode1 = Request.Query["fromCode"];
            string toCode1 = Request.Query["toCode"];
            string MostCommonJourneyIdsConnectingTheTwoPoints = flatPredictionsDB.AsQueryable().Where(x => x.ScheduledArrival > DateTime.Now.AddDays(-2) && x.ScheduledArrival < DateTime.Now.AddDays(-1) && (x.NaptanId == fromCode1 || x.NaptanId == toCode1)).GroupBy(x => x.Id).OrderByDescending(x => x.Count()).Select(x => x.Key).First();

            List<FlatPrediction> stopOnTheMostCommonJourneyId = flatPredictionsDB.AsQueryable().Where(x => x.ScheduledArrival > DateTime.Now.AddDays(-2) && x.ScheduledArrival < DateTime.Now.AddDays(-1) && x.Id == MostCommonJourneyIdsConnectingTheTwoPoints).ToList();
            DateTime fromStopTime = stopOnTheMostCommonJourneyId.Where(x => x.NaptanId == fromCode1).First().ScheduledArrival;
            DateTime toStopTime = stopOnTheMostCommonJourneyId.Where(x => x.NaptanId == toCode1).First().ScheduledArrival;
            List<string> IntermediateStops = stopOnTheMostCommonJourneyId.Where(x => x.ScheduledArrival >= fromStopTime && x.ScheduledArrival <= toStopTime).OrderBy(x => x.ScheduledArrival).Select(x => x.NaptanId).ToList();

            if (fromStopTime > toStopTime)
            {
                return "Buses seem to arrive at the fromStop before they leave at the toStop.";
            }
            else
            {
                return JsonConvert.SerializeObject(IntermediateStops);
            }
        }
    }
}
