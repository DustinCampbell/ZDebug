using System.Windows.Controls;

namespace ZDebug.UI.ViewModel
{
    internal abstract class ViewModelWithViewBase<T> : ViewModelBase
        where T : ContentControl
    {
        private readonly string viewName;
        private T view;

        protected ViewModelWithViewBase(string viewName)
        {
            this.viewName = viewName;
        }

        internal void SetView(T view)
        {
            this.view = view;
            this.view.DataContext = this;
        }

        protected T View
        {
            get { return view; }
        }

        protected internal virtual void Initialize()
        {
            // do nothing...
        }

        internal string ViewName
        {
            get { return viewName; }
        }
    }
}
