using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace normalTileStepTest
{
    public class Tile
    {
        public enum TileType //Simple enum for all the different tile types
        {
            None,
            Crud,
            Player,
            Gold,
            Bullet,
            Enemy,
            PortalDown,
            PortalUp
        }

        public TileType tileType; //Basic information for each tile; what do you look like, where are you in the grid.
        public Point canvasCoords;
        public Point gameCoords;
        public Bitmap tileImage;

        public static void setupTileList(List<Tile> tileList, int xTileCount, int yTileCount, Size tileSize)
        {
            if (tileList != null)
            {
                tileList.Clear();
            }

            for (int y = 0; y < yTileCount; y++)
            {
                for (int x = 0; x < xTileCount; x++)
                {
                    Tile newTile = new Tile();
                    newTile.gameCoords = new Point(x, y);
                    newTile.canvasCoords = new Point(x * tileSize.Width, y * tileSize.Height);
                    newTile.tileType = TileType.None;
                    newTile.tileImage = Properties.Resources.blank;
                    tileList.Add(newTile);
                }
            }
        }

        //Simple search function for coords in a tile list.
        public static Tile tileListSearch(List<Tile> tileListFunc, Point coords)
        {
            foreach (Tile t in tileListFunc)
            {
                if (t.gameCoords == coords)
                {
                    return t;
                }
            }
            return null;
        }

        //Returns if a certain kind of tile can be walked over
        public static bool collisionHandler(Tile.TileType tileType)
        {
            if (tileType == TileType.None)
            {
                return true;
            }
            else if (tileType == TileType.Crud)
            {
                return false;
            }
            else if (tileType == TileType.Player)
            {
                return false;
            }
            else if (tileType == TileType.Gold)
            {
                return true;
            }
            else if (tileType == TileType.Bullet)
            {
                return false; //This being false makes movement right behind a bullet kind of weird but if you don't do it then you can just walk right on top of the bombs and it freezes them in place. 
            }
            else if (tileType == TileType.Enemy)
            {
                return false;
            }
            else if (tileType == TileType.PortalDown)
            {
                return true;
            }
            else if (tileType == TileType.PortalUp)
            {
                return true;
            }
            else
            {
                return true;
            }
        }
    }
}
