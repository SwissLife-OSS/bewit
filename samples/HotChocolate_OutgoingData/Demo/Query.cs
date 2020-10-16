namespace Demo
{
    public class Query
    {
        public string GetDownloadUrl(string fileId)
        {
            return $"http://foo.bar/id={fileId}";
        }
    }
}
