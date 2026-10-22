using System;
using System.Collections.Generic;

namespace IntelligentUrbanRouting.Models
{
    public class RoutingConstraints
    {
        public bool AvoidHighTraffic { get; set; }

        public bool AvoidClosedRoads { get; set; }

        public bool RestrictRoadTypes { get; set; }

        public HashSet<string> RestrictedRoadTypes { get; set; }

        public HashSet<string> ExcludedLocationIDs { get; set; }

        public HashSet<string> ExcludedRoadIDs { get; set; }


        public RoutingConstraints()
        {
            AvoidHighTraffic = false;

            AvoidClosedRoads = false;

            RestrictRoadTypes = false;


            RestrictedRoadTypes =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            ExcludedLocationIDs =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);


            ExcludedRoadIDs =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}