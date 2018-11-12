using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace OneScript.WebHost.Infrastructure
{
    public static class SwaggerServicePlugin
    {
        public static void AddOneScriptSwagger(this IServiceCollection services, IConfiguration conf)
        {
            services.AddSwaggerGen(options =>
            {
                var info = LoadFromConfig(conf);
                options.SwaggerDoc(info.Version, info);
            });
        }

        private static Info LoadFromConfig(IConfiguration config)
        {

            var infoSection = config?.GetSection("Info");

            Info info = new Info
            {
                Version = GetSectionValue(infoSection, "version", "v1"),
                Title = GetSectionValue(infoSection, "name", "OneScipt API"),
                Description = GetSectionValue(infoSection, "description"),
                TermsOfService = GetSectionValue(infoSection, "terms")
            };

            var licSection = infoSection?.GetSection("License");

            if (licSection != null)
            {
                License lic = new License
                {
                    Name = GetSectionValue(licSection, "name"),
                    Url = GetSectionValue(licSection, "url")
                };
                info.License = lic;
            }

            var contactSection = infoSection?.GetSection("Contact");

            if (contactSection != null)
            {
                Contact contact = new Contact
                {
                    Name = GetSectionValue(contactSection, "name"),
                    Email = GetSectionValue(contactSection, "email"),
                    Url = GetSectionValue(contactSection, "url")
                };
                info.Contact = contact;
            }

            return info;
        }

        private static string GetSectionValue(IConfigurationSection section, string name, string defaultValue = null)
        {
            return section?.GetValue<string>(name) ?? defaultValue;
        }
    }
}
