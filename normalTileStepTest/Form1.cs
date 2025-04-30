using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
//These are all default Windows C# libraries. 

namespace normalTileStepTest
{
    public partial class Form1 : Form
    {
        //Stores dimensions of the game grid in the form of the amount of tiles up and to the side.
        public int xTileCount;
        public int yTileCount;

        //Define random, size of tile to render, and create the player.
        Random random = new Random();
        Size tileSize = new Size(10, 10);
        Player player = new Player();

        //Create list of maps, map index, and mapcount which says how many maps are in the list for the player events since I can't make these definitions public
        List<Map> mapList = new List<Map>();
        public int mapIndex = 0;
        public int mapCount;

        private bool precomputedCanReach;
        public bool refreshFlood = true;

        public Form1()
        {
            InitializeComponent();

            Map newMap = new Map();

            xTileCount = pbRender.Width / tileSize.Width;
            yTileCount = pbRender.Height / tileSize.Height;

            newMap.noise.SetNoiseType(FastNoise.NoiseType.Perlin);
            newMap.noise.SetFrequency((float)0.15);

            Tile.setupTileList(newMap.tileList, xTileCount, yTileCount, tileSize);
            newMap.seed = random.Next(0, 100000);

            mapList.Add(newMap);

            generateMap(newMap.seed, 0, 5);

            player.form = this;
            player.coords = newMap.playerPos;
            Tile playerTile = Tile.tileListSearch(newMap.tileList, player.coords);
            playerTile.tileImage = player.playerSprite;
            playerTile.tileType = Tile.TileType.Player;
        }

        private void pbRender_Paint(object sender, PaintEventArgs e)
        {
            //Go through every tile in this current map and draw them based on the tile size and tile count.
            Map currentMap = mapList[mapIndex];
            Graphics g = e.Graphics;
            for (int y = 0; y < yTileCount; y++)
            {
                for (int x = 0; x < xTileCount; x++)
                {
                    Tile currentTile = Tile.tileListSearch(currentMap.tileList, new Point(x, y));
                    g.DrawImage(currentTile.tileImage, new Rectangle(currentTile.canvasCoords, tileSize));
                }
            }
        }

        private void renderTimer_Tick(object sender, EventArgs e)
        {
            //Tick event, every 1 ms.
            pbRender.Invalidate(); //This forces the picture box to draw stuff again
            mapCount = mapList.Count(); //Set the "mapCount" for the Player functions.

            //Set label elements in the form.
            scoreLabel.Text = "Score: " + player.score.ToString(); 
            levelLabel.Text = "Level " + (mapIndex + 1).ToString();
            ammoLabel.Text = "Ammo: " + player.ammo.ToString();
            healthLabel.Text = "Health: " + player.health.ToString();
        }

        public void gameStep()//When the player movies, this will run. Makes the enemies and bullets move when the player does. Pretty simple.
        {
            if (player.health > 0)
            {
                foreach (Projectile p in mapList[mapIndex].projectilesToRemove)
                {
                    mapList[mapIndex].projectileList.Remove(p);
                }

                if (mapList[mapIndex].projectileList != null)
                {
                    foreach (Projectile p in mapList[mapIndex].projectileList)
                    {
                        p.move(mapList[mapIndex]);
                    }
                }

                foreach (Enemy e in mapList[mapIndex].enemiesToRemove)
                {
                    mapList[mapIndex].enemyList.Remove(e);
                }

                if (mapList[mapIndex].enemyList != null && mapList[mapIndex].enemyList.Count > 0)
                {
                    if (refreshFlood)
                    {
                        precomputedCanReach = Enemy.CanEnemyReachPlayer(mapList[mapIndex], player);
                        refreshFlood = false;
                    }
                    foreach (Enemy e in mapList[mapIndex].enemyList)
                    {
                        e.precomputedCanReach = precomputedCanReach;
                        e.move(mapList[mapIndex], player);
                    }
                }

                if (player.health <= 0)
                {
                    deadSequence();
                }
            }
            else
            {
                deadSequence();
            }
        }

        public void deadSequence()//This function is used when the player dies. Just shows your score and resets everything.
        {
            MessageBox.Show("YOU'RE DEAD! Final score: " + player.score);
            player.health = 3;
            player.score = 0;
            player.ammo = 50;
            player.coords = new Point(0, 0);

            mapList.Clear();
            mapIndex = 0;
            Map newMap = new Map();

            //Set noise configuration
            newMap.noise.SetNoiseType(FastNoise.NoiseType.Perlin);
            newMap.noise.SetFrequency((float)0.15);
            newMap.seed = random.Next(0, 100000);

            Tile.setupTileList(newMap.tileList, xTileCount, yTileCount, tileSize);

            //Add map to list.
            mapList.Add(newMap);

            generateMap(random.Next(0, 100000), 0, 5);
        }

        public void generateMap(int seed, int indexChange, int enemyCount)
        {
            Map currentMap = mapList[mapIndex + indexChange];
            currentMap.noise.SetSeed(seed);
            currentMap.random = new Random();

            foreach (Tile t in currentMap.tileList)
            {
                float noiseVal = currentMap.noise.GetNoise(t.gameCoords.X, t.gameCoords.Y);
                if (noiseVal >= 0.1)
                {
                    t.tileType = Tile.TileType.Crud;
                    t.tileImage = Properties.Resources.crud;
                }
                else
                {
                    int randVar = currentMap.random.Next(0, 100);
                    if (randVar >= 99)
                    {
                        t.tileType = Tile.TileType.Gold;
                        t.tileImage = Properties.Resources.gold;
                    }
                    else
                    {
                        t.tileType = Tile.TileType.None;
                        t.tileImage = Properties.Resources.blank;
                    }
                }
            }

            for(int i = 0; i < enemyCount; i++)
            {
                Enemy enemy = new Enemy();
                enemy.form = this;
                enemy.enemySprite = Properties.Resources.enemy;

                try
                {
                    Point randomPoint;
                    Tile enemyTile;
                    do
                    {
                        int xInt = mapList[mapIndex + indexChange].random.Next(0, xTileCount);
                        int yInt = mapList[mapIndex + indexChange].random.Next(0, yTileCount);
                        randomPoint = new Point(xInt, yInt);
                        enemyTile = Tile.tileListSearch(mapList[mapIndex + indexChange].tileList, randomPoint);
                    }
                    while (enemyTile.tileType != Tile.TileType.None);

                    enemy.coords = enemyTile.gameCoords;
                    mapList[mapIndex + indexChange].enemyList.Add(enemy);

                    enemyTile.tileType = Tile.TileType.Enemy;
                    enemyTile.tileImage = Properties.Resources.enemy;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        //Simplified public map generator function for the Player class when it passes through a portal.
        public void createMapSimple(Bitmap image, Tile.TileType tileType)
        {
            Map newMap = new Map(); //Define map
            Tile.setupTileList(newMap.tileList, xTileCount, yTileCount, tileSize); //Init the tiles
            newMap.noise.SetNoiseType(FastNoise.NoiseType.Perlin); //Setup noise
            newMap.noise.SetFrequency((float)0.15);
            newMap.seed = random.Next(0, 100000);
            mapList.Add(newMap); //Add to map list
            generateMap(newMap.seed, 1, 5); //Generate the objects in grid
            Tile playerTile = Tile.tileListSearch(newMap.tileList, player.coords); //Place player in game
            playerTile.tileImage = player.playerSprite;
            playerTile.tileType = Tile.TileType.Player;
            player.previousTileSprite = image; //Set "previous x" variables for player to custom stuff. Used for putting the up portal.
            player.previousTileType = tileType;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Save this map as a variable so it doesn't call every time.
            Map currentMap = mapList[mapIndex];

            //Detect key movements for input. I am aware that this is awful practise but idk how else to do it.
            if (e.KeyCode == Keys.Q && !(e.Modifiers == Keys.Shift))
            {
                player.move(-1, -1, mapList, xTileCount, yTileCount);
            }
            else if (e.KeyCode == Keys.W && !(e.Modifiers == Keys.Shift))
            {
                player.move(0, -1, mapList, xTileCount, yTileCount);
            }
            else if (e.KeyCode == Keys.E && !(e.Modifiers == Keys.Shift))
            {
                player.move(1, -1, mapList, xTileCount, yTileCount);
            }
            else if (e.KeyCode == Keys.A && !(e.Modifiers == Keys.Shift))
            {
                player.move(-1, 0, mapList, xTileCount, yTileCount);
            }
            else if (e.KeyCode == Keys.S)
            {
                //placeholder
                gameStep();
            }
            else if (e.KeyCode == Keys.D && !(e.Modifiers == Keys.Shift))
            {
                player.move(1, 0, mapList, xTileCount, yTileCount);
            }
            else if (e.KeyCode == Keys.Z && !(e.Modifiers == Keys.Shift))
            {
                player.move(-1, 1, mapList, xTileCount, yTileCount);
            }
            else if (e.KeyCode == Keys.X && !(e.Modifiers == Keys.Shift))
            {
                player.move(0, 1, mapList, xTileCount, yTileCount);
            }
            else if (e.KeyCode == Keys.C && !(e.Modifiers == Keys.Shift))
            {
                player.move(1, 1, mapList, xTileCount, yTileCount);
            }


            
            //Detect key combo for shooting. Also bad coding.
            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Q)
            {
                player.shoot(-1, -1, currentMap, xTileCount, yTileCount);
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.W)
            {
                player.shoot(0, -1, currentMap, xTileCount, yTileCount);
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.E)
            {
                player.shoot(1, -1, currentMap, xTileCount, yTileCount);
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.A)
            {
                player.shoot(-1, 0, currentMap, xTileCount, yTileCount);
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.D)
            {
                player.shoot(1, 0, currentMap, xTileCount, yTileCount);
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Z)
            {
                player.shoot(-1, 1, currentMap, xTileCount, yTileCount);
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.X)
            {
                player.shoot(0, 1, currentMap, xTileCount, yTileCount);
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.C)
            {
                player.shoot(1, 1, currentMap, xTileCount, yTileCount);
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //Leaving this empty, might need to have it in the future.
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
