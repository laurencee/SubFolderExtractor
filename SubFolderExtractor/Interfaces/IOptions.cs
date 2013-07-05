namespace SubFolderExtractor.Interfaces
{
    public interface IOptions
    {
        bool RenameToFolder { get; set; }
        bool DeleteAfterExtract { get; set; }
    }
}
