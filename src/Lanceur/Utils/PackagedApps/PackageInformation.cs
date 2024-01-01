using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Xml.Linq;
using Windows.ApplicationModel;

namespace Lanceur.Utils.PackagedApps
{
    internal static class PackageInformation
    {
        #region Enums

        private enum Hresult : uint
        {
            Ok = 0x0000,
        }

        [Flags]
        private enum Stgm : uint
        {
            Read = 0x0,
            DenyWrite = 0x20,
        }

        public enum PackageVersion
        {
            Windows10,
            Windows81,
            Windows8,
            Unknown
        }

        #endregion Enums

        #region Methods

        private static string Get(Package package, Func<AppxPackageHelper.IAppxManifestApplication, string> visitor)
        {
            var helper = new AppxPackageHelper();
            var path = Path.Combine(package.InstalledLocation.Path, "AppxManifest.xml");

            const uint noAttribute = 0x80;
            const Stgm exclusiveRead = Stgm.Read | Stgm.DenyWrite;
            var hResult = SHCreateStreamOnFileEx(path, exclusiveRead, noAttribute, false, null, out IStream stream);

            if (hResult != Hresult.Ok) return string.Empty;

            var apps = helper.GetAppsFromManifest(stream);

            return apps.Count > 0 ? visitor(apps[0]) : string.Empty;

        }

        private static PackageVersion InitPackageVersion(string[] namespaces)
        {
            var versionFromNamespace = new Dictionary<string, PackageVersion>
            {
                {"http://schemas.microsoft.com/appx/manifest/foundation/windows10", PackageVersion.Windows10},
                {"http://schemas.microsoft.com/appx/2013/manifest", PackageVersion.Windows81},
                {"http://schemas.microsoft.com/appx/2010/manifest", PackageVersion.Windows8},
            };

            foreach (var n in versionFromNamespace.Keys)
            {
                if (namespaces.Contains(n))
                {
                    return versionFromNamespace[n];
                }
            }
            return PackageVersion.Unknown;
        }

        private static string LogoUriFromManifest(AppxPackageHelper.IAppxManifestApplication app, PackageVersion version)
        {
            var logoKeyFromVersion = new Dictionary<PackageVersion, string>
                {
                    { PackageVersion.Windows10, "Square44x44Logo" },
                    { PackageVersion.Windows81, "Square30x30Logo" },
                    { PackageVersion.Windows8, "SmallLogo" },
                };
            if (logoKeyFromVersion.ContainsKey(version))
            {
                var key = logoKeyFromVersion[version];
                app.GetStringValue(key, out string logoUri);
                return logoUri;
            }
            else
            {
                return string.Empty;
            }
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern Hresult SHCreateStreamOnFileEx(string fileName, Stgm grfMode, uint attributes, bool create, IStream reserved, out IStream stream);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern Hresult SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, uint cchOutBuf, IntPtr ppvReserved);

        /// http://www.hanselman.com/blog/GetNamespacesFromAnXMLDocumentWithXPathDocumentAndLINQToXML.aspx
        private static string[] XmlNamespaces(string path)
        {
            var document = XDocument.Load(path);
            if (document.Root == null) return Array.Empty<string>();

            var namespaces = document.Root.Attributes().
                               Where(a => a.IsNamespaceDeclaration).
                               GroupBy(
                                   a => a.Name.Namespace == XNamespace.None ? string.Empty : a.Name.LocalName,
                                   a => XNamespace.Get(a.Value)
                               ).Select(
                                   g => g.First().ToString()
                               ).ToArray();
            return namespaces;
        }

        internal static string GetAppUserModelId(Package package)
        {
            return Get(package, app =>
            {
                app.GetAppUserModelId(out var result);
                return result;
            });
        }

        internal static string GetIcon(Package package)
        {
            return Get(package, app =>
            {
                var path = Path.Combine(package.InstalledLocation.Path, "AppxManifest.xml");
                var namespaces = XmlNamespaces(path);
                var version = InitPackageVersion(namespaces);
                var location = Path.GetDirectoryName(path);

                var relativeUri = LogoUriFromManifest(app, version);
                var theme = ShouldSystemUseDarkMode() ? "contrast-black" : "contrast-light";

                return LogoPathFromUri(relativeUri, location, theme, version);
            });
        }

        internal static string LogoPathFromUri(string uri, string location, string theme, PackageVersion version)
        {
            // all https://msdn.microsoft.com/windows/uwp/controls-and-patterns/tiles-and-notifications-app-assets
            // windows 10 https://msdn.microsoft.com/en-us/library/windows/apps/dn934817.aspx
            // windows 8.1 https://msdn.microsoft.com/en-us/library/windows/apps/hh965372.aspx#target_size
            // windows 8 https://msdn.microsoft.com/en-us/library/windows/apps/br211475.aspx
            string path;
            if (uri.Contains("\\"))
            {
                path = Path.Combine(location, uri);
            }
            else
            {
                // for C:\Windows\MiracastView etc
                path = Path.Combine(location, "Assets", uri);
            }

            var extension = Path.GetExtension(path);
            if (extension != null)
            {
                var end = path.Length - extension.Length;
                var prefix = path.Substring(0, end);
                var paths = new List<string> { path };

                var scaleFactors = new Dictionary<PackageVersion, List<int>>
                    {
                        // scale factors on win10: https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-app-assets#asset-size-tables,
                        { PackageVersion.Windows10, new List<int> { 100, 125, 150, 200, 400 } },
                        { PackageVersion.Windows81, new List<int> { 100, 120, 140, 160, 180 } },
                        { PackageVersion.Windows8, new List<int> { 100 } }
                    };

                if (scaleFactors.ContainsKey(version))
                {
                    foreach (var factor in scaleFactors[version])
                    {
                        paths.Add($"{prefix}.scale-{factor}{extension}");
                        paths.Add($"{prefix}.scale-{factor}_{theme}{extension}");
                        paths.Add($"{prefix}.{theme}_scale-{factor}{extension}");
                    }
                }

                paths = paths.OrderByDescending(x => x.Contains(theme)).ToList();
                var selected = paths.FirstOrDefault(File.Exists);
                if (!string.IsNullOrEmpty(selected))
                {
                    return selected;
                }

                var appIconSize = 36;
                var targetSizes = new List<int> { 16, 24, 30, 36, 44, 60, 72, 96, 128, 180, 256 }.AsParallel();
                var pathFactorPairs = new Dictionary<string, int>();

                foreach (var factor in targetSizes)
                {
                    var simplePath      = $"{prefix}.targetsize-{factor}{extension}";
                    var suffixThemePath = $"{prefix}.targetsize-{factor}_{theme}{extension}";
                    var prefixThemePath = $"{prefix}.{theme}_targetsize-{factor}{extension}";

                    paths.Add(simplePath);
                    paths.Add(suffixThemePath);
                    paths.Add(prefixThemePath);

                    pathFactorPairs.Add(simplePath, factor);
                    pathFactorPairs.Add(suffixThemePath, factor);
                    pathFactorPairs.Add(prefixThemePath, factor);
                }

                paths = paths.OrderByDescending(x => x.Contains(theme)).ToList();
                var selectedIconPath = paths.OrderBy(x =>
                {
                    var val = pathFactorPairs.ContainsKey(x) ? pathFactorPairs[x] : default;
                    return Math.Abs(val - appIconSize);
                }).FirstOrDefault(File.Exists);

                return !string.IsNullOrEmpty(selectedIconPath) ? selectedIconPath : string.Empty;
            }

            return string.Empty;
        }

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool ShouldSystemUseDarkMode();

        #endregion Methods
    }
}