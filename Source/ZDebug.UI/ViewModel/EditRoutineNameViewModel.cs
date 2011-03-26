using System.ComponentModel.Composition;
using System.Windows;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class EditRoutineNameViewModel : ViewModelWithViewBase<Window>
    {
        private string name;

        public EditRoutineNameViewModel()
            : base("EditRoutineNameView")
        {
        }

        public bool AcceptableName
        {
            get { return name.Length > 0; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChanged("AcceptableName");
            }
        }
    }
}
