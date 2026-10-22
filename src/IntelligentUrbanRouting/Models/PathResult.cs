using System.Collections.Generic;

namespace IntelligentUrbanRouting.Models
{
    public class PathResult
    {
        public bool PathFound { get; set; }

        public List<string> Path { get; set; }

        public int TotalTravelTime { get; set; }

        public PathResult()
        {
            PathFound = false;

            Path =
                new List<string>();

            TotalTravelTime = 0;

        }
    }
}
