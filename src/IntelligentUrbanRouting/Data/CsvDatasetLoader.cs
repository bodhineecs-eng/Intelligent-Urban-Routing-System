using System;
using System.Globalization;
using System.IO;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Data
{
    public class CsvDatasetLoader
    {
        public RoadGraph LoadDataset(
            string nodesFilePath,
            string edgesFilePath)
        {
            RoadGraph graph =
                new RoadGraph();


            LoadLocations(
                nodesFilePath,
                graph);


            LoadRoads(
                edgesFilePath,
                graph);


            return graph;
        }


        // =====================================================
        // LOAD LOCATIONS
        // =====================================================

        private void LoadLocations(
            string filePath,
            RoadGraph graph)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Nodes CSV file was not found.",
                    filePath);
            }


            string[] lines =
                File.ReadAllLines(filePath);


            // Start from 1 to skip the CSV header.
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(
                    lines[i]))
                {
                    continue;
                }


                string[] data =
                    lines[i].Split(',');


                if (data.Length < 4)
                {
                    throw new FormatException(
                        "Invalid location data at line " +
                        (i + 1));
                }


                string locationID =
                    data[0].Trim();


                string name =
                    data[1].Trim();


                if (string.IsNullOrWhiteSpace(
                    locationID))
                {
                    throw new FormatException(
                        "LocationID is empty at line " +
                        (i + 1));
                }


                if (string.IsNullOrWhiteSpace(
                    name))
                {
                    throw new FormatException(
                        "Location name is empty at line " +
                        (i + 1));
                }


                if (!double.TryParse(
                    data[2].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double latitude))
                {
                    throw new FormatException(
                        "Invalid latitude at line " +
                        (i + 1));
                }


                if (!double.TryParse(
                    data[3].Trim(),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double longitude))
                {
                    throw new FormatException(
                        "Invalid longitude at line " +
                        (i + 1));
                }


                Location location =
                    new Location(
                        locationID,
                        name,
                        latitude,
                        longitude);


                graph.AddLocation(
                    location);
            }
        }


        // =====================================================
        // LOAD ROADS
        // =====================================================

        private void LoadRoads(
            string filePath,
            RoadGraph graph)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Edges CSV file was not found.",
                    filePath);
            }


            string[] lines =
                File.ReadAllLines(filePath);


            // Start from 1 to skip the CSV header.
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(
                    lines[i]))
                {
                    continue;
                }


                string[] data =
                    lines[i].Split(',');


                if (data.Length < 9)
                {
                    throw new FormatException(
                        "Invalid road data at line " +
                        (i + 1));
                }


                string roadID =
                    data[0].Trim();

                string from =
                    data[1].Trim();

                string to =
                    data[2].Trim();

                string roadName =
                    data[3].Trim();


                if (!int.TryParse(
                    data[4].Trim(),
                    out int travelTime))
                {
                    throw new FormatException(
                        "Invalid travel time at line " +
                        (i + 1));
                }


                if (!int.TryParse(
                    data[5].Trim(),
                    out int traffic))
                {
                    throw new FormatException(
                        "Invalid traffic level at line " +
                        (i + 1));
                }


                string congestionCategory =
                    data[6].Trim();

                string roadType =
                    data[7].Trim();

                string status =
                    data[8].Trim();


                if (string.IsNullOrWhiteSpace(
                    roadID))
                {
                    throw new FormatException(
                        "RoadID is empty at line " +
                        (i + 1));
                }


                if (!graph.ContainsLocation(
                    from))
                {
                    throw new FormatException(
                        "Road " +
                        roadID +
                        " contains unknown source location: " +
                        from);
                }


                if (!graph.ContainsLocation(
                    to))
                {
                    throw new FormatException(
                        "Road " +
                        roadID +
                        " contains unknown destination location: " +
                        to);
                }


                Road road =
                    new Road(
                        roadID,
                        from,
                        to,
                        roadName,
                        travelTime,
                        traffic,
                        congestionCategory,
                        roadType,
                        status);


                graph.AddRoad(
                    road);
            }
        }
    }
}