using System.Net;
using DnsServerCore.ApplicationCommon;
using TechnitiumLibrary.Net.Dns;

class LogEntry
{
    public readonly DateTimeOffset timestamp;
    public readonly DnsDatagram request;
    public readonly IPEndPoint remoteEP;
    public readonly DnsTransportProtocol protocol;
    public readonly DnsDatagram response;

    public LogEntry(
        DateTime timestamp,
        DnsDatagram request,
        IPEndPoint remoteEP,
        DnsTransportProtocol protocol,
        DnsDatagram response
    )
    {
        var utcTimestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        this.timestamp = new DateTimeOffset(utcTimestamp);
        this.request = request;
        this.remoteEP = remoteEP;
        this.protocol = protocol;
        this.response = response;
    }

    public string ToCsv()
    {
        string[] param = new string[]{
            // time(unix_ms)
            this.timestamp.ToUnixTimeMilliseconds().ToString(),
            // client_ip
            this.remoteEP.Address.ToString(),
            // answer
            this.Answer(),
            // request_domain
            this.request.Question[0].Name,
            // request_type (A, AAAA, etc.)
            ((int)this.request.Question[0].Type).ToString(),
            // protocol (UDP, TCP, TLS, HTTPS)
            ((int)this.protocol).ToString(),
            // response_type (Recursive, Cached, etc.)
            (this.response.Tag == null
             ? (int)DnsServerResponseType.Recursive
             : (int)(DnsServerResponseType)this.response.Tag)
                .ToString(),
            // rcode
            ((int)this.response.RCODE).ToString(),
            // class
            ((int)this.request.Question[0].Class).ToString(),
        };

        var csv = string.Join(",", param);
        return csv;
    }

    private string Answer()
    {
        string? answer = null;

        if (this.response.Answer.Count == 0)
        {
            answer = null;
        }
        else if ((this.response.Answer.Count > 2) && this.response.IsZoneTransfer)
        {
            answer = "[ZONE TRANSFER]";
        }
        else
        {
            for (int i = 0; i < this.response.Answer.Count; i++)
            {
                if (answer is null)
                    answer = this.response.Answer[i].RDATA.ToString();
                else
                    answer += ", " + this.response.Answer[i].RDATA.ToString();
            }
        }
        return answer == null ? "" : "\"" + answer + "\"";
    }
}
