using System.Windows.Controls;

namespace ZDebug.UI.ViewModel
{
    public abstract class ViewModelWithViewBase<TView> : ViewModelBase
        where TView : ContentControl
    {
        private readonly string viewName;
        private TView view;

        protected ViewModelWithViewBase(string viewName)
        {
            this.viewName = viewName;
        }

        internal void SetView(TView view)
        {
            this.view = view;
            this.view.DataContext = this;
        }

        protected TView View
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
