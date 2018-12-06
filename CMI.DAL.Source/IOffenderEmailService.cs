using System;
using System.Collections.Generic;

namespace CMI.DAL.Source
{
    public interface IOffenderEmailService
    {
        IEnumerable<OffenderEmail> GetAllOffenderEmails(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
