using System.Collections.Generic;
using Authy.Net;
using Newtonsoft.Json;

namespace Authy2FA.Domain.Authy
{
    public class SendApprovalRequestResult : AuthyResult
    {
        [JsonProperty("approval_request")]
        public IDictionary<string, string> ApprovalRequest { get; set; }
    }
}