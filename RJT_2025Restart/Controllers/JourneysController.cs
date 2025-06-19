using Dapper;
using DuckDB.NET.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Xml;
using static RJT_ASPNET.Models.Models;

namespace RJT_2025Restart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class JourneysController : Controller
    {
        // GET: JourneysController
        public CombinedReturnObject Index()
        {

            string fromCode = Request.Query["fromCode"];
            string toCode = Request.Query["toCode"];
            string dateString = Request.Query["dateString"];

            string startTime = Request.Query["startTime"];
            string endTime = Request.Query["endTime"];

            string year = Request.Query["year"];

            string service = null;
            if (Request.Query["service"].ToString() != null)
            {
                service = Request.Query["service"];
            }

            // https://realjourneytime.azurewebsites.net/index.php?method=Journeys&fromCode=43002104001&toCode=43000413401&dateString=2018-03-06                  

            DateTime parsedDate;
            bool IsADate = DateTime.TryParse(dateString, out parsedDate);

            List<FlatPrediction> DeparturesFromStopsOfInterest = new List<FlatPrediction>();

            using (DuckDBConnection DuckDB = new("Data Source=:memory:"))
            {
                DuckDB.Open();

                if (IsADate == true)
                {
                    if (service == null)
                    {
                        DeparturesFromStopsOfInterest = DuckDB.Query<FlatPrediction>($"select * from 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND ScheduledArrival > '{parsedDate.ToString("o")}' and ScheduledArrival < '{parsedDate.AddDays(1).ToString("o")}' and (NaptanId = '{fromCode}' or NaptanId = '{toCode}')").ToList();
                        //DeparturesFromStopsOfInterest = flatPredictionsDB.Find(x => x.ScheduledArrival > parsedDate && x.ScheduledArrival < parsedDate.AddDays(1) && (x.NaptanId == fromCode || x.NaptanId == toCode)).ToList();
                    }
                    else
                    {
                        DeparturesFromStopsOfInterest = DuckDB.Query<FlatPrediction>($"select * from 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND ScheduledArrival > '{parsedDate.ToString("o")}' and ScheduledArrival < '{parsedDate.AddDays(1).ToString("o")}' and (NaptanId = '{fromCode}' or NaptanId = '{toCode}') and LineName = '{service}'").ToList();
                        //DeparturesFromStopsOfInterest = flatPredictionsDB.Find(x => x.ScheduledArrival > parsedDate && x.ScheduledArrival < parsedDate.AddDays(1) && (x.NaptanId == fromCode || x.NaptanId == toCode) && (x.LineName == service)).ToList();
                    }
                }
                else
                {
                    if (service == null)
                    {
                        DeparturesFromStopsOfInterest = DuckDB.Query<FlatPrediction>($"select * from 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND (NaptanId = '{fromCode}' or NaptanId = '{toCode}')").Take(100000).ToList();
                        //DeparturesFromStopsOfInterest = flatPredictionsDB.Find(x => x.ScheduledArrival > DateTime.Now.AddDays(-56) && (x.NaptanId == fromCode || x.NaptanId == toCode)).ToList();
                    }
                    else
                    {
                        DeparturesFromStopsOfInterest = DuckDB.Query<FlatPrediction>($"select * from 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND ScheduledArrival > '{DateTime.Now.AddDays(-56).ToString("o")}' and LineName = '{service}' and (NaptanId = '{fromCode}' or NaptanId = '{toCode}')").ToList();
                        //DeparturesFromStopsOfInterest = flatPredictionsDB.Find(x => x.ScheduledArrival > DateTime.Now.AddDays(-56) && x.LineName == service && (x.NaptanId == fromCode || x.NaptanId == toCode)).ToList();
                    }
                }

                foreach (FlatPrediction flatPrediction in DeparturesFromStopsOfInterest)
                {
                    flatPrediction.UniqueDepartureId = flatPrediction.ScheduledArrival.Date.ToString("yyyy-MM-dd") + "_" + flatPrediction.Id;
                }
                List<FlatPrediction> FilteredDeparturesFromStopsOfInterest = new List<FlatPrediction>();


                // load in the list of holidays
                string query = $"select * from 'Data/RJT.holidays.clean.parquet';";
                List<DayType> DayTypes = DuckDB.Query<DayType>(query).ToList();                


                // SATURDAYS
                if (dateString.ToLower() == "saturdays")
                {
                    FilteredDeparturesFromStopsOfInterest = DeparturesFromStopsOfInterest.Where(x => x.ScheduledArrival.DayOfWeek == DayOfWeek.Saturday).ToList();
                }

                // SUNDAYS
                else if (dateString.ToLower() == "sundays")
                {
                    FilteredDeparturesFromStopsOfInterest = DeparturesFromStopsOfInterest.Where(x => x.ScheduledArrival.DayOfWeek == DayOfWeek.Sunday).ToList();
                }

                else if (dateString.ToLower() == "weekdays")
                {
                    FilteredDeparturesFromStopsOfInterest = DeparturesFromStopsOfInterest.Where(x => x.ScheduledArrival.DayOfWeek != DayOfWeek.Saturday && x.ScheduledArrival.DayOfWeek != DayOfWeek.Sunday).ToList();
                }

                // SCHOOL HOLIDAY WEEKDAY
                else if (dateString.ToLower() == "schoolholidayweekday")
                {
                    foreach (DayType day in DayTypes)
                    {
                        if (day.TypeOfDay == "BirminghamSchoolHoliday")
                        {
                            if (day.Date.DayOfWeek != DayOfWeek.Saturday && day.Date.DayOfWeek != DayOfWeek.Sunday)
                            {
                                FilteredDeparturesFromStopsOfInterest.AddRange(DeparturesFromStopsOfInterest.Where(x => x.ScheduledArrival.Date == day.Date).ToList());
                            }
                        }
                    }
                }

                // SCHOOL HOLIDAY WEEKEND
                else if (dateString.ToLower() == "schoolholidayweekend")
                {
                    foreach (DayType day in DayTypes)
                    {
                        if (day.TypeOfDay == "BirminghamSchoolHoliday")
                        {
                            if (day.Date.DayOfWeek == DayOfWeek.Saturday || day.Date.DayOfWeek == DayOfWeek.Sunday)
                            {
                                FilteredDeparturesFromStopsOfInterest.AddRange(DeparturesFromStopsOfInterest.Where(x => x.ScheduledArrival.Date == day.Date).ToList());
                            }
                        }
                    }
                }

                // BANK HOLIDAY
                else if (dateString.ToLower() == "bankholiday")
                {
                    foreach (DayType day in DayTypes)
                    {
                        if (day.TypeOfDay == "BankHoliday")
                        {
                            FilteredDeparturesFromStopsOfInterest.AddRange(DeparturesFromStopsOfInterest.Where(x => x.ScheduledArrival.Date == day.Date).ToList());
                        }
                    }
                }

                // this is a really niche case
                else if (dateString.ToLower() == "all")
                {
                    // this is a really expensive call -- we collect all historic journeys, no matter how old.
                    if (service == null)
                    {
                        DeparturesFromStopsOfInterest = DuckDB.Query<FlatPrediction>($"select * from 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND NaptanId = '{fromCode}' or NaptanId = '{toCode}')").ToList();
                        //DeparturesFromStopsOfInterest = flatPredictionsDB.Find(x => (x.NaptanId == fromCode || x.NaptanId == toCode)).ToList();
                    }
                    else
                    {
                        DeparturesFromStopsOfInterest = DuckDB.Query<FlatPrediction>($"select * from 'Data/RJT.flatprediction.clean.small.slim.parquet' where datepart('Year', ScheduledArrival) = '{year}' AND LineName = '{service}' and (NaptanId = '{fromCode}' or NaptanId = '{toCode}')").ToList();
                        //DeparturesFromStopsOfInterest = flatPredictionsDB.Find(x => x.LineName == service && (x.NaptanId == fromCode || x.NaptanId == toCode)).ToList();
                    }
                    foreach (FlatPrediction flatPrediction in DeparturesFromStopsOfInterest)
                    {
                        flatPrediction.UniqueDepartureId = flatPrediction.ScheduledArrival.Date.ToString("yyyy-MM-dd") + "_" + flatPrediction.Id;
                    }

                    FilteredDeparturesFromStopsOfInterest = DeparturesFromStopsOfInterest;


                }

                else if (IsADate == true)
                {
                    // THIS GETS DEPARTURES FOR A SINGLE DATE IT IS THE DEFAULT BEHAVIOUR
                    FilteredDeparturesFromStopsOfInterest = DeparturesFromStopsOfInterest.Where(x => x.ScheduledArrival.Date == parsedDate).ToList();
                }

                ConcurrentBag<string> uniqueIDs = new ConcurrentBag<string>(DeparturesFromStopsOfInterest.GroupBy(x => x.UniqueDepartureId).Select(g => g.First()).OrderBy(x => x.ScheduledArrival).Select(x => x.UniqueDepartureId));
                ConcurrentBag<JourneyTime> JourneyTimesBag = new ConcurrentBag<JourneyTime>();

                ILookup<string, FlatPrediction> FlatPredictionLookup = FilteredDeparturesFromStopsOfInterest.ToLookup(x => string.Concat(x.UniqueDepartureId, x.NaptanId), x => x);

                Parallel.ForEach(uniqueIDs, new ParallelOptions { MaxDegreeOfParallelism = 16 }, (uniqueID) =>
                {
                    //FlatPrediction ToStop = FilteredDeparturesFromStopsOfInterest.Where(x => x.UniqueDepartureId == uniqueID && x.NaptanId == toCode).FirstOrDefault();
                    //FlatPrediction FromStop = FilteredDeparturesFromStopsOfInterest.Where(x => x.UniqueDepartureId == uniqueID && x.NaptanId == fromCode).FirstOrDefault();
                    
                    FlatPrediction? ToStop = FlatPredictionLookup[string.Concat(uniqueID, toCode)].FirstOrDefault();
                    FlatPrediction? FromStop = FlatPredictionLookup[string.Concat(uniqueID, fromCode)].FirstOrDefault();

                    if (ToStop != null && FromStop != null)
                    {
                        if (ToStop.ExpectedArrival != null && FromStop.ExpectedArrival != null)
                        {
                            JourneyTime journeyTime = new JourneyTime();
                            journeyTime.ScheduledJourneyTime = ToStop.ScheduledArrival - FromStop.ScheduledArrival;
                            journeyTime.RealJourneyTime = ToStop.ExpectedArrival - FromStop.ExpectedArrival;
                            if ((ToStop.ExpectedArrival - FromStop.ExpectedArrival) != null)
                            {
                                journeyTime.RealJourneyTimeISO = XmlConvert.ToString((ToStop.ExpectedArrival - FromStop.ExpectedArrival).Value);
                            }
                            journeyTime.From = fromCode;
                            journeyTime.To = toCode;
                            journeyTime.ScheduledDepartureTime = FromStop.ScheduledArrival;
                            journeyTime.RealDepartureTime = FromStop.ExpectedArrival.Value;
                            journeyTime.Operator = ToStop.OperatorName;
                            journeyTime.Service = ToStop.LineName;
                            journeyTime.UniqueID = uniqueID;
                            JourneyTimesBag.Add(journeyTime);
                        }
                    }
                });

                List<JourneyTime> JourneyTimes = new List<JourneyTime>(JourneyTimesBag.ToList().OrderBy(x => x.RealDepartureTime));

                List<WorstJourneyTime> worstJourneyTimes = new List<WorstJourneyTime>();
                List<RealJourneyTime> realJourneyTimes = new List<RealJourneyTime>();

                // Now do binned worst journey times
                // When we're just looking at one day, it's easy.
                if (IsADate == true)
                {
                    int binSize = 15;

                    for (DateTime StartDateTime = parsedDate; StartDateTime < parsedDate.AddDays(1); StartDateTime = StartDateTime.AddMinutes(15))
                    {

                        List<JourneyTime> JourneysWithin15MinutesEachWay = JourneyTimes.Where(x => x.RealDepartureTime > StartDateTime.AddMinutes(-binSize) && x.RealDepartureTime < StartDateTime.AddMinutes(binSize)).ToList();
                        TimeSpan? WorstJourneyTime = null;
                        if (JourneysWithin15MinutesEachWay.Count > 0)
                        {
                            WorstJourneyTime = JourneysWithin15MinutesEachWay.OrderByDescending(x => x.RealJourneyTime).FirstOrDefault().RealJourneyTime.Value;
                        }

                        TimeSpan? ScheduledTimeSpan = null;
                        if (JourneyTimes.Where(x => x.RealDepartureTime > StartDateTime.AddMinutes(-binSize) && x.RealDepartureTime < StartDateTime.AddMinutes(binSize)).Count() > 0)
                        {
                            ScheduledTimeSpan = TimeSpan.FromMinutes(JourneyTimes.Where(x => x.RealDepartureTime > StartDateTime.AddMinutes(-binSize) && x.RealDepartureTime < StartDateTime.AddMinutes(binSize)).Average(x => x.ScheduledJourneyTime.TotalMinutes));
                        }

                        WorstJourneyTime worstJourneyTime = new WorstJourneyTime()
                        {
                            StartTime = StartDateTime,
                            JourneyTime = WorstJourneyTime,
                            ScheduledTime = ScheduledTimeSpan
                        };

                        worstJourneyTimes.Add(worstJourneyTime);
                    }


                    // create combined return object
                    CombinedReturnObject combinedReturnObject = new CombinedReturnObject()
                    {
                        JourneyTimes = JourneyTimes,
                        WorstJourneyTimes = worstJourneyTimes
                    };

                    // Output to JSON at the command line
                    return combinedReturnObject;

                }

                // but when we're looking at more than one day, that won't work. 
                else
                {
                    int binSize = 15;
                    for (TimeSpan StartTime = new TimeSpan(0); StartTime < new TimeSpan(1, 0, 0, 0); StartTime = StartTime.Add(new TimeSpan(0, 15, 0)))
                    {
                        List<JourneyTime> JourneysWithin15MinutesEachWay = JourneyTimes.Where(x => x.RealDepartureTime.TimeOfDay > StartTime.Add(new TimeSpan(0, -binSize, 0)) && x.RealDepartureTime.TimeOfDay < StartTime.Add(new TimeSpan(0, binSize, 0))).ToList();
                        TimeSpan? RJT95 = null;
                        TimeSpan? ScheduledTimeSpan = null;
                        if (JourneysWithin15MinutesEachWay.Count > 0)
                        {
                            List<JourneyTime> RJT95List = JourneysWithin15MinutesEachWay.OrderByDescending(x => x.RealJourneyTime).ToList();
                            RJT95 = RJT95List[(int)(RJT95List.Count * 0.05)].RealJourneyTime.Value;
                            ScheduledTimeSpan = RJT95List[(int)(RJT95List.Count * 0.05)].ScheduledJourneyTime;
                        }

                        RealJourneyTime realJourneyTime = new RealJourneyTime()
                        {
                            StartTime = StartTime,
                            Percentile = 95,
                            JourneyTime = RJT95,
                            ScheduledTime = ScheduledTimeSpan
                        };

                        realJourneyTimes.Add(realJourneyTime);
                    }

                    // create combined return object
                    CombinedReturnObject combinedReturnObjectPercentiles = new CombinedReturnObject()
                    {
                        JourneyTimes = JourneyTimes,
                        RealJourneyTimes = realJourneyTimes
                    };

                    return combinedReturnObjectPercentiles;
                }
            }
        }
    }
}