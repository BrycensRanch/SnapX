namespace ShareX.Avalonia;

public class AvaloniaChangelog : CommonUI.Changelog
{
    public AvaloniaChangelog(string version) : base(version)
    {
        Version = version;
    }

    public override void Display()
    {
        throw new NotImplementedException("AvaloniaChangelog.Display is not implemented");
    }
}
