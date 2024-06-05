using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace IpBlockerNetcore.Models.Domain
{
    public class BlackList
    {
        public int Id { get; set; }     
#pragma warning disable CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.
        public string IpAdresi { get; set; }
#pragma warning restore CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.
        public int DangerLevel { get; set; }
        public DateTime Date { get; set; }
        public string? DomainName { get; set; }
        public string? Country { get; set; }
    }
   
}
