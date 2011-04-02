using System.Collections.Generic;
using System.Diagnostics;

namespace ZDebug.Compiler.Analysis.ControlFlow
{
    internal class Block
    {
        private readonly bool isEntry;
        private readonly bool isExit;
        private readonly List<Block> jumpTargets;

        public Block(bool isEntry = false, bool isExit = false)
        {
            Debug.Assert(!(isEntry && isExit));

            this.isEntry = isEntry;
            this.isExit = isExit;
            this.jumpTargets = new List<Block>();
        }

        public void AddJumpTarget(Block block)
        {
            this.jumpTargets.Add(block);
        }

        public IEnumerable<Block> JumpTargets
        {
            get
            {
                foreach (var jumpTarget in this.jumpTargets)
                {
                    yield return jumpTarget;
                }
            }
        }

        public override string ToString()
        {
            if (this.isEntry)
            {
                return "Entry block";
            }
            else if (this.isExit)
            {
                return "Exit block";
            }
            else
            {
                return "Invalid block";
            }
        }
    }
}
