using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace normalTileStepTest
{
    class Player
    {
        //Basic player information
        public Point coords = new Point(0, 0);
        public Bitmap playerSprite = Properties.Resources.character;
        public Bitmap previousTileSprite = Properties.Resources.blank;
        public Tile.TileType previousTileType = Tile.TileType.None;
        public int score = 0;
        public int ammo = 50;
        public int health = 3;

        //Empty Form1 class so when player is defined in Form1 the player will have a reference
        public Form1 form;

        //Move function
        public void move(int xDelta, int yDelta, List<Map> mapList, int xTileCount, int yTileCount)
        {
            //Define tiles for where you are and where you want to go
            Tile currentTile = Tile.tileListSearch(mapList[form.mapIndex].tileList, coords);
            Tile targetTile = Tile.tileListSearch(mapList[form.mapIndex].tileList, new Point(coords.X + xDelta, coords.Y + yDelta)); //Use delta tiles to see which tile to move to. Made it this way for diagonals.

            if (targetTile != null) //See if tile coordinate is actually on known grid.
            {
                if (Tile.collisionHandler(targetTile.tileType) && targetTile.tileType != Tile.TileType.Gold && targetTile.tileType != Tile.TileType.PortalDown && targetTile.tileType != Tile.TileType.PortalUp) //If you can step on this tile and it is not gold, create, or a portal, then move
                {
                    //Make the player move and set previous tile back to normal
                    currentTile.tileImage = previousTileSprite;
                    currentTile.tileType = previousTileType;

                    previousTileSprite = targetTile.tileImage;
                    previousTileType = targetTile.tileType;

                    coords = new Point(coords.X + xDelta, coords.Y + yDelta);
                    targetTile.tileType = Tile.TileType.Player;
                    targetTile.tileImage = playerSprite;

                    form.gameStep();
                }
                else if (targetTile.tileType == Tile.TileType.Gold) //Exception for gold
                {
                    //If gold, increase score and move.
                    score++;
                    Random r = new Random();
                    ammo += r.Next(0, 10);
                    currentTile.tileImage = previousTileSprite;
                    currentTile.tileType = previousTileType;

                    previousTileSprite = Properties.Resources.blank;
                    previousTileType = Tile.TileType.None;

                    coords = new Point(coords.X + xDelta, coords.Y + yDelta);
                    targetTile.tileType = Tile.TileType.Player;
                    targetTile.tileImage = playerSprite;

                    //Detect if any gold is left in the map
                    bool goldFound = true;
                    foreach (Tile t in mapList[form.mapIndex].tileList)
                    {
                        if (t.tileType == Tile.TileType.Gold)
                        {
                            goldFound = false;
                        }
                    }

                    //If there is not, then spawn in a portal in a random clear place on the current map.
                    if (goldFound)
                    {
                        try
                        {
                            Point randomPoint;
                            Tile selectedTile;
                            do
                            {
                                int xInt = mapList[form.mapIndex].random.Next(0, xTileCount);
                                int yInt = mapList[form.mapIndex].random.Next(0, yTileCount);
                                randomPoint = new Point(xInt, yInt);
                                selectedTile = Tile.tileListSearch(mapList[form.mapIndex].tileList, randomPoint);
                            }
                            while (selectedTile.tileType != Tile.TileType.None);

                            selectedTile.tileType = Tile.TileType.PortalDown;
                            selectedTile.tileImage = Properties.Resources.portalDown;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }

                    form.gameStep();
                }
                else if (targetTile.tileType == Tile.TileType.PortalDown) //Exception for portal down
                {
                    if (form.mapIndex + 1 > form.mapCount - 1) //If the next map is not already generated, make one
                    {
                        mapList[form.mapIndex].playerPos = new Point(coords.X + xDelta, coords.Y + yDelta);
                        currentTile.tileImage = previousTileSprite;
                        currentTile.tileType = previousTileType;
                        form.createMapSimple(Properties.Resources.portalUp, Tile.TileType.PortalUp);
                        form.mapIndex++;
                        Tile portalTile = Tile.tileListSearch(mapList[form.mapIndex].tileList, coords);
                        portalTile.tileImage = Properties.Resources.portalUp;
                        portalTile.tileType = Tile.TileType.PortalUp;
                        //coords = mapList[form.mapIndex].playerPos;
                    }
                    else //If next map already exists, just go to it
                    {
                        mapList[form.mapIndex].playerPos = new Point(coords.X + xDelta, coords.Y + yDelta);
                        currentTile.tileImage = previousTileSprite;
                        currentTile.tileType = previousTileType;
                        previousTileSprite = Properties.Resources.portalUp;
                        previousTileType = Tile.TileType.PortalUp;
                        form.mapIndex++;
                        coords = mapList[form.mapIndex].playerPos;
                    }
                }
                else if (targetTile.tileType == Tile.TileType.PortalUp) //Exception for portal up
                {
                    if (form.mapIndex - 1 >= 0) //If there is a level behind you, then go to it. If not, do nothing.
                    {
                        mapList[form.mapIndex].playerPos = new Point(coords.X + xDelta, coords.Y + yDelta);
                        currentTile.tileImage = previousTileSprite;
                        currentTile.tileType = previousTileType;
                        form.mapIndex--;
                        previousTileSprite = Properties.Resources.portalDown;
                        previousTileType = Tile.TileType.PortalDown;
                        coords = mapList[form.mapIndex].playerPos;
                    }
                }
            }
        }

        //Kind of similar to movement but spawns in a bullet and adds to the map list.
        public void shoot(int xDelta, int yDelta, Map map, int xTileCount, int yTileCount)
        {
            if (ammo > 0)
            {
                Tile newTile = Tile.tileListSearch(map.tileList, new Point(coords.X + xDelta, coords.Y + yDelta));
                if (newTile != null && newTile.tileType != Tile.TileType.PortalDown && newTile.tileType != Tile.TileType.PortalUp && newTile.tileType != Tile.TileType.Gold)
                {
                    form.refreshFlood = true;
                    Projectile newProjectile = new Projectile();
                    newProjectile.form = form;
                    newProjectile.coords = coords;
                    newProjectile.velocity = new Point(xDelta, yDelta);
                    newProjectile.tileSprite = Properties.Resources.bomb;
                    newProjectile.tileType = Tile.TileType.Bullet;
                    map.projectileList.Add(newProjectile);
                    form.gameStep();
                    ammo--;
                }
            }
        }
    }
}
