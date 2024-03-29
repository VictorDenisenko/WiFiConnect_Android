﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace WiFiConnect
{
    public struct CommonStruct
    {
        static public string devicePortalAdminName = "Administrator";
        static public string devicePortalPassword = null;
        static public int lastConnectedId = 0;
        static public string lastConnectedSsid = "";
        static public string strMessage = "";
        static public string notifyType = "";
        static public string robotIpAddress = "";
        static public string publicGuid = "";
        
    }

    public delegate Task<string> Function(string devicePortalPassword, string devicePortalAdminName, string softApAddress);

    public partial class BasicAlgorthm 
    {
        MainPage mp;
        public BasicAlgorthm()
        {
            mp = MainPage.mainPage;
        }

        private async Task<string> LookingForGuidAsync(string devicePortalPassword, string devicePortalAdminName, string x)
        {
            Uri uri = new Uri("http://192.168.137.1:8080/api/wifi/interfaces");
            string guid = "";
            HttpResponseMessage response;
            string result = "";
            try
            {
                var authData = string.Format("{0}:{1}", devicePortalAdminName, devicePortalPassword);
                var authHeaderValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authData));

                using (HttpClient client = new HttpClient())
                {
                    client.MaxResponseContentBufferSize = 256000;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
                    client.Timeout = TimeSpan.FromSeconds(3);

                    response = await client.GetAsync(uri).ConfigureAwait(true);

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            return response.StatusCode.ToString();
                        }
                    if (response.IsSuccessStatusCode)
                    {
                        result = Convert.ToString(response.StatusCode);

                        if (response.Content != null)
                        {
                            string responseBodyAsText = "";
                            responseBodyAsText = await response.Content.ReadAsStringAsync();
                            guid = getGUID(Convert.ToString(responseBodyAsText));
                            CommonStruct.publicGuid = guid;
                            if (guid == "")
                            {
                                result = "";
                                return result;
                            }
                        }
                    }
                    else
                    {
                        result = "";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
                result = "WrongNet";
                return result; 
            }

            if ((result == "OK") && (guid != ""))
            {
                return result;
            }
            else if ((result == "OK") && (guid == ""))
            {
                result = "GuidFail";
                return result;
            }
            else
            {
                result = "";
                return result;
            }
        }

        public async Task<string> ConnectRobotToWiFiAsync(string networkName, string networkSecurityKey, string x)
        {

            string result1 = await LookingForGuidAsync(CommonStruct.devicePortalPassword, "Administrator", "");

            if ((result1 == "WrongNet") || (result1 == "Unauthorized"))
            {
                return result1;
            }

            if (result1 != "OK")
            {
                return result1;
            }

            string result = "";
            string ssid = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(networkName));
            string key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(networkSecurityKey));
            Uri setIpUri = new Uri("http://192.168.137.1:8080/api/wifi/network?interface=" + CommonStruct.publicGuid + "&ssid=" + ssid + "&op=connect&key=" + key + "&createprofile=yes");
            var authData = string.Format("{0}:{1}", CommonStruct.devicePortalAdminName, CommonStruct.devicePortalPassword);
            var authHeaderValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authData));
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.MaxResponseContentBufferSize = 256000;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
                    HttpContent content = null;
                    using (HttpResponseMessage response = await client.PostAsync(setIpUri, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = Convert.ToString(response.StatusCode);
                        }
                        else
                        {
                            result = Convert.ToString(response.ReasonPhrase);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                result = ex.Message; 
            }
            return result;
        }

        private string getGUID(string json)
        {
            string guid = "";
            if (json.Contains("GUID"))
            {
                int position = json.IndexOf("GUID");
                string shortString = json.Substring(position);
                char[] separator = new char[] { '{', '}' };
                string[] array = shortString.Split(separator);
                guid = array[1];
            }
            else
            {
                guid = "";
            }
            return guid;
        }
             
 
        public async Task<string> IteratorAsync(string x, string y, string z, Function func)
        {
            string result = "";
            DateTime dt = DateTime.Now;
            long startTicks = dt.Ticks;
            long oneSec = 10000000;//Одна сек = 10 млн. тиков
            result = await func(x, y, z);
            long timeEllapsed = 0;
            int loopNumber = 0;

            if ( result =="WrongNet") return result;

            if ((result != "OK") && (result != "NotConnected"))
            {
                while (((result != "OK") && (result != "NotConnected")) && (timeEllapsed < 120))
                {
                    result = await func(x, y, z);
                    dt = DateTime.Now;
                    timeEllapsed = (dt.Ticks - startTicks) / oneSec;//
                    if (timeEllapsed > 30)
                    {
                        mp.NotifyUser("Check if the robot is ON. May be you need to reboot it.", MainPage.NotifyType.ErrorMessage);
                        timeEllapsed = 0;
                        return result;
                    }
                    loopNumber++;
                    if (loopNumber > 10) return result;
                }
            }
            return result;
        }
    }
}
