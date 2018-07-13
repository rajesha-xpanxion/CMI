using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public interface IOffenderService
    {
        IEnumerable<Offender> GetAllOffenderDetails(DateTime lastExecutionDateTime);
    }
}
