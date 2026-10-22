using System;
using System.Collections.Generic;

namespace IntelligentUrbanRouting.Algorithms
{
    public class TarjanAlgorithm
    {
        private Dictionary<string, List<string>> graph;

        private Dictionary<string, int> discoveryTime;

        private Dictionary<string, int> lowTime;

        private Dictionary<string, string?> parent;

        private HashSet<string> articulationPoints;

        private List<(string From, string To)> bridges;

        private HashSet<string> visited;

        private int time;


        public TarjanAlgorithm()
        {
            graph =
                new Dictionary<string, List<string>>();

            discoveryTime =
                new Dictionary<string, int>();

            lowTime =
                new Dictionary<string, int>();

            parent =
                new Dictionary<string, string?>();

            articulationPoints =
                new HashSet<string>();

            bridges =
                new List<(string From, string To)>();

            visited =
                new HashSet<string>();

            time = 0;
        }


        // =====================================================
        // FIND ARTICULATION POINTS
        // =====================================================

        public HashSet<string> FindArticulationPoints(
            Dictionary<string, List<string>> structuralGraph)
        {
            RunTarjan(
                structuralGraph);

            return articulationPoints;
        }


        // =====================================================
        // FIND BRIDGES
        // =====================================================

        public List<(string From, string To)> FindBridges(
            Dictionary<string, List<string>> structuralGraph)
        {
            RunTarjan(
                structuralGraph);

            return bridges;
        }


        // =====================================================
        // RUN TARJAN ALGORITHM
        // =====================================================

        private void RunTarjan(
            Dictionary<string, List<string>> structuralGraph)
        {
            graph =
                structuralGraph;


            discoveryTime =
                new Dictionary<string, int>();


            lowTime =
                new Dictionary<string, int>();


            parent =
                new Dictionary<string, string?>();


            articulationPoints =
                new HashSet<string>();


            bridges =
                new List<(string From, string To)>();


            visited =
                new HashSet<string>();


            time = 0;


            // Run DFS from every unvisited location.
            // This also handles disconnected graphs.
            foreach (
                string location
                in graph.Keys)
            {
                if (!visited.Contains(
                    location))
                {
                    parent[location] =
                        null;

                    DFS(
                        location);
                }
            }
        }


        // =====================================================
        // DEPTH-FIRST SEARCH
        // =====================================================

        private void DFS(
            string current)
        {
            visited.Add(
                current);


            discoveryTime[current] =
                ++time;


            lowTime[current] =
                discoveryTime[current];


            int children = 0;


            foreach (
                string neighbour
                in graph[current])
            {
                // =============================================
                // UNVISITED NEIGHBOUR
                // =============================================

                if (!visited.Contains(
                    neighbour))
                {
                    children++;


                    parent[neighbour] =
                        current;


                    DFS(
                        neighbour);


                    // Update low value after returning
                    // from the DFS subtree.
                    lowTime[current] =
                        Math.Min(
                            lowTime[current],
                            lowTime[neighbour]);


                    // =========================================
                    // ROOT ARTICULATION POINT
                    // =========================================

                    if (parent[current] == null &&
                        children > 1)
                    {
                        articulationPoints.Add(
                            current);
                    }


                    // =========================================
                    // NON-ROOT ARTICULATION POINT
                    // =========================================

                    if (parent[current] != null &&
                        lowTime[neighbour] >=
                        discoveryTime[current])
                    {
                        articulationPoints.Add(
                            current);
                    }


                    // =========================================
                    // BRIDGE
                    // =========================================

                    if (lowTime[neighbour] >
                        discoveryTime[current])
                    {
                        bridges.Add(
                            (
                                current,
                                neighbour
                            ));
                    }
                }


                // =============================================
                // BACK EDGE
                // =============================================

                else if (parent[current] !=
                         neighbour)
                {
                    lowTime[current] =
                        Math.Min(
                            lowTime[current],
                            discoveryTime[neighbour]);
                }
            }
        }
    }
}