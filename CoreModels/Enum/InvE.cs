namespace CoreModels.Enum
{
    public class InvE
    {
        public enum InvType //库存交易出入类型
        {
            采购进货 = 1101,
            采购退货 = 1102,
            销售退货 = 1201,
            销售出仓 = 1202,
            调拨入 = 1301,
            调拨出 = 1302,
            盘点 = 1401,
            差异 = 1501,
            领用退仓 = 1601,
            领用出仓 = 1602,
            期初 = 1701,
            其他进仓 = 1801,
            其他出仓 = 1802,
            加工进仓 = 1901,
            加工出仓 = 1902,
            发票 = 2001
        }

        public enum SfcMainTypeE //单据类型
        {
            期初 = 1,
            盘点 = 2,
            调拨 = 3
        }
    }
}