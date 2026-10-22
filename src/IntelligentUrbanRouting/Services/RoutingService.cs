using System;
using System.Collections.Generic;
using IntelligentUrbanRouting.Algorithms;
using IntelligentUrbanRouting.Graphs;
using IntelligentUrbanRouting.Models;

namespace IntelligentUrbanRouting.Services
{
    public class RoutingService
    {
        private readonly RoadGraph graph;

        private readonly DijkstraAlgorithm dijkstra;


        public RoutingService(
            RoadGraph graph)
        {
            this.graph =
                graph ??
                throw new ArgumentNullException(
                    nameof(graph));


            dijkstra =
                new DijkstraAlgorithm(graph);
        }


        // =====================================================
        // FASTEST OPERATIONAL PATH
        // =====================================================

        public PathResult FindOptimalPath(
            string start,
            string destination)
        {
            RoutingConstraints constraints =
                new RoutingConstraints();


            constraints.AvoidClosedRoads = true;


            return dijkstra.FindShortestPath(
                start,
                destination,
                constraints);
        }
        // =====================================================
        // AVOID HIGH-TRAFFIC ROADS
        // =====================================================

        public PathResult FindLowTrafficPath(
            string start,
            string destination)
        {
            RoutingConstraints constraints =
                new RoutingConstraints();


            constraints.AvoidClosedRoads = true;

            constraints.AvoidHighTraffic = true;


            return dijkstra.FindShortestPath(
                start,
                destination,
                constraints);
        }


        // =====================================================
        // AVOID SELECTED ROAD TYPE
        // =====================================================

        public PathResult FindPathAvoidingRoadTypes(
            string start,
            string destination,
            IEnumerable<string> restrictedTypes)
        {
            RoutingConstraints constraints =
                new RoutingConstraints();


            constraints.AvoidClosedRoads = true;

            constraints.RestrictRoadTypes = true;


            foreach (
                string roadType
                in restrictedTypes)
            {
                if (!string.IsNullOrWhiteSpace(
                    roadType))
                {
                    constraints
                        .RestrictedRoadTypes
                        .Add(
                            roadType.Trim());
                }
            }


            return dijkstra.FindShortestPath(
                start,
                destination,
                constraints);
        }


        // =====================================================
        // AVOID SELECTED LOCATION
        // =====================================================

        public PathResult FindPathAvoidingLocation(
            string start,
            string destination,
            string excludedLocationID)
        {
            RoutingConstraints constraints =
                new RoutingConstraints();


            constraints.AvoidClosedRoads = true;


            constraints
                .ExcludedLocationIDs
                .Add(
                    excludedLocationID);


            return dijkstra.FindShortestPath(
                start,
                destination,
                constraints);
        }


        // =====================================================
        // AVOID SELECTED ROAD
        // =====================================================

        public PathResult FindPathAvoidingRoad(
            string start,
            string destination,
            string excludedRoadID)
        {
            RoutingConstraints constraints =
                new RoutingConstraints();


            constraints.AvoidClosedRoads = true;


            constraints
                .ExcludedRoadIDs
                .Add(
                    excludedRoadID);


            return dijkstra.FindShortestPath(
                start,
                destination,
                constraints);
        }


        // =====================================================
        // BASELINE PATH
        //
        // Used only for comparing a route before and after
        // applying road-closure restrictions.
        // =====================================================

        public PathResult FindBaselinePath(
            string start,
            string destination)
        {
            RoutingConstraints constraints =
                new RoutingConstraints();


            // Deliberately does not exclude closed roads.
            // This is a comparison baseline, not an
            // operational route recommendation.

            return dijkstra.FindShortestPath(
                start,
                destination,
                constraints);
        }


        // =====================================================
        // VALIDATION HELPERS
        // =====================================================

        public bool LocationExists(
            string locationID)
        {
            return graph.ContainsLocation(
                locationID);
        }


        public bool RoadExists(
            string roadID)
        {
            return graph.ContainsRoad(
                roadID);
        }
    }
}
