using Newtonsoft.Json;

namespace Facebook.Csharp.Business.Sdk.Sdk
{
    public class APIDataResponse<T>
    {
        [JsonProperty("data")]
        public readonly T[] Data;

        [JsonProperty("paging")]
        public readonly Paging Paging;

        public APIDataResponse(T[] data, Paging paging = null)
        {
            Data = data;
            Paging = paging;
        }
    }

    public class Paging
    {
        [JsonProperty("cursors")]
        public Cursors Cursors { get; set; }

        [JsonProperty("previous")]
        public string Previous { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }
    }

    public class Cursors
    {
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }
    }

    public class UpdateDeleteResponse
    {
        [JsonProperty("success", Required = Required.Always)]
        public bool Success;

        [JsonConstructor]
        public UpdateDeleteResponse(bool success)
        {
            Success = success;
        }
    }

    public class ErrorResponse
    {
        [JsonProperty("message", Required = Required.Always)]
        public readonly string Message;
        [JsonProperty("type")]
        public readonly string Type;
        [JsonProperty("code")]
        public readonly int Code;
        [JsonProperty("error_subcode")]
        public readonly int SubCode;
        [JsonProperty("error_user_title")]
        public readonly string UserTitle;
        [JsonProperty("error_user_msg")]
        public readonly string UserMessage;
        [JsonProperty("fbtrace_id")]
        public readonly string MetaTraceId;

        [JsonConstructor]
        public ErrorResponse(string message, string type, int code, int subCode, string userTitle, string userMessage, string metaTraceId)
        {
            Message = message;
            Type = type;
            Code = code;
            SubCode = subCode;
            UserTitle = userTitle;
            UserMessage = userMessage;
            MetaTraceId = metaTraceId;
        }

        public override string ToString()
        {
            return $"{nameof(Message)}: {Message}, {nameof(Type)}: {Type}, {nameof(Code)}: {Code}, {nameof(SubCode)}: {SubCode}, {nameof(UserTitle)}: {UserTitle}, {nameof(UserMessage)}: {UserMessage}, {nameof(MetaTraceId)}: {MetaTraceId}";
        }
    }
}
