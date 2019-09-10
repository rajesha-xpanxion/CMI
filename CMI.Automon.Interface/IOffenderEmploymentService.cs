using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMI.Automon.Interface
{
    public interface IOffenderEmploymentService
    {
        int SaveOffenderEmploymentDetails(string CmiDbConnString, OffenderEmployment offenderEmploymentDetails);
        void DeleteOffenderEmploymentDetails(string CmiDbConnString, OffenderEmployment offenderEmploymentDetails);
        IEnumerable<OffenderEmployment> GetAllOffenderEmployments(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl);
    }
}
