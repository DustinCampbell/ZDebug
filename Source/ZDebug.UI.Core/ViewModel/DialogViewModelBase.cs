using System.Windows;

namespace ZDebug.UI.ViewModel
{
    public abstract class DialogViewModelBase : ViewModelWithViewBase<Window>
    {
        protected DialogViewModelBase(string viewName)
            : base(viewName)
        {
        }

        public bool? ShowDialog(Window owner = null)
        {
            var view = base.CreateView();
            view.Owner = owner;
            return view.ShowDialog();
        }
    }
}
