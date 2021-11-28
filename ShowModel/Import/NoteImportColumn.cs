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

        public NoteImportColumn(string name, Func<Applicant, ICollection<Note>> collection_getter, string current_user_name)
            : base(name, (a, s) => AddNote(s, a, collection_getter, current_user_name), (a, s) => collection_getter(a).Where(n => n.Text == s).Any())
        {
            base.MatchExisting = false;
        }

        private static void AddNote(string note_text, Applicant applicant, Func<Applicant, ICollection<Note>> getter, string current_user_name)
        {
            if (string.IsNullOrWhiteSpace(note_text))
                return; // nothing to do
            var notes = getter(applicant);
            notes.Add(new Note
            {
                Applicant = applicant,
                Text = note_text,
                Author = current_user_name
            });
        }
    }
}
