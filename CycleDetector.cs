using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;

namespace BooleanNetworkSupportTool
{
    class CycleDetector
    {
        public string result;//*********************REMOVE

        public List<string> visitedTrace = new List<string>();

        public List<List<string>> cycles = new List<List<string>>();
        
        public List<string> getNeighbours(string current, AdjacencyGraph<string, Edge<string>> g)
        {
            List<string> neighbours = new List<string>();

            foreach (Edge<string> edge in g.Edges)
            {
                if (edge.Source == current)
                {
                    neighbours.Add(edge.GetOtherVertex(current));
                }
            }

            return neighbours;
        }

        internal void DFSHelper(string src, bool[] visited, HashSet<string> discoveredVertices, HashSet<string> visitedVertices, AdjacencyGraph<string, Edge<string>> g)
        {
            //visited[src] = true;
            //result += (src + "->");
            List<string> neighbours = getNeighbours(src, g);

            visitedVertices.Add(src);
            visitedTrace.Add(src);

            foreach (string v in neighbours)
            {

                if (visitedTrace.Contains(v))
                {
                    int cycleStart = visitedTrace.IndexOf(v);
                    visitedTrace.Add(v);
                    
                    result = string.Join("->", visitedTrace.GetRange(cycleStart, visitedTrace.Count() - cycleStart).ToArray());
                    cycles.Add(new List<string>(visitedTrace.GetRange(cycleStart, visitedTrace.Count() - cycleStart)));
                }

                if (!visitedVertices.Contains(v))
                {
                    DFSHelper(v, visited, discoveredVertices, visitedVertices, g);
                }
            }
        }

        public void DFS(AdjacencyGraph<string, Edge<string>> graph)
        {
            bool[] visited = new bool[graph.Vertices.Count() + 1];
            HashSet<string> discoveredVertices = new HashSet<string>();
            HashSet<string> visitedVertices = new HashSet<string>();

            string first = graph.Vertices.First();

            IEnumerator<string> iterator = graph.Vertices.GetEnumerator();

            while (visitedVertices.Count() < graph.Vertices.Count())
            {
                iterator.MoveNext();
                first = iterator.Current;
                if (!visitedVertices.Contains(first))
                {
                    DFSHelper(first, visited, discoveredVertices, visitedVertices, graph);
                }
            }
        }
    }
}