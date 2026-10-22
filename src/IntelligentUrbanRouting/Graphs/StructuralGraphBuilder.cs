using System.Collections.Generic;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Graphs
{
    public class StructuralGraphBuilder
    {
        public Dictionary<string, List<string>> Build(
            RoadGraph roadGraph)
        {
            Dictionary<string, List<string>>
                structuralGraph =
                    new Dictionary<string, List<string>>(
                        System.StringComparer.OrdinalIgnoreCase);


            // =================================================
            // ADD ALL LOCATIONS
            // =================================================

            foreach (
                Location location
                in roadGraph.GetAllLocations())
            {
                structuralGraph[
                    location.LocationID] =
                        new List<string>();
            }


            // =================================================
            // CREATE UNDIRECTED STRUCTURAL CONNECTIONS
            //
            // Direction and road status are intentionally
            // ignored. The result represents physical/weak
            // connectivity, including closed infrastructure.
            // =================================================

            foreach (
                Road road
                in roadGraph.GetAllRoads())
            {
                if (!structuralGraph.ContainsKey(
                        road.From) ||
                    !structuralGraph.ContainsKey(
                        road.To))
                {
                    continue;
                }


                // From -> To
                if (!structuralGraph[road.From]
                    .Contains(road.To))
                {
                    structuralGraph[road.From]
                        .Add(road.To);
                }


                // To -> From
                if (!structuralGraph[road.To]
                    .Contains(road.From))
                {
                    structuralGraph[road.To]
                        .Add(road.From);
                }
            }


            return structuralGraph;
        }
    }
}
