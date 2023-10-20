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
using System.Threading.Tasks;
using System.Web;

namespace Facebook.Csharp.Business.Sdk.Sdk
{
  public class APINodeList<T> : List<T>, IAPIResponse<T> where T : APINode
  {
    private APICollectionRequest<T> _request;
    private string? _after;
    private string? _rawValue;
    private string _appSecret;
    private bool _autoPagination;

    // See https://developers.facebook.com/docs/graph-api/using-graph-api/#paging
    private string? _previous;
    private string? _next;

    public APINodeList(APICollectionRequest<T> request, string rawValue, Paging paging)
    {
      _request = request;
      _rawValue = rawValue;
      _after = paging?.Cursors?.After;
      _previous = paging?.Cursors?.Before;
      _next = paging?.Next;
      _previous = paging?.Previous;
    }

    public APIException Exception => null;

    public T Head()
    {
      return Count > 0 ? this[0] : null;
    }

    public string ToRawResponse()
    {
      return _rawValue;
    }

    public Task<APINodeList<T>> NextPage()
    {
      return NextPage(0);
    }

    public Task<APINodeList<T>> NextPage(int limit)
    {
      if (!string.IsNullOrEmpty(_next)) {
        if (string.IsNullOrEmpty(_appSecret)) {
          _request.OverrideUrl = _next;
        } else {
            var uri = new Uri(_next);
            var connector = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
            var newUrl = $"{_next}{connector}appsecret_proof={HttpUtility.UrlEncode(_appSecret)}";
            _request.OverrideUrl = newUrl;
        }
        return _request.ExecuteAsync();
      }

      if (string.IsNullOrEmpty(_after))
      {
        return Task.FromResult((APINodeList<T>) null);
      }

      _request.OverrideUrl = null;
      var extraParams = new Dictionary<string, object>();
      if (limit > 0)
      {
        extraParams.Add("limit", limit);
      }
      extraParams.Add("after", _after);
      return _request.ExecuteAsync(extraParams);
    }
  }
}
