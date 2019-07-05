using CMI.Nexus.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Nexus.Interface
{
    public interface ICommonService
    {
        bool UpdateId(string clientId, ReplaceIntegrationIdDetails replaceIntegrationIdDetails);
    }
}
