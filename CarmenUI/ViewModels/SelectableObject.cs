using Carmen.ShowModel.Requirements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class SelectableObject<T>
    {
        public ObservableCollection<T> Collection { get; init; }
        public T ObjectValue { get; init; }

        public bool IsSelected
        {
            get => Collection.Contains(ObjectValue);
            set
            {
                if (value)
                {
                    if (!Collection.Contains(ObjectValue))
                        Collection.Add(ObjectValue);
                }
                else
                {
                    if (Collection.Contains(ObjectValue))
                        Collection.Remove(ObjectValue);
                }
            }
        }

        public SelectableObject(ObservableCollection<T> collection, T objectValue)
        {
            this.Collection = collection;
            this.ObjectValue = objectValue;
        }
    }
}
