using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Phone.Info;

namespace WhatTheWord
{
    public class Instrumentation
    {
        public readonly string Url = "http://www.kooappsservers.com/kooappsPlatform/logToSql.php";
        public readonly string AppName = "com.kooapps.guessthisword";

        private static Instrumentation instrumentationInstance;

        private Instrumentation() { }

        public static Instrumentation GetInstance()
        {
            if (instrumentationInstance == null)
            {
                instrumentationInstance = new Instrumentation();
            }
            return instrumentationInstance;
        }

        public string getDeviceUniqueId()
        {
            string unqiueidAsString = "Unknown";
            object uniqueid = DeviceExtendedProperties.GetValue("DeviceUniqueId");
            if (uniqueid != null)
            {
                unqiueidAsString = BitConverter.ToString((byte[])uniqueid);
            }
            return unqiueidAsString;
        }

        public string getMD5Hash(string input)
        {
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            MD5Managed md5 = new MD5Managed();
            byte[] hash = md5.ComputeHash(bs);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2").ToLower());
            }

            return sb.ToString();
        }

        private string concatenateParametersForHash(Dictionary<string, string> parameters)
        {
            var list = parameters.Keys.ToList();
            list.Sort();

            StringBuilder sb = new StringBuilder();
            foreach (var key in list)
            {
                sb.Append(parameters[key]);
            }
                
            return sb.ToString();
        }

        private string generateHashForInstrumentation(
            string uid, Dictionary<string, string> parameters)
        {
            /* Hash algorithm
             * 
             * MD5(MD5(uid) + values of parameters alphabetically ordered by the keys)
             */

            // Concatenate the values of parameters.
            // The values are sorted based on the keys in alphabetical order

            var list = parameters.Keys.ToList();
            list.Sort();

            StringBuilder sb = new StringBuilder();
            foreach (var key in list)
            {
                sb.Append(parameters[key]);
            }
            string sortedParametervalues = sb.ToString();

            // MD5(uid) + values of parameters
            string hash = getMD5Hash(uid) + sortedParametervalues;

            // MD5(MD5(uid) + values of parameters)
            return getMD5Hash(hash);
        }


        public string concatenateParameters(
            Dictionary<string, string> parameters, string delimiter)
        {
            string query = String.Empty;

            int i = 0;
            foreach (var param in parameters)
            {
                if (i != 0) { query += delimiter; }
                i++;

                //query += param.Key + "=" + Uri.EscapeDataString(param.Value);
                query += param.Key + "=" + param.Value;
            }

            return query;
        }

        private string generateLogString(
            string event_category,
            string event_name,
            string type,
            string sub_type,
            string hc_amt)
        {
            var parameters = new Dictionary<string, string>();

            if (!String.IsNullOrWhiteSpace(event_category))
            { parameters["event_category"] = event_category; }

            if (!String.IsNullOrWhiteSpace(event_name))
            { parameters["event_name"] = event_name; }

            if (!String.IsNullOrWhiteSpace(type))
            { parameters["type"] = type; }

            parameters["level"] = App.Current.StateData.CurrentLevel.ToString();
            parameters["hc_bal"] = App.Current.StateData.Coins.ToString();

            if (!String.IsNullOrWhiteSpace(hc_amt))
            { parameters["hc_amt"] = hc_amt; }

            DateTime utcNow = DateTime.UtcNow;
            parameters["startdate"] = utcNow.Date.ToString("d");
            parameters["starttime"] = utcNow.ToString("HH:mm:ss");

            return concatenateParameters(parameters, ";;;");
        }

        private Dictionary<string, string> generateInstrumentationParameters(
            string event_category,
            string event_name,
            string type,
            string sub_type,
            string hc_amt)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters["appName"] = this.AppName;
            parameters["flight"] = App.Current.ConfigData.flight.ToString();
            parameters["version"] = "1";
            parameters["uid"] = getDeviceUniqueId();

            string deviceName = DeviceStatus.DeviceName;
            if (!String.IsNullOrWhiteSpace(deviceName))
            {   parameters["device"] = DeviceStatus.DeviceName; }

            string osVersion = Environment.OSVersion.Version.ToString();
            if (!String.IsNullOrWhiteSpace(osVersion))
            {   parameters["ios"] = osVersion;  }

            parameters["log"] = generateLogString(
                event_category,
                event_name,
                type,
                sub_type,
                hc_amt);

            return parameters;
        }

        public string getInstrumentationUri(
            string event_category,
            string event_name,
            string type,
            string sub_type,
            string hc_amt)
        {
            Dictionary<string, string> parameters = generateInstrumentationParameters(
                event_category,
                event_name,
                type,
                sub_type,
                hc_amt);
            
            string hash = generateHashForInstrumentation(getDeviceUniqueId(), parameters);

            string uri = this.Url + "?" +
                concatenateParameters(parameters, "&") +
                "&hash=" + hash;

            return uri;
        }

        public void sendInstrumentation(
            string event_category,
            string event_name,
            string type,
            string sub_type,
            string hc_amt)
        {
            string uri = getInstrumentationUri(
                event_category,
                event_name,
                type,
                sub_type,
                hc_amt);

            WebClient client = new WebClient();
            client.DownloadStringCompleted += client_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri(uri));
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs args)
        {
            bool success = false;
            if (!args.Cancelled && args.Error == null)
            {
                if (args.Result.StartsWith("status=ok;;;"))
                {
                    success = true;
                }
            }

            if (!success)
            {
                System.Diagnostics.Debug.WriteLine("sendInstrumentation: failed");
            }
        }
    }
}
