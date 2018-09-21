using CMI.DAL.Source.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMI.DAL.Source
{
    public interface IOffenderNoteService
    {
        IEnumerable<OffenderNote> GetAllOffenderNotes(DateTime? lastExecutionDateTime);
    }
}
