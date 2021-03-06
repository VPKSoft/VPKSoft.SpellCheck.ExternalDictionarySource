﻿#region License
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

using System;
using System.Diagnostics;
using System.Windows.Forms;
using VPKSoft.ExternalDictionaryPackage;

namespace DictionaryPackageTestApplication
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void lbSpdxLicense_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://spdx.org/licenses/");
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void btVPKSoft_Click(object sender, EventArgs e)
        {
            tbName.Text = @"Voikko";
            tbLibrary.Text = @"VoikkoSharp.dll";
            tbCompany.Text = @"VPKSoft";
            tbCopyright.Text = @"Copyright © VPKSoft " + DateTime.Now.Year;
            tbCulture.Text = @"fi";
            tbCultureDescription.Text = @"Finnish";
            // ReSharper disable once StringLiteralTypo (Finnish)
            tbCultureDescriptionNative.Text = @"Suomi";
            tbSpdxLicense.Text = @"MIT";
            tbUrl.Text = @"https://www.vpksoft.net";
        }

        private void DisplayXmlData(string fileName, bool fromXmlFile)
        {
            try
            {
                var data = fromXmlFile
                    ? DictionaryPackage.GetXmlDefinitionDataFromDefinitionFile(fileName)
                    : DictionaryPackage.GetXmlDefinitionData(fileName);
                tbName.Text = data.name;
                tbLibrary.Text = data.lib;
                tbCompany.Text = data.company;
                tbCopyright.Text = data.copyright;
                tbCulture.Text = data.cultureName;
                tbCultureDescription.Text = data.cultureDescription;
                tbCultureDescriptionNative.Text = data.cultureDescriptionNative;
                tbSpdxLicense.Text = data.spdxLicenseId;
                tbUrl.Text = data.url;
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void DisplayException(Exception exception)
        {
            MessageBox.Show(this, $@"Error occurred: '{exception.Message}'.", @"Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void mnuGenerateXmlDefinition_Click(object sender, EventArgs e)
        {
            try
            {
                if (odDll.ShowDialog() == DialogResult.OK)
                {
                    var result = DictionaryPackage.GenerateXmlDefinition(odDll.FileName, tbName.Text,
                        tbCompany.Text, tbCopyright.Text, tbCulture.Text, tbCultureDescription.Text,
                        tbCultureDescriptionNative.Text, tbSpdxLicense.Text, tbUrl.Text);

                    DisplayXmlData(result, true);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void generateADictionaryPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (odDll.ShowDialog() == DialogResult.OK)
                {
                    var dll = odDll.FileName;
                    fbFolder.Description = @"Select library location";
                    if (fbFolder.ShowDialog() == DialogResult.OK)
                    {
                        var input = fbFolder.SelectedPath;
                        fbFolder.Description = @"Select package output folder";
                        if (fbFolder.ShowDialog() == DialogResult.OK)
                        {
                            var output = fbFolder.SelectedPath;
                            var result = DictionaryPackage.CreatePackage(input, output, dll);
                            MessageBox.Show(this, $@"DictionaryPackage.CreatePackage call: '{result}'.", @"Result",
                                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void testInstallADictionaryPackageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (odZip.ShowDialog() == DialogResult.OK)
                {
                    var zip = odZip.FileName;
                    fbFolder.Description = @"Select install folder";
                    if (fbFolder.ShowDialog() == DialogResult.OK)
                    {
                        var output = fbFolder.SelectedPath;
                        var result = DictionaryPackage.InstallPackage(zip, output);
                        DisplayXmlData(result, true);
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void mnuDisplayXmlData_Click(object sender, EventArgs e)
        {
            try
            {
                if (odXml.ShowDialog() == DialogResult.OK)
                {
                    var xml = odXml.FileName;
                    DisplayXmlData(xml, true);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void lbSpdxLicenseLinkValue_Click(object sender, EventArgs e)
        {
            try
            {
                var link = ((Label) sender).Text;
                if (link.StartsWith("http"))
                {
                    Process.Start(link);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }

        private void tbSpdxLicense_TextChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox) sender;
            lbSpdxLicenseLinkValue.Text = DictionaryPackage.GetSpdxLicenseUrl(textBox.Text);
        }

        private void mnuUnInstall_Click(object sender, EventArgs e)
        {
            try
            {
                if (odXml.ShowDialog() == DialogResult.OK)
                {
                    var xml = odXml.FileName;
                    DictionaryPackage.UnInstallPackage(xml);
                }
            }
            catch (Exception ex)
            {
                DisplayException(ex);
            }
        }
    }
}
