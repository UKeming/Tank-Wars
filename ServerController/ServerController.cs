// @File: ServerController.cs
// @Created: 2021/04/18
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using Model;
using NetworkUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using TankWars;

namespace ServerController
{
    /// <summary>
    ///     The Controller can control all the data received from server and process it. Change Model by the data processed,
    ///     and notice View to update changes.
    /// </summary>
    public class Controller
    {
        public delegate void MessageHandler(string message);


        // Default game settings (Can not be load from file):
        private readonly int TankSpeed = 3;
        private readonly int MaxBeam = 3;
        private readonly int MaxPowerup = 3;
        private readonly int MaxPowerupDelay = 1650;
        private readonly int ProjectileSpeed = 10;

        // Settings for New Mode
        private readonly int FastShootTime = 500;
        private readonly int MaxBounceTime = 3;
        private readonly double ShotDelayRatio = 0.2;
        private readonly double SpeedUpRatio = 2;
        private readonly int SpeedUpTime = 500;
        private readonly int StartHP = 3;

        // Locks:
        private readonly object tankLock = new object();
        private readonly object clientsLock = new object();
        private readonly object projectileLock = new object();

        // Keep all the connected clients.
        public List<SocketState> clients = new List<SocketState>();

        // The dataList received from clients, seperate them by clients ID.
        public Dictionary<int, Queue<string>> dataList = new Dictionary<int, Queue<string>>();


        // Default game settings (Can be changed by loading settings file):
        private int GameMode;
        private int MSPerFrame = 90;
        private int FramePerShot = 80;
        private int RespawnRate = 300;

        // Keep track of the IDs.
        private int powerupID;
        private int projectileID;
        private int beamID;

        // If the last data received is not end with '\n', then it will be saved here and combine with other data until the next data that is ending with '\n' is received.
        private string remainedData = "";

        // The world that saves all data: Tank, Projectile, Powerups....
        private World world;


        /// <summary>
        ///     Initialize the Controller.
        /// </summary>
        public Controller(MessageHandler handler)
        {
            try
            {
                MessageArrived += handler;
                LoadSettings("settings.xml");
                Networking.StartServer(OnConnection, 11000);
                var updateWorldThread = new Thread(UpdateWorld);
                updateWorldThread.Start();
                MessageArrived("Server is Running, waiting for clients.");
            }
            catch (Exception e)
            {
                MessageArrived("Fail to initalize the server: " + e.Message);
            }
        }

        public event MessageHandler MessageArrived;

        /// <summary>
        ///     Update world.
        /// </summary>
        private void UpdateWorld()
        {
            var watch = new Stopwatch();
            var random = new Random();

            // Generate powerups.
            while (powerupID < MaxPowerup) world.powerups.Add(powerupID, new Powerup(powerupID++, RandomLocation(0)));

            while (true)
            {
                var tobeSend = "";

                // Move tank, detect collision.
                lock (tankLock)
                {
                    foreach (var tank in world.tanks.Values)
                    {
                        if (tank.GetRespawnCD() == -1)
                        {
                            if (CollidingWithWall(tank.PredictMove(), 30) == null && PointInRectangle(tank.PredictMove(),
                                 new Vector2D(0, 0), world.size / 2-30, world.size / 2-30))
                                tank.Move();
                            foreach (var powerup in world.powerups.Values)
                                if (!powerup.IsDead() &&
                                    PointInRectangle(powerup.GetLocation(), tank.GetLocation(), 30, 30))
                                {
                                    if (GameMode == 0)
                                        tank.GetPowerup(0);
                                    else
                                        tank.GetPowerup(random.Next(0, 3));
                                    powerup.SetDied(random.Next(0, MaxPowerupDelay));
                                }

                            if (tank.GetHP() == 0)
                            {
                                tank.SetDied(true);
                                tank.StartRespawn();
                            }
                            tobeSend += JsonConvert.SerializeObject(tank) + "\n";
                            tank.SetDied(false);
                        }
                        else if (tank.GetRespawnCD() == 0) tank.Respawn(RandomLocation(30));
                        tank.DecreaseCD();
                    }
                }

                // Respawn died powerup.
                foreach (var powerup in world.powerups.Values)
                {
                    powerup.DecreaseCD();
                    if (powerup.ReadyForRespawn())
                        powerup.Respawn(RandomLocation(15));
                    tobeSend += JsonConvert.SerializeObject(powerup) + "\n";
                }

                // Move Projectile and detect collision.
                lock (projectileLock)
                {
                    foreach (var projectile in world.projectiles.Values)
                        if (!projectile.IsDead())
                        {
                            projectile.Move();
                            if (!PointInRectangle(projectile.GetLocation(), new Vector2D(0, 0), world.size / 2, world.size / 2))
                                projectile.SetDied();
                            else
                            {
                                var collideWall = CollidingWithWall(projectile.GetLocation(), 0);
                                if (collideWall != null)
                                {
                                    if (GameMode == 0)
                                    {
                                        projectile.SetDied();
                                    }
                                    else if (projectile.bounceTime < MaxBounceTime)
                                    {

                                        double smallX = Math.Min(collideWall.getStartPoint().GetX(), collideWall.getEndPoint().GetX());
                                        double smallY = Math.Min(collideWall.getStartPoint().GetY(), collideWall.getEndPoint().GetY());
                                        double largeX = Math.Max(collideWall.getStartPoint().GetX(), collideWall.getEndPoint().GetX());
                                        double largeY = Math.Max(collideWall.getStartPoint().GetY(), collideWall.getEndPoint().GetY());
                                        if ((Intersects(new Vector2D(smallX - 25, smallY - 25), new Vector2D(1, 0), projectile.GetLocation(), ProjectileSpeed) || Intersects(new Vector2D(smallX - 25, largeY + 25), new Vector2D(1, 0), projectile.GetLocation(), ProjectileSpeed)))
                                            projectile.Bounce(true);
                                        else
                                            projectile.Bounce(false);

                                    }
                                    else
                                    {
                                        projectile.SetDied();
                                    }
                                }

                                lock (tankLock)
                                {
                                    foreach (var tank in world.tanks.Values)
                                        if (tank.GetHP() > 0 && projectile.GetOwner() != tank.GetID() && PointInRectangle(
                                            projectile.GetLocation(), tank.GetLocation(),
                                            30, 30))
                                        {
                                            tank.DecreaseHP();
                                            if (tank.GetHP() == 0)
                                                world.tanks[projectile.GetOwner()].IncreaseScore();
                                            projectile.SetDied();
                                        }
                                }
                            }

                            tobeSend += JsonConvert.SerializeObject(projectile) + "\n";
                        }

                    // Send message to clients.
                    lock (clientsLock)
                    {
                        foreach (var client in clients) Networking.Send(client.TheSocket, tobeSend);
                    }

                    // Wait.
                    watch.Start();
                    while (watch.ElapsedMilliseconds < MSPerFrame) Thread.Sleep(1);
                    watch.Reset();
                }
            }
        }

        /// <summary>
        ///     Do when connection is successfully established.
        /// </summary>
        /// <param name="ss"></param>
        public void OnConnection(SocketState ss)
        {
            if (ss.ErrorOccurred)
            {
                MessageArrived(ss.ErrorMessage);
            }
            else
            {
                // Send world size, ID, and walls to client.

                Networking.Send(ss.TheSocket, ss.ID + "\n" + world.size + "\n");
                foreach (var wall in world.walls.Values)
                    Networking.Send(ss.TheSocket, JsonConvert.SerializeObject(wall) + "\n");

                ss.OnNetworkAction = OnReceiveName;
                Networking.GetData(ss);

                // Continue receiving clients.
                Networking.StartServer(OnConnection, 11000);
            }
        }

        /// <summary>
        ///     Execute when player's name is received.
        /// </summary>
        /// <param name="ss"></param>
        private void OnReceiveName(SocketState ss)
        {
            GetData(ss);
            var name = dataList[(int)ss.ID].Dequeue().Replace("\n", "");
            var tank = new Tank((int)ss.ID, name, TankSpeed, SpeedUpTime, FastShootTime, SpeedUpRatio, ShotDelayRatio,
                StartHP, RandomLocation(30), FramePerShot, RespawnRate, MaxBeam);

            Networking.Send(ss.TheSocket, JsonConvert.SerializeObject(tank) + "\n");
            tank.SetJoined(false);
            lock (tankLock)
            {
                world.tanks[(int)ss.ID] = tank;
            }

            // Add the client to the list where every clients in will be sent data every frame.
            lock (clientsLock)
            {
                clients.Add(ss);
            }

            MessageArrived(name + " has joined the game.");

            ss.OnNetworkAction = OnReceive;
            Networking.GetData(ss);
        }

        /// <summary>
        ///     Execute on receive.
        /// </summary>
        /// <param name="ss"></param>
        private void OnReceive(SocketState ss)
        {
            if (ss.ErrorOccurred)
            {
                MessageArrived(world.tanks[(int)ss.ID].GetName() + " has disconnected.");
                DisConnect(ss);
            }
            else
            {
                GetData(ss);
                while (dataList[(int)ss.ID].Count > 1)
                {
                    TankStatus status;
                    var data = dataList[(int)ss.ID].Dequeue();
                    if (data[data.Length - 1] == '\n')
                        try
                        {
                            var ID = (int)ss.ID;
                            // Parse data.
                            status = JsonConvert.DeserializeObject<TankStatus>(data);

                            // Move.
                            var angle = dirToAngle(status.moving);
                            if (angle.Length() != 0)
                            {
                                world.tanks[ID].SetOrientation(angle);
                                world.tanks[ID].SetMove();
                            }
                            else
                            {
                                world.tanks[ID].SetStop();
                            }

                            world.tanks[ID].SetAiming(status.tdir);

                            // Fire.
                            if (status.fire == "main" && world.tanks[ID].ProjReady())
                                lock (projectileLock)
                                {
                                    world.projectiles[projectileID] = new Projectile(projectileID++, ProjectileSpeed,
                                        world.tanks[ID].GetLocation(), world.tanks[ID].GetAiming(), ID);
                                }

                            if (status.fire == "alt" && world.tanks[ID].BeamShotReady())
                            {
                                var beam = new Beam(beamID++, world.tanks[ID].GetLocation(),
                                    world.tanks[ID].GetAiming(), ID);
                                lock (clientsLock)
                                {
                                    foreach (var client in clients)
                                        Networking.Send(client.TheSocket, JsonConvert.SerializeObject(beam) + "\n");
                                }

                                lock (tankLock)
                                {
                                    foreach (var tank in world.tanks.Values)
                                        if (tank.GetHP() > 0 && Intersects(world.tanks[(int)ss.ID].GetLocation(),
                                            world.tanks[(int)ss.ID].GetAiming(), tank.GetLocation(), 30))
                                        {
                                            world.tanks[(int)ss.ID].IncreaseScore();
                                            tank.SetDied(true);
                                        }
                                }
                            }
                        }
                        catch
                        {
                        }
                }

                Networking.GetData(ss);
            }
        }

        /// <summary>
        ///     Determines if a ray intersects a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        private bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            var a = rayDir.Dot(rayDir);
            var b = ((rayOrig - center) * 2.0).Dot(rayDir);
            var c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            var disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            var root1 = -b + Math.Sqrt(disc);
            var root2 = -b - Math.Sqrt(disc);

            return root1 > 0.0 && root2 > 0.0;
        }

        /// <summary>
        ///     Translate player direction from string to Vector2D.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private Vector2D dirToAngle(string dir)
        {
            switch (dir)
            {
                case "left":
                    return new Vector2D(-1, 0);
                case "up":
                    return new Vector2D(0, -1);
                case "right":
                    return new Vector2D(1, 0);
                case "down":
                    return new Vector2D(0, 1);
            }

            return new Vector2D(0, 0);
        }

        /// <summary>
        ///     Generate a random location that is not colliding with walls for an object in map in Vector2D.
        /// </summary>
        /// <param name="width">The width of the object, used to detect whether collide with walls</param>
        /// <returns></returns>
        private Vector2D RandomLocation(double width)
        {
            var random = new Random();
            while (true)
            {
                var location = new Vector2D(random.Next(-world.size / 2 + 50, world.size / 2 - 50),
                    random.Next(-world.size / 2 + 50, world.size / 2 - 50));
                if (CollidingWithWall(location, width) == null) return location;
            }
        }

        /// <summary>
        ///     Determine whether the given square is colliding with wall.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="hWidth"></param>
        /// <returns>The wall that is colliding with the given square.</returns>
        private Wall CollidingWithWall(Vector2D origin, double hWidth)
        {
            foreach (var wall in world.walls.Values)
            {
                var startPoint = wall.getStartPoint();
                var endPoint = wall.getEndPoint();
                double wallHHeight = Math.Abs(startPoint.GetY() - endPoint.GetY()) / 2;
                double wallHWidth = Math.Abs(startPoint.GetX() - endPoint.GetX()) / 2;
                // Vertical case
                if (startPoint.GetX() - endPoint.GetX() == 0)
                {
                    if (PointInRectangle(origin, new Vector2D(startPoint.GetX(), Math.Min(startPoint.GetY(), endPoint.GetY()) + wallHHeight), hWidth + wallHHeight + 25, 25 + hWidth))
                        return wall;
                }
                // Horizontal case
                else
                {

                    if (PointInRectangle(origin, new Vector2D(Math.Min(startPoint.GetX(), endPoint.GetX()) + wallHWidth, startPoint.GetY()), hWidth + 25, 25 + hWidth + wallHWidth))
                        return wall;
                }
            }
            return null;
        }

        /// <summary>
        ///     Determine if the givenpoint is in the rectangle.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="hWidth"></param>
        /// <param name="recOri"></param>
        /// <param name="recHalfWidth"></param>
        /// <returns></returns>
        private bool PointInRectangle(Vector2D point, Vector2D recOri, double recHalfHeight, double recHalfWidth)
        {
            return (point.GetX() < recOri.GetX() + recHalfWidth) && (point.GetX() > recOri.GetX() - recHalfWidth) && (point.GetY() < recOri.GetY() + recHalfHeight) && (point.GetY() > recOri.GetY() - recHalfHeight);
        }

        /// <summary>
        ///     Disconnect a client and stop sending data to it.
        /// </summary>
        /// <param name="ss"></param>
        private void DisConnect(SocketState ss)
        {
            lock (clientsLock)
            {
                world.tanks[(int)ss.ID].Disconnect();
                clients.Remove(ss);
                foreach (var client in clients)
                    Networking.Send(client.TheSocket, JsonConvert.SerializeObject(world.tanks[(int)ss.ID]) + "\n");
            }

            lock (tankLock)
            {
                world.tanks.Remove((int)ss.ID);
            }
        }

        /// <summary>
        ///     Get data from socket, process it by seperating them by '\n' to list and save to dataList.
        /// </summary>
        /// <param name="ss"></param>
        public void GetData(SocketState ss)
        {
            var data = ss.GetData();
            ss.RemoveData(0, data.Length);
            foreach (var item in Regex.Split(data, @"(?<=[\n])"))
                if (item.Length > 0)
                {
                    if (remainedData != "" && item[item.Length - 1] == '\n')
                    {
                        dataList[(int)ss.ID].Enqueue(remainedData + item);
                        remainedData = "";
                    }
                    else if (item[item.Length - 1] != '\n')
                    {
                        remainedData += item;
                    }
                    else
                    {
                        if (!dataList.ContainsKey((int)ss.ID))
                            dataList[(int)ss.ID] = new Queue<string>();
                        dataList[(int)ss.ID].Enqueue(item);
                    }
                }
        }

        /// <summary>
        ///     Load settings.xml to setup game mode.
        /// </summary>
        /// <param name="pathToFile"></param>
        public void LoadSettings(string pathToFile)
        {
            using (var reader = XmlReader.Create(pathToFile))
            {
                double x = 0;
                double y = 0;
                var startPoint = new Vector2D(0, 0);
                var wallID = 1;
                // Parse the setting file.
                while (reader.Read())
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "UniverseSize":
                                reader.Read();
                                world = new World(int.Parse(reader.Value));
                                break;
                            case "GameMode":
                                reader.Read();
                                GameMode = int.Parse(reader.Value);
                                break;
                            case "MSPerFrame":
                                reader.Read();
                                MSPerFrame = int.Parse(reader.Value);
                                break;
                            case "FramesPerShot":
                                reader.Read();
                                FramePerShot = int.Parse(reader.Value);
                                break;
                            case "RespawnRate":
                                reader.Read();
                                RespawnRate = int.Parse(reader.Value);
                                break;
                            case "x":
                                reader.Read();
                                x = double.Parse(reader.Value);
                                break;
                            case "y":
                                reader.Read();
                                y = double.Parse(reader.Value);
                                break;
                        }
                    }
                    else
                    {
                        if (reader.Name == "p1")
                        {
                            startPoint = new Vector2D(x, y);
                            x = 0;
                            y = 0;
                        }

                        if (reader.Name == "p2")
                        {
                            world.walls[wallID] = new Wall(wallID++, startPoint, new Vector2D(x, y));
                            x = 0;
                            y = 0;
                        }
                    }
            }
        }

        /// <summary>
        ///     The class represent a player's state.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        private class TankStatus
        {
            /// <summary>
            ///     The fire mode, can be alt, main, none.
            /// </summary>
            [JsonProperty] public string fire;

            /// <summary>
            ///     The player's move direction, can be left, right, up, down, none.
            /// </summary>
            [JsonProperty] public string moving;

            /// <summary>
            ///     Target direction, the direction of where the player is shooting.
            /// </summary>
            [JsonProperty] public Vector2D tdir;

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="moveMode"></param>
            /// <param name="fireMode"></param>
            /// <param name="dir"></param>
            public TankStatus(string moveMode, string fireMode, Vector2D dir)
            {
                moving = moveMode;
                fire = fireMode;
                tdir = dir;
            }
        }
    }
}