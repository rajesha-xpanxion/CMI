using System;
using System.Collections.Generic;

namespace CMI.DAL.Source
{
    public interface IOffenderNoteService
    {
        IEnumerable<OffenderNote> GetAllOffenderNotes(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
