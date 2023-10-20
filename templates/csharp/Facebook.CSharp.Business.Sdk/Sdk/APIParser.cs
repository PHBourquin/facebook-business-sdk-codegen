using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Facebook.Csharp.Business.Sdk.Sdk;

public class APIParser
{   
    public static bool ParseUpdateDeleteResponse(string json)
    {
        ThrowOnErrorResponse(json);
        var obj = JsonConvert.DeserializeObject<UpdateDeleteResponse>(json);
        return obj?.Success ?? false;
    }

    public static void ThrowOnErrorResponse(string json)
    {
      var errorResponse = JObject.Parse(json)["error"];
      if (errorResponse != null)
      {
        var facebookErrorResponse = errorResponse.ToObject<ErrorResponse>();
        throw new Exception($"Got an error from facebook API: {facebookErrorResponse}");
      }
    }
}