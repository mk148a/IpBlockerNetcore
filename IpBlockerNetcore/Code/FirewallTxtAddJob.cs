using Quartz;
using System.Net.NetworkInformation;
using WindowsFirewallHelper.Addresses;
using WindowsFirewallHelper;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using IpBlockerNetcore.Data;
using System;
using IpBlockerNetcore.Models.Domain;
using System.IO;
using System.Data;
using System.Net;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;

namespace IpBlockerNetcore.Code
{
    [DisallowConcurrentExecution]
    public class FirewallTxtAddJob : IJob
    {
        private readonly IDbContextFactory<IpBlockerNetcoreContext> _contextFactory;
        private readonly ILogger<FirewallTxtAddJob> _logger;

        public FirewallTxtAddJob(IDbContextFactory<IpBlockerNetcoreContext> contextFactory, ILogger<FirewallTxtAddJob> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string result = await GetFirewallTxtIpList();
            // Büyük veri işlemi tamamlandıktan sonra
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Console.Out.WriteLineAsync("FirewallAddTxtJob is executing. Result: " + result);
        }

        public async Task AddToNewFirewallRule(string ip, string durum)
        {
            var _context = await _contextFactory.CreateDbContextAsync();
            var rules = FirewallManager.Instance.Rules.Where(o =>
                o.Direction == FirewallDirection.Inbound && o.Name.StartsWith("BlockAbuseIp"));
            string ruleName = "BlockAbuseIp";

            if (rules.Any())
            {
                ruleName += "-" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToFileTime();
            }

            var newRule = FirewallManager.Instance.CreateApplicationRule(
                FirewallManager.Instance.GetProfile().Type,
                ruleName,
                FirewallAction.Block,
                null
            );

            newRule.Direction = FirewallDirection.Inbound;
            newRule.Action = FirewallAction.Block;
            newRule.Protocol = FirewallProtocol.Any;
            newRule.Scope = FirewallScope.All;
            newRule.Profiles = FirewallProfiles.Public | FirewallProfiles.Private | FirewallProfiles.Domain;
            newRule.RemoteAddresses = new IAddress[] { SingleIP.Parse(ip) };

            var banlog = new BanLog
            {
                Data = "Yeni firewall girdisi -> Son Banlanan İp Adresi: " + ip + " " + durum,
                Date = DateTime.Now
            };

            await _context.BanLog.AddAsync(banlog);
            await _context.SaveChangesAsync();

            FirewallManager.Instance.Rules.Add(newRule);
        }

        public class Benchmark : IDisposable
        {
            private readonly Stopwatch timer = new Stopwatch();
            private readonly string benchmarkName;

            public Benchmark(string benchmarkName)
            {
                this.benchmarkName = benchmarkName;
                timer.Start();
            }

            public void Dispose()
            {
                timer.Stop();
                Console.WriteLine($"{benchmarkName} {timer.Elapsed}");
            }
        }

        public async Task AddToFirewall(string ip, string durum = "")
        {
            try
            {
                var _context = await _contextFactory.CreateDbContextAsync();
                var rules = FirewallManager.Instance.Rules.Where(o =>
                    o.Direction == FirewallDirection.Inbound && o.Name.StartsWith("BlockAbuseIp")).ToList();

                IAddress newIp = SingleIP.Parse(ip);

                bool dahaOnceEklendimi = rules.Any(x => x.RemoteAddresses.Any(addr => addr.ToString() == newIp.ToString()));
                if (!dahaOnceEklendimi)
                {
                    var uygunKural = rules.FirstOrDefault(x => x.RemoteAddresses.Count() < 1000);
                    if (uygunKural != null)
                    {
                        var remoteAddresses = uygunKural.RemoteAddresses.ToList();
                        remoteAddresses.Add(newIp);
                        uygunKural.RemoteAddresses = remoteAddresses.ToArray();

                        var banlog = new BanLog
                        {
                            Data = "Son Txt Eklenen - rearrange içersin İp Adresi: " + ip + " " + durum + " " + uygunKural.Name + "/" + uygunKural.RemoteAddresses.Count(),
                            Date = DateTime.Now
                        };

                        await _context.BanLog.AddAsync(banlog);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        await AddToNewFirewallRule(ip, durum);
                    }
                }
                else
                {
                    var eklenenKurallar = rules.Where(x => x.RemoteAddresses.Any(addr => addr.ToString() == newIp.ToString()));
                    foreach (var eklenenKural in eklenenKurallar)
                    {
                        _logger.LogWarning(ip + " nolu ip daha önce " + eklenenKural.Name + " adlı kural içine eklenmiş");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Firewall kuralı eklenirken hata oluştu.");
            }
        }

        public async Task<string> GetFirewallTxtIpList()
        {
            List<string> ipList = new List<string>();
            try
            {
                ipList = File.ReadAllLines("banipdb.txt").ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "IP adresleri dosyasından okunurken hata oluştu.");
            }

            foreach (var scanip in ipList)
            {
                await AddToFirewall(scanip, "Tehlike Oranı: %" + 999);
            }

            return "complete";
        }
    }

}
