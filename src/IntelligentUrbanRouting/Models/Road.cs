namespace IntelligentUrbanRouting.Models
{
    public class Road
    {
        public string RoadID { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string RoadName { get; set; }

        public int TravelTime { get; set; }

        public int Traffic { get; set; }

        public string CongestionCategory { get; set; }

        public string RoadType { get; set; }

        public string Status { get; set; }


        public Road(
            string roadID,
            string from,
            string to,
            string roadName,
            int travelTime,
            int traffic,
            string congestionCategory,
            string roadType,
            string status)
        {
            RoadID = roadID;
            From = from;
            To = to;
            RoadName = roadName;
            TravelTime = travelTime;
            Traffic = traffic;
            CongestionCategory = congestionCategory;
            RoadType = roadType;
            Status = status;
        }
    }
}