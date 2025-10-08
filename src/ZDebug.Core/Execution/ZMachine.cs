using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;
using ZDebug.Core.Text;

namespace ZDebug.Core.Execution;

public abstract partial class ZMachine
{
    public readonly byte Version;
    public readonly ushort GlobalVariableTableAddress;

    protected readonly Story Story;
    protected readonly byte[] Memory;
    protected readonly ZText ZText;

    protected readonly OutputStreamCollection OutputStreams;
    protected IScreen Screen;
    protected ISoundEngine SoundEngine;
    protected IMessageLog MessageLog;

    private Random random = new Random();

    protected ZMachine(Story story)
    {
        Story = story;
        Memory = story.Memory;
        Version = story.Version;
        ZText = new ZText(Memory);

        GlobalVariableTableAddress = Header.ReadGlobalVariableTableAddress(Memory);

        OutputStreams = new OutputStreamCollection(story);
        RegisterScreen(NullScreen.Instance);
        RegisterSoundEngine(NullSoundEngine.Instance);
        RegisterMessageLog(NullMessageLog.Instance);
    }

    private void SetScreenDimensions()
    {
        if (Version >= 4)
        {
            Header.WriteScreenHeightInLines(Memory, Screen.ScreenHeightInLines);
            Header.WriteScreenWidthInColumns(Memory, Screen.ScreenWidthInColumns);
        }

        if (Version >= 5)
        {
            Header.WriteScreenHeightInUnits(Memory, Screen.ScreenHeightInUnits);
            Header.WriteScreenWidthInUnits(Memory, Screen.ScreenWidthInUnits);
            Header.WriteFontHeightInUnits(Memory, Screen.FontHeightInUnits);
            Header.WriteFontWidthInUnits(Memory, Screen.FontWidthInUnits);
        }
    }

    public void RegisterScreen(IScreen screen)
    {
        Screen = screen ?? NullScreen.Instance;

        SetScreenDimensions();

        if (Version >= 5)
        {
            Memory.WriteByte(0x2c, (byte)Screen.DefaultBackgroundColor);
            Memory.WriteByte(0x2d, (byte)Screen.DefaultForegroundColor);
        }

        OutputStreams.RegisterScreen(Screen);
    }

    public void RegisterSoundEngine(ISoundEngine soundEngine) => SoundEngine = soundEngine ?? NullSoundEngine.Instance;

    public void RegisterMessageLog(IMessageLog messageLog) => MessageLog = messageLog ?? NullMessageLog.Instance;

    public void SetRandomSeed(int seed) => random = new Random(seed);

    protected ushort GenerateRandomNumber(ushort minValue, ushort maxValue) => (ushort)random.Next(minValue, maxValue);
}
