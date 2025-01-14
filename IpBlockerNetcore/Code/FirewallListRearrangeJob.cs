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
    public class FirewallListRearrangeJob : IJob
    {
        private readonly ILogger<FirewallListRearrangeJob> _logger;
        private readonly IDbContextFactory<IpBlockerNetcoreContext> _contextFactory;

        public FirewallListRearrangeJob(IDbContextFactory<IpBlockerNetcoreContext> contextFactory, ILogger<FirewallListRearrangeJob> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }
            
        public async Task Execute(IJobExecutionContext context)
        {
            string result = await FirewallListRearrange();
            // Büyük veri işlemi tamamlandıktan sonra
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Console.Out.WriteLineAsync("FirewallListRearrangeJob is executing. Result: " + result);
        }

        public async Task AddToNewFirewallRule(string ip, string durum)
        {
            var _context = await _contextFactory.CreateDbContextAsync();

            var rules = FirewallManager.Instance.Rules.Where(o =>
                o.Direction == FirewallDirection.Inbound &&
                o.Name.StartsWith("BlockAbuseIp")
            );

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

            FirewallManager.Instance.Rules.Add(newRule);
        }

        public async Task AddToFirewall(string ip, string durum = "")
        {
            var _context = await _contextFactory.CreateDbContextAsync();
            
            var rules = FirewallManager.Instance.Rules.Where(o =>
                o.Direction == FirewallDirection.Inbound &&
                o.Name.StartsWith("BlockAbuseIp")
            ).ToList();

            IAddress newIp = SingleIP.Parse(ip);

            bool dahaOnceEklendimi = rules.Any(x => x.RemoteAddresses.Any(addr => addr.ToString() == newIp.ToString()));
            if (!dahaOnceEklendimi)
            {
                try
                {

              
                var uygunKural = rules.FirstOrDefault(x => x.RemoteAddresses.Count() < 1000);
                if (uygunKural != null)
                {
                    var remoteAddresses = uygunKural.RemoteAddresses.ToList();
                    remoteAddresses.Add(newIp);
                    uygunKural.RemoteAddresses = remoteAddresses.ToArray();
                }
                else
                {
                    await AddToNewFirewallRule(ip, durum);
                }
                }
                catch (Exception e )
                {

                    _logger.LogCritical(e.ToString());
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

        public async Task<string> FirewallListRearrange()
        {
            var _context = await _contextFactory.CreateDbContextAsync();
            var rules = FirewallManager.Instance.Rules.Where(o =>
                o.Direction == FirewallDirection.Inbound &&
                o.Name.StartsWith("BlockAbuseIp")
            );

            var allIpList = rules.SelectMany(x => x.RemoteAddresses.Select(y => y.ToString())).Distinct().ToList();
            allIpList = allIpList.Select(Version.Parse).OrderBy(arg => arg).Select(arg => arg.ToString()).ToList();
           var blackListIpAddresses = _context.BlackList.Select(blacklist => blacklist.IpAdresi).ToList();

            foreach (var ip in blackListIpAddresses)
            {
                try
                {

              
                if (!allIpList.Contains(ip))
                {
                    allIpList.Add(ip);
                }
                }
                catch (Exception e)
                {

                    _logger.LogCritical(e.ToString());
                }
            }
            foreach (var rule in rules.ToList())
            {
                try
                {
                    FirewallManager.Instance.Rules.Remove(rule);
                }
                catch (Exception e)
                {

                    _logger.LogCritical(e.ToString());
                }
            }

            int totalCount = allIpList.Count;
            int completedCount = 0;

            foreach (var ip in allIpList)
            {
                await AddToFirewall(ip, "rearrange");

                completedCount++;
                double percentage = (double)completedCount / totalCount * 100;
                _logger.LogInformation(DateTime.Now+Environment.NewLine+ $"Progress: {percentage:F2}% completed.");
            }

            return "complete";
        }
    }

}
