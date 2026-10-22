using System;
using System.Collections.Generic;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Graphs
{
    public class RoadGraph
    {
        private readonly Dictionary<string, Location>
            locations;


        private readonly Dictionary<string, List<Road>>
            adjacencyList;


        private readonly HashSet<string>
            roadIDs;


        public RoadGraph()
        {
            locations =
                new Dictionary<string, Location>(
                    StringComparer.OrdinalIgnoreCase);


            adjacencyList =
                new Dictionary<string, List<Road>>(
                    StringComparer.OrdinalIgnoreCase);


            roadIDs =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
        }


        // =====================================================
        // ADD LOCATION
        // =====================================================

        public void AddLocation(
            Location location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(
                    nameof(location));
            }


            if (string.IsNullOrWhiteSpace(
                location.LocationID))
            {
                throw new ArgumentException(
                    "LocationID cannot be empty.");
            }


            if (locations.ContainsKey(
                location.LocationID))
            {
                throw new ArgumentException(
                    "Duplicate LocationID: " +
                    location.LocationID);
            }


            locations.Add(
                location.LocationID,
                location);


            adjacencyList.Add(
                location.LocationID,
                new List<Road>());
        }


        // =====================================================
        // ADD ROAD
        // =====================================================

        public void AddRoad(
            Road road)
        {
            if (road == null)
            {
                throw new ArgumentNullException(
                    nameof(road));
            }


            if (string.IsNullOrWhiteSpace(
                road.RoadID))
            {
                throw new ArgumentException(
                    "RoadID cannot be empty.");
            }


            if (roadIDs.Contains(
                road.RoadID))
            {
                throw new ArgumentException(
                    "Duplicate RoadID: " +
                    road.RoadID);
            }


            if (!locations.ContainsKey(
                road.From))
            {
                throw new ArgumentException(
                    "Unknown source location: " +
                    road.From);
            }


            if (!locations.ContainsKey(
                road.To))
            {
                throw new ArgumentException(
                    "Unknown destination location: " +
                    road.To);
            }


            // Dijkstra requires non-negative edge weights.
            if (road.TravelTime < 0)
            {
                throw new ArgumentException(
                    "Travel time cannot be negative.");
            }


            // Traffic scale = 1 to 5.
            if (road.Traffic < 1 ||
                road.Traffic > 5)
            {
                throw new ArgumentException(
                    "Traffic level must be between 1 and 5.");
            }


            bool validStatus =
                road.Status.Equals(
                    "Open",
                    StringComparison.OrdinalIgnoreCase) ||

                road.Status.Equals(
                    "Close",
                    StringComparison.OrdinalIgnoreCase) ||

                road.Status.Equals(
                    "Closed",
                    StringComparison.OrdinalIgnoreCase);


            if (!validStatus)
            {
                throw new ArgumentException(
                    "Invalid road status: " +
                    road.Status);
            }


            adjacencyList[road.From]
                .Add(road);


            roadIDs.Add(
                road.RoadID);
        }


        // =====================================================
        // GET OUTGOING ROADS
        // =====================================================

        public IReadOnlyList<Road> GetRoads(
            string locationID)
        {
            if (adjacencyList.TryGetValue(
                locationID,
                out List<Road>? roads))
            {
                return roads.AsReadOnly();
            }


            throw new KeyNotFoundException(
                "Location " +
                locationID +
                " does not exist.");
        }


        // =====================================================
        // GET LOCATION
        // =====================================================

        public Location GetLocation(
            string locationID)
        {
            if (locations.TryGetValue(
                locationID,
                out Location? location))
            {
                return location;
            }


            throw new KeyNotFoundException(
                "Location " +
                locationID +
                " does not exist.");
        }


        // =====================================================
        // GET ALL LOCATIONS
        // =====================================================

        public List<Location> GetAllLocations()
        {
            return new List<Location>(
                locations.Values);
        }


        // =====================================================
        // GET ALL ROADS
        // =====================================================

        public List<Road> GetAllRoads()
        {
            List<Road> allRoads =
                new List<Road>();


            foreach (
                List<Road> roads
                in adjacencyList.Values)
            {
                allRoads.AddRange(
                    roads);
            }


            return allRoads;
        }


        // =====================================================
        // CHECK LOCATION
        // =====================================================

        public bool ContainsLocation(
            string locationID)
        {
            if (string.IsNullOrWhiteSpace(
                locationID))
            {
                return false;
            }


            return locations.ContainsKey(
                locationID);
        }


        // =====================================================
        // CHECK ROAD
        // =====================================================

        public bool ContainsRoad(
            string roadID)
        {
            if (string.IsNullOrWhiteSpace(
                roadID))
            {
                return false;
            }


            return roadIDs.Contains(
                roadID);
        }


        // =====================================================
        // LOCATION COUNT
        // =====================================================

        public int LocationCount()
        {
            return locations.Count;
        }


        // =====================================================
        // ROAD COUNT
        // =====================================================

        public int RoadCount()
        {
            return roadIDs.Count;
        }
    }
}