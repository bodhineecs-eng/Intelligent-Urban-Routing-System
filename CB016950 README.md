# Intelligent Urban Routing and Network Analysis System

Student ID: **CB016950**  
Module: **COMP50065 Data Structures and Algorithms**
Language: **C# 12** · Framework: **.NET 8.0** · Dependencies: **none**

## Overview

This C# console application models an urban road network as a directed, weighted graph. It finds operational routes, applies routing constraints, calculates locations reachable within a time budget, checks network connectivity, detects bottleneck locations and roads, and compares the selected algorithms with alternative approaches.

The graph uses travel time as its non-negative edge weight. Road closure, congestion, road type, excluded locations, and excluded roads can be used as routing filters.

## Main Features

- Find the fastest operational path with Dijkstra's algorithm.
- Find a constrained path that also avoids high traffic, a selected road type, a selected location, or a selected road.
- Find all locations reachable within a travel-time budget.
- Check weak physical connectivity and strong operational connectivity.
- Detect bottleneck locations (articulation points) and bottleneck roads (bridges).
- Compare Dijkstra with Bellman-Ford, BFS with DFS, and Tarjan with brute-force removal.
- Run a sparse-versus-dense routing scalability study.
- Compare a baseline route with a route that avoids road closures.
- Display and add location and road records.

## Requirements

- .NET 8 SDK or a later compatible SDK
- A terminal, PowerShell, or command prompt

No external NuGet packages are required.

## Build and Run

From the repository root, run:

```powershell
dotnet build IntelligentUrbanRouting.sln --configuration Release
dotnet run --project src/IntelligentUrbanRouting/IntelligentUrbanRouting.csproj
```

The build has been verified successfully with no warnings or errors.

## Console Menu

```text
1. Find Fastest Operational Path
2. Find Constrained Path
3. Reachability Within Time Budget
4. Network Connectivity & Bottlenecks
5. Performance Evaluation
6. Compare Baseline vs Road Closures
7. Add Data
0. Exit
```

The program loads the small dataset when it starts. The performance menu allows the benchmark dataset to be selected.

## Graph Representation

The road network is represented as a directed, weighted adjacency list:

```csharp
Dictionary<string, List<Road>>
```

- The dictionary key is a location ID.
- The value is the list of roads leaving that location.
- Dictionary access to a location's outgoing-road list is average `O(1)`.
- Traversing the neighbours of a location is `O(deg(v))`.
- The total adjacency-list storage is `O(V + E)`.

An adjacency matrix was rejected because it requires `O(V^2)` storage and scans `O(V)` entries to traverse the neighbours of one location. The supplied road datasets are sparse, so an adjacency list avoids storing non-existent connections.

### Directed Roads

Each row in `roads.csv` represents one directed road. A two-way physical road is represented by two independent rows:

```text
A -> B
B -> A
```

Each direction can have a different travel time, congestion level, road type, and open or closed status. A one-way road has only one direction available, or its reverse record may be marked closed.

## Algorithms

| Function | Selected algorithm | Comparison | Selected complexity |
|---|---|---|---|
| Fastest and constrained routing | Dijkstra | Bellman-Ford | `O((V + E) log V)` time |
| Reachability within a time budget | Dijkstra distance calculation | Bellman-Ford validation | `O((V + E) log V)` time |
| Connectivity | BFS | DFS | `O(V + E)` time, `O(V)` auxiliary space |
| Bottleneck locations and roads | Tarjan | Brute-force node and edge removal | `O(V + E)` time |

### Dijkstra and Bellman-Ford

Dijkstra is the main routing algorithm because every travel-time weight is non-negative. It uses distance and predecessor dictionaries, a visited set, and a minimum-priority queue. Before relaxing a road, the algorithm checks whether that road is allowed by the active constraints.

Bellman-Ford is used as an independent comparison. It supports negative weights, but this capability is not required by the dataset. Its worst-case time complexity is `O(VE)`, compared with Dijkstra's `O((V + E) log V)`.

### BFS and DFS

BFS is used for connectivity and stores its frontier in a queue. DFS is implemented with a stack and is used as the comparison algorithm. Both run in `O(V + E)` time and use `O(V)` auxiliary space.

- **Weak physical connectivity:** roads are converted to undirected structural connections, closed roads are included, and BFS runs once.
- **Strong operational connectivity:** road direction is preserved, closed roads are excluded, and BFS runs on both the forward and reversed graphs.

### Tarjan and Brute Force

Tarjan's algorithm runs DFS on the undirected structural graph. Discovery times and low-link values identify articulation points and bridges in one `O(V + E)` traversal.

The validation alternatives repeatedly remove each location or road and recount connected components:

- Brute-force node removal: `O(V(V + E))`
- Brute-force edge removal: `O(E(V + E))`

## Dataset

The project includes two CSV datasets under `data/`.

| Dataset | Locations | Directed roads | Purpose |
|---|---:|---:|---|
| Small | 26 | 60 | Functional demonstrations and results that can be checked manually |
| Large | 1,000 | 2,400 | Performance and scalability testing |

The small dataset uses real Colombo location names and coordinates. The road links and their travel time, congestion, type, and status values were prepared for this project. The large dataset is a regular synthetic grid.

### `locations.csv`

Each row represents one location.

| Field | Meaning |
|---|---|
| `location_id` | Unique location identifier |
| `location_name` | Display name |
| `latitude` | Latitude coordinate |
| `longitude` | Longitude coordinate |

### `roads.csv`

Each row represents one directed road.

| Field | Meaning |
|---|---|
| `road_id` | Unique road identifier |
| `from_id` | Source location |
| `to_id` | Destination location |
| `road_name` | Road display name |
| `travel_time_min` | Non-negative routing weight in minutes |
| `congestion_level` | Numeric traffic level |
| `congestion_category` | Descriptive traffic category |
| `road_type` | Road classification used by constraints |
| `Status` | Open or closed state |

## Scalability Study

`SyntheticGraphFactory` generates reproducible graphs with a fixed random seed.

- Sparse study: 100, 250, 500, 1,000, and 2,000 locations, with approximately 2.4 outgoing roads per location.
- Dense study: 50, 100, 200, and 300 locations, with approximately 20% of all possible directed roads.

Dijkstra and Bellman-Ford receive the same graph, start, destination, and constraints. A warm-up run is performed before measurement. Smaller tests use more repetitions than larger tests, and the reported value is the average execution time. The program verifies that both algorithms return the same path-found status and total travel time.

Results are written to:

```text
data/performance_scaling_results.csv
```

## Validation

The Performance Evaluation menu (option 5) checks that:

- Dijkstra and Bellman-Ford return matching route results.
- BFS and DFS return matching connectivity results.
- Tarjan and brute-force node removal return the same articulation points.
- Tarjan and brute-force edge removal return the same bridges.

Manual testing also confirmed that:

- Constrained routes exclude disallowed elements.
- Reachability results stay within the selected time budget.
- Closed roads are excluded from operational routes.
- Invalid identifiers and invalid numeric input are handled without crashing the program.

## Project Structure

```text
IntelligentUrbanRouting/
|-- IntelligentUrbanRouting.sln
|-- README.md
|-- data/
|   |-- small/
|   |   |-- locations.csv
|   |   `-- roads.csv
|   |-- large/
|   |   |-- locations.csv
|   |   `-- roads.csv
|   `-- performance_scaling_results.csv
`-- src/
    `-- IntelligentUrbanRouting/
        |-- Algorithms/
        |-- Data/
        |-- Graphs/
        |-- Models/
        |-- Services/
        |-- IntelligentUrbanRouting.csproj
        `-- Program.cs
```

## Assumptions

- Travel times are fixed, known, and non-negative.
- Travel time is the routing weight.
- Congestion, road type, and status act as routing filters rather than additional weights.
- Location IDs and road IDs are unique.
- Bottleneck analysis treats roads as undirected physical connections and ignores weight, direction, and status.

## Limitations

- The real-name dataset contains only 26 locations and 60 roads.
- The 1,000-location dataset is synthetic and does not represent a complete real city layout.
- Travel times and congestion do not change during a journey.
- The synthetic grid does not contain the same bottleneck patterns as the small dataset.
- Tarjan uses recursive DFS, which could overflow the call stack on an extremely deep graph.
- Very short benchmark measurements can vary with the runtime environment.

## Possible Improvements

- Stop reachability processing when the next priority-queue entry exceeds the time budget.
- Average routing benchmarks over many random start and destination pairs.
- Support time-dependent or live traffic travel times.
- Test with a larger real-world city road dataset.
- Replace recursive Tarjan traversal with an iterative implementation for extremely large graphs.

## Data Modification Warning

The **Add Data** menu writes new records to the small CSV dataset. Make a backup before demonstrating this option if the original submission dataset must remain unchanged.
