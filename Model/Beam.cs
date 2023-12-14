// @File: Beam.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        [JsonProperty(PropertyName = "beam")] private int ID;

        [JsonProperty(PropertyName = "dir")] private Vector2D orientation;

        [JsonProperty(PropertyName = "org")] private Vector2D origin;

        [JsonProperty(PropertyName = "owner")] private int owner;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="origin"></param>
        /// <param name="orientation"></param>
        /// <param name="owner"></param>
        public Beam(int ID, Vector2D origin, Vector2D orientation, int owner)
        {
            this.ID = ID;
            this.origin = origin;
            this.orientation = orientation;
            this.owner = owner;
        }


        /// <summary>
        ///     Get Beam ID.
        /// </summary>
        /// <returns>Beam ID.</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Get current orientation.
        /// </summary>
        /// <returns>Current orientation in Vector2D.</returns>
        public Vector2D GetOrientation()
        {
            return orientation;
        }

        /// <summary>
        ///     Origin.
        /// </summary>
        /// <returns>Origin in Vector2D.</returns>
        public Vector2D GetOrigin()
        {
            return origin;
        }

        /// <summary>
        ///     Owner.
        /// </summary>
        /// <returns>Return owner.</returns>
        public int GetOwner()
        {
            return owner;
        }
    }
}