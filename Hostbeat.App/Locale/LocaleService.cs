using Hostbeat.Core.Interfaces;
using Windows.ApplicationModel.Resources;

namespace Hostbeat.Locale
{
    public class LocaleService : ILocale
    {
        private readonly ResourceLoader _resourceLoader;

        public LocaleService(ResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public string GetString(string nameId)
        {
            return _resourceLoader.GetString(nameId);
        }
    }
}
