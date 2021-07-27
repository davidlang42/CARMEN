using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class AddableObject
    {
        public string AddButtonText { get; set; }
        public Func<object> CreateObject { get; set; }

        public AddableObject(string add_button_text, Func<object> create_action)
        {
            AddButtonText = add_button_text;
            CreateObject = create_action;
        }
    }
}
