using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using runWPF = nac.wpf.utilities.Run.Run;
using System.Threading.Tasks;
using controls = WebPad.Dependencies.General.WPFUserControls;
using System.Linq;

namespace WebPad.Dependencies.General.Tests.WPFUserControls.LocalFolderBrowser
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task BasicControlDisplayTest()
        {
            var result = await runWPF.runWithUIThread(win =>
            {
                var browser = new controls.LocalFolderBrowser.LocalFolderBrowser();
                win.Content = browser;
            });

            Assert.IsTrue(result.IsError == false);
        }


        [TestMethod]
        public void FileExtensionParsingTest()
        {
            var browser = new controls.LocalFolderBrowser.LocalFolderBrowser();
            browser.FileExtensions = "";
            Assert.IsTrue(browser.FileExtensionsList.Count() == 0 && browser.FileExtensionsList.Any() == false, "List failed to work when empty");

            browser.FileExtensions = "php csv txt;xml; ps1, bat";
            Assert.IsTrue(browser.FileExtensionsList.Count() == 6, "Failed to split the correct amount of extensions");
            Assert.IsTrue(string.Equals("ps1", browser.FileExtensionsList.ElementAt(4)), "Failed to identify 4th element as ps1");

            browser.FileExtensions = ".txt, .apple; .fbi.test,xml";
            Assert.IsTrue(browser.FileExtensionsList.Count() == 4, "Failed to split the correct amount x2");
            Assert.IsTrue(string.Equals("fbi.test", browser.FileExtensionsList.ElementAt(2)), "Failed to get rid of leading '.' ");
        }


        [TestMethod]
        public async Task ControlDisplayWithFileExtensionFilter()
        {
            var result = await runWPF.runWithUIThread(win =>
            {
                var browser = new controls.LocalFolderBrowser.LocalFolderBrowser();
                win.Content = browser;

                browser.FileExtensions = "php bat";
            });

            Assert.IsTrue(result.IsError == false);
        }
    }
}
