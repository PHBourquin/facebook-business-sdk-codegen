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
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Facebook.Csharp.Business.Sdk.Sdk
{
  public class APIContext
  {
    private const string DefaultApiBase = APIConfig.DEFAULT_API_BASE;
    private const string DefaultApiVersion = APIConfig.DEFAULT_API_VERSION;

    protected APIContext(string endpointBase, string version, string accessToken, string appSecret)
    {
      Version = version;
      EndpointBase = endpointBase;
      AccessToken = accessToken;
      AppSecret = appSecret;
      HttpClient = new HttpClient();
    }

    public APIContext(string accessToken)
      : this(DefaultApiBase, DefaultApiVersion, accessToken, null)
    {
    }

    public APIContext(string accessToken, string appSecret)
      : this(DefaultApiBase, DefaultApiVersion, accessToken, appSecret)
    {
    }

    public string EndpointBase { get; private set; }
    public string AccessToken { get; private set; }
    public string AppSecret { get; private set; }
    public string AppSecretProof => Sha256(AppSecret, AccessToken);
    public string Version { get; private set; }
    public HttpClient HttpClient { get; set; }

    public bool HasAppSecret()
    {
      return AppSecret != null;
    }

    private string Sha256(string secret, string message)
    {
      try
      {
        var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return ToHex(bytes);
      }
      catch (Exception)
      {
        return null;
      }
    }

    private static string ToHex(byte[] bytes)
    {
      var sb = new StringBuilder();
      foreach (byte b in bytes)
      {
          sb.Append(b.ToString("x2"));
      }
      return sb.ToString();
    }
  }
}