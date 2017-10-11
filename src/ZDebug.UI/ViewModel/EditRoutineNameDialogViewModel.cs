using System.Composition;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class EditRoutineNameDialogViewModel : DialogViewModelBase
    {
        private string name;

        [ImportingConstructor]
        public EditRoutineNameDialogViewModel()
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
