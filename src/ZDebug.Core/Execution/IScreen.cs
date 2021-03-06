﻿namespace ZDebug.Core.Execution
{
    public interface IScreen : IOutputStream, IInputStream
    {
        void Clear(int window);
        void ClearAll(bool unsplit = false);

        void Split(int height);
        void Unsplit();

        void SetWindow(int window);

        int GetCursorLine();
        int GetCursorColumn();
        void SetCursor(int line, int column);
        void SetTextStyle(ZTextStyle style);
        void SetForegroundColor(ZColor color);
        void SetBackgroundColor(ZColor color);
        ZFont SetFont(ZFont font);

        void ShowStatus();

        byte ScreenHeightInLines { get; }
        byte ScreenWidthInColumns { get; }
        ushort ScreenHeightInUnits { get; }
        ushort ScreenWidthInUnits { get; }
        byte FontHeightInUnits { get; }
        byte FontWidthInUnits { get; }

        ZColor DefaultBackgroundColor { get; }
        ZColor DefaultForegroundColor { get; }
    }
}
