
namespace CMI.DAL.Source.Models
{
    public class SourceConfig
    {
        public bool IsDevMode { get; set; }

        public string AutoMonDbConnString { get; set; }

        public string TestDataJsonRepoPath { get; set; }
    }
}
