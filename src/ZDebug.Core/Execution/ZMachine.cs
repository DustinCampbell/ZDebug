using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;
using ZDebug.Core.Text;

namespace ZDebug.Core.Execution
{
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
            this.Story = story;
            this.Memory = story.Memory;
            this.Version = story.Version;
            this.ZText = new ZText(this.Memory);

            this.GlobalVariableTableAddress = Header.ReadGlobalVariableTableAddress(this.Memory);

            this.OutputStreams = new OutputStreamCollection(story);
            RegisterScreen(NullScreen.Instance);
            RegisterSoundEngine(NullSoundEngine.Instance);
            RegisterMessageLog(NullMessageLog.Instance);
        }

        private void SetScreenDimensions()
        {
            if (this.Version >= 4)
            {
                Header.WriteScreenHeightInLines(this.Memory, this.Screen.ScreenHeightInLines);
                Header.WriteScreenWidthInColumns(this.Memory, this.Screen.ScreenWidthInColumns);
            }

            if (this.Version >= 5)
            {
                Header.WriteScreenHeightInUnits(this.Memory, this.Screen.ScreenHeightInUnits);
                Header.WriteScreenWidthInUnits(this.Memory, this.Screen.ScreenWidthInUnits);
                Header.WriteFontHeightInUnits(this.Memory, this.Screen.FontHeightInUnits);
                Header.WriteFontWidthInUnits(this.Memory, this.Screen.FontWidthInUnits);
            }
        }

        public void RegisterScreen(IScreen screen)
        {
            this.Screen = screen ?? NullScreen.Instance;

            SetScreenDimensions();

            if (this.Version >= 5)
            {
                this.Memory.WriteByte(0x2c, (byte)this.Screen.DefaultBackgroundColor);
                this.Memory.WriteByte(0x2d, (byte)this.Screen.DefaultForegroundColor);
            }

            this.OutputStreams.RegisterScreen(this.Screen);
        }

        public void RegisterSoundEngine(ISoundEngine soundEngine)
        {
            this.SoundEngine = soundEngine ?? NullSoundEngine.Instance;
        }

        public void RegisterMessageLog(IMessageLog messageLog)
        {
            this.MessageLog = messageLog ?? NullMessageLog.Instance;
        }

        public void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }

        protected ushort GenerateRandomNumber(ushort minValue, ushort maxValue)
        {
            return (ushort)random.Next(minValue, maxValue);
        }
    }
}
