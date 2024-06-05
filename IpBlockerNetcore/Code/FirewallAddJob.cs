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
using Newtonsoft.Json;
using RestSharp;
using Method = Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange.Method;
using System.Security.Policy;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Internal;
using static Azure.Core.HttpHeader;
using System.Linq;

namespace IpBlockerNetcore.Code
{
    [DisallowConcurrentExecution]
    public class FirewallAddJob : IJob
    {
        private readonly IDbContextFactory<IpBlockerNetcoreContext> _contextFactory;
        private readonly ILogger<FirewallAddJob> _logger;

        public FirewallAddJob(IDbContextFactory<IpBlockerNetcoreContext> contextFactory, ILogger<FirewallAddJob> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string result = await GetAndScanScannableIpList();
            await Console.Out.WriteLineAsync("FirewallAddJob is executing. Result: " + result);
        }

        public async Task AddToNewFirewallRule(string ip, string durum)
        {
            var _context = await _contextFactory.CreateDbContextAsync();
            var rules = FirewallManager.Instance.Rules.Where(o => o.Direction == FirewallDirection.Inbound && o.Name.StartsWith("BlockAbuseIp"));
            string ruleName = @"BlockAbuseIp";

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

        public async Task AddToFirewall(string ip, string durum = "")
        {
            try
            {
                var _context = await _contextFactory.CreateDbContextAsync();
                var rules = FirewallManager.Instance.Rules.Where(o => o.Direction == FirewallDirection.Inbound && o.Name.StartsWith("BlockAbuseIp")).ToHashSet();

                if (rules.Any())
                {
                    IAddress newIp = SingleIP.Parse(ip);
                    bool isAlreadyAdded = rules.Any(x => x.RemoteAddresses.Contains(newIp));

                    if (!isAlreadyAdded)
                    {
                        var rule = rules.FirstOrDefault(x => x.RemoteAddresses.Count() < 500);
                        if (rule != null)
                        {
                            var remoteAddresses = rule.RemoteAddresses.ToList();
                            remoteAddresses.Add(newIp);
                            rule.RemoteAddresses = remoteAddresses.ToArray();

                            var banlog = new BanLog
                            {
                                Data = "Son Banlanan İp Adresi: " + ip + " " + durum + " " + rule.Name + "/" + rule.RemoteAddresses.Count(),
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
                        _logger.LogWarning(ip + " nolu ip daha önce eklenmiş");
                    }
                }
                else
                {
                    await AddToNewFirewallRule(ip, durum);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Firewall kuralı eklenirken hata oluştu.");
            }
        }

        public async Task<string> GetAndScanScannableIpList()
        {
            var _context = await _contextFactory.CreateDbContextAsync();
            var scanIps = _context.ScanIp.ToHashSet();
            var whiteListIps = _context.WhiteList.Select(x => x.IpAdresi).ToHashSet();
            var blackListIps = _context.BlackList.Select(x => x.IpAdresi).ToHashSet();

            int sorguSayisi = 0;
            foreach (var scanIp in scanIps)
            {
                try
                {
                    string dataResult = await ScanIpAsync(scanIp.IpAdresi);
                    if (!string.IsNullOrEmpty(dataResult))
                    {
                        var parsedJson = JsonConvert.DeserializeObject<AbuseIpDb>(dataResult);
                        sorguSayisi++;
                        if ((bool)!parsedJson.Data.IsPublic)
                        {
                            if (!whiteListIps.Contains(scanIp.IpAdresi))
                            {
                                await AddToWhiteList(scanIp);
                            }
                        }
                        else
                        {
                            await HandlePublicIp(scanIp, parsedJson, whiteListIps, blackListIps);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogCritical(scanIp.IpAdresi + " nolu ip hatası: " + e);
                }
            }

            return "complete";
        }

        private async Task<string> ScanIpAsync(string ip)
        {
            string[] apiKeys = { "5a703af6bfa645406fbf9e67518d2dbfd1b580bfd1411a3491a9057c3da24c8bf5c7fe2402b9a55c",
                     "953dc7a392ef245ce13b95f67264eebd4137916e96b4f9fcf68415e01343937b02cc0ad54d95c7bc",
                     "eaebf3965af40a01a30c7ebfa5efd1535689233ca24447c633c3d55fc2794e637fa9cfe79af3de6d",
                     "021d3e3d830c350a1bd1348f3b7f841aa879597b17c4b873eaad3760f84a5735ed3059d0eee8bc1b",
                     "aa9c59c3f12516ddd3865861e10d7ae4d801160dafb20ad35574f0ee74067fce66c84fbe3a1cbd79",
                     "cf14ca36b5075c4fa057d64921de4e6d888595f2b597aa52c15b218ffdd7f03d78538dcefc6b47a7",
                     "123e034c9bf55bdacdd0b29f9878690049edf0e68949a2c3082d94332d06f5056d710cb90703b355",
                     "6c3b2477143ffdbdbd803105f9ee2f740a8bac42b3a682d5c78f4c4c893dcdc50ec9b235bcf01696",
                     "faa59936326bb3f00e5867437b9cd482d7190d84854046af1db52f90942a25e5a69727c8e05ed85d",
                     "f44e451e0f0c54ba0503d95e967ffa812a7707caa60ce7a5d6763db07bbc572cafb9ab5407456d53" };
            foreach (var apiKey in apiKeys)
            {
                var options = new RestClientOptions("https://api.abuseipdb.com/api/v2/")
                {
                    Proxy = new WebProxy("socks5://74.119.147.209", 4145),
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };
                var client = new RestClient(options);
                var request = new RestRequest("check");

                request.AddHeader("Accept", "application/json");
                request.AddHeader("Key", apiKey);
                request.AddParameter("ipAddress", ip);
                request.AddParameter("verbose", "");

                var response = await client.ExecuteAsync(request);
                var remainingRateLimit = response.Headers.FirstOrDefault(x => x.Name == "X-RateLimit-Remaining")?.Value;

                if (remainingRateLimit != null && int.Parse(remainingRateLimit.ToString()) > 1)
                {
                    return response.Content;
                }
            }

            return null;
        }

        private async Task AddToWhiteList(ScanIp scanIp)
        {
            var _context = await _contextFactory.CreateDbContextAsync();
            var newWhiteList = new WhiteList
            {
                IpAdresi = scanIp.IpAdresi,
                DangerLevel = 0,
                Date = DateTime.Now
            };

            await _context.WhiteList.AddAsync(newWhiteList);
            _context.ScanIp.Remove(scanIp);
            await _context.SaveChangesAsync();
        }

        private async Task HandlePublicIp(ScanIp scanIp, AbuseIpDb parsedJson, HashSet<string> whiteListIps, HashSet<string> blackListIps)
        {
            var _context = await _contextFactory.CreateDbContextAsync();
            var tehlikeYuzde = parsedJson.Data.AbuseConfidenceScore;

            if (parsedJson.Data.TotalReports > 0)
            {
                if (!blackListIps.Contains(scanIp.IpAdresi))
                {
                    await AddToFirewall(scanIp.IpAdresi, "Tehlike Oranı: %" + tehlikeYuzde);

                    var newBlackList = new BlackList
                    {
                        IpAdresi = scanIp.IpAdresi,
                        DangerLevel = (int)tehlikeYuzde,
                        Date = DateTime.Now,
                        DomainName = parsedJson.Data.Domain,
                        Country = parsedJson.Data.CountryName
                    };

                    await _context.BlackList.AddAsync(newBlackList);
                    _context.ScanIp.Remove(scanIp);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                if (!whiteListIps.Contains(scanIp.IpAdresi))
                {
                    var newWhiteList = new WhiteList
                    {
                        IpAdresi = scanIp.IpAdresi,
                        DangerLevel = 0,
                        Date = DateTime.Now
                    };

                    await _context.WhiteList.AddAsync(newWhiteList);
                    _context.ScanIp.Remove(scanIp);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }

}
