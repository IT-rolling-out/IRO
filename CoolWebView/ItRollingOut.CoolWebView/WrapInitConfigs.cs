using S2A.Plugins.WebViewSuite.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace S2A.Plugins.WebViewSuite
{
    public class WrapInitConfigs:ICloneable
    {
        public List<Func<IWebViewWrapPlugin>> PluginFactories { get; set; }

        public WrapSettings Settings { get; set; }

        public List<Regex> SitesWithJsBridge { get; set; } = new List<Regex>() { new Regex("^") };

        public List<string> IncludedJs { get; set; }

        public Dictionary<string, Func<object>> JsInterfaces { get; set; } = new Dictionary<string, Func<object>>();       

        public Dictionary<string, object> CloneJsInterfaces()
        {
            var res = new Dictionary<string, object>();
            foreach (var item in JsInterfaces)
            {
                res.Add(
                    item.Key,
                    item.Value()
                );
            }
            return res;
        }

        public List<Regex> CloneSitesWithJsBridge()
        {
            return SitesWithJsBridge?.ToList();
        }

        public List<IWebViewWrapPlugin> ClonePlugins()
        {         
            if (PluginFactories == null)
                return null;
            var res = new List<IWebViewWrapPlugin>();
            foreach (var fac in PluginFactories)
            {
                res.Add(fac.Invoke());
            }
            return res;
        }

        public object Clone()
        {
            var res = new WrapInitConfigs();
            res.JsInterfaces = JsInterfaces.ToDictionary(x=>x.Key,x=>x.Value);
            res.PluginFactories = PluginFactories.ToList();
            res.Settings = Settings;
            res.SitesWithJsBridge = SitesWithJsBridge.ToList();
            return res;
        }
    }
}
