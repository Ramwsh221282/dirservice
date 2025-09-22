namespace DirectoryService.Core.Common.Interfaces;

public interface ISoftDeletable
{
    public bool Deleted { get; set; }

    public void Delete()
    {
        Deleted = true;
    }

    public void Restore()
    {
        Deleted = false;
    }
}
