using GDF;
using Urho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDF_example_mplay
{
    public class instance_impactfloor : InstanceMultiplay
    {
        //stel in dat dit voorwerp precies een seconde blijft bestaan
        int framesalive = Game.Settings.MaxFPS;

        public instance_impactfloor(uint id) : base(id)
        {
        }

        //dit voorwerp wordt eenmalig over het netwerk verzonden
        //daarbij hoeft niets ingesteld te worden, want de positie wordt automatisch al meegestuurd
        public override Enums.NetworkSynchronisationMode SynchonisationMode => Enums.NetworkSynchronisationMode.SendOnce;

        public override List<string> SynchronizedFields_continually => new List<string> { };
        public override List<string> SynchronizedFields_creation => new List<string> { };
        public override List<string> SynchronizedFields_destruction => new List<string> { };
        public override List<string> SynchronizedFields_update => new List<string> { };

        public override void NetworkValuesReceived(Enums.NetworkMessageType messageType, Instance buffer)
        {
        }

        protected override void Start()
        {
            this.Model.SetSphere();
            this.Model.Size = new Vector3(0.4f, 0.1f, 0.4f);
            this.Model.SetColor(new Color(0.1f, 0.1f, 0.1f, 0.1f));
            this.Model.SetMaterial(Enums.Material.Multiply);
            this.Model.DrawShadow = false;
        }

        protected override void Step_Master()
        {
            //laat het voorwerp op den duur verdwijnen
            this.framesalive--;

            if (this.framesalive == 0)
            {
                this.Destroy();
            }
        }
        protected override void Step_Slave()
        {
            Step_Master();
        }
    }
}
