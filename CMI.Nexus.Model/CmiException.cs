using System;
using System.Runtime.Serialization;

namespace CMI.Nexus.Model
{
    [Serializable]
    public class CmiException : Exception
    {
        public CmiException()
            : base()
        {
        }

        public CmiException(string message)
            : base(message)
        {
        }

        protected CmiException(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }
    }
}
