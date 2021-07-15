using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DatabaseExplorer
{
    class CallbackCommand : ICommand
    {
        public Action<object?>? Callback { get; set; }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => Callback?.Invoke(parameter);
    }
}
