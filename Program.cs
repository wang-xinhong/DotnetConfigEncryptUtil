using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CN.Wangxinhong.DotnetConfigEncUtil
{
    class Program
    {
        //TODO: add diagnostics debug infomations

        private readonly static String XPATH_Configuration = "/configuration";
        private readonly static String XPATH_Configuration_ConfigSections = "/configuration/configSections";

        /// <summary>
        /// default encoding of the config file（utf-8）
        /// </summary>
        private readonly static Encoding DEFAULT_XML_Encoding = Encoding.UTF8;

        /// <summary>
        /// default protection provider（rsa protection provider）
        /// </summary>
        private readonly static String DEFAULT_ProtectProvider = "RsaProtectedConfigurationProvider";

        static void ThrowArgumentException()
        {
            Console.WriteLine(
                "Usage:      --configPath C:\\test\\web.config " +
                "            --configSection DatabaseConfiguration" +
                "            --mode encrypt/decrypt" 
              //+ "            --output C:\\test\\web.config.new "
              );
            throw new ArgumentException();
        }

        static int Main(string[] args)
        {
            // the config file path（web.config or app.config）
            String configPath = String.Empty;
            // config section to encrypt/decrypt
            String configSection = String.Empty;
            // encrypt or decrypt
            Boolean encryptionFlag = true;

            #region process the commandline arguments
            Arguments CommandLine = new Arguments(args);

            if (CommandLine["configPath"] != null)
                configPath = CommandLine["configPath"];
            else
                ThrowArgumentException();

            if (CommandLine["configSection"] != null)
                configSection = CommandLine["configSection"];
            else
                ThrowArgumentException();

            if (CommandLine["mode"] != null)
            {
                if (CommandLine["mode"].Equals("encrypt", StringComparison.InvariantCultureIgnoreCase))
                    encryptionFlag = true;
                else if (CommandLine["mode"].Equals("decrypt", StringComparison.InvariantCultureIgnoreCase))
                    encryptionFlag = false;
                else
                    ThrowArgumentException();
            }
            else
                ThrowArgumentException();
            #endregion

            // cut over header section list 
            var swapHeaderSectionsList = CutSelectedConfigSection(configPath);

            try
            {
                // encrypt the config path
                var fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = configPath;
                var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                var section = configuration.GetSection(configSection);

                if (section != null && encryptionFlag && !section.SectionInformation.IsProtected)
                {
                    section.SectionInformation.ProtectSection(DEFAULT_ProtectProvider);
                }
                else if (section != null && !encryptionFlag && section.SectionInformation.IsProtected)
                {
                    section.SectionInformation.UnprotectSection();
                }
                else
                {
                    if (section == null)
                        // no such section, unexpected
                        return (int)ReturnResult.CONFIG_SECTION_NOT_FOUND;
                    else
                        // to encrypt what already encrypted, or decrypt which plain text
                        // take no action, and nop to the file
                        return (int)ReturnResult.NOACTION_TAKEN;
                }

                configuration.Save(ConfigurationSaveMode.Minimal, true);
            }
            catch (Exception ex)
            {
                return (int)ReturnResult.EXCEPTION_DETECTED;
            }
            finally
            {
                // write back header Section List
                RestoreSwapedConfigSection(configPath, swapHeaderSectionsList);
            }
            return (int)ReturnResult.SUCCESS;
        }

        /// <summary>
        /// restore the swaped config section to file
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="swapHeaderSectionsList"></param>
        private static void RestoreSwapedConfigSection(String configPath, XmlNode swapHeaderSectionsList)
        {
            var xmlDocument = new XmlDocument();
            using (XmlTextReader xmlReader = new XmlTextReader(configPath))
            {
                xmlDocument.Load(xmlReader);
            }
            var headConfiguration = xmlDocument.SelectSingleNode(XPATH_Configuration);
            var currentSwapHeaderSectionsList = xmlDocument.ImportNode(swapHeaderSectionsList, deep: true);
            var headerSectionsList = xmlDocument.SelectSingleNode(XPATH_Configuration_ConfigSections);
            headConfiguration.ReplaceChild(currentSwapHeaderSectionsList, headerSectionsList);
            using (XmlTextWriter xmlWriter = new XmlTextWriter(configPath, DEFAULT_XML_Encoding))
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlDocument.Save(xmlWriter);
            }
        }

        /// <summary>
        /// cut over the selected config section and save to file
        /// </summary>
        /// <param name="configPath">config file path</param>
        /// <returns>the selected config section</returns>
        private static XmlNode CutSelectedConfigSection(String configPath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (XmlTextReader xmlReader = new XmlTextReader(configPath))
            {
                xmlDocument.Load(xmlReader);
            }
            var headerSectionsList = xmlDocument.SelectSingleNode(XPATH_Configuration_ConfigSections);
            if (headerSectionsList == null)
                return null;
            XmlNode swapHeaderSectionsList = headerSectionsList.CloneNode(deep: true);
            headerSectionsList.RemoveAll();
            using (XmlTextWriter xmlWriter = new XmlTextWriter(configPath, DEFAULT_XML_Encoding))
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlDocument.Save(xmlWriter);
            }
            return swapHeaderSectionsList;
        }
    }
}
