using CMI.Automon.Model;
using System;
using System.Collections.Generic;

namespace CMI.Automon.Interface
{
    public interface IOffenderNoteService
    {
        IEnumerable<OffenderNote> GetAllOffenderNotes(string CmiDbConnString, DateTime? lastExecutionDateTime);
    }
}
