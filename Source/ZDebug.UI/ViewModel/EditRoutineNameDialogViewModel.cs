using System.ComponentModel.Composition;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class EditRoutineNameDialogViewModel : DialogViewModelBase
    {
        private string name;

        [ImportingConstructor]
        private EditRoutineNameDialogViewModel()
            : base("EditRoutineNameDialogView")
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
