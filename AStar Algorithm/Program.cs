using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static AStar_Algorithm.Node;

namespace AStar_Algorithm
{
    internal class Program
    {
        static readonly int mapWidth  = 43;
        static readonly int mapHeight = 151;
        const char blockChar = '█';
        const char emptyCellChar = ' ';
        static void Main(string[] args)
        {
            Console.WindowHeight = Console.LargestWindowHeight;
            Console.WindowWidth = Console.LargestWindowWidth;
            Stopwatch mazeGeneration = new Stopwatch();
            mazeGeneration.Start();
            Node[,] maze = GenerateMaze(mapWidth, mapHeight);
            mazeGeneration.Stop();
            PrintMaze(maze);
            Console.ReadKey();
            Console.Clear();
            Point[] obstacles = GetObstaclesFromMaze(maze);
            Random random = new Random();

            int x; int y;
            Point start;
            do
            {
                x = random.Next(mapWidth);
                y = random.Next(mapHeight);

            } while (x % 2 == 0 || y % 2 == 0);
            start = new Point(x, y);
            Point end;
            do
            {
                x = random.Next(mapWidth);
                y = random.Next(mapHeight);

            } while (x % 2 == 0 || y % 2 == 0);
            end = new Point(x, y);

            start.x = start.y = 1;
            end.x = mapWidth - 2;
            end.y = mapHeight - 2;

            Stopwatch astar = new Stopwatch();
            astar.Start();
            var path = AStar_FindPath(mapWidth, mapHeight, obstacles, start, end, MovementDirections.STRAIGHT_LINE);
            astar.Stop();
            Console.Title = $"Recursive backtracking and A* Demo | Maze generation: {mazeGeneration.ElapsedMilliseconds} ms | A*: {astar.ElapsedMilliseconds} ms";
            PrintPath(path, obstacles, start, end);
            Console.ReadKey();
        }
        static Point[] GetObstaclesFromMaze(in Node[,] maze)
        {
            List<Node> obstacles = new List<Node>();
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if (maze[i, j].nodeType == NodeType.OBSTACLE)
                    {
                        obstacles.Add(maze[i, j]);
                    }
                }
            }
            return obstacles.ToArray();
        }
        public static void PrintMaze(in Node[,] maze)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    Console.Write(maze[i, j].nodeType == NodeType.EMPTY ? emptyCellChar : blockChar);
                }
                Console.WriteLine();
            }
        }
        public static void PrintPath(List<Point> generatedPath, Point[] obstacles, Point start, Point end)
        {
            if (generatedPath is null) return;

            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    if(obstacles.Any(el => el.x == i && el.y == j))
                    {
                        Console.BackgroundColor = ConsoleColor.Blue;
                        Console.Write(blockChar);
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    else if(generatedPath.Any(el => el.x == i && el.y == j))
                    {
                        if(i == start.x && j == start.y)
                        {
                            Console.BackgroundColor = ConsoleColor.Green;
                            Console.Write(emptyCellChar);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else if (i == end.x && j == end.y)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.Write(emptyCellChar);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Magenta;
                            Console.Write(emptyCellChar);
                            Console.BackgroundColor = ConsoleColor.Black;
                        }
                    }
                    else
                    {
                        Console.Write(emptyCellChar);
                    }
                }
                Console.WriteLine();
            }
        }
        public static Node[,] GenerateMaze(int width, int height)
        {
            int emptyNodes = 0;
            Node[,] GenerateMap()
            {
                Node[,] generatedMap = new Node[width, height];
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if(i % 2 != 0 && j % 2 != 0)
                        {
                            generatedMap[i, j] = new Node(i, j, NodeType.EMPTY);
                            emptyNodes++;
                        }
                        else
                        {
                            generatedMap[i, j] = new Node(i, j, NodeType.OBSTACLE);
                        }
                        
                    }
                }
                return generatedMap;
            }
            Node[,] maze = GenerateMap();
            Random random = new Random();
            Node startNode = maze[1, 1];// [random.Next(width), random.Next(height)];
            List<Node> visitedCells = new List<Node>() { startNode };

            Stack<Node> stack = new Stack<Node>();
            stack.Push(startNode);
            Vector[] movementDirections = new Vector[]
            {
                new Vector(0, 2),
                new Vector(0, -2),
                new Vector(2, 0),
                new Vector(-2, 0)
            };

            List<Node> GetNeighbours(Node currentCell)
            {
                List<Node> neighbouringCells = new List<Node>();
                foreach (var directionVector in movementDirections)
                {
                    Node neighbouringCell = new Node(directionVector.x + currentCell.x, directionVector.y + currentCell.y);
                    if (!neighbouringCell.IsInBonds(width, height)) continue;
                    if (visitedCells.Any(el => el == neighbouringCell)) continue;
                    neighbouringCells.Add(neighbouringCell);
                    //visitedCells.Add(neighbouringCell);
                }
                return neighbouringCells;
            }

            Node Backtrack(Node lastNode)
            {
                Node currentNode = lastNode;
                while (GetNeighbours(currentNode).Count == 0)
                {
                    currentNode = currentNode.parent;
                }
                return currentNode;
            }

            int Average(int x, int y)
            {
                return (int)Math.Floor((double)((x + y) / 2));
            }

            while (true)
            {
                Node currentCell = stack.Pop();
                List<Node> neighbouringCells = GetNeighbours(currentCell);

                if (neighbouringCells.Count != 0)
                {
                    Node neighbouringCell = neighbouringCells[random.Next(neighbouringCells.Count)];
                    maze[Average(currentCell.x, neighbouringCell.x), Average(currentCell.y, neighbouringCell.y)].nodeType = NodeType.EMPTY;
                    neighbouringCell.parent = currentCell;
                    visitedCells.Add(neighbouringCell);
                    stack.Push(neighbouringCell);
                }
                else 
                {
                    if (visitedCells.Count == emptyNodes) return maze;
                    stack.Push(Backtrack(currentCell));
                }
            }
        }
        public static List<Point> AStar_FindPath(int mapWidth, int mapHeight, Point[] obstacles, Point start, Point destination, MovementDirections direction)
        {
            if (start == destination) return new List<Point>();

            else if (!destination.IsInBonds(mapWidth, mapHeight)) throw new ArgumentException("Destination is not in map!");
            
            else if (!start.IsInBonds(mapWidth, mapHeight)) throw new ArgumentException("Start is not in map!");
            
            else if (obstacles.Any(el => el == start)) throw new ArgumentException("Start cannot be an obstacle!");
            
            else if (obstacles.Any(el => el == destination)) throw new ArgumentException("Destionation cannot be an obstacle!");

            Vector[] movementDirections;
            if (direction == MovementDirections.DIAGONAL)
            {
                movementDirections = new Vector[]
                {
                new Vector(0, 1),
                new Vector(0, -1),
                new Vector(1, 0),
                new Vector(-1, 0),
                new Vector(1, 1),
                new Vector(1, -1),
                new Vector(-1, -1),
                new Vector(-1, 1)
                };
            }
            else
            {
                movementDirections = new Vector[]
                {
                new Vector(0, 1),
                new Vector(0, -1),
                new Vector(1, 0),
                new Vector(-1, 0)
                };
            }
            List<Point> ReconstructPath(Node lastNode)
            {
                List<Point> path = new List<Point>();
                Node currentNode = lastNode;
                while (currentNode != start)
                {
                    path.Add(currentNode);
                    currentNode = currentNode.parent;
                }
                path.Add(currentNode);
                path.Reverse();
                return path;
            }

            Node startNode = Node.PointToNode(start);
            startNode.f = startNode.HeuristicDistance(destination);
            List<Node> closedSet = new List<Node>();
            List<Node> openSet = new List<Node>() { startNode };

            Node FindNodeWithLowestFcost() => openSet.OrderBy(x => x.f).First();

            while (openSet.Count > 0)
            {
                Node currentNode = FindNodeWithLowestFcost();
                if (currentNode == destination) return ReconstructPath(currentNode);
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                foreach (var directionVector in movementDirections)
                {
                    int successor_x = currentNode.x + directionVector.x; // x = column | y = row
                    int successor_y = currentNode.y + directionVector.y;
                    Node successor = new Node(successor_x, successor_y, parent: currentNode);
                    if      (!successor.IsInBonds(mapWidth, mapHeight)) continue;
                    else if (obstacles.Any(x => x == successor)) continue;
                    else if (closedSet.Any(x => x == successor)) continue;

                    int tentative_g = currentNode.g + successor.DistanceToNeighbor(currentNode);

                    if(!openSet.Any(x => x == successor))
                    {
                        openSet.Add(successor);
                    }
                    else if(tentative_g >= successor.g)
                    {
                        continue;
                    }

                    successor.parent = currentNode;
                    successor.g = tentative_g;
                    successor.f += successor.HeuristicDistance(destination);
                }
            }
            return null;
        }
        public static T[] ShuffleArrayRandomly<T>(T[] array)
        {
            Random random = new Random();
            for (int i = 0; i < array.Length; i++)
            {
                SwapReference(ref array[i], ref array[random.Next(array.Length)]);
            }
            return array;
        }
        public static void SwapReference<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }
    }

    class Point
    {
        public int x;
        public int y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString()
        {
            return $"[{x}; {y}]";
        }
        public (int, int) Deconstruct()
        {
            return (x, y);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator == (Point pointA, Point pointB)
        {
            return pointA.x == pointB.x && pointA.y == pointB.y;
        }
        public static bool operator != (Point pointA, Point pointB)
        {
            return pointA.x != pointB.x || pointA.y != pointB.y;
        }
        public bool IsInBonds(int width, int height)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }
    }
    sealed class Vector : Point
    {
        public Vector(int x, int y) : base(x, y) { }
    }
    internal sealed class Node : Point
    {
        internal NodeType nodeType;
        internal int g; // from start to this
        internal int f; // g + h
        internal Node parent;
        internal Node(int x, int y, NodeType nodeType = NodeType.EMPTY) : base(x, y)
        {
            this.nodeType = nodeType;
        }
        internal Node(int x, int y, Node parent, NodeType nodeType = NodeType.EMPTY) : base(x, y)
        {
            this.nodeType = nodeType;
            this.parent = parent;
        }
        internal new (int, int, NodeType) Deconstruct()
        {
            return (x, y, nodeType);
        }
        public override string ToString()
        {
            return $"[{x}; {y}]; f:{f}; g:{g}; {nodeType}";
        }
        internal enum NodeType
        {
            EMPTY = 1,
            OBSTACLE = 2
        }
        internal static Node PointToNode(Point point, NodeType pointType = NodeType.EMPTY)
        {
            return new Node(point.x, point.y, pointType);
        }
        internal int DistanceToNeighbor(Node neighbor)
        {
            return 1;
        }
        internal int HeuristicDistance(Point destination)
        {
            int dx = Math.Abs(x - destination.x);
            int dy = Math.Abs(y - destination.y);
            return dx + dy;
        }
        public enum MovementDirections
        {
            STRAIGHT_LINE,
            DIAGONAL
        }
    }
}
