using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace normalTileStepTest
{
    class Map
    {
        //Stupid simple class, just stores tile info so you can go to multiple levels and come back to previous ones.
        public List<Tile> tileList = new List<Tile>();

        public List<Projectile> projectileList = new List<Projectile>();
        public List<Projectile> projectilesToRemove = new List<Projectile>();

        public List<Enemy> enemyList = new List<Enemy>();
        public List<Enemy> enemiesToRemove = new List<Enemy>();

        public Random random = new Random();
        public int seed;
        public FastNoise noise = new FastNoise(); //FastNoise is an external library sourced from here: https://github.com/Auburn/FastNoise_CSharp
        public Point playerPos = new Point(0, 0);
    }
}
