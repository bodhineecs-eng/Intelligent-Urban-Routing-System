using System;
using System.Collections.Generic;
using IntelligentUrbanRouting.Algorithms;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Services
{
    public class ReachabilityService
    {
        private readonly DijkstraAlgorithm dijkstra;


        public ReachabilityService(
            RoadGraph graph)
        {
            dijkstra =
                new DijkstraAlgorithm(
                    graph ??
                    throw new ArgumentNullException(
                        nameof(graph)));
        }


        public Dictionary<string, int>
            FindReachableLocations(
                string start,
                int timeBudget)
        {
            if (timeBudget < 0)
            {
                return new Dictionary<string, int>();
            }


            Dictionary<string, int> reachable =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);


            RoutingConstraints constraints =
                new RoutingConstraints();


            // Reachability represents locations
            // reachable using currently usable roads.
            constraints.AvoidClosedRoads = true;


            Dictionary<string, int> distances =
                dijkstra.CalculateDistances(
                    start,
                    constraints);


            foreach (
                KeyValuePair<string, int> location
                in distances)
            {
                // Do not include source.
                if (string.Equals(
                    location.Key,
                    start,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                // Ignore unreachable locations.
                if (location.Value ==
                    int.MaxValue)
                {
                    continue;
                }


                // Include only locations whose shortest
                // travel time is within the budget.
                if (location.Value <=
                    timeBudget)
                {
                    reachable.Add(
                        location.Key,
                        location.Value);
                }
            }


            return reachable;
        }
    }
}