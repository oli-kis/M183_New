namespace M183.Controllers.Dto
{
    public class NewsDisplayDTO
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string Detail { get; set; }
        public DateTime postedDate { get; set; }
        public bool isAdminNews { get; set; }
        public int authorId { get; set; }
        public string authorUsername { get; set; }
    }
}
