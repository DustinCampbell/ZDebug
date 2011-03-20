using System;
using System.Windows;
using System.Windows.Controls;
namespace ZDebug.UI.ViewModel
{
    public static class ViewModelWithView<TViewModel, TView>
        where TViewModel : ViewModelWithViewBase<TView>, new()
        where TView : ContentControl
    {
        private static string GetUriString(TViewModel viewModel)
        {
            var assemblyName = typeof(TViewModel).Assembly.GetName().Name;
            var viewName = viewModel.ViewName;

            return string.Format("/{0};component/Views/{1}.xaml", assemblyName, viewModel.ViewName);
        }

        public static TView Create()
        {
            var viewModel = new TViewModel();
            var uri = new Uri(GetUriString(viewModel), UriKind.Relative);
            var view = Application.LoadComponent(uri) as TView;
            viewModel.SetView(view);
            viewModel.Initialize();
            return view;
        }
    }
}
