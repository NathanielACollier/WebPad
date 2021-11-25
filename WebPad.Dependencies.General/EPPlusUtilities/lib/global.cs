using OfficeOpenXml;

namespace WebPad.Dependencies.General.EPPlusUtilities.lib
{
    public static class global
    {

        public static void setup()
        {
            OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }
    }

}