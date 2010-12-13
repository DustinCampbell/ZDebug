using System;

namespace ZDebug.Core.Execution
{
    public interface IScreen : IOutputStream
    {
        void Clear(int window);
        void ClearAll(bool unsplit = false);

        void Split(int height);
        void Unsplit();

        void SetWindow(int window);

        void SetTextStyle(ZTextStyle style);

        byte ScreenHeightInLines { get; }
        byte ScreenWidthInColumns { get; }
        ushort ScreenHeightInUnits { get; }
        ushort ScreenWidthInUnits { get; }
        byte FontHeightInUnits { get; }
        byte FontWidthInUnits { get; }

        event EventHandler DimensionsChanged;
    }
}
