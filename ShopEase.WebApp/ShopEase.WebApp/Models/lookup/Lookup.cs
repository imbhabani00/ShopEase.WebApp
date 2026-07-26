namespace ShopEase.WebApp.Models.lookup
{
    public class Lookup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class LookupList
    {
        public List<Lookup> LookupData { get; set; }
        public int ReturnValue { get; set; }
    }
}