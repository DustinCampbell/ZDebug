using ZDebug.Core.Text;

namespace ZDebug.Core.Objects;

public class ZObject
{
    private readonly ZObjectTable objectTable;
    private readonly ZText ztext;
    private readonly ushort address;
    private readonly ushort number;

    internal ZObject(ZObjectTable objectTable, ZText ztext, ushort address, ushort number)
    {
        this.objectTable = objectTable;
        this.ztext = ztext;
        this.address = address;
        this.number = number;
    }

    public ushort Address => address;

    public ushort Number => number;

    public string ShortName
    {
        get
        {
            var shortNameZWords = PropertyTable.GetShortNameZWords();
            return ztext.ZWordsAsString(shortNameZWords, ZTextFlags.All);
        }
    }

    private ZObject GetObjectByNumber(int number)
    {
        return number > 0
            ? objectTable.GetByNumber(number)
            : null;
    }

    public bool HasParent => objectTable.ReadParentNumberByObjectAddress(address) != 0;

    public ZObject Parent => GetObjectByNumber(objectTable.ReadParentNumberByObjectAddress(address));

    public bool HasSibling => objectTable.ReadSiblingNumberByObjectAddress(address) != 0;

    public ZObject Sibling => GetObjectByNumber(objectTable.ReadSiblingNumberByObjectAddress(address));

    public bool HasChild => objectTable.ReadChildNumberByObjectAddress(address) != 0;

    public ZObject Child => GetObjectByNumber(objectTable.ReadChildNumberByObjectAddress(address));

    public bool HasAttribute(byte attribute) => objectTable.HasAttributeByObjectAddress(address, attribute);

    public void SetAttribute(byte attribute) => objectTable.SetAttributeValueByObjectAddress(address, attribute, true);

    public void ClearAttribute(byte attribute) => objectTable.SetAttributeValueByObjectAddress(address, attribute, false);

    public bool[] GetAllAttributes() => objectTable.GetAllAttributeByObjectAddress(address);

    public ZPropertyTable PropertyTable
    {
        get => objectTable.GetPropertyTable(
              objectTable.ReadPropertyTableAddressByObjectAddress(address));
    }
}
