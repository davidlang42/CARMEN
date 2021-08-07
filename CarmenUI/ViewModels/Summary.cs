using ShowModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI.ViewModels
{
    public abstract class Summary : DependencyObject
    {
        public struct Row
        {
            public string? Success { get; set; }
            public string? Fail { get; set; }
            public bool IsFail => !string.IsNullOrEmpty(Fail);
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status), typeof(ProcessStatus), typeof(Summary), new PropertyMetadata(ProcessStatus.Loading));

        public ProcessStatus Status {
            get => (ProcessStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public ObservableCollection<Row> Rows { get; init; } = new();

        public abstract Task LoadAsync(ShowContext c);

        protected void StartLoad()
        {
            Status = ProcessStatus.Loading;
            Rows.Clear();
        }

        protected void FinishLoad(bool complete_if_no_errors)
        {
            if (Rows.Any(r => r.IsFail))
                Status = ProcessStatus.Error;
            else if (complete_if_no_errors)
                Status = ProcessStatus.Complete;
            else
                Status = ProcessStatus.None;
        }
    }
}
