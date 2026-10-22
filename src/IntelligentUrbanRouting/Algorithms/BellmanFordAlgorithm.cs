using System;
using System.Collections.Generic;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Algorithms
{
    public class BellmanFordAlgorithm
    {
        private readonly RoadGraph graph;


        public BellmanFordAlgorithm(
            RoadGraph graph)
        {
            this.graph =
                graph ??
                throw new ArgumentNullException(
                    nameof(graph));
        }


        // =====================================================
        // FIND SHORTEST PATH
        // =====================================================

        public PathResult FindShortestPath(
            string start,
            string destination,
            RoutingConstraints? constraints = null)
        {
            PathResult result =
                new PathResult();


            if (!graph.ContainsLocation(start) ||
                !graph.ContainsLocation(destination))
            {
                return result;
            }


            constraints ??=
                new RoutingConstraints();


            // The source or destination cannot itself
            // be an excluded location.
            if (constraints
                    .ExcludedLocationIDs
                    .Contains(start) ||
                constraints
                    .ExcludedLocationIDs
                    .Contains(destination))
            {
                return result;
            }


            Dictionary<string, int> distances =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);


            Dictionary<string, string> previous =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);


            List<Location> locations =
                graph.GetAllLocations();


            List<Road> roads =
                graph.GetAllRoads();


            foreach (
                Location location
                in locations)
            {
                distances[
                    location.LocationID] =
                        int.MaxValue;
            }


            distances[start] = 0;


            // =================================================
            // RELAX ALL EDGES V - 1 TIMES
            // =================================================

            for (int i = 0;
                 i < locations.Count - 1;
                 i++)
            {
                bool updated = false;


                foreach (
                    Road road
                    in roads)
                {
                    if (!IsRoadAllowed(
                        road,
                        constraints))
                    {
                        continue;
                    }


                    if (distances[road.From] ==
                        int.MaxValue)
                    {
                        continue;
                    }


                    int newDistance =
                        distances[road.From] +
                        road.TravelTime;


                    if (newDistance <
                        distances[road.To])
                    {
                        distances[road.To] =
                            newDistance;


                        previous[road.To] =
                            road.From;


                        updated = true;
                    }
                }


                // If an entire pass makes no changes,
                // shortest distances are already stable.
                if (!updated)
                {
                    break;
                }
            }


            // =================================================
            // NEGATIVE-CYCLE CHECK
            // =================================================

            foreach (
                Road road
                in roads)
            {
                if (!IsRoadAllowed(
                    road,
                    constraints))
                {
                    continue;
                }


                if (distances[road.From] ==
                    int.MaxValue)
                {
                    continue;
                }


                if (distances[road.From] +
                    road.TravelTime <
                    distances[road.To])
                {
                    throw new InvalidOperationException(
                        "A negative-weight cycle was detected.");
                }
            }


            // =================================================
            // NO PATH
            // =================================================

            if (distances[destination] ==
                int.MaxValue)
            {
                return result;
            }


            // =================================================
            // RECONSTRUCT PATH
            // =================================================

            List<string> path =
                new List<string>();


            string current =
                destination;


            while (!string.Equals(
                current,
                start,
                StringComparison.OrdinalIgnoreCase))
            {
                path.Add(
                    current);


                if (!previous.TryGetValue(
                    current,
                    out string? previousLocation))
                {
                    return new PathResult();
                }


                current =
                    previousLocation;
            }


            path.Add(
                start);


            path.Reverse();


            result.PathFound = true;

            result.Path = path;

            result.TotalTravelTime =
                distances[destination];


            return result;
        }


        // =====================================================
        // CHECK ROUTING CONSTRAINTS
        //
        // This must use the same filtering rules as Dijkstra
        // so the performance comparison is fair.
        // =====================================================

        private bool IsRoadAllowed(
            Road road,
            RoutingConstraints constraints)
        {
            // 1. Selected RoadID
            if (constraints
                .ExcludedRoadIDs
                .Contains(road.RoadID))
            {
                return false;
            }


            // 2. Selected LocationID
            if (constraints
                    .ExcludedLocationIDs
                    .Contains(road.From) ||
                constraints
                    .ExcludedLocationIDs
                    .Contains(road.To))
            {
                return false;
            }


            // 3. Closed road
            bool isClosed =
                road.Status.Equals(
                    "Close",
                    StringComparison.OrdinalIgnoreCase) ||
                road.Status.Equals(
                    "Closed",
                    StringComparison.OrdinalIgnoreCase);


            if (constraints.AvoidClosedRoads &&
                isClosed)
            {
                return false;
            }


            // 4. High traffic
            // Levels 4 and 5 are high traffic.
            if (constraints.AvoidHighTraffic &&
                road.Traffic > 3)
            {
                return false;
            }


            // 5. Selected road type
            if (constraints.RestrictRoadTypes &&
                constraints
                    .RestrictedRoadTypes
                    .Contains(road.RoadType))
            {
                return false;
            }


            return true;
        }
    }
}