namespace IpBlockerNetcore.Models.Domain
{
    public class Response
    {
        public int pageNumber { get; set; }
        public int totalPages { get; set; }
        public int totalEntries { get; set; }
#pragma warning disable CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.
        public List<Entry> entries { get; set; }
#pragma warning restore CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.
    }
}
