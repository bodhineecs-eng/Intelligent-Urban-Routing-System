using System.Globalization;
using System.Text;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Data
{
    public sealed class CsvDataWriter
    {
        private readonly string locationsFilePath;
        private readonly string roadsFilePath;
        private readonly object syncRoot = new object();


        public CsvDataWriter(
            string locationsFilePath,
            string roadsFilePath)
        {
            this.locationsFilePath =
                locationsFilePath;

            this.roadsFilePath =
                roadsFilePath;

            EnsureFileExists(
                locationsFilePath,
                "Locations CSV file was not found.");

            EnsureFileExists(
                roadsFilePath,
                "Roads CSV file was not found.");
        }


        public Location SaveLocation(
            string name,
            double latitude,
            double longitude)
        {
            ValidateCsvText(
                name,
                "Location name");

            if (latitude < -90 ||
                latitude > 90 ||
                double.IsNaN(latitude))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(latitude),
                    "Latitude must be between -90 and 90.");
            }

            if (longitude < -180 ||
                longitude > 180 ||
                double.IsNaN(longitude))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(longitude),
                    "Longitude must be between -180 and 180.");
            }

            lock (syncRoot)
            {
                string locationID =
                    GetNextID(
                        locationsFilePath,
                        "location_id");

                Location location =
                    new Location(
                        locationID,
                        name.Trim(),
                        latitude,
                        longitude);

                AppendRecord(
                    locationsFilePath,
                    string.Join(
                        ",",
                        location.LocationID,
                        location.Name,
                        location.Latitude.ToString(
                            "R",
                            CultureInfo.InvariantCulture),
                        location.Longitude.ToString(
                            "R",
                            CultureInfo.InvariantCulture)));

                return location;
            }
        }


        public Road SaveRoad(
            string from,
            string to,
            string roadName,
            int travelTime,
            int congestionLevel,
            string congestionCategory,
            string roadType,
            string status)
        {
            ValidateCsvText(
                from,
                "From location ID");

            ValidateCsvText(
                to,
                "To location ID");

            ValidateCsvText(
                roadName,
                "Road name");

            ValidateCsvText(
                congestionCategory,
                "Congestion category");

            ValidateCsvText(
                roadType,
                "Road type");

            ValidateCsvText(
                status,
                "Status");

            if (travelTime < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(travelTime),
                    "Travel time cannot be negative.");
            }

            if (congestionLevel < 1 ||
                congestionLevel > 5)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(congestionLevel),
                    "Congestion level must be between 1 and 5.");
            }

            bool validStatus =
                status.Equals(
                    "Open",
                    StringComparison.OrdinalIgnoreCase) ||
                status.Equals(
                    "Close",
                    StringComparison.OrdinalIgnoreCase) ||
                status.Equals(
                    "Closed",
                    StringComparison.OrdinalIgnoreCase);

            if (!validStatus)
            {
                throw new ArgumentException(
                    "Status must be Open, Close, or Closed.",
                    nameof(status));
            }

            lock (syncRoot)
            {
                string roadID =
                    GetNextID(
                        roadsFilePath,
                        "road_id");

                Road road =
                    new Road(
                        roadID,
                        from.Trim(),
                        to.Trim(),
                        roadName.Trim(),
                        travelTime,
                        congestionLevel,
                        congestionCategory.Trim(),
                        roadType.Trim(),
                        status.Trim());

                AppendRecord(
                    roadsFilePath,
                    string.Join(
                        ",",
                        road.RoadID,
                        road.From,
                        road.To,
                        road.RoadName,
                        road.TravelTime.ToString(
                            CultureInfo.InvariantCulture),
                        road.Traffic.ToString(
                            CultureInfo.InvariantCulture),
                        road.CongestionCategory,
                        road.RoadType,
                        road.Status));

                return road;
            }
        }


        private static string GetNextID(
            string filePath,
            string columnName)
        {
            int maximumID = 0;
            int lineNumber = 0;

            foreach (string line in File.ReadLines(filePath))
            {
                lineNumber++;

                if (lineNumber == 1 ||
                    string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int separatorIndex =
                    line.IndexOf(',');

                string idText =
                    (separatorIndex >= 0
                        ? line[..separatorIndex]
                        : line).Trim();

                if (!int.TryParse(
                        idText,
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out int currentID))
                {
                    throw new FormatException(
                        "Invalid " +
                        columnName +
                        " at line " +
                        lineNumber +
                        ".");
                }

                maximumID =
                    Math.Max(
                        maximumID,
                        currentID);
            }

            return checked(maximumID + 1)
                .ToString(
                    CultureInfo.InvariantCulture);
        }


        private static void AppendRecord(
            string filePath,
            string record)
        {
            string linePrefix =
                EndsWithLineBreak(filePath)
                    ? string.Empty
                    : Environment.NewLine;

            File.AppendAllText(
                filePath,
                linePrefix +
                record +
                Environment.NewLine,
                new UTF8Encoding(false));
        }


        private static bool EndsWithLineBreak(
            string filePath)
        {
            using FileStream stream =
                new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

            if (stream.Length == 0)
            {
                return true;
            }

            stream.Seek(
                -1,
                SeekOrigin.End);

            int finalByte =
                stream.ReadByte();

            return finalByte == '\n' ||
                   finalByte == '\r';
        }


        private static void ValidateCsvText(
            string value,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    fieldName +
                    " cannot be empty.");
            }

            if (value.Contains(',') ||
                value.Contains('\r') ||
                value.Contains('\n'))
            {
                throw new ArgumentException(
                    fieldName +
                    " cannot contain commas or line breaks.");
            }
        }


        private static void EnsureFileExists(
            string filePath,
            string message)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    message,
                    filePath);
            }
        }
    }
}
