using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using WindowsFirewallHelper;

namespace IpBlockerNetcore.Models.Domain
{
   
    public class  IPAddresi
    {
        [Key]
        public int Id { get; set; }
        [Required, MinLength(4), MaxLength(16)]
#pragma warning disable CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.
        public byte[] IPAddressBytes { get; set; }
#pragma warning restore CS8618 // Null atanamaz alan, oluşturucudan çıkış yaparken null olmayan bir değer içermelidir. Alanı null atanabilir olarak bildirmeyi düşünün.

        [NotMapped]
        public IPAddress IPAdresi
        {
            get { return new IPAddress(IPAddressBytes); }
            set { IPAddressBytes = value.GetAddressBytes(); }
        }
    }
    

}
