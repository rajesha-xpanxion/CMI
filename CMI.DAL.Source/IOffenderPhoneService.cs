using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public interface IOffenderPhoneService
    {
        IEnumerable<OffenderPhone> GetAllOffenderPhones(DateTime? lastExecutionDateTime);
    }
}
