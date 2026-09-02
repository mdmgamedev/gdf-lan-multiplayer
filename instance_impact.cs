using GDF;
using Urho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDF_example_mplay
{
    public class instance_impact : InstanceMultiplay
    {
        public instance_impact(uint id) : base(id)
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
            this.Model.Size = Vector3.One * 0.5f;
            this.Model.SetColor(Color.Yellow);
            this.Model.SetMaterial(Enums.Material.Add);
            this.Model.DrawShadow = false;
        }

        protected override void Step_Master()
        {
            this.Destroy();
        }

        protected override void Step_Slave()
        {
            this.Destroy();
        }
    }
}
