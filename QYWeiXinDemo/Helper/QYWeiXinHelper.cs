using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace QYWeiXinDemo.Helper
{
    /// <summary>
    /// 发送企业微信消息
    /// </summary>
    class QYWeiXinHelper
    {
        private static QYWeiXinObject qyWxObj;
        private static QYWeiXinObject QYWeiXinObj
        {
            get
            {
                if (qyWxObj == null)
                {
                    qyWxObj = new QYWeiXinObject();
                    qyWxObj.CorpID = "wxbe5bc";
                    qyWxObj.Secret = "k1Nv0450ngQEQky9L3tSO0d";
                    qyWxObj.AgentId = 0;
                    qyWxObj.UrlTemplate = new QYWeiXinUrl()
                    {
                        Token = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid=[CorpID]&corpsecret=[Secret]".Replace("[CorpID]", qyWxObj.CorpID).Replace("[Secret]", qyWxObj.Secret),
                        Depart = "https://qyapi.weixin.qq.com/cgi-bin/department/list?access_token=[ACCESS_TOKEN]&id=[ID]",
                        Member = "https://qyapi.weixin.qq.com/cgi-bin/user/simplelist?access_token=[ACCESS_TOKEN]&department_id=[DEPARTMENT_ID]&fetch_child=[FETCH_CHILD]",
                        MemberDetail = "https://qyapi.weixin.qq.com/cgi-bin/user/list?access_token=[ACCESS_TOKEN]&department_id=[DEPARTMENT_ID]&fetch_child=[FETCH_CHILD]",
                        SendMsg = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token=[ACCESS_TOKEN]"
                    };
                }

                return qyWxObj;
            }
        }

        public static void DoAlert(string fromUser, string toUser, string title, string Content, string link)
        {
            DateTime curDate = DateTime.Now;

            //开线程发送
            Thread thread = new Thread(() => SendQYWeiXinMsg(toUser, fromUser, curDate, title, Content, link));
            thread.IsBackground = true;
            thread.Start();
        }

        public static void DoAlert(string fromUser, string toUser, string title, string Content)
        {
            DoAlert(fromUser, toUser, title, Content, "");
        }

        private static void SendQYWeiXinMsg(string to, string from, DateTime dateTime, string title, string msgInfo, string link)
        {
            JObject jMsgParam = new JObject();
            jMsgParam.Add("touser", to);
            jMsgParam.Add("agentid", QYWeiXinObj.AgentId);

            if (string.IsNullOrEmpty(link) == false)
            {
                string desription = "<div class=\"gray\">[DATETIME]([FROMUSER])</div>\n\n[MESSAGE]\n";
                desription = ReplaceMsgParameter(from, dateTime, title, msgInfo, desription);

                jMsgParam.Add("msgtype", "textcard");
                JObject textCard = new JObject();
                textCard.Add("title", title);
                textCard.Add("description", desription);
                textCard.Add("url", link);
                jMsgParam.Add("textcard", textCard);
            }
            else
            {
                string desription = "[TITLE] \n\n [MESSAGE] \n\n [DATETIME]([FROMUSER])";
                desription = ReplaceMsgParameter(from, dateTime, title, msgInfo, desription);

                jMsgParam.Add("msgtype", "text");
                JObject text = new JObject();
                text.Add("content", desription);
                jMsgParam.Add("text", text);
            }

            string postData = jMsgParam.ToString();
            if (QYWeiXinObj.Cache == null)
            {
                //获取token
                GetTokenFromWeb();
            }

            if (QYWeiXinObj.Cache != null)
            {
                SendMsgFromWeb(postData);
            }
        }

        private static void SendMsgFromWeb(string postData)
        {
            try
            {
                string ret = SecurityHttpUtil.DealPost(QYWeiXinObj.Url.SendMsg, postData);
                if (string.IsNullOrEmpty(ret) == false)
                {
                    JToken errcode, errmsg;
                    JObject jObj = JObject.Parse(ret);
                    if (jObj.TryGetValue("errcode", out errcode) && jObj.TryGetValue("errmsg", out errmsg))
                    {
                        JValue errcodeV, errmsgV;
                        errcodeV = errcode as JValue;
                        errmsgV = errmsg as JValue;
                        if (errcodeV != null && (long)errcodeV.Value == 0)
                        {
                            Logger.Write(LogLevel.Debug, "QYWeiXin:SendMsg from api is ok!" + ret);
                        }
                        else
                        {
                            Logger.Write(LogLevel.Error, "QYWeiXin:SendMsg from api is error,there is not correct reponse!" + ret);
                        }
                    }
                    else
                    {
                        Logger.Write(LogLevel.Error, "QYWeiXin:SendMsg from api is error,there is not correct reponse!" + ret);
                    }
                }
                else
                {
                    Logger.Write(LogLevel.Error, "QYWeiXin:SendMsg from api is error,there is no reponse!");
                }
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Error, "QYWeiXin:SendMsg is error!" + ex.ToString());
            }
        }

        private static void GetTokenFromWeb()
        {
            try
            {
                string tokenJson = SecurityHttpUtil.DealGet(QYWeiXinObj.UrlTemplate.Token);
                if (string.IsNullOrEmpty(tokenJson) == false)
                {
                    JToken errcode, access_token, expires_in;

                    JObject jObj = JObject.Parse(tokenJson);
                    if (jObj.TryGetValue("errcode", out errcode) && jObj.TryGetValue("access_token", out access_token) && jObj.TryGetValue("expires_in", out expires_in))
                    {
                        JValue errcodeV, access_tokenV, expires_inV;
                        errcodeV = errcode as JValue;
                        access_tokenV = access_token as JValue;
                        expires_inV = expires_in as JValue;

                        if (errcodeV != null && (long)errcodeV.Value == 0)
                        {
                            QYWeiXinObj.SetCahce((string)access_tokenV.Value, (long)expires_inV.Value);
                        }
                        else
                        {
                            Logger.Write(LogLevel.Error, "QYWeiXin:GetToken from api is error,there is not correct reponse!" + tokenJson);
                        }
                    }
                    else
                    {
                        Logger.Write(LogLevel.Error, "QYWeiXin:GetToken from api is error,there is not correct response!" + tokenJson);
                    }
                }
                else
                {
                    Logger.Write(LogLevel.Error, "QYWeiXin:GetToken from api is error,there is no reponse!");
                }
            }
            catch (Exception ex)
            {
                Logger.Write(LogLevel.Error, "QYWeiXin:GetToken is error!" + ex.ToString());
            }
        }

        private static string ReplaceMsgParameter(string from, DateTime dateTime, string title, string msgInfo, string desription)
        {
            return desription.Replace("[TITLE]", title).Replace("[MESSAGE]", msgInfo).Replace("[FROMUSER]", from).Replace("[DATETIME]", dateTime.ToString("yyyy-MM-dd HH:mm"));
        }


        #region 内部类
        class QYWeiXinObject
        {
            /// <summary>
            /// 公司ID
            /// </summary>
            public string CorpID { get; set; }

            /// <summary>
            /// 应用ID
            /// </summary>
            public int AgentId { get; set; }

            /// <summary>
            /// 密钥
            /// </summary>
            public string Secret { get; set; }

            /// <summary>
            /// Token缓存
            /// </summary>
            private QYWeiXinCache cache;

            /// <summary>
            /// 获取Token缓存
            /// </summary>
            public QYWeiXinCache Cache
            {
                get
                {
                    if (cache != null)
                    {
                        long curTimeStamp = GetTimstamp();
                        //超过上一次的超时时间，则认为已经失效 额外附加30秒用于本次数据交互
                        if (curTimeStamp - cache.TimeStamp + 30 > cache.ExpiresIn)
                        {
                            return null;
                        }
                        else
                        {
                            return cache;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            private static long GetTimstamp()
            {
                TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);
                return (long)ts.TotalSeconds;
            }

            /// <summary>
            /// 设置Token缓存
            /// </summary>
            /// <param name="AccessToken"></param>
            /// <param name="ExpiresIn"></param>
            public void SetCahce(string AccessToken, long ExpiresIn)
            {
                long curTimeStamp = GetTimstamp();
                if (cache == null)
                {
                    cache = new QYWeiXinCache();
                }

                cache.AccessToken = AccessToken;
                cache.ExpiresIn = ExpiresIn;
                cache.TimeStamp = curTimeStamp;

                ReplaceUrlToken();
            }

            /// <summary>
            /// Url
            /// </summary>
            public QYWeiXinUrl Url { get; set; }

            public QYWeiXinUrl UrlTemplate { get; set; }

            /// <summary>
            /// 替换URL中涉及到的Token
            /// </summary>
            /// <param name="url"></param>
            /// <param name="token"></param>
            private void ReplaceUrlToken()
            {
                if (Url == null)
                {
                    Url = new QYWeiXinUrl();
                }

                if (Cache != null)
                {
                    Url.Depart = UrlTemplate.Depart.Replace("[ACCESS_TOKEN]", Cache.AccessToken);
                    Url.Member = UrlTemplate.Member.Replace("[ACCESS_TOKEN]", Cache.AccessToken);
                    Url.MemberDetail = UrlTemplate.MemberDetail.Replace("[ACCESS_TOKEN]", Cache.AccessToken);
                    Url.SendMsg = UrlTemplate.SendMsg.Replace("[ACCESS_TOKEN]", Cache.AccessToken);
                }
            }
        }

        class QYWeiXinCache
        {
            /// <summary>
            /// 记录的Token
            /// </summary>
            public string AccessToken { get; set; }

            /// <summary>
            /// 失效时长秒
            /// </summary>
            public long ExpiresIn { get; set; }

            /// <summary>
            /// Token生成时间戳
            /// </summary>
            public long TimeStamp { get; set; }
        }

        class QYWeiXinUrl
        {
            /// <summary>
            /// 获取Token的Url
            /// </summary>
            public string Token { get; set; }

            /// <summary>
            /// 获取部门的Url
            /// </summary>
            public string Depart { get; set; }

            /// <summary>
            /// 获取部门成员Url
            /// </summary>
            public string Member { get; set; }

            /// <summary>
            /// 获取部门成员详细信息Url
            /// </summary>
            public string MemberDetail { get; set; }

            /// <summary>
            /// 通过应用发消息Url
            /// </summary>
            public string SendMsg { get; set; }
        }


        #endregion
    }
}
