using System;
using System.Windows;
using System.Windows.Controls;
namespace ZDebug.UI.ViewModel
{
    public static class ViewModelWithView
    {
        public static TView Create<TViewModel, TView>()
            where TViewModel : ViewModelWithViewBase<TView>, new()
            where TView : ContentControl
        {
            var viewModel = new TViewModel();
            var uri = new Uri("/ZDebug.UI;component/Views/" + viewModel.ViewName + ".xaml", UriKind.Relative);
            var view = Application.LoadComponent(uri) as TView;
            viewModel.SetView(view);
            viewModel.Initialize();
            return view;
        }
    }
}
