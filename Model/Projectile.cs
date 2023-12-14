// @File: Projectile.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        public int bounceTime;
        [JsonProperty(PropertyName = "died")] private bool died;

        [JsonProperty(PropertyName = "proj")] private int ID;

        [JsonProperty(PropertyName = "loc")] private Vector2D location;

        [JsonProperty(PropertyName = "dir")] private Vector2D orientation;

        [JsonProperty(PropertyName = "owner")] private int owner;

        public int speed;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="location"></param>
        /// <param name="orientation"></param>
        /// <param name="owner"></param>
        public Projectile(int ID, int speed, Vector2D location, Vector2D orientation, int owner)
        {
            this.ID = ID;
            this.speed = speed;
            this.location = location;
            this.orientation = orientation;
            this.owner = owner;
            bounceTime = 0;
        }

        /// <summary>
        ///     Move.
        /// </summary>
        public void Move()
        {
            location = location + orientation * speed;
        }

        /// <summary>
        ///     Set died.
        /// </summary>
        public void SetDied()
        {
            died = true;
            bounceTime = 0;
        }

        /// <summary>
        ///     Get projectile ID.
        /// </summary>
        /// <returns>Projectile ID.</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Get Owner.
        /// </summary>
        /// <returns>Owner ID.</returns>
        public int GetOwner()
        {
            return owner;
        }

        /// <summary>
        ///     Get location in Vector2D.
        /// </summary>
        /// <returns>Location in Vector2D.</returns>
        public Vector2D GetLocation()
        {
            return location;
        }

        /// <summary>
        ///     Get orientation.
        /// </summary>
        /// <returns>Orientation in Vector2D.</returns>
        public Vector2D GetOrientation()
        {
            return orientation;
        }

        public void Bounce(bool upDown)
        {
            bounceTime++;
            if (upDown)
                orientation = new Vector2D(orientation.GetX(), orientation.GetY() * -1);
            else
                orientation = new Vector2D(orientation.GetX() * -1, orientation.GetY());
        }

        /// <summary>
        ///     Whether is dead or not.
        /// </summary>
        /// <returns>Bool</returns>
        public bool IsDead()
        {
            return died;
        }
    }
}