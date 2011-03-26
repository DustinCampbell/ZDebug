using System.Xml.Linq;

namespace ZDebug.UI.Services
{
    public interface IPersistable
    {
        void Load(XElement xml);
        XElement Store();
    }
}
