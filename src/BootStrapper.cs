using MQTTSniffer.Extensions;
using MQTTSniffer.Services;
using Serilog;
using Splat;
using Splat.Serilog;

namespace MQTTSniffer
{
    public static class Bootstrapper
    {
        public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
        {
            // These .InitializeX() methods will add ReactiveUI platform 
            // registrations to your container. They MUST be present if
            // you *override* the default Locator.
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            services.UseSerilogFullLogger();
            //services.RegisterConstant<ILogger>(new Splat.ConsoleLogger());
            services.RegisterConstant<IExtensionImporter>(new ExtensionImporter(resolver.GetService<ILogManager>().GetLogger<ExtensionImporter>()));
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