using System;
using System.Collections.Generic;

namespace IntelligentUrbanRouting.Algorithms
{
    public class DepthFirstSearch
    {
        // =====================================================
        // DFS TRAVERSAL
        // =====================================================

        public HashSet<string> Traverse(
            Dictionary<string, List<string>> graph,
            string start)
        {
            HashSet<string> visited =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            if (!graph.ContainsKey(start))
            {
                return visited;
            }


            Stack<string> stack =
                new Stack<string>();


            stack.Push(
                start);


            while (stack.Count > 0)
            {
                string current =
                    stack.Pop();


                if (visited.Contains(
                    current))
                {
                    continue;
                }


                visited.Add(
                    current);


                foreach (
                    string neighbour
                    in graph[current])
                {
                    if (!visited.Contains(
                        neighbour))
                    {
                        stack.Push(
                            neighbour);
                    }
                }
            }


            return visited;
        }
    }
}