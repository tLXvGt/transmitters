﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Transmitters
{
    public class Graph
    {
        public List<Node> nodes;
        public List<Edge> edges;

        public Graph()
        {
            nodes = new List<Node>();
            edges = new List<Edge>();
        }

        public Graph(List<Node> inputNodes, List<Edge> inputEdges)
        {
            nodes = inputNodes;
            edges = inputEdges;
        }


        public void GenerateGXL(string fileName)
        {
            XElement gxl = new XElement("gxl");
            XElement graph = new XElement("graph");

            foreach (var item in nodes)
            {
                XElement node = new XElement("node");
                XAttribute id = new XAttribute("id", item.name);
                XAttribute xCoordinate = new XAttribute("x",
                    Math.Round(item.radius * Math.Cos(item.angle * Math.PI / 180), 4));
                XAttribute yCoordinate = new XAttribute("y",
                    Math.Round(item.radius * Math.Sin(item.angle * Math.PI / 180), 4));

                node.Add(id, xCoordinate, yCoordinate);
                graph.Add(node);
            }

            foreach (var item in edges)
            {
                XElement edge = new XElement("edge");
                XAttribute id = new XAttribute("id", item.id);
                XAttribute start = new XAttribute("start", item.start.name);
                XAttribute end = new XAttribute("end", item.end.name);

                edge.Add(id, start, end);
                graph.Add(edge);
            }

            gxl.Add(graph);
            gxl.Save(fileName);
        }

        public static Graph ReadGXL(string fileName)
        {
            XDocument gxlDocument = XDocument.Load(fileName);
            var graphNode = gxlDocument.Descendants().Where(n => n.Name == "graph");

            var rawNodes = graphNode.Descendants().Where(n => n.Name == "node");
            var rawEdges = graphNode.Descendants().Where(e => e.Name == "edge");

            var listOfReceivedNodes = new List<Node>();
            foreach (var node in rawNodes)
            {
                var resolvedName = node.Attribute("id").Value.ToString();

                var xCoordinate = double.Parse(node.Attribute("x").Value.ToString(), CultureInfo.InvariantCulture);
                var yCoordinate = double.Parse(node.Attribute("y").Value.ToString(), CultureInfo.InvariantCulture);

                var resolvedRadius = Math.Sqrt(Math.Pow(xCoordinate, 2) + Math.Pow(yCoordinate, 2));
                var resolvedAngle = Math.Atan2(yCoordinate, xCoordinate) * 180 / Math.PI;

                listOfReceivedNodes.Add(new Node(resolvedName, resolvedRadius, resolvedAngle));
            }

            var listOfReceivedEdges = new List<Edge>();
            foreach (var edge in rawEdges)
            {
                var resolvedId = edge.Attribute("id").Value.ToString();
                var resolvedStart = edge.Attribute("start").Value.ToString();
                var resolvedEnd = edge.Attribute("end").Value.ToString();

                listOfReceivedEdges.Add(new Edge(
                    listOfReceivedNodes.Where(n => n.name == resolvedStart).Single(),
                    listOfReceivedNodes.Where(n => n.name == resolvedEnd).Single(),
                    int.Parse(resolvedId)
                ));
            }

            return new Graph(listOfReceivedNodes, listOfReceivedEdges);
        }
    }

    public class Node
    {
        public string name;
        public double radius;
        public double angle;
        public int neighbourCount;
        public int color;

        public Node(string name, double radius, double angle)
        {
            this.name = name;
            this.radius = radius;
            this.angle = angle;
            neighbourCount = 0;
            color = -1;
        }

        public double CalculateDistance(Node neighbour)
        {
            return Math.Sqrt(Math.Pow(radius, 2) + Math.Pow(neighbour.radius, 2) - 2 * radius * neighbour.radius
                * Math.Cos(angle * Math.PI / 180 - neighbour.angle * Math.PI / 180));
        }

        public bool HasColor()
        {
            if (color == -1)
            {
                return false;
            }

            return true;
        }

        public bool CanColor(int inputColor, List<Edge> edges)
        {
            var neighbours = GetNeighbours(edges);

            foreach (var neighbour in neighbours)
            {
                if (neighbour.color == inputColor)
                {
                    return false;
                }
            }

            return true;
        }

        public List<Node> GetNeighbours(List<Edge> edges)
        {
            var neighbours = new List<Node>();

            foreach (var edge in edges)
            {
                if (edge.start.name == name)
                {
                    neighbours.Add(edge.end);
                }
                else if (edge.end.name == name)
                {
                    neighbours.Add(edge.start);
                }
            }

            return neighbours;
        }
    }

    public class Edge
    {
        public Node start;
        public Node end;
        public int id;

        public static int lastEdgeId = 1;

        public Edge(Node start, Node end)
        {
            this.start = start;
            this.end = end;
            id = lastEdgeId++;
        }

        public Edge(Node start, Node end, int id)
        {
            this.start = start;
            this.end = end;
            this.id = id;
        }
    }
}
