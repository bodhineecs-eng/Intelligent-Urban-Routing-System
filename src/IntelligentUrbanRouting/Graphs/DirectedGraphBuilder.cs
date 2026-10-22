using System;
using System.Collections.Generic;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Graphs
{
    public class DirectedGraphBuilder
    {
        public Dictionary<string, List<string>> Build(
            RoadGraph roadGraph,
            bool reverse = false,
            bool includeClosedRoads = false)
        {
            Dictionary<string, List<string>> directedGraph =
                new Dictionary<string, List<string>>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (Location location in roadGraph.GetAllLocations())
            {
                directedGraph[location.LocationID] =
                    new List<string>();
            }

            foreach (Road road in roadGraph.GetAllRoads())
            {
                if (!includeClosedRoads && IsClosed(road))
                {
                    continue;
                }

                string from = reverse ? road.To : road.From;
                string to = reverse ? road.From : road.To;

                if (!directedGraph.ContainsKey(from) ||
                    !directedGraph.ContainsKey(to))
                {
                    continue;
                }

                if (!directedGraph[from].Contains(to))
                {
                    directedGraph[from].Add(to);
                }
            }

            return directedGraph;
        }

        private static bool IsClosed(Road road)
        {
            return road.Status.Equals(
                       "Close",
                       StringComparison.OrdinalIgnoreCase) ||
                   road.Status.Equals(
                       "Closed",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
