using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Applicants
{
    /// <summary>
    /// A textual note attached to an applicant.
    /// </summary>
    public class Note : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int NoteId { get; private set; }

        private Applicant applicant = null!;
        public virtual Applicant Applicant
        {
            get => applicant;
            set
            {
                if (applicant == value)
                    return;
                applicant = value;
                OnPropertyChanged();
            }
        }

        private string text = "";
        public string Text
        {
            get => text;
            set
            {
                if (text == value)
                    return;
                text = value;
                OnPropertyChanged();
            }
        }

        private string author = "";
        public string Author
        {
            get => author;
            set
            {
                if (author == value)
                    return;
                author = value;
                OnPropertyChanged();
            }
        }

        private DateTime timestamp = DateTime.Now;
        public DateTime Timestamp
        {
            get => timestamp;
            set
            {
                if (timestamp == value)
                    return;
                timestamp = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
