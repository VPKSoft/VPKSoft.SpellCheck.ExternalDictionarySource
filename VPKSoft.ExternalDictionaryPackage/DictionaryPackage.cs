#region License
/*
MIT License

Copyright(c) 2021 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ionic.Zip;

namespace VPKSoft.ExternalDictionaryPackage
{
    /// <summary>
    /// A class to help package external dictionary libraries for VPKSoft.SpellCheckUtility and VPKSoft.ScintillaSpellCheck spell checking libraries.
    /// </summary>
    public class DictionaryPackage
    {
        /// <summary>
        /// The dictionary XML file end part.
        /// </summary>
        private const string DictionaryXmlFileEndPart = ".Dictionary.xml";

        /// <summary>
        /// The URL start part for SPDX licenses at nuget.org.
        /// </summary>
        private const string NuGetLicenseUrl = "https://licenses.nuget.org/";

        /// <summary>
        /// Creates an external dictionary package from a specified path. The package must contain at least a definition XML (SpellCheckLibrary.xml) and an assembly referenced in the XML file.
        /// </summary>
        /// <param name="fromPath">The path where the dictionary package data exists.</param>
        /// <param name="toPath">The path where the compressed dictionary package (Zip) should be placed at.</param>
        /// <param name="assemblyDllFile">The assembly file containing the spell checking library interface.</param>
        /// <returns>The compressed (Zip) file name containing the created custom library package.</returns>
        /// <exception cref="System.IO.FileNotFoundException">The assembly file or the definition XML file does not exist.</exception>
        public static string CreatePackage(string fromPath, string toPath, string assemblyDllFile)
        {
            if (!File.Exists(assemblyDllFile))
            {
                throw new FileNotFoundException(assemblyDllFile);
            }

            var packageName = Path.GetFileNameWithoutExtension(assemblyDllFile);

            string definitionFile = Path.Combine(fromPath, packageName + DictionaryXmlFileEndPart);

            if (!File.Exists(definitionFile))
            {
                throw new FileNotFoundException(definitionFile);
            }

            var files = Directory.GetFiles(fromPath, "*", SearchOption.AllDirectories);

            var zipFile = Path.Combine(toPath, packageName + ".zip");

            using (ZipFile zip = new ZipFile())
            {
                foreach (var file in files)
                {
                    var path = Path.GetDirectoryName(file)?.Remove(0, fromPath.Length).TrimStart('\\').TrimEnd('\\');
                    zip.AddFile(file, packageName + "\\" + path);
                }

                zip.Save(zipFile);
            }

            return zipFile;
        }

        /// <summary>
        /// Installs the package.
        /// </summary>
        /// <param name="packageFileName">Name of the package file.</param>
        /// <param name="pathTo">The path to.</param>
        /// <returns>System.String.</returns>
        public static string InstallPackage(string packageFileName, string pathTo)
        {
            var searchPath = string.Empty;
            using (ZipFile zip = ZipFile.Read(packageFileName))
            {
                foreach (ZipEntry entry in zip)
                {
                    if (searchPath == string.Empty)
                    {
                        searchPath = Path.GetDirectoryName(entry.FileName)?.Split('\\').FirstOrDefault() ?? string.Empty;
                    }
                }

                var deletePath = Path.Combine(pathTo, searchPath);
                if (Directory.Exists(deletePath) && searchPath != string.Empty)
                {
                    Directory.Delete(deletePath, true);
                }

                zip.ExtractAll(pathTo);
            }

            var directory = Path.Combine(pathTo, searchPath);
            var info = new DirectoryInfo(directory);
            var files = info.GetFiles("*" + DictionaryXmlFileEndPart, SearchOption.AllDirectories);

            return files.FirstOrDefault()?.FullName;
        }

        /// <summary>
        /// Removes the installed package from a specified path of the XML definition file.
        /// </summary>
        /// <param name="xmlDefinitionFile">The XML definition file.</param>
        /// <returns><c>true</c> if the package was successfully removed, <c>false</c> otherwise.</returns>
        public static bool UnInstallPackage(string xmlDefinitionFile)
        {
            var deletePath = Path.GetDirectoryName(xmlDefinitionFile);
            if (Directory.Exists(deletePath))
            {
                Directory.Delete(deletePath, true);
                return true;
            }

            return false;
        }

        // ReSharper disable twice CommentTypo
        /// <summary>
        /// Generates the XML definition file for a custom dictionary package.
        /// </summary>
        /// <param name="assemblyDllFile">The assembly DLL file of the custom dictionary interface.</param>
        /// <param name="name">The name for the custom dictionary interface.</param>
        /// <param name="company">The company for the custom dictionary interface.</param>
        /// <param name="copyright">The copyright for the custom dictionary interface.</param>
        /// <param name="cultureName">Name of the culture (languagecode2-country/regioncode2) for the custom dictionary interface.</param>
        /// <param name="cultureDescription">The culture description in English for the custom dictionary interface.</param>
        /// <param name="cultureDescriptionNative">The culture description in native language for the custom dictionary interface.</param>
        /// <param name="spdxLicenseId">The SPDX license identifier (See: https://spdx.org/licenses/).</param>) for the custom dictionary interface.
        /// <param name="url">The URL for the custom dictionary interface.</param>
        /// <returns>The name of the generated XML file.</returns>
        public static string GenerateXmlDefinition(string assemblyDllFile, string name, string company, 
            string copyright, string cultureName, string cultureDescription, 
            string cultureDescriptionNative, string spdxLicenseId, string url)
        {
            var packageName = Path.GetFileNameWithoutExtension(assemblyDllFile);
            string definitionFile = Path.Combine(Path.GetDirectoryName(assemblyDllFile) ?? string.Empty, packageName + DictionaryXmlFileEndPart);

            if (File.Exists(definitionFile))
            {
                File.Delete(definitionFile);
            }

            // create an element for the data..
            XElement documentElement = 
                new XElement("SpellCheckerInterface", 
                    new XAttribute("name", name),
                    new XAttribute("lib", Path.GetFileName(assemblyDllFile)),
                    new XElement("Company", new XAttribute("value", company)),
                    new XElement("CopyrightText", new XAttribute("value", copyright)),
                    // ReSharper disable twice StringLiteralTypo
                    new XComment("The culture name in the format languagecode2-country/regioncode2."),
                    new XElement("CultureName", new XAttribute("value", cultureName)),
                    new XComment("The culture description both in English and in native language."),
                    new XElement("CultureDescription", new XAttribute("value", cultureDescription)),
                    new XElement("CultureDescriptionNative", new XAttribute("value", cultureDescriptionNative)),
                    new XComment("See: https://spdx.org/licenses/"),
                    new XElement("LicenseSpdx", new XAttribute("value", spdxLicenseId)),
                    new XElement("Url", new XAttribute("value", url)));


            var document = new XDocument(new XDeclaration("1.0", "utf-8", ""), documentElement);

            document.Save(definitionFile);

            return definitionFile;
        }

        /// <summary>
        /// Gets the XML definition data from a custom dictionary package.
        /// </summary>
        /// <param name="assemblyDllFile">The assembly DLL file name.</param>
        /// <returns>A (System.String name, System.String lib, System.String company, System.String copyright, System.String cultureName, System.String cultureDescription, System.String cultureDescriptionNative, System.String spdxLicenseId, System.String url) containing the data.</returns>
        public static (string name, string lib, string company,
            string copyright, string cultureName, string cultureDescription,
            string cultureDescriptionNative, string spdxLicenseId, string url)
            GetXmlDefinitionData(string assemblyDllFile)
        {
            var packageName = Path.GetFileNameWithoutExtension(assemblyDllFile);
            var definitionFile = Path.Combine(Path.GetDirectoryName(assemblyDllFile) ?? string.Empty, packageName + DictionaryXmlFileEndPart);
            return GetXmlDefinitionDataFromDefinitionFile(definitionFile);
        }

        /// <summary>
        /// Gets the XML definition data from a specified definition file.
        /// </summary>
        /// <param name="xmlDefinitionFile">The XML definition file name.</param>
        /// <returns>A (System.String name, System.String lib, System.String company, System.String copyright, System.String cultureName, System.String cultureDescription, System.String cultureDescriptionNative, System.String spdxLicenseId, System.String url) containing the data.</returns>
        /// <exception cref="InvalidDataException">The XML file is not valid.</exception>
        public static (string name, string lib, string company,
                            string copyright, string cultureName, string cultureDescription,
                            string cultureDescriptionNative, string spdxLicenseId, string url)
                        GetXmlDefinitionDataFromDefinitionFile(string xmlDefinitionFile)
        {

            var file = File.OpenText(xmlDefinitionFile);
            var document = XDocument.Load(file);
            file.Dispose();
            var name = document.Root?.Attribute("name")?.Value;
            var lib = document.Root?.Attribute("lib")?.Value;

            if (name == null || lib == null)
            {
                throw new InvalidDataException("The XML file is not valid.");
            }

            var company = document.Root?.Element("Company")?.Attribute("value")?.Value;
            var copyright = document.Root?.Element("CopyrightText")?.Attribute("value")?.Value;
            var cultureName = document.Root?.Element("CultureName")?.Attribute("value")?.Value;
            var cultureDescription = document.Root?.Element("CultureDescription")?.Attribute("value")?.Value;
            var cultureDescriptionNative = document.Root?.Element("CultureDescriptionNative")?.Attribute("value")?.Value;
            var spdxLicenseId = document.Root?.Element("LicenseSpdx")?.Attribute("value")?.Value;
            var url = document.Root?.Element("Url")?.Attribute("value")?.Value;

            return (name, lib, company, copyright, cultureName, cultureDescription, cultureDescriptionNative,
                spdxLicenseId, url);
        }

        /// <summary>
        /// Gets the SPDX license URL (https://licenses.nuget.org/).
        /// </summary>
        /// <param name="spdxLicenseId">The SPDX license identifier.</param>
        /// <returns>A string containing an URL to the specified license.</returns>
        public static string GetSpdxLicenseUrl(string spdxLicenseId)
        {
            return NuGetLicenseUrl + spdxLicenseId;
        }
    }
}
