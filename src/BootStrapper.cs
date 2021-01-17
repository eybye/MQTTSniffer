using MQTTSniffer.Extensions;
using MQTTSniffer.Services;
using Splat;

namespace MQTTSniffer
{
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            // These .InitializeX() methods will add ReactiveUI platform 
            // registrations to your container. They MUST be present if
            // you *override* the default Locator.
            services.RegisterConstant<ILogger>(new Splat.ConsoleLogger());
            services.RegisterConstant<IExtensionImporter>(new ExtensionImporter());
            services.RegisterConstant<IIEncoderDecoderService>(new EncoderDecoderService(resolver.GetService<IExtensionImporter>()));
        }
        public static IReadonlyDependencyResolver ServiceProvider
        {
            get
            {
                return Splat.Locator.Current;
            }
        }
    }
}