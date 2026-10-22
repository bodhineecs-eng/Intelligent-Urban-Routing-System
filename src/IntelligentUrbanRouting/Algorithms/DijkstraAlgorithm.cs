using System;
using System.Collections.Generic;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Algorithms
{
    public class DijkstraAlgorithm
    {
        private readonly RoadGraph graph;


        public DijkstraAlgorithm(
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


            // An excluded location cannot be the
            // source or destination.
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


            HashSet<string> visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            PriorityQueue<string, int> priorityQueue =
                new PriorityQueue<string, int>();


            foreach (
                Location location
                in graph.GetAllLocations())
            {
                distances[
                    location.LocationID] =
                        int.MaxValue;
            }


            distances[start] = 0;


            priorityQueue.Enqueue(
                start,
                0);


            while (priorityQueue.Count > 0)
            {
                string current =
                    priorityQueue.Dequeue();


                if (visited.Contains(
                    current))
                {
                    continue;
                }


                visited.Add(
                    current);


                if (string.Equals(
                    current,
                    destination,
                    StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }


                foreach (
                    Road road
                    in graph.GetRoads(current))
                {
                    if (!IsRoadAllowed(
                        road,
                        constraints))
                    {
                        continue;
                    }


                    string neighbour =
                        road.To;


                    if (!distances.ContainsKey(
                        neighbour))
                    {
                        continue;
                    }


                    int newDistance =
                        distances[current] +
                        road.TravelTime;


                    if (newDistance <
                        distances[neighbour])
                    {
                        distances[neighbour] =
                            newDistance;


                        previous[neighbour] =
                            current;


                        priorityQueue.Enqueue(
                            neighbour,
                            newDistance);
                    }
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


            string currentLocation =
                destination;


            while (!string.Equals(
                currentLocation,
                start,
                StringComparison.OrdinalIgnoreCase))
            {
                path.Add(
                    currentLocation);


                if (!previous.ContainsKey(
                    currentLocation))
                {
                    return new PathResult();
                }


                currentLocation =
                    previous[currentLocation];
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
        // CALCULATE ALL SHORTEST DISTANCES
        // =====================================================

        public Dictionary<string, int> CalculateDistances(
            string start,
            RoutingConstraints? constraints = null)
        {
            Dictionary<string, int> distances =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);


            if (!graph.ContainsLocation(start))
            {
                return distances;
            }


            constraints ??=
                new RoutingConstraints();


            if (constraints
                .ExcludedLocationIDs
                .Contains(start))
            {
                return distances;
            }


            PriorityQueue<string, int> priorityQueue =
                new PriorityQueue<string, int>();


            HashSet<string> visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            foreach (
                Location location
                in graph.GetAllLocations())
            {
                distances[
                    location.LocationID] =
                        int.MaxValue;
            }


            distances[start] = 0;


            priorityQueue.Enqueue(
                start,
                0);


            while (priorityQueue.Count > 0)
            {
                string current =
                    priorityQueue.Dequeue();


                if (visited.Contains(
                    current))
                {
                    continue;
                }


                visited.Add(
                    current);


                foreach (
                    Road road
                    in graph.GetRoads(current))
                {
                    if (!IsRoadAllowed(
                        road,
                        constraints))
                    {
                        continue;
                    }


                    string neighbour =
                        road.To;


                    if (!distances.ContainsKey(
                        neighbour))
                    {
                        continue;
                    }


                    int newDistance =
                        distances[current] +
                        road.TravelTime;


                    if (newDistance <
                        distances[neighbour])
                    {
                        distances[neighbour] =
                            newDistance;


                        priorityQueue.Enqueue(
                            neighbour,
                            newDistance);
                    }
                }
            }


            return distances;
        }


        // =====================================================
        // CHECK WHETHER ROAD SATISFIES CONSTRAINTS
        // =====================================================

        private bool IsRoadAllowed(
            Road road,
            RoutingConstraints constraints)
        {
            // =================================================
            // 1. SELECTED ROAD EXCLUSION
            // =================================================

            if (constraints
                .ExcludedRoadIDs
                .Contains(road.RoadID))
            {
                return false;
            }


            // =================================================
            // 2. SELECTED LOCATION EXCLUSION
            // =================================================

            if (constraints
                    .ExcludedLocationIDs
                    .Contains(road.From) ||

                constraints
                    .ExcludedLocationIDs
                    .Contains(road.To))
            {
                return false;
            }


            // =================================================
            // 3. CLOSED ROAD
            // =================================================

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


            // =================================================
            // 4. HIGH TRAFFIC
            //
            // 1 = Very Low
            // 2 = Low
            // 3 = Moderate
            // 4 = High
            // 5 = Very High
            //
            // Therefore levels 4 and 5 are excluded.
            // =================================================

            if (constraints.AvoidHighTraffic &&
                road.Traffic > 3)
            {
                return false;
            }


            // =================================================
            // 5. SELECTED ROAD-TYPE RESTRICTION
            // =================================================

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
