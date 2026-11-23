using GDF;
using Urho;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDF_start
{
    public class instance_sphere : Instance
    {
        public instance_sphere(uint id) : base(id) { }

        protected override void Start()
        {
            Collider.SetBox();

            Model.SetBox();
            Model.SetColor(Color.Black);
            Model.SetReflection(0.9f);
            Model.Size = Vector3.One * 1.8f;

            FacingDirectionSpeed.Y = 0.1f;
            FacingDirectionSpeed.X = 0.05f;
        }

        protected override void Step()
        {
            Y += (float)Math.Sin(FrameCount / 30.0) * 0.02f;
        }
    }
}
