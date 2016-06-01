using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtmCliUtility
{
    public static class DocumentsV1012Helper
    {
        public static Documents Create(string ownerFsrarId, object item, ItemChoiceType itemChoiceType)
        {
            return new Documents()
            {
                Document = new DocBody()
                {
                    Item = item,
                    ItemElementName = itemChoiceType
                },
                Owner = new SenderInfo()
                {
                    FSRAR_ID = ownerFsrarId
                },
                Version = "1.0"
            };
        }
    }
}
