using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace QYWeiXinDemo.Helper
{
    /// <summary>
    /// 该类主要是处理网络请求
    /// </summary>
    class SecurityHttpUtil
    {

        /// <summary>
        /// 该方法主要是处理POST请求,返回处理返回的结果字符串
        /// </summary>
        /// <param name="posturl">请求的接口链接</param>
        /// <param name="postdata">请求的数据</param>
        /// <returns>返回的结果</returns>
        /// 
        #region 该方法主要是处理POST请求,返回处理的结果字符串
        public static string DealPost(string posturl, string postdata, string cookie = "")
        {
            string responseString = "";//post返回的结果
            ServicePointManager.ServerCertificateValidationCallback
                        += RemoteCertificateValidate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(posturl);
            req.Method = "POST";
            if (string.IsNullOrEmpty(cookie) == false)
                req.Headers["Cookie"] = cookie;
            byte[] postBytes = Encoding.UTF8.GetBytes(postdata);
            req.ContentType = "application/json; charset=utf-8";
            req.ContentLength = Encoding.UTF8.GetByteCount(postdata);
            Stream stream = req.GetRequestStream();
            stream.Write(postBytes, 0, postBytes.Length);
            req.Timeout = 30000;
            stream.Close();
            var response = req.GetResponse();
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            responseString = streamRead.ReadToEnd();
            response.Close();
            streamRead.Close();
            return responseString;
        }
        #endregion

        #region 信任https请求证书
        private static bool RemoteCertificateValidate(
            object sender, X509Certificate cert,
             X509Chain chain, SslPolicyErrors error)
        {
            System.Console.WriteLine("Warning, trust any certificate");
            return true;
        }
        #endregion

        #region 该方法主要是处理Get请求,返回处理的结果字符串
        public static string DealGet(string url)
        {
            //设置安全的类型
            ServicePointManager.ServerCertificateValidationCallback
                       += RemoteCertificateValidate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            string content = reader.ReadToEnd();
            reader.Close();
            return content;
        }
        #endregion

        #region 实现Http上传
        public static string UploadByHttp(string posturl, string path)
        {
            ServicePointManager.ServerCertificateValidationCallback
                    += RemoteCertificateValidate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            HttpWebRequest request = WebRequest.Create(posturl) as HttpWebRequest;
            CookieContainer cookieContainer = new CookieContainer();
            request.CookieContainer = cookieContainer;
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线
            request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            int pos = path.LastIndexOf("\\");
            string fileName = path.Substring(pos + 1);
            //请求头部信息 
            StringBuilder sbHeader = new StringBuilder(string.Format("Content-Disposition:form-data;name=\"file\";filename=\"{0}\"\r\nContent-Type:application/octet-stream\r\n\r\n", fileName));
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(sbHeader.ToString());
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] bArr = new byte[fs.Length];
            fs.Read(bArr, 0, bArr.Length);
            fs.Close();
            Stream postStream = request.GetRequestStream();
            //写入照片信息
            postStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
            postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            postStream.Write(bArr, 0, bArr.Length);
            postStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            postStream.Close();
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream instream = response.GetResponseStream();
            StreamReader sr = new StreamReader(instream, Encoding.UTF8);
            //返回结果网页（html）代码
            string content = sr.ReadToEnd();
            return content;
        }
        #endregion
    }
}
