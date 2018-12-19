using ItRollingOut.Reflection;
using System;
using System.Collections.Generic;

namespace ItRollingOut.CustomIoc
{
    public static class Plugins
    {
        public static List<IEasyPlugin> FindAllPlugins()
        {
            var typesWithMyAttribute = ReflectionHelpers.FindAllWithAttribute(typeof(EasyPluginAttribute), true);
            var plugins = new List<IEasyPlugin>();
            foreach (var plugType in typesWithMyAttribute)
            {
                try
                {
                    plugins.Add((IEasyPlugin)Activator.CreateInstance(plugType));
                }
                catch
                {
                    // ignored
                }
            }

            return plugins;
        }

        public static void InitPluginsWith(IIocSystem ioc, IEnumerable<IEasyPlugin> plugins)
        {
            foreach (var plug in plugins)
            {
                plug.Init(ioc);
            }
        }
    }
}
