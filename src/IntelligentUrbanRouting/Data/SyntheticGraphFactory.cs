using System;
using System.Collections.Generic;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Data
{
    public class SyntheticGraphFactory
    {
        private const int RandomSeed = 2026;


        public RoadGraph CreateSparse(
            int nodeCount,
            double averageOutgoingRoads = 2.4)
        {
            int targetRoadCount =
                (int)Math.Ceiling(
                    nodeCount * averageOutgoingRoads);

            return Create(
                nodeCount,
                targetRoadCount,
                "Sparse");
        }


        public RoadGraph CreateDense(
            int nodeCount,
            double density = 0.20)
        {
            long possibleRoadCount =
                (long)nodeCount * (nodeCount - 1);

            int targetRoadCount =
                (int)Math.Round(
                    possibleRoadCount * density);

            return Create(
                nodeCount,
                targetRoadCount,
                "Dense");
        }


        public static string GetLocationID(
            int index)
        {
            return "N" + index.ToString("D5");
        }


        private static RoadGraph Create(
            int nodeCount,
            int requestedRoadCount,
            string roadPrefix)
        {
            if (nodeCount < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(nodeCount),
                    "A synthetic graph needs at least two nodes.");
            }

            int maximumRoadCount =
                checked(nodeCount * (nodeCount - 1));

            int targetRoadCount =
                Math.Clamp(
                    requestedRoadCount,
                    nodeCount,
                    maximumRoadCount);

            RoadGraph graph =
                new RoadGraph();

            int gridWidth =
                (int)Math.Ceiling(
                    Math.Sqrt(nodeCount));

            for (int index = 0;
                 index < nodeCount;
                 index++)
            {
                double latitude =
                    6.9000 +
                    (index / gridWidth) * 0.001;

                double longitude =
                    79.8500 +
                    (index % gridWidth) * 0.001;

                graph.AddLocation(
                    new Location(
                        GetLocationID(index),
                        "Synthetic Location " + index,
                        latitude,
                        longitude));
            }

            HashSet<long> connections =
                new HashSet<long>();

            int roadIndex = 0;

            // A reverse-ordered directed cycle guarantees that
            // every node remains reachable while avoiding an
            // unrealistically favourable edge order for
            // Bellman-Ford during the scaling experiment.
            for (int fromIndex = 0;
                 fromIndex < nodeCount;
                 fromIndex++)
            {
                int toIndex =
                    (fromIndex - 1 + nodeCount) %
                    nodeCount;

                AddRoad(
                    graph,
                    connections,
                    roadPrefix,
                    fromIndex,
                    toIndex,
                    roadIndex++);
            }

            Random random =
                new Random(
                    RandomSeed +
                    nodeCount * 397 +
                    targetRoadCount);

            while (roadIndex < targetRoadCount)
            {
                int fromIndex =
                    random.Next(nodeCount);

                int toIndex =
                    random.Next(nodeCount);

                if (fromIndex == toIndex)
                {
                    continue;
                }

                long key =
                    CreateConnectionKey(
                        fromIndex,
                        toIndex,
                        nodeCount);

                if (!connections.Add(key))
                {
                    continue;
                }

                AddRoadCore(
                    graph,
                    roadPrefix,
                    fromIndex,
                    toIndex,
                    roadIndex++);
            }

            return graph;
        }


        private static void AddRoad(
            RoadGraph graph,
            HashSet<long> connections,
            string roadPrefix,
            int fromIndex,
            int toIndex,
            int roadIndex)
        {
            int nodeCount =
                graph.LocationCount();

            connections.Add(
                CreateConnectionKey(
                    fromIndex,
                    toIndex,
                    nodeCount));

            AddRoadCore(
                graph,
                roadPrefix,
                fromIndex,
                toIndex,
                roadIndex);
        }


        private static void AddRoadCore(
            RoadGraph graph,
            string roadPrefix,
            int fromIndex,
            int toIndex,
            int roadIndex)
        {
            int travelTime =
                1 + ((fromIndex * 17 + toIndex * 31) % 30);

            int traffic =
                1 + ((fromIndex + toIndex) % 5);

            string congestionCategory =
                traffic >= 4 ? "High" :
                traffic == 3 ? "Moderate" :
                "Low";

            graph.AddRoad(
                new Road(
                    roadPrefix.Substring(0, 1).ToUpperInvariant() +
                        roadIndex.ToString("D7"),
                    GetLocationID(fromIndex),
                    GetLocationID(toIndex),
                    roadPrefix + " Road " + roadIndex,
                    travelTime,
                    traffic,
                    congestionCategory,
                    "Local Road",
                    "Open"));
        }


        private static long CreateConnectionKey(
            int fromIndex,
            int toIndex,
            int nodeCount)
        {
            return (long)fromIndex * nodeCount +
                   toIndex;
        }
    }
}
