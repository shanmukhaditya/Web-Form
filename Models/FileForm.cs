namespace demo.Models
{
    public class FileForm
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string fileNames { get; set; }
        public IEnumerable<IFormFile> files { get; set; }

    }


}
