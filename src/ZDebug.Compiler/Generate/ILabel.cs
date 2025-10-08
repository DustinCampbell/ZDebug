namespace ZDebug.Compiler.Generate;

public interface ILabel
{
    void Mark();

    void Branch(bool @short = false);
    void BranchIf(Condition condition, bool @short = false);
}
