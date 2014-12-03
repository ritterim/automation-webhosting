using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimDev.Automation.WebHosting.Utilities;

namespace RimDev.Automation.WebHosting
{
    public class HostConfiguration
    {
        private HostConfiguration(XDocument appHostConfig)
        {
            Document = appHostConfig;
        }

        public static async Task<HostConfiguration> LoadAsync(string appHostConfigPath)
        {
            Check.NotNull(appHostConfigPath, "appHostConfigPath");

            using (var fileStream = new FileStream(appHostConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
            using (var textReader = new StreamReader(fileStream))
            {
                return await LoadAsync(textReader);
            }
        }

        public static async Task<HostConfiguration> LoadAsync(TextReader textReader)
        {
            Check.NotNull(textReader, "textReader");

            var content = await textReader.ReadToEndAsync().ConfigureAwait(false);
            var document = XDocument.Parse(content);
            return new HostConfiguration(document);
        }

        protected XDocument Document { get; private set; }

        public void CreateSite(string name, string physicalPath, int httpPort, int httpsPort, string virtualPath = "/")
        {
            Check.NotNull(name, "name");
            Check.NotNull(physicalPath, "physicalPath");

            var sites = Document.Root
                .Element("system.applicationHost")
                .Element("sites");

            var lastId = sites.Elements("site").Select(x => (int?)x.Attribute("id")).Max();

            sites.Add(
                new XElement("site",
                    new XAttribute("id", lastId.GetValueOrDefault() + 1),
                    new XAttribute("name", name),
                    new XElement("application",
                        new XAttribute("path", virtualPath),
                        new XAttribute("applicationPool", /* TODO: Make this configurable */ "Clr4IntegratedAppPool"),
                        new XElement("virtualDirectory",
                            new XAttribute("path", virtualPath),
                            new XAttribute("physicalPath", physicalPath))
                    ),
                    new XElement("bindings",
                        new XElement("binding",
                            new XAttribute("protocol", Uri.UriSchemeHttp),
                            new XAttribute("bindingInformation", string.Format("*:{0}:localhost", httpPort))
                        ),
                        new XElement("binding",
                            new XAttribute("protocol", Uri.UriSchemeHttps),
                            new XAttribute("bindingInformation", string.Format("*:{0}:localhost", httpsPort))
                        )
                    )
                )
            );
        }

        public void RemoveSite(string name)
        {
            Check.NotNull(name, "name");

            Document.Root
                .Element("system.applicationHost")
                .Element("sites")
                .Elements()
                .Where(x => string.Equals(name, (string)x.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                .Remove();
        }

        public async Task SaveChanges(string filePath)
        {
            Check.NotNull(filePath, "filePath");

            Document.Save(filePath, SaveOptions.OmitDuplicateNamespaces);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous))
            using (var textWriter = new StreamWriter(fileStream))
            {
                await SaveChanges(textWriter);
                await textWriter.FlushAsync();
            }
        }

        public async Task SaveChanges(TextWriter textWriter)
        {
            Check.NotNull(textWriter, "textWriter");

            using (var xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings { Async = true, Indent = true }))
            {
                Document.WriteTo(xmlWriter);
                await xmlWriter.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
