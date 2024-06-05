namespace IpBlockerNetcore.Models.Domain
{
    public class ScanIp
    {
        public int Id { get; set; }
#pragma warning disable CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.
        public string IpAdresi { get; set; }
#pragma warning restore CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.
        public DateTime Date { get; set; }
    }
}
