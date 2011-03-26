using System;
using System.Windows;
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

        protected virtual void ViewCreated(TView view)
        {
            // do nothing...
        }

        private string GetViewUriString()
        {
            return string.Format("/{0};component/Views/{1}.xaml",
                this.GetType().Assembly.GetName().Name,
                viewName);
        }

        public TView CreateView()
        {
            var uri = new Uri(GetViewUriString(), UriKind.Relative);
            this.view = Application.LoadComponent(uri) as TView;
            this.view.DataContext = this;

            ViewCreated(this.view);

            return this.view;
        }
    }
}
