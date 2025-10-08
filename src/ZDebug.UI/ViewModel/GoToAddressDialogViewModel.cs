using System;
using System.Composition;
using System.Globalization;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class GoToAddressDialogViewModel : DialogViewModelBase
    {
        private int address;
        private string addressText;

        [ImportingConstructor]
        public GoToAddressDialogViewModel()
            : base("GoToAddressDialogView")
        {
        }

        public bool AcceptableAddress => address > 0;

        public int Address => address;

        public string AddressText
        {
            get
            {
                return addressText;
            }
            set
            {
                addressText = value;
                if (!Int32.TryParse(addressText, NumberStyles.HexNumber, null, out address))
                {
                    if (!Int32.TryParse(addressText, NumberStyles.Integer, null, out address))
                    {
                        address = 0;
                    }
                }
                PropertyChanged("AcceptableAddress");
            }
        }
    }
}
