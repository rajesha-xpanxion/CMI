using CMI.Automon.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMI.Automon.Interface
{
    public interface IOffenderNoteService
    {
        IEnumerable<OffenderNote> GetAllOffenderNotes(string CmiDbConnString, DateTime? lastExecutionDateTime, DataTable officerLogonsToFilterTbl);

        int SaveOffenderNoteDetails(string CmiDbConnString, OffenderNote offenderNoteDetails);
    }
}
