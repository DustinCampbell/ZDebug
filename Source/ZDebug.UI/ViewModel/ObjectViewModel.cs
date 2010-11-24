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
    }
}
