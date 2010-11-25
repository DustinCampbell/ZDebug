using ZDebug.Core.Objects;

namespace ZDebug.UI.ViewModel
{
    internal sealed class ObjectViewModel : ViewModelBase
    {
        private readonly ZObject obj;

        public ObjectViewModel(ZObject obj)
        {
            this.obj = obj;
        }

        public int Number
        {
            get { return obj.Number; }
        }

        public int Parent
        {
            get { return obj.HasParent ? obj.Parent.Number : 0; }
        }

        public int Sibling
        {
            get { return obj.HasSibling ? obj.Sibling.Number : 0; }
        }

        public int Child
        {
            get { return obj.HasChild ? obj.Child.Number : 0; }
        }

        public string ShortName
        {
            get { return string.Empty; }
        }
    }
}
