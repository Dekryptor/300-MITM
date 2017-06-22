using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MITM
{
    class Packets
    {

        public enum PacketCmd : short
        {
            PasswordWrong = 11238
        }

        public class PacketHeader
        {
            public PacketHeader()
            {
            }

            public PacketHeader(byte[] bytes)
            {
                var reader = new BinaryReader(new MemoryStream(bytes));
                cmd = reader.ReadInt16();
                reader.ReadInt32();
                reader.ReadUInt16();
                var n = reader.ReadUInt16();
                reader.Close();
            }

            public short cmd;
        }
    }

}
