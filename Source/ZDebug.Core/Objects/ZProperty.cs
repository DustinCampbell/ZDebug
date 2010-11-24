using System;

namespace ZDebug.Core.Objects
{
    public struct ZProperty
    {
        private readonly int address;
        private readonly int number;
        private readonly int dataAddress;
        private readonly byte[] data;

        internal ZProperty(int address, int number, int dataAddress, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Array cannot be empty.", "data");
            }

            this.address = address;
            this.number = number;
            this.dataAddress = dataAddress;
            this.data = data;
        }

        public int Address
        {
            get { return address; }
        }

        public int Number
        {
            get { return number; }
        }

        public int DataAddress
        {
            get { return dataAddress; }
        }

        public int DataLength
        {
            get { return data.Length; }
        }

        public bool IsByte
        {
            get { return data.Length == 1; }
        }

        public bool IsWord
        {
            get { return data.Length == 2; }
        }

        public byte AsByte
        {
            get { return data[0]; }
        }

        public ushort AsWord
        {
            get
            {
                var b1 = data[0];
                var b2 = data[1];

                return (ushort)(b1 << 8 | b2);
            }
        }
    }
}
