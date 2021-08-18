namespace GridInvestigation.Models
{
    public class Level1
    {
        public string Id { get; set; }
        public Level2 Level2 { get; set; }
    }
    public class Level2
    {
        public string Id { get; set; }
        public Level3 Level3 { get; set; }
    }
    public class Level3
    {
        public string Id { get; set; }
        public Level4 Level4 { get; set; }
    }
    public class Level4
    {
        public string Id { get; set; }
        public Level5 Level5 { get; set; }
    }
    public class Level5
    {
        public string Id { get; set; }
        public Level6 Level6 { get; set; }
    }
    public class Level6
    {
        public string Id { get; set; }
    }
}
