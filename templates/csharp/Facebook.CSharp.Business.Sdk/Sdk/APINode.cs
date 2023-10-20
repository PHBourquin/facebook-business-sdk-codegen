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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Facebook.Csharp.Business.Sdk.Sdk
{
  public class APINode : IAPIResponse<APINode>
  {
    protected string rawValue = null;

    [JsonIgnore]
    public APIContext Context { get; set; }

    public APIException Exception => null;

    public string ToRawResponse()
    {
      return rawValue;
    }

    public APINode Head()
    {
      return this;
    }

    public static APINodeList<APINode> ParseCollectionResponse(string json, APIContext context, APIRequest<APINode> request)
    {
      return null;
    }

    public static APINode ParseSingleResponse(string json, APIContext context, APIRequest<APINode> request)
    {
      return null;
    }
  }
}
