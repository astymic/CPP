using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lb_11.Interfaces
{
    public interface ICustomSerializable
    {
        void Serialize(BinaryWriter writer);
    }
}
