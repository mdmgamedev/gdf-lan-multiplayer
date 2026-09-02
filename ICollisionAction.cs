using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDF_example_mplay
{
    public interface ICollisionAction
    {
        bool CollisionAction(instance_player player);
        bool IsSolid();
    }
}
