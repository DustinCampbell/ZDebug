using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Objects;

namespace ZDebug.UI.ViewModel
{
    internal sealed class ObjectViewModel : ViewModelBase
    {
        private readonly ZObject obj;
        private readonly ReadOnlyCollection<PropertyViewModel> properties;

        public ObjectViewModel(ZObject obj)
        {
            this.obj = obj;

            var props = new List<PropertyViewModel>();
            foreach (var prop in obj.PropertyTable)
            {
                props.Add(new PropertyViewModel(prop));
            }

            properties = new ReadOnlyCollection<PropertyViewModel>(props);
        }

        public int Number => obj.Number;

        public int Parent => obj.HasParent ? obj.Parent.Number : 0;

        public int Sibling => obj.HasSibling ? obj.Sibling.Number : 0;

        public int Child => obj.HasChild ? obj.Child.Number : 0;

        public string ShortName => obj.ShortName;

        public string Attributes
        {
            get
            {
                var attributes = obj.GetAllAttributes();

                var list = new List<string>();
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i])
                    {
                        list.Add(i.ToString());
                    }
                }

                if (list.Count > 0)
                {
                    return string.Join(", ", list);
                }
                else
                {
                    return "None";
                }
            }
        }

        public int PropertyTableAddress => obj.PropertyTable.Address;

        public ReadOnlyCollection<PropertyViewModel> Properties => properties;
    }
}
