using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IntelligentUrbanRouting.Algorithms;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Services
{
    public class BenchmarkService
    {
        private readonly RoadGraph graph;
        private readonly StructuralGraphBuilder structuralGraphBuilder;
        private AlgorithmBenchmarkResult? bfsBenchmarkResult;
        private AlgorithmBenchmarkResult? dfsBenchmarkResult;

        // Run each algorithm several times and use the average.
        private const int Repetitions = 100;


        public BenchmarkService(RoadGraph graph)
        {
            this.graph =
                graph ??
                throw new ArgumentNullException(nameof(graph));

            structuralGraphBuilder =
                new StructuralGraphBuilder();
        }


        // =====================================================
        // SHORTEST PATH - DIJKSTRA
        // =====================================================

        public AlgorithmBenchmarkResult BenchmarkDijkstra(
            string start,
            string destination,
            RoutingConstraints constraints)
        {
            DijkstraAlgorithm algorithm =
                new DijkstraAlgorithm(graph);

            // Warm-up run so JIT compilation has less effect
            // on the measured runs.
            algorithm.FindShortestPath(
                start,
                destination,
                constraints);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            PathResult result =
                new PathResult();

            for (int i = 0; i < Repetitions; i++)
            {
                result =
                    algorithm.FindShortestPath(
                        start,
                        destination,
                        constraints);
            }

            stopwatch.Stop();

            long memoryAfter =
                GC.GetTotalMemory(true);

            return new AlgorithmBenchmarkResult
            {
                AlgorithmName = "Dijkstra",
                AverageMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds /
                    Repetitions,

                ApproximateMemoryBytes =
                    Math.Max(
                        0,
                        memoryAfter - memoryBefore),

                PathResult = result
            };
        }


        // =====================================================
        // SHORTEST PATH - BELLMAN-FORD
        // =====================================================

        public AlgorithmBenchmarkResult BenchmarkBellmanFord(
            string start,
            string destination,
            RoutingConstraints constraints)
        {
            BellmanFordAlgorithm algorithm =
                new BellmanFordAlgorithm(graph);

            algorithm.FindShortestPath(
                start,
                destination,
                constraints);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            PathResult result =
                new PathResult();

            for (int i = 0; i < Repetitions; i++)
            {
                result =
                    algorithm.FindShortestPath(
                        start,
                        destination,
                        constraints);
            }

            stopwatch.Stop();

            long memoryAfter =
                GC.GetTotalMemory(true);

            return new AlgorithmBenchmarkResult
            {
                AlgorithmName = "Bellman-Ford",
                AverageMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds /
                    Repetitions,

                ApproximateMemoryBytes =
                    Math.Max(
                        0,
                        memoryAfter - memoryBefore),

                PathResult = result
            };
        }


        // =====================================================
        // CONNECTIVITY - BFS
        // =====================================================

        public AlgorithmBenchmarkResult BenchmarkBFS()
        {
            EnsureConnectivityBenchmarks();

            return bfsBenchmarkResult!;
        }


        // =====================================================
        // CONNECTIVITY - DFS
        // =====================================================

        public AlgorithmBenchmarkResult BenchmarkDFS()
        {
            EnsureConnectivityBenchmarks();

            return dfsBenchmarkResult!;
        }


        private void EnsureConnectivityBenchmarks()
        {
            if (bfsBenchmarkResult != null &&
                dfsBenchmarkResult != null)
            {
                return;
            }

            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(graph);

            List<Location> locations =
                graph.GetAllLocations();

            if (locations.Count == 0)
            {
                bfsBenchmarkResult =
                    new AlgorithmBenchmarkResult
                    {
                        AlgorithmName = "BFS",
                        Connected = false
                    };

                dfsBenchmarkResult =
                    new AlgorithmBenchmarkResult
                    {
                        AlgorithmName = "DFS",
                        Connected = false
                    };

                return;
            }

            string start =
                locations[0].LocationID;

            BreadthFirstSearch bfs =
                new BreadthFirstSearch();

            DepthFirstSearch dfs =
                new DepthFirstSearch();

            // Warm up both implementations before measuring.
            bfs.Traverse(
                structuralGraph,
                start);

            dfs.Traverse(
                structuralGraph,
                start);

            long bfsMemory =
                MeasureBfsMemory(
                    bfs,
                    structuralGraph,
                    start);

            long dfsMemory =
                MeasureDfsMemory(
                    dfs,
                    structuralGraph,
                    start);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long bfsElapsedTicks = 0;
            long dfsElapsedTicks = 0;

            HashSet<string> bfsVisited =
                new HashSet<string>();

            HashSet<string> dfsVisited =
                new HashSet<string>();

            // Alternate the execution order so that neither
            // algorithm consistently benefits from running first.
            for (int i = 0; i < Repetitions; i++)
            {
                if (i % 2 == 0)
                {
                    long bfsStart =
                        Stopwatch.GetTimestamp();

                    bfsVisited =
                        bfs.Traverse(
                            structuralGraph,
                            start);

                    bfsElapsedTicks +=
                        Stopwatch.GetTimestamp() -
                        bfsStart;

                    long dfsStart =
                        Stopwatch.GetTimestamp();

                    dfsVisited =
                        dfs.Traverse(
                            structuralGraph,
                            start);

                    dfsElapsedTicks +=
                        Stopwatch.GetTimestamp() -
                        dfsStart;
                }
                else
                {
                    long dfsStart =
                        Stopwatch.GetTimestamp();

                    dfsVisited =
                        dfs.Traverse(
                            structuralGraph,
                            start);

                    dfsElapsedTicks +=
                        Stopwatch.GetTimestamp() -
                        dfsStart;

                    long bfsStart =
                        Stopwatch.GetTimestamp();

                    bfsVisited =
                        bfs.Traverse(
                            structuralGraph,
                            start);

                    bfsElapsedTicks +=
                        Stopwatch.GetTimestamp() -
                        bfsStart;
                }
            }

            bfsBenchmarkResult =
                new AlgorithmBenchmarkResult
            {
                AlgorithmName = "BFS",

                AverageMilliseconds =
                    TicksToAverageMilliseconds(
                        bfsElapsedTicks),

                ApproximateMemoryBytes =
                    bfsMemory,

                Connected =
                    bfsVisited.Count ==
                    graph.LocationCount()
            };

            dfsBenchmarkResult =
                new AlgorithmBenchmarkResult
            {
                AlgorithmName = "DFS",

                AverageMilliseconds =
                    TicksToAverageMilliseconds(
                        dfsElapsedTicks),

                ApproximateMemoryBytes =
                    dfsMemory,

                Connected =
                    dfsVisited.Count ==
                    graph.LocationCount()
            };
        }


        private double TicksToAverageMilliseconds(
            long elapsedTicks)
        {
            return elapsedTicks *
                   1000.0 /
                   Stopwatch.Frequency /
                   Repetitions;
        }


        private long MeasureBfsMemory(
            BreadthFirstSearch bfs,
            Dictionary<string, List<string>> structuralGraph,
            string start)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            HashSet<string> visited =
                new HashSet<string>();

            for (int i = 0; i < Repetitions; i++)
            {
                visited =
                    bfs.Traverse(
                        structuralGraph,
                        start);
            }

            long memoryAfter =
                GC.GetTotalMemory(true);

            GC.KeepAlive(visited);

            return Math.Max(
                0,
                memoryAfter - memoryBefore);
        }


        private long MeasureDfsMemory(
            DepthFirstSearch dfs,
            Dictionary<string, List<string>> structuralGraph,
            string start)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            HashSet<string> visited =
                new HashSet<string>();

            for (int i = 0; i < Repetitions; i++)
            {
                visited =
                    dfs.Traverse(
                        structuralGraph,
                        start);
            }

            long memoryAfter =
                GC.GetTotalMemory(true);

            GC.KeepAlive(visited);

            return Math.Max(
                0,
                memoryAfter - memoryBefore);
        }


        // =====================================================
        // BOTTLENECK LOCATIONS - TARJAN
        // =====================================================

        public AlgorithmBenchmarkResult
            BenchmarkTarjanArticulationPoints()
        {
            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(graph);

            TarjanAlgorithm tarjan =
                new TarjanAlgorithm();

            tarjan.FindArticulationPoints(
                structuralGraph);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            HashSet<string> result =
                new HashSet<string>();

            for (int i = 0; i < Repetitions; i++)
            {
                result =
                    tarjan.FindArticulationPoints(
                        structuralGraph);
            }

            stopwatch.Stop();

            long memoryAfter =
                GC.GetTotalMemory(true);

            return new AlgorithmBenchmarkResult
            {
                AlgorithmName =
                    "Tarjan Articulation Points",

                AverageMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds /
                    Repetitions,

                ApproximateMemoryBytes =
                    Math.Max(
                        0,
                        memoryAfter - memoryBefore),

                CriticalLocations =
                    new HashSet<string>(
                        result,
                        StringComparer.OrdinalIgnoreCase)
            };
        }


        // =====================================================
        // BOTTLENECK LOCATIONS - BRUTE FORCE
        // =====================================================

        public AlgorithmBenchmarkResult
            BenchmarkBruteForceArticulationPoints()
        {
            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(graph);

            BruteForceBottleneckAlgorithm bruteForce =
                new BruteForceBottleneckAlgorithm();

            bruteForce.FindArticulationPoints(
                structuralGraph);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            HashSet<string> result =
                new HashSet<string>();

            for (int i = 0; i < Repetitions; i++)
            {
                result =
                    bruteForce.FindArticulationPoints(
                        structuralGraph);
            }

            stopwatch.Stop();

            long memoryAfter =
                GC.GetTotalMemory(true);

            return new AlgorithmBenchmarkResult
            {
                AlgorithmName =
                    "Brute-Force Node Removal",

                AverageMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds /
                    Repetitions,

                ApproximateMemoryBytes =
                    Math.Max(
                        0,
                        memoryAfter - memoryBefore),

                CriticalLocations =
                    new HashSet<string>(
                        result,
                        StringComparer.OrdinalIgnoreCase)
            };
        }


        // =====================================================
        // BOTTLENECK ROADS - TARJAN
        // =====================================================

        public AlgorithmBenchmarkResult
            BenchmarkTarjanBridges()
        {
            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(graph);

            TarjanAlgorithm tarjan =
                new TarjanAlgorithm();

            tarjan.FindBridges(
                structuralGraph);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            List<(string From, string To)> result =
                new List<(string From, string To)>();

            for (int i = 0; i < Repetitions; i++)
            {
                result =
                    tarjan.FindBridges(
                        structuralGraph);
            }

            stopwatch.Stop();

            long memoryAfter =
                GC.GetTotalMemory(true);

            return new AlgorithmBenchmarkResult
            {
                AlgorithmName =
                    "Tarjan Bridges",

                AverageMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds /
                    Repetitions,

                ApproximateMemoryBytes =
                    Math.Max(
                        0,
                        memoryAfter - memoryBefore),

                CriticalRoads = result
            };
        }


        // =====================================================
        // BOTTLENECK ROADS - BRUTE FORCE
        // =====================================================

        public AlgorithmBenchmarkResult
            BenchmarkBruteForceBridges()
        {
            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(graph);

            BruteForceBottleneckAlgorithm bruteForce =
                new BruteForceBottleneckAlgorithm();

            bruteForce.FindBridges(
                structuralGraph);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore =
                GC.GetTotalMemory(true);

            Stopwatch stopwatch =
                Stopwatch.StartNew();

            List<(string From, string To)> result =
                new List<(string From, string To)>();

            for (int i = 0; i < Repetitions; i++)
            {
                result =
                    bruteForce.FindBridges(
                        structuralGraph);
            }

            stopwatch.Stop();

            long memoryAfter =
                GC.GetTotalMemory(true);

            return new AlgorithmBenchmarkResult
            {
                AlgorithmName =
                    "Brute-Force Edge Removal",

                AverageMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds /
                    Repetitions,

                ApproximateMemoryBytes =
                    Math.Max(
                        0,
                        memoryAfter - memoryBefore),

                CriticalRoads = result
            };
        }


        // =====================================================
        // RESULT COMPARISON HELPERS
        // =====================================================

        public bool PathsMatch(
            AlgorithmBenchmarkResult first,
            AlgorithmBenchmarkResult second)
        {
            if (first.PathResult == null ||
                second.PathResult == null)
            {
                return false;
            }

            if (first.PathResult.PathFound !=
                second.PathResult.PathFound)
            {
                return false;
            }

            if (!first.PathResult.PathFound &&
                !second.PathResult.PathFound)
            {
                return true;
            }

            // There can be multiple equally-short paths.
            // Therefore compare shortest travel time rather
            // than requiring an identical node sequence.
            return first.PathResult.TotalTravelTime ==
                   second.PathResult.TotalTravelTime;
        }


        public bool ConnectivityMatches(
            AlgorithmBenchmarkResult first,
            AlgorithmBenchmarkResult second)
        {
            return first.Connected ==
                   second.Connected;
        }


        public bool CriticalLocationsMatch(
            AlgorithmBenchmarkResult first,
            AlgorithmBenchmarkResult second)
        {
            return first.CriticalLocations.SetEquals(
                second.CriticalLocations);
        }


        public bool CriticalRoadsMatch(
            AlgorithmBenchmarkResult first,
            AlgorithmBenchmarkResult second)
        {
            HashSet<string> firstEdges =
                new HashSet<string>(
                    first.CriticalRoads.Select(
                        edge =>
                            NormaliseEdge(
                                edge.From,
                                edge.To)),
                    StringComparer.OrdinalIgnoreCase);

            HashSet<string> secondEdges =
                new HashSet<string>(
                    second.CriticalRoads.Select(
                        edge =>
                            NormaliseEdge(
                                edge.From,
                                edge.To)),
                    StringComparer.OrdinalIgnoreCase);

            return firstEdges.SetEquals(
                secondEdges);
        }


        private string NormaliseEdge(
            string from,
            string to)
        {
            if (string.Compare(
                from,
                to,
                StringComparison.OrdinalIgnoreCase) <= 0)
            {
                return from + "|" + to;
            }

            return to + "|" + from;
        }
    }


    // =========================================================
    // COMMON BENCHMARK RESULT
    // =========================================================

    public class AlgorithmBenchmarkResult
    {
        public string AlgorithmName { get; set; } =
            string.Empty;

        public double AverageMilliseconds { get; set; }

        public long ApproximateMemoryBytes { get; set; }

        public PathResult? PathResult { get; set; }

        public bool Connected { get; set; }

        public HashSet<string> CriticalLocations { get; set; } =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        public List<(string From, string To)> CriticalRoads
        { get; set; } =
                new List<(string From, string To)>();
    }
}
