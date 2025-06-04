using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRT.Interfaces
{
    public interface ICustomSerializable
    {
        void Serialize(BinaryWriter writer);
        //static object Deserialize(BinaryReader reader) => throw new NotImplementedException();
    }
}
