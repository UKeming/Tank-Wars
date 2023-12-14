// @File: Wall.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty(PropertyName = "wall")] private int ID;

        [JsonProperty(PropertyName = "p1")] private Vector2D point1;

        [JsonProperty(PropertyName = "p2")] private Vector2D point2;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        public Wall(int ID, Vector2D point1, Vector2D point2)
        {
            this.ID = ID;
            this.point1 = point1;
            this.point2 = point2;
        }

        /// <summary>
        ///     Return the start point of the wall.
        /// </summary>
        /// <returns></returns>
        public Vector2D getStartPoint()
        {
            return point1;
        }

        /// <summary>
        ///     Return the end point of the wall.
        /// </summary>
        /// <returns></returns>
        public Vector2D getEndPoint()
        {
            return point2;
        }

        /// <summary>
        ///     Get the ID of the wall.
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return ID;
        }
    }
}