// @File: Tank.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using Newtonsoft.Json;
using TankWars;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tdir")] private Vector2D aiming;

        private int beamNum, speed, projCD, respawnCD, speedUpCD, fastShootCD;

        [JsonProperty(PropertyName = "died")] private bool died;

        [JsonProperty(PropertyName = "dc")] private bool disconnected;

        [JsonProperty(PropertyName = "hp")] private int hitPoints;

        [JsonProperty(PropertyName = "tank")] private int ID;

        [JsonProperty(PropertyName = "join")] private bool joined;

        [JsonProperty(PropertyName = "loc")] private Vector2D location;

        [JsonProperty(PropertyName = "name")] private string name;

        [JsonProperty(PropertyName = "bdir")] private Vector2D orientation;

        [JsonProperty(PropertyName = "score")] private int score;

        private double speedRate, shootRate;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="name"></param>
        /// <param name="hitPoints"></param>
        /// <param name="location"></param>
        /// <param name="projMaxCD"></param>
        /// <param name="respawnMaxCD"></param>
        public Tank(int ID, string name, int speed, int speedUpMaxCD, int fastShootMaxCD, double speedRate,
            double shootRate, int hitPoints, Vector2D location, int projMaxCD, int respawnMaxCD, int maxBeamNum)
        {
            this.ID = ID;
            this.name = name;
            this.speed = speed;
            normalSpeed = speed;
            this.speedRate = speedRate;
            this.shootRate = shootRate;
            this.hitPoints = hitPoints;
            this.location = location;
            score = 0;
            joined = true;
            died = false;
            disconnected = false;
            aiming = new Vector2D(0, -1);
            orientation = new Vector2D(0, -1);
            beamNum = 0;
            this.speedUpMaxCD = speedUpMaxCD;
            this.fastShootMaxCD = fastShootMaxCD;
            this.respawnMaxCD = respawnMaxCD;
            this.projMaxCD = projMaxCD;
            this.maxBeamNum = maxBeamNum;
            respawnCD = -1;
            projCD = 0;
        }

        private int projMaxCD { get; }

        private int speedUpMaxCD { get; }

        private int fastShootMaxCD { get; }

        private int respawnMaxCD { get; }

        private int normalSpeed { get; }

        private int maxBeamNum { get; }

        /// <summary>
        ///     Current tank's score.
        /// </summary>
        public void IncreaseScore()
        {
            score++;
        }

        /// <summary>
        ///     Decrease current tank's hit point.
        /// </summary>
        public void DecreaseHP()
        {
            if (hitPoints > 0)
                hitPoints--;
        }

        /// <summary>
        ///     Invoke if current tank's hit point becomes 0.
        /// </summary>
        /// <param name="died"></param>
        public void SetDied(bool died)
        {
            if (died) hitPoints = 0;
            this.died = died;
        }

        /// <summary>
        ///     Start respawn process.
        /// </summary>
        public void StartRespawn()
        {
            respawnCD = respawnMaxCD;
        }

        /// <summary>
        ///     Decrease cool down time.
        /// </summary>
        public void DecreaseCD()
        {
            if (projCD > 0)
                projCD--;
            if (respawnCD > 0)
                respawnCD--;
            if (speedUpCD > 0)
                speedUpCD--;
            if (fastShootCD > 0)
                fastShootCD--;
        }

        public void Disconnect()
        {
            died = true;
            hitPoints = 0;
            disconnected = true;
        }

        /// <summary>
        ///     Get respawn cool down time.
        /// </summary>
        /// <returns></returns>
        public int GetRespawnCD()
        {
            return respawnCD;
        }

        /// <summary>
        ///     Respawn to given location.
        /// </summary>
        /// <param name="location"></param>
        public void Respawn(Vector2D location)
        {
            this.location = location;
            orientation = new Vector2D(0, -1);
            hitPoints = 3;
            respawnCD = -1;
        }

        /// <summary>
        ///     Whether projectile shot is ready.
        /// </summary>
        /// <returns></returns>
        public bool ProjReady()
        {
            if (projCD == 0)
            {
                if (fastShootCD > 0)
                    projCD = (int) (projMaxCD * shootRate);
                else
                    projCD = projMaxCD;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Whether beam shot is ready.
        /// </summary>
        /// <returns></returns>
        public bool BeamShotReady()
        {
            if (beamNum > 0)
            {
                beamNum--;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Invoke if current tank gets a power up.
        /// </summary>
        public void GetPowerup(int type)
        {
            switch (type)
            {
                case 0:
                    if (beamNum < maxBeamNum)
                        beamNum++;
                    break;
                case 1:
                    fastShootCD = fastShootMaxCD;
                    break;
                case 2:
                    speedUpCD = speedUpMaxCD;
                    break;
            }
        }

        /// <summary>
        ///     Move.
        /// </summary>
        public void Move()
        {
            location = PredictMove();
        }

        /// <summary>
        ///     Move prediction.
        /// </summary>
        /// <returns>The predict value of the location after moving.</returns>
        public Vector2D PredictMove()
        {
            return location + orientation * speed;
        }

        /// <summary>
        ///     Move speed.
        /// </summary>
        public void SetMove()
        {
            if (speedUpCD > 0)
                speed = (int) (normalSpeed * speedRate);
            else
                speed = normalSpeed;
        }

        /// <summary>
        ///     Stop moving.
        /// </summary>
        public void SetStop()
        {
            speed = 0;
        }

        /// <summary>
        ///     Return the location of the Tank.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocation()
        {
            return location;
        }

        /// <summary>
        ///     Set location to given Vector2D point.
        /// </summary>
        /// <param name="location"></param>
        public void SetLocation(Vector2D location)
        {
            this.location = location;
        }

        /// <summary>
        ///     Return the direction of the tank.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetOrientation()
        {
            return orientation;
        }

        /// <summary>
        ///     Set turret orientation.
        /// </summary>
        /// <param name="ori"></param>
        public void SetOrientation(Vector2D ori)
        {
            orientation = ori;
        }

        /// <summary>
        ///     Set player join status.
        /// </summary>
        /// <param name="joined"></param>
        public void SetJoined(bool joined)
        {
            this.joined = joined;
        }

        /// <summary>
        ///     Return the aiming direction.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetAiming()
        {
            return aiming;
        }

        /// <summary>
        ///     Set aiming direction.
        /// </summary>
        /// <param name="aiming"></param>
        public void SetAiming(Vector2D aiming)
        {
            this.aiming = aiming;
        }

        /// <summary>
        ///     Return the ID of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        ///     Return the hit points of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetHP()
        {
            return hitPoints;
        }

        /// <summary>
        ///     Return the name of the tank.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        ///     Return the score of the tank.
        /// </summary>
        /// <returns></returns>
        public int GetScore()
        {
            return score;
        }

        /// <summary>
        ///     Return a bool value indicate whether the tank is dead.
        /// </summary>
        /// <returns></returns>
        public bool IsDead()
        {
            return died;
        }

        /// <summary>
        ///     Return a bool value indicate whether the tank has disconnected.
        ///     The value will only be true in one frame.
        /// </summary>
        /// <returns></returns>
        public bool DisConnected()
        {
            return disconnected;
        }

        /// <summary>
        ///     Return a bool value indicate whether the tank has joined.
        ///     The value will only be true in one frame.
        /// </summary>
        /// <returns></returns>
        public bool Joined()
        {
            return joined;
        }
    }
}