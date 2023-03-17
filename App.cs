using DnsServerCore.ApplicationCommon;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using TechnitiumLibrary.Net.Dns;
using TechnitiumLibrary.Net.Dns.ResourceRecords;

namespace QueryLogsDb
{
    public class App : IDnsApplication, IDnsQueryLogger
    {
        IDnsServer _dnsServer;
        string _vmUrl;
        Timer? _queueTimer;
        readonly ConcurrentQueue<LogEntry> _queuedLogs = new ConcurrentQueue<LogEntry>();
        private static HttpClient _client;

        const int QUEUE_TIMER_INTERVAL = 10000;
        const int BULK_INSERT_COUNT = 1000;

        private Lazy<string> CSV_SCHEMA = new Lazy<string>(() =>
        {
            var data = new[]{
                ("time", "unix_ms"),
                ("label", "client_ip"),
                ("label", "answer"),
                ("label", "request_domain"),
                ("metric", "request_type"),
                ("metric", "protocol"),
                ("metric", "response_type"),
                ("metric", "rcode"),
                ("metric", "class")
            };

            var str = "";
            var i = 1;
            foreach (var item in data)
            {
                str += $"{i}:{item.Item1}:{item.Item2},";
                i++;
            }
            str = str.TrimEnd(',');

            return str;
        });
        string ENDPOINT_IMPORT_CSV
        {
            get
            {
                return _vmUrl
                    + "/api/v1/import/csv?format="
                    + CSV_SCHEMA.Value
                    + "&extra_label=job=technitium&extra_label=instance=technitium";
            }
        }

        public string Description
        {
            get
            {
                return "Logs DNS request to db.";
            }
        }

        private void BulkInsertLogs()
        {
            try
            {
                List<LogEntry> logs = new List<LogEntry>(BULK_INSERT_COUNT);

                while (true)
                {
                    while ((logs.Count < BULK_INSERT_COUNT) && _queuedLogs.TryDequeue(out LogEntry? log))
                    {
                        logs.Add(log);
                    }

                    if (logs.Count < 1)
                        break;

                    string csv = "";
                    foreach (LogEntry log in logs)
                    {
                        csv += log.ToCsv();
                        csv += "\n";
                    }
                    csv.TrimEnd('\n');

#if DEBUG
                    Console.WriteLine(csv);
#endif
                    var content = new StringContent(csv, Encoding.UTF8, "text/csv");
                    var response = _client.PostAsync(ENDPOINT_IMPORT_CSV, content).Result;

                    logs.Clear();
                }
            }
            catch (Exception ex)
            {
                if (_dnsServer is not null)
                    _dnsServer.WriteLog(ex);
            }
        }


        public Task InitializeAsync(IDnsServer dnsServer, string configStr)
        {
            _dnsServer = dnsServer;

            using JsonDocument jsonDocument = JsonDocument.Parse(configStr);
            JsonElement config = jsonDocument.RootElement;

            _vmUrl = config.GetProperty("vmUrl").GetString() ?? throw new Exception();

            _client = new HttpClient();

            _queueTimer = new Timer((object? state) =>
            {
                try
                {
                    BulkInsertLogs();
                }
                catch (Exception ex)
                {
                    _dnsServer.WriteLog(ex);
                }
                finally
                {
                    if (_queueTimer is not null)
                        _queueTimer.Change(QUEUE_TIMER_INTERVAL, Timeout.Infinite);
                }
            });

            _queueTimer.Change(QUEUE_TIMER_INTERVAL, Timeout.Infinite);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_queueTimer is not null)
            {
                _queueTimer.Dispose();
                _queueTimer = null;
            }
            return;
        }

        public Task InsertLogAsync(DateTime timestamp, DnsDatagram request, IPEndPoint remoteEP, DnsTransportProtocol protocol, DnsDatagram response)
        {
            _queuedLogs.Enqueue(new LogEntry(timestamp, request, remoteEP, protocol, response));
            return Task.CompletedTask;
        }

        public Task<DnsLogPage> QueryLogsAsync(
          long pageNumber,
          int entriesPerPage,
          bool descendingOrder,
          DateTime? start,
          DateTime? end,
          IPAddress clientIpAddress,
          DnsTransportProtocol? protocol,
          DnsServerResponseType? responseType,
          DnsResponseCode? rcode,
          string qname,
          DnsResourceRecordType? qtype,
          DnsClass? qclass
        )
        {
            Console.WriteLine("QueryLogsAsync");
            throw new NotImplementedException();
        }

    }
}
