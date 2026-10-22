namespace IntelligentUrbanRouting.Models
{
    public class Location
    {
        public string LocationID { get; set; }

        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }


        public Location(
            string locationID,
            string name,
            double latitude,
            double longitude)
        {
            LocationID = locationID;
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}