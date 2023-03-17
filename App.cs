using DnsServerCore.ApplicationCommon;
using DnsServerCore.Dns.Applications;
using DnsServerCore.Dns;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TechnitiumLibrary.Net.Dns;
using TechnitiumLibrary.Net.Dns.ResourceRecords;

namespace QueryLogsSqlite
{

    public class App : IDnsApplication, IDnsQueryLogger
    {
        IDnsServer _dnsServer;
        string _connectionString;

        public string Description
        {
            get
            {
                return "Logs DNS request to db.";
            }
        }


        public Task InitializeAsync(IDnsServer dnsServer, string configStr)
        {
            _dnsServer = dnsServer;

            using JsonDocument jsonDocument = JsonDocument.Parse(configStr);
            JsonElement config = jsonDocument.RootElement;

            string connectionString = config.GetProperty("connectionString").GetString() ?? throw new Exception();

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task InsertLogAsync(DateTime timestamp, DnsDatagram request, IPEndPoint remoteEP, DnsTransportProtocol protocol, DnsDatagram response)
        {
            throw new NotImplementedException();
        }

        public Task<DnsLogPage> QueryLogsAsync(long pageNumber, int entriesPerPage, bool descendingOrder, DateTime? start, DateTime? end, IPAddress clientIpAddress, DnsTransportProtocol? protocol, DnsServerResponseType? responseType, DnsResponseCode? rcode, string qname, DnsResourceRecordType? qtype, DnsClass? qclass)
        {
            throw new NotImplementedException();
        }
    }
}
