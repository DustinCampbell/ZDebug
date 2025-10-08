using NUnit.Framework;
using ZDebug.Core.Extensions;
using ZDebug.Core.Tests.Utilities;

namespace ZDebug.Core.Tests;

[TestFixture]
public class ByteArrayExtensionsTests
{
    #region ReadByte Tests

    [Test, Category(Categories.Utilties)]
    public void ReadByte_WithIndex_ReturnsCorrectByte()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];

        Assert.That(array.ReadByte(0), Is.EqualTo(0x12));
        Assert.That(array.ReadByte(1), Is.EqualTo(0x34));
        Assert.That(array.ReadByte(2), Is.EqualTo(0x56));
        Assert.That(array.ReadByte(3), Is.EqualTo(0x78));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadByte_WithRefIndex_ReturnsCorrectByteAndIncrementsIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        int index = 0;

        Assert.That(array.ReadByte(ref index), Is.EqualTo(0x12));
        Assert.That(index, Is.EqualTo(1));

        Assert.That(array.ReadByte(ref index), Is.EqualTo(0x34));
        Assert.That(index, Is.EqualTo(2));

        Assert.That(array.ReadByte(ref index), Is.EqualTo(0x56));
        Assert.That(index, Is.EqualTo(3));

        Assert.That(array.ReadByte(ref index), Is.EqualTo(0x78));
        Assert.That(index, Is.EqualTo(4));
    }

    #endregion

    #region ReadBytes Tests

    [Test, Category(Categories.Utilties)]
    public void ReadBytes_WithIndex_ReturnsCorrectBytes()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC];
        byte[] result = array.ReadBytes(1, 3);

        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result, Is.EqualTo(new byte[] { 0x34, 0x56, 0x78 }));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadBytes_WithIndexZeroLength_ReturnsEmptyArray()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        byte[] result = array.ReadBytes(0, 0);

        Assert.That(result.Length, Is.EqualTo(0));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadBytes_WithRefIndex_ReturnsCorrectBytesAndIncrementsIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC];
        int index = 1;

        byte[] result = array.ReadBytes(ref index, 3);

        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result, Is.EqualTo(new byte[] { 0x34, 0x56, 0x78 }));
        Assert.That(index, Is.EqualTo(4));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadBytes_WithRefIndexZeroLength_ReturnsEmptyArrayAndDoesNotChangeIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        int index = 2;

        byte[] result = array.ReadBytes(ref index, 0);

        Assert.That(result.Length, Is.EqualTo(0));
        Assert.That(index, Is.EqualTo(2));
    }

    #endregion

    #region ReadWord Tests

    [Test, Category(Categories.Utilties)]
    public void ReadWord_WithIndex_ReadsCorrectBigEndianValue()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];

        Assert.That(array.ReadWord(0), Is.EqualTo(0x1234));
        Assert.That(array.ReadWord(2), Is.EqualTo(0x5678));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadWord_WithRefIndex_ReadsCorrectBigEndianValueAndIncrementsIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        int index = 0;

        Assert.That(array.ReadWord(ref index), Is.EqualTo(0x1234));
        Assert.That(index, Is.EqualTo(2));

        Assert.That(array.ReadWord(ref index), Is.EqualTo(0x5678));
        Assert.That(index, Is.EqualTo(4));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadWord_WithMaxValue_ReturnsCorrectValue()
    {
        byte[] array = [0xFF, 0xFF];

        Assert.That(array.ReadWord(0), Is.EqualTo(0xFFFF));
    }

    #endregion

    #region ReadWords Tests

    [Test, Category(Categories.Utilties)]
    public void ReadWords_WithIndex_ReadsCorrectBigEndianValues()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC];
        ushort[] result = array.ReadWords(0, 3);

        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(0x1234));
        Assert.That(result[1], Is.EqualTo(0x5678));
        Assert.That(result[2], Is.EqualTo(0x9ABC));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadWords_WithIndexZeroLength_ReturnsEmptyArray()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        ushort[] result = array.ReadWords(0, 0);

        Assert.That(result.Length, Is.EqualTo(0));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadWords_WithRefIndex_ReadsCorrectBigEndianValuesAndIncrementsIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC];
        int index = 0;

        ushort[] result = array.ReadWords(ref index, 3);

        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(0x1234));
        Assert.That(result[1], Is.EqualTo(0x5678));
        Assert.That(result[2], Is.EqualTo(0x9ABC));
        Assert.That(index, Is.EqualTo(6));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadWords_WithRefIndexZeroLength_ReturnsEmptyArrayAndDoesNotChangeIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        int index = 2;

        ushort[] result = array.ReadWords(ref index, 0);

        Assert.That(result.Length, Is.EqualTo(0));
        Assert.That(index, Is.EqualTo(2));
    }

    #endregion

    #region ReadDWord Tests

    [Test, Category(Categories.Utilties)]
    public void ReadDWord_WithIndex_ReadsCorrectBigEndianValue()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0];

        Assert.That(array.ReadDWord(0), Is.EqualTo(0x12345678u));
        Assert.That(array.ReadDWord(4), Is.EqualTo(0x9ABCDEF0u));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadDWord_WithRefIndex_ReadsCorrectBigEndianValueAndIncrementsIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0];
        int index = 0;

        Assert.That(array.ReadDWord(ref index), Is.EqualTo(0x12345678u));
        Assert.That(index, Is.EqualTo(4));

        Assert.That(array.ReadDWord(ref index), Is.EqualTo(0x9ABCDEF0u));
        Assert.That(index, Is.EqualTo(8));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadDWord_WithMaxValue_ReturnsCorrectValue()
    {
        byte[] array = [0xFF, 0xFF, 0xFF, 0xFF];

        Assert.That(array.ReadDWord(0), Is.EqualTo(0xFFFFFFFFu));
    }

    #endregion

    #region ReadDWords Tests

    [Test, Category(Categories.Utilties)]
    public void ReadDWords_WithIndex_ReadsCorrectBigEndianValues()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x11, 0x22, 0x33, 0x44];
        uint[] result = array.ReadDWords(0, 3);

        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(0x12345678u));
        Assert.That(result[1], Is.EqualTo(0x9ABCDEF0u));
        Assert.That(result[2], Is.EqualTo(0x11223344u));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadDWords_WithIndexZeroLength_ReturnsEmptyArray()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        uint[] result = array.ReadDWords(0, 0);

        Assert.That(result.Length, Is.EqualTo(0));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadDWords_WithRefIndex_ReadsCorrectBigEndianValuesAndIncrementsIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x11, 0x22, 0x33, 0x44];
        int index = 0;

        uint[] result = array.ReadDWords(ref index, 3);

        Assert.That(result.Length, Is.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(0x12345678u));
        Assert.That(result[1], Is.EqualTo(0x9ABCDEF0u));
        Assert.That(result[2], Is.EqualTo(0x11223344u));
        Assert.That(index, Is.EqualTo(12));
    }

    [Test, Category(Categories.Utilties)]
    public void ReadDWords_WithRefIndexZeroLength_ReturnsEmptyArrayAndDoesNotChangeIndex()
    {
        byte[] array = [0x12, 0x34, 0x56, 0x78];
        int index = 2;

        uint[] result = array.ReadDWords(ref index, 0);

        Assert.That(result.Length, Is.EqualTo(0));
        Assert.That(index, Is.EqualTo(2));
    }

    #endregion

    #region WriteByte Tests

    [Test, Category(Categories.Utilties)]
    public void WriteByte_WithIndex_WritesCorrectByte()
    {
        byte[] array = new byte[4];

        array.WriteByte(0, 0x12);
        array.WriteByte(1, 0x34);
        array.WriteByte(2, 0x56);
        array.WriteByte(3, 0x78);

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78 }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteByte_WithRefIndex_WritesCorrectByteAndIncrementsIndex()
    {
        byte[] array = new byte[4];
        int index = 0;

        array.WriteByte(ref index, 0x12);
        Assert.That(index, Is.EqualTo(1));

        array.WriteByte(ref index, 0x34);
        Assert.That(index, Is.EqualTo(2));

        array.WriteByte(ref index, 0x56);
        Assert.That(index, Is.EqualTo(3));

        array.WriteByte(ref index, 0x78);
        Assert.That(index, Is.EqualTo(4));

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78 }));
    }

    #endregion

    #region WriteBytes Tests

    [Test, Category(Categories.Utilties)]
    public void WriteBytes_WithIndex_WritesCorrectBytes()
    {
        byte[] array = new byte[6];
        byte[] values = [0x34, 0x56, 0x78];

        array.WriteBytes(1, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x00, 0x34, 0x56, 0x78, 0x00, 0x00 }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteBytes_WithRefIndex_WritesCorrectBytesAndIncrementsIndex()
    {
        byte[] array = new byte[6];
        byte[] values = [0x34, 0x56, 0x78];
        int index = 1;

        array.WriteBytes(ref index, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x00, 0x34, 0x56, 0x78, 0x00, 0x00 }));
        Assert.That(index, Is.EqualTo(4));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteBytes_WithEmptyArray_DoesNotChangeTargetOrIndex()
    {
        byte[] array = new byte[4];
        byte[] values = [];
        int index = 2;

        array.WriteBytes(ref index, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00 }));
        Assert.That(index, Is.EqualTo(2));
    }

    #endregion

    #region WriteWord Tests

    [Test, Category(Categories.Utilties)]
    public void WriteWord_WithIndex_WritesBigEndianValue()
    {
        byte[] array = new byte[4];

        array.WriteWord(0, 0x1234);
        array.WriteWord(2, 0x5678);

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78 }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteWord_WithRefIndex_WritesBigEndianValueAndIncrementsIndex()
    {
        byte[] array = new byte[4];
        int index = 0;

        array.WriteWord(ref index, 0x1234);
        Assert.That(index, Is.EqualTo(2));

        array.WriteWord(ref index, 0x5678);
        Assert.That(index, Is.EqualTo(4));

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78 }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteWord_WithMaxValue_WritesCorrectBytes()
    {
        byte[] array = new byte[2];

        array.WriteWord(0, 0xFFFF);

        Assert.That(array, Is.EqualTo(new byte[] { 0xFF, 0xFF }));
    }

    #endregion

    #region WriteWords Tests

    [Test, Category(Categories.Utilties)]
    public void WriteWords_WithIndex_WritesBigEndianValues()
    {
        byte[] array = new byte[6];
        ushort[] values = [0x1234, 0x5678, 0x9ABC];

        array.WriteWords(0, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteWords_WithRefIndex_WritesBigEndianValuesAndIncrementsIndex()
    {
        byte[] array = new byte[6];
        ushort[] values = [0x1234, 0x5678, 0x9ABC];
        int index = 0;

        array.WriteWords(ref index, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC }));
        Assert.That(index, Is.EqualTo(6));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteWords_WithEmptyArray_DoesNotChangeTarget()
    {
        byte[] array = new byte[4];
        ushort[] values = [];

        array.WriteWords(0, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00 }));
    }

    #endregion

    #region WriteDWord Tests

    [Test, Category(Categories.Utilties)]
    public void WriteDWord_WithIndex_WritesBigEndianValue()
    {
        byte[] array = new byte[8];

        array.WriteDWord(0, 0x12345678u);
        array.WriteDWord(4, 0x9ABCDEF0u);

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteDWord_WithRefIndex_WritesBigEndianValueAndIncrementsIndex()
    {
        byte[] array = new byte[8];
        int index = 0;

        array.WriteDWord(ref index, 0x12345678u);
        Assert.That(index, Is.EqualTo(4));

        array.WriteDWord(ref index, 0x9ABCDEF0u);
        Assert.That(index, Is.EqualTo(8));

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteDWord_WithMaxValue_WritesCorrectBytes()
    {
        byte[] array = new byte[4];

        array.WriteDWord(0, 0xFFFFFFFFu);

        Assert.That(array, Is.EqualTo(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }));
    }

    #endregion

    #region WriteDWords Tests

    [Test, Category(Categories.Utilties)]
    public void WriteDWords_WithIndex_WritesBigEndianValues()
    {
        byte[] array = new byte[12];
        uint[] values = [0x12345678u, 0x9ABCDEF0u, 0x11223344u];

        array.WriteDWords(0, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x11, 0x22, 0x33, 0x44 }));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteDWords_WithRefIndex_WritesBigEndianValuesAndIncrementsIndex()
    {
        byte[] array = new byte[12];
        uint[] values = [0x12345678u, 0x9ABCDEF0u, 0x11223344u];
        int index = 0;

        array.WriteDWords(ref index, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x11, 0x22, 0x33, 0x44 }));
        Assert.That(index, Is.EqualTo(12));
    }

    [Test, Category(Categories.Utilties)]
    public void WriteDWords_WithEmptyArray_DoesNotChangeTarget()
    {
        byte[] array = new byte[4];
        uint[] values = [];

        array.WriteDWords(0, values);

        Assert.That(array, Is.EqualTo(new byte[] { 0x00, 0x00, 0x00, 0x00 }));
    }

    #endregion

    #region Round-trip Tests

    [Test, Category(Categories.Utilties)]
    public void RoundTrip_ByteReadWrite_PreservesValue()
    {
        byte[] array = new byte[1];
        byte value = 0xAB;

        array.WriteByte(0, value);
        byte result = array.ReadByte(0);

        Assert.That(result, Is.EqualTo(value));
    }

    [Test, Category(Categories.Utilties)]
    public void RoundTrip_WordReadWrite_PreservesValue()
    {
        byte[] array = new byte[2];
        ushort value = 0xABCD;

        array.WriteWord(0, value);
        ushort result = array.ReadWord(0);

        Assert.That(result, Is.EqualTo(value));
    }

    [Test, Category(Categories.Utilties)]
    public void RoundTrip_DWordReadWrite_PreservesValue()
    {
        byte[] array = new byte[4];
        uint value = 0xABCDEF12u;

        array.WriteDWord(0, value);
        uint result = array.ReadDWord(0);

        Assert.That(result, Is.EqualTo(value));
    }

    [Test, Category(Categories.Utilties)]
    public void RoundTrip_BytesReadWrite_PreservesValues()
    {
        byte[] array = new byte[5];
        byte[] values = [0x12, 0x34, 0x56];

        array.WriteBytes(1, values);
        byte[] result = array.ReadBytes(1, 3);

        Assert.That(result, Is.EqualTo(values));
    }

    [Test, Category(Categories.Utilties)]
    public void RoundTrip_WordsReadWrite_PreservesValues()
    {
        byte[] array = new byte[6];
        ushort[] values = [0x1234, 0x5678, 0x9ABC];

        array.WriteWords(0, values);
        ushort[] result = array.ReadWords(0, 3);

        Assert.That(result, Is.EqualTo(values));
    }

    [Test, Category(Categories.Utilties)]
    public void RoundTrip_DWordsReadWrite_PreservesValues()
    {
        byte[] array = new byte[12];
        uint[] values = [0x12345678u, 0x9ABCDEF0u, 0x11223344u];

        array.WriteDWords(0, values);
        uint[] result = array.ReadDWords(0, 3);

        Assert.That(result, Is.EqualTo(values));
    }

    #endregion
}
