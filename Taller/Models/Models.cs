using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Taller.Models
{


    public class Configurations
    {
        private static System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
        public static string ServerUrl
        {
            get
            {
                return (string)settingsReader.GetValue("Url", typeof(String)) + "/";
            }
        }

        public static string HeaderName
        {
            get
            {
                return (string)settingsReader.GetValue("HeaderName", typeof(String));
            }
        }

        public static string HeaderValue
        {
            get
            {
                return (string)settingsReader.GetValue("HeaderValue", typeof(String));
            }
        }
    }

}