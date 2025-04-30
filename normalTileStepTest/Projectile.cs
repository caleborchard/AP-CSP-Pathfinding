using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace normalTileStepTest
{
    class Projectile
    {
        public Form1 form;

        public Point coords;
        public Point velocity;

        public Tile.TileType tileType;
        public Bitmap tileSprite;

        private Tile.TileType previousTile = Tile.TileType.None;
        private Bitmap previousSprite = Properties.Resources.blank;

        private List<Tile.TileType> irreplacableTiles = new List<Tile.TileType>() { Tile.TileType.Player, Tile.TileType.PortalDown, Tile.TileType.PortalUp }; //These tiles can't be drawn over. Hacky fix for the player disappearing when a bullet is spawned in next to a wall or enemy.

        public void move(Map map)//Function runs to move this bullet.
        {
            form.refreshFlood = true;
            Tile currentTile = Tile.tileListSearch(map.tileList, coords);
            Tile nextTile = Tile.tileListSearch(map.tileList, new Point(coords.X + velocity.X, coords.Y + velocity.Y));
            if (nextTile != null && nextTile.tileType != Tile.TileType.PortalDown && nextTile.tileType != Tile.TileType.PortalUp && nextTile.tileType != Tile.TileType.Player)
            {
                if (Tile.collisionHandler(nextTile.tileType) && nextTile.tileType != Tile.TileType.Enemy)
                {
                    if (!(irreplacableTiles.Contains(currentTile.tileType)))
                    {
                        currentTile.tileType = previousTile;
                        currentTile.tileImage = previousSprite;
                        previousTile = nextTile.tileType;
                        previousSprite = nextTile.tileImage;
                    }
                    nextTile.tileType = tileType;
                    nextTile.tileImage = tileSprite;
                    coords = new Point(coords.X + velocity.X, coords.Y + velocity.Y);
                }
                else if (nextTile.tileType == Tile.TileType.Enemy)//If enemy, kill it and remove the bullet.
                {
                    Enemy finalEnemy = null;//Search for enemy from coords.
                    foreach (Enemy e in map.enemyList)
                    {
                        if (e.coords == nextTile.gameCoords)
                        {
                            finalEnemy = e;
                        }
                    }
                    if (finalEnemy != null)//If this search wroked, go on.
                    {
                        map.enemiesToRemove.Add(finalEnemy);

                        nextTile.tileImage = Properties.Resources.blank;
                        nextTile.tileType = Tile.TileType.None;
                        if (!(irreplacableTiles.Contains(currentTile.tileType)))
                        {
                            currentTile.tileType = Tile.TileType.None;
                            currentTile.tileImage = Properties.Resources.blank;
                        }
                        map.projectilesToRemove.Add(this);
                    }
                }
                else//If none of these special cases happen, get rid of yourself and explode.
                {
                    nextTile.tileImage = Properties.Resources.blank;
                    nextTile.tileType = Tile.TileType.None;
                    map.projectilesToRemove.Add(this);
                    if (!(irreplacableTiles.Contains(currentTile.tileType)))
                    {
                        currentTile.tileType = Tile.TileType.None;
                        currentTile.tileImage = Properties.Resources.blank;
                    }
                }
            }
            else
            {
                map.projectilesToRemove.Add(this);
                currentTile.tileType = Tile.TileType.None;
                currentTile.tileImage = Properties.Resources.blank;
            }
        }
    }
}
