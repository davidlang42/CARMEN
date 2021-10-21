using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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

        public abstract Task LoadAsync(ShowContext c, CancellationToken cancel);

        protected void StartLoad()
        {
            Status = ProcessStatus.Loading;
            Rows.Clear();
        }

        protected void FinishLoad(CancellationToken cancel, bool incomplete = false)
        {
            if (cancel.IsCancellationRequested)
                return; // leave status as loading if we were cancelled
            if (incomplete)
                Status = ProcessStatus.None;
            else if (Rows.Any(r => r.IsFail))
                Status = ProcessStatus.Error;
            else
                Status = ProcessStatus.Complete;
        }

        /// <summary>Unfortunately some DB providers run async calls sychronously, making this ugly wrapper nessesary.
        /// See: https://stackoverflow.com/questions/29016698/can-the-oracle-managed-driver-use-async-await-properly </summary>
        protected static async Task<T> RealAsync<T>(Func<T> func)
            => await Task.Run(() => func()); //TODO figure out if there are other places in the code which need this outside summaries, maybe I can make my own extension methods which take precendence over the EF core ones? and/or throw an application exception if an async operation is called on the ShowContext. OR can I wrap it in a Task.Run at that level?

        /// <summary>Unfortunately some DB providers run async calls sychronously, making this ugly wrapper nessesary.
        /// See: https://stackoverflow.com/questions/29016698/can-the-oracle-managed-driver-use-async-await-properly </summary>
        protected static async Task RealAsync(Action func)
            => await Task.Run(() => func());
    }
}
