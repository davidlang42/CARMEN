using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public class Column : IOrdered, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; init;  } = "";
        public string? Format { get; init; }

        private bool show = true;
        public bool Show
        {
            get => show;
            set
            {
                if (show == value)
                    return;
                show = value;
                OnPropertyChanged();
            }
        }

        private int order;
        public int Order
        {
            get => order;
            set
            {
                if (order == value)
                    return;
                order = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Parameterless constructor required for serialization</summary>
        public Column()
        { }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class Column<T> : Column
    {
        public Func<T, object?> ValueGetter { get; }

        public Column(string name, Func<T, object?> getter, string? format = null)
        {
            Name = name;
            ValueGetter = getter;
            Format = format;
        }
    }
}
