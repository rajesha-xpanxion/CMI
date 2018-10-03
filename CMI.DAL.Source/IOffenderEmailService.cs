using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public interface IOffenderEmailService
    {
        IEnumerable<OffenderEmail> GetAllOffenderEmails(string CMIDBConnString, DateTime? lastExecutionDateTime);
    }
}
