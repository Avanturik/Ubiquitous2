using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ubiquitous2PluginInstaller
{
    public class PEHeader
    {
        public enum MachineType
        {
            Native = 0, I386 = 0x014c, Itanium = 0x0200, x64 = 0x8664, Error = 0xFFFF
        }

        public static MachineType GetMachineType(string fileName)
        {
            const int PE_POINTER_OFFSET = 60;
            const int MACHINE_OFFSET = 4;
            byte[] data = new byte[4096];
            try
            {
                using (Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    s.Read(data, 0, 4096);
                }
                // dos header is 64 bytes, last element, long (4 bytes) is the address of the PE header
                int PE_HEADER_ADDR = BitConverter.ToInt32(data, PE_POINTER_OFFSET);
                int machineUint = BitConverter.ToUInt16(data, PE_HEADER_ADDR + MACHINE_OFFSET);
                return (MachineType)machineUint;
            }
            catch
            {                
                return MachineType.Error;
            }
        }
    }
}
