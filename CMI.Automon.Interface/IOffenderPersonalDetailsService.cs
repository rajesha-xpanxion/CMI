using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.Automon.Interface
{
    public interface IOffenderPersonalDetailsService
    {
        void SaveOffenderPersonalDetails(string CmiDbConnString, Offender offenderDetails);
    }
}
