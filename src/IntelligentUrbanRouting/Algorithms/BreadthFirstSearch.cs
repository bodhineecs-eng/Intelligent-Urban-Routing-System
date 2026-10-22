using System.Collections.Generic;

namespace IntelligentUrbanRouting.Algorithms
{
    public class BreadthFirstSearch
    {
        public HashSet<string> Traverse(
            Dictionary<string, List<string>> graph,
            string start)
        {
            HashSet<string> visited =
                new HashSet<string>();

            Queue<string> queue =
                new Queue<string>();


            // Check whether the starting location exists
            if (!graph.ContainsKey(start))
            {
                return visited;
            }


            // Start BFS
            visited.Add(start);

            queue.Enqueue(start);


            while (queue.Count > 0)
            {
                string current =
                    queue.Dequeue();


                foreach (string neighbour
                         in graph[current])
                {
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);

                        queue.Enqueue(neighbour);
                    }
                }
            }


            return visited;
        }
    }
}