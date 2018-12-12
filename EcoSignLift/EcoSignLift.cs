using Asphalt;
using Asphalt.Api.Event;
using Asphalt.Service;
using Asphalt.Storeable;
using Eco.Core.Plugins.Interfaces;

namespace EcoSignLift
{
    [AsphaltPlugin]
    public class EcoSignLift : IModKitPlugin
    {
        [Inject]
        [StorageLocation("config")]
        [DefaultValues(nameof(GetConfigValues))]
        public static IStorage Config { get; set; }

        public void OnEnable()
        {
            EventManager.RegisterListener(new SignEventHandler());
        }

        public static KeyDefaultValue[] GetConfigValues()
        {
            return new KeyDefaultValue[]
            {
               new KeyDefaultValue("MaximumLiftHeight", 5000),
               new KeyDefaultValue("RequireItemNextToSign", false),
               new KeyDefaultValue("NameOfRequiredItemNextToSign", "FlatSteel"),
            };
        }

        public string GetStatus()
        {
            return "OK";
        }
    }
}
