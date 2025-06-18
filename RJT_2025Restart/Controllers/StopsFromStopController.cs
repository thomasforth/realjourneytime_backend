using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static RJT_ASPNET.Models.Models;

namespace RJT_2025Restart.Controllers
{
    [Produces("application/json")]

    public class StopsFromStopController : Controller
    {
        // GET: StopsFromStopController
        public List<NaptanStop> Index()
        {
            string fromStop = Request.Query["fromCode"];
            List<string> ReachableStops = reachableStopsDB.Distinct<string>("ToStop", "{FromStop: '" + fromStop + "'}").ToList();
            List<NaptanStop> DetailedReachableStops = naptanDB.Aggregate().Match(x => ReachableStops.Contains(x.stop_id)).ToList();
            return DetailedReachableStops;
        }
    }
}
