/**
* Copyright (c) 2015-present, Facebook, Inc. All rights reserved.
*
* You are hereby granted a non-exclusive, worldwide, royalty-free license to
* use, copy, modify, and distribute this software in source code or binary
* form for use in connection with the web services and APIs provided by
* Facebook.
*
* As with any software that integrates with the Facebook platform, your use
* of this software is subject to the Facebook Developer Principles and
* Policies [http://developers.facebook.com/policy/]. This copyright notice
* shall be included in all copies or substantial portions of the software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
* DEALINGS IN THE SOFTWARE.
*
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Facebook.Csharp.Business.Sdk.Sdk
{
  public abstract class APIRequest<T> where T : APINode
  {
    private const string UserAgent = APIConfig.USER_AGENT;

    private readonly string _nodeId;
    private readonly string _endpoint;
    private readonly string _method;
    private IDictionary<string, object> _parameters = new Dictionary<string, object>();
    private List<string> _returnFields;

    private static readonly IDictionary<string, string> FileToContentTypeMap = new Dictionary<string, string>
    {
      {".atom", "application/atom+xml"},
      {".rss", "application/rss+xml"},
      {".xml", "application/xml"},
      {".csv", "text/csv"},
      {".txt", "text/plain"}
    };

    protected List<string> ParamNames;

    protected APIRequest(APIContext context, string nodeId, string endpoint, string method, IEnumerable<string> paramNames)
    {
      Context = context;
      _nodeId = nodeId;
      _endpoint = endpoint;
      _method = method;
      ParamNames = new List<string>(paramNames);
    }

    public APIContext Context { get; protected set; }
    public abstract bool IsCollectionResponse { get; }

    protected Task<string> SendRequestAsync(Dictionary<string, object> extraParams, CancellationToken cancellationToken)
    {
      // extraParams are one-time params for this call,
      // so that the APIRequest can be reused later on.
      if ("GET".Equals(_method, StringComparison.Ordinal))
      {
        return SendGet(GetApiUrl(), GetAllParams(extraParams), Context.HttpClient, cancellationToken);
      }
      if ("POST".Equals(_method, StringComparison.Ordinal))
      {
        return SendPost(GetApiUrl(), GetAllParams(extraParams), Context.HttpClient, cancellationToken);
      }
      if ("DELETE".Equals(_method, StringComparison.Ordinal))
      {
        return SendDelete(GetApiUrl(), GetAllParams(extraParams), Context.HttpClient, cancellationToken);
      }

      var message = "Unsupported http method. Currently only GET, POST, and DELETE are supported";
      throw new APIException(message, new ArgumentException(message));
    }

    private string? GetApiUrl()
    {
      return string.IsNullOrEmpty(OverrideUrl)
        ? Context.EndpointBase + "/" + Context.Version + "/" + _nodeId + _endpoint
        : OverrideUrl;
    }

    private static async Task<string> SendGet(string? apiUrl, Dictionary<string, object> allParams, HttpClient client, CancellationToken cancellationToken)
    {
      var sb = new StringBuilder(apiUrl);
      bool firstEntry = true;
      foreach (var entry in allParams)
      {
        sb.Append(firstEntry ? "?" : "&")
          .Append(WebUtility.UrlEncode(entry.Key))
          .Append("=")
          .Append(WebUtility.UrlEncode(ValueToString(entry.Value)));
        firstEntry = false;
      }
      var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());
      request.Headers.Add("UserAgent", UserAgent);
      var response = await client.SendAsync(request, cancellationToken);
      return await response.Content.ReadAsStringAsync();
    }

    private static async Task<string> SendPost(string? apiUrl, Dictionary<string, object> allParams, HttpClient client, CancellationToken cancellationToken)
    {
      using (var content = new MultipartFormDataContent())
      {
        foreach (var entry in allParams)
        {
          var fileValue = entry.Value as FileInfo;
          if (fileValue != null)
          {
            using (FileStream fs = fileValue.OpenRead())
            {
              content.Add(new StreamContent(fs), fileValue.Name, fileValue.Name);
            }
          }
          else
          {
            content.Add(new StringContent(entry.Value.ToString(), Encoding.UTF8), entry.Key);
          }
        }
        var response = await client.PostAsync(apiUrl, content, cancellationToken);
        return await response.Content.ReadAsStringAsync();
      }
    }

    private static async Task<string> SendDelete(string? apiUrl, Dictionary<string, object> allParams, HttpClient client, CancellationToken cancellationToken)
    {
      var sb = new StringBuilder(apiUrl);
      bool firstEntry = true;
      foreach (var entry in allParams)
      {
        sb.Append(firstEntry ? "?" : "&")
          .Append(WebUtility.UrlEncode(entry.Key))
          .Append("=")
          .Append(WebUtility.UrlEncode(ValueToString(entry.Value)));
        firstEntry = false;
      }
      var response = await client.DeleteAsync(sb.ToString(), cancellationToken);
      return await response.Content.ReadAsStringAsync();
    }

    private static string ValueToString(object input)
    {
      if (input == null)
      {
        return "null";
      }
      if (input is IDictionary<string, object>
        || input is IDictionary<string, string>
        || input is IList<string>
        || input is IList<object>)
      {
        return JsonConvert.SerializeObject(input);
      }
      return input.ToString();
    }

    private Dictionary<string, object> GetAllParams(Dictionary<string, object> extraParams)
    {
      if (!string.IsNullOrEmpty(OverrideUrl))
      {
        return new Dictionary<string, object>();
      }

      var allParams = new Dictionary<string, object>(_parameters);
      foreach (var pair in extraParams)
      {
        allParams[pair.Key] = pair.Value;
      }
      allParams["access_token"] = Context.AccessToken;
      if (Context.HasAppSecret())
      {
        allParams["appsecret_proof"] = Context.AppSecretProof;
      }
      if (_returnFields != null)
      {
        allParams["fields"] = JoinStringList(_returnFields);
      }
      return allParams;
    }

    private static string JoinStringList(List<string> list)
    {
      if (list == null) return "";
      return String.Join(",", list.ToArray());
    }

    protected void SetParamInternal(string parameter, object value)
    {
      _parameters[parameter] = value;
    }

    protected void SetParamsInternal(IDictionary<string, object> parameters)
    {
      _parameters = parameters;
    }

    protected void RequestFieldInternal(string field, bool value)
    {
      if (_returnFields == null)
      {
        _returnFields = new List<string>();
      }
      if (value == true && !_returnFields.Contains(field))
      {
        _returnFields.Add(field);
      }
      else
      {
        _returnFields.Remove(field);
      }
    }

    /// <summary>
    /// This is a hacky way to implement pagination
    /// In current implementaion, request is based on nodeId/endpoint/param
    /// However in case we have paging, the previous/next returns the overall
    /// url already. In that case, we don't want to parse and reconstruct the
    /// url. Thus add this override to use the returned url directly.
    /// </summary>
    public string? OverrideUrl { get; set; }
  }
}
