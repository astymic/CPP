using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lb_11.Interfaces
{
    public interface ICustomSerializable: IName
    {
        void Serialize(BinaryWriter writer);
        static object Deserialize(BinaryReader reader) => throw new NotImplementedException(); // Must be genereic
    }
}
