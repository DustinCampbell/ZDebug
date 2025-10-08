using System.Collections.Generic;
using System.Diagnostics;

namespace ZDebug.Compiler.Analysis.ControlFlow;

internal class Block
{
    private readonly bool isEntry;
    private readonly bool isExit;
    private readonly List<Block> jumpSources;
    private readonly List<Block> jumpTargets;

    public Block(bool isEntry = false, bool isExit = false)
    {
        Debug.Assert(!(isEntry && isExit));

        this.isEntry = isEntry;
        this.isExit = isExit;
        jumpSources = [];
        jumpTargets = [];
    }

    public void AddJumpTarget(Block block)
    {
        jumpTargets.Add(block);
        block.jumpSources.Add(this);
    }

    public List<Block> JumpSources => jumpSources;

    public List<Block> JumpTargets => jumpTargets;

    public override string ToString()
    {
        if (isEntry)
        {
            return "Entry block";
        }
        else if (isExit)
        {
            return "Exit block";
        }
        else
        {
            return "Invalid block";
        }
    }
}
