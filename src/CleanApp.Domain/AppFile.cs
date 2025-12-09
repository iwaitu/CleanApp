namespace CleanApp.Domain
{
    public class AppFile : BaseEntity
    {
        public string Id { get; set; } = Ulid.NewUlid().ToString();
        public string FileName { get; set; } = string.Empty;
        public int SizeInBytes { get; set; } = 0;
    }
}
