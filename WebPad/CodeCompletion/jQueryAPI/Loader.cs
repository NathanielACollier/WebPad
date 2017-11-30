using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using WebPad.Utilities;

namespace WebPad.CodeCompletion.jQueryAPI
{
    public class Loader
    {
        public JQueryDocs LoadFromResource()
        {
            using (var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WebPad.Resources.jQuery1.6.4_api.xml"))
            {
                return Load(manifestResourceStream);
            }
        }

        public JQueryDocs Load(Stream stream)
        {
            var docs = new JQueryDocs();
            using (var xmlResource = new StreamReader(stream))
            {
                var xDoc = XDocument.Load(xmlResource);
                xDoc.Descendants("entry")
                    .Select(i => CreateEntry(i))
                    .Apply(i => docs.Entries.Add(i))
                    .ToList();
            }
            return docs;
        }

        private static Entry CreateEntry(XElement xElement)
        {
            var entry = new Entry(
                    xElement.Attribute("name").DefaultValueIfNull("unknown"),
                    MapMemberTypeString(xElement.Attribute("type").DefaultValueIfNull("unknown")),
                    xElement.Attribute("return").DefaultValueIfNull("void"),
                    xElement.Element("desc").DefaultValueIfNull(String.Empty),
                    xElement.Element("longdesc").InnerXml()
                );
            CreateEntrySignatures(xElement, entry);
            CreateEntryExamples(xElement, entry);
            return entry;
        }

        private static void CreateEntryExamples(XElement xElement, Entry entry)
        {
            var xExamples = xElement.Descendants("example");
            foreach (var xExample in xExamples)
            {
                var example = new Example(
                    xExample.Element("desc").DefaultValueIfNull("No description"),
                    xExample.Element("css").DefaultValueIfNull(String.Empty),
                    xExample.Element("html").DefaultValueIfNull(String.Empty),
                    xExample.Element("code").DefaultValueIfNull(String.Empty));
                xExample.Descendants("js").Apply(
                    x => example.JavascriptReferences.Add(x.Attribute("src").DefaultValueIfNull(String.Empty)));
                entry.Examples.Add(example);
            }
        }

        private static void CreateEntrySignatures(XElement xElement, Entry entry)
        {
            var xSignatures = xElement.Descendants("signature");
            foreach(var xSignature in xSignatures)
            {
                var signature = new Signature(
                    xSignature.Element("added").DefaultValueIfNull(String.Empty));
                CreateSignatureArguments(xSignature, signature);
                entry.Signatures.Add(signature);
            }
        }

        private static void CreateSignatureArguments(XElement xSignature, Signature signature)
        {
            var xArguments = xSignature.Elements("argument");
            foreach (var xArgument in xArguments)
            {
                var argument = new Argument(
                        xArgument.Attribute("name").DefaultValueIfNull("unknown"),
                        xArgument.Attribute("type").DefaultValueIfNull("unknown"),
                        Boolean.Parse(xArgument.Attribute("optional").DefaultValueIfNull("false")),
                        xArgument.Element("desc").DefaultValueIfNull(String.Empty)
                    );
                signature.Arguments.Add(argument);
            }
        }

        private static MemberType MapMemberTypeString(string memberType)
        {
            switch (memberType.ToLower())
            {
                case "method":
                    return MemberType.Method;
                case "property":
                    return MemberType.Property;
                case "selector":
                    return MemberType.Selector;
                case "template-tag":
                    return MemberType.TemplateTag;
                default:
                    throw new ArgumentException(String.Format("The memberType '{0}' is not recognised.", memberType), "memberType");
            }
        }
    }
}