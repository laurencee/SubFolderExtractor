namespace SubFolderExtractor
{
    public class ExtractionStartedEvent
    {
        public ExtractionStartedEvent(bool isExtracting)
        {
            IsExtracting = isExtracting;
        }

        public bool IsExtracting { get; private set; }
    }
}