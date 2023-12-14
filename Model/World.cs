// @File: World.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using System.Collections.Generic;

namespace Model
{
    /// <summary>
    ///     The World contains all items in the game.
    /// </summary>
    public class World
    {
        public Dictionary<int, Beam> beams;
        public Dictionary<int, Powerup> powerups;

        public Dictionary<int, Projectile> projectiles;

        // Size of the world.
        public int size;

        // The objects that the world will persist:
        public Dictionary<int, Tank> tanks;
        public Dictionary<int, Wall> walls;

        /// <summary>
        ///     Initialize the world.
        /// </summary>
        /// <param name="size">Size of the world.</param>
        public World(int size)
        {
            tanks = new Dictionary<int, Tank>();
            walls = new Dictionary<int, Wall>();
            projectiles = new Dictionary<int, Projectile>();
            powerups = new Dictionary<int, Powerup>();
            beams = new Dictionary<int, Beam>();
            this.size = size;
        }
    }
}