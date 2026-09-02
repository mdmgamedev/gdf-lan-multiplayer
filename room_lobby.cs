using GDF;
using Urho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho.Network;

namespace GDF_example_mplay
{
    public class room_lobby : RoomTCP
    {
        public DataBase DataBase;
        public instance_player OwnPlayer;

        public room_lobby() : base() { }

        protected override void Start()
        {
            this.DataBase = new DataBase("settings");
            this.DataBase.Load();

            this.Size = Vector3.One * 1000;
            this.Light.Ambient = 0.5f;
            this.Light.Brightness = 1.2f;
            this.RemoveGrid();

            // Create 2D HUD elements: crosshair
            this.Image2dCreate("crosshair", "crosshair.png", 15.95f, 16f / Game.AspectRatio - 0.05f, 0.1f, 0.1f);

            // Create floor instance
            this.InstanceCreate(0, -0.5f, 0, typeof(instance_floor));

            // Host or join LAN server
            if (this.DataBase.GetBool("host"))
            {
                Console.WriteLine("Hosting LAN Server...");
                if (InitNetworking_host(1112))
                {
                    Console.WriteLine("LAN socket bound successfully on port 1112.");
                }
                else
                {
                    Console.WriteLine("Failed to initialize LAN host (port 1112 in use or blocked).");
                }
            }
            else
            {
                string targetIp = this.DataBase.GetString("joinIp");
                Console.WriteLine("Connecting to LAN host at: " + targetIp);

                if (InitNetworking_join(targetIp, 1112))
                {
                    Console.WriteLine("Successfully initiated LAN connection to " + targetIp);
                }
                else
                {
                    Console.WriteLine("Failed to connect to LAN host at " + targetIp);
                }
            }

            // Spawn local player instance
            this.OwnPlayer = (instance_player)InstanceCreate(0, 1f, 0, typeof(instance_player));
            this.OwnPlayer.Name = this.DataBase.GetString("playerName");
            this.Camera.SetCameraMode_FirstPerson(this.OwnPlayer, 0);
        }

        protected override void Step()
        {
            if (GetKeyPress(Key.Esc))
            {
                if (this.DataBase.GetBool("host"))
                {
                    foreach (instance_player player in GetInstancesByType(typeof(instance_player)))
                    {
                        if (player != this.OwnPlayer)
                        {
                            player.SendMessageToMaster(2, 0);
                        }
                    }
                }
                this.CloseNetworking();
                Game.GotoRoom(typeof(room_main));
            }
        }
    }
}