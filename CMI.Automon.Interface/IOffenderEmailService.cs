using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMI.Automon.Interface
{
    public interface IOffenderEmailService
    {
        IEnumerable<OffenderEmail> GetAllOffenderEmails(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl);

        int SaveOffenderEmailDetails(string CmiDbConnString, OffenderEmail offenderEmailDetails);
    }
}
