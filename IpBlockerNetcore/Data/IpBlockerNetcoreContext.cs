using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IpBlockerNetcore.Models.Domain;

namespace IpBlockerNetcore.Data
{
    public class IpBlockerNetcoreContext : DbContext
    {
        public IpBlockerNetcoreContext (DbContextOptions<IpBlockerNetcoreContext> options)
            : base(options)
        {
        }

#pragma warning disable CS0108 // Üye devralınmış üyeyi gizler; yeni anahtar sözcük eksik
        public DbSet<IpBlockerNetcore.Models.Domain.Entry> Entry { get; set; } = default!;
#pragma warning restore CS0108 // Üye devralınmış üyeyi gizler; yeni anahtar sözcük eksik

        public DbSet<IpBlockerNetcore.Models.Domain.BanLog> BanLog { get; set; } = default!;

        public DbSet<IpBlockerNetcore.Models.Domain.WhiteList> WhiteList { get; set; } = default!;

        public DbSet<IpBlockerNetcore.Models.Domain.BlackList> BlackList { get; set; } = default!;

        public DbSet<IpBlockerNetcore.Models.Domain.ScanIp> ScanIp { get; set; } = default!;
    }
}
