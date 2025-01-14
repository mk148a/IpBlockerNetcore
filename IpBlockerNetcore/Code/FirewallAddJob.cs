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
using IpBlockerNetcore.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace IpBlockerNetcore.Code
{
    [DisallowConcurrentExecution]
    public class FirewallAddJob : IJob
    {
        private readonly SemaphoreSlim _semaphoreCheck;
        private readonly IDbContextFactory<IpBlockerNetcoreContext> _contextFactory;
        private readonly ILogger<FirewallAddJob> _logger;
        private int processedIpsCount = 0; // İşlenen IP sayısı
        private DateTime lastCalculationTime = DateTime.UtcNow; // Son hız ölçümü
        private readonly IConfiguration _configuration; // Ekleme
        private List<string> apiKeys = new List<string>(); // Güncellendi


        Dictionary<string, DateTime> overLimitApiKeys = new Dictionary<string, DateTime>();
        public FirewallAddJob(IDbContextFactory<IpBlockerNetcoreContext> contextFactory, ILogger<FirewallAddJob> logger, IConfiguration configuration)
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _semaphoreCheck = new SemaphoreSlim(12);
            LoadApiKeys(); // API anahtarlarını yükle
            _configuration = configuration;
        }
        private void LoadApiKeys()
        {
            try
            {
                // API anahtarlarının bulunduğu dosyanın yolunu al
                // Örneğin, "Config/apikeys.txt" olarak belirlenmiş
                string apiKeysFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "apikeys.txt");

                if (!File.Exists(apiKeysFilePath))
                {
                    _logger.LogError($"API anahtarları dosyası bulunamadı: {apiKeysFilePath}");
                    throw new FileNotFoundException($"API anahtarları dosyası bulunamadı: {apiKeysFilePath}");
                }

                // Dosyayı satır satır oku ve boş olmayan satırları apiKeys listesine ekle
                apiKeys = File.ReadAllLines(apiKeysFilePath)
                              .Where(line => !string.IsNullOrWhiteSpace(line))
                              .ToList();

                if (apiKeys.Count == 0)
                {
                    _logger.LogError("API anahtarları dosyası boş.");
                    throw new InvalidOperationException("API anahtarları dosyası boş.");
                }

                _logger.LogInformation($"{apiKeys.Count} adet API anahtarı yüklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "API anahtarları yüklenirken hata oluştu.");
                throw; // Uygulamayı durdurmak için yeniden fırlat
            }
        }
        public async Task Execute(IJobExecutionContext context)
        {
            Console.Clear();
            string result = await GetAndScanScannableIpList();


            // Büyük veri işlemi tamamlandıktan sonra
            GC.Collect();
            GC.WaitForPendingFinalizers();
            await Console.Out.WriteLineAsync("FirewallAddJob is executing. Result: " + result);
        }

        public async Task AddToNewFirewallRule(string ip, string durum, IpBlockerNetcoreContext context)
        {
          
            var rules = FirewallManager.Instance.Rules.Where(o => 
            o.Direction == FirewallDirection.Inbound && 
            o.Name.StartsWith("BlockAbuseIp"));

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

            await context.BanLog.AddAsync(banlog);
            await context.SaveChangesAsync();

            FirewallManager.Instance.Rules.Add(newRule);
        }

        public async Task AddToFirewall(string ip, string durum, IpBlockerNetcoreContext context)
        {
            try
            {
                var rules = FirewallManager.Instance.Rules.Where(o =>
                    o.Direction == FirewallDirection.Inbound &&
                    o.Name.StartsWith("BlockAbuseIp")
                ).ToList();

                if (rules.Any())
                {
                    IAddress newIp = SingleIP.Parse(ip);
                    bool isAlreadyAdded = rules.Any(x => x.RemoteAddresses.Any(a => a.ToString() == newIp.ToString()));

                    if (!isAlreadyAdded)
                    {
                        var besyuzdenKucukOlanKurallarVarmi = rules.Any(x => x.RemoteAddresses.Count() < 1000);
                        if (besyuzdenKucukOlanKurallarVarmi)
                        {
                            var rule = rules.FirstOrDefault(x => x.RemoteAddresses.Count() < 1000);
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

                                await context.BanLog.AddAsync(banlog);
                                await context.SaveChangesAsync();
                            }
                            else
                            {
                                await AddToNewFirewallRule(ip, durum, context);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning(ip + " nolu ip daha önce eklenmiş");
                    }
                }
                else
                {
                    await AddToNewFirewallRule(ip, durum, context);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Firewall kuralı eklenirken hata oluştu.");
            }
        }
        public async Task<string> GetAndScanScannableIpList()
        {
            // Ana DbContext örneği oluştur
            using var mainContext = await _contextFactory.CreateDbContextAsync();

            // WhiteList'teki IP'leri sil
            var resultDelete = await mainContext.Database.ExecuteSqlRawAsync("EXEC DeleteScanIpByWhiteList");
            await mainContext.SaveChangesAsync();

            var whiteListIps = mainContext.WhiteList.Select(x => x.IpAdresi).ToHashSet();
            var blackListIps = mainContext.BlackList.Select(x => x.IpAdresi).ToHashSet();
            var scanIps = mainContext.ScanIp.ToHashSet();

            int sorguSayisi = 0;
            int apiKeyIndex = 0; // API anahtarı indeksi

            var tasks = new List<Task>();
            Random random = new Random();

            foreach (var scanIp in scanIps)
            {
                await _semaphoreCheck.WaitAsync();

                try
                {
                    // API anahtarını döngüsel olarak kullan
                    string apiKey = apiKeys[apiKeyIndex];
                    apiKeyIndex = (apiKeyIndex + 1) % apiKeys.Count;

                    tasks.Add(Task.Run(async () =>
                    {
                        // Her task için yeni bir DbContext örneği oluştur
                        using var taskContext = await _contextFactory.CreateDbContextAsync();

                        try
                        {
                            string dataResult = await ScanIpAsync(scanIp.IpAdresi, apiKey);
                            
                            if (!string.IsNullOrEmpty(dataResult))
                            {
                                var parsedJson = JsonConvert.DeserializeObject<AbuseIpDb>(dataResult);
                               
                                sorguSayisi++;
                                if (parsedJson == null)
                                {
                                    Console.WriteLine(scanIp.IpAdresi + " ipsi için jsona çevirme null oldu");
                                    taskContext.ScanIp.Remove(scanIp);
                                    await taskContext.SaveChangesAsync();
                                }
                                else
                                {
                                    if ((bool)!parsedJson.Data.IsPublic)
                                    {
                                        if (!whiteListIps.Contains(scanIp.IpAdresi))
                                        {
                                            await AddToWhiteList(scanIp, taskContext);
                                        }
                                    }
                                    else
                                    {
                                        await HandlePublicIp(scanIp, parsedJson, whiteListIps, blackListIps, taskContext);
                                    }
                                }

                              

                                Interlocked.Increment(ref processedIpsCount); // İşlenen IP sayısını thread-safe olarak arttır
                                CalculateProcessingSpeedAndETA(taskContext); // Hızı ve ETA'yı hesapla
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            Console.WriteLine("GetAndScanScannableIpList " + scanIp + " için zaman aşımına uğradı, bir sonraki işleme geçiliyor.");
                            _logger.LogInformation(DateTime.Now + "-" + Environment.NewLine + "GetAndScanScannableIpList işlemi " + scanIp + " için zaman aşımına uğradı, bir sonraki işleme geçiliyor.");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(DateTime.Now + Environment.NewLine + "GetAndScanScannableIpListMain" + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                            _logger.LogError(DateTime.Now + Environment.NewLine + "GetAndScanScannableIpListMain" + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace);
                        }
                        finally
                        {
                            _semaphoreCheck.Release();
                        }
                    }));
                }
                catch (Exception e)
                {
                    _logger.LogCritical(scanIp.IpAdresi + " nolu ip hatası: " + e);
                }
            }

            await Task.WhenAll(tasks);

            return "complete";
        }

        private void CalculateProcessingSpeedAndETA(IpBlockerNetcoreContext context)
        {
            // İşleme hızını her 10 saniyede bir hesapla
            if ((DateTime.UtcNow - lastCalculationTime).TotalSeconds >= 60)
            {
                int remainingIpsCount = context.ScanIp.Count(); // Kalan IP sayısı
                int processingSpeed = (int)(processedIpsCount / (DateTime.UtcNow - lastCalculationTime).TotalMinutes); // Dakikada işlenen IP

                // Hızı sıfırla ve zamanı güncelle
                Interlocked.Exchange(ref processedIpsCount, 0);
                lastCalculationTime = DateTime.UtcNow;

                // Tahmini bitiş süresini hesapla
                double estimatedMinutesRemaining = remainingIpsCount / (double)processingSpeed;
                TimeSpan eta = TimeSpan.FromMinutes(estimatedMinutesRemaining);

                // Gün, saat ve dakika olarak biçimlendirme
                int days = eta.Days;
                int hours = eta.Hours;
                int minutes = eta.Minutes;

                _logger.LogInformation($"Dakikada işlenen IP: {processingSpeed}, Tahmini Bitiş Süresi: {days} gün, {hours} saat, {minutes} dakika.");
                Console.WriteLine(DateTime.Now + "--" + $"Dakikada işlenen IP: {processingSpeed}, Tahmini Bitiş Süresi: {days} gün, {hours} saat, {minutes} dakika." + Environment.NewLine + "Kalan Ip:" + remainingIpsCount + " adet");
            }
        }
        public async Task<string?> ScanIpAsync(string ip, string apiKey)
        {
            string url = "https://api.abuseipdb.com/api/v2/check";

            // Günlük limiti dolmuş anahtarları kontrol et
            if (overLimitApiKeys.ContainsKey(apiKey) && DateTime.UtcNow < overLimitApiKeys[apiKey])
            {
                return null; // Eğer anahtarın resetlenme zamanı geçmediyse, onu atla
            }

            var ipRequestSender = new IpBasedRequestSender("95.217.119.229");
            var response = await ipRequestSender.SendRequestAsync(url, apiKey, ip);

            if (response != null)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(DateTime.Now + "-->" + response.StatusCode + Environment.NewLine + (await response.Content.ReadAsStringAsync()));
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        // Reset zamanını al ve kaydet
                        if (response.Headers.TryGetValues("X-RateLimit-Reset", out var resetValues))
                        {
                            var resetTimestamp = long.Parse(resetValues.First());
                            var resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimestamp).UtcDateTime;
                            overLimitApiKeys[apiKey] = resetTime;
                            _logger.LogWarning($"{apiKey} günlük kotayı aştı ve {resetTime} UTC'ye kadar kullanılamayacak.");
                        }
                        return null;
                    }
                }

                if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var rateLimitValues))
                {
                    var remainingRateLimit = int.Parse(rateLimitValues.First());

                    if (remainingRateLimit > 1)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
            else
            {
                _logger.LogCritical($"{apiKey} api key response null döndü.");
            }

            return null;
        }

        private async Task AddToWhiteList(ScanIp scanIp, IpBlockerNetcoreContext context)
        {
            var newWhiteList = new WhiteList
            {
                IpAdresi = scanIp.IpAdresi,
                DangerLevel = 0,
                Date = DateTime.Now
            };

            await context.WhiteList.AddAsync(newWhiteList);
            context.ScanIp.Remove(scanIp);
            await context.SaveChangesAsync();
        }
        public async Task<bool> IsWhitelistedDomain(string domainName)
        {
            string[] whitelistedDomains = {
                "google.com", "bing.com", "yahoo.com", "yandex.com", "baidu.com",
                "facebook.com", "pinterest.com", "instagram.com", "twitter.com"
            };
            if (domainName != null)
            {
                return whitelistedDomains.Any(domain => domainName.Contains(domain));
            }
            else return false;
        }
        private async Task HandlePublicIp(ScanIp scanIp, AbuseIpDb parsedJson, HashSet<string> whiteListIps, HashSet<string> blackListIps, IpBlockerNetcoreContext context)
        {
            try
            {
                var tehlikeYuzde = parsedJson.Data.AbuseConfidenceScore;
                Console.WriteLine(scanIp.IpAdresi + " için tehlike yüzde :" + tehlikeYuzde);
                if (parsedJson.Data.TotalReports > 0)
                {
                    if (await IsWhitelistedDomain(parsedJson.Data.Domain))
                    {
                        _logger.LogInformation($"Skipped adding {parsedJson.Data.Domain} to firewall rules as it's identified as a whitelisted domain.");
                        if (!whiteListIps.Contains(scanIp.IpAdresi))
                        {
                            var newWhiteList = new WhiteList
                            {
                                IpAdresi = scanIp.IpAdresi,
                                DangerLevel = 0,
                                Date = DateTime.Now
                            };

                            await context.WhiteList.AddAsync(newWhiteList);
                           
                        }
                        context.ScanIp.Remove(scanIp);
                        await context.SaveChangesAsync();
                        return;
                    }
                    else
                    {
                        if (!blackListIps.Contains(scanIp.IpAdresi))
                        {
                            await AddToFirewall(scanIp.IpAdresi, "Tehlike Oranı: %" + tehlikeYuzde, context);

                            var newBlackList = new BlackList
                            {
                                IpAdresi = scanIp.IpAdresi,
                                DangerLevel = (int)tehlikeYuzde,
                                Date = DateTime.Now,
                                DomainName = parsedJson.Data.Domain,
                                Country = parsedJson.Data.CountryName
                            };

                            await context.BlackList.AddAsync(newBlackList);
                          
                        }
                        context.ScanIp.Remove(scanIp);
                        await context.SaveChangesAsync();
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

                        await context.WhiteList.AddAsync(newWhiteList);
                      
                    }
                    context.ScanIp.Remove(scanIp);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(DateTime.Now + " - " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }
    }

}
