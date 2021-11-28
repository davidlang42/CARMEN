using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.ShowModel.Import
{
    class NoteImportColumn : ImportColumn
    {
        public override bool MatchExisting
        {
            get => base.MatchExisting;
            set { }
        }

        public NoteImportColumn(string name, Func<Applicant, ICollection<Note>> collection_getter)
            : base(name, (a, s) => AddNote(s, a, collection_getter), (a, s) => collection_getter(a).Where(n => n.Text == s).Any())
        {
            base.MatchExisting = false;
        }

        private static void AddNote(string note_text, Applicant applicant, Func<Applicant, ICollection<Note>> getter)
        {
            if (string.IsNullOrWhiteSpace(note_text))
                return; // nothing to do
            var notes = getter(applicant);
            notes.Add(new Note
            {
                Applicant = applicant,
                Text = note_text,
                Author = "Importer" //TODO set note author when importing
            });
        }
    }
}
