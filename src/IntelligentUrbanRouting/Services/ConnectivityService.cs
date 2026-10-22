using System.Collections.Generic;
using IntelligentUrbanRouting.Algorithms;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Services
{
    public class ConnectivityService
    {
        private readonly RoadGraph roadGraph;

        private readonly StructuralGraphBuilder
            structuralGraphBuilder;

        private readonly DirectedGraphBuilder
            directedGraphBuilder;

        private readonly BreadthFirstSearch bfs;


        public ConnectivityService(
            RoadGraph graph)
        {
            roadGraph = graph;

            structuralGraphBuilder =
                new StructuralGraphBuilder();

            directedGraphBuilder =
                new DirectedGraphBuilder();

            bfs =
                new BreadthFirstSearch();
        }


        public bool IsConnected()
        {
            return IsWeaklyConnected();
        }


        // Physical/weak connectivity: every recorded road is
        // treated as two-way and closed roads are included.
        public bool IsWeaklyConnected()
        {
            if (roadGraph.LocationCount() == 0)
            {
                return false;
            }


            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(
                        roadGraph);


            List<Location> locations =
                roadGraph.GetAllLocations();


            string start =
                locations[0].LocationID;


            HashSet<string> visited =
                bfs.Traverse(
                    structuralGraph,
                    start);


            return visited.Count ==
                   roadGraph.LocationCount();
        }


        // Operational strong connectivity: road direction is
        // preserved and closed roads are excluded. A graph is
        // strongly connected when one BFS reaches every node in
        // both the graph and its reversed graph.
        public bool IsStronglyConnected()
        {
            if (roadGraph.LocationCount() == 0)
            {
                return false;
            }

            List<Location> locations =
                roadGraph.GetAllLocations();

            string start =
                locations[0].LocationID;

            Dictionary<string, List<string>> forwardGraph =
                directedGraphBuilder.Build(
                    roadGraph,
                    reverse: false,
                    includeClosedRoads: false);

            HashSet<string> forwardVisited =
                bfs.Traverse(
                    forwardGraph,
                    start);

            if (forwardVisited.Count !=
                roadGraph.LocationCount())
            {
                return false;
            }

            Dictionary<string, List<string>> reversedGraph =
                directedGraphBuilder.Build(
                    roadGraph,
                    reverse: true,
                    includeClosedRoads: false);

            HashSet<string> reverseVisited =
                bfs.Traverse(
                    reversedGraph,
                    start);

            return reverseVisited.Count ==
                   roadGraph.LocationCount();
        }
    }
}
