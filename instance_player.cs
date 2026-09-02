using GDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace GDF_example_mplay
{
    public class instance_player : InstanceMultiplay
    {
        private Text2d nametag;
        private Image2d hitscreen;
        private int shootcounter = 0;
        private Vector3 flowToPosition, flowToRotation;

        public string Name = "X";
        public string PlayerColor;



        public instance_player(uint id) : base(id)
        {
        }

        public override Enums.NetworkSynchronisationMode SynchonisationMode => Enums.NetworkSynchronisationMode.SendContinually;

        public override List<string> SynchronizedFields_continually => new List<string> { "Position", "FacingDirection" };

        public override List<string> SynchronizedFields_creation => new List<string> { "Name" };

        public override List<string> SynchronizedFields_destruction => new List<string> { };

        public override List<string> SynchronizedFields_update => new List<string> { };

        public override void NetworkValuesReceived(Enums.NetworkMessageType messageType, Instance buffer)
        {
            instance_player sender = (instance_player)buffer;

            if(messageType == Enums.NetworkMessageType.Creation)
            {
                Name = sender.Name;
                Game.CurrentRoom.GetText2d("name" + Id).SetText(Name);
            }
            if (messageType == Enums.NetworkMessageType.Continually)
            {
                this.flowToPosition = sender.Position;
                this.flowToRotation = sender.FacingDirection;
            }
        }

        protected override void Start()
        {
            this.Collider.SetSphere();

            this.nametag = Game.CurrentRoom.Text2dCreate("name" + Id, this.Name, 0, 0, 15, true);
            this.nametag.SetColor(Color.Magenta);
            this.hitscreen = Game.CurrentRoom.Image2dCreate("hit" + Id, "pixel.png", 0, 0, 32, Game.CurrentRoom.UIHeight, Enums.Depth._19_Top);
            this.hitscreen.SetBlendMode(BlendMode.Add);
            this.hitscreen.SetColor(Color.FromByteFormat(0, 0, 0, 128));
        }

        protected override void Step_Master()
        {
            //beweeg het voorwerp met WASD, en zorg ervoor dat het niet door een instance_ball heen kan lopen
            if (GetKey(Key.W))
                this._2D_PositionAdd(this.FacingDirection.Y, 0.5f);
            if (GetKey(Key.S))
                this._2D_PositionAdd(this.FacingDirection.Y + 180, 0.3f);
            if (GetKey(Key.A))
                this._2D_PositionAdd(this.FacingDirection.Y - 90, 0.3f);
            if (GetKey(Key.D))
                this._2D_PositionAdd(this.FacingDirection.Y + 90, 0.3f);

            //pas de kijkrichting aan op de beweging van de muis
            this.FacingDirection.Y += Game.Input.MouseXchange * 2f;
            this.FacingDirection.X += Game.Input.MouseYchange * 2f;

            //zorg ervoor dat de speler niet te ver omhoog of naar beneden kijkt
            if (this.FacingDirection.X > 80 && this.FacingDirection.X < 180)
                this.FacingDirection.X = 80;
            if (this.FacingDirection.X < 270 && this.FacingDirection.X > 180)
                this.FacingDirection.X = 270;

            this.shootcounter++;

            if (this.shootcounter > 5 && Game.Input.MouseButtonLeft)
            {
                this.shootcounter = 0;
                Sounds.Play("laser.ogg");

                //maak een variabele aan die de plek bewaart waar de kogel inslaat
                Vector3 intersection = Vector3.Zero;

                //schiet een lijn in de kijkrichting en geef een voorwerp terug waar die lijn doorheen gaat
                Instance rayInstance = this.DetectNearestRaycastIntersection(this.FacingDirection, typeof(Instance), out intersection, 500);

                //als er een voorwerp is raakgeschoten
                if (rayInstance != null)
                {
                    //speel een impactgeluid, en maak een instance_impact op de plek waar de lijn is ingeslagen
                    Sounds.Play("Slam.wav", intersection, this.Position, 150);
                    Game.CurrentRoom.InstanceCreate(intersection, typeof(instance_impact));

                    //als de vloer geraakt wordt, maak dan een instance_impactfloor op de plek waar de lijn is ingeslagen
                    if (rayInstance.GetType() == typeof(instance_floor))
                    {
                        Game.CurrentRoom.InstanceCreate(intersection, typeof(instance_impactfloor));
                    }

                    if(rayInstance.GetType() == typeof(instance_player))
                    {
                        instance_player player = (instance_player)rayInstance;
                        player.SendMessageToMaster(1, 0);
                    }
                }
            }
        }

        protected override void Step_Slave()
        {
            Position = Methods.FlowPosition(Position, this.flowToPosition, 8);
            FacingDirection = Methods.FlowRotation(FacingDirection, this.flowToRotation, 8);

            this.nametag.Position = Game.CurrentRoom.GetWorldTo2Dposition(
                new Vector3(Position.X, Position.Y + 1.2f, Position.Z));

            room_lobby lobby = (room_lobby)Game.CurrentRoom;
            float distanceToLocalPlayer = Methods._2D_DistanceBetweenPoints(lobby.OwnPlayer.Position, Position);
            this.nametag.SetFont(Methods.Map(distanceToLocalPlayer, 1, 50, 20, 2));

            if(!OwnerIsActive)
            {
                this.Destroy();
            }
        }

        protected override void OnMessageReceived(byte fromNetworkId, byte messageId, object value)
        {
            if(messageId == 1) //enemy hit
            {
                this.hitscreen.SetColor(Color.FromByteFormat(128, 0, 0, 128));
                this.hitscreen.Timers[0].SetTimeout(4, true);
                this.hitscreen.Timers[0].OnTimerElapsed += delegate { this.hitscreen.SetColor(Color.Black); };

                int r = (int)Math.Floor(Randoms.Next(1, 7));
                Sounds.Play("auw" + r + ".ogg");
            }
            if(messageId == 2) //host quit
            {
                Game.CurrentRoomMultiplay.CloseNetworking();
                Game.GotoRoom(typeof(room_main));
            }
        }

        public override void OnDestroy()
        {
            this.nametag.Destroy();
        }
    }
}
