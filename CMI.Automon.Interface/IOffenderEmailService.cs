using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderEmailService
    {
        IEnumerable<OffenderEmail> GetAllOffenderEmails(string CmiDbConnString, DateTime? lastExecutionDateTime);

        void SaveOffenderEmailDetails(string CmiDbConnString, OffenderEmail offenderEmailDetails);
    }
}
