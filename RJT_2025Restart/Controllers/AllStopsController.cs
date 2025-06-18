using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static RJT_ASPNET.Models.Models;

namespace RJT_2025Restart.Controllers
{
    [Produces("application/json")]

    public class AllStopsController : Controller
    {
        // GET: AllStopsController
        public List<NaptanStop> Index()
        {
            List<string> uniqueStops = reachableStopsDB.Distinct<string>("FromStop", "{}").ToList();
            List<NaptanStop> uniqueStopsDetailed = naptanDB.Aggregate().Match(x => uniqueStops.Contains(x.stop_id)).ToList();
            return uniqueStopsDetailed;
        }
    }
}
