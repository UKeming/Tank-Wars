// @File: Powerup.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        private int currentCD = -1;
        [JsonProperty(PropertyName = "died")] private bool died;

        [JsonProperty(PropertyName = "power")] private int ID;

        [JsonProperty(PropertyName = "loc")] private Vector2D location;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="location"></param>
        public Powerup(int ID, Vector2D location)
        {
            this.ID = ID;
            this.location = location;
            died = false;
        }

        public void DecreaseCD()
        {
            if (currentCD > 0)
                currentCD--;
        }

        public void SetDied(int CD)
        {
            currentCD = CD;
            died = true;
        }


        public bool ReadyForRespawn()
        {
            return currentCD == 0;
        }

        /// <summary>
        ///     Set power up location.
        /// </summary>
        /// <param name="location"></param>
        public void Respawn(Vector2D location)
        {
            died = false;
            this.location = location;
            currentCD = -1;
        }

        /// <summary>
        ///     Get Powerup ID.
        /// </summary>
        /// <returns>Power ID.</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Get location in Vector2D.
        /// </summary>
        /// <returns>Location in Vector2D.</returns>
        public Vector2D GetLocation()
        {
            return location;
        }

        public bool IsDead()
        {
            return died;
        }
    }
}