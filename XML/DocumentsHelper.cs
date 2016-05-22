using System;

namespace UtmCliUtility.XML
{
    public static class DocumentsHelper
    {
        private static Documents Create(object item, ItemChoiceType itemChoiceType, string ownerFsrarId)
        {
            if (item == null) throw new ArgumentNullException("docBody");
            if (ownerFsrarId == null) throw new ArgumentNullException("ownerFsrarId");
            return new Documents()
            {
                Document = new DocBody()
                {
                    Item = item,
                    ItemElementName = itemChoiceType
                },
                Version = "1.0",
                Owner = new SenderInfo()
                {
                    FSRAR_ID = ownerFsrarId
                }
            };
        }

        public static Documents CreateWayBillAct(string ownerFsrarId, string wbRegId, bool isAccept, string actNumber, DateTime actDate, string notes = null)
        {
            if (wbRegId == null) throw new ArgumentNullException("wbRegId");
            if (actNumber == null) throw new ArgumentNullException("actNumber");
            var actType = new WayBillActType()
            {
                Identity = Guid.NewGuid().ToString(),
                Header = new WayBillActTypeHeader()
                {
                    ActDate = actDate,
                    ACTNUMBER = actNumber,
                    IsAccept = isAccept ? AcceptType.Accepted : AcceptType.Rejected,
                    Note = notes,
                    WBRegId = wbRegId
                }
            };
            return Create(actType, ItemChoiceType.WayBillAct, ownerFsrarId);
        }
    }
}
