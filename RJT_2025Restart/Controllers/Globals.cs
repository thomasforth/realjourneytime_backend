namespace RJT_2025Restart.Controllers
{
    public class Globals
    {

        public void CalculateReachableStops()
        {
            string ReachableStopsQuery = "select NaptanId, DestinationNaptanId, Count(DestinationNaptanId) as Count from 'RJT.flatprediction.clean.parquet' group by NaptanId,DestinationNaptanId having Count > 10000 order by Count Desc;";
        
        
        
        
        }




    }
}

