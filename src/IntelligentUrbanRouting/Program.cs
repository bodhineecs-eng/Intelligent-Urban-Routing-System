using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using IntelligentUrbanRouting.Data;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;
using IntelligentUrbanRouting.Services;

namespace IntelligentUrbanRouting;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            string smallDataDirectory = GetDataDirectory("small");
            RoadGraph roadGraph = LoadDatasetFromDirectory(smallDataDirectory);
            CsvDataWriter dataWriter = new CsvDataWriter(
                Path.Combine(smallDataDirectory, "locations.csv"),
                Path.Combine(smallDataDirectory, "roads.csv"));
            RoutingService routingService = new RoutingService(roadGraph);
            ReachabilityService reachabilityService = new ReachabilityService(roadGraph);
            ConnectivityService connectivityService = new ConnectivityService(roadGraph);
            BottleneckService bottleneckService = new BottleneckService(roadGraph);
            Console.WriteLine("==========================================");
            Console.WriteLine("     INTELLIGENT URBAN ROUTING SYSTEM");
            Console.WriteLine("==========================================");
            Console.WriteLine("Small CSV dataset loaded successfully.");
            Console.WriteLine("Locations: " + roadGraph.LocationCount());
            Console.WriteLine("Roads: " + roadGraph.RoadCount());
            Console.WriteLine();
            Console.WriteLine("Available Locations:");
            foreach (Location allLocation in roadGraph.GetAllLocations())
            {
                Console.WriteLine(allLocation.LocationID + " - " + allLocation.Name);
            }
            bool flag = true;
            while (flag)
            {
                Console.WriteLine();
                Console.WriteLine("==========================================");
                Console.WriteLine("                 MAIN MENU");
                Console.WriteLine("==========================================");
                Console.WriteLine("1. Find Fastest Operational Path");
                Console.WriteLine("2. Find Constrained Path");
                Console.WriteLine("3. Reachability Within Time Budget");
                Console.WriteLine("4. Network Connectivity & Bottlenecks");
                Console.WriteLine("5. Performance Evaluation");
                Console.WriteLine("6. Compare Baseline vs Road Closures");
                Console.WriteLine("7. Add Data");
                Console.WriteLine("0. Exit");
                Console.WriteLine("==========================================");
                Console.Write("Enter choice: ");
                switch (Console.ReadLine() ?? string.Empty)
                {
                    case "1":
                        HandleOptimalPath(routingService);
                        break;
                    case "2":
                        HandleConstrainedPath(routingService);
                        break;
                    case "3":
                        HandleReachability(reachabilityService, routingService);
                        break;
                    case "4":
                        HandleNetworkAnalysis(connectivityService, bottleneckService);
                        break;
                    case "5":
                        HandlePerformanceEvaluation();
                        break;
                    case "6":
                        HandleClosureComparison(routingService);
                        break;
                    case "7":
                        HandleDataMenu(roadGraph, dataWriter);
                        break;
                    case "0":
                        flag = false;
                        Console.WriteLine("Program ended.");
                        break;
                    default:
                        Console.WriteLine("Invalid menu choice.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: " + ex.Message);
        }
    }

    private static RoadGraph LoadSmallDataset()
    {
        return LoadDataset("small");
    }

    private static RoadGraph LoadDataset(string datasetSize)
    {
        return LoadDatasetFromDirectory(GetDataDirectory(datasetSize));
    }

    private static RoadGraph LoadDatasetFromDirectory(string dataDirectory)
    {
        string nodesFilePath = Path.Combine(dataDirectory, "locations.csv");
        string edgesFilePath = Path.Combine(dataDirectory, "roads.csv");
        CsvDatasetLoader csvDatasetLoader = new CsvDatasetLoader();
        return csvDatasetLoader.LoadDataset(nodesFilePath, edgesFilePath);
    }

    private static string GetDataDirectory(string datasetSize)
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            string solutionPath = Path.Combine(
                directory.FullName,
                "IntelligentUrbanRouting.sln");
            string sourceDataDirectory = Path.Combine(
                directory.FullName,
                "data",
                datasetSize);

            if (File.Exists(solutionPath) && Directory.Exists(sourceDataDirectory))
            {
                return sourceDataDirectory;
            }

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "data", datasetSize);
    }

    private static void HandleDataMenu(
        RoadGraph roadGraph,
        CsvDataWriter dataWriter)
    {
        bool dataMenuOpen = true;

        while (dataMenuOpen)
        {
            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine("                  ADD DATA");
            Console.WriteLine("==========================================");
            Console.WriteLine("1. Show All Locations");
            Console.WriteLine("2. Show All Roads");
            Console.WriteLine("3. Save New Location");
            Console.WriteLine("4. Save New Road");
            Console.WriteLine("0. Back to Main Menu");
            Console.WriteLine("==========================================");
            Console.Write("Enter choice: ");

            switch (Console.ReadLine() ?? string.Empty)
            {
                case "1":
                    DisplayAllLocations(roadGraph);
                    break;
                case "2":
                    DisplayAllRoads(roadGraph);
                    break;
                case "3":
                    SaveNewLocation(roadGraph, dataWriter);
                    break;
                case "4":
                    SaveNewRoad(roadGraph, dataWriter);
                    break;
                case "0":
                    dataMenuOpen = false;
                    break;
                default:
                    Console.WriteLine("Invalid data menu choice.");
                    break;
            }
        }
    }

    private static void DisplayAllLocations(RoadGraph roadGraph)
    {
        Console.WriteLine();
        Console.WriteLine("===== ALL LOCATIONS =====");
        Console.WriteLine("location_id | location_name | latitude | longitude");

        foreach (Location location in roadGraph.GetAllLocations())
        {
            Console.WriteLine(
                location.LocationID + " | " +
                location.Name + " | " +
                location.Latitude.ToString("R", CultureInfo.InvariantCulture) + " | " +
                location.Longitude.ToString("R", CultureInfo.InvariantCulture));
        }
    }

    private static void DisplayAllRoads(RoadGraph roadGraph)
    {
        Console.WriteLine();
        Console.WriteLine("===== ALL ROADS =====");
        Console.WriteLine(
            "road_id | from_id | to_id | road_name | travel_time_min | " +
            "congestion_level | congestion_category | road_type | Status");

        foreach (Road road in roadGraph.GetAllRoads())
        {
            Console.WriteLine(
                road.RoadID + " | " +
                road.From + " | " +
                road.To + " | " +
                road.RoadName + " | " +
                road.TravelTime + " | " +
                road.Traffic + " | " +
                road.CongestionCategory + " | " +
                road.RoadType + " | " +
                road.Status);
        }
    }

    private static void SaveNewLocation(
        RoadGraph roadGraph,
        CsvDataWriter dataWriter)
    {
        Console.WriteLine();
        Console.WriteLine("===== SAVE NEW LOCATION =====");

        string name = ReadRequiredText("Enter Location Name: ");
        double longitude = ReadDouble(
            "Enter Longitude: ",
            -180,
            180);
        double latitude = ReadDouble(
            "Enter Latitude: ",
            -90,
            90);

        Location location = dataWriter.SaveLocation(
            name,
            latitude,
            longitude);

        roadGraph.AddLocation(location);

        Console.WriteLine(
            "Location saved successfully with location_id " +
            location.LocationID +
            ".");
    }

    private static void SaveNewRoad(
        RoadGraph roadGraph,
        CsvDataWriter dataWriter)
    {
        Console.WriteLine();
        Console.WriteLine("===== SAVE NEW ROAD =====");

        string from = ReadLocationID(
            roadGraph,
            "Enter From Location ID: ");
        string to = ReadLocationID(
            roadGraph,
            "Enter To Location ID: ");
        string roadName = ReadRequiredText("Enter Road Name: ");
        int travelTime = ReadInteger(
            "Enter Travel Time in Minutes: ",
            0,
            int.MaxValue);
        int congestionLevel = ReadInteger(
            "Enter Congestion Level (1-5): ",
            1,
            5);
        string congestionCategory = ReadRequiredText(
            "Enter Congestion Category: ");
        string roadType = ReadRequiredText("Enter Road Type: ");
        string status = ReadRoadStatus();

        Road road = dataWriter.SaveRoad(
            from,
            to,
            roadName,
            travelTime,
            congestionLevel,
            congestionCategory,
            roadType,
            status);

        roadGraph.AddRoad(road);

        Console.WriteLine(
            "Road saved successfully with road_id " +
            road.RoadID +
            ".");
    }

    private static void HandleOptimalPath(RoutingService routingService)
    {
        Console.WriteLine();
        Console.WriteLine("===== FASTEST OPERATIONAL PATH =====");
        Console.Write("Start location ID: ");
        string start = Console.ReadLine() ?? string.Empty;
        Console.Write("Destination location ID: ");
        string destination = Console.ReadLine() ?? string.Empty;
        if (ValidateStartDestination(routingService, start, destination))
        {
            PathResult result = routingService.FindOptimalPath(start, destination);
            DisplayPathResult(result);
        }
    }

    private static void HandleConstrainedPath(RoutingService routingService)
    {
        Console.WriteLine();
        Console.WriteLine("===== CONSTRAINED ROUTING =====");
        Console.WriteLine("1. Avoid High-Traffic Roads");
        Console.WriteLine("2. Avoid Selected Road Type");
        Console.WriteLine("3. Avoid Selected Location");
        Console.WriteLine("4. Avoid Selected Road");
        Console.Write("Enter choice: ");
        string text = Console.ReadLine() ?? string.Empty;
        Console.Write("Start location ID: ");
        string text2 = Console.ReadLine() ?? string.Empty;
        Console.Write("Destination location ID: ");
        string text3 = Console.ReadLine() ?? string.Empty;
        if (!ValidateStartDestination(routingService, text2, text3))
        {
            return;
        }
        PathResult result;
        switch (text)
        {
            case "1":
                result = routingService.FindLowTrafficPath(text2, text3);
                break;
            case "2":
                {
                    string? text6 = AskRoadType();
                    if (text6 == null)
                    {
                        return;
                    }
                    result = routingService.FindPathAvoidingRoadTypes(text2, text3, new List<string> { text6 });
                    break;
                }
            case "3":
                {
                    Console.Write("Location ID to avoid: ");
                    string text5 = Console.ReadLine() ?? string.Empty;
                    if (!routingService.LocationExists(text5))
                    {
                        Console.WriteLine("Location does not exist.");
                        return;
                    }
                    if (string.Equals(text5, text2, StringComparison.OrdinalIgnoreCase) || string.Equals(text5, text3, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("The start or destination cannot be excluded.");
                        return;
                    }
                    result = routingService.FindPathAvoidingLocation(text2, text3, text5);
                    break;
                }
            case "4":
                {
                    Console.Write("Road ID to avoid: ");
                    string text4 = Console.ReadLine() ?? string.Empty;
                    if (!routingService.RoadExists(text4))
                    {
                        Console.WriteLine("Road does not exist.");
                        return;
                    }
                    result = routingService.FindPathAvoidingRoad(text2, text3, text4);
                    break;
                }
            default:
                Console.WriteLine("Invalid constraint choice.");
                return;
        }
        DisplayPathResult(result);
    }

    private static void HandleReachability(ReachabilityService reachabilityService, RoutingService routingService)
    {
        Console.WriteLine();
        Console.WriteLine("===== REACHABILITY =====");
        Console.Write("Start location ID: ");
        string text = Console.ReadLine() ?? string.Empty;
        if (!routingService.LocationExists(text))
        {
            Console.WriteLine("Start location does not exist.");
            return;
        }
        Console.Write("Time budget in minutes: ");
        if (!int.TryParse(Console.ReadLine(), out var result) || result < 0)
        {
            Console.WriteLine("Invalid time budget.");
            return;
        }
        Dictionary<string, int> dictionary = reachabilityService.FindReachableLocations(text, result);
        Console.WriteLine();
        Console.WriteLine("Reachable Locations:");
        if (dictionary.Count == 0)
        {
            Console.WriteLine("None");
            return;
        }
        foreach (KeyValuePair<string, int> item in dictionary.OrderBy((KeyValuePair<string, int> item) => item.Value))
        {
            Console.WriteLine(item.Key + " = " + item.Value + " minutes");
        }
    }

    private static void HandleNetworkAnalysis(ConnectivityService connectivityService, BottleneckService bottleneckService)
    {
        Console.WriteLine();
        Console.WriteLine("===== NETWORK CONNECTIVITY =====");
        bool weaklyConnected =
            connectivityService.IsWeaklyConnected();
        bool stronglyConnected =
            connectivityService.IsStronglyConnected();
        Console.WriteLine(
            "Physical / weak connectivity " +
            "(direction ignored; closed roads included): " +
            (weaklyConnected ? "YES" : "NO"));
        Console.WriteLine(
            "Operational strong connectivity " +
            "(direction preserved; closed roads excluded): " +
            (stronglyConnected ? "YES" : "NO"));
        Console.WriteLine();
        Console.WriteLine("===== BOTTLENECK LOCATIONS =====");
        HashSet<string> hashSet = bottleneckService.FindBottleneckLocations();
        if (hashSet.Count == 0)
        {
            Console.WriteLine("No articulation points found.");
        }
        else
        {
            foreach (string item in hashSet.OrderBy((string location) => location))
            {
                Console.WriteLine("- " + item);
            }
        }
        Console.WriteLine();
        Console.WriteLine("===== BOTTLENECK ROADS =====");
        List<(string, string)> list = bottleneckService.FindBottleneckRoads();
        if (list.Count == 0)
        {
            Console.WriteLine("No bridges found.");
            return;
        }
        foreach (var item2 in list)
        {
            Console.WriteLine("- " + item2.Item1 + " <-> " + item2.Item2);
        }
    }

    private static void HandlePerformanceEvaluation()
    {
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("          PERFORMANCE EVALUATION");
        Console.WriteLine("==========================================");
        Console.WriteLine("Select size of the dataset");
        Console.WriteLine("1. Large");
        Console.WriteLine("2. Small");
        Console.Write("Enter choice: ");
        string text = Console.ReadLine() ?? string.Empty;
        string text2 = text;
        string text3 = text2;
        string datasetSize;
        string text4;
        if (!(text3 == "1"))
        {
            if (!(text3 == "2"))
            {
                Console.WriteLine("Invalid dataset choice.");
                return;
            }
            datasetSize = "small";
            text4 = "SMALL DATA";
        }
        else
        {
            datasetSize = "large";
            text4 = "LARGE DATA";
        }
        RoadGraph roadGraph = LoadDataset(datasetSize);
        Console.WriteLine();
        Console.WriteLine("Dataset: " + text4);
        Console.WriteLine("Nodes: " + roadGraph.LocationCount());
        Console.WriteLine("Edges: " + roadGraph.RoadCount());
        BenchmarkService benchmarkService = new BenchmarkService(roadGraph);
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("1. SHORTEST PATH COMPARISON");
        Console.WriteLine("Dijkstra vs Bellman-Ford");
        Console.WriteLine("==========================================");
        Console.Write("Start location: ");
        string text5 = Console.ReadLine() ?? string.Empty;
        Console.Write("Destination: ");
        string text6 = Console.ReadLine() ?? string.Empty;
        if (!roadGraph.ContainsLocation(text5) || !roadGraph.ContainsLocation(text6))
        {
            Console.WriteLine("Invalid start or destination.");
            return;
        }
        RoutingConstraints routingConstraints = new RoutingConstraints();
        routingConstraints.AvoidClosedRoads = true;
        CompareRoutingAlgorithms(benchmarkService, text5, text6, routingConstraints);
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("2. CONSTRAINED ROUTING COMPARISON");
        Console.WriteLine("Dijkstra vs Bellman-Ford");
        Console.WriteLine("==========================================");
        Console.WriteLine("1. Avoid High-Traffic Roads");
        Console.WriteLine("2. Avoid Selected Road Type");
        Console.WriteLine("3. Avoid Closed Roads");
        Console.WriteLine("4. Avoid Selected Location");
        Console.WriteLine("5. Avoid Selected Road");
        Console.Write("Select constraint: ");
        string text7 = Console.ReadLine() ?? string.Empty;
        Console.Write("Start location: ");
        string text8 = Console.ReadLine() ?? string.Empty;
        Console.Write("Destination: ");
        string text9 = Console.ReadLine() ?? string.Empty;
        if (!roadGraph.ContainsLocation(text8) || !roadGraph.ContainsLocation(text9))
        {
            Console.WriteLine("Invalid start or destination.");
            return;
        }
        RoutingConstraints routingConstraints2 = new RoutingConstraints();
        switch (text7)
        {
            case "1":
                routingConstraints2.AvoidClosedRoads = true;
                routingConstraints2.AvoidHighTraffic = true;
                Console.WriteLine("Constraint: Avoid traffic levels 4 and 5.");
                break;
            case "2":
                {
                    routingConstraints2.AvoidClosedRoads = true;
                    routingConstraints2.RestrictRoadTypes = true;
                    string? text11 = AskRoadType();
                    if (text11 == null)
                    {
                        return;
                    }
                    routingConstraints2.RestrictedRoadTypes.Add(text11);
                    Console.WriteLine("Constraint: Avoid " + text11);
                    break;
                }
            case "3":
                routingConstraints2.AvoidClosedRoads = true;
                Console.WriteLine("Constraint: Avoid closed roads.");
                break;
            case "4":
                {
                    routingConstraints2.AvoidClosedRoads = true;
                    Console.Write("Location ID to avoid: ");
                    string text12 = Console.ReadLine() ?? string.Empty;
                    if (!roadGraph.ContainsLocation(text12))
                    {
                        Console.WriteLine("Location does not exist.");
                        return;
                    }
                    if (string.Equals(text12, text8, StringComparison.OrdinalIgnoreCase) || string.Equals(text12, text9, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("The start or destination cannot be excluded.");
                        return;
                    }
                    routingConstraints2.ExcludedLocationIDs.Add(text12);
                    Console.WriteLine("Constraint: Avoid location " + text12);
                    break;
                }
            case "5":
                {
                    routingConstraints2.AvoidClosedRoads = true;
                    Console.Write("Road ID to avoid: ");
                    string text10 = Console.ReadLine() ?? string.Empty;
                    if (!roadGraph.ContainsRoad(text10))
                    {
                        Console.WriteLine("Road does not exist.");
                        return;
                    }
                    routingConstraints2.ExcludedRoadIDs.Add(text10);
                    Console.WriteLine("Constraint: Avoid road " + text10);
                    break;
                }
            default:
                Console.WriteLine("Invalid constraint.");
                return;
        }
        CompareRoutingAlgorithms(benchmarkService, text8, text9, routingConstraints2);
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("3. CONNECTIVITY COMPARISON");
        Console.WriteLine("BFS vs DFS");
        Console.WriteLine("==========================================");
        AlgorithmBenchmarkResult algorithmBenchmarkResult = benchmarkService.BenchmarkBFS();
        AlgorithmBenchmarkResult algorithmBenchmarkResult2 = benchmarkService.BenchmarkDFS();
        DisplayConnectivityBenchmark(algorithmBenchmarkResult);
        DisplayConnectivityBenchmark(algorithmBenchmarkResult2);
        Console.WriteLine();
        Console.WriteLine("Results Match: " + (benchmarkService.ConnectivityMatches(algorithmBenchmarkResult, algorithmBenchmarkResult2) ? "YES" : "NO"));
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("4. BOTTLENECK LOCATION COMPARISON");
        Console.WriteLine("Tarjan vs Brute-Force Node Removal");
        Console.WriteLine("==========================================");
        AlgorithmBenchmarkResult algorithmBenchmarkResult3 = benchmarkService.BenchmarkTarjanArticulationPoints();
        AlgorithmBenchmarkResult algorithmBenchmarkResult4 = benchmarkService.BenchmarkBruteForceArticulationPoints();
        DisplayLocationBenchmark(algorithmBenchmarkResult3);
        DisplayLocationBenchmark(algorithmBenchmarkResult4);
        Console.WriteLine();
        Console.WriteLine("Results Match: " + (benchmarkService.CriticalLocationsMatch(algorithmBenchmarkResult3, algorithmBenchmarkResult4) ? "YES" : "NO"));
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("5. BOTTLENECK ROAD COMPARISON");
        Console.WriteLine("Tarjan vs Brute-Force Edge Removal");
        Console.WriteLine("==========================================");
        AlgorithmBenchmarkResult algorithmBenchmarkResult5 = benchmarkService.BenchmarkTarjanBridges();
        AlgorithmBenchmarkResult algorithmBenchmarkResult6 = benchmarkService.BenchmarkBruteForceBridges();
        DisplayRoadBenchmark(algorithmBenchmarkResult5);
        DisplayRoadBenchmark(algorithmBenchmarkResult6);
        Console.WriteLine();
        Console.WriteLine("Results Match: " + (benchmarkService.CriticalRoadsMatch(algorithmBenchmarkResult5, algorithmBenchmarkResult6) ? "YES" : "NO"));

        // The growth and density evidence belongs to the
        // performance workflow, not to the main application menu.
        HandleScalabilityStudy();

        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("     PERFORMANCE EVALUATION COMPLETE");
        Console.WriteLine("==========================================");
        Console.WriteLine("Memory values are approximate .NET managed-memory changes.");
    }

    private static void HandleScalabilityStudy()
    {
        Console.WriteLine();
        Console.WriteLine("==========================================");
        Console.WriteLine("       SPARSE / DENSE SCALING STUDY");
        Console.WriteLine("==========================================");
        Console.WriteLine(
            "Each value is an average after an unmeasured warm-up run.");
        Console.WriteLine("Running benchmarks...");

        ScalabilityBenchmarkService benchmark =
            new ScalabilityBenchmarkService();

        List<ScalabilityBenchmarkResult> results =
            benchmark.RunDefaultStudy();

        string? currentNetworkType = null;

        foreach (ScalabilityBenchmarkResult result in results)
        {
            if (!string.Equals(
                currentNetworkType,
                result.NetworkType,
                StringComparison.Ordinal))
            {
                currentNetworkType = result.NetworkType;
                Console.WriteLine();
                Console.WriteLine(currentNetworkType.ToUpperInvariant());
                Console.WriteLine(
                    "Nodes | Edges | Density | Reps | " +
                    "Dijkstra ms | Bellman-Ford ms | Match");
            }

            Console.WriteLine(
                result.NodeCount.ToString().PadLeft(5) + " | " +
                result.EdgeCount.ToString().PadLeft(5) + " | " +
                result.Density.ToString("F4").PadLeft(7) + " | " +
                result.Repetitions.ToString().PadLeft(4) + " | " +
                result.DijkstraAverageMilliseconds
                    .ToString("F6").PadLeft(11) + " | " +
                result.BellmanFordAverageMilliseconds
                    .ToString("F6").PadLeft(15) + " | " +
                (result.ResultsMatch ? "YES" : "NO"));
        }

        string dataDirectory =
            Directory.GetParent(GetDataDirectory("small"))?.FullName ??
            AppContext.BaseDirectory;

        string csvPath =
            Path.Combine(
                dataDirectory,
                "performance_scaling_results.csv");

        benchmark.WriteCsv(
            csvPath,
            results);

        Console.WriteLine();
        Console.WriteLine("Chart-ready CSV saved to:");
        Console.WriteLine(csvPath);
        Console.WriteLine(
            "Memory columns report average managed bytes allocated per run.");
    }

    private static void CompareRoutingAlgorithms(BenchmarkService benchmark, string start, string destination, RoutingConstraints constraints)
    {
        AlgorithmBenchmarkResult algorithmBenchmarkResult = benchmark.BenchmarkDijkstra(start, destination, constraints);
        AlgorithmBenchmarkResult algorithmBenchmarkResult2 = benchmark.BenchmarkBellmanFord(start, destination, constraints);
        DisplayRoutingBenchmark(algorithmBenchmarkResult);
        DisplayRoutingBenchmark(algorithmBenchmarkResult2);
        Console.WriteLine();
        Console.WriteLine("Results Match: " + (benchmark.PathsMatch(algorithmBenchmarkResult, algorithmBenchmarkResult2) ? "YES" : "NO"));
    }

    private static void DisplayRoutingBenchmark(AlgorithmBenchmarkResult result)
    {
        Console.WriteLine();
        Console.WriteLine(result.AlgorithmName);
        if (result.PathResult == null || !result.PathResult.PathFound)
        {
            Console.WriteLine("Result: No path found");
        }
        else
        {
            Console.WriteLine("Result: " + string.Join(" -> ", result.PathResult.Path));
            Console.WriteLine("Travel Time: " + result.PathResult.TotalTravelTime + " minutes");
        }
        Console.WriteLine("Average Execution Time: " + result.AverageMilliseconds.ToString("F6") + " ms");
        Console.WriteLine("Approx. Managed Memory Change: " + result.ApproximateMemoryBytes + " bytes");
    }

    private static void DisplayConnectivityBenchmark(AlgorithmBenchmarkResult result)
    {
        Console.WriteLine();
        Console.WriteLine(result.AlgorithmName);
        Console.WriteLine("Connected: " + (result.Connected ? "YES" : "NO"));
        Console.WriteLine("Average Execution Time: " + result.AverageMilliseconds.ToString("F6") + " ms");
        Console.WriteLine("Approx. Managed Memory Change: " + result.ApproximateMemoryBytes + " bytes");
    }

    private static void DisplayLocationBenchmark(AlgorithmBenchmarkResult result)
    {
        Console.WriteLine();
        Console.WriteLine(result.AlgorithmName);
        Console.WriteLine("Critical Locations: " + ((result.CriticalLocations.Count == 0) ? "None" : string.Join(", ", result.CriticalLocations.OrderBy((string location) => location))));
        Console.WriteLine("Average Execution Time: " + result.AverageMilliseconds.ToString("F6") + " ms");
        Console.WriteLine("Approx. Managed Memory Change: " + result.ApproximateMemoryBytes + " bytes");
    }

    private static void DisplayRoadBenchmark(AlgorithmBenchmarkResult result)
    {
        Console.WriteLine();
        Console.WriteLine(result.AlgorithmName);
        Console.WriteLine("Critical Connections:");
        if (result.CriticalRoads.Count == 0)
        {
            Console.WriteLine("None");
        }
        else
        {
            foreach (var criticalRoad in result.CriticalRoads)
            {
                Console.WriteLine(criticalRoad.From + " <-> " + criticalRoad.To);
            }
        }
        Console.WriteLine("Average Execution Time: " + result.AverageMilliseconds.ToString("F6") + " ms");
        Console.WriteLine("Approx. Managed Memory Change: " + result.ApproximateMemoryBytes + " bytes");
    }

    private static void HandleClosureComparison(RoutingService routingService)
    {
        Console.WriteLine();
        Console.WriteLine("===== BASELINE VS ROAD CLOSURES =====");
        Console.Write("Start location ID: ");
        string start = Console.ReadLine() ?? string.Empty;
        Console.Write("Destination location ID: ");
        string destination = Console.ReadLine() ?? string.Empty;
        if (ValidateStartDestination(routingService, start, destination))
        {
            PathResult result = routingService.FindBaselinePath(start, destination);
            PathResult result2 = routingService.FindOptimalPath(start, destination);
            Console.WriteLine();
            Console.WriteLine("Baseline Path:");
            DisplayPathResult(result);
            Console.WriteLine();
            Console.WriteLine("Operational Path (Closed Roads Avoided):");
            DisplayPathResult(result2);
        }
    }

    private static string ReadRequiredText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = ReadConsoleLine().Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("A value is required.");
                continue;
            }

            if (value.Contains(',') || value.Contains('\r') || value.Contains('\n'))
            {
                Console.WriteLine("The value cannot contain commas or line breaks.");
                continue;
            }

            return value;
        }
    }

    private static double ReadDouble(
        string prompt,
        double minimum,
        double maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = ReadConsoleLine();

            if (double.TryParse(
                    value,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double result) &&
                result >= minimum &&
                result <= maximum)
            {
                return result;
            }

            Console.WriteLine(
                "Enter a number between " +
                minimum.ToString(CultureInfo.InvariantCulture) +
                " and " +
                maximum.ToString(CultureInfo.InvariantCulture) +
                ".");
        }
    }

    private static int ReadInteger(
        string prompt,
        int minimum,
        int maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            string value = ReadConsoleLine();

            if (int.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int result) &&
                result >= minimum &&
                result <= maximum)
            {
                return result;
            }

            Console.WriteLine(
                "Enter a whole number between " +
                minimum.ToString(CultureInfo.InvariantCulture) +
                " and " +
                maximum.ToString(CultureInfo.InvariantCulture) +
                ".");
        }
    }

    private static string ReadLocationID(
        RoadGraph roadGraph,
        string prompt)
    {
        while (true)
        {
            string locationID = ReadRequiredText(prompt);

            if (roadGraph.ContainsLocation(locationID))
            {
                return locationID;
            }

            Console.WriteLine("Location does not exist.");
        }
    }

    private static string ReadRoadStatus()
    {
        while (true)
        {
            string status = ReadRequiredText(
                "Enter Status (Open/Close/Closed): ");

            if (status.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                return "Open";
            }

            if (status.Equals("Close", StringComparison.OrdinalIgnoreCase))
            {
                return "Close";
            }

            if (status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                return "Closed";
            }

            Console.WriteLine("Status must be Open, Close, or Closed.");
        }
    }

    private static string ReadConsoleLine()
    {
        return Console.ReadLine() ??
            throw new EndOfStreamException("No console input was available.");
    }

    private static string? AskRoadType()
    {
        Console.WriteLine();
        Console.WriteLine("Select Road Type to Avoid:");
        Console.WriteLine("1. Main Road");
        Console.WriteLine("2. Mountain Road");
        Console.WriteLine("3. Coastal Road");
        Console.WriteLine("4. Express Route");
        Console.Write("Enter choice: ");
        switch (Console.ReadLine() ?? string.Empty)
        {
            case "1":
                return "Main Road";
            case "2":
                return "Mountain Road";
            case "3":
                return "Coastal Road";
            case "4":
                return "Express Route";
            default:
                Console.WriteLine("Invalid road type.");
                return null;
        }
    }

    private static bool ValidateStartDestination(RoutingService routingService, string start, string destination)
    {
        if (!routingService.LocationExists(start))
        {
            Console.WriteLine("Start location does not exist.");
            return false;
        }
        if (!routingService.LocationExists(destination))
        {
            Console.WriteLine("Destination location does not exist.");
            return false;
        }
        return true;
    }

    private static void DisplayPathResult(PathResult result)
    {
        if (!result.PathFound)
        {
            Console.WriteLine("No path found.");
            return;
        }
        Console.WriteLine("Path: " + string.Join(" -> ", result.Path));
        Console.WriteLine(
            "Travel Time: " +
            result.TotalTravelTime +
            " minutes");
    }
}
