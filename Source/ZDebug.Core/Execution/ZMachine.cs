using ZDebug.Core.Text;
namespace ZDebug.Core.Execution
{
    public abstract class ZMachine
    {
        protected readonly Story Story;
        protected readonly byte[] Memory;
        protected readonly byte Version;
        protected readonly ZText ZText;

        protected ZMachine(Story story)
        {
            this.Story = story;
            this.Memory = story.Memory;
            this.Version = story.Version;
            this.ZText = new ZText(this.Memory);
        }
    }
}
