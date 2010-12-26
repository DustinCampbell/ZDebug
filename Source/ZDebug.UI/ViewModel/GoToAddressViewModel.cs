using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;

namespace ZDebug.UI.ViewModel
{
    internal sealed class GoToAddressViewModel : ViewModelWithViewBase<Window>
    {
        private int address;
        private string addressText;

        public GoToAddressViewModel()
            : base("GoToAddressView")
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
