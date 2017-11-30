using System;
using System.Collections.Generic;

namespace WebPad.CodeCompletion.jQueryAPI
{
    public enum MemberType
    {
        Method,
        Property,
        Selector,
        TemplateTag
    }

    public class JQueryDocs
    {
        public JQueryDocs()
        {
            Entries = new List<Entry>();
        }

        public IList<Entry> Entries { get; private set; }
    }

    public class Entry
    {
        public Entry(string name, MemberType type, string returnType, string description, string longDescription)
        {
            Type = type;
            Name = name;
            ReturnType = returnType;
            Description = description;
            LongDescription = longDescription;
            Examples = new List<Example>();
            Categories = new List<String>();
            Signatures = new List<Signature>();
        }

        public MemberType Type { get; private set; }
        public string Name { get; private set; }
        public string ReturnType { get; private set; }
        public string Description { get; private set; }
        public string LongDescription { get; private set; }
        public IList<Example> Examples { get; private set; }
        public IList<String> Categories { get; private set; }
        public IList<Signature> Signatures { get; private set; }
    }

    public class Signature
    {
        public Signature(string added)
        {
            Added = added;
            Arguments = new List<Argument>();
        }

        public string Added { get; private set; }
        public IList<Argument> Arguments { get; private set; }
    }

    public class Argument
    {
        public Argument(string name, string type, bool optional, string description)
        {
            Name = name;
            Type = type;
            Optional = optional;
            Description = description;
        }

        public string Name { get; private set; }
        public string Type { get; private set; }
        public Boolean Optional { get; private set; }
        public string Description { get; private set; }
    }

    public class Example
    {
        public Example(string description, string css, string html, string code)
        {
            Description = description;
            Css = css;
            Html = html;
            Code = code;
            JavascriptReferences = new List<string>();
        }

        public string Description { get; private set; }
        public string Css { get; private set; }
        public string Html { get; private set; }
        public string Code { get; private set; }
        public IList<string> JavascriptReferences { get; private set; }
    }
}