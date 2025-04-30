using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace normalTileStepTest
{
    class Enemy
    {
        public Point coords = new Point(0, 5);
        public Bitmap enemySprite;

        public Tile.TileType previousTileType = Tile.TileType.None;
        public Bitmap previousTileSprite = Properties.Resources.blank;

        public List<Tile.TileType> disallowedTypes = new List<Tile.TileType> { Tile.TileType.Crud, Tile.TileType.Bullet };

        public Form1 form;

        public bool precomputedCanReach;

        //The following code in the sections I have marked with comments was mostly made in a past personal project and was made with the assistance of AI, specifically GPT 3.5 Turbo.
#region ai generated code
        private int GetManhattanDistance(Tile tile, Point target)
        {
            return Math.Abs(tile.gameCoords.X - target.X) + Math.Abs(tile.gameCoords.Y - target.Y);
        }

        public Tile FindNextTile(Map map, Point start, Point target, List<Tile.TileType> disallowedTypes)
        {
            Tile startTile = Tile.tileListSearch(map.tileList, start);
            Tile targetTile = Tile.tileListSearch(map.tileList, target);

            if (targetTile == null || disallowedTypes.Contains(targetTile.tileType))
            {
                return null; // Invalid target tile or target tile is a disallowed type
            }

            HashSet<Tile> openSet = new HashSet<Tile>();
            HashSet<Tile> closedSet = new HashSet<Tile>();
            openSet.Add(startTile);

            Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

            Dictionary<Tile, int> gScore = new Dictionary<Tile, int>();
            gScore[startTile] = 0;

            Dictionary<Tile, int> fScore = new Dictionary<Tile, int>();
            fScore[startTile] = GetManhattanDistance(startTile, target);

            while (openSet.Count > 0)
            {
                Tile current = null;
                int lowestFScore = int.MaxValue;

                foreach (var tile in openSet)
                {
                    // Check that the key type in TryGetValue matches the key type used in the dictionary
                    int f;
                    if (fScore.TryGetValue(tile, out f) && f < lowestFScore)
                    {
                        lowestFScore = f;
                        current = tile;
                    }
                }


                if (current == targetTile)
                {
                    // Reconstruct the path and return the next tile
                    while (cameFrom.ContainsKey(current))
                    {
                        Tile nextTile = cameFrom[current];
                        if (nextTile == startTile)
                            return current;
                        current = nextTile;
                    }
                    break;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(map, current.gameCoords.X, current.gameCoords.Y))
                {
                    if (neighbor == null || closedSet.Contains(neighbor) || disallowedTypes.Contains(neighbor.tileType))
                    {
                        continue;
                    }

                    int tentativeGScore = gScore[current] + 1;
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= gScore[neighbor])
                    {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + GetManhattanDistance(neighbor, target);
                }
            }

            return null; // If the function reaches this point, there is no valid path.
        }

        private IEnumerable<Tile> GetNeighbors(Map map, int x, int y)
        {
            yield return Tile.tileListSearch(map.tileList, new Point(x + 1, y));
            yield return Tile.tileListSearch(map.tileList, new Point(x - 1, y));
            yield return Tile.tileListSearch(map.tileList, new Point(x, y + 1));
            yield return Tile.tileListSearch(map.tileList, new Point(x, y - 1));
            yield return Tile.tileListSearch(map.tileList, new Point(x + 1, y + 1)); // Diagonal neighbor (top-right)
            yield return Tile.tileListSearch(map.tileList, new Point(x + 1, y - 1)); // Diagonal neighbor (bottom-right)
            yield return Tile.tileListSearch(map.tileList, new Point(x - 1, y + 1)); // Diagonal neighbor (top-left)
            yield return Tile.tileListSearch(map.tileList, new Point(x - 1, y - 1)); // Diagonal neighbor (bottom-left)
        }

        public static bool CanEnemyReachPlayer(Map map, Player player)
        {
            // Find the player's tile
            Tile playerTile = Tile.tileListSearch(map.tileList, player.coords);
            if (playerTile == null)
            {
                // Player not found, handle the error or return false
                return false;
            }

            // Initialize the queue for BFS
            Queue<Tile> queue = new Queue<Tile>();
            queue.Enqueue(playerTile);

            // Create a set to keep track of visited tiles
            HashSet<Tile> visitedTiles = new HashSet<Tile>();
            visitedTiles.Add(playerTile);

            // Define the possible neighbor offsets (up, down, left, right, diagonal)
            int[] dx = { 0, 0, -1, 1, -1, -1, 1, 1 };
            int[] dy = { -1, 1, 0, 0, -1, 1, -1, 1 };

            while (queue.Count > 0)
            {
                Tile currentTile = queue.Dequeue();

                // Check if the current tile is an enemy
                if (currentTile.tileType == Tile.TileType.Enemy)
                {
                    return true; // Found an enemy with a valid path to the player
                }

                // Explore neighbors (including diagonals)
                for (int i = 0; i < dx.Length; i++)
                {
                    int neighborX = currentTile.gameCoords.X + dx[i];
                    int neighborY = currentTile.gameCoords.Y + dy[i];

                    // Check if the neighbor is within bounds
                    if (Tile.tileListSearch(map.tileList, new Point(neighborX, neighborY)) != null)
                    {
                        Tile neighborTile = GetTileAtLocation(map, neighborX, neighborY);

                        // Check if the neighbor has not been visited and is not an obstacle
                        if (!visitedTiles.Contains(neighborTile) && neighborTile.tileType != Tile.TileType.Crud && neighborTile.tileType != Tile.TileType.Bullet)
                        {
                            queue.Enqueue(neighborTile);
                            visitedTiles.Add(neighborTile);
                        }
                    }
                }
            }

            // If no enemy was found, return false
            return false;
        }

        // Helper function to get the tile at a specific location
        private static Tile GetTileAtLocation(Map map, int x, int y)
        {
            return map.tileList.Find(tile => tile.gameCoords.X == x && tile.gameCoords.Y == y);
        }
#endregion
        //The section of code created in the past with help from an AI Large Language Model ends here.

        //This is my own straight line "pathfinding" function. Much faster and just for fun, close to the game that inspired this project.
        public Point GetNextTile(Point currentPosition, Point targetPosition)
        {
            Point direction = new Point(targetPosition.X - currentPosition.X, targetPosition.Y - currentPosition.Y);
            direction.X = Math.Sign(direction.X);
            direction.Y = Math.Sign(direction.Y);
            return new Point(currentPosition.X + direction.X, currentPosition.Y + direction.Y);
        }

        private void moveInternal(Map map, int xDelta, int yDelta)//Internal move function with more parameters. For manual movement if you're not using a pathfinder.
        {
            Tile newTile = Tile.tileListSearch(map.tileList, new Point(coords.X + xDelta, coords.Y + yDelta));

            if (coords.X + xDelta > -1 && coords.X + xDelta < form.xTileCount && coords.Y + yDelta > -1 && coords.Y + yDelta < form.yTileCount)
            {
                if (newTile.tileType != Tile.TileType.Crud && newTile.tileType != Tile.TileType.Enemy)
                {
                    Tile currentTile = Tile.tileListSearch(map.tileList, coords);
                    currentTile.tileImage = previousTileSprite;
                    currentTile.tileType = previousTileType;

                    if (!(disallowedTypes.Contains(currentTile.tileType)))
                    {
                        previousTileSprite = newTile.tileImage;
                        previousTileType = newTile.tileType;
                    }
                    else
                    {
                        previousTileSprite = Properties.Resources.blank;
                        previousTileType = Tile.TileType.None;
                    }

                    coords.X += xDelta;
                    coords.Y += yDelta;
                    newTile.tileType = Tile.TileType.Enemy;
                    newTile.tileImage = enemySprite;
                }
            }
        }

        public void move(Map map, Player player)//Exposed move code.
        {
            //MessageBox.Show(precomputedCanReach.ToString());
            if (precomputedCanReach)
            {
                Tile playerTile = Tile.tileListSearch(map.tileList, player.coords);
                Tile nextTile = FindNextTile(map, coords, playerTile.gameCoords, disallowedTypes); //A* tile
                //Tile nextTile = Tile.tileListSearch(map.tileList, GetNextTile(coords, playerTile.gameCoords)); //Straight line tile
                if (nextTile != null)
                {
                    if (nextTile.tileType != Tile.TileType.Player)
                    {
                        moveInternal(map, -(coords.X - nextTile.gameCoords.X), -(coords.Y - nextTile.gameCoords.Y));
                    }
                    else
                    {
                        player.health--;
                        return;
                    }
                }
            }
            else
            {
                wanderMove(map);
            }
        }

        private void wanderMove(Map map)//Run this function if the player is not reachable. Just move randomly.
        {
            Random rand = map.random;
            List<Tile> validTiles = new List<Tile>();

            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    Tile newTile = Tile.tileListSearch(map.tileList, new Point(coords.X + offsetX, coords.Y + offsetY));
                    if (newTile != null && !(disallowedTypes.Contains(newTile.tileType)))
                    {
                        validTiles.Add(newTile);
                    }
                }
            }

            if (validTiles.Count > 0)
            {
                // Choose a random valid tile from the list
                Tile randomTile = validTiles[rand.Next(validTiles.Count)];

                // Move to the randomly selected valid tile
                int offsetX = randomTile.gameCoords.X - coords.X;
                int offsetY = randomTile.gameCoords.Y - coords.Y;
                moveInternal(map, offsetX, offsetY);
            }
        }
    }
}
