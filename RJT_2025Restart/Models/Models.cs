using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RJT_ASPNET.Models
{
    public class Models
    {
        public class ReachableStop
        {
            public string FromStop { get; set; }
            public string ToStop { get; set; }
        }

        public class DayType
        {
            public BsonObjectId _id { get; set; }
            public string TypeOfDay { get; set; }
            public DateTime Date { get; set; }
        }


        public class CombinedReturnObject
        {
            public List<JourneyTime> JourneyTimes { get; set; }
            public List<WorstJourneyTime> WorstJourneyTimes { get; set; }
        }

        public class CombinedReturnObjectPercentiles
        {
            public List<JourneyTime> JourneyTimes { get; set; }
            public List<RealJourneyTime> RealJourneyTimes { get; set; }
        }

        public class WorstJourneyTime
        {
            public DateTime StartTime { get; set; }
            public TimeSpan? JourneyTime { get; set; }
            public TimeSpan? ScheduledTime { get; set; }
        }

        public class RealJourneyTime
        {
            public int Percentile { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan? JourneyTime { get; set; }
            public TimeSpan? ScheduledTime { get; set; }
        }

        public class JourneyTime
        {
            public DateTime ScheduledDepartureTime { get; set; }
            public DateTime RealDepartureTime { get; set; }
            public string RealJourneyTimeISO { get; set; }
            public TimeSpan? RealJourneyTime { get; set; }
            public TimeSpan ScheduledJourneyTime { get; set; }
            public string Operator { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string Service { get; set; }
            public string UniqueID { get; set; }
        }

        public class BusJourney
        {
            public string Id { get; set; }
            public string NaptanId { get; set; }
            public string stop_name { get; set; }
            public double stop_lat { get; set; }
            public double stop_lon { get; set; }
            public string ExpectedArrival { get; set; }
            public string ScheduledArrival { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class NaptanStop
        {
            [BsonId]
            [BsonElement("_id")]
            public string stop_id { get; set; }
            [BsonElement("NaptanCode")]
            public string stop_code { get; set; }
            [BsonElement("CommonName")]
            public string stop_name { get; set; }
            [BsonElement("Latitude")]
            public double stop_lat { get; set; }
            [BsonElement("Longitude")]
            public double stop_lon { get; set; }
            [BsonElement("StopType")]
            public string vehicle_type { get; set; }
        }

        public class FlatPrediction
        {
            [BsonId]
            public string _id { get; set; }

            public string uniqueKey { get; set; }
            public string UniqueDepartureId { get; set; } // this Id is calculated later and is unique across days
            public string Id { get; set; } // this Id is only unique per day
            public string DestinationName { get; set; }
            public string DestinationNaptanId { get; set; }
            public string Direction { get; set; }
            public DateTime? ExpectedArrival { get; set; }
            public DateTime ScheduledArrival { get; set; }
            public string ModeName { get; set; }
            public int? StopSequence { get; set; }
            public string NaptanId { get; set; }
            public string LineId { get; set; }
            public string LineName { get; set; }
            public string OperatorName { get; set; }
            public string City { get; set; }
        }
    }
}
