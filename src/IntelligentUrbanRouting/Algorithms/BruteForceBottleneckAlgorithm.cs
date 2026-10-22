using System;
using System.Collections.Generic;

namespace IntelligentUrbanRouting.Algorithms
{
    public class BruteForceBottleneckAlgorithm
    {
        // =====================================================
        // FIND ARTICULATION POINTS BY NODE REMOVAL
        // =====================================================

        public HashSet<string> FindArticulationPoints(
            Dictionary<string, List<string>> graph)
        {
            HashSet<string> articulationPoints =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            int originalComponents =
                CountConnectedComponents(
                    graph,
                    null,
                    null);


            foreach (
                string removedNode
                in graph.Keys)
            {
                int componentsAfterRemoval =
                    CountConnectedComponents(
                        graph,
                        removedNode,
                        null);


                if (componentsAfterRemoval >
                    originalComponents)
                {
                    articulationPoints.Add(
                        removedNode);
                }
            }


            return articulationPoints;
        }


        // =====================================================
        // FIND BRIDGES BY EDGE REMOVAL
        // =====================================================

        public List<(string From, string To)> FindBridges(
            Dictionary<string, List<string>> graph)
        {
            List<(string From, string To)> bridges =
                new List<(string From, string To)>();


            int originalComponents =
                CountConnectedComponents(
                    graph,
                    null,
                    null);


            HashSet<string> checkedEdges =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            foreach (
                KeyValuePair<string, List<string>> entry
                in graph)
            {
                string from =
                    entry.Key;


                foreach (
                    string to
                    in entry.Value)
                {
                    string edgeKey =
                        CreateEdgeKey(
                            from,
                            to);


                    if (checkedEdges.Contains(
                        edgeKey))
                    {
                        continue;
                    }


                    checkedEdges.Add(
                        edgeKey);


                    int componentsAfterRemoval =
                        CountConnectedComponents(
                            graph,
                            null,
                            (from, to));


                    if (componentsAfterRemoval >
                        originalComponents)
                    {
                        bridges.Add(
                            (from, to));
                    }
                }
            }


            return bridges;
        }


        // =====================================================
        // COUNT CONNECTED COMPONENTS
        // =====================================================

        private int CountConnectedComponents(
            Dictionary<string, List<string>> graph,
            string? removedNode,
            (string From, string To)? removedEdge)
        {
            HashSet<string> visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            int componentCount = 0;


            foreach (
                string node
                in graph.Keys)
            {
                if (removedNode != null &&
                    string.Equals(
                        node,
                        removedNode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                if (visited.Contains(
                    node))
                {
                    continue;
                }


                componentCount++;


                Queue<string> queue =
                    new Queue<string>();


                visited.Add(
                    node);


                queue.Enqueue(
                    node);


                while (queue.Count > 0)
                {
                    string current =
                        queue.Dequeue();


                    foreach (
                        string neighbour
                        in graph[current])
                    {
                        // Skip removed node.
                        if (removedNode != null &&
                            string.Equals(
                                neighbour,
                                removedNode,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }


                        // Skip removed edge.
                        if (removedEdge.HasValue &&
                            IsSameUndirectedEdge(
                                current,
                                neighbour,
                                removedEdge.Value.From,
                                removedEdge.Value.To))
                        {
                            continue;
                        }


                        if (!visited.Contains(
                            neighbour))
                        {
                            visited.Add(
                                neighbour);


                            queue.Enqueue(
                                neighbour);
                        }
                    }
                }
            }


            return componentCount;
        }


        // =====================================================
        // CHECK WHETHER TWO EDGES REPRESENT SAME
        // UNDIRECTED CONNECTION
        // =====================================================

        private bool IsSameUndirectedEdge(
            string from1,
            string to1,
            string from2,
            string to2)
        {
            return
                (
                    string.Equals(
                        from1,
                        from2,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        to1,
                        to2,
                        StringComparison.OrdinalIgnoreCase)
                )
                ||
                (
                    string.Equals(
                        from1,
                        to2,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        to1,
                        from2,
                        StringComparison.OrdinalIgnoreCase)
                );
        }


        // =====================================================
        // CREATE UNIQUE UNDIRECTED EDGE KEY
        // =====================================================

        private string CreateEdgeKey(
            string from,
            string to)
        {
            if (string.Compare(
                from,
                to,
                StringComparison.OrdinalIgnoreCase) < 0)
            {
                return from +
                       "|" +
                       to;
            }


            return to +
                   "|" +
                   from;
        }
    }
}