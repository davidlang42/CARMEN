using Carmen.ShowModel.Applicants;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Import
{
    public class ImportColumn : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private InputColumn? selectedInput;
        public InputColumn? SelectedInput
        {
            get => selectedInput;
            set
            {
                if (selectedInput == value)
                    return;
                selectedInput = value;
                OnPropertyChanged();
            }
        }

        private bool matchExisting;
        public virtual bool MatchExisting
        {
            get => matchExisting;
            set
            {
                if (matchExisting == value)
                    return;
                matchExisting = value;
                OnPropertyChanged();
            }
        }

        public string Name { get; }
        /// <summary>Should throw ParseException if string value is invalid.</summary>
        public Action<Applicant, string> ValueSetter { get; }
        /// <summary>Should return false if the string value is invalid, or doesn't match the applicant's existing value.</summary>
        public Func<Applicant, string, bool> ValueComparer { get; }

        public ImportColumn(string name, Action<Applicant, string> setter, Func<Applicant, string, bool> comparer)
        {
            Name = name;
            ValueSetter = setter;
            ValueComparer = comparer;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static ImportColumn ForString(string name, Func<Applicant, string> getter, Action<Applicant, string> setter, bool ignore_case = true)
            => new ImportColumn(name, (a, s) => setter(a, s), ignore_case ?
                (a, s) => getter(a).Equals(s, StringComparison.InvariantCultureIgnoreCase) : (a, s) => getter(a).Equals(s));

        public static ImportColumn ForNullable<T>(string name, Func<Applicant, T?> getter, Action<Applicant, T?> setter, Func<string, T?> parse)
        {
            void value_setter(Applicant a, string s) => setter(a, parse(s));
            bool value_comparer(Applicant a, string s)
            {
                T? parsed_value;
                try
                {
                    parsed_value = parse(s);
                }
                catch (ParseException ex)
                {
                    Log.Error(ex, $"{nameof(ImportColumn)}.{nameof(ForNullable)}<{typeof(T).Name}> {nameof(value_comparer)}");
                    return false;
                }
                var existing_value = getter(a);
                if (existing_value == null && parsed_value == null)
                    return true;
                if (existing_value == null || parsed_value == null)
                    return false;
                return parsed_value.Equals(getter(a));
            }
            return new(name, value_setter, value_comparer);
        }

        public static ImportColumn ForConditionalNullable<T>(string name, Func<Applicant, T?> getter, Action<Applicant, T?> setter, Func<Applicant, string, T?> parse)
        {
            void value_setter(Applicant a, string s) => setter(a, parse(a, s));
            bool value_comparer(Applicant a, string s)
            {
                T? parsed_value;
                try
                {
                    parsed_value = parse(a, s);
                }
                catch (ParseException ex)
                {
                    Log.Error(ex, $"{nameof(ImportColumn)}.{nameof(ForConditionalNullable)}<{typeof(T).Name}> {nameof(value_comparer)}");
                    return false;
                }
                var existing_value = getter(a);
                if (existing_value == null && parsed_value == null)
                    return true;
                if (existing_value == null || parsed_value == null)
                    return false;
                return parsed_value.Equals(getter(a));
            }
            return new(name, value_setter, value_comparer);
        }
    }
}
