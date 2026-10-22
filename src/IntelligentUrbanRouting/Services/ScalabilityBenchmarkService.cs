using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using IntelligentUrbanRouting.Algorithms;
using IntelligentUrbanRouting.Data;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Services
{
    public class ScalabilityBenchmarkService
    {
        private readonly SyntheticGraphFactory graphFactory;


        public ScalabilityBenchmarkService()
        {
            graphFactory =
                new SyntheticGraphFactory();
        }


        public List<ScalabilityBenchmarkResult> RunDefaultStudy()
        {
            List<ScalabilityBenchmarkResult> results =
                new List<ScalabilityBenchmarkResult>();

            int[] sparseSizes =
                { 100, 250, 500, 1000, 2000 };

            int[] denseSizes =
                { 50, 100, 200, 300 };

            foreach (int nodeCount in sparseSizes)
            {
                results.Add(
                    BenchmarkGraph(
                        "Sparse road network",
                        graphFactory.CreateSparse(nodeCount),
                        GetRepetitions(nodeCount)));
            }

            foreach (int nodeCount in denseSizes)
            {
                results.Add(
                    BenchmarkGraph(
                        "Dense transit network",
                        graphFactory.CreateDense(nodeCount),
                        GetRepetitions(nodeCount)));
            }

            return results;
        }


        public void WriteCsv(
            string filePath,
            IEnumerable<ScalabilityBenchmarkResult> results)
        {
            List<string> lines =
                new List<string>
                {
                    "NetworkType,Nodes,Edges,Density,Repetitions," +
                    "DijkstraMilliseconds,BellmanFordMilliseconds," +
                    "DijkstraAllocatedBytes,BellmanFordAllocatedBytes," +
                    "ResultsMatch"
                };

            foreach (ScalabilityBenchmarkResult result in results)
            {
                lines.Add(
                    string.Join(
                        ",",
                        result.NetworkType,
                        result.NodeCount.ToString(
                            CultureInfo.InvariantCulture),
                        result.EdgeCount.ToString(
                            CultureInfo.InvariantCulture),
                        result.Density.ToString(
                            "F6",
                            CultureInfo.InvariantCulture),
                        result.Repetitions.ToString(
                            CultureInfo.InvariantCulture),
                        result.DijkstraAverageMilliseconds.ToString(
                            "F6",
                            CultureInfo.InvariantCulture),
                        result.BellmanFordAverageMilliseconds.ToString(
                            "F6",
                            CultureInfo.InvariantCulture),
                        result.DijkstraAverageAllocatedBytes.ToString(
                            CultureInfo.InvariantCulture),
                        result.BellmanFordAverageAllocatedBytes.ToString(
                            CultureInfo.InvariantCulture),
                        result.ResultsMatch ? "Yes" : "No"));
            }

            File.WriteAllLines(
                filePath,
                lines);
        }


        private static ScalabilityBenchmarkResult BenchmarkGraph(
            string networkType,
            RoadGraph graph,
            int repetitions)
        {
            string start =
                SyntheticGraphFactory.GetLocationID(
                    graph.LocationCount() - 1);

            string destination =
                SyntheticGraphFactory.GetLocationID(0);

            RoutingConstraints constraints =
                new RoutingConstraints
                {
                    AvoidClosedRoads = true
                };

            DijkstraAlgorithm dijkstra =
                new DijkstraAlgorithm(graph);

            BellmanFordAlgorithm bellmanFord =
                new BellmanFordAlgorithm(graph);

            AlgorithmMeasurement dijkstraMeasurement =
                Measure(
                    () => dijkstra.FindShortestPath(
                        start,
                        destination,
                        constraints),
                    repetitions);

            AlgorithmMeasurement bellmanFordMeasurement =
                Measure(
                    () => bellmanFord.FindShortestPath(
                        start,
                        destination,
                        constraints),
                    repetitions);

            long possibleEdges =
                (long)graph.LocationCount() *
                (graph.LocationCount() - 1);

            bool resultsMatch =
                dijkstraMeasurement.Result.PathFound ==
                    bellmanFordMeasurement.Result.PathFound &&
                (!dijkstraMeasurement.Result.PathFound ||
                 dijkstraMeasurement.Result.TotalTravelTime ==
                    bellmanFordMeasurement.Result.TotalTravelTime);

            return new ScalabilityBenchmarkResult
            {
                NetworkType = networkType,
                NodeCount = graph.LocationCount(),
                EdgeCount = graph.RoadCount(),
                Density = possibleEdges == 0
                    ? 0.0
                    : graph.RoadCount() / (double)possibleEdges,
                Repetitions = repetitions,
                DijkstraAverageMilliseconds =
                    dijkstraMeasurement.AverageMilliseconds,
                BellmanFordAverageMilliseconds =
                    bellmanFordMeasurement.AverageMilliseconds,
                DijkstraAverageAllocatedBytes =
                    dijkstraMeasurement.AverageAllocatedBytes,
                BellmanFordAverageAllocatedBytes =
                    bellmanFordMeasurement.AverageAllocatedBytes,
                ResultsMatch = resultsMatch
            };
        }


        private static AlgorithmMeasurement Measure(
            Func<PathResult> algorithm,
            int repetitions)
        {
            // Warm-up removes most one-time JIT compilation cost.
            algorithm();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long allocatedBefore =
                GC.GetAllocatedBytesForCurrentThread();

            long startedAt =
                Stopwatch.GetTimestamp();

            PathResult result =
                new PathResult();

            for (int repetition = 0;
                 repetition < repetitions;
                 repetition++)
            {
                result = algorithm();
            }

            long elapsedTicks =
                Stopwatch.GetTimestamp() -
                startedAt;

            long allocatedBytes =
                GC.GetAllocatedBytesForCurrentThread() -
                allocatedBefore;

            return new AlgorithmMeasurement
            {
                AverageMilliseconds =
                    elapsedTicks * 1000.0 /
                    Stopwatch.Frequency /
                    repetitions,
                AverageAllocatedBytes =
                    allocatedBytes / repetitions,
                Result = result
            };
        }


        private static int GetRepetitions(
            int nodeCount)
        {
            if (nodeCount <= 250)
            {
                return 30;
            }

            if (nodeCount <= 1000)
            {
                return 10;
            }

            return 5;
        }


        private class AlgorithmMeasurement
        {
            public double AverageMilliseconds { get; set; }

            public long AverageAllocatedBytes { get; set; }

            public PathResult Result { get; set; } =
                new PathResult();
        }
    }


    public class ScalabilityBenchmarkResult
    {
        public string NetworkType { get; set; } =
            string.Empty;

        public int NodeCount { get; set; }

        public int EdgeCount { get; set; }

        public double Density { get; set; }

        public int Repetitions { get; set; }

        public double DijkstraAverageMilliseconds { get; set; }

        public double BellmanFordAverageMilliseconds { get; set; }

        public long DijkstraAverageAllocatedBytes { get; set; }

        public long BellmanFordAverageAllocatedBytes { get; set; }

        public bool ResultsMatch { get; set; }
    }
}
