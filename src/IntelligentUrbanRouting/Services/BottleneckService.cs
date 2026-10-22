using System.Collections.Generic;
using IntelligentUrbanRouting.Algorithms;
using IntelligentUrbanRouting.Graphs;

namespace IntelligentUrbanRouting.Services
{
    public class BottleneckService
    {
        private readonly RoadGraph roadGraph;

        private readonly StructuralGraphBuilder
            structuralGraphBuilder;


        public BottleneckService(
            RoadGraph graph)
        {
            roadGraph = graph;

            structuralGraphBuilder =
                new StructuralGraphBuilder();
        }


        // =====================================================
        // BOTTLENECK LOCATIONS
        // =====================================================

        public HashSet<string>
            FindBottleneckLocations()
        {
            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(
                        roadGraph);


            TarjanAlgorithm tarjan =
                new TarjanAlgorithm();


            return tarjan.FindArticulationPoints(
                structuralGraph);
        }


        // =====================================================
        // BOTTLENECK ROADS
        // =====================================================

        public List<(string From, string To)>
            FindBottleneckRoads()
        {
            Dictionary<string, List<string>>
                structuralGraph =
                    structuralGraphBuilder.Build(
                        roadGraph);


            TarjanAlgorithm tarjan =
                new TarjanAlgorithm();


            return tarjan.FindBridges(
                structuralGraph);
        }
    }
}