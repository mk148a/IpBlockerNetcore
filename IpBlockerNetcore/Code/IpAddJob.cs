using IpBlockerNetcore.Data;
using IpBlockerNetcore.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Net.NetworkInformation;

[DisallowConcurrentExecution]
public class IpAddJob : IJob
{
    private readonly ILogger<IpAddJob> _logger;
    private readonly IDbContextFactory<IpBlockerNetcoreContext> _contextFactory;

    public IpAddJob(IDbContextFactory<IpBlockerNetcoreContext> contextFactory, ILogger<IpAddJob> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        string result = await ShowActiveTcpConnections();
        // Büyük veri işlemi tamamlandıktan sonra
        GC.Collect();
        GC.WaitForPendingFinalizers();
        await Console.Out.WriteLineAsync("IpAddJob is executing. Result: " + result);
    }

    public async Task<string> ShowActiveTcpConnections()
    {
        try
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var activeConnections = properties.GetActiveTcpConnections();

            var allIps = activeConnections
                .SelectMany(x => new[] { x.RemoteEndPoint.Address.MapToIPv4().ToString(), x.LocalEndPoint.Address.MapToIPv4().ToString() })
                .Distinct()
                .ToList();

            var whiteListIps = _context.WhiteList.Select(x => x.IpAdresi).ToHashSet();
            var blackListIps = _context.BlackList.Select(x => x.IpAdresi).ToHashSet();
            var scannedIps = _context.ScanIp.Select(x => x.IpAdresi).ToHashSet();

            var newIps = allIps.Where(ip => !whiteListIps.Contains(ip) && !blackListIps.Contains(ip) && !scannedIps.Contains(ip)).ToList();

            foreach (var ip in newIps)
            {
                try
                {
                    _logger.LogWarning($"{ip} nolu ip ekleniyor");
                    var yeni = new ScanIp
                    {
                        IpAdresi = ip,
                        Date = DateTime.Now
                    };

                    await _context.ScanIp.AddAsync(yeni);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, $"{ip} adresi eklenirken hata oluştu.");
                }
            }

            await _context.SaveChangesAsync();
            return "complete";
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "ShowActiveTcpConnections çalışırken hata oluştu.");
            return "failed";
        }
    }
}
