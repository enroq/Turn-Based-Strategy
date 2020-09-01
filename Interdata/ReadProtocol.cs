using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eta.Interdata
{
    public enum ProtocolVersion
    {
        One,
        Two
    }

    public class ReadProtocol
    {
        private static ProtocolVersion m_Version = ProtocolVersion.One;

        public static ProtocolVersion GetVersion()
        {
            return m_Version;
        }
    }
}
