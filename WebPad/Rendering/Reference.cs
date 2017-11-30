using System;

namespace WebPad.Rendering
{
    public class Reference
    {
        public Reference(ReferenceTypes type, string url)
        {
            Type = type;
            Url = url;
        }

       public Reference()
       {
       }

       public ReferenceTypes Type { get; set; }
       public string Url { get; set; }

        public string GetHtml()
        {
            switch (Type)
            {
                case ReferenceTypes.Javascript:
                    return string.Format("<script type='text/javascript' src='{0}'></script>", Url);
                case ReferenceTypes.Css:
                    return string.Format("<link rel='stylesheet' href='{0}' >", Url);
                default:
                    throw new ArgumentOutOfRangeException("Type", Type, "The reference type must be a member of the ReferenceTypes enum.");
            }
        }
    }
}