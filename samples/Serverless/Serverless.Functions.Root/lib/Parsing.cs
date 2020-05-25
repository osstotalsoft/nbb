// Copyright (c) Alex Ellis 2017. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Serverless.Functions.Root.Lib
{
     public class HttpFormatter {
        public string Format(string body)  {
            StringBuilder outBuffer = new StringBuilder();
            outBuffer.Append("HTTP/1.1 " + 200 + " OK\r\n");
            outBuffer.Append("Content-Length: "+body.Length+"\r\n");
            outBuffer.Append("Content-Type: text/html\r\n");
            outBuffer.Append("Connection: Close\r\n");  
            outBuffer.Append("\r\n");
            outBuffer.Append(body);
            return outBuffer.ToString();
        }
    }

    public class BodyParser
    {
        public string Parse(TextReader reader, int length) {
            var buffer = new char[length];
            reader.ReadBlock(buffer, 0, length);

            return new string(buffer);
        }
    }

    public class HttpRequest {
        public string Method { get; set; }

        public string URI { get; set; }

        public NameValueCollection HttpHeaders { get; set;}

        public int ContentLength { 
            get {           
                if(HttpHeaders["Content-Length"] == null) {
                    return 0;
                }

                return Int32.Parse(HttpHeaders["Content-Length"]); 
            }
            
        }
    }

    public class HeaderParser {

        public HttpRequest Parse(TextReader reader) {
            var req = new HttpRequest{
                HttpHeaders = new NameValueCollection()
            };
            string line;
            while(!string.IsNullOrEmpty(line = reader.ReadLine())) {

                int keyIndex = line.IndexOf(":");
                if (keyIndex > -1) {
                    string key = line.Substring(0, keyIndex);
                    string value = line.Substring(keyIndex+2);
                    req.HttpHeaders.Add(key, value);
                }
            }

            return req;
        }
    }
}