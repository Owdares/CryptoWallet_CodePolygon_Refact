using Serilog.Core;
using Serilog.Events;

namespace Blaved.Core.Utility
{
    public class RemovePropertiesEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent le, ILogEventPropertyFactory lepf)
        {
            le.RemovePropertyIfPresent("RequestPath");
            le.RemovePropertyIfPresent("ConnectionId");
            le.RemovePropertyIfPresent("ActionId");
            le.RemovePropertyIfPresent("ActionName");
        }
    }


}
