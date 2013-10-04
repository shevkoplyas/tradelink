using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQFeedBroker
{
    public interface IQFPCI
    {
        string PC { get; }
    }

    internal class IQFPCIHelper
    {
        internal static IQFPCI get()
        {
            try
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var types = asm.GetTypes();
                var tgt = typeof(IQFPCI);
                foreach (var type in types)
                {
                    if (tgt.IsAssignableFrom(type) && (type != tgt))
                    {
                        try
                        {
                            IQFPCI ti = (IQFPCI)Activator.CreateInstance(type);
                            if ((ti == null) || string.IsNullOrWhiteSpace(ti.PC))
                                continue;
                            return ti;
                        }
                        catch 
                        {
                            return null;
                        }
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
