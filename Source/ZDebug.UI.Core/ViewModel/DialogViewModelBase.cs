using System.Windows;

namespace ZDebug.UI.ViewModel
{
    public abstract class DialogViewModelBase : ViewModelWithViewBase<Window>
    {
        protected DialogViewModelBase(string viewName)
            : base(viewName)
        {
        }

        protected virtual void OnDialogShown(bool? result)
        {
        }

        public bool? ShowDialog(Window owner = null)
        {
            var view = base.CreateView();
            view.Owner = owner;

            var result = view.ShowDialog();
            OnDialogShown(result);

            return result;
        }
    }
}
