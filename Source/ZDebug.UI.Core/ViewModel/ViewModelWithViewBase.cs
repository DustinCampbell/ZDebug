using System;
using System.Windows.Controls;
using System.Windows.Threading;

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

        protected void Dispatch(Action method, DispatcherPriority priority)
        {
            this.View.Dispatcher.BeginInvoke(method, priority);
        }

        protected void Dispatch(Action method)
        {
            Dispatch(method, DispatcherPriority.Normal);
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
