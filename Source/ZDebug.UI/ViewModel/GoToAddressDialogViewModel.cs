using System;
using System.ComponentModel.Composition;
using System.Globalization;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class GoToAddressDialogViewModel : DialogViewModelBase
    {
        private int address;
        private string addressText;

        [ImportingConstructor]
        private GoToAddressDialogViewModel()
            : base("GoToAddressDialogView")
        {
        }

        public bool AcceptableAddress
        {
            get
            {
                return address > 0;
            }
        }

        public int Address
        {
            get { return address; }
        }

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
