using System;

namespace ZDebug.Core.Basics
{
    internal static class ObjectHelpers
    {
        public static int GetPropertyDefaultsCount(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 31;
            }
            else if (version >= 4 && version <= 8)
            {
                return 63;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetPropertyDefaultsTableSize(byte version)
        {
            return GetPropertyDefaultsCount(version) * 2;
        }

        public static int GetMaxObjects(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 255;
            }
            else if (version >= 4 && version <= 8)
            {
                return 65535;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetEntrySize(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 9;
            }
            else if (version >= 4 && version <= 8)
            {
                return 14;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetAttributeBytesSize(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 4;
            }
            else if (version >= 4 && version <= 8)
            {
                return 6;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetAttributeCount(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 32;
            }
            else if (version >= 4 && version <= 8)
            {
                return 48;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetParentOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 4;
            }
            else if (version >= 4 && version <= 8)
            {
                return 6;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetSiblingOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 5;
            }
            else if (version >= 4 && version <= 8)
            {
                return 8;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetChildOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 6;
            }
            else if (version >= 4 && version <= 8)
            {
                return 10;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetPropertyTableAddressOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 7;
            }
            else if (version >= 4 && version <= 8)
            {
                return 12;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetNumberSize(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 1;
            }
            else if (version >= 4 && version <= 8)
            {
                return 2;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }
    }
}
