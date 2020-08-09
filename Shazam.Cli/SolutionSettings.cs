namespace Shazam.Cli
{
    public class SolutionSettings
    {
        public string SolutionName { get; set; }
        public string PublisherPrefix { get; set; }
        public string PublisherName { get; set; }
        public string SolutionExportDirectory { get; set; }
        public string SolutionPackageType {get; set;}
        public string SolutionPackageMapFilePath{get; set;}
        public string ErrorLevel{get; set;}
        public string SolutionExtractPath{get; set;}
        public string ToolsVersion { get; set; }
        public string CdsSolutionProjectPath{get; set;}
    }
}
