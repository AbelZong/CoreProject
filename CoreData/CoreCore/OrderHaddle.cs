using CoreModels;
using MySql.Data.MySqlClient;
using System;
using CoreModels.XyCore;
using Dapper;
using System.Collections.Generic;
using CoreModels.XyComm;
using static CoreModels.Enum.OrderE;
using CoreData.CoreUser;
using CoreModels.XyUser;
namespace CoreData.CoreCore
{
    public static class OrderHaddle
    {
        ///<summary>
        ///查询业务流程资料
        ///</summary>
        public static DataResult GetConfig(int CoID)
        {
            var result = new DataResult(1,null);    
            var res = BusinessHaddle.GetBusiness(CoID);
            var bu = res.d as BusinessData;
            var u = bu.businessData as Business;
            result.d = u;
            return result;
        }
        ///<summary>
        ///查询订单List
        ///</summary>
        public static DataResult GetOrderList(OrderParm cp)
        {
            var result = new DataResult(1,null);    
            string sqlcount = "select count(id) from `order` where 1=1";
            string sqlcommand = @"select ID,Type,DealerType,IsMerge,IsSplit,OSource,SoID,ODate,PayDate,BuyerShopID,ShopName,Amount,PaidAmount,ExAmount,IsCOD,Status,AbnormalStatus,
                                  StatusDec,RecMessage,SendMessage,Express,RecLogistics,RecCity,RecDistrict,RecAddress,RecName,ExWeight,Distributor,SupDistributor,InvoiceTitle,
                                  PlanDate,SendWarehouse,SendDate,ExCode,Creator,RecTel,RecPhone,ExID from `order` where 1=1"; 
            string wheresql = string.Empty;
            if(cp.CoID != 1)//公司编号
            {
                wheresql = wheresql + " and coid = " + cp.CoID;
            }
            if(cp.ID > 0)//内部订单号
            {
                wheresql = wheresql + " and id = " + cp.ID;
            }
            if(cp.SoID > 0)//外部订单号
            {
                wheresql = wheresql + " and soid = " + cp.SoID;
            }
            if(!string.IsNullOrEmpty(cp.PayNbr))//付款单号
            {
               wheresql = wheresql + " and paynbr = '" + cp.PayNbr + "'";
            }
            if(!string.IsNullOrEmpty(cp.BuyerShopID))//买家账号
            {
               wheresql = wheresql + " and buyershopid like '%" + cp.BuyerShopID + "%'";
            }
            if(!string.IsNullOrEmpty(cp.ExCode))//快递单号
            {
               wheresql = wheresql + " and excode like '%" + cp.ExCode + "%'";
            }
            if(!string.IsNullOrEmpty(cp.RecName))//收货人
            {
               wheresql = wheresql + " and recname like '%" + cp.RecName + "%'";
            }
            if(!string.IsNullOrEmpty(cp.RecPhone))//手机
            {
               wheresql = wheresql + " and recphone like '%" + cp.RecPhone + "%'";
            }
            if(!string.IsNullOrEmpty(cp.RecTel))//固定电话
            {
               wheresql = wheresql + " and rectel like '%" + cp.RecTel + "%'";
            }
            if(!string.IsNullOrEmpty(cp.RecLogistics))//收货人省
            {
               wheresql = wheresql + " and reclogistics like '%" + cp.RecLogistics + "%'";
            }
            if(!string.IsNullOrEmpty(cp.RecCity))//收货人城市
            {
               wheresql = wheresql + " and reccity like '%" + cp.RecCity + "%'";
            }
            if(!string.IsNullOrEmpty(cp.RecDistrict))//收货人县区
            {
               wheresql = wheresql + " and recdistrict like '%" + cp.RecDistrict + "%'";
            }
            if(!string.IsNullOrEmpty(cp.RecAddress))//详细地址
            {
               wheresql = wheresql + " and recaddress like '%" + cp.RecAddress + "%'";
            }
            if (cp.StatusList != null)//状态List
            {
                wheresql = wheresql + " AND status in ("+ string.Join(",", cp.StatusList) + ")" ;
            }
            if (cp.AbnormalStatusList != null)//异常状态List
            {
                string status = " AND abnormalstatus in (0,"+ string.Join(",", cp.AbnormalStatusList) + ")" ;
                if(cp.StatusList != null)
                {
                    if(cp.StatusList.Count == 1 && cp.StatusList[0] == 7)
                    {
                        status = " AND abnormalstatus in ("+ string.Join(",", cp.AbnormalStatusList) + ")" ;
                    }
                }
                else
                {
                    status = " AND abnormalstatus in ("+ string.Join(",", cp.AbnormalStatusList) + ")" ;
                }
                wheresql = wheresql + status ;
            }
            if(cp.IsRecMsgYN.ToUpper() == "Y")
            {
                if(cp.RecMessage == null)
                {
                    wheresql = wheresql + " AND recmessage != '' and status in (0,1,2,7)" ;
                }
                else
                {
                    wheresql = wheresql + " AND recmessage like '%" + cp.RecMessage + "%' and status in (0,1,2,7)" ;
                }
            }
            if(cp.IsRecMsgYN.ToUpper() == "N")
            {
                wheresql = wheresql + " AND recmessage = '' and status in (0,1,2,7)" ;
            }
            if(cp.IsSendMsgYN.ToUpper() == "Y")
            {
                if(cp.SendMessage == null)
                {
                    wheresql = wheresql + " AND sendmessage != '' and status in (0,1,2,7)" ;
                }
                else 
                {
                    wheresql = wheresql + " AND sendmessage like '%" + cp.SendMessage + "%' and status in (0,1,2,7)" ;
                }
            }
            if(cp.IsSendMsgYN.ToUpper() == "N")
            {
                wheresql = wheresql + " AND sendmessage = '' and status in (0,1,2,7)" ;
            }
            if(cp.Datetype.ToUpper() == "ODATE")
            {
                wheresql = wheresql + " AND odate between '" + cp.DateStart + "' and '" + cp.DateEnd + "'" ;
            }
            if(cp.Datetype.ToUpper() == "SENDDATE")
            {
                wheresql = wheresql + " AND senddate between '" + cp.DateStart + "' and '" + cp.DateEnd + "'" ;
            }
            if(cp.Datetype.ToUpper() == "PAYDATE")
            {
                wheresql = wheresql + " AND paydate between '" + cp.DateStart + "' and '" + cp.DateEnd + "'" ;
            }
            if(cp.Datetype.ToUpper() == "PLANDATE")
            {
                wheresql = wheresql + " AND plandate between '" + cp.DateStart + "' and '" + cp.DateEnd + "'" ;
            }
            if(!string.IsNullOrEmpty(cp.Skuid))
            {
               wheresql = wheresql + " and exists(select id from orderitem where oid = order.id and skuid = '" + cp.Skuid + "')";
            }
            if(!string.IsNullOrEmpty(cp.GoodsCode))
            {
               wheresql = wheresql + " and exists(select id from orderitem where oid = order.id and GoodsCode = '" + cp.Skuid + "')";
            }
            if(cp.Ordqtystart > 0)
            {
                wheresql = wheresql + " AND ordqty >= " +  cp.Ordqtystart + " and status in (0,1,2,7)";
            }
            if(cp.Ordqtyend > 0)
            {
                wheresql = wheresql + " AND ordqty <= " +  cp.Ordqtyend + " and status in (0,1,2,7)";
            }
            if(cp.Ordamtstart > 0)
            {
                wheresql = wheresql + " AND amount >= " +  cp.Ordamtstart + " and status in (0,1,2,7)";
            }
            if(cp.Ordamtend > 0)
            {
                wheresql = wheresql + " AND amount <= " +  cp.Ordamtend + " and status in (0,1,2,7)";
            }
            if(!string.IsNullOrEmpty(cp.Skuname))
            {
               wheresql = wheresql + " and exists(select id from orderitem where oid = order.id and skuname like '%" + cp.Skuname + "%') and status in (0,1,2,7)";
            }
            if(!string.IsNullOrEmpty(cp.Norm))
            {
               wheresql = wheresql + " and exists(select id from orderitem where oid = order.id and norm like '%" + cp.Norm + "%') and status in (0,1,2,7)";
            }
            if(cp.ShopStatus != null)
            {
                string shopstatus = string.Empty;
                foreach(var x in cp.ShopStatus)
                {
                    shopstatus = shopstatus + "'" + x + "',";
                }
                shopstatus = shopstatus.Substring(0,shopstatus.Length - 1);
                wheresql = wheresql + " AND shopstatus in (" +  shopstatus + ")";
            }
            if(cp.OSource > -1)
            {
                wheresql = wheresql + " AND osource = " +  cp.OSource;
            }
            if (cp.Type != null)
            {
                wheresql = wheresql + " AND type in ("+ string.Join(",", cp.Type) + ")" ;
            }
            if(cp.IsCOD.ToUpper() == "Y")
            {
                wheresql = wheresql + " AND iscod = true" ;
            }
            if(cp.IsCOD.ToUpper() == "N")
            {
                wheresql = wheresql + " AND iscod = false" ;
            }
            if(cp.IsPaid.ToUpper() == "Y")
            {
                wheresql = wheresql + " AND IsPaid = true" ;
            }
            if(cp.IsPaid.ToUpper() == "N")
            {
                wheresql = wheresql + " AND IsPaid = false" ;
            }
            if (cp.IsShopSelectAll == false &&　cp.ShopID != null)
            {
                wheresql = wheresql + " AND shopid in ("+ string.Join(",", cp.ShopID) + ")" ;
            }
            if(cp.IsDisSelectAll == true)
            {
                wheresql = wheresql + " AND dealertype = 2" ;
            }
            else
            {
                if(cp.Distributor != null)
                {
                    string distributor = string.Empty;
                    foreach(var x in cp.Distributor)
                    {
                        distributor = distributor + "'" + x + "',";
                    }
                    distributor = distributor.Substring(0,distributor.Length - 1);
                    wheresql = wheresql + " AND dealertype = 2 AND distributor in (" +  distributor + ")";
                }
            }
            if(cp.ExID != null)
            {
                wheresql = wheresql + " AND exid in ("+ string.Join(",", cp.ExID) + ")" ;
            }
            if(cp.SendWarehouse != null)
            {
                wheresql = wheresql + " AND WarehouseID in ("+ string.Join(",", cp.SendWarehouse) + ")" ;
            }
            if(cp.Others != null)
            {
                if(cp.Others.Contains(4))
                {
                    wheresql = wheresql + " and IsInvoice = true";
                }
                if(cp.Others.Contains(0) == true &&　cp.Others.Contains(0) == false)
                {
                    wheresql = wheresql + " and IsMerge = true";
                }
                if(cp.Others.Contains(0) == false &&　cp.Others.Contains(0) == true)
                {
                    wheresql = wheresql + " and IsMerge = false";
                }
                if(cp.Others.Contains(1) == true &&　cp.Others.Contains(3) == false)
                {
                    wheresql = wheresql + " and IsSplit = true";
                }
                if(cp.Others.Contains(1) == false &&　cp.Others.Contains(3) == true)
                {
                    wheresql = wheresql + " and IsSplit = false";
                }
            }
            if(!string.IsNullOrEmpty(cp.SortField) && !string.IsNullOrEmpty(cp.SortDirection))//排序
            {
                wheresql = wheresql + " ORDER BY "+cp.SortField +" "+ cp.SortDirection;
            }
            var res = new OrderData();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    int count = conn.QueryFirst<int>(sqlcount + wheresql);
                    decimal pagecnt = Math.Ceiling(decimal.Parse(count.ToString())/decimal.Parse(cp.NumPerPage.ToString()));
                    int dataindex = (cp.PageIndex - 1)* cp.NumPerPage;
                    wheresql = wheresql + " limit " + dataindex.ToString() + " ," + cp.NumPerPage.ToString();
                    var u = conn.Query<OrderQuery>(sqlcommand + wheresql).AsList();
                    res.Datacnt = count;
                    res.Pagecnt = pagecnt;
                    res.Ord = u;
                    //订单资料
                    List<int> ItemID = new List<int>();
                    List<int> MID = new List<int>();
                    foreach(var a in res.Ord)
                    {
                        a.TypeString = GetTypeName(a.Type);
                        a.AbnormalStatusDec = a.StatusDec;
                        a.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);
                        if(!string.IsNullOrEmpty(a.ExID.ToString()))
                        {
                            a.ExpNamePinyin = GetExpNamePinyin(cp.CoID,a.ExID);
                        }
                        if(!string.IsNullOrEmpty(a.PaidAmount))
                        {
                            if(decimal.Parse(a.PaidAmount) == 0)
                            {
                                a.PayDate = null;
                            }
                        }
                        if(a.OSource != 3)
                        {
                            a.Creator = "";
                        }
                        if(a.IsMerge == true)
                        {
                            MID.Add(a.ID);
                        }
                        ItemID.Add(a.ID);
                    }
                    //处理soid
                    var y = new List<Order>();
                    if(MID.Count > 0)
                    {
                        sqlcommand = "select MergeOID,soid from `order` where coid = @Coid and MergeOID in @ID";
                        y = conn.Query<Order>(sqlcommand,new{Coid = cp.CoID,ID = MID}).AsList();
                    }
                    sqlcommand = @"select id,oid,SkuAutoID,Img,Qty,GoodsCode,SkuID,SkuName,Norm,RealPrice,Amount,ShopSkuID,IsGift,Weight from orderitem 
                                        where oid in @ID and coid = @Coid";
                    var item = conn.Query<SkuList>(sqlcommand,new{ID = ItemID,Coid = cp.CoID}).AsList();
                    List<int> skuid = new List<int>();
                    foreach(var i in item)
                    {
                        skuid.Add(i.SkuAutoID);
                    }
                    sqlcommand = "select Skuautoid,(StockQty - LockQty + VirtualQty) as InvQty from inventory_sale where coid = @Coid and Skuautoid in @Skuid";
                    var inv = conn.Query<Inv>(sqlcommand,new{Coid=cp.CoID,Skuid = skuid}).AsList();
                    foreach(var i in item)
                    {
                        i.InvQty = 0;
                        foreach(var j in inv)
                        {
                            if(i.SkuAutoID == j.Skuautoid)
                            {
                                i.InvQty = j.InvQty;
                                break;
                            }
                        }
                    }
                    foreach(var a in res.Ord)
                    {
                        if(a.IsMerge == true)
                        {
                            var soid = new List<long>();
                            soid.Add(a.SoID);
                            foreach(var b in y)
                            {
                                if(a.ID == b.MergeOID)
                                {
                                    soid.Add(b.SoID);
                                }
                            }
                            a.SoIDList = soid;
                        }
                        var sd = new List<SkuList>();
                        foreach(var i in item)
                        {
                            if(a.ID == i.OID)
                            {
                                sd.Add(i);
                            }
                        }
                        a.SkuList = sd;
                    }
                    result.d = res;             
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }           
            return result;
        }
        ///<summary>
        ///查询订单List single
        ///</summary>
        public static DataResult GetOrderListSingle(List<int> OID,int CoID)
        {
            var result = new DataResult(1,null);    
            string sqlcommand = @"select ID,Type,DealerType,IsMerge,IsSplit,OSource,SoID,ODate,PayDate,BuyerShopID,ShopName,Amount,PaidAmount,ExAmount,IsCOD,Status,AbnormalStatus,
                                  StatusDec,RecMessage,SendMessage,Express,RecLogistics,RecCity,RecDistrict,RecAddress,RecName,ExWeight,Distributor,SupDistributor,InvoiceTitle,
                                  PlanDate,SendWarehouse,SendDate,ExCode,Creator,RecTel,RecPhone,ExID from `order` where id in @ID and coid =@Coid"; 
            var res = new OrderData();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{   
                    var u = conn.Query<OrderQuery>(sqlcommand,new{ID=OID,Coid = CoID}).AsList();
                    res.Datacnt = u.Count;
                    res.Pagecnt = 1;
                    res.Ord = u;
                    //订单资料
                    List<int> ItemID = new List<int>();
                    List<int> MID = new List<int>();
                    foreach(var a in res.Ord)
                    {
                        a.TypeString = GetTypeName(a.Type);
                        a.AbnormalStatusDec = a.StatusDec;
                        a.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);
                        if(!string.IsNullOrEmpty(a.ExID.ToString()))
                        {
                            a.ExpNamePinyin = GetExpNamePinyin(CoID,a.ExID);
                        }
                        if(!string.IsNullOrEmpty(a.PaidAmount))
                        {
                            if(decimal.Parse(a.PaidAmount) == 0)
                            {
                                a.PayDate = null;
                            }
                        }
                        if(a.OSource != 3)
                        {
                            a.Creator = "";
                        }
                        if(a.IsMerge == true)
                        {
                            MID.Add(a.ID);
                        }
                        ItemID.Add(a.ID);
                    }
                    //处理soid
                    var y = new List<Order>();
                    if(MID.Count > 0)
                    {
                        sqlcommand = "select MergeOID,soid from `order` where coid = @Coid and MergeOID in @ID";
                        y = conn.Query<Order>(sqlcommand,new{Coid = CoID,ID = MID}).AsList();
                    }
                    sqlcommand = @"select id,oid,SkuAutoID,Img,Qty,GoodsCode,SkuID,SkuName,Norm,RealPrice,Amount,ShopSkuID,IsGift,Weight from orderitem 
                                        where oid in @ID and coid = @Coid";
                    var item = conn.Query<SkuList>(sqlcommand,new{ID = ItemID,Coid = CoID}).AsList();
                    List<int> skuid = new List<int>();
                    foreach(var i in item)
                    {
                        skuid.Add(i.SkuAutoID);
                    }
                    sqlcommand = "select Skuautoid,(StockQty - LockQty + VirtualQty) as InvQty from inventory_sale where coid = @Coid and Skuautoid in @Skuid";
                    var inv = conn.Query<Inv>(sqlcommand,new{Coid=CoID,Skuid = skuid}).AsList();
                    foreach(var i in item)
                    {
                        i.InvQty = 0;
                        foreach(var j in inv)
                        {
                            if(i.SkuAutoID == j.Skuautoid)
                            {
                                i.InvQty = j.InvQty;
                                break;
                            }
                        }
                    }
                    foreach(var a in res.Ord)
                    {
                        if(a.IsMerge == true)
                        {
                            var soid = new List<long>();
                            soid.Add(a.SoID);
                            foreach(var b in y)
                            {
                                if(a.ID == b.MergeOID)
                                {
                                    soid.Add(b.SoID);
                                }
                            }
                            a.SoIDList = soid;
                        }
                        var sd = new List<SkuList>();
                        foreach(var i in item)
                        {
                            if(a.ID == i.OID)
                            {
                                sd.Add(i);
                            }
                        }
                        a.SkuList = sd;
                    }
                    result.d = res;             
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }           
            return result;
        }
        ///<summary>
        ///刷新单笔订单明细显示
        ///</summary>
        public static DataResult GetSingleOrdItem(int OID,int CoID)
        {
            var result = new DataResult(1,null);   
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string wheresql = @"select id,SkuAutoID,Img,Qty,GoodsCode,SkuID,SkuName,Norm,RealPrice,Amount,ShopSkuID,IsGift,Weight from orderitem 
                                        where oid = " + OID + " and coid =" + CoID;
                    var u = conn.Query<SkuList>(wheresql).AsList();
                    foreach(var i in u)
                    {
                        i.InvQty = GetInvQty(CoID,i.SkuAutoID);
                    }
                    result.d = u;
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            return result;
        }
        ///<summary>
        ///根据Sku查询库存
        ///</summary>
        public static int GetInvQty(int CoID,int skuid)
        {
            int invqty = 0;
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string wheresql = "select ifnull(StockQty - LockQty + VirtualQty,0) from inventory_sale where coid = " + CoID + " and Skuautoid = " + skuid;
                    invqty = conn.QueryFirst<int>(wheresql);
                }catch(Exception ex){
                    result.d  = ex.Message;
                    invqty = 0;
                    conn.Dispose();
                }
            } 
            return invqty;
        }
        ///<summary>
        ///查询快递公司的拼音
        ///</summary>
        public static string GetExpNamePinyin(int CoID,int expid)
        {
            string ExpNamePinyin = string.Empty;
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CommConnectString) ){
                try{
                    string wheresql = "select ExpNamePinyin from express where coid = " + CoID + " and ID = " + expid;
                    ExpNamePinyin = conn.QueryFirst<string>(wheresql);
                }catch(Exception ex){
                    result.d  = ex.Message;
                    ExpNamePinyin = string.Empty;
                    conn.Dispose();
                }
            } 
            return ExpNamePinyin;
        }
        ///<summary>
        ///新增订单
        ///</summary>
        public static DataResult InsertOrder(Order ord,string UserName,int CoID,bool isFaceToFace)
        {
            var result = new DataResult(1,null);   
            var rec = new RecInfo();
            var log = new LogInsert();
            ord.CoID = CoID;
            ord.Creator = UserName;
            ord.Modifier = UserName;
            using(var conn = new MySqlConnection(DbBase.CommConnectString) ){
                try{
                    if ( ord.ShopID == -1)
                    {
                        result.s = -1;
                        result.d = "店铺必须有值!";
                        return result;
                    }
                    if(ord.ShopID == 0)
                    {
                        ord.ShopName = "{线下}";
                        ord.ShopSit = 35;
                    }
                    else
                    {
                        string wheresql = "select * from Shop where id =" + ord.ShopID + " and coid =" + CoID;
                        var u = conn.Query<Shop>(wheresql).AsList();
                        if(u.Count == 0)
                        {
                            result.s = -1;
                            result.d = "此店铺不存在!!";
                            return result;
                        }
                        else
                        {
                            if(u[0].Enable == false || u[0].Deleted == true)
                            {
                                result.s = -1;
                                result.d = "此店铺已停用或已删除!!";
                                return result;
                            }
                            else
                            {
                                ord.ShopName = u[0].ShopName;
                                ord.ShopSit = u[0].SitType;
                            }
                        }
                    }
                    if(isFaceToFace == true)
                    {
                        ord.Express = "现场取货";
                        var ee = GetExpID("现场取货",CoID);
                        if(ee.s == -1)
                        {
                            result.s = -1;
                            result.d = "请先设定现场取货的快递资料!!";
                            return result;
                        }
                        ord.ExID = ee.s;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            //检查必输栏位是否都有值
            if ( ord.ODate == null)
            {
                result.s = -1;
                result.d = "订单日期必须有值!";
                return result;
            }
            if(isFaceToFace == false)
            {
                if ( ord.RecLogistics == null || ord.RecCity == null || ord.RecDistrict == null || ord.RecAddress == null)
                {
                    result.s = -1;
                    result.d = "收货地址必须有值!";
                    return result;
                }
                if ( ord.RecName == null)
                {
                    result.s = -1;
                    result.d = "收货人必须有值!";
                    return result;
                }
            }
            bool isRecExsit = true;
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    //检查平台编号是否重复
                    if(ord.SoID > 0)
                    {
                        string wheresql = "select count(id) from `order` where soid = " + ord.SoID + " and coid = " + CoID;
                        int u = conn.QueryFirst<int>(wheresql);
                        if(u > 0)
                        {
                            result.s = -1;
                            result.d = "线上订单号已经存在,请重新输入!";
                            return result;
                        }
                    }
                    else
                    {
                        string rand = DateTime.Now.Ticks.ToString();
                        ord.SoID = long.Parse(rand.Substring(0, 11));
                    }
                    //检查收货人是否存在
                    if(ord.BuyerShopID == null)
                    {
                        ord.BuyerShopID = ord.RecName;
                    }
                    if(ord.BuyerShopID != null && ord.RecName != null && ord.RecLogistics != null && ord.RecCity != null && ord.RecDistrict != null && ord.RecAddress != null)
                    {
                        string wheresql = "select count(id) from recinfo where coid = " + CoID + " and buyerid = '" + ord.BuyerShopID + "' and receiver = '" + ord.RecName + 
                                           "' and address = '" + ord.RecAddress + "' and logistics = '" + ord.RecLogistics + "' and city = '" + ord.RecCity + 
                                           "' and district = '" + ord.RecDistrict + "'";
                        int u = conn.QueryFirst<int>(wheresql);
                        if(u > 0)
                        {
                            wheresql = "select id from recinfo where coid = " + CoID + " and buyerid = '" + ord.BuyerShopID + "' and receiver = '" + ord.RecName + 
                                           "' and address = '" + ord.RecAddress + "' and logistics = '" + ord.RecLogistics + "' and city = '" + ord.RecCity + 
                                           "' and district = '" + ord.RecDistrict + "'";
                            u = conn.QueryFirst<int>(wheresql);
                            ord.BuyerID = u;
                        }
                        else
                        {
                            isRecExsit = false;
                            rec.BuyerId = ord.BuyerShopID;
                            rec.Receiver = ord.RecName;
                            rec.Tel = ord.RecTel;
                            rec.Phone = ord.RecPhone;
                            rec.Logistics = ord.RecLogistics;
                            rec.City = ord.RecCity;
                            rec.District = ord.RecDistrict;
                            rec.Address = ord.RecAddress;
                            rec.ZipCode = ord.RecZip;
                            rec.Express = ord.Express;
                            rec.ExID = ord.ExID;
                            rec.CoID = CoID;
                            rec.Creator = UserName;
                            rec.ShopSit = ord.ShopSit;
                        }
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            //检查订单是否符合合并的条件
            var res = isCheckMerge(ord);
            int reasonid = 0;
            if(res.s == 1)
            {
                reasonid = GetReasonID("等待订单合并",CoID,7).s;
                if(reasonid == -1)
                {
                    result.s = -1;
                    result.d = "请先设定【等待订单合并】的异常";
                    return result;
                }
            }
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlCommandText = string.Empty;
                int count =0;
                if(isRecExsit == false)
                {
                    sqlCommandText = @"INSERT INTO recinfo(BuyerId,Receiver,Tel,Phone,Logistics,City,District,Address,ZipCode,Express,ExID,CoID,Creator,ShopSit) VALUES(
                            @BuyerId,@Receiver,@Tel,@Phone,@Logistics,@City,@District,@Address,@ZipCode,@Express,@ExID,@CoID,@Creator,@ShopSit)";
                    count =CoreDBconn.Execute(sqlCommandText,rec,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    else
                    {
                        int rtn = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                        ord.BuyerID = rtn;
                        rec.ID = rtn;
                    }
                }
                if(res.s == 1)
                {
                    ord.Status = 7;
                    ord.AbnormalStatus = reasonid;
                    ord.StatusDec = "等待订单合并";
                }
                else
                {
                    ord.Status = 0;
                    ord.AbnormalStatus = 0;
                    ord.StatusDec = "";
                }
                sqlCommandText = @"INSERT INTO `order`(ODate,CoID,BuyerID,BuyerShopID,ShopID,ShopName,ShopSit,SoID,RecName,RecLogistics,RecCity,RecDistrict,RecAddress,RecZip,
                                                            RecTel,RecPhone,RecMessage,SendMessage,Express,ExID,Creator,Modifier,Status,AbnormalStatus,StatusDec) VALUES(
                                        @ODate,@CoID,@BuyerID,@BuyerShopID,@ShopID,@ShopName,@ShopSit,@SoID,@RecName,@RecLogistics,@RecCity,@RecDistrict,@RecAddress,@RecZip,
                                                            @RecTel,@RecPhone,@RecMessage,@SendMessage,@Express,@ExID,@Creator,@Modifier,@Status,@AbnormalStatus,@StatusDec)";
                count =CoreDBconn.Execute(sqlCommandText,ord,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                else
                {
                    int rtn = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                    result.d = rtn;
                    rec.OID=rtn;
                    ord.ID = rtn;
                }
                log.OID = ord.ID;
                log.SoID = ord.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "接单时间";
                log.Remark = "手工下单时间";
                log.CoID = CoID;
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                   VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count =CoreDBconn.Execute(loginsert,log,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                if(ord.Status == 7)
                {
                    log.LogDate = DateTime.Now;
                    log.Title = "标记异常";
                    log.Remark = "等待订单合并(自动)";
                    count =CoreDBconn.Execute(loginsert,log,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                }
                if(isRecExsit == false)
                {
                    sqlCommandText = @"update recinfo set OID = @OID where id = @ID and coid = @Coid";
                    count =CoreDBconn.Execute(sqlCommandText,rec,TransCore);
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                if(res.s == 1)
                {
                    List<int> idlist = res.d as List<int>;
                    string querySql = "select id,soid from `order` where id in @ID and coid = @Coid and status <> 7";
                    var v = CoreDBconn.Query<Order>(querySql,new{ID = idlist,Coid = CoID}).AsList();
                    if(v.Count > 0)
                    {
                        sqlCommandText = @"update `order` set status = 7,abnormalstatus = @Abnormalstatus,statusdec = '等待订单合并' where id in @ID and coid = @Coid and status <> 7";
                        count =CoreDBconn.Execute(sqlCommandText,new {Abnormalstatus = reasonid,ID = idlist,Coid = CoID},TransCore);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        foreach(var c in v)
                        {
                            log.OID = c.ID;
                            log.SoID = c.SoID;
                            log.LogDate = DateTime.Now;
                            log.Title = "标记异常";
                            log.Remark = "等待订单合并(自动)";
                            count =CoreDBconn.Execute(loginsert,log,TransCore);
                            if(count < 0)
                            {
                                result.s = -3002;
                                return result;
                            }
                        }
                    }
                }
                TransCore.Commit();
            }catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return  result;
        }
        ///<summary>
        ///检查订单是否符合合并的条件
        ///</summary>
        public static DataResult isCheckMerge(Order ord)
        {
            var result = new DataResult(1,null);   
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string wheresql = "select id from `order` where coid = " + ord.CoID + " and buyershopid = '" + ord.BuyerShopID + "' and recname = '" + ord.RecName + 
                                      "' and reclogistics = '" + ord.RecLogistics + "' and reccity = '" + ord.RecCity + "' and recdistrict = '" + ord.RecDistrict + 
                                      "' and recaddress = '" + ord.RecAddress + "' and status in (0,1,2,7)";
                    var u = conn.Query<Order>(wheresql).AsList();
                    if(u.Count > 0)
                    {
                        List<int> id = new List<int>();
                        foreach(var a in u)
                        {
                            id.Add(a.ID);
                        }
                        result.s = 1;
                        result.d = id;
                    }
                    else
                    {
                        result.s = 0;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            return result;
        }
        ///<summary>
        ///根据异常原因获取id
        ///</summary>
        public static DataResult GetReasonID(string ReasonName,int CoID,int OrdStatus)
        {
            var result = new DataResult(1,null);   
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string wheresql = "select id from orderabnormal where coid = " + CoID + " and name = '" + ReasonName + "' and OrdStatus = " + OrdStatus;
                    int u = conn.QueryFirst<int>(wheresql);
                    if(u > 0)
                    {
                        result.s = u;
                    }
                    else
                    {
                        result.s = -1;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            return result;
        }
        ///<summary>
        ///根据异常id获取名称
        ///</summary>
        public static DataResult GetReasonName(int ReasonID,int CoID,int OrdStatus)
        {
            var result = new DataResult(1,null);   
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string wheresql = "select name from orderabnormal where coid = " + CoID + " and id = '" + ReasonID + "' and OrdStatus = " + OrdStatus;
                    string u = conn.QueryFirst<string>(wheresql);
                    if(!string.IsNullOrEmpty(u))
                    {
                        result.s = 1;
                        result.d = u;
                    }
                    else
                    {
                        result.s = -1;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            return result;
        }
        ///<summary>
        ///根据快递名称抓取ID
        ///</summary>
        public static DataResult GetExpID(string Express,int CoID)
        {
            var result = new DataResult(1,null);   
            using(var conn = new MySqlConnection(DbBase.CommConnectString) ){
                try{
                    string wheresql = "select id from express where coid = " + CoID + " and ExpName = '" + Express + "'";
                    int u = conn.QueryFirst<int>(wheresql);
                    if(u > 0)
                    {
                        result.s = u;
                    }
                    else
                    {
                        result.s = -1;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            return result;
        }
        ///<summary>
        ///根据快递ID获取名称
        ///</summary>
        public static DataResult GetExpName(int Express,int CoID)
        {
            var result = new DataResult(1,null);   
            using(var conn = new MySqlConnection(DbBase.CommConnectString) ){
                try{
                    string wheresql = "select ExpName from express where coid = " + CoID + " and id = " + Express;
                    string u = conn.QueryFirst<string>(wheresql);
                    if(!string.IsNullOrEmpty(u))
                    {
                        result.s = 1;
                        result.d = u;
                    }
                    else
                    {
                        result.s = -1;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            return result;
        }
        ///<summary>
        ///更新订单
        ///</summary>
        public static DataResult UpdateOrder(string ExAmount,string Remark,string InvTitle,string Logistics,string City,string District,string Address,string Name,
                                            string tel,string phone,int OID,string UserName,int CoID)
        {
            var result = new DataResult(1,null);   
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var logs = new List<LogInsert>();
            var OrdOlder = new Order();
            var CancleOrdAb = new Order();
            int reasonid = 0;
            List<int> idlist = new List<int>();
            int x = 0;//未审核订单修改标记
            int y = 0;//审核订单修改标记
            int z = 0;//地址修改标记
            string RecLogistics="",RecCity="",RecDistrict="",RecAddress="",RecName="";
            string querySql = "select * from `order` where id = @ID and coid = @Coid";
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    var u = conn.Query<Order>(querySql,new{ID = OID,Coid = CoID}).AsList();
                    if(u.Count == 0)
                    {
                        result.s = -1;
                        result.d = "参数无效";
                        return result;
                    }
                    ischeckPaid = u[0].IsPaid;
                    OrdOlder = u[0] as Order;
                    RecLogistics = OrdOlder.RecLogistics;
                    RecCity = OrdOlder.RecCity;
                    RecDistrict = OrdOlder.RecDistrict;
                    RecAddress = OrdOlder.RecAddress;
                    RecName = OrdOlder.RecName;
                    if(OrdOlder.Status == 0 || OrdOlder.Status == 1 || OrdOlder.Status == 7)
                    {
                        if(ExAmount != null && OrdOlder.ExAmount != ExAmount)
                        {
                            var log = new LogInsert();
                            log.OID = OrdOlder.ID;
                            log.SoID = OrdOlder.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "手动修改运费";
                            log.Remark = "运费" + OrdOlder.ExAmount + " => " + ExAmount;
                            log.CoID = CoID;
                            logs.Add(log);
                            OrdOlder.ExAmount = ExAmount;
                            x++;
                        }
                        string addressOlder = OrdOlder.RecLogistics + OrdOlder.RecCity + OrdOlder.RecDistrict + OrdOlder.RecAddress;
                        if(Logistics != null && OrdOlder.RecLogistics != Logistics)
                        {
                            OrdOlder.RecLogistics = Logistics;
                            x++;
                            z++;
                        }
                        if(City != null && OrdOlder.RecCity != City)
                        {
                            OrdOlder.RecCity = City;
                            x++;
                            z++;
                        }
                        if(District != null && OrdOlder.RecDistrict != District)
                        {
                            OrdOlder.RecDistrict = District;
                            x++;
                            z++;
                        }
                        if(Address != null && OrdOlder.RecAddress != Address)
                        {
                            OrdOlder.RecAddress = Address;
                            x++;
                            z++;
                        }
                        if(z > 0)
                        {
                            var log = new LogInsert();
                            log.OID = OrdOlder.ID;
                            log.SoID = OrdOlder.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "手动修改收货地址";
                            log.Remark = "收货地址" + addressOlder + " => " + OrdOlder.RecLogistics + OrdOlder.RecCity + OrdOlder.RecDistrict + OrdOlder.RecAddress;
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        if(Name != null && OrdOlder.RecName != Name)
                        {
                            var log = new LogInsert();
                            log.OID = OrdOlder.ID;
                            log.SoID = OrdOlder.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "手动修改收货人";
                            log.Remark = "收货人" + OrdOlder.RecName + " => " + Name;
                            log.CoID = CoID;
                            logs.Add(log);
                            OrdOlder.RecName = Name;
                            x++;
                            z++;
                        }
                        if(tel != null && OrdOlder.RecTel != tel)
                        {
                            var log = new LogInsert();
                            log.OID = OrdOlder.ID;
                            log.SoID = OrdOlder.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "手动修改电话";
                            log.Remark = "电话" + OrdOlder.RecTel + " => " + tel;
                            log.CoID = CoID;
                            logs.Add(log);
                            OrdOlder.RecTel = tel;
                            x++;
                        }
                        if(phone != null && OrdOlder.RecPhone != phone)
                        {
                            var log = new LogInsert();
                            log.OID = OrdOlder.ID;
                            log.SoID = OrdOlder.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "手动修改手机";
                            log.Remark = "手机" + OrdOlder.RecPhone + " => " + phone;
                            log.CoID = CoID;
                            logs.Add(log);
                            OrdOlder.RecPhone = phone;
                            x++;
                        }
                    }
                    if(Remark != null && OrdOlder.SendMessage != Remark)
                    {
                        var log = new LogInsert();
                        log.OID = OrdOlder.ID;
                        log.SoID = OrdOlder.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "手动修改卖家备注";
                        log.Remark = "备注" + OrdOlder.SendMessage + " => " + Remark;
                        log.CoID = CoID;
                        logs.Add(log);
                        OrdOlder.SendMessage = Remark;
                        x++;
                        y++;
                    }
                    if(InvTitle != null && OrdOlder.InvoiceTitle != InvTitle)
                    {
                        var log = new LogInsert();
                        log.OID = OrdOlder.ID;
                        log.SoID = OrdOlder.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "手动修改发票抬头";
                        log.Remark = "发票抬头" + OrdOlder.InvoiceTitle + " => " + InvTitle;
                        log.CoID = CoID;
                        logs.Add(log);
                        OrdOlder.InvoiceTitle = InvTitle;
                        x++;
                        y++;
                    }
                    //检查订单是否符合合并的条件
                    bool isCancle = false;
                    if(z > 0)
                    {
                        //若订单本身是等待合并时，先判断是否需要还原资料
                        if(OrdOlder.Status == 7 && OrdOlder.StatusDec == "等待订单合并")
                        {
                            isCancle = true;
                            
                            var ck = isCheckCancleMerge(OID,CoID,OrdOlder.BuyerShopID,RecName,RecLogistics,RecCity,RecDistrict,RecAddress);
                            if(ck.s == 1)
                            {
                                int oid = int.Parse(ck.d.ToString());
                                querySql = "select * from `order` where id = " + oid + " and coid = " + CoID;
                                var v = conn.Query<Order>(querySql).AsList();
                                CancleOrdAb = v[0] as Order;
                                var log = new LogInsert();
                                log.OID = CancleOrdAb.ID;
                                log.SoID = CancleOrdAb.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "取消异常标记";
                                log.Remark = "等待订单合并(自动)";
                                log.CoID = CoID;
                                logs.Add(log);
                                if(CancleOrdAb.IsPaid == true)
                                {
                                    CancleOrdAb.Status = 1;
                                }
                                else
                                {
                                    CancleOrdAb.Status = 0;
                                }
                                CancleOrdAb.AbnormalStatus = 0;
                                CancleOrdAb.StatusDec="";
                                CancleOrdAb.Modifier = UserName;
                                CancleOrdAb.ModifyDate = DateTime.Now;
                            }
                        }
                        //检查订单是否符合合并的条件
                        var res = isCheckMerge(OrdOlder);
                        if(res.s == 1)
                        {
                            if(isCancle == false)
                            {
                                reasonid = GetReasonID("等待订单合并",CoID,7).s;
                                if(reasonid == -1)
                                {
                                    result.s = -1;
                                    result.d = "请先设定【等待订单合并】的异常";
                                    return result;
                                }
                                OrdOlder.Status = 7;
                                OrdOlder.AbnormalStatus = reasonid;
                                OrdOlder.StatusDec="等待订单合并";
                                var log = new LogInsert();
                                log.OID = OrdOlder.ID;
                                log.SoID = OrdOlder.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "标记异常";
                                log.Remark = "等待订单合并(自动)";
                                log.CoID = CoID;
                                logs.Add(log);
                            }
                            
                            idlist = res.d as List<int>;
                            querySql = "select id,soid from `order` where id in @ID and coid = @Coid and status <> 7";
                            var v = conn.Query<Order>(querySql,new{ID = idlist,Coid = CoID}).AsList();
                            if(v.Count > 0)
                            {
                                foreach(var c in v)
                                {
                                    var log = new LogInsert();
                                    log.OID = c.ID;
                                    log.SoID = c.SoID;
                                    log.Type = 0;
                                    log.LogDate = DateTime.Now;
                                    log.UserName = UserName;
                                    log.Title = "标记异常";
                                    log.Remark = "等待订单合并(自动)";
                                    log.CoID = CoID;
                                    logs.Add(log);
                                }
                            }
                            else
                            {
                                idlist = new List<int>();
                            }
                        }  
                        else
                        {
                            if(isCancle == true)
                            {
                                var log = new LogInsert();
                                log.OID = OrdOlder.ID;
                                log.SoID = OrdOlder.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "取消异常标记";
                                log.Remark = "等待订单合并(自动)";
                                log.CoID = CoID;
                                logs.Add(log);
                                if(OrdOlder.IsPaid == true)
                                {
                                    OrdOlder.Status = 1;
                                }
                                else
                                {
                                    OrdOlder.Status = 0;
                                }
                                OrdOlder.AbnormalStatus = 0;
                                OrdOlder.StatusDec="";
                            }
                        } 
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlCommandText = string.Empty;
                int count =0;
                OrdOlder.Modifier = UserName;
                OrdOlder.ModifyDate = DateTime.Now;
                if(x > 0)
                {
                    if(business.isskulock == 0)
                    {
                        item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + OID + " and coid = " + CoID).AsList();
                    }
                    if(decimal.Parse(OrdOlder.SkuAmount) + decimal.Parse(ExAmount) == decimal.Parse(OrdOlder.PaidAmount))
                    {
                        OrdOlder.IsPaid = true;
                        if(OrdOlder.Status != 7)
                        {
                            OrdOlder.Status = 1;
                        }
                        if(OrdOlder.Status == 7 && OrdOlder.StatusDec=="付款金额不等于应付金额")
                        {
                            OrdOlder.Status = 1;
                            OrdOlder.AbnormalStatus = 0;
                            OrdOlder.StatusDec="";
                            var log = new LogInsert();
                            log.OID = OrdOlder.ID;
                            log.SoID = OrdOlder.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "取消异常标记";
                            log.Remark = "付款金额不等于应付金额";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        if(OrdOlder.Status != 7 && OrdOlder.Type == 3)
                        {
                            OrdOlder.Status = 8;
                        }
                        //更新库存锁定量
                        if(business.isskulock == 0 && ischeckPaid == false)
                        {
                            foreach(var i in item)
                            {
                                sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                                if(count < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        OrdOlder.IsPaid = false;
                        if(OrdOlder.Status != 7)
                        {
                            if(decimal.Parse(OrdOlder.PaidAmount) == 0)
                            {
                                OrdOlder.Status = 0;
                            }
                            else
                            {
                                reasonid = GetReasonID("付款金额不等于应付金额",CoID,7).s;
                                if(reasonid == -1)
                                {
                                    result.s = -1;
                                    result.d = "请先设定【付款金额不等于应付金额】的异常";
                                    return result;
                                }
                                OrdOlder.Status = 7;
                                OrdOlder.AbnormalStatus = reasonid;
                                OrdOlder.StatusDec="付款金额不等于应付金额";
                                var log = new LogInsert();
                                log.OID = OrdOlder.ID;
                                log.SoID = OrdOlder.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "标记异常";
                                log.Remark = "付款金额不等于应付金额";
                                log.CoID = CoID;
                                logs.Add(log);
                            }                            
                        }
                        //更新库存锁定量
                        if(business.isskulock == 0 && ischeckPaid == true)
                        {
                            foreach(var i in item)
                            {
                                sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                                if(count < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    sqlCommandText = @"update `order` set ExAmount = @ExAmount,Amount = ExAmount + SkuAmount,RecLogistics=@RecLogistics,RecCity=@RecCity,RecDistrict=@RecDistrict,
                                        RecAddress=@RecAddress,RecName=@RecName,RecTel=@RecTel,RecPhone=@RecPhone,SendMessage=@SendMessage,InvoiceTitle=@InvoiceTitle,Status=@Status,
                                        AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,IsPaid=@IsPaid,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    count =CoreDBconn.Execute(sqlCommandText,OrdOlder,TransCore);
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                else
                {
                    if(y > 0)
                    {
                        sqlCommandText = @"update `order` set SendMessage=@SendMessage,InvoiceTitle=@InvoiceTitle,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                        count =CoreDBconn.Execute(sqlCommandText,OrdOlder,TransCore);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                }
                if(CancleOrdAb.ID > 0)
                {
                    sqlCommandText = @"update `order` set Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    count =CoreDBconn.Execute(sqlCommandText,CancleOrdAb,TransCore);
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                if(idlist.Count > 0)
                {
                    sqlCommandText = @"update `order` set status = 7,abnormalstatus = @Abnormalstatus,statusdec = '等待订单合并' where id in @ID and coid = @Coid and status <> 7";
                    count =CoreDBconn.Execute(sqlCommandText,new {Abnormalstatus = reasonid,ID = idlist,Coid = CoID},TransCore);
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count =CoreDBconn.Execute(loginsert,logs,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }                
                TransCore.Commit();
            }catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var ress = new UpdateOrd();
            var aa = GetOrderEdit(OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            ress.Order = aa.d as OrderEdit;
            aa = GetOrderLog(OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            ress.Log = aa.d as List<OrderLog>;
            result.d = ress;
            return  result;
        }
        ///<summary>
        ///检查订单是否需取消等待合并
        ///</summary>
        public static DataResult isCheckCancleMerge(int OID,int CoID,string BuyerShopID,string RecName,string RecLogistics,string RecCity,string RecDistrict,string RecAddress)
        {
            var result = new DataResult(1,null);   
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string wheresql = "select id from `order` where coid = " + CoID + " and id != "+ OID + " and buyershopid = '" + BuyerShopID + "' and recname = '" + RecName + 
                                      "' and reclogistics = '" + RecLogistics + "' and reccity = '" + RecCity + "' and recdistrict = '" + RecDistrict + 
                                      "' and recaddress = '" + RecAddress + "' and status = 7 and statusdec = '等待订单合并'";
                    var u = conn.Query<Order>(wheresql).AsList();
                    if(u.Count == 1)
                    {
                        result.s = 1;
                        result.d = u[0].ID;
                    }
                    else
                    {
                        result.s = 0;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            return result;
        }
        ///<summary>
        ///查询买家账号
        ///</summary>
        public static DataResult GetRecInfoList(RecInfoParm cp)
        {
            var result = new DataResult(1,null);    
            string sqlcount = "select count(id) from recinfo where 1=1";
            string sqlcommand = "select ID,BuyerId,ShopSit,Receiver,Logistics,City,District,Address,Tel,Phone from recinfo where 1=1"; 
            string wheresql = string.Empty;
            if(cp.CoID != 1)//公司编号
            {
                wheresql = wheresql + " and coid = " + cp.CoID;
            }
            if(!string.IsNullOrEmpty(cp.BuyerId))
            {
                wheresql = wheresql + " and buyerid = '" + cp.BuyerId + "'";
            }
            if(!string.IsNullOrEmpty(cp.Receiver))
            {
                wheresql = wheresql + " and receiver = '" + cp.Receiver + "'";
            }
            if(cp.ShopSit >= 0)
            {
                wheresql = wheresql + " and shopsit = " + cp.ShopSit;
            }
            if(!string.IsNullOrEmpty(cp.SortField) && !string.IsNullOrEmpty(cp.SortDirection))//排序
            {
                wheresql = wheresql + " ORDER BY "+ cp.SortField +" "+ cp.SortDirection;
            }
            var res = new RecInfoData();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    int count = conn.QueryFirst<int>(sqlcount + wheresql);
                    decimal pagecnt = Math.Ceiling(decimal.Parse(count.ToString())/decimal.Parse(cp.NumPerPage.ToString()));
                    int dataindex = (cp.PageIndex - 1)* cp.NumPerPage;
                    wheresql = wheresql + " limit " + dataindex.ToString() + " ," + cp.NumPerPage.ToString();
                    var u = conn.Query<RecInfo>(sqlcommand + wheresql).AsList();
                    res.Datacnt = count;
                    res.Pagecnt = pagecnt;
                    res.Recinfo = u;
                    result.d = res;             
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }           
            return result;
        }
        ///<summary>
        ///新增订单明细
        ///</summary>
        public static DataResult InsertOrderDetail(int id,List<int> skuid,int CoID,string Username,bool IsQuick)
        {
            var result = new DataResult(1,null);  
            var res = new OrderDetailInsert();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var logs = new List<LogInsert>();
            string sqlCommandText = "";
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select status,soid,amount,PaidAmount,AbnormalStatus,StatusDec,Type,IsPaid from `order` where id =" + id + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    if (u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以新增明细!";
                        return result;
                    }
                }
                ischeckPaid = u[0].IsPaid;
                List<InsertFailReason> rt = new List<InsertFailReason>();
                List<int> rr = new List<int>();
                decimal amt = 0, weight = 0;
                foreach (int a in skuid)
                {
                    InsertFailReason rf = new InsertFailReason();
                    string skusql = "select skuid,skuname,norm,img,goodscode,enable,saleprice,weight from coresku where id =" + a + " and coid =" + CoID;
                    var s = CoreDBconn.Query<SkuInsert>(skusql).AsList();
                    if (s.Count == 0)
                    {
                        rf.id = a;
                        rf.reason = "此商品不存在!";
                        rt.Add(rf);
                        continue;
                    }
                    else
                    {
                        if (s[0].enable == false)
                        {
                            rf.id = a;
                            rf.reason = "此商品已停用!";
                            rt.Add(rf);
                            continue;
                        }
                    }
                    int x = CoreDBconn.QueryFirst<int>("select count(id) from orderitem where oid = " + id + " and coid =" + CoID + " and skuautoid = " + a + " AND IsGift = false");
                    if (x > 0)
                    {
                        rf.id = a;
                        rf.reason = null;
                        rt.Add(rf);
                        continue;
                    }
                    sqlCommandText = @"INSERT INTO orderitem(oid,soid,coid,skuautoid,skuid,skuname,norm,GoodsCode,qty,saleprice,realprice,amount,img,weight,totalweight,DiscountRate,creator,modifier) 
                                            VALUES(@OID,@Soid,@Coid,@Skuautoid,@Skuid,@Skuname,@Norm,@GoodsCode,@Qty,@Saleprice,@Saleprice,@Saleprice,@Img,@Weight,@Weight,@Qty,@Creator,@Creator)";
                    var args = new
                    {
                        OID = id,
                        Soid = u[0].SoID,
                        Skuautoid = a,
                        Skuid = s[0].skuid,
                        Skuname = s[0].skuname,
                        Norm = s[0].norm,
                        GoodsCode = s[0].goodscode,
                        Qty = 1,
                        Saleprice = s[0].saleprice,
                        Img = s[0].img,
                        Weight = s[0].weight,
                        Coid = CoID,
                        Creator = Username
                    };
                    count = CoreDBconn.Execute(sqlCommandText, args, TransCore);
                    if (count <= 0)
                    {
                        rf.id = a;
                        rf.reason = "新增明细失败!";
                        rt.Add(rf);
                    }
                    else
                    {
                        amt += decimal.Parse(s[0].saleprice);
                        weight += decimal.Parse(s[0].weight);
                        rr.Add(a);
                        var log = new LogInsert();
                        log.OID = id;
                        log.SoID = u[0].SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = Username;
                        log.Title = "添加商品";
                        log.Remark = s[0].skuid;
                        log.CoID = CoID;
                        logs.Add(log);
                    }
                }
                //更新订单的金额和重量
                if (rr.Count > 0)
                {
                    bool IsPaid;
                    int status = u[0].Status,AbnormalStatus=u[0].AbnormalStatus;
                    string StatusDec = u[0].StatusDec;
                    if(business.isskulock == 0)
                    {
                        item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + id + " and coid = " + CoID).AsList();
                    }
                    if(decimal.Parse(u[0].Amount) + amt == decimal.Parse(u[0].PaidAmount))
                    {
                        IsPaid = true;
                        if(status != 7)
                        {
                            status = 1;
                        }
                        if(status == 7 && StatusDec == "付款金额不等于应付金额")
                        {
                            status = 1;
                            AbnormalStatus = 0;
                            StatusDec = "";
                            var log = new LogInsert();
                            log.OID = id;
                            log.SoID = u[0].SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = Username;
                            log.Title = "取消异常标记";
                            log.Remark = "付款金额不等于应付金额";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        if(status != 7 && u[0].Type == 3)
                        {
                            status = 8;
                        }
                        if(business.isskulock == 0 && ischeckPaid == false)
                        {
                            foreach(var i in item)
                            {
                                sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                                if(count < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        IsPaid = false;
                        if(status != 7)
                        {
                            if(decimal.Parse(u[0].PaidAmount) == 0)
                            {
                                status = 0;
                            }
                            else
                            {
                                int reasonid = GetReasonID("付款金额不等于应付金额",CoID,7).s;
                                if(reasonid == -1)
                                {
                                    result.s = -1;
                                    result.d = "请先设定【付款金额不等于应付金额】的异常";
                                    return result;
                                }
                                status = 7;
                                AbnormalStatus = reasonid;
                                StatusDec = "付款金额不等于应付金额";
                                var log = new LogInsert();
                                log.OID = id;
                                log.SoID = u[0].SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = Username;
                                log.Title = "标记异常";
                                log.Remark = "付款金额不等于应付金额";
                                log.CoID = CoID;
                                logs.Add(log);
                            }
                        }
                        if(business.isskulock == 0 && ischeckPaid == true)
                        {
                            foreach(var i in item)
                            {
                                sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                                if(count < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    sqlCommandText = @"update `order` set SkuAmount = SkuAmount + @SkuAmount,Amount = SkuAmount + ExAmount,ExWeight = ExWeight + @ExWeight,
                                            OrdQty = OrdQty + @OrdQty,Modifier=@Modifier,ModifyDate=@ModifyDate,IsPaid=@IsPaid,status=@Status,AbnormalStatus=@AbnormalStatus,
                                            StatusDec=@StatusDec where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlCommandText, new { SkuAmount = amt, ExWeight = weight, OrdQty = rr.Count,Modifier = Username, ModifyDate = DateTime.Now,
                                                IsPaid=IsPaid,Status=status, ID = id, CoID = CoID,AbnormalStatus=AbnormalStatus,StatusDec=StatusDec }, TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    int r = CoreDBconn.Execute(loginsert,logs, TransCore);
                    if (r < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                }
                res.successIDs = rr;
                res.failIDs = rt;
                
                if (result.s == 1)
                {
                    TransCore.Commit();
                }

                // wheresql = "select status,amount,ExWeight from `order` where id =" + id + " and coid =" + CoID;
                // u = CoreDBconn.Query<Order>(wheresql).AsList();
                // sin.Amount = u[0].Amount;
                // sin.Status = u[0].Status;
                // sin.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                // sin.Weight = u[0].ExWeight;
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            if(IsQuick == true)
            {
                var ff = RefreshOrderItemQuick(id,CoID);
                if(ff.s == -1)
                {
                    result.s = -1;
                    result.d = ff.d;
                    return result;
                }
                res.Order = ff.d as SingleOrderItem;
                result.d = res;
            }
            else
            {
                var ress = new OrderDetailInsertI();
                ress.successIDs = res.successIDs;
                ress.failIDs = res.failIDs;
                var ff = RefreshOrderItem(id,CoID);
                if(ff.s == -1)
                {
                    result.s = -1;
                    result.d = ff.d;
                    return result;
                }
                ress.Order = ff.d as RefreshItem;
                result.d = ress;
            }
            
            return result;
        }
        ///<summary>
        ///删除订单明细
        ///</summary>
        public static DataResult DeleteOrderDetail(int id,int oid,int CoID,string Username,bool IsQuick)
        {
            var result = new DataResult(1,null);  
            var logs = new List<LogInsert>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select status,soid,amount,PaidAmount,AbnormalStatus,StatusDec,Type,IsPaid from `order` where id =" + oid + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    if (u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以删除明细!";
                        return result;
                    }
                }
                ischeckPaid = u[0].IsPaid;
                decimal amt = 0, weight = 0,qty = 0;
                wheresql = @"select id,skuid,realprice,qty,amount,totalweight,isgift from orderitem where oid = " + oid + " and coid =" + CoID + " and id = " + id;
                var x = CoreDBconn.Query<OrderItem>(wheresql).AsList();
                if (x.Count > 0)
                {
                    if(x[0].IsGift == false)
                    {
                        qty += x[0].Qty;
                    }
                    amt += decimal.Parse(x[0].Amount);
                    weight += decimal.Parse(x[0].TotalWeight);
                    var log = new LogInsert();
                    log.OID = oid;
                    log.SoID = u[0].SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = Username;
                    log.Title = "删除商品";
                    log.Remark = x[0].SkuID + "(" + x[0].RealPrice + "*" + x[0].Qty + ")";
                    log.CoID = CoID;
                    logs.Add(log);
                }
                string sqlCommandText = @"delete from orderitem where oid = @OID and coid = @CoID and id = @ID";
                int count = CoreDBconn.Execute(sqlCommandText,new {OID=oid,CoID=CoID,ID = id}, TransCore);
                if (count < 0)
                {
                    result.s = -3004;
                    return result;
                }
                bool IsPaid;
                int status = u[0].Status,AbnormalStatus=u[0].AbnormalStatus;
                string StatusDec = u[0].StatusDec;
                if(business.isskulock == 0)
                {
                    item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + oid + " and coid = " + CoID).AsList();
                }
                if(decimal.Parse(u[0].Amount) - amt == decimal.Parse(u[0].PaidAmount))
                {
                    IsPaid = true;
                    if(status != 7)
                    {
                        status = 1;
                    }
                    if(status == 7 && StatusDec == "付款金额不等于应付金额")
                    {
                        status = 1;
                        AbnormalStatus = 0;
                        StatusDec = "";
                        var log = new LogInsert();
                        log.OID = oid;
                        log.SoID = u[0].SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = Username;
                        log.Title = "取消异常标记";
                        log.Remark = "付款金额不等于应付金额";
                        log.CoID = CoID;
                        logs.Add(log);
                    }
                    if(status != 7 && u[0].Type == 3)
                    {
                        status = 8;
                    }
                    if(business.isskulock == 0 && ischeckPaid == false)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    IsPaid = false;
                    if(status != 7)
                    {
                        if(decimal.Parse(u[0].PaidAmount) == 0)
                        {
                            status = 0;
                        }
                        else
                        {
                            int reasonid = GetReasonID("付款金额不等于应付金额",CoID,7).s;
                            if(reasonid == -1)
                            {
                                result.s = -1;
                                result.d = "请先设定【付款金额不等于应付金额】的异常";
                                return result;
                            }
                            status = 7;
                            AbnormalStatus = reasonid;
                            StatusDec="付款金额不等于应付金额";
                            var log = new LogInsert();
                            log.OID = oid;
                            log.SoID = u[0].SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = Username;
                            log.Title = "标记异常";
                            log.Remark = "付款金额不等于应付金额";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                    }
                    if(business.isskulock == 0 && ischeckPaid == true)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                //更新订单的金额和重量
                sqlCommandText = @"update `order` set SkuAmount = SkuAmount - @SkuAmount,Amount = SkuAmount + ExAmount,ExWeight = ExWeight - @ExWeight,
                                  OrdQty = OrdQty - @OrdQty, Modifier=@Modifier,ModifyDate=@ModifyDate,IsPaid=@IsPaid,status=@Status,AbnormalStatus=@AbnormalStatus,
                                  StatusDec=@StatusDec  where ID = @ID and CoID = @CoID";
                count = CoreDBconn.Execute(sqlCommandText, new { SkuAmount = amt, ExWeight = weight, OrdQty = qty,Modifier = Username, ModifyDate = DateTime.Now, 
                                            IsPaid=IsPaid,Status=status,ID = oid, CoID = CoID,AbnormalStatus=AbnormalStatus,StatusDec=StatusDec }, TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int r = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (r < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
                // wheresql = "select status,amount,ExWeight from `order` where id =" + oid + " and coid =" + CoID;
                // u = CoreDBconn.Query<Order>(wheresql).AsList();
                // sin.Amount = u[0].Amount;
                // sin.Status = u[0].Status;
                // sin.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                // sin.Weight = u[0].ExWeight;
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            if(IsQuick == true)
            {
                var ff = RefreshOrderItemQuick(oid,CoID);
                if(ff.s == -1)
                {
                    result.s = -1;
                    result.d = ff.d;
                    return result;
                }
                result.d = ff.d;
            }
            else
            {
                var ff = RefreshOrderItem(oid,CoID);
                if(ff.s == -1)
                {
                    result.s = -1;
                    result.d = ff.d;
                    return result;
                }
                result.d = ff.d;
            }
            return result;
        }
        ///<summary>
        ///更新订单明细
        ///</summary>
        public static DataResult UpdateOrderDetail(int id,int oid,int CoID,string Username,decimal price,int qty,string SkuName,bool IsQuick)
        {
            var result = new DataResult(1,null);  
            var logs = new List<LogInsert>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select status,soid,amount,PaidAmount,AbnormalStatus,StatusDec,Type,IsPaid from `order` where id =" + oid + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    if (u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以修改明细!";
                        return result;
                    }
                }
                ischeckPaid = u[0].IsPaid;
                string sqlCommandText = "update orderitem set ";
                var p = new DynamicParameters();
                decimal amt = 0, weight = 0,pricenew = 0,qtynew = 0;
                wheresql = @"select id,skuid,realprice,qty,amount,totalweight,weight,saleprice,skuname,isgift from orderitem 
                             where oid = " + oid + " and coid =" + CoID + " and id = " + id;
                var x = CoreDBconn.Query<OrderItem>(wheresql).AsList();
                if (x.Count > 0)
                {
                    amt = decimal.Parse(x[0].Amount);
                    weight = decimal.Parse(x[0].TotalWeight);
                    pricenew = decimal.Parse(x[0].RealPrice);
                    qtynew = x[0].Qty;
                    if(price != -1)
                    {
                        if(price != decimal.Parse(x[0].RealPrice))
                        {
                            pricenew = price;
                            sqlCommandText = sqlCommandText + "realprice = @Realprice,";
                            p.Add("@Realprice", price);
                            var log = new LogInsert();
                            log.OID = oid;
                            log.SoID = u[0].SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = Username;
                            log.Title = "修改单价";
                            log.Remark = x[0].SkuID + " " + x[0].RealPrice + "=>" + price;
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                    }
                    if(qty != -1)
                    {
                        if(qty != x[0].Qty)
                        {
                            qtynew = qty;
                            sqlCommandText = sqlCommandText + "qty = @Qty,";
                            p.Add("@Qty", qty);
                            var log = new LogInsert();
                            log.OID = oid;
                            log.SoID = u[0].SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = Username;
                            log.Title = "修改数量";
                            log.Remark = x[0].SkuID+ " " + x[0].Qty + "=>" + qtynew;
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                    }
                    if(SkuName != null)
                    {
                        if(SkuName != x[0].SkuName)
                        {
                            sqlCommandText = sqlCommandText + "SkuName = @SkuName,";
                            p.Add("@SkuName", SkuName);
                            var log = new LogInsert();
                            log.OID = oid;
                            log.SoID = u[0].SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = Username;
                            log.Title = "修改商品名称";
                            log.Remark = x[0].SkuID + " " + x[0].SkuName + "=>" + SkuName;
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                    }
                    sqlCommandText = sqlCommandText + "amount = @Amount,DiscountRate = @DiscountRate,TotalWeight=@TotalWeight,modifier=@Modifier,ModifyDate=@ModifyDate " + 
                                                       "where oid = @Oid and coid = @Coid and id = @ID";
                    p.Add("@Amount", pricenew * qtynew);
                    p.Add("@DiscountRate", pricenew/decimal.Parse(x[0].SalePrice));
                    p.Add("@TotalWeight", qtynew * decimal.Parse(x[0].Weight));
                    p.Add("@Modifier", Username);
                    p.Add("@ModifyDate", DateTime.Now);
                    p.Add("@Oid", oid);
                    p.Add("@Coid", CoID);
                    p.Add("@ID", id);
                    int count = CoreDBconn.Execute(sqlCommandText, p, TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    bool IsPaid;
                    int status = u[0].Status,AbnormalStatus = u[0].AbnormalStatus;
                    string StatusDec = u[0].StatusDec;
                    if(business.isskulock == 0)
                    {
                        item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + oid + " and coid = " + CoID).AsList();
                    }
                    if(decimal.Parse(u[0].Amount) + pricenew * qtynew - amt == decimal.Parse(u[0].PaidAmount))
                    {
                        IsPaid = true;
                        if(status != 7)
                        {
                            status = 1;
                        }
                        if(status == 7 && StatusDec == "付款金额不等于应付金额")
                        {
                            status = 1;
                            AbnormalStatus = 0;
                            StatusDec = "";
                            var log = new LogInsert();
                            log.OID = oid;
                            log.SoID = u[0].SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = Username;
                            log.Title = "取消异常标记";
                            log.Remark = "付款金额不等于应付金额";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        if(status != 7 && u[0].Type == 3)
                        {
                            status = 8;
                        }
                        if(business.isskulock == 0 && ischeckPaid == false)
                        {
                            foreach(var i in item)
                            {
                                sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                                if(count < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        IsPaid = false;
                        if(status != 7)
                        {
                            if(decimal.Parse(u[0].PaidAmount) == 0)
                            {
                                status = 0;
                            }
                            else
                            {
                                int reasonid = GetReasonID("付款金额不等于应付金额",CoID,7).s;
                                if(reasonid == -1)
                                {
                                    result.s = -1;
                                    result.d = "请先设定【付款金额不等于应付金额】的异常";
                                    return result;
                                }
                                status = 7;
                                AbnormalStatus = reasonid;
                                StatusDec="付款金额不等于应付金额";
                                var log = new LogInsert();
                                log.OID = oid;
                                log.SoID = u[0].SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = Username;
                                log.Title = "标记异常";
                                log.Remark = "付款金额不等于应付金额";
                                log.CoID = CoID;
                                logs.Add(log);
                            }
                        }
                        if(business.isskulock == 0 && ischeckPaid == true)
                        {
                            foreach(var i in item)
                            {
                                sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                                if(count < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    decimal Ordqty = 0;
                    if(x[0].IsGift == false)
                    {
                        Ordqty = qtynew - x[0].Qty;
                    }
                    sqlCommandText = @"update `order` set SkuAmount = SkuAmount + @SkuAmount,Amount = SkuAmount + ExAmount,ExWeight = ExWeight + @ExWeight,
                                        ordqty = ordqty + @Ordqty ,Modifier=@Modifier,ModifyDate=@ModifyDate,IsPaid=@IsPaid,status=@Status,AbnormalStatus=@AbnormalStatus, 
                                        StatusDec =@StatusDec where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlCommandText, new { SkuAmount = pricenew * qtynew - amt, ExWeight = qtynew * decimal.Parse(x[0].Weight) - weight,
                                                Ordqty = Ordqty, Modifier = Username, ModifyDate = DateTime.Now,IsPaid=IsPaid,Status=status, ID = oid, CoID = CoID,
                                                AbnormalStatus=AbnormalStatus, StatusDec = StatusDec}, TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    int r = CoreDBconn.Execute(loginsert,logs, TransCore);
                    if (r < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                }
                TransCore.Commit();
                // wheresql = "select status,amount,ExWeight from `order` where id =" + oid + " and coid =" + CoID;
                // u = CoreDBconn.Query<Order>(wheresql).AsList();
                // sin.Amount = u[0].Amount;
                // sin.Status = u[0].Status;
                // sin.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                // sin.Weight = u[0].ExWeight;
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            if(IsQuick == true)
            {
                var ff = RefreshOrderItemQuick(oid,CoID);
                if(ff.s == -1)
                {
                    result.s = -1;
                    result.d = ff.d;
                    return result;
                }
                result.d = ff.d;
            }
            else
            {
                var ff = RefreshOrderItem(oid,CoID);
                if(ff.s == -1)
                {
                    result.s = -1;
                    result.d = ff.d;
                    return result;
                }
                result.d = ff.d;
            }
            return result;
        }
        ///<summary>
        ///手工支付
        ///</summary>
        public static DataResult ManualPay(PayInfo pay,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select * from `order` where id =" + pay.OID + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                var ord = new Order();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    ord = u[0] as Order;
                    if (ord.Status != 0 && ord.Status != 1 && ord.Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以新增付款!";
                        return result;
                    }
                }
                ischeckPaid = ord.IsPaid;
                pay.SoID = ord.SoID;
                decimal PaidAmount=0,PayAmount=0,Amount=0;
                if(!string.IsNullOrEmpty(ord.PaidAmount))
                {
                    PaidAmount = decimal.Parse(ord.PaidAmount);
                }
                if(!string.IsNullOrEmpty(ord.PayAmount))
                {
                    PayAmount = decimal.Parse(ord.PayAmount);
                }
                if(!string.IsNullOrEmpty(ord.Amount))
                {
                    Amount = decimal.Parse(ord.Amount);
                }
                if(Amount - PaidAmount <= 0)
                {
                    result.s = -1;
                    result.d = "该笔订单已完成支付，不需再支付!";
                    return result;
                }
                pay.RecID = ord.BuyerID;
                pay.RecName = ord.RecName;
                pay.Title = ord.InvoiceTitle;
                pay.DataSource = 0;
                pay.Status = 1;
                pay.CoID = CoID;
                pay.Creator = UserName;
                pay.CreateDate = DateTime.Now;
                pay.Confirmer = UserName;
                pay.ConfirmDate = DateTime.Now;
                pay.BuyerShopID = ord.BuyerShopID;
                var log = new LogInsert();
                log.OID = pay.OID;
                log.SoID = pay.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "添加支付";
                log.Remark = pay.Payment + pay.PayAmount;
                log.CoID = CoID;
                logs.Add(log);
                if(PaidAmount + decimal.Parse(pay.PayAmount) != Amount &&　ord.Status != 7 )
                {
                    log = new LogInsert();
                    log.OID = pay.OID;
                    log.SoID = pay.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "判断金额不符";
                    log.Remark = "标记付款金额不等于应付金额";
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 7;
                    var rss = GetReasonID("付款金额不等于应付金额",CoID,7);
                    if(rss.s == -1)
                    {
                        result.s = -1;
                        result.d = rss.d;
                        return result;
                    }
                    ord.AbnormalStatus = rss.s;
                    ord.StatusDec = "付款金额不等于应付金额";
                }
                if(PaidAmount + decimal.Parse(pay.PayAmount) == Amount &&　ord.StatusDec == "付款金额不等于应付金额" && ord.Status == 7)
                {
                    log = new LogInsert();
                    log.OID = pay.OID;
                    log.SoID = pay.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消异常标记";
                    log.Remark = ord.StatusDec;
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 1;
                    ord.AbnormalStatus = 0;
                    ord.StatusDec = "";
                }
                log = new LogInsert();
                log.OID = pay.OID;
                log.SoID = pay.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "支付单确认";
                log.CoID = CoID;
                logs.Add(log);
                //新增支付单资料
                string sqlCommandText = @"INSERT INTO payinfo(PayNbr,RecID,RecName,OID,SOID,Payment,PayAccount,PayDate,Title,Amount,PayAmount,DataSource,Status,CoID,Creator,CreateDate,Confirmer,ConfirmDate,BuyerShopID) 
                                    VALUES(@PayNbr,@RecID,@RecName,@OID,@SOID,@Payment,@PayAccount,@PayDate,@Title,@Amount,@PayAmount,@DataSource,@Status,@CoID,@Creator,@CreateDate,@Confirmer,@ConfirmDate,@BuyerShopID)";
                int count = CoreDBconn.Execute(sqlCommandText,pay,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                //更新订单
                if(business.isskulock == 0)
                {
                    item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + pay.OID + " and coid = " + CoID).AsList();
                }
                ord.PaidAmount = (PaidAmount + decimal.Parse(pay.PayAmount)).ToString();
                ord.PayAmount = (PayAmount + decimal.Parse(pay.PayAmount)).ToString();
                if(ord.PayDate == null || ord.PayDate <= DateTime.Parse("1900-01-01"))
                {
                    ord.PayDate = pay.PayDate;
                }
                if(string.IsNullOrEmpty(ord.PayNbr))
                {
                    ord.PayNbr = pay.PayNbr;
                }
                if(ord.PaidAmount == ord.Amount)
                {
                    ord.IsPaid = true;
                    if(ord.Status != 7)
                    {
                        ord.Status = 1;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    if(ord.Status != 7 && ord.Type == 3)
                    {
                        ord.Status = 8;
                    }
                    if(business.isskulock == 0 && ischeckPaid == false)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    ord.IsPaid = false;
                    if(ord.Status != 7)
                    {
                        ord.Status = 0;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    if(business.isskulock == 0 && ischeckPaid == true)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                ord.Modifier = UserName;
                ord.ModifyDate = DateTime.Now;
                sqlCommandText = @"update `order` set PaidAmount = @PaidAmount,PayAmount = @PayAmount,PayDate =@PayDate,PayNbr = @PayNbr,IsPaid=@IsPaid,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                    StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                count = CoreDBconn.Execute(sqlCommandText,ord, TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var res = new UpdatePay();
            var aa = GetOrderEdit(pay.OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Order = aa.d as OrderEdit;

            aa = GetOrderPay(pay.OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Pay = aa.d as List<OrderPay>;

            aa = GetOrderLog(pay.OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Log = aa.d as List<OrderLog>;
            result.d = res;
            return result;
        }
        ///<summary>
        ///取消支付审核
        ///</summary>
        public static DataResult CancleConfirmPay(int oid,int payid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select * from `order` where id =" + oid  + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                var ord = new Order();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    ord = u[0] as Order;
                    if (ord.Status != 0 && ord.Status != 1 && ord.Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以取消核准付款";
                        return result;
                    }
                }
                ischeckPaid = ord.IsPaid;
                decimal PaidAmount=0,PayAmount=0,Amount=0;
                if(!string.IsNullOrEmpty(ord.PaidAmount))
                {
                    PaidAmount = decimal.Parse(ord.PaidAmount);
                }
                if(!string.IsNullOrEmpty(ord.PayAmount))
                {
                    PayAmount = decimal.Parse(ord.PayAmount);
                }
                if(!string.IsNullOrEmpty(ord.Amount))
                {
                    Amount = decimal.Parse(ord.Amount);
                }
                var log = new LogInsert();
                log.OID = oid;
                log.SoID = ord.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "支付单取消确认";
                log.CoID = CoID;
                logs.Add(log);
                wheresql = "select PayAmount,status from payinfo where id =" + payid + " and coid =" + CoID;
                var payinfo = CoreDBconn.Query<PayInfo>(wheresql).AsList();
                if(payinfo.Count == 0)
                {
                    result.s = -1;
                    result.d = "该笔支付单参数异常!";
                    return result;
                }
                else
                {
                    if(payinfo[0].Status != 1)
                    {
                        result.s = -1;
                        result.d = "该笔支付单不可取消确认!";
                        return result;
                    }
                }
                decimal pay = decimal.Parse(payinfo[0].PayAmount);
                if(PaidAmount - pay != Amount && PaidAmount - pay > 0 &&　ord.Status != 7)
                {
                    log = new LogInsert();
                    log.OID = oid;
                    log.SoID = ord.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "判断金额不符";
                    log.Remark = "标记付款金额不等于应付金额";
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 7;
                    var rss = GetReasonID("付款金额不等于应付金额",CoID,7);
                    if(rss.s == -1)
                    {
                        result.s = -1;
                        result.d = rss.d;
                        return result;
                    }
                    ord.AbnormalStatus = rss.s;
                    ord.StatusDec = "付款金额不等于应付金额";
                }
                if(PaidAmount - pay == 0 &&　ord.StatusDec == "付款金额不等于应付金额" && ord.Status == 7)
                {
                    log = new LogInsert();
                    log.OID = oid;
                    log.SoID = ord.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消异常标记";
                    log.Remark = ord.StatusDec;
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 0;
                    ord.AbnormalStatus = 0;
                    ord.StatusDec = "";
                }
                if(PaidAmount - pay == Amount &&　ord.StatusDec == "付款金额不等于应付金额" && ord.Status == 7)
                {
                    log = new LogInsert();
                    log.OID = oid;
                    log.SoID = ord.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消异常标记";
                    log.Remark = ord.StatusDec;
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 1;
                    ord.AbnormalStatus = 0;
                    ord.StatusDec = "";
                }
                //更新支付单资料
                string sqlCommandText = @"update payinfo set Status = 0,Confirmer=@Confirmer,ConfirmDate = @ConfirmDate where id = @ID and coid = @Coid";
                int count = CoreDBconn.Execute(sqlCommandText,new {Confirmer = "",ConfirmDate=new DateTime(),ID = payid,Coid = CoID },TransCore);
                if(count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                //更新订单
                if(business.isskulock == 0)
                {
                    item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + oid + " and coid = " + CoID).AsList();
                }
                ord.PaidAmount = (PaidAmount - pay).ToString();
                ord.PayAmount = (PayAmount - pay).ToString();
                ord.IsPaid = false;
                if(decimal.Parse(ord.PaidAmount) == 0)
                {
                    ord.PayDate = new DateTime();
                    ord.PayNbr = null;
                }
                if(ord.PaidAmount == ord.Amount)
                {
                    ord.IsPaid = true;
                    if(ord.Status != 7)
                    {
                        ord.Status = 1;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    if(ord.Status != 7 && ord.Type == 3)
                    {
                        ord.Status = 8;
                    }
                    if(business.isskulock == 0 && ischeckPaid == false)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    ord.IsPaid = false;
                    if(ord.Status != 7)
                    {
                        ord.Status = 0;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    if(business.isskulock == 0 && ischeckPaid == true)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                ord.Modifier = UserName;
                ord.ModifyDate = DateTime.Now;
                sqlCommandText = @"update `order` set PaidAmount = @PaidAmount,PayAmount = @PayAmount,PayDate =@PayDate,PayNbr = @PayNbr,IsPaid=@IsPaid,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                    StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                count = CoreDBconn.Execute(sqlCommandText,ord, TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            
            var res = new UpdatePay();
            var aa = GetOrderEdit(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Order = aa.d as OrderEdit;

            aa = GetOrderPay(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Pay = aa.d as List<OrderPay>;

            aa = GetOrderLog(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Log = aa.d as List<OrderLog>;
            result.d = res;

            return result;
        }
        ///<summary>
        ///支付审核
        ///</summary>
        public static DataResult ConfirmPay(int oid,int payid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select * from `order` where id =" + oid  + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                var ord = new Order();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    ord = u[0] as Order;
                    if (ord.Status != 0 && ord.Status != 1 && ord.Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以核准付款!";
                        return result;
                    }
                }
                ischeckPaid = u[0].IsPaid;
                decimal PaidAmount=0,PayAmount=0,Amount=0;
                if(!string.IsNullOrEmpty(ord.PaidAmount))
                {
                    PaidAmount = decimal.Parse(ord.PaidAmount);
                }
                if(!string.IsNullOrEmpty(ord.PayAmount))
                {
                    PayAmount = decimal.Parse(ord.PayAmount);
                }
                if(!string.IsNullOrEmpty(ord.Amount))
                {
                    Amount = decimal.Parse(ord.Amount);
                }
                if(Amount - PaidAmount <= 0)
                {
                    result.s = -1;
                    result.d = "该笔订单已完成支付，不需再支付!";
                    return result;
                }
                var log = new LogInsert();
                log.OID = oid;
                log.SoID = ord.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "支付单确认";
                log.CoID = CoID;
                logs.Add(log);
                wheresql = "select PayAmount,status,paydate,paynbr from payinfo where id =" + payid + " and coid =" + CoID;
                var payinfo = CoreDBconn.Query<PayInfo>(wheresql).AsList();
                if(payinfo.Count == 0)
                {
                    result.s = -1;
                    result.d = "该笔支付单参数异常!";
                    return result;
                }
                else
                {
                    if(payinfo[0].Status != 0)
                    {
                        result.s = -1;
                        result.d = "该笔支付单不可确认!";
                        return result;
                    }
                }
                decimal pay = decimal.Parse(payinfo[0].PayAmount);
                if(PaidAmount +  pay != Amount &&　ord.Status != 7)
                {
                    log = new LogInsert();
                    log.OID = oid;
                    log.SoID = ord.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "判断金额不符";
                    log.Remark = "标记付款金额不等于应付金额";
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 7;
                    var rss = GetReasonID("付款金额不等于应付金额",CoID,7);
                    if(rss.s == -1)
                    {
                        result.s = -1;
                        result.d = rss.d;
                        return result;
                    }
                    ord.AbnormalStatus = rss.s;
                    ord.StatusDec = "付款金额不等于应付金额";
                }
                if(PaidAmount + pay == Amount &&　ord.StatusDec == "付款金额不等于应付金额" && ord.Status == 7)
                {
                    log = new LogInsert();
                    log.OID = oid;
                    log.SoID = ord.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消异常标记";
                    log.Remark = ord.StatusDec;
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 1;
                    ord.AbnormalStatus = 0;
                    ord.StatusDec = "";
                }
                //更新支付单资料
                string sqlCommandText = @"update payinfo set Status = 1,Confirmer=@Confirmer,ConfirmDate = @ConfirmDate where id = @ID and coid = @Coid";
                int count = CoreDBconn.Execute(sqlCommandText,new {Confirmer = UserName,ConfirmDate=DateTime.Now,ID = payid,Coid = CoID },TransCore);
                if(count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                //更新订单
                if(business.isskulock == 0)
                {
                    item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + oid + " and coid = " + CoID).AsList();
                }
                ord.PaidAmount = (PaidAmount + pay).ToString();
                ord.PayAmount = (PayAmount + pay).ToString();
                if(ord.PayDate == null || ord.PayDate <= DateTime.Parse("1900-01-01"))
                {
                    ord.PayDate = payinfo[0].PayDate;
                }
                if(string.IsNullOrEmpty(ord.PayNbr))
                {
                    ord.PayNbr = payinfo[0].PayNbr;
                }
                if(ord.PaidAmount == ord.Amount)
                {
                    ord.IsPaid = true;
                    if(ord.Status != 7)
                    {
                        ord.Status = 1;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    if(ord.Status != 7 && ord.Type == 3)
                    {
                        ord.Status = 8;
                    }
                    if(business.isskulock == 0 && ischeckPaid == false)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    ord.IsPaid = false;
                    if(ord.Status != 7)
                    {
                        ord.Status = 0;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    if(business.isskulock == 0 && ischeckPaid == true)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                ord.Modifier = UserName;
                ord.ModifyDate = DateTime.Now;
                sqlCommandText = @"update `order` set PaidAmount = @PaidAmount,PayAmount = @PayAmount,PayDate =@PayDate,PayNbr = @PayNbr,IsPaid=@IsPaid,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                    StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                count = CoreDBconn.Execute(sqlCommandText,ord, TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var res = new UpdatePay();
            var aa = GetOrderEdit(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Order = aa.d as OrderEdit;

            aa = GetOrderPay(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Pay = aa.d as List<OrderPay>;

            aa = GetOrderLog(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Log = aa.d as List<OrderLog>;
            result.d = res;
            return result;
        }
        ///<summary>
        ///支付作废
        ///</summary>
        public static DataResult CanclePay(int oid,int payid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select status,soid from `order` where id =" + oid + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    if (u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以作废付款!";
                        return result;
                    }
                }
                var log = new LogInsert();
                log.OID = oid;
                log.SoID = u[0].SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "支付单作废";
                log.CoID = CoID;
                logs.Add(log);
                wheresql = "select status from payinfo where id =" + payid + " and coid =" + CoID;
                var payinfo = CoreDBconn.Query<PayInfo>(wheresql).AsList();
                if(payinfo.Count == 0)
                {
                    result.s = -1;
                    result.d = "该笔支付单参数异常!";
                    return result;
                }
                else
                {
                    if(payinfo[0].Status != 0)
                    {
                        result.s = -1;
                        result.d = "该笔支付单不可作废!";
                        return result;
                    }
                }
                //更新支付单资料
                string sqlCommandText = @"update payinfo set Status = 2 where id = " + payid + " and coid = " + CoID;
                int count = CoreDBconn.Execute(sqlCommandText,TransCore);
                if(count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var res = new CanclePay();
            var aa = GetOrderPay(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Pay = aa.d as List<OrderPay>;

            aa = GetOrderLog(oid,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Log = aa.d as List<OrderLog>;
            result.d = res;
            return result;
        }
        ///<summary>
        ///快速支付
        ///</summary>
        public static DataResult QuickPay(int id,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var pay = new PayInfo();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select * from `order` where id =" + id + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                var ord = new Order();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    ord = u[0] as Order;
                    if (ord.Status != 0 && ord.Status != 1 && ord.Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以新增付款!";
                        return result;
                    }
                }
                ischeckPaid = ord.IsPaid;
                decimal PaidAmount=0,PayAmount=0,Amount=0;
                if(!string.IsNullOrEmpty(ord.PaidAmount))
                {
                    PaidAmount = decimal.Parse(ord.PaidAmount);
                }
                if(!string.IsNullOrEmpty(ord.PayAmount))
                {
                    PayAmount = decimal.Parse(ord.PayAmount);
                }
                if(!string.IsNullOrEmpty(ord.Amount))
                {
                    Amount = decimal.Parse(ord.Amount);
                }
                if(Amount - PaidAmount <= 0)
                {
                    result.s = -1;
                    result.d = "该笔订单已完成支付，不需再支付!";
                    return result;
                }
                pay.RecID = ord.BuyerID;
                pay.RecName = ord.RecName;
                pay.Title = ord.InvoiceTitle;
                pay.DataSource = 0;
                pay.Status = 1;
                pay.CoID = CoID;
                pay.OID = id;
                pay.SoID = u[0].SoID;
                pay.Payment = "快速支付";
                pay.PayNbr = "S" + DateTime.Now.Ticks.ToString().Substring(0,12);
                pay.PayDate = DateTime.Now;
                pay.PayAmount = (Amount - PaidAmount).ToString();
                pay.Amount = (Amount - PaidAmount).ToString();
                pay.Creator = UserName;
                pay.CreateDate = DateTime.Now;
                pay.Confirmer = UserName;
                pay.ConfirmDate = DateTime.Now;
                pay.BuyerShopID = ord.BuyerShopID;
                var log = new LogInsert();
                log.OID = pay.OID;
                log.SoID = pay.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "添加支付";
                log.Remark = "快速支付" + pay.PayAmount;
                log.CoID = CoID;
                logs.Add(log);
                if(ord.StatusDec == "付款金额不等于应付金额" && ord.Status == 7)
                {
                    log = new LogInsert();
                    log.OID = pay.OID;
                    log.SoID = pay.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消异常标记";
                    log.Remark = ord.StatusDec;
                    log.CoID = CoID;
                    logs.Add(log);
                    ord.Status = 1;
                    ord.AbnormalStatus = 0;
                    ord.StatusDec = "";
                }
                if(ord.Status != 7)
                {
                    ord.Status = 1;
                    ord.AbnormalStatus = 0;
                    ord.StatusDec = "";
                }
                if(ord.Status != 7 && ord.Type == 3)
                {
                    ord.Status = 8;
                }
                log = new LogInsert();
                log.OID = pay.OID;
                log.SoID = pay.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "支付单确认";
                log.CoID = CoID;
                logs.Add(log);
                //新增支付单资料
                string sqlCommandText = @"INSERT INTO payinfo(PayNbr,RecID,RecName,OID,SOID,Payment,PayAccount,PayDate,Title,Amount,PayAmount,DataSource,Status,CoID,Creator,CreateDate,Confirmer,ConfirmDate,BuyerShopID) 
                                    VALUES(@PayNbr,@RecID,@RecName,@OID,@SOID,@Payment,@PayAccount,@PayDate,@Title,@Amount,@PayAmount,@DataSource,@Status,@CoID,@Creator,@CreateDate,@Confirmer,@ConfirmDate,@BuyerShopID)";
                int count = CoreDBconn.Execute(sqlCommandText,pay,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                //更新订单
                if(business.isskulock == 0)
                {
                    item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + id + " and coid = " + CoID).AsList();
                    if(ischeckPaid == false)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                ord.PaidAmount = Amount.ToString();
                ord.PayAmount = (Amount - PaidAmount + decimal.Parse(ord.PayAmount)).ToString();
                if(ord.PayDate == null || ord.PayDate <= DateTime.Parse("1900-01-01"))
                {
                    ord.PayDate = pay.PayDate;
                }
                if(string.IsNullOrEmpty(ord.PayNbr))
                {
                    ord.PayNbr = pay.PayNbr;
                }
                ord.IsPaid = true;
                ord.Modifier = UserName;
                ord.ModifyDate = DateTime.Now;
                sqlCommandText = @"update `order` set PaidAmount = @PaidAmount,PayAmount = @PayAmount,PayDate =@PayDate,PayNbr = @PayNbr,IsPaid=@IsPaid,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                    StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                count = CoreDBconn.Execute(sqlCommandText,ord, TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                 count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var res = new UpdatePay();
            var aa = GetOrderEdit(pay.OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Order = aa.d as OrderEdit;

            aa = GetOrderPay(pay.OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Pay = aa.d as List<OrderPay>;

            aa = GetOrderLog(pay.OID,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Log = aa.d as List<OrderLog>;
            result.d = res;
            return result;
        }
        ///<summary>
        ///抓取异常List
        ///</summary>
        public static DataResult GetAbnormalList(int CoID,int status)
        {
            var result = new DataResult(1,null);
            string sqlcommand = "select ID as value,Name as label,iscustom from orderabnormal where coid =" + CoID + " and OrdStatus = " + status + " order by IsCustom asc,ID asc"; 
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    var u = conn.Query<OrderAbnormal>(sqlcommand).AsList();
                    result.d = u;             
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }    
            return result;
        }
        ///<summary>
        ///异常单转正常
        ///</summary>
        public static DataResult TransferNormal(List<int> oid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            var logs = new List<LogInsert>();
            var res = new TransferNormalReturn();
            var su = new List<TransferNormalReturnSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var confrimID = new List<int>();
            string sqlCommandText = string.Empty;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select id,soid,StatusDec,IsPaid,coid,status,Type from `order` where id in @ID and coid = @Coid";
                var u = CoreDBconn.Query<Order>(wheresql,new {ID = oid,Coid = CoID}).AsList();
                foreach(var a in u)
                {
                    if(a.Status != 7)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "异常状态才可以转正常单";
                        fa.Add(ff);
                        continue;
                    }
                    if(business.isautogoodsreviewed == true && a.StatusDec == "缺货")
                    {
                        //检查订单是否自动判断为缺货
                        wheresql = "select Remark from orderlog where oid = " + a.ID + " and coid = " + CoID + " and type = 0 order by LogDate desc";
                        string Remark = CoreDBconn.QueryFirst<string>(wheresql);
                        if(Remark == "缺货(提交仓库时判断)")
                        {
                            confrimID.Add(a.ID);
                        }
                    }
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消异常标记";
                    log.Remark = a.StatusDec;
                    log.CoID = CoID;
                    logs.Add(log);
                    var ss = new TransferNormalReturnSuccess();
                    ss.ID = a.ID;
                    if(a.IsPaid == true)
                    {
                        if(a.Type == 3)
                        {
                            a.Status = 8;
                            ss.Status = 8;
                            ss.StatusDec = "等供销商发货";
                        }
                        else
                        {
                            a.Status = 1;
                            ss.Status = 1;
                            ss.StatusDec = "已付款待审核";
                        }
                    }
                    else
                    {
                        a.Status = 0;
                        ss.Status = 0;
                        ss.StatusDec = "待付款";
                    }
                    su.Add(ss);
                    a.AbnormalStatus = 0;
                    a.StatusDec = "";
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    int count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }                
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int j = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (j < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            if(confrimID.Count > 0)
            {
                ConfirmOrder(confrimID,CoID,UserName);
            }
            return result;
        }
        ///<summary>
        ///订单合并资料筛选
        ///</summary>
        public static DataResult GetMergeOrd(int oid,int CoID)
        {
            var result = new DataResult(1,null);
            var res = new List<MergerOrd>();
            string sqlcommand = "select * from `order` where id = " + oid + " and coid = " + CoID; 
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    var u = conn.Query<Order>(sqlcommand).AsList();
                    if(u.Count == 0)
                    {
                        result.s = -1;
                        result.d = "内部订单号无效!";
                        return result;
                    }
                    else
                    {
                        if(u[0].Status != 1 &&　u[0].Status != 2)
                        {
                            result.s = -1;
                            result.d = "此订单状态不符合合并的条件!";
                            return result;
                        }
                        if(u[0].DealerType == 2)
                        {
                            result.s = -1;
                            result.d = "供销订单不允许合并,请联系分销商合并订单!";
                            return result;
                        }
                    }
                    var rr = new MergerOrd();
                    rr.type = "A";
                    sqlcommand = "select id,img,qty from orderitem  where oid = " + oid + " and coid = " + CoID;
                    var sku = conn.Query<SkuMerge>(sqlcommand).AsList();
                    u[0].Sku = sku;
                    rr.MOrd = u;
                    res.Add(rr);
                    //抓取推荐合并项
                    sqlcommand = "select * from `order` where coid = " + CoID + " and buyershopid = '" + u[0].BuyerShopID + "' and recname = '" + u[0].RecName + 
                                "' and reclogistics = '" + u[0].RecLogistics + "' and reccity = '" + u[0].RecCity + "' and recdistrict = '" + u[0].RecDistrict + 
                                "' and recaddress = '" + u[0].RecAddress + "' and status in (1,7,2) and IsPaid = true and id != " + oid;
                    var tt = conn.Query<Order>(sqlcommand).AsList();
                    rr = new MergerOrd();
                    rr.type = "L";
                    if(tt.Count > 0)
                    {
                        foreach(var a in tt)
                        {
                            sqlcommand = "select id,img,qty from orderitem  where oid = " + a.ID + " and coid = " + CoID;
                            sku = conn.Query<SkuMerge>(sqlcommand).AsList();
                            a.Sku = sku;
                        }
                    }
                    rr.MOrd = tt;
                    res.Add(rr);
                    //抓取中风险合并项
                    sqlcommand = "select * from `order` where coid = " + CoID + " and status in (1,7,2) and IsPaid = true" + " and ((" + 
                                "buyershopid = '" + u[0].BuyerShopID + "' and ( recname != '" + u[0].RecName + 
                                "' or reclogistics != '" + u[0].RecLogistics + "' or reccity != '" + u[0].RecCity + "' or recdistrict != '" + u[0].RecDistrict + 
                                "' or recaddress != '" + u[0].RecAddress + "')) or (" + 
                                "buyershopid != '" + u[0].BuyerShopID + "' and recname = '" + u[0].RecName + 
                                "' and reclogistics = '" + u[0].RecLogistics + "' and reccity = '" + u[0].RecCity + "' and recdistrict = '" + u[0].RecDistrict + 
                                "' and recaddress = '" + u[0].RecAddress + "'))" ;
                    tt = conn.Query<Order>(sqlcommand).AsList();
                    rr = new MergerOrd();
                    rr.type = "M";
                    if(tt.Count > 0)
                    {
                        foreach(var a in tt)
                        {
                            sqlcommand = "select id,img,qty from orderitem  where oid = " + a.ID + " and coid = " + CoID;
                            sku = conn.Query<SkuMerge>(sqlcommand).AsList();
                            a.Sku = sku;
                        }
                    }
                    rr.MOrd = tt;
                    res.Add(rr);
                    //抓取高风险合并项
                    sqlcommand = "select * from `order` where coid = " + CoID + " and buyershopid = '" + u[0].BuyerShopID + "' and recname = '" + u[0].RecName + 
                                "' and reclogistics = '" + u[0].RecLogistics + "' and reccity = '" + u[0].RecCity + "' and recdistrict = '" + u[0].RecDistrict + 
                                "' and recaddress = '" + u[0].RecAddress + "' and status in (0,7) and IsPaid = false";
                    tt = conn.Query<Order>(sqlcommand).AsList();
                    rr = new MergerOrd();
                    rr.type = "H";
                    if(tt.Count > 0)
                    {
                        foreach(var a in tt)
                        {
                            sqlcommand = "select id,img,qty from orderitem  where oid = " + a.ID + " and coid = " + CoID;
                            sku = conn.Query<SkuMerge>(sqlcommand).AsList();
                            a.Sku = sku;
                        }
                    }
                    rr.MOrd = tt;
                    res.Add(rr);
                    
                    result.d = res;             
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }    
            return result;
        }
        ///<summary>
        ///订单合并
        ///</summary>
        public static DataResult OrdMerger(int oid,List<int> MID,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            List<int> MerID = new List<int>();
            foreach(var a in MID)
            {
                if(a != oid)
                {
                    MerID.Add(a);
                }
            }
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlcommand = "select * from `order` where id = " + oid + " and coid = " + CoID;
                var u = CoreDBconn.Query<Order>(sqlcommand).AsList();
                if(u.Count == 0)
                {
                    result.s = -1;
                    result.d = "主订单单号无效";
                    return result;
                }
                var MainOrd = u[0] as Order;
                sqlcommand = "select * from `order` where id  in @ID and coid = @Coid";
                u = CoreDBconn.Query<Order>(sqlcommand,new{ID = MerID,Coid = CoID}).AsList();
                if(u.Count == 0)
                {
                    result.s = -1;
                    result.d = "无需要合并的订单!";
                    return result;
                }
                var OrdList = u as List<Order>;
                string remark="";
                foreach(var o in OrdList)
                {
                    o.MergeOID = oid;
                    o.MergeSoID = MainOrd.SoID;
                    o.Status = 5;
                    o.AbnormalStatus = 0;
                    o.StatusDec = "";
                    o.Modifier = UserName;
                    o.ModifyDate = DateTime.Now;
                    MainOrd.OrdQty = MainOrd.OrdQty + o.OrdQty;
                    MainOrd.SkuAmount = (decimal.Parse(MainOrd.SkuAmount) + decimal.Parse(o.SkuAmount)).ToString();
                    MainOrd.PaidAmount = (decimal.Parse(MainOrd.PaidAmount) + decimal.Parse(o.PaidAmount)).ToString();
                    MainOrd.PayAmount = (decimal.Parse(MainOrd.PayAmount) + decimal.Parse(o.PayAmount)).ToString();
                    MainOrd.ExAmount = (decimal.Parse(MainOrd.ExAmount) + decimal.Parse(o.ExAmount)).ToString();
                    MainOrd.RecMessage = MainOrd.RecMessage + o.RecMessage;
                    MainOrd.SendMessage = MainOrd.SendMessage + o.SendMessage;
                    MainOrd.ExWeight = (decimal.Parse(MainOrd.ExWeight) + decimal.Parse(o.ExWeight)).ToString();
                    
                    var log = new LogInsert();
                    log.OID = o.ID;
                    log.SoID = o.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "被合并";
                    log.Remark = oid.ToString();
                    log.CoID = CoID;
                    logs.Add(log);
                    remark = remark + o.ID.ToString() + ",";

                    sqlcommand = @"update `order` set MergeOID = @MergeOID,MergeSoID = @MergeSoID,Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,
                                    Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    int r = CoreDBconn.Execute(sqlcommand,o,TransCore);
                    if (r < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                var logn = new LogInsert();
                logn.OID = MainOrd.ID;
                logn.SoID = MainOrd.SoID;
                logn.Type = 0;
                logn.LogDate = DateTime.Now;
                logn.UserName = UserName;
                logn.Title = "合并";
                logn.Remark = remark.Substring(0,remark.Length - 1);
                logn.CoID = CoID;
                logs.Add(logn);

                MainOrd.Amount = (decimal.Parse(MainOrd.ExAmount) + decimal.Parse(MainOrd.SkuAmount)).ToString();
                MainOrd.IsMerge = true;
                if(decimal.Parse(MainOrd.Amount) == decimal.Parse(MainOrd.PaidAmount))
                {
                    MainOrd.IsPaid = true;
                    MainOrd.Status = 1;
                    MainOrd.AbnormalStatus = 0;
                    MainOrd.StatusDec = "";
                    if(MainOrd.Type == 3)
                    {
                        MainOrd.Status = 8;
                    }
                }
                else
                {
                    MainOrd.IsPaid = false;
                    if(decimal.Parse(MainOrd.PaidAmount) == 0)
                    {
                        MainOrd.Status = 0;
                        MainOrd.AbnormalStatus = 0;
                        MainOrd.StatusDec = "";
                    }
                    else
                    {
                        int reasonid = GetReasonID("付款金额不等于应付金额",CoID,7).s;
                        if(reasonid == -1)
                        {
                            result.s = -1;
                            result.d = "请先设定【付款金额不等于应付金额】的异常";
                            return result;
                        }
                        MainOrd.Status = 7;
                        MainOrd.AbnormalStatus = reasonid;
                        MainOrd.StatusDec="付款金额不等于应付金额";
                        var log = new LogInsert();
                        log.OID = MainOrd.ID;
                        log.SoID = MainOrd.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "标记异常";
                        log.Remark = "付款金额不等于应付金额";
                        log.CoID = CoID;
                        logs.Add(log);
                    }
                }
                // MainOrd.AbnormalStatus = 0;
                // MainOrd.StatusDec = "";
                MainOrd.Modifier = UserName;
                MainOrd.ModifyDate = DateTime.Now;

                sqlcommand = @"update `order` set OrdQty = @OrdQty,SkuAmount = @SkuAmount,PaidAmount=@PaidAmount,PayAmount=@PayAmount,ExAmount=@ExAmount,RecMessage=@RecMessage,
                                SendMessage=@SendMessage,ExWeight=@ExWeight,Amount=@Amount,IsMerge=@IsMerge,IsPaid=@IsPaid,Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,
                                Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                int count = CoreDBconn.Execute(sqlcommand,MainOrd,TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                //订单明细
                sqlcommand = "select * from orderitem where oid  in @ID and coid = @Coid";
                var item = CoreDBconn.Query<OrderItem>(sqlcommand,new{ID = MerID,Coid = CoID}).AsList();
                if(item.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单明细异常!";
                    return result;
                }
                foreach(var i in item)
                {
                    sqlcommand = "select * from orderitem where oid  = @ID and coid = @Coid and skuautoid = @Sku";
                    var x = CoreDBconn.Query<OrderItem>(sqlcommand,new{ID = oid,Coid = CoID,Sku = i.SkuAutoID}).AsList();
                    if(x.Count == 0)
                    {
                        sqlcommand = @"INSERT INTO orderitem(oid,soid,coid,skuautoid,skuid,skuname,norm,qty,saleprice,realprice,amount,discountrate,img,weight,totalweight,isgift,remark,creator,modifier) 
                                    VALUES(@OID,@Soid,@Coid,@Skuautoid,@Skuid,@Skuname,@Norm,@Qty,@Saleprice,@Realprice,@Amount,@DiscountRate,@Img,@Weight,@Totalweight,@IsGift,@Remark,@Creator,@Modifier)";
                        i.OID = oid;
                        i.SoID = MainOrd.SoID;
                        i.Modifier = UserName;
                        count = CoreDBconn.Execute(sqlcommand,i, TransCore);
                        if (count < 0)
                        {
                            result.s = -3002;
                            return result;
                        }
                    }
                    else
                    {
                        sqlcommand = @"update orderitem set qty = qty + @Qty,amount = amount + @Amount,realprice = amount/qty,totalweight = weight * qty,
                                        modifier = @Modifier,modifydate = @ModifyDate where id = @ID and coid = @Coid";
                        count = CoreDBconn.Execute(sqlcommand,new{Qty = i.Qty,Amount = i.Amount,Modifier=UserName,ModifyDate=DateTime.Now,ID=x[0].ID,Coid = CoID}, TransCore);
                        if (count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                }
                //付款明细
                sqlcommand = "select * from payinfo where oid in @ID and coid = @Coid";
                var pay = CoreDBconn.Query<PayInfo>(sqlcommand,new{ID = MerID,Coid = CoID}).AsList();
                foreach(var p in pay)
                {
                    sqlcommand = @"update payinfo set oid = @Oid,soid = @Soid where oid in @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlcommand,new{Oid = MainOrd.ID,Soid = MainOrd.SoID,ID=MerID,Coid = CoID}, TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }

            return result;
        }
        ///<summary>
        ///订单合并还原
        ///</summary>
        public static DataResult CancleOrdMerge(List<int> oid,string Type,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var MainOrd = new List<Order>();
            if (Type.ToUpper() == "B")
            {
                string sqlcommand = "select * from `order` where IsMerge = true and status in (0,1,7) and coid = " + CoID + " and IsSplit = false"; 
                using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                    try{    
                        var u = conn.Query<Order>(sqlcommand).AsList();
                        if(u.Count == 0)
                        {
                            result.s = -1;
                            result.d = "无符合条件的资料!";
                            return result;
                        }
                        else
                        {
                            MainOrd = u;
                        }         
                    }catch(Exception ex){
                        result.s = -1;
                        result.d = ex.Message;
                        conn.Dispose();
                    }
                }    
            }
            else
            {
                string sqlcommand = "select * from `order` where id in @ID and coid = @Coid"; 
                using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                    try{    
                        var u = conn.Query<Order>(sqlcommand,new{id = oid,coid = CoID}).AsList();
                        if(u.Count == 0)
                        {
                            result.s = -1;
                            result.d = "参数异常!";
                            return result;
                        }
                        else
                        {
                            foreach(var a in u)
                            {
                                if(a.IsMerge == true && a.IsSplit == false && (a.Status == 0 ||a.Status == 1 ||a.Status == 7))
                                {
                                    MainOrd.Add(a);
                                }
                            }
                        }         
                    }catch(Exception ex){
                        result.s = -1;
                        result.d = ex.Message;
                        conn.Dispose();
                    }
                }    
            }
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                foreach(var a in MainOrd)
                {
                    List<int> merid = new List<int>();
                    string sqlcommand = "select * from `order` where MergeOID = " + a.ID + " and coid = " + CoID; 
                    var u = CoreDBconn.Query<Order>(sqlcommand).AsList();
                    foreach(var b in u)
                    {
                        merid.Add(b.ID);
                        b.MergeOID = 0;
                        b.MergeSoID = 0;
                        if(b.IsPaid == true)
                        {
                            b.Status = 1;
                            if(b.Type == 3)
                            {
                                b.Status = 8;
                            }
                        }
                        else
                        {
                            b.Status = 0;
                        }
                        b.Modifier = UserName;
                        b.ModifyDate = DateTime.Now;
                        a.OrdQty = a.OrdQty - b.OrdQty;
                        a.SkuAmount = (decimal.Parse(a.SkuAmount) - decimal.Parse(b.SkuAmount)).ToString();
                        a.PaidAmount = (decimal.Parse(a.PaidAmount) - decimal.Parse(b.PaidAmount)).ToString();
                        a.PayAmount = (decimal.Parse(a.PayAmount) - decimal.Parse(b.PayAmount)).ToString();
                        a.ExAmount = (decimal.Parse(a.ExAmount) - decimal.Parse(b.ExAmount)).ToString();
                        a.ExWeight = (decimal.Parse(a.ExWeight) - decimal.Parse(b.ExWeight)).ToString();
                        
                        var log = new LogInsert();
                        log.OID = b.ID;
                        log.SoID = b.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "合并还原";
                        log.CoID = CoID;
                        logs.Add(log);

                        sqlcommand = @"update `order` set MergeOID = @MergeOID,MergeSoID = @MergeSoID,Status=@Status,
                                        Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                        int r = CoreDBconn.Execute(sqlcommand,b,TransCore);
                        if (r < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        if(decimal.Parse(b.PaidAmount) >0)
                        {
                            //付款明细
                            sqlcommand = @"update payinfo set oid = @Oid,soid = @Soid where oid = @ID and coid = @Coid and PayNbr =@PayNbr";
                            r = CoreDBconn.Execute(sqlcommand,new{Oid = b.ID,Soid = b.SoID,ID=a.ID,Coid = CoID,PayNbr = b.PayNbr}, TransCore);
                            if (r < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                    a.Amount = (decimal.Parse(a.ExAmount) + decimal.Parse(a.SkuAmount)).ToString();
                    a.IsMerge = false;
                    if(decimal.Parse(a.Amount) == decimal.Parse(a.PaidAmount))
                    {
                        a.IsPaid = true;
                        a.Status = 1;
                        if(a.Type == 3)
                        {
                            a.Status = 8;
                        }
                    }
                    else
                    {
                        a.IsPaid = false;
                        a.Status = 0;
                    }
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;

                    var logn = new LogInsert();
                    logn.OID = a.ID;
                    logn.SoID = a.SoID;
                    logn.Type = 0;
                    logn.LogDate = DateTime.Now;
                    logn.UserName = UserName;
                    logn.Title = "合并还原";
                    logn.CoID = CoID;
                    logs.Add(logn);

                    sqlcommand = @"update `order` set OrdQty = @OrdQty,SkuAmount = @SkuAmount,PaidAmount=@PaidAmount,PayAmount=@PayAmount,ExAmount=@ExAmount,
                                    ExWeight=@ExWeight,Amount=@Amount,IsMerge=@IsMerge,IsPaid=@IsPaid,Status=@Status,
                                    Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    int count = CoreDBconn.Execute(sqlcommand,a,TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    //订单明细
                    sqlcommand = "select * from orderitem where oid  in @ID and coid = @Coid";
                    var item = CoreDBconn.Query<OrderItem>(sqlcommand,new{ID = merid,Coid = CoID}).AsList();
                    if(item.Count == 0)
                    {
                        result.s = -1;
                        result.d = "订单明细异常!";
                        return result;
                    }
                    foreach(var i in item)
                    {
                        sqlcommand = "select * from orderitem where oid  = @ID and coid = @Coid and skuautoid = @Sku";
                        var x = CoreDBconn.Query<OrderItem>(sqlcommand,new{ID = a.ID,Coid = CoID,Sku = i.SkuAutoID}).AsList();
                        if(x[0].Qty == i.Qty)
                        {
                            sqlcommand = @"delete from orderitem where oid  = @ID and coid = @Coid and skuautoid = @Sku";
                            count = CoreDBconn.Execute(sqlcommand,new{ID = a.ID,Coid = CoID,Sku = i.SkuAutoID}, TransCore);
                            if (count < 0)
                            {
                                result.s = -3004;
                                return result;
                            }
                        }
                        else
                        {
                            sqlcommand = @"update orderitem set qty = qty - @Qty,amount = amount - @Amount,realprice = amount/qty,totalweight = weight * qty,
                                            modifier = @Modifier,modifydate = @ModifyDate where id = @ID and coid = @Coid";
                            count = CoreDBconn.Execute(sqlcommand,new{Qty = i.Qty,Amount = i.Amount,Modifier=UserName,ModifyDate=DateTime.Now,ID=x[0].ID,Coid = CoID}, TransCore);
                            if (count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int ii = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (ii < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///订单拆分
        ///</summary>
        public static DataResult OrdSplit(int oid,List<SplitOrd> SplitArray,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            int i = 0,j = 0,qtyNew = 0;
            decimal amt = 0,amtNew = 0,weight = 0,weightNew = 0;
            foreach(var a in SplitArray)
            {
                if(a.Qty == a.QtyNew)
                {
                    i ++;
                }
                if(a.QtyNew == 0)
                {
                    j ++;
                }
                if(a.QtyNew > a.Qty)
                {
                    result.s = -1;
                    result.d = "拆分数量不能大于原订单数量!";
                    return result;
                }
                qtyNew = qtyNew + a.QtyNew;
                amt = amt + (a.Qty - a.QtyNew) * a.Price;
                amtNew = amtNew + a.QtyNew * a.Price;
                weight = weight + (a.Qty - a.QtyNew) * a.Weight;
                weightNew = weightNew + a.QtyNew * a.Weight;
            }
            if(i == SplitArray.Count)
            {
                result.s = -1;
                result.d = "订单数量不能全部拆分!";
                return result;
            }
            if(j == SplitArray.Count)
            {
                result.s = -1;
                result.d = "订单拆分数量不能为零!";
                return result;
            }
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlcommand = "select * from `order` where id = " + oid + " and coid = " + CoID;
                var u = CoreDBconn.Query<Order>(sqlcommand).AsList();
                var ord = u[0] as Order;
                var ordNew = u[0] as Order;
                long soid = u[0].SoID;
                if(u.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号参数无效!";
                    return result;
                }
                else
                {
                    if(u[0].Status != 1 && u[0].Status != 2)
                    {
                        result.s = -1;
                        result.d = "只有已付款待审核/已审核待配快递的订单才可以拆分!";
                        return result;
                    }
                    if(u[0].DealerType == 2)
                    {
                        result.s = -1;
                        result.d = "供销订单不允许拆分,请联系分销商拆分订单!";
                        return result;
                    }
                }
                decimal PaidAmount = 0,PayAmount = 0,ExAmount = 0;
                PaidAmount = decimal.Parse(ord.PaidAmount);
                PayAmount = decimal.Parse(ord.PayAmount);
                ExAmount = decimal.Parse(ord.ExAmount);
                //更新原订单
                ord.IsSplit = true;
                ord.OrdQty = ord.OrdQty - qtyNew;
                ord.SkuAmount = amt.ToString();
                ord.PaidAmount =  Math.Round(amt/(amt + amtNew) * PaidAmount,2).ToString();
                ord.PayAmount =  Math.Round(amt/(amt + amtNew) * PayAmount,2).ToString();
                ord.ExAmount =  Math.Round(amt/(amt + amtNew) * ExAmount,2).ToString();
                ord.Amount = (amt + decimal.Parse(ord.ExAmount)).ToString();
                ord.ExWeight = weight.ToString();
                ord.Modifier = UserName;
                ord.ModifyDate = DateTime.Now;
                sqlcommand = @"update `order` set IsSplit = @IsSplit,OrdQty=@OrdQty,SkuAmount=@SkuAmount,PaidAmount=@PaidAmount,PayAmount=@PayAmount,ExAmount=@ExAmount,
                                Amount = @Amount,ExWeight = @ExWeight,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                int count = CoreDBconn.Execute(sqlcommand,ord,TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                //新增订单
                ordNew.MergeOID = ord.ID;
                ordNew.SoID = long.Parse(DateTime.Now.Ticks.ToString().Substring(0, 11));
                ordNew.MergeSoID = soid;
                ordNew.OrdQty = qtyNew;
                ordNew.SkuAmount = amtNew.ToString();
                ordNew.PaidAmount =  Math.Round(amtNew/(amt + amtNew) * PaidAmount,2).ToString();
                ordNew.PayAmount =  Math.Round(amtNew/(amt + amtNew) * PayAmount,2).ToString();
                ordNew.ExAmount =  Math.Round(amtNew/(amt + amtNew) * ExAmount,2).ToString();
                ordNew.Amount = (amtNew + decimal.Parse(ordNew.ExAmount)).ToString();
                ordNew.ExWeight = weightNew.ToString();
                ordNew.Creator = UserName;
                ordNew.Modifier = UserName;
                sqlcommand = @"INSERT INTO `order`(MergeOID,Type,DealerType,IsMerge,IsSplit,OSource,ODate,CoID,BuyerID,BuyerShopID,ShopID,ShopName,ShopSit,SoID,MergeSoID,
                                                    OrdQty,Amount,SkuAmount,PaidAmount,PayAmount,ExAmount,IsInvoice,InvoiceType,InvoiceTitle,InvoiceDate,IsPaid,PayDate,
                                                    PayNbr,IsCOD,Status,AbnormalStatus,StatusDec,ShopStatus,RecName,RecLogistics,RecCity,RecDistrict,RecAddress,RecZip,
                                                    RecTel,RecPhone,RecMessage,SendMessage,Express,ExID,ExWeight,Creator,Modifier) 
                                VALUES(@MergeOID,@Type,@DealerType,@IsMerge,@IsSplit,@OSource,@ODate,@CoID,@BuyerID,@BuyerShopID,@ShopID,@ShopName,@ShopSit,@SoID,@MergeSoID,
                                       @OrdQty,@Amount,@SkuAmount,@PaidAmount,@PayAmount,@ExAmount,@IsInvoice,@InvoiceType,@InvoiceTitle,@InvoiceDate,@IsPaid,@PayDate,
                                       @PayNbr,@IsCOD,@Status,@AbnormalStatus,@StatusDec,@ShopStatus,@RecName,@RecLogistics,@RecCity,@RecDistrict,@RecAddress,@RecZip,
                                       @RecTel,@RecPhone,@RecMessage,@SendMessage,@Express,@ExID,@ExWeight,@Creator,@Modifier)";
                count = CoreDBconn.Execute(sqlcommand,ordNew,TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                int rtn = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                //订单明细处理
                foreach(var a in SplitArray)
                {
                    if(a.QtyNew == 0)//未拆分明细，不需处理
                    {
                        continue;
                    }
                    if(a.QtyNew == a.Qty)//数量全部拆分到新订单
                    {
                        sqlcommand = @"update orderitem set OID = @OID,SoID=@SoID,Modifier=@Modifier,ModifyDate=@ModifyDate where oid = @ID and CoID = @CoID and skuautoid = @Sku";
                        count = CoreDBconn.Execute(sqlcommand,new{OID = rtn,SoID = ordNew.SoID,Modifier=UserName,ModifyDate = DateTime.Now,ID = oid,CoID = CoID,Sku = a.Skuid},TransCore);
                        if (count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                    if(a.QtyNew > 0 && a.QtyNew != a.Qty)//拆分订单
                    {
                        //更新原订单数量
                        sqlcommand = @"update orderitem set Qty = @Qty,Amount=RealPrice*Qty,TotalWeight = Weight*Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where oid = @ID and CoID = @CoID and skuautoid = @Sku";
                        count = CoreDBconn.Execute(sqlcommand,new{Qty = a.Qty - a.QtyNew,Modifier=UserName,ModifyDate = DateTime.Now,ID = oid,CoID = CoID,Sku = a.Skuid},TransCore);
                        if (count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        //新增订单明细
                        sqlcommand = "select * from  orderitem  where oid = " + oid + " and CoID =" + CoID + " and skuautoid =" + a.Skuid;
                        var item = CoreDBconn.Query<OrderItem>(sqlcommand).AsList();
                        item[0].OID = rtn;
                        item[0].SoID = ordNew.SoID;
                        item[0].Qty = a.QtyNew;
                        item[0].Amount = (a.QtyNew * decimal.Parse(item[0].RealPrice)).ToString();
                        item[0].TotalWeight = (a.QtyNew * decimal.Parse(item[0].Weight)).ToString();
                        item[0].Creator = UserName;
                        item[0].Modifier = UserName;
                        sqlcommand = @"INSERT INTO orderitem (OID,SoID,CoID,SkuAutoID,SkuID,SkuName,Norm,Qty,SalePrice,RealPrice,Amount,DiscountRate,img,Weight,TotalWeight,
                                                            IsGift,Remark,Creator,Modifier) 
                                        VALUES(@OID,@SoID,@CoID,@SkuAutoID,@SkuID,@SkuName,@Norm,@Qty,@SalePrice,@RealPrice,@Amount,@DiscountRate,@img,@Weight,@TotalWeight,
                                            @IsGift,@Remark,@Creator,@Modifier)";
                        count = CoreDBconn.Execute(sqlcommand,item[0],TransCore);
                        if (count < 0)
                        {
                            result.s = -3002;
                            return result;
                        }
                    }
                }
                //log写入
                var log = new LogInsert();
                log.OID = oid;
                log.SoID = soid;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "被拆分";
                log.Remark = rtn.ToString();
                log.CoID = CoID;
                logs.Add(log);
                log = new LogInsert();
                log.OID = rtn;
                log.SoID = ordNew.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "拆分";
                log.Remark = oid.ToString();
                log.CoID = CoID;
                logs.Add(log);
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                //payinfo写入
                sqlcommand = "select * from payinfo where oid = " + oid + " and coid = " + CoID;
                var pay = CoreDBconn.Query<PayInfo>(sqlcommand).AsList();
                foreach(var p in pay)
                {
                    decimal Amount = decimal.Parse(p.Amount);
                    PayAmount = decimal.Parse(p.PayAmount);
                    //原明细
                    p.Amount = Math.Round(amt/(amt + amtNew) * Amount,2).ToString();
                    p.PayAmount = Math.Round(amt/(amt + amtNew) * PayAmount,2).ToString();
                    sqlcommand = @"update payinfo set Amount = @Amount,PayAmount=@PayAmount  where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlcommand,p,TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    //新明细
                    p.CoID = CoID;
                    p.OID = rtn;
                    p.SoID = ordNew.SoID;
                    p.Amount = Math.Round(amtNew/(amt + amtNew) * Amount,2).ToString();
                    p.PayAmount = Math.Round(amtNew/(amt + amtNew) * PayAmount,2).ToString();
                    sqlcommand = @"INSERT INTO payinfo(PayNbr,RecID,RecName,OID,SOID,Payment,PayAccount,SellerAccount,Platform,PayDate,Bank,BankName,Title,Name,Amount,
                                                    PayAmount,DiscountFree,DataSource,Status,Creator,Confirmer,ConfirmDate,BuyerShopID) 
                                VALUES(@PayNbr,@RecID,@RecName,@OID,@SOID,@Payment,@PayAccount,@SellerAccount,@Platform,@PayDate,@Bank,@BankName,@Title,@Name,@Amount,
                                       @PayAmount,@DiscountFree,@DataSource,@Status,@Creator,@Confirmer,@ConfirmDate,@BuyerShopID)";
                    count = CoreDBconn.Execute(sqlcommand,p,TransCore);
                    if (count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }

            return result;
        }
        ///<summary>
        ///修改运费
        ///</summary>
        public static DataResult ModifyFreight(List<int> oid,decimal freight,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var ress = new ModifyFreightReturn();
            var su = new List<ModifyFreightSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                // string sqlcommand = "select count(id) from `order` where id in @ID and coid = @Coid";
                // int rtn = CoreDBconn.QueryFirst<int>(sqlcommand,new{ID = oid,Coid = CoID});
                // if(rtn > 0)
                // {
                //     result.s = -1;
                //     result.d = "待付款/已付款待审核/异常的订单才可以修改运费";
                //     return result;
                // }
                string sqlcommand = "select id,soid,ExAmount,SkuAmount,PaidAmount,status,AbnormalStatus,StatusDec,Type,IsPaid from `order` where id in @ID and coid = @Coid "; 
                var ord = CoreDBconn.Query<Order>(sqlcommand,new{ID = oid,Coid = CoID}).AsList();
                foreach(var a in ord)
                {
                    if(a.Status != 0 &&a.Status != 1 && a.Status != 7)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有待付款/已付款待审核/异常的订单才可以修改运费";
                        fa.Add(ff);
                        continue;
                    }
                    ischeckPaid = a.IsPaid;
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "手动修改运费";
                    log.Remark = "运费 " + a.ExAmount + "=>" + freight.ToString();                  
                    log.CoID = CoID;
                    logs.Add(log);

                    decimal skuamt = decimal.Parse(a.SkuAmount);
                    var ss = new ModifyFreightSuccess();
                    ss.ID = a.ID;
                    ss.AbnormalStatus = a.AbnormalStatus;
                    ss.AbnormalStatusDec = a.StatusDec;
                    ss.ExAmount = freight.ToString();
                    ss.Amount = (skuamt + freight).ToString();
                    if(business.isskulock == 0)
                    {
                        item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + a.ID + " and coid = " + CoID).AsList();
                    }
                    if(skuamt + freight == decimal.Parse(a.PaidAmount))
                    {
                        a.IsPaid = true;
                        if(a.Status != 7)
                        {
                            a.Status = 1;
                            ss.Status = 1;
                            ss.StatusDec = "已付款待审核";
                        }
                        if(a.Status == 7 && a.StatusDec == "付款金额不等于应付金额")
                        {
                            a.Status = 1;
                            a.AbnormalStatus = 0;
                            a.StatusDec = "";
                            ss.Status = 1;
                            ss.StatusDec = "已付款待审核";
                            ss.AbnormalStatus = a.AbnormalStatus;
                            ss.AbnormalStatusDec = a.StatusDec;
                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "取消异常标记";
                            log.Remark = "付款金额不等于应付金额";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        if(a.Status !=7 && a.Type == 3)
                        {
                            a.Status =8;
                        }
                        if(business.isskulock == 0 && ischeckPaid == false)
                        {
                            foreach(var it in item)
                            {
                                sqlcommand = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                int co =CoreDBconn.Execute(sqlcommand,new{Qty = it.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = it.SkuAutoID,CoID = CoID},TransCore);
                                if(co < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        a.IsPaid = false;
                        if(decimal.Parse(a.PaidAmount) == 0)
                        {
                            a.Status = 0;
                            a.AbnormalStatus = 0;
                            a.StatusDec = "";
                            ss.Status = 0;
                            ss.StatusDec = "待付款";
                            ss.AbnormalStatus = a.AbnormalStatus;
                            ss.AbnormalStatusDec = a.StatusDec;
                        }
                        else
                        {
                            int reasonid = GetReasonID("付款金额不等于应付金额",CoID,7).s;
                            if(reasonid == -1)
                            {
                                result.s = -1;
                                result.d = "请先设定【付款金额不等于应付金额】的异常";
                                return result;
                            }
                            a.Status = 7;
                            a.AbnormalStatus = reasonid;
                            a.StatusDec="付款金额不等于应付金额";
                            ss.Status = 7;
                            ss.StatusDec = "异常";
                            ss.AbnormalStatus = a.AbnormalStatus;
                            ss.AbnormalStatusDec = a.StatusDec;
                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "标记异常";
                            log.Remark = "付款金额不等于应付金额";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        if(business.isskulock == 0 && ischeckPaid == true)
                        {
                            foreach(var it in item)
                            {
                                sqlcommand = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                int co =CoreDBconn.Execute(sqlcommand,new{Qty = it.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = it.SkuAutoID,CoID = CoID},TransCore);
                                if(co < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    su.Add(ss);
                    sqlcommand = @"update `order` set ExAmount = @ExAmount,Amount = SkuAmount + ExAmount,IsPaid = @IsPaid,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                  modifier = @Modifier,modifydate=@ModifyDate,StatusDec=@StatusDec where id = @ID and coid = @Coid";  
                    int i = CoreDBconn.Execute(sqlcommand,new {ExAmount=freight,IsPaid=a.IsPaid,Status=a.Status,Modifier=UserName,ModifyDate=DateTime.Now,ID = a.ID,Coid=CoID,
                                              AbnormalStatus = a.AbnormalStatus,StatusDec=a.StatusDec}, TransCore);
                    if (i < 0)
                    {
                        result.s = -3003;
                        return result;
                    }            
                }                           
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }      
                ress.SuccessIDs = su;
                ress.FailIDs = fa;
                result.d = ress;                        
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///初始资料
        ///</summary>
        public static DataResult GetInitData(int CoID)                
        {
            var result = new DataResult(1,null);
            var res = new OrdInitData();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                    //订单状态设定
                    var ss = new List<OStatus>();
                    foreach (int  myCode in Enum.GetValues(typeof(OrdStatus)))
                    {
                        var s = new OStatus();
                        s.value = myCode.ToString();
                        s.label = Enum.GetName(typeof(OrdStatus), myCode);//获取名称
                        if(myCode == 0 ||myCode == 1 ||myCode == 2 || myCode == 3 ||myCode == 7)
                        {
                            int i = conn.QueryFirst<int>("select count(id) from `order` where status = " + myCode + " and coid =" + CoID);
                            s.count = i;
                        }
                        ss.Add(s);
                    }
                    res.OrdStatus = ss;
                    //订单异常状态设定
                    var re = GetAbnormalList(CoID,7);
                    if(re.s == -1)
                    {
                        result.s = -1;
                        result.d = re.d;
                        return result;
                    }
                    var ab = re.d as List<OrderAbnormal>;
                    ss = new List<OStatus>();
                    foreach(var a in ab)
                    {
                        var s = new OStatus();
                        s.value= a.value;
                        s.label = a.label;
                        int i = conn.QueryFirst<int>("select count(id) from `order` where status = 7 and coid =" + CoID + " and AbnormalStatus =" + a.value);
                        s.count = i;
                        ss.Add(s);
                    }
                    res.OrdAbnormalStatus = ss;
                    //分销商
                    string sqlcommand = "select ID,DistributorName as Name from distributor where coid =" + CoID + " and enable = true and type = 0";
                    var Distributor = conn.Query<AbnormalReason>(sqlcommand).AsList();
                    var aa = new List<Filter>();
                    foreach(var d in Distributor)
                    {
                        var a = new Filter();
                        a.value = d.ID.ToString();
                        a.label = d.Name;
                        aa.Add(a);
                    }
                    res.Distributor = aa;
                    
                    }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            //仓库资料
            List<Filter> wh = new List<Filter>();
            var w = CoreComm.WarehouseHaddle.getWarelist(CoID.ToString());
            foreach(var h in w)
            {
                var a = new Filter();
                a.value = h.id.ToString();
                a.label = h.warename;
                wh.Add(a);
            }
            res.Warehouse = wh ;
            //买家留言条件设定
            var ff = new List<Filter>();
            var f = new Filter();
            f.value = "A";
            f.label = "不过滤";
            ff.Add(f);
            f = new Filter();
            f.value = "N";
            f.label = "无留言";
            ff.Add(f);
            f = new Filter();
            f.value = "Y";
            f.label = "有留言";
            ff.Add(f);
            res.BuyerRemark = ff;
            //卖家备注
            ff = new List<Filter>();
            f = new Filter();
            f.value = "A";
            f.label = "不过滤";
            ff.Add(f);
            f = new Filter();
            f.value = "N";
            f.label = "无备注";
            ff.Add(f);
            f = new Filter();
            f.value = "Y";
            f.label = "有备注";
            ff.Add(f);
            res.SellerRemark = ff;
            //订单资料来源
            var oo = new List<Filter>();
            var o = new Filter();
            o.value = "-1";
            o.label = "--- 不限 ---";
            oo.Add(o);
            foreach (int  myCode in Enum.GetValues(typeof(OrdSource)))
            {
                o = new Filter();
                o.value = myCode.ToString();
                o.label = Enum.GetName(typeof(OrdSource), myCode);//获取名称
                oo.Add(o);
            }
            res.OSource = oo;
            //订单类型
            oo = new List<Filter>();
            o = new Filter();
            o.value = "0";
            o.label = "普通订单";
            oo.Add(o);
            o = new Filter();
            o.value = "1";
            o.label = "补发订单";
            oo.Add(o);
            o = new Filter();
            o.value = "2";
            o.label = "换货订单";
            oo.Add(o);
            o = new Filter();
            o.value = "3";
            o.label = "天猫分销";
            oo.Add(o);
            o = new Filter();
            o.value = "4";
            o.label = "天猫供销";
            oo.Add(o);
            o = new Filter();
            o.value = "5";
            o.label = "协同订单";
            oo.Add(o);
            o = new Filter();
            o.value = "6";
            o.label = "普通订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "7";
            o.label = "补发订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "8";
            o.label = "换货订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "9";
            o.label = "天猫供销,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "10";
            o.label = "协同订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "11";
            o.label = "普通订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "12";
            o.label = "补发订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "13";
            o.label = "换货订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "14";
            o.label = "天猫供销,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "15";
            o.label = "协同订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "16";
            o.label = "普通订单,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "17";
            o.label = "补发订单,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "18";
            o.label = "换货订单,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "19";
            o.label = "天猫供销,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "20";
            o.label = "协同订单,分销+,供销+";
            oo.Add(o);
            res.OType = oo;
            //贷款方式
            ff = new List<Filter>();
            f = new Filter();
            f.value = "A";
            f.label = "所有(不区分货款方式)";
            ff.Add(f);
            f = new Filter();
            f.value = "N";
            f.label = "在线支付(非货到付款)";
            ff.Add(f);
            f = new Filter();
            f.value = "Y";
            f.label = "货到付款";
            ff.Add(f);
            res.LoanType = ff;
            //是否付款
            ff = new List<Filter>();
            f = new Filter();
            f.value = "A";
            f.label = "所有(不区分是否付款)";
            ff.Add(f);
            f = new Filter();
            f.value = "N";
            f.label = "未付款";
            ff.Add(f);
            f = new Filter();
            f.value = "Y";
            f.label = "已付款";
            ff.Add(f);
            res.IsPaid = ff;
            //获取店铺List
            var shop = CoreComm.ShopHaddle.getShopEnum(CoID.ToString()) as List<shopEnum>;
            ff = new List<Filter>();
            foreach(var t in shop)
            {
                f = new Filter();
                f.value = t.value.ToString();
                f.label = t.label;
                ff.Add(f);
            }
            res.Shop = ff;  
            //快递Lsit
            var Express = CoreComm.ExpressHaddle.GetExpressSimple(CoID).d as List<ExpressSimple>;
            ff = new List<Filter>();
            foreach(var t in Express)
            {
                f = new Filter();
                f.value = t.ID;
                f.label = t.Name;
                ff.Add(f);
            }
            res.Express = ff;  
            //其他
            oo = new List<Filter>();
            o = new Filter();
            o.value = "0";
            o.label = "合并订单";
            oo.Add(o);
            o = new Filter();
            o.value = "1";
            o.label = "拆分订单";
            oo.Add(o);
            o = new Filter();
            o.value = "2";
            o.label = "非合并订单";
            oo.Add(o);
            o = new Filter();
            o.value = "3";
            o.label = "非拆分订单";
            oo.Add(o);
            o = new Filter();
            o.value = "4";
            o.label = "开发票";
            oo.Add(o);
            res.Others = oo;

            result.d = res;
            return result;
        }
        ///<summary>
        ///读取各状态订单笔数
        ///</summary>
        public static DataResult GetStatusCount(int CoID)
        {
            var result = new DataResult(1,null);
            var res = new StatusCount();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                    //订单状态设定
                    var ss = new List<OStatusCnt>();
                    foreach (int  myCode in Enum.GetValues(typeof(OrdStatus)))
                    {
                        if(myCode == 0 ||myCode == 1 ||myCode == 2 ||myCode == 3 || myCode == 7)
                        {
                            var s = new OStatusCnt();
                            s.value = myCode;
                            int i = conn.QueryFirst<int>("select count(id) from `order` where status = " + myCode + " and coid =" + CoID);
                            s.count = i;
                            ss.Add(s);
                        }
                    }
                    res.OrdStatus = ss;
                    //订单异常状态设定
                    var re = GetAbnormalList(CoID,7);
                    if(re.s == -1)
                    {
                        result.s = -1;
                        result.d = re.d;
                        return result;
                    }
                    var ab = re.d as List<OrderAbnormal>;
                    ss = new List<OStatusCnt>();
                    foreach(var a in ab)
                    {
                        var s = new OStatusCnt();
                        s.value= int.Parse(a.value);
                        int i = conn.QueryFirst<int>("select count(id) from `order` where status = 7 and coid =" + CoID + " and AbnormalStatus =" + a.value);
                        s.count = i;
                        ss.Add(s);
                    }
                    res.OrdAbnormalStatus = ss;
                    result.d = res;
                    }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }    
            return result;
        }
        ///<summary>
        ///获取type说明
        ///</summary>
        public static string GetTypeName(int type)                
        {
            string Name = string.Empty;
            if(type == 0)
            {
                Name = "普通订单";
            }
            if(type == 1)
            {
                Name = "补发订单";
            }
            if(type == 2)
            {
                Name = "换货订单";
            }
            if(type == 3)
            {
                Name = "天猫分销";
            }
            if(type == 4)
            {
                Name = "天猫供销";
            }
            if(type == 5)
            {
                Name = "协同订单";
            }
            if(type == 6)
            {
                Name = "普通订单,分销+";
            }
            if(type == 7)
            {
                Name = "补发订单,分销+";
            }
            if(type == 8)
            {
                Name = "换货订单,分销+";
            }
            if(type == 9)
            {
                Name = "天猫供销,分销+";
            }
            if(type == 10)
            {
                Name = "协同订单,分销+";
            }
            if(type == 11)
            {
                Name = "普通订单,供销+";
            }
            if(type == 12)
            {
                Name = "补发订单,供销+";
            }
            if(type == 13)
            {
                Name = "换货订单,供销+";
            }
            if(type == 14)
            {
                Name = "天猫供销,供销+";
            }
            if(type == 15)
            {
                Name = "协同订单,供销+";
            }
            if(type == 16)
            {
                Name = "普通订单,分销+,供销+";
            }
            if(type == 17)
            {
                Name = "补发订单,分销+,供销+";
            }
            if(type == 18)
            {
                Name = "换货订单,分销+,供销+";
            }
            if(type == 19)
            {
                Name = "天猫供销,分销+,供销+";
            }
            if(type == 20)
            {
                Name = "协同订单,分销+,供销+";
            }            
            return Name;
        }
        ///<summary>
        ///平台订单新增入口
        ///</summary>
        public static DataResult ImportOrderInsert(ImportOrderInsert Order,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            var gifti = new List<GiftRule>();
            var spe = GetOrderSpecial(CoID).d as OrdSpecialSingle;
            //检查平台单号是否已经存在，若是，则不能新增
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string wheresql = "select count(id) from `order` where soid =" + Order.SoID + " and coid =" + CoID;
                    int u = conn.QueryFirst<int>(wheresql);
                    if(u > 0)
                    {
                        result.s = -1;
                        result.d = "该订单已经导入!";
                        return result;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            int shopid = 0,shopsit = 0;
            var logs = new List<LogInsert>();
            List<int> idlist = new List<int>();
            using(var conn = new MySqlConnection(DbBase.CommConnectString) ){
                try{
                    string wheresql = "select * from Shop where ShopName ='" + Order.ShopName + "' and coid =" + CoID;
                    var u = conn.Query<Shop>(wheresql).AsList();
                    if(u.Count > 0)
                    {
                        shopid = u[0].ID;
                        shopsit = u[0].SitType;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                var orderitem = new List<OrderItem>();
                decimal Amount = 0,TotalWeight = 0;
                int Qty = 0;
                string sqlcommand = "";
                bool IsSku = true,isRecExsit = true,isMerge = false;
                var rec = new RecInfo();
                //处理订单明细
                foreach(var a in Order.Item)
                {
                    var item = new OrderItem();
                    item.SoID = Order.SoID;
                    item.CoID = CoID;
                    item.SkuID = a.SkuID;
                    item.Qty = a.Qty;
                    item.RealPrice = a.Price.ToString();
                    item.Amount = a.Amount.ToString();
                    item.Remark = a.Remark;
                    item.Creator = UserName;
                    item.CreateDate = DateTime.Now;
                    item.Modifier = UserName;
                    item.ModifyDate= DateTime.Now;
                    item.ShopSkuID = a.ShopSkuID;
                    Amount = Amount + a.Amount;
                    Qty = Qty + a.Qty;
                    int i = CoreDBconn.QueryFirst<int>("select count(id) from coresku where skuid = '" + a.SkuID + "' and coid = " + CoID);
                    if(i == 0)
                    {
                        IsSku = false;
                    }
                    else
                    {
                        int j = CoreDBconn.QueryFirst<int>("select id from coresku where skuid = '" + a.SkuID + "' and coid = " + CoID);
                        sqlcommand = "select skuid,skuname,norm,img,goodscode,enable,saleprice,weight from coresku where id =" + j + " and coid =" + CoID;
                        var s = CoreDBconn.Query<SkuInsert>(sqlcommand).AsList();
                        item.SkuAutoID = j;
                        item.SkuName = s[0].skuname;
                        item.Norm = s[0].norm;
                        item.GoodsCode = s[0].goodscode;
                        item.SalePrice = s[0].saleprice;
                        item.DiscountRate = Math.Round(a.Price/decimal.Parse(item.SalePrice),2).ToString();
                        item.img = s[0].img;
                        item.Weight = s[0].weight;
                        item.TotalWeight = (a.Qty * decimal.Parse(s[0].weight)).ToString();
                        item.IsGift = false;
                        TotalWeight = TotalWeight + decimal.Parse(item.TotalWeight);
                    }
                    orderitem.Add(item);
                }
                //处理付款资料
                var PayList= new List<PayInfo>();
                foreach(var a in Order.Pay)
                {
                    var pay = new PayInfo();
                    pay.PayNbr = a.PayNbr;
                    pay.RecName = Order.RecName;
                    pay.SoID = Order.SoID;
                    pay.Payment = a.Payment;
                    pay.PayAccount = a.PayAccount;
                    pay.SellerAccount = a.SellerAccount;
                    pay.Platform = a.Platform;
                    pay.PayDate = a.PayDate;
                    pay.Bank = a.Bank;
                    pay.BankName= a.BankName;
                    pay.Title = a.Title;
                    pay.Name = a.Name;
                    pay.Amount = a.Amount;
                    pay.PayAmount = a.PayAmount;
                    pay.DataSource = 0;
                    pay.Status = 1;
                    pay.CoID = CoID;
                    pay.Creator = UserName;
                    pay.CreateDate = DateTime.Now;
                    pay.Confirmer = UserName;
                    pay.ConfirmDate= DateTime.Now;
                    pay.BuyerShopID = Order.BuyerShopID;
                    PayList.Add(pay);
                }
                //产生订单资料
                var ord = new Order();
                ord.Type = Order.Type;
                ord.DealerType = 0;
                if(!string.IsNullOrEmpty(Order.Distributor))
                {
                    ord.DealerType = 1;
                }
                if(!string.IsNullOrEmpty(Order.SupDistributor))
                {
                    ord.DealerType = 2;
                }
                ord.OSource = Order.OSource;
                ord.ODate = Order.ODate;
                ord.CoID = CoID;
                ord.BuyerShopID = Order.BuyerShopID;
                ord.ShopID = shopid;
                ord.ShopName = Order.ShopName;
                ord.ShopSit = shopsit;
                ord.SoID = Order.SoID;
                ord.OrdQty = Qty;
                ord.Amount = Order.Amount;
                ord.SkuAmount = Amount.ToString();
                ord.PaidAmount = Order.PaidAmount.ToString();
                ord.PayAmount = Order.PayAmount.ToString();
                ord.ExAmount = Order.ExAmount.ToString();
                ord.IsInvoice = Order.IsInvoice;
                ord.InvoiceType = Order.InvoiceType;
                ord.InvoiceTitle = Order.InvoiceTitle;
                ord.InvoiceDate = Order.InvoiceDate;
                if(ord.Amount == ord.PaidAmount)
                {
                    ord.IsPaid = true;
                    ord.Status = 1;
                    if(ord.Type == 3)
                    {
                        ord.Status =8;
                    }
                    if(business.isskulock == 0)
                    {
                        foreach(var i in orderitem)
                        {
                            sqlcommand = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            int co =CoreDBconn.Execute(sqlcommand,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(co < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                else
                {
                    ord.IsPaid = false;
                    ord.Status = 0;
                }
                if(PayList.Count > 0)
                {
                    ord.PayDate = PayList[0].PayDate;
                    ord.PayNbr = PayList[0].PayNbr;
                }
                ord.IsCOD = Order.IsCOD;
                ord.AbnormalStatus = 0;
                ord.StatusDec = "";
                ord.ShopStatus = Order.ShopStatus;
                ord.RecName = Order.RecName;
                ord.RecLogistics = Order.RecLogistics;
                ord.RecCity = Order.RecCity;
                ord.RecDistrict = Order.RecDistrict;
                ord.RecAddress = Order.RecAddress;
                ord.RecZip = Order.RecZip;
                ord.RecTel = Order.RecTel;
                ord.RecPhone = Order.RecPhone;
                ord.RecMessage = Order.RecMessage;
                ord.ExWeight = TotalWeight.ToString();
                ord.Distributor = Order.Distributor;
                ord.SupDistributor = Order.SupDistributor;
                ord.Creator = UserName;
                ord.CreateDate = DateTime.Now;
                ord.Modifier = UserName;
                ord.ModifyDate = DateTime.Now;
                //增加赠品
                var gi = GiftHaddle.SetGift(ord,orderitem,CoID,UserName);
                if(gi.s == 1)
                {
                    var gire = gi.d as GiftInsertOrderReturn;
                    // ord.OrdQty = ord.OrdQty + gire.Qty;
                    ord.ExWeight = (decimal.Parse(ord.ExWeight) + gire.Exweight).ToString();
                    orderitem = gire.Item;
                    gifti = gire.Gift;
                }
                //标记异常
                if(IsSku == false)
                {
                    int reasonid = GetReasonID("商品编码缺失",CoID,7).s;
                    if(reasonid == -1)
                    {
                        result.s = -1;
                        result.d = "请先设定【商品编码缺失】的异常";
                        return result;
                    }
                    ord.Status = 7;
                    ord.AbnormalStatus = reasonid;
                    ord.StatusDec = "商品编码缺失";
                }
                //检查是否标记特殊单
                if(ord.Status != 7 && (!string.IsNullOrEmpty(spe.Shop) || !string.IsNullOrEmpty(spe.RecMessage) || !string.IsNullOrEmpty(spe.SendMessage) || !string.IsNullOrEmpty(spe.RecAddress)))
                {
                    bool speFlag = true;
                    if(!string.IsNullOrEmpty(spe.Shop))
                    {
                        string[] sp = spe.Shop.Split(',');
                        int i = 0;
                        foreach(var pp in sp)
                        {
                            if (ord.ShopID.ToString() == pp)
                            {
                                i ++;
                                break;
                            }
                        }
                        if(i ==  0) 
                        { 
                            speFlag = false;
                        }
                    }
                    if(!string.IsNullOrEmpty(spe.RecMessage))
                    {
                        string[] sp = spe.RecMessage.Split(',');
                        int i = 0;
                        foreach(var pp in sp)
                        {
                            if (ord.RecMessage.Contains(pp))
                            {
                                i ++;
                                break;
                            }
                        }
                        if(i ==  0) 
                        { 
                            speFlag = false;
                        }
                    }
                    if(!string.IsNullOrEmpty(spe.SendMessage))
                    {
                        string[] sp = spe.SendMessage.Split(',');
                        int i = 0;
                        foreach(var pp in sp)
                        {
                            if (ord.SendMessage.Contains(pp))
                            {
                                i ++;
                                break;
                            }
                        }
                        if(i ==  0) 
                        { 
                            speFlag = false;
                        }
                    }
                    if(!string.IsNullOrEmpty(spe.RecAddress))
                    {
                        string[] sp = spe.RecAddress.Split(',');
                        int i = 0;
                        foreach(var pp in sp)
                        {
                            if (ord.RecAddress.Contains(pp))
                            {
                                i ++;
                                break;
                            }
                        }
                        if(i ==  0) 
                        { 
                            speFlag = false;
                        }
                    }
                    if( speFlag == true)
                    {
                        int reasonid = GetReasonID("特殊单",CoID,7).s;
                        if(reasonid == -1)
                        {
                            result.s = -1;
                            result.d = "请先设定【特殊单】的异常";
                            return result;
                        }
                        ord.Status = 7;
                        ord.AbnormalStatus = reasonid;
                        ord.StatusDec = "特殊单";
                    }
                }
                if(ord.Status != 7 && business.ismergeorder == true)
                {
                    //检查订单是否符合合并的条件
                    var res = isCheckMerge(ord);
                    int reasonid = 0;
                    if(res.s == 1)
                    {
                        isMerge = true;
                        idlist = res.d as List<int>;
                        reasonid = GetReasonID("等待订单合并",CoID,7).s;
                        if(reasonid == -1)
                        {
                            result.s = -1;
                            result.d = "请先设定【等待订单合并】的异常";
                            return result;
                        }
                        ord.Status = 7;
                        ord.AbnormalStatus = reasonid;
                        ord.StatusDec = "等待订单合并";
                    }
                }
                //检查收货人是否存在
                sqlcommand = "select count(id) from recinfo where coid = " + CoID + " and buyerid = '" + Order.BuyerShopID + "' and receiver = '" + Order.RecName + 
                                "' and address = '" + Order.RecAddress + "' and logistics = '" + Order.RecLogistics + "' and city = '" + Order.RecCity + 
                                "' and district = '" + Order.RecDistrict + "'";
                int u = CoreDBconn.QueryFirst<int>(sqlcommand);
                if(u > 0)
                {
                    sqlcommand = "select id from recinfo where coid = " + CoID + " and buyerid = '" + Order.BuyerShopID + "' and receiver = '" + Order.RecName + 
                                "' and address = '" + Order.RecAddress + "' and logistics = '" + Order.RecLogistics + "' and city = '" + Order.RecCity + 
                                "' and district = '" + Order.RecDistrict + "'";
                    u = CoreDBconn.QueryFirst<int>(sqlcommand);
                    ord.BuyerID = u;
                    foreach(var p in PayList)
                    {
                        p.RecID = u;
                    }
                }
                else
                {
                    isRecExsit = false;
                    rec.BuyerId = ord.BuyerShopID;
                    rec.Receiver = ord.RecName;
                    rec.Tel = ord.RecTel;
                    rec.Phone = ord.RecPhone;
                    rec.Logistics = ord.RecLogistics;
                    rec.City = ord.RecCity;
                    rec.District = ord.RecDistrict;
                    rec.Address = ord.RecAddress;
                    rec.ZipCode = ord.RecZip;
                    rec.Express = ord.Express;
                    rec.ExID = ord.ExID;
                    rec.CoID = CoID;
                    rec.Creator = UserName;
                    rec.ShopSit = ord.ShopSit;
                }
                //更新收货人信息
                int count =0;
                if(isRecExsit == false)
                {
                    sqlcommand = @"INSERT INTO recinfo(BuyerId,Receiver,Tel,Phone,Logistics,City,District,Address,ZipCode,Express,ExID,CoID,Creator,ShopSit) VALUES(
                            @BuyerId,@Receiver,@Tel,@Phone,@Logistics,@City,@District,@Address,@ZipCode,@Express,@ExID,@CoID,@Creator,@ShopSit)";
                    count =CoreDBconn.Execute(sqlcommand,rec,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    else
                    {
                        int rtn = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                        ord.BuyerID = rtn;
                        foreach(var p in PayList)
                        {
                            p.RecID = rtn;
                        }
                        rec.ID = rtn;
                    }
                }
                //新增订单资料
                sqlcommand = @"INSERT INTO `order`(Type,DealerType,OSource,ODate,CoID,BuyerID,BuyerShopID,ShopID,ShopName,ShopSit,SoID,OrdQty,Amount,SkuAmount,PaidAmount,PayAmount,
                                                   ExAmount,IsInvoice,InvoiceType,InvoiceTitle,InvoiceDate,IsPaid,Status,PayDate,PayNbr,IsCOD,AbnormalStatus,StatusDec,ShopStatus,
                                                   RecName,RecLogistics,RecCity,RecDistrict,RecAddress,RecZip,RecTel,RecPhone,RecMessage,ExWeight,Distributor,SupDistributor,Creator,Modifier) 
                                            VALUES(@Type,@DealerType,@OSource,@ODate,@CoID,@BuyerID,@BuyerShopID,@ShopID,@ShopName,@ShopSit,@SoID,@OrdQty,@Amount,@SkuAmount,@PaidAmount,@PayAmount,
                                                   @ExAmount,@IsInvoice,@InvoiceType,@InvoiceTitle,@InvoiceDate,@IsPaid,@Status,@PayDate,@PayNbr,@IsCOD,@AbnormalStatus,@StatusDec,@ShopStatus,
                                                   @RecName,@RecLogistics,@RecCity,@RecDistrict,@RecAddress,@RecZip,@RecTel,@RecPhone,@RecMessage,@ExWeight,@Distributor,@SupDistributor,@Creator,@Modifier)";
                count =CoreDBconn.Execute(sqlcommand,ord,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                else
                {
                    int rtn = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                    rec.OID=rtn;
                    ord.ID = rtn;
                    foreach(var i in orderitem)
                    {
                        i.OID = rtn;
                    }
                    foreach(var p in PayList)
                    {
                        p.OID = rtn;
                    }
                }
                //新增日志
                var log = new LogInsert();
                log.OID = ord.ID;
                log.SoID = ord.SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "接单时间";
                log.Remark = "接口下载订单时间";
                log.CoID = CoID;
                logs.Add(log);
                if(ord.Status == 7)
                {
                    log = new LogInsert();
                    log.OID = ord.ID;
                    log.SoID = ord.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "标记异常";
                    log.Remark = ord.StatusDec;
                    log.CoID = CoID;
                    logs.Add(log);
                }
                //新增订单明细
                sqlcommand = @"INSERT INTO orderitem(oid,soid,coid,skuautoid,skuid,skuname,norm,GoodsCode,qty,saleprice,realprice,amount,DiscountRate,img,
                                                     weight,totalweight,IsGift,Remark,creator,modifier,ShopSkuID) 
                                              VALUES(@OID,@Soid,@Coid,@Skuautoid,@Skuid,@Skuname,@Norm,@GoodsCode,@Qty,@Saleprice,@Realprice,@Amount,@DiscountRate,@Img,
                                                     @Weight,@Totalweight,@IsGift,@Remark,@Creator,@Creator,@ShopSkuID)";
                count = CoreDBconn.Execute(sqlcommand, orderitem, TransCore);
                if (count <= 0)
                {
                    result.s = -3002;
                    return result;
                }
                //新增付款资料
                sqlcommand = @"INSERT INTO payinfo(PayNbr,RecID,RecName,OID,SOID,Payment,PayAccount,SellerAccount,Platform,PayDate,Bank,BankName,Title,Name,Amount,
                                                   PayAmount,DataSource,Status,CoID,Creator,CreateDate,Confirmer,ConfirmDate,BuyerShopID) 
                                           VALUES(@PayNbr,@RecID,@RecName,@OID,@SOID,@Payment,@PayAccount,@SellerAccount,@Platform,@PayDate,@Bank,@BankName,@Title,@Name,@Amount,
                                                  @PayAmount,@DataSource,@Status,@CoID,@Creator,@CreateDate,@Confirmer,@ConfirmDate,@BuyerShopID)";
                count = CoreDBconn.Execute(sqlcommand,PayList,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                //待合并资料更新
                if(isMerge == true)
                {
                    string querySql = "select id,soid from `order` where id in @ID and coid = @Coid and status <> 7";
                    var v = CoreDBconn.Query<Order>(querySql,new{ID = idlist,Coid = CoID}).AsList();
                    if(v.Count > 0)
                    {
                        sqlcommand = @"update `order` set status = 7,abnormalstatus = @Abnormalstatus,statusdec = '等待订单合并' where id in @ID and coid = @Coid and status <> 7";
                        count =CoreDBconn.Execute(sqlcommand,new {Abnormalstatus = ord.AbnormalStatus,ID = idlist,Coid = CoID},TransCore);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        foreach(var c in v)
                        {
                            log = new LogInsert();
                            log.OID = c.ID;
                            log.SoID = c.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "标记异常";
                            log.Remark = "等待订单合并";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                    }
                }
                //更新日志
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                   VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count =CoreDBconn.Execute(loginsert,logs,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                //更新收货人资料
                if(isRecExsit == false)
                {
                    sqlcommand = @"update recinfo set OID = @OID where id = @ID and coid = @Coid";
                    count =CoreDBconn.Execute(sqlcommand,rec,TransCore);
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                result.d = ord.ID;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            if(gifti.Count > 0)
            {
                var CommDBconn = new MySqlConnection(DbBase.CommConnectString);
                CommDBconn.Open();
                var TransComm = CommDBconn.BeginTransaction();
                try
                {
                    foreach(var a in gifti)
                    {
                        string sql = "update gift set GivenQty=@GivenQty where id=@ID and coid=@Coid";
                        int count = CommDBconn.Execute(sql,a,TransComm);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                    TransComm.Commit();
                }
                catch (Exception e)
                {
                    TransComm.Rollback();
                    TransComm.Dispose();
                    result.s = -1;
                    result.d = e.Message;
                }
                finally
                {
                    TransComm.Dispose();
                    CommDBconn.Dispose();
                }
            }
            return result;
        }
        ///<summary>
        ///平台订单更新入口
        ///</summary>
        public static DataResult ImportOrderUpdate(ImportOrderUpdate Order,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            string sqlcommand = "select id,soid,coid,recname,amount,PaidAmount,PayAmount,status,IsPaid,PayDate,PayNbr,ShopStatus,Type from `order` where soid = " + Order.SoID + " and coid = " + CoID;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                var ord = CoreDBconn.Query<Order>(sqlcommand).AsList();
                ischeckPaid = ord[0].IsPaid;
                //处理付款资料
                var PayList= new List<PayInfo>();
                decimal amount = 0;
                bool isUpdate = false;
                foreach(var a in Order.Pay)
                {
                    sqlcommand = "select count(id) from payinfo where oid = " + ord[0].ID + " and coid = " + CoID + " and PayNbr = '" + a.PayNbr + "'";
                    int i = CoreDBconn.QueryFirst<int>(sqlcommand);
                    if (i > 0) continue;
                    var pay = new PayInfo();
                    pay.PayNbr = a.PayNbr;
                    pay.RecName = ord[0].RecName;
                    pay.OID = ord[0].ID;
                    pay.SoID = ord[0].SoID;
                    pay.Payment = a.Payment;
                    pay.PayAccount = a.PayAccount;
                    pay.SellerAccount = a.SellerAccount;
                    pay.Platform = a.Platform;
                    pay.PayDate = a.PayDate;
                    pay.Bank = a.Bank;
                    pay.BankName= a.BankName;
                    pay.Title = a.Title;
                    pay.Name = a.Name;
                    pay.Amount = a.Amount;
                    pay.PayAmount = a.PayAmount;
                    pay.DataSource = 0;
                    pay.Status = 1;
                    pay.CoID = CoID;
                    pay.Creator = UserName;
                    pay.CreateDate = DateTime.Now;
                    pay.Confirmer = UserName;
                    pay.ConfirmDate= DateTime.Now;
                    pay.BuyerShopID = ord[0].BuyerShopID;
                    PayList.Add(pay);
                    amount = amount + decimal.Parse(pay.PayAmount);
                }
                if(amount > 0 && (ord[0].Status ==1 || ord[0].Status==0 ||ord[0].Status == 7))
                {
                    ord[0].PaidAmount = (decimal.Parse(ord[0].PaidAmount) + amount).ToString();
                    ord[0].PayAmount = (decimal.Parse(ord[0].PayAmount) + amount).ToString();
                    if(ord[0].PaidAmount == ord[0].Amount)
                    {
                        ord[0].IsPaid = true;
                        if(ord[0].Status != 7)
                        {
                            ord[0].Status = 1;
                        }
                        if(ord[0].Status !=7 && ord[0].Type == 3)
                        {
                            ord[0].Status =8;
                        }
                    }
                    else
                    {
                        ord[0].IsPaid = false;
                        if(ord[0].Status != 7)
                        {
                            ord[0].Status = 0;
                        }
                    }
                    if(string.IsNullOrEmpty(ord[0].PayNbr))
                    {
                        ord[0].PayNbr = PayList[0].PayNbr;
                        ord[0].PayDate = PayList[0].PayDate;
                    }
                    isUpdate = true;
                }
                if(Order.ShopStatus != ord[0].ShopStatus)
                {
                    ord[0].ShopStatus = Order.ShopStatus;
                    isUpdate = true;
                }
                if(isUpdate == true)
                {
                    ord[0].Modifier = UserName;
                    ord[0].ModifyDate = DateTime.Now;
                    sqlcommand = @"update `order` set PaidAmount = @PaidAmount,PayAmount =@PayAmount,IsPaid=@IsPaid,Status=@Status,ShopStatus=@ShopStatus,PayNbr=@PayNbr,
                                   PayDate=@PayDate,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    int count = CoreDBconn.Execute(sqlcommand,ord[0],TransCore);
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    if(business.isskulock == 0 && ischeckPaid == false && ord[0].IsPaid == true)
                    {
                        item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + ord[0].ID + " and coid = " + CoID).AsList();
                        foreach(var i in item)
                        {
                            sqlcommand = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlcommand,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///快捷修改明细资料刷新
        ///</summary>
        public static DataResult RefreshOrderItemQuick(int id,int CoID)
        {
            var result = new DataResult(1,null);
            var sin = new SingleOrderItem();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                        string wheresql = "select status,amount,ExWeight from `order` where id =" + id + " and coid =" + CoID;
                        var u = conn.Query<Order>(wheresql).AsList();
                        sin.Amount = u[0].Amount;
                        sin.Status = u[0].Status;
                        sin.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                        sin.Weight = u[0].ExWeight;
                    }
                    catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            var ff = GetSingleOrdItem(id,CoID);
            if(ff.s == -1)
            {
                result.s = -1;
                result.d = ff.d;
                return result;
            }
            sin.SkuList = ff.d as List<SkuList>;
            result.d = sin;
            return result;
        }
        ///<summary>
        ///修改明细资料刷新
        ///</summary>
        public static DataResult RefreshOrderItem(int id,int CoID)
        {
            var result = new DataResult(1,null);
            var res = new RefreshItem();
            var aa = GetOrderEdit(id,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Order = aa.d as OrderEdit;

            aa = GetOrderItem(id,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.OrderItem = aa.d as List<OrderItemEdit>;

            aa = GetOrderLog(id,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Log = aa.d as List<OrderLog>;

            result.d = res;
            return result;
        }
        ///<summary>
        ///新增赠品
        ///</summary>
        public static DataResult InsertGift(int id,List<int> skuid,int CoID,string Username)
        {
            var result = new DataResult(1,null);  
            var res = new OrderDetailInsert();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            var logs = new List<LogInsert>();
            string sqlCommandText = string.Empty;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select status,soid,amount,PaidAmount,IsPaid from `order` where id =" + id + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    if (u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以添加赠品!";
                        return result;
                    }
                }
                List<InsertFailReason> rt = new List<InsertFailReason>();
                List<int> rr = new List<int>();
                decimal weight = 0;
                foreach (int a in skuid)
                {
                    InsertFailReason rf = new InsertFailReason();
                    string skusql = "select skuid,skuname,norm,img,goodscode,enable,saleprice,weight from coresku where id =" + a + " and coid =" + CoID;
                    var s = CoreDBconn.Query<SkuInsert>(skusql).AsList();
                    if (s.Count == 0)
                    {
                        rf.id = a;
                        rf.reason = "此商品不存在!";
                        rt.Add(rf);
                        continue;
                    }
                    if (s[0].enable == false)
                    {
                        rf.id = a;
                        rf.reason = "此商品已停用!";
                        rt.Add(rf);
                        continue;
                    }
                    weight = weight + decimal.Parse(s[0].weight);
                    int x = CoreDBconn.QueryFirst<int>("select count(id) from orderitem where oid = " + id + " and coid =" + CoID + " and skuautoid = " + a + " AND IsGift = true");
                    if(x == 0)
                    {
                        sqlCommandText = @"INSERT INTO orderitem(oid,soid,coid,skuautoid,skuid,skuname,norm,GoodsCode,qty,saleprice,img,weight,totalweight,IsGift,creator,modifier) 
                                        VALUES(@OID,@Soid,@Coid,@Skuautoid,@Skuid,@Skuname,@Norm,@GoodsCode,@Qty,@Saleprice,@Img,@Weight,@Weight,@IsGift,@Creator,@Creator)";
                        var args = new
                        {
                            OID = id,
                            Soid = u[0].SoID,
                            Skuautoid = a,
                            Skuid = s[0].skuid,
                            Skuname = s[0].skuname,
                            Norm = s[0].norm,
                            GoodsCode = s[0].goodscode,
                            Qty = 1,
                            Saleprice = s[0].saleprice,
                            Img = s[0].img,
                            Weight = s[0].weight,
                            Coid = CoID,
                            Creator = Username,
                            IsGift = true
                        };
                        int count = CoreDBconn.Execute(sqlCommandText, args, TransCore);
                        if (count <= 0)
                        {
                            rf.id = a;
                            rf.reason = "新增明细失败!";
                            rt.Add(rf);
                            res.failIDs = rt;
                            return result;
                        }
                    }
                    else
                    {
                        sqlCommandText = @"update orderitem set totalweight = weight * qty,modifier=@Modifier,modifydate = @ModifyDate 
                                        where oid = @ID and coid = @Coid and skuautoid = @Skuautoid and IsGift = true";
                        var args = new
                        {
                            ID = id,
                            Skuautoid = a,
                            Coid = CoID,
                            Modifier = Username,
                            ModifyDate = DateTime.Now
                        };
                        int count = CoreDBconn.Execute(sqlCommandText, args, TransCore);
                        if (count <= 0)
                        {
                            rf.id = a;
                            rf.reason = "更新明细失败!";
                            rt.Add(rf);
                            continue;
                        }
                    }
                    if(business.isskulock == 0 && u[0].IsPaid == true)
                    {
                        sqlCommandText = @"update inventory_sale set LockQty = LockQty + 1,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                        int co =CoreDBconn.Execute(sqlCommandText,new{Modifier=Username,ModifyDate=DateTime.Now,ID = a,CoID = CoID},TransCore);
                        if(co < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                    rr.Add(a);
                    var log = new LogInsert();
                    log.OID = id;
                    log.SoID = u[0].SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = Username;
                    log.Title = "添加赠品";
                    log.Remark = s[0].skuid;
                    log.CoID = CoID;
                    logs.Add(log);     
                }           
                //更新订单的数量和重量
                if (rr.Count > 0)
                {
                    sqlCommandText = @"update `order` set ExWeight = ExWeight + @ExWeight,OrdQty = OrdQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    int count = CoreDBconn.Execute(sqlCommandText, new { ExWeight = weight,Qty = rr.Count,Modifier = Username, ModifyDate = DateTime.Now, ID = id, CoID = CoID }, TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    int r = CoreDBconn.Execute(loginsert,logs, TransCore);
                    if (r < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                }
                res.successIDs = rr;
                res.failIDs = rt;
                if (result.s == 1)
                {
                    TransCore.Commit();
                }
                // wheresql = "select status,amount,ExWeight from `order` where id =" + id + " and coid =" + CoID;
                // u = CoreDBconn.Query<Order>(wheresql).AsList();
                // sin.Amount = u[0].Amount;
                // sin.Status = u[0].Status;
                // sin.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                // sin.Weight = u[0].ExWeight;
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var ff = RefreshOrderItemQuick(id,CoID);
            if(ff.s == -1)
            {
                result.s = -1;
                result.d = ff.d;
                return result;
            }
            res.Order = ff.d as SingleOrderItem;
            // res.Order = sin;
            result.d = res;
            return result;
        }
        ///<summary>
        ///订单明细换货
        ///</summary>
        public static DataResult ChangeOrderDetail(int id,int oid,int skuidNew,int CoID,string Username)
        {
            var result = new DataResult(1,null);  
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            var logs = new List<Log>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select status,soid,amount,PaidAmount,IsPaid from `order` where id =" + oid + " and coid =" + CoID;
                var u = CoreDBconn.Query<Order>(wheresql).AsList();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "此订单不存在!";
                    return result;
                }
                else
                {
                    if (u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                    {
                        result.s = -1;
                        result.d = "只有待付款/已付款待审核/异常的订单才可以换货!";
                        return result;
                    }
                }
                string skusql = "select skuid,skuname,norm,img,goodscode,enable,saleprice,weight from coresku where id =" + skuidNew + " and coid =" + CoID;
                var s = CoreDBconn.Query<SkuInsert>(skusql).AsList();
                if (s.Count == 0)
                {
                    result.s = -1;
                    result.d = "换货的商品不存在!";
                    return result;
                }
                if (s[0].enable == false)
                {
                    result.s = -1;
                    result.d = "换货的商品已停用!";
                    return result;
                }
                string sqlCommandText = "select * from orderitem where oid = @OID and coid = @Coid and id = @ID";
                var item = CoreDBconn.Query<OrderItem>(sqlCommandText,new{OID=oid,Coid=CoID,ID=id}).AsList();
                if(business.isskulock == 0 && u[0].IsPaid == true)
                {
                    sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                    int co =CoreDBconn.Execute(sqlCommandText,new{Qty = item[0].Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = item[0].SkuAutoID,CoID = CoID},TransCore);
                    if(co < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                    co =CoreDBconn.Execute(sqlCommandText,new{Qty = item[0].Qty,Modifier=Username,ModifyDate=DateTime.Now,ID = skuidNew,CoID = CoID},TransCore);
                    if(co < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                sqlCommandText = "select count(id) from orderitem where oid = @OID and coid = @Coid and SkuAutoID = @Sku and IsGift = @IsGift";
                int count = CoreDBconn.QueryFirst<int>(sqlCommandText,new{OID = oid,Coid = CoID,Sku=skuidNew,IsGift=item[0].IsGift});
                if(count > 0)
                {
                    result.s = -1;
                    result.d = "换货的商品已经存在于订单明细!";
                    return result;
                }
                decimal weight = decimal.Parse(item[0].TotalWeight);
                int qty = item[0].Qty;
                sqlCommandText = @"update orderitem set SkuAutoID=@SkuAutoID,SkuID=@SkuID,SkuName=@SkuName,Norm=@Norm,GoodsCode=@GoodsCode,img=@img,
                                   Weight=@Weight,TotalWeight = Weight * Qty,Modifier =@Modifier,ModifyDate = @ModifyDate where oid = @OID and 
                                   coid = @Coid and id = @ID";
                var args = new{SkuAutoID =skuidNew,SkuID=s[0].skuid, SkuName=s[0].skuname, Norm=s[0].norm, GoodsCode=s[0].goodscode, img =s[0].img,Weight=s[0].weight,
                               Modifier = Username, ModifyDate=DateTime.Now, ID=id, Coid=CoID, OID = oid};
                count = CoreDBconn.Execute(sqlCommandText,args,TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                var log = new LogInsert();
                log.OID = oid;
                log.SoID = u[0].SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = Username;
                log.Title = "更改商品";
                log.Remark = item[0].SkuID + "=>" + s[0].skuid;
                log.CoID = CoID;

                sqlCommandText = @"update `order` set ExWeight = ExWeight - @ExWeight + @ExWeightNew,Modifier=@Modifier,ModifyDate=@ModifyDate 
                                    where ID = @ID and CoID = @CoID";
                count = CoreDBconn.Execute(sqlCommandText, new { ExWeight = weight, ExWeightNew = qty * decimal.Parse(s[0].weight) ,Modifier = Username, 
                                            ModifyDate = DateTime.Now, ID = oid, CoID = CoID }, TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int r = CoreDBconn.Execute(loginsert,log, TransCore);
                if (r < 0)
                {
                    result.s = -3002;
                    return result;
                }

                TransCore.Commit();
                // wheresql = "select status,amount,ExWeight from `order` where id =" + oid + " and coid =" + CoID;
                // u = CoreDBconn.Query<Order>(wheresql).AsList();
                // sin.Amount = u[0].Amount;
                // sin.Status = u[0].Status;
                // sin.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                // sin.Weight = u[0].ExWeight;
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var ff = RefreshOrderItemQuick(oid,CoID);
            if(ff.s == -1)
            {
                result.s = -1;
                result.d = ff.d;
                return result;
            }
            // sin.SkuList = 
            result.d = ff.d as SingleOrderItem;
            return result;
        }
        ///<summary>
        ///抓取单笔订单资料
        ///</summary>
        public static DataResult GetOrderEdit(int id,int CoID)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                        string sqlcommand = @"select ID,ShopName,ODate,OSource,SoID,PayDate,BuyerShopID,ExAmount,Express,ExCode,RecMessage,SendMessage,InvoiceTitle,RecLogistics,
                                            RecCity,RecDistrict,RecAddress,RecName,RecTel,RecPhone,Status,AbnormalStatus,StatusDec as AbnormalStatusDec,ExID,SkuAmount,Amount,
                                            PaidAmount From `order` where id = @ID and coid = @Coid";
                        var order = conn.Query<OrderEdit>(sqlcommand,new{ID=id,Coid=CoID}).AsList();
                        if(order.Count == 0)
                        {
                            result.s = -1;
                            result.d = "订单单号无效!";
                            return result;
                        }
                        order[0].OSource = Enum.GetName(typeof(OrdSource), int.Parse(order[0].OSource));
                        order[0].StatusDec = Enum.GetName(typeof(OrdStatus), order[0].Status);
                        if(!string.IsNullOrEmpty(order[0].ExID.ToString()))
                        {
                            order[0].ExpNamePinyin = GetExpNamePinyin(CoID,order[0].ExID);
                        }
                        if(!string.IsNullOrEmpty(order[0].PaidAmount))
                        {
                            if(decimal.Parse(order[0].PaidAmount) == 0)
                            {
                                order[0].PayDate = null;
                            }
                        }
                        result.d = order[0];
                    }
                    catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            return result;
        }
        ///<summary>
        ///抓取单笔订单的付款资料
        ///</summary>
        public static DataResult GetOrderPay(int id,int CoID)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                        string sqlcommand = @"select ID,PayNbr,Payment,PayDate,PayAmount,Status From payinfo where
                                            oid = @ID and coid = @Coid and Status != 2 order by PayDate Desc";
                        var pay = conn.Query<OrderPay>(sqlcommand,new{ID=id,Coid=CoID}).AsList();
                        result.d = pay;
                    }
                    catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            return result;
        }
        ///<summary>
        ///抓取单笔订单的订单明细
        ///</summary>
        public static DataResult GetOrderItem(int id,int CoID)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                        string sqlcommand = @"select ID,SkuAutoID,SkuID,SkuName,Norm,GoodsCode,Qty,SalePrice,RealPrice,Amount,img,IsGift,ShopSkuID From orderitem where
                                            oid = @ID and coid = @Coid";
                        var item = conn.Query<OrderItemEdit>(sqlcommand,new{ID=id,Coid=CoID}).AsList();
                        foreach(var i in item)
                        {
                            i.InvQty = GetInvQty(CoID,i.SkuAutoID);
                        }
                        result.d = item;
                    }
                    catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            return result;
        }
        ///<summary>
        ///抓取单笔订单的日志
        ///</summary>
        public static DataResult GetOrderLog(int id,int CoID)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                        string sqlcommand = @"select ID,LogDate,UserName,Title,Remark From orderlog where
                                            oid = @ID and coid = @Coid and type = 0 order by LogDate Asc";
                        var Log = conn.Query<OrderLog>(sqlcommand,new{ID=id,Coid=CoID}).AsList();
                        result.d = Log;
                    }
                    catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            return result;
        }
        ///<summary>
        ///返回订单详情
        ///</summary>
        public static DataResult GetOrderSingle(int id,int CoID)
        {
            var result = new DataResult(1,null);
            var res = new OrderSingle();
            var aa = GetOrderEdit(id,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Order = aa.d as OrderEdit;

            aa = GetOrderPay(id,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Pay = aa.d as List<OrderPay>;

            aa = GetOrderItem(id,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.OrderItem = aa.d as List<OrderItemEdit>;

            aa = GetOrderLog(id,CoID);
            if(aa.s == -1)
            {
                result.s = -1;
                result.d = aa.d;
                return result;
            }
            res.Log = aa.d as List<OrderLog>;
            result.d = res;
            return result;
        }
        ///<summary>
        ///修改备注
        ///</summary>
        public static DataResult ModifyRemark(int id,int CoID,string Username,string Remark)
        {
            var result = new DataResult(1,null);
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlcommand = @"select soid,SendMessage,status from `order` where id = " + id + " and coid = " + CoID;
                var order = CoreDBconn.Query<Order>(sqlcommand).AsList();
                if(order.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单无效!";
                    return result;
                }
                if(order[0].Status == 8)
                {
                    result.s = -1;
                    result.d = "等供销商发货的订单不可以改备注!";
                    return result;
                }
                var log = new LogInsert();
                log.OID = id;
                log.SoID = order[0].SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = Username;
                log.Title = "修改备注";
                log.Remark = order[0].SendMessage + "=>" + Remark;
                log.CoID = CoID;

                sqlcommand = @"update `order` set SendMessage = @SendMessage,Modifier=@Modifier,ModifyDate=@ModifyDate 
                               where ID = @ID and CoID = @CoID";
                int count = CoreDBconn.Execute(sqlcommand, new {SendMessage = Remark,Modifier = Username,ModifyDate = DateTime.Now,ID = id,CoID = CoID},TransCore);
                if (count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,log, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///修改收货地址
        ///</summary>
        public static DataResult ModifyAddress(string Logistics,string City,string District,string Address,string Name,string tel,string phone,int OID,string UserName,int CoID)
        {
            var result = new DataResult(1,null);   
            var logs = new List<LogInsert>();
            var log = new LogInsert();
            var OrdOlder = new Order();
            var CancleOrdAb = new Order();
            int reasonid = 0;
            List<int> idlist = new List<int>();
            int x = 0;//资料修改标记
            int z = 0;//地址修改标记
            string RecLogistics="",RecCity="",RecDistrict="",RecAddress="",RecName="";
            string querySql = "select * from `order` where id = @ID and coid = @Coid";
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    var u = conn.Query<Order>(querySql,new{ID = OID,Coid = CoID}).AsList();
                    if(u.Count == 0)
                    {
                        result.s = -1;
                        result.d = "参数无效";
                        return result;
                    }
                    else
                    {
                        if(u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                        {
                            result.s = -1;
                            result.d = "只有待审核/已付款待审核/异常的订单才可以修改收货地址!";
                            return result;
                        }
                    }
                    OrdOlder = u[0] as Order;
                    RecLogistics = OrdOlder.RecLogistics;
                    RecCity = OrdOlder.RecCity;
                    RecDistrict = OrdOlder.RecDistrict;
                    RecAddress = OrdOlder.RecAddress;
                    RecName = OrdOlder.RecName;
                    string addressOlder = RecLogistics + RecCity + RecDistrict + RecAddress;
                    if(OrdOlder.RecLogistics != Logistics)
                    {
                        OrdOlder.RecLogistics = Logistics;
                        x++;
                        z++;
                    }
                    if(OrdOlder.RecCity != City)
                    {
                        OrdOlder.RecCity = City;
                        x++;
                        z++;
                    }
                    if(OrdOlder.RecDistrict != District)
                    {
                        OrdOlder.RecDistrict = District;
                        x++;
                        z++;
                    }
                    if(OrdOlder.RecAddress != Address)
                    {
                        OrdOlder.RecAddress = Address;
                        x++;
                        z++;
                    }
                    if(z > 0)
                    {
                        log = new LogInsert();
                        log.OID = OrdOlder.ID;
                        log.SoID = OrdOlder.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "手动修改收货地址";
                        log.Remark = "收货地址" + addressOlder + " => " + OrdOlder.RecLogistics + OrdOlder.RecCity + OrdOlder.RecDistrict + OrdOlder.RecAddress;
                        log.CoID = CoID;
                        logs.Add(log);
                    }
                    if(OrdOlder.RecName != Name)
                    {
                        log = new LogInsert();
                        log.OID = OrdOlder.ID;
                        log.SoID = OrdOlder.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "手动修改收货人";
                        log.Remark = "收货人" + OrdOlder.RecName + " => " + Name;
                        log.CoID = CoID;
                        logs.Add(log);
                        OrdOlder.RecName = Name;
                        x++;
                        z++;
                    }
                    if(OrdOlder.RecTel != tel)
                    {
                        log = new LogInsert();
                        log.OID = OrdOlder.ID;
                        log.SoID = OrdOlder.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "手动修改电话";
                        log.Remark = "电话" + OrdOlder.RecTel + " => " + tel;
                        log.CoID = CoID;
                        logs.Add(log);
                        OrdOlder.RecTel = tel;
                        x++;
                    }
                    if(OrdOlder.RecPhone != phone)
                    {
                        log = new LogInsert();
                        log.OID = OrdOlder.ID;
                        log.SoID = OrdOlder.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "手动修改手机";
                        log.Remark = "手机" + OrdOlder.RecPhone + " => " + phone;
                        log.CoID = CoID;
                        logs.Add(log);
                        OrdOlder.RecPhone = phone;
                        x++;
                    }
                    //检查订单是否符合合并的条件
                    bool isCheck = false;
                    if(z > 0)
                    {
                        //若订单本身是等待合并时，先判断是否需要还原资料
                        if(OrdOlder.Status == 7 && OrdOlder.StatusDec == "等待订单合并")
                        {
                            isCheck= true;
                            var ck = isCheckCancleMerge(OID,CoID,OrdOlder.BuyerShopID,RecName,RecLogistics,RecCity,RecDistrict,RecAddress);
                            if(ck.s == 1)
                            {
                                int oid = int.Parse(ck.d.ToString());
                                querySql = "select * from `order` where id = " + oid + " and coid = " + CoID;
                                var v = conn.Query<Order>(querySql).AsList();
                                CancleOrdAb = v[0] as Order;
                                log = new LogInsert();
                                log.OID = CancleOrdAb.ID;
                                log.SoID = CancleOrdAb.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "取消异常标记";
                                log.Remark = "等待订单合并(自动)";
                                log.CoID = CoID;
                                logs.Add(log);
                                if(CancleOrdAb.IsPaid == true)
                                {
                                    CancleOrdAb.Status = 1;
                                    if(CancleOrdAb.Type == 3)
                                    {
                                        CancleOrdAb.Status = 8;
                                    }
                                }
                                else
                                {
                                    CancleOrdAb.Status = 0;
                                }
                                CancleOrdAb.AbnormalStatus = 0;
                                CancleOrdAb.StatusDec="";
                                CancleOrdAb.Modifier = UserName;
                                CancleOrdAb.ModifyDate = DateTime.Now;
                            }
                        }
                        //检查订单是否符合合并的条件
                        var res = isCheckMerge(OrdOlder);
                        if(res.s == 1)
                        {
                            if(isCheck== false)
                            {
                                reasonid = GetReasonID("等待订单合并",CoID,7).s;
                                if(reasonid == -1)
                                {
                                    result.s = -1;
                                    result.d = "请先设定【等待订单合并】的异常";
                                    return result;
                                }
                                OrdOlder.Status = 7;
                                OrdOlder.AbnormalStatus = reasonid;
                                OrdOlder.StatusDec="等待订单合并";
                                log = new LogInsert();
                                log.OID = OrdOlder.ID;
                                log.SoID = OrdOlder.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "标记异常";
                                log.Remark = "等待订单合并(自动)";
                                log.CoID = CoID;
                                logs.Add(log);
                            }
                            idlist = res.d as List<int>;
                            querySql = "select id,soid from `order` where id in @ID and coid = @Coid and status <> 7";
                            var v = conn.Query<Order>(querySql,new{ID = idlist,Coid = CoID}).AsList();
                            if(v.Count > 0)
                            {
                                foreach(var c in v)
                                {
                                    log = new LogInsert();
                                    log.OID = c.ID;
                                    log.SoID = c.SoID;
                                    log.Type = 0;
                                    log.LogDate = DateTime.Now;
                                    log.UserName = UserName;
                                    log.Title = "标记异常";
                                    log.Remark = "等待订单合并(自动)";
                                    log.CoID = CoID;
                                    logs.Add(log);
                                }
                            }
                            else
                            {
                                idlist = new List<int>();
                            }
                        }   
                        else
                        {
                            if(isCheck == true)
                            {
                                log = new LogInsert();
                                log.OID = OrdOlder.ID;
                                log.SoID = OrdOlder.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "取消异常标记";
                                log.Remark = "等待订单合并(自动)";
                                log.CoID = CoID;
                                logs.Add(log);
                                if(OrdOlder.IsPaid == true)
                                {
                                    OrdOlder.Status = 1;
                                    if(OrdOlder.Type == 3)
                                    {
                                        OrdOlder.Status = 8;
                                    }
                                }
                                else
                                {
                                    OrdOlder.Status = 0;
                                }
                                OrdOlder.AbnormalStatus = 0;
                                OrdOlder.StatusDec="";
                            }
                        }
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlCommandText = string.Empty;
                int count =0;
                OrdOlder.Modifier = UserName;
                OrdOlder.ModifyDate = DateTime.Now;
                if(x > 0)
                {
                    sqlCommandText = @"update `order` set RecLogistics=@RecLogistics,RecCity=@RecCity,RecDistrict=@RecDistrict,RecAddress=@RecAddress,RecName=@RecName,
                                       RecTel=@RecTel,RecPhone=@RecPhone,Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,
                                       ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    count =CoreDBconn.Execute(sqlCommandText,OrdOlder,TransCore);
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    if(CancleOrdAb.ID > 0)
                    {
                        sqlCommandText = @"update `order` set Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                        count =CoreDBconn.Execute(sqlCommandText,CancleOrdAb,TransCore);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                    if(idlist.Count > 0)
                    {
                        sqlCommandText = @"update `order` set status = 7,abnormalstatus = @Abnormalstatus,statusdec = '等待订单合并' where id in @ID and coid = @Coid and status <> 7";
                        count =CoreDBconn.Execute(sqlCommandText,new {Abnormalstatus = reasonid,ID = idlist,Coid = CoID},TransCore);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    count =CoreDBconn.Execute(loginsert,logs,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }         
                }       
                TransCore.Commit();
            }catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return  result;
        }
        ///<summary>
        ///设定快递显示
        ///</summary>
        public static DataResult GetExp(int CoID,bool IsQuick,string Logistics,string City,string District)
        {
            var result = new DataResult(1,null);
            var res = new SetExp();
            if(IsQuick == true)
            {
                using(var conn = new MySqlConnection(DbBase.CommConnectString) ){
                    try{  
                            string sqlcommand = "select id from area where Name = '" + Logistics + "' and LevelType = 1";
                            int id = conn.QueryFirst<int>(sqlcommand);
                            sqlcommand = "select id from area where Name = '" + City + "' and LevelType = 2 and ParentId = " + id;
                            id = conn.QueryFirst<int>(sqlcommand);
                            sqlcommand = "select id from area where Name = '" + District + "' and LevelType = 3 and ParentId = " + id;
                            id = conn.QueryFirst<int>(sqlcommand);
                            sqlcommand = @"select kd_name,cp_name_raw,cp_location,delivery_contact,delivery_area_1,delivery_area_0 From kd_cj_02 where
                                           city_id_raw = " + id + " and kd_id_sys > 0 order by kd_name";
                            var u = conn.Query<LogisticsNetwork>(sqlcommand).AsList();
                            res.LogisticsNetwork = u;
                        }
                        catch(Exception ex){
                        result.s = -1;
                        result.d = ex.Message;
                        conn.Dispose();
                    }
                }
            }
            var Express = CoreComm.ExpressHaddle.GetExpressSimple(CoID).d as List<ExpressSimple>;
            // var a = new ExpressSimple();
            // a.ID = "A";
            // a.Name= "{清空已设快递}";
            // Express.Add(a);
            // a = new ExpressSimple();
            // a.ID = "B";
            // a.Name= "{让系统自动计算}";
            // Express.Add(a);
            // a = new ExpressSimple();
            // a.ID = "C";
            // a.Name= "{菜鸟智选物流}";
            // Express.Add(a);
            res.Express = Express;
            result.d = res;
            return result;
        }
        ///<summary>
        ///设定快递更新
        ///</summary>
        public static DataResult SetExp(List<int> oid,int CoID,string ExpID,string ExpName,string UserName)
        {            
            var result = new DataResult(1,null);
            string title = "手工指定快递";
            if(ExpID == "A")
            {
                title = "手工清空快递";
            }
            if(ExpID == "B")
            {
                title = "自动计算快递";
            }
            if(ExpID == "C")
            {
                title = "菜鸟智选快递";
            }
            var logs = new List<LogInsert>();
            var re = new List<int>();
            var ress = new SetExpReturn();
            var su = new List<SetExpSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlcommand = "select * from `order` where id in @ID and coid = @Coid";
                var u = CoreDBconn.Query<Order>(sqlcommand,new{id = oid,Coid = CoID}).AsList();
                foreach(var a in u)
                {
                    if(a.Status == 3 || a.Status == 4 || a.Status == 5 || a.Status == 6 || a.Status == 8)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有待付款/已付款待审核/已审核待配快递/异常的订单才可以设定快递";
                        fa.Add(ff);
                        continue;
                    }
                    if(ExpID == "B")
                    {
                        var res = new DataResult(1,"圆通速递");//待新增方法
                        if(res.s == -1)
                        {
                            var ff = new TransferNormalReturnFail();
                            ff.ID = a.ID;
                            ff.Reason = res.d.ToString();
                            fa.Add(ff);
                            continue;
                            }
                        ExpID = res.s.ToString();
                        ExpName = res.d.ToString();
                    }
                    if(ExpID == "C")
                    {
                        var res = new DataResult(1,"圆通速递");//待新增方法
                        if(res.s == -1)
                        {
                            var ff = new TransferNormalReturnFail();
                            ff.ID = a.ID;
                            ff.Reason = res.d.ToString();
                            fa.Add(ff);
                            continue;
                        }
                        ExpID = res.s.ToString();
                        ExpName = res.d.ToString();
                    }
                    if(ExpID == "A")
                    {
                        ExpID = null;
                        ExpName = null;
                    }
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = title;
                    if(ExpID == "A")
                    {
                        log.Remark = a.Express;
                    }
                    else
                    {
                        log.Remark = ExpName;
                    }
                    log.CoID = CoID;
                    logs.Add(log);
                    if(u[0].Status != 2 || ExpID == null)
                    {
                        sqlcommand = @"update `order` set ExID=@ExID,Express =@Express,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                        int j =CoreDBconn.Execute(sqlcommand,new{ExID=ExpID,Express=ExpName,Modifier=UserName,ModifyDate=DateTime.Now,id = a.ID,Coid = CoID},TransCore);
                        if(j < 0)
                        {
                            result.s = -3003;
                            return result;
                        }     
                    }
                    else
                    {
                        a.ExID = int.Parse(ExpID);
                        a.Express = ExpName;
                        log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "提交仓库发货";
                        log.CoID = CoID;
                        logs.Add(log);
                        //获取电子面单
                        string ExCode = "";
                        if(a.Express != "现场取货")
                        {
                            sqlcommand = "select ExpNo as ExCode from expnounused where CoID = " + CoID + " and Express = " + a.ExID;
                            var e = CoreDBconn.Query<ExpnoUnused>(sqlcommand).AsList();
                            if(e.Count == 0)
                            {
                                int reasonid = GetReasonID("获取电子面单错误",CoID,7).s;
                                if(reasonid == -1)
                                {
                                    result.s = -1;
                                    result.d = "请先设定【获取电子面单错误】的异常";
                                    return result;
                                }
                                a.Status = 7;
                                a.AbnormalStatus = reasonid;
                                a.StatusDec="获取电子面单错误";
                                log = new LogInsert();
                                log.OID = a.ID;
                                log.SoID = a.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "标记异常";
                                log.Remark = "获取电子面单错误";
                                log.CoID = CoID;
                                logs.Add(log);
                            }
                            else
                            {
                                ExCode = e[0].ExCode;
                                log = new LogInsert();
                                log.OID = a.ID;
                                log.SoID = a.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "获取电子面单";
                                log.Remark = ExCode;
                                log.CoID = CoID;
                                logs.Add(log);
                                //快递单号处理
                                sqlcommand = "delete from expnounused where CoID = " + CoID + " and Express = " + a.ExID + " and ExpNo = '" + ExCode + "'";
                                int jcount = CoreDBconn.Execute(sqlcommand,TransCore);
                                if(jcount < 0)
                                {
                                    result.s = -3004;
                                    return result;
                                }
                                sqlcommand = @"INSERT INTO expnoused(CoID,Express,ExpNo,OID) VALUES(@CoID,@Express,@ExpNo,@OID)";
                                jcount = CoreDBconn.Execute(sqlcommand,new{CoID=CoID,Express=a.ExID,ExpNo=ExCode,OID=a.ID},TransCore);
                                if(jcount < 0)
                                {
                                    result.s = -3002;
                                    return result;
                                }
                            }
                        }
                        if(a.Status != 7)
                        {
                            a.Status = 3;
                            a.ExCode = "0";//待新增方法
                        }
                        a.Modifier = UserName;
                        a.ModifyDate = DateTime.Now;
                        //更新订单
                        sqlcommand = @"update `order` set ExID=@ExID,Express=@Express,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                    StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                        int qcount = CoreDBconn.Execute(sqlcommand,a,TransCore);     
                        if(qcount < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        if(a.Status != 7)
                        {
                            int SID= 0;
                            //投递资料处理
                            var saleout = new SaleOutInsert();
                            saleout.OID = a.ID;
                            saleout.SoID = a.SoID;
                            saleout.DocDate = DateTime.Now;
                            saleout.Status = 0;
                            saleout.ExpName = a.Express;
                            saleout.ExCode = ExCode;
                            saleout.RecMessage = a.RecMessage;
                            saleout.RecLogistics = a.RecLogistics;
                            saleout.RecDistrict = a.RecDistrict;
                            saleout.RecCity = a.RecCity;
                            saleout.RecAddress = a.RecAddress;
                            saleout.RecZip = a.RecZip;
                            saleout.RecName = a.RecName;
                            saleout.RecPhone = a.RecPhone;
                            saleout.ExWeight = a.ExWeight;
                            saleout.ExCost = a.ExCost;
                            saleout.Amount = a.Amount;
                            saleout.OrdQty = a.OrdQty;
                            saleout.SendWarehouse = a.SendWarehouse;
                            saleout.PayDate = a.PayDate;
                            saleout.SendMessage = a.SendMessage;
                            saleout.CoID = CoID;
                            saleout.Creator = UserName;
                            saleout.Modifier = UserName;
                            saleout.ExID = a.ExID;
                            saleout.ShopID = a.ShopID;
                            saleout.Distributor = a.Distributor;
                            sqlcommand = @"INSERT INTO saleout(OID,SoID,DocDate,Status,ExpName,ExCode,RecMessage,RecLogistics,RecDistrict,RecCity,RecAddress,RecZip,RecName,Distributor,
                                                                    RecPhone,ExWeight,ExCost,Amount,OrdQty,SendWarehouse,PayDate,SendMessage,CoID,Creator,Modifier,ExID,ShopID) 
                                                            VALUES(@OID,@SoID,@DocDate,@Status,@ExpName,@ExCode,@RecMessage,@RecLogistics,@RecDistrict,@RecCity,@RecAddress,@RecZip,@RecName,@Distributor,
                                                                    @RecPhone,@ExWeight,@ExCost,@Amount,@OrdQty,@SendWarehouse,@PayDate,@SendMessage,@CoID,@Creator,@Modifier,@ExID,@ShopID)";
                            int y = CoreDBconn.Execute(sqlcommand,saleout,TransCore);
                            if(y < 0)
                            {
                                result.s = -3002;
                                return result;
                            }
                            else
                            {
                                SID = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                            }
                            var  ru = CoreComm.WarehouseHaddle.wareInfoByID(a.WarehouseID.ToString(),CoID.ToString()).d as dynamic;;
                            var wa = ru.Lst as List<wareInfo>;
                            int ItCoid = 0;
                            if(wa[0].itcoid == 0)
                            {
                                ItCoid = wa[0].coid;
                            }
                            else
                            {
                                ItCoid = wa[0].itcoid;
                            }
                            //更新库存资料
                            sqlcommand = "select * from orderitem where oid = " + a.ID + " and coid = " + CoID;
                            var item = CoreDBconn.Query<OrderItem>(sqlcommand).AsList();
                            foreach(var i in item)
                            {
                                sqlcommand = @"update inventory_sale set PickQty = PickQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                                int jcount = CoreDBconn.Execute(sqlcommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=CoID},TransCore);     
                                if(jcount < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                                sqlcommand = @"update inventory set PickQty =PickQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                                jcount = CoreDBconn.Execute(sqlcommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=ItCoid},TransCore);     
                                if(jcount < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                                var saleoutitem = new SaleOutItemInsert();
                                saleoutitem.SID = SID;
                                saleoutitem.OID = i.OID;
                                saleoutitem.SoID = i.SoID;
                                saleoutitem.SkuAutoID = i.SkuAutoID;
                                saleoutitem.SkuID = i.SkuID;
                                saleoutitem.SkuName = i.SkuName;
                                saleoutitem.Norm = i.Norm;
                                saleoutitem.GoodsCode = i.GoodsCode;
                                saleoutitem.Qty = i.Qty;
                                saleoutitem.SalePrice = i.SalePrice;
                                saleoutitem.RealPrice = i.RealPrice;
                                saleoutitem.Amount = i.Amount;
                                saleoutitem.DiscountRate = i.DiscountRate;
                                saleoutitem.img = i.img;
                                saleoutitem.ShopSkuID = i.ShopSkuID;
                                saleoutitem.Weight = i.Weight;
                                saleoutitem.TotalWeight = i.TotalWeight;
                                saleoutitem.IsGift = i.IsGift;
                                saleoutitem.CoID = i.CoID;
                                saleoutitem.Creator = UserName;
                                saleoutitem.Modifier = UserName;
                                sqlcommand = @"INSERT INTO saleoutitem(SID,OID,SoID,SkuAutoID,SkuID,SkuName,Norm,GoodsCode,Qty,SalePrice,RealPrice,Amount,DiscountRate,img,
                                                                    ShopSkuID,Weight,TotalWeight,IsGift,CoID,Creator,Modifier) 
                                                                VALUES(@SID,@OID,@SoID,@SkuAutoID,@SkuID,@SkuName,@Norm,@GoodsCode,@Qty,@SalePrice,@RealPrice,@Amount,@DiscountRate,@img,
                                                                    @ShopSkuID,@Weight,@TotalWeight,@IsGift,@CoID,@Creator,@Modifier)";
                                jcount = CoreDBconn.Execute(sqlcommand,saleoutitem,TransCore);
                                if(jcount < 0)
                                {
                                    result.s = -3002;
                                    return result;
                                }
                            }
                            
                        }
                    }
                    var ss = new SetExpSuccess();
                    ss.ID = a.ID;
                    ss.ExID = ExpID;
                    ss.Express = ExpName;
                    if(!string.IsNullOrEmpty(ExpID))
                    {
                        ss.ExpNamePinyin = GetExpNamePinyin(CoID,int.Parse(ExpID));
                    }
                    su.Add(ss);
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int count =CoreDBconn.Execute(loginsert,logs,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }     
                ress.SuccessIDs = su;
                ress.FailIDs = fa;
                result.d = ress;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///设定仓库基本显示
        ///</summary>
        public static DataResult GetWarehouse(int CoID)
        {
            var result = new DataResult(1,null);
            var wh = new List<Filter>();
            var w = CoreComm.WarehouseHaddle.getWarelist(CoID.ToString());
            foreach(var h in w)
            {
                var a = new Filter();
                a.value = h.id.ToString();
                a.label = h.warename;
                wh.Add(a);
            }
            var b = new Filter();
            b.value = "A";
            b.label = "{依照订单分配策略自动计算}";
            wh.Add(b);
            result.d = wh;
            return result;
        }
        ///<summary>
        ///订单策略挑选仓库
        ///</summary>
        public static DataResult StrategyWarehouse(Order ord,List<OrderItem> itemm,int CoID)
        {
            var result = new DataResult(1,null);
            var lst = new List<OrdWhStrategy>();
            int rtn = 0;
            using(var conn = new MySqlConnection(DbBase.CommConnectString) ){
                try{  
                        //抓取本仓的代号
                        string sqlcommand = @"select ID from ware_third_party where coid = " + CoID + " and enable in (0,2) and itcoid = 0";
                        rtn = conn.QueryFirst<int>(sqlcommand);
                        sqlcommand = @"select * from wareploy where coid = " + CoID;
                        var u = conn.Query<OrdWhStrategy>(sqlcommand).AsList();
                        if(u.Count == 0)
                        {
                            result.s = rtn;
                            return result;
                        }
                        //将符合订单条件的资料筛选出来
                        foreach(var a in u)
                        {
                            //限定省份
                            if(!string.IsNullOrEmpty(a.Province))
                            {
                                sqlcommand = @"select ID from area where LevelType = 1 and Name = '" +ord.RecLogistics + "'";
                                int p = conn.QueryFirst<int>(sqlcommand);
                                if(!a.Province.Contains(p.ToString()))
                                {
                                    continue;
                                }
                            }
                            //限定店铺
                            if(!string.IsNullOrEmpty(a.Shopid))
                            {
                                string[] shopid = a.Shopid.Split(',');
                                int p = 0;
                                foreach(var s in shopid)
                                {
                                    if(s == ord.ShopID.ToString())
                                    {
                                        p++;
                                        break;
                                    }
                                }
                                if(p == 0)
                                {
                                    continue;
                                }
                            }
                            //包含商品
                            if(!string.IsNullOrEmpty(a.ContainSkus))
                            {
                                bool ischeck = false;
                                foreach(var i in itemm)
                                {
                                    if(i.IsGift == true) continue;
                                    if(string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainSkus.Contains(i.SkuID))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainSkus.Contains(i.SkuID) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainSkus.Contains(i.SkuID) && i.Qty >= int.Parse(a.MinNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainSkus.Contains(i.SkuID) && i.Qty >= int.Parse(a.MinNum) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                }
                                if(ischeck == false)
                                {
                                    continue;
                                }
                            }
                            //包含款式
                            if(!string.IsNullOrEmpty(a.ContainGoods))
                            {
                                bool ischeck = false;
                                foreach(var i in itemm)
                                {
                                    if(i.IsGift == true) continue;
                                    if(string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainGoods.Contains(i.GoodsCode))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainGoods.Contains(i.GoodsCode) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainGoods.Contains(i.GoodsCode) && i.Qty >= int.Parse(a.MinNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.ContainGoods.Contains(i.GoodsCode) && i.Qty >= int.Parse(a.MinNum) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                }
                                if(ischeck == false)
                                {
                                    continue;
                                }
                            }
                            //排除商品
                            if(!string.IsNullOrEmpty(a.RemoveSkus))
                            {
                                bool ischeck = false;
                                foreach(var i in itemm)
                                {
                                    if(i.IsGift == true) continue;
                                    if(string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveSkus.Contains(i.SkuID))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveSkus.Contains(i.SkuID) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveSkus.Contains(i.SkuID) && i.Qty >= int.Parse(a.MinNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveSkus.Contains(i.SkuID) && i.Qty >= int.Parse(a.MinNum) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                }
                                if(ischeck == true)
                                {
                                    continue;
                                }
                            }
                             //排除款式
                            if(!string.IsNullOrEmpty(a.RemoveGoods))
                            {
                                bool ischeck = false;
                                foreach(var i in itemm)
                                {
                                    if(i.IsGift == true) continue;
                                    if(string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveGoods.Contains(i.GoodsCode))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveGoods.Contains(i.GoodsCode) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveGoods.Contains(i.GoodsCode) && i.Qty >= int.Parse(a.MinNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                    if(!string.IsNullOrEmpty(a.MinNum) && !string.IsNullOrEmpty(a.MaxNum))
                                    {
                                        if(a.RemoveGoods.Contains(i.GoodsCode) && i.Qty >= int.Parse(a.MinNum) && i.Qty <= int.Parse(a.MaxNum))
                                        {
                                            ischeck = true;
                                            break;
                                        }
                                    }
                                }
                                if(ischeck == true)
                                {
                                    continue;
                                }
                            }
                            //贷款方式
                            if(a.Payment == 2 && ord.IsCOD == false)
                            {
                                continue;
                            }
                            if(a.Payment == 3 && ord.IsCOD == true)
                            {
                                continue;
                            }
                            lst.Add(a);
                        }
                        if(lst.Count == 0)
                        {
                            result.s = rtn;
                            return result;
                        }
                    }
                    catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                        var u = new List<OrdWhStrategy>();
                        foreach(var a in u)
                        {
                            // 分销商
                            if(!string.IsNullOrEmpty(a.Did))
                            {
                                string[] did = a.Did.Split(',');
                                int p = 0;
                                if(!string.IsNullOrEmpty(ord.Distributor))
                                {
                                    string sqlcommand = @"select ID from distributor where DistributorName = '" +ord.RecLogistics + "' and coid = " + CoID + " and Type = 0";
                                    int d = conn.QueryFirst<int>(sqlcommand);
                                    foreach(var s in did)
                                    {
                                        if(s == d.ToString())
                                        {
                                            p++;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach(var s in did)
                                    {
                                        if(s == "0")
                                        {
                                            p++;
                                            break;
                                        }
                                    }
                                }
                                if(p == 0)
                                {
                                    continue;
                                }
                            }
                            u.Add(a);
                        }
                        lst = u;
                        if(lst.Count == 0)
                        {
                            result.s = rtn;
                            return result;
                        }
                        
                    }
                    catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }
            //取得优先级最小的资料
            int Priority = 10000;
            foreach(var a in lst)
            {
                if(Priority > int.Parse(a.Level))
                {
                    Priority = int.Parse(a.Level);
                    result.s = int.Parse(a.Wid);
                }
            }
            return result;
        }
        ///<summary>
        ///设定仓库更新
        ///</summary>
        public static DataResult SetWarehouse(List<int> oid,int CoID,string WhID,string UserName)
        {
            var result = new DataResult(1,null);
            var ress = new SetWarehouseReturn();
            var su = new List<SetWarehouseSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            string Remark = "手工指定仓库:";
            if(WhID == "A")
            {
                Remark = "系统自动指定仓库:";
            }
            string WhName = "";
            var w = CoreComm.WarehouseHaddle.getWarelist(CoID.ToString());
            if(WhID !="A")
            {
                foreach(var a in w)
                {
                    if(WhID == a.id.ToString())
                    {
                        WhName = a.warename;
                        break;
                    }
                }
            }
            var logs = new List<LogInsert>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlcommand = "select * from `order` where id in @ID and coid = @Coid and status in (0,1,7) and DealerType != 1";
                var u = CoreDBconn.Query<Order>(sqlcommand,new{ID = oid,Coid = CoID}).AsList();
                foreach(var a in u)
                {
                    if(a.Status != 0 && a.Status != 1 && a.Status != 7)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有待付款/已付款待审核/异常的订单才可以修改仓库!";
                        fa.Add(ff);
                        continue;
                    }   
                    if(a.DealerType == 1 || a.Type == 3)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "分销订单不可以修改仓库!";
                        fa.Add(ff);
                        continue;
                    }   
                    if(WhID == "A")
                    {
                        sqlcommand = "select * from orderitem where oid = @ID and coid = @Coid";
                        var item = CoreDBconn.Query<OrderItem>(sqlcommand,new{ID = a.ID,Coid = CoID}).AsList();
                        var res = StrategyWarehouse(a,item,CoID);
                        if(res.s == -1)
                        {
                            var ff = new TransferNormalReturnFail();
                            ff.ID = a.ID;
                            ff.Reason = res.d.ToString();
                            fa.Add(ff);
                            continue;
                        }
                        WhID = res.s.ToString();
                        foreach(var t in w)
                        {
                            if(WhID == t.id.ToString())
                            {
                                WhName = t.warename;
                                break;
                            }
                        }
                    }
                    sqlcommand = @"update `order` set WarehouseID=@WarehouseID,SendWarehouse=@SendWarehouse,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    int g =CoreDBconn.Execute(sqlcommand,new{WarehouseID=WhID,SendWarehouse=WhName,Modifier=UserName,ModifyDate=DateTime.Now,ID=a.ID,Coid=CoID},TransCore);
                    if(g < 0)
                    {
                        result.s = -3003;
                        return result;
                    }     
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "修改发货仓库";
                    log.Remark = Remark + WhName;
                    log.CoID = CoID;
                    logs.Add(log);
                    var ss = new SetWarehouseSuccess();
                    ss.ID = a.ID;
                    ss.Warehouse = WhName;
                    su.Add(ss);
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int count =CoreDBconn.Execute(loginsert,logs,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }   
                ress.SuccessIDs = su;
                ress.FailIDs = fa;
                result.d = ress;  
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///订单审核
        ///</summary>
        public static DataResult ConfirmOrder(List<int> oid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var bu = GetConfig(CoID);   
            var business = bu.d as Business;
            var res = new TransferNormalReturn();
            var su = new List<TransferNormalReturnSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var w = CoreComm.WarehouseHaddle.getWarelist(CoID.ToString());
            foreach(var o in oid)
            {
                var logs = new List<LogInsert>();
                var log = new LogInsert();
                string ExCode = string.Empty;
                int count = 0;
                var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
                CoreDBconn.Open();
                var TransCore = CoreDBconn.BeginTransaction();
                try
                {
                    string sqlcommand = @"select * from `order` where id = " + o + " and coid = " + CoID;
                    var u = CoreDBconn.Query<Order>(sqlcommand,new{ID = oid,Coid = CoID}).AsList();
                    if(u.Count == 0)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = o;
                        ff.Reason = "订单不存在!";
                        fa.Add(ff);
                        continue;
                    }
                    if(u[0].Amount != u[0].PaidAmount)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = o;
                        ff.Reason = "订单应付金额不等于付款金额!";
                        fa.Add(ff);
                        continue;
                    }
                    var a = u[0] as Order;
                    if(a.Status != 1)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有已付款待审核的订单才可以审核!";
                        fa.Add(ff);
                        continue;
                    }
                    sqlcommand = "select * from orderitem where oid = @ID and coid = @Coid";
                    var item = CoreDBconn.Query<OrderItem>(sqlcommand,new{ID = a.ID,Coid = CoID}).AsList();
                    //仓库为空时，选择仓库
                    if(string.IsNullOrEmpty(a.SendWarehouse))
                    {
                        var ss = StrategyWarehouse(a,item,CoID);
                        if(ss.s == -1)
                        {
                            var ff = new TransferNormalReturnFail();
                            ff.ID = a.ID;
                            ff.Reason = ss.d.ToString();
                            fa.Add(ff);
                            continue;
                        }
                        a.WarehouseID = ss.s.ToString();
                        foreach(var y in w)
                        {
                            if(a.WarehouseID == y.id.ToString())
                            {
                                a.SendWarehouse = y.warename;
                                break;
                            }
                        }
                        log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "修改发货仓库";
                        log.Remark = "审核时自动计算:" + a.SendWarehouse;
                        log.CoID = CoID;
                        logs.Add(log);
                    }
                    if(business.isignoresku == false)
                    {
                        bool IsCheckInv = false;
                        //检查订单是否缺货
                        foreach(var i in item)
                        {
                            int invqty = GetInvQty(CoID,i.SkuAutoID);
                            if(invqty < i.Qty)
                            {
                                IsCheckInv = true;
                                break;
                            }
                        }
                        if(IsCheckInv == true)
                        {
                            int reasonid = GetReasonID("缺货",CoID,7).s;
                            if(reasonid == -1)
                            {
                                result.s = -1;
                                result.d = "请先设定【缺货】的异常";
                                return result;
                            }
                            a.Status = 7;
                            a.AbnormalStatus = reasonid;
                            a.StatusDec="缺货";
                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "审核确认";
                            log.CoID = CoID;
                            logs.Add(log);
                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "标记异常";
                            log.Remark = "缺货(提交仓库时判断)";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                    }
                    if(business.isautosetexpress == false)
                    {
                        a.Status = 2;
                        log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "审核确认";
                        log.CoID = CoID;
                        logs.Add(log);
                    }
                    //快递为空时,自动计算快递
                    if(string.IsNullOrEmpty(a.Express) && a.Status !=7 && a.Status !=2)
                    {
                        var ss = new DataResult(1,"圆通快递");//待新增方法
                        if(ss.s == -1)
                        {
                            a.Status = 2;
                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "审核确认";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        if(a.Status != 2)
                        {
                            a.ExID = ss.s;
                            a.Express = ss.d.ToString();

                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "自动计算快递";
                            log.Remark = a.Express;
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                    }
                    if(a.Status != 2 &&　a.Status != 7) 
                    {
                        log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "审核确认";
                        log.CoID = CoID;
                        logs.Add(log);
                        log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "提交仓库发货";
                        log.CoID = CoID;
                        logs.Add(log);
                    }
                    //获取电子面单
                    if(a.Express != "现场取货" && a.Status != 2 &&　a.Status != 7)
                    {
                        sqlcommand = "select ExpNo as ExCode from expnounused where CoID = " + CoID + " and Express = " + a.ExID;
                        var e = CoreDBconn.Query<ExpnoUnused>(sqlcommand).AsList();
                        if(e.Count == 0)
                        {
                            int reasonid = GetReasonID("获取电子面单错误",CoID,7).s;
                            if(reasonid == -1)
                            {
                                result.s = -1;
                                result.d = "请先设定【获取电子面单错误】的异常";
                                return result;
                            }
                            a.Status = 7;
                            a.AbnormalStatus = reasonid;
                            a.StatusDec="获取电子面单错误";
                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "标记异常";
                            log.Remark = "获取电子面单错误";
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        else
                        {
                            ExCode = e[0].ExCode;
                            log = new LogInsert();
                            log.OID = a.ID;
                            log.SoID = a.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "获取电子面单";
                            log.Remark = ExCode;
                            log.CoID = CoID;
                            logs.Add(log);
                            //快递单号处理
                            sqlcommand = "delete from expnounused where CoID = " + CoID + " and Express = " + a.ExID + " and ExpNo = '" + ExCode + "'";
                            count = CoreDBconn.Execute(sqlcommand,TransCore);
                            if(count < 0)
                            {
                                result.s = -3004;
                                return result;
                            }
                            sqlcommand = @"INSERT INTO expnoused(CoID,Express,ExpNo,OID) VALUES(@CoID,@Express,@ExpNo,@OID)";
                            count = CoreDBconn.Execute(sqlcommand,new{CoID=CoID,Express=a.ExID,ExpNo=ExCode,OID=a.ID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3002;
                                return result;
                            }
                        }
                    }
                    if(a.Status != 2 &&　a.Status != 7)
                    {
                        a.Status = 3;
                        a.ExCode = "0";//待新增方法
                    }
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    //更新订单
                    sqlcommand = @"update `order` set WarehouseID =@WarehouseID,SendWarehouse=@SendWarehouse,ExID=@ExID,Express=@Express,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                   StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlcommand,a,TransCore);     
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    //更新库存资料
                    if(a.Status != 7)
                    {
                        var  ru = CoreComm.WarehouseHaddle.wareInfoByID(a.WarehouseID.ToString(),CoID.ToString()).d as dynamic;;
                        var wa = ru.Lst as List<wareInfo>;
                        int ItCoid = 0;
                        if(wa[0].itcoid == 0)
                        {
                            ItCoid = wa[0].coid;
                        }
                        else
                        {
                            ItCoid = wa[0].itcoid;
                        }
                        foreach(var i in item)
                        {
                            if(business.isskulock == 1)
                            {
                                sqlcommand = @"update inventory_sale set LockQty = LockQty + @Qty,PickQty = PickQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate 
                                               where Skuautoid = @ID and coid = @Coid";
                            }
                            else
                            {
                                sqlcommand = @"update inventory_sale set PickQty = PickQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                            }                          
                            count = CoreDBconn.Execute(sqlcommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=CoID},TransCore);     
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                            sqlcommand = @"update inventory set PickQty =PickQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                            count = CoreDBconn.Execute(sqlcommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=ItCoid},TransCore);     
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }    
                    if(a.Status != 2 &&　a.Status != 7)
                    {
                        int SID = 0;
                        //投递资料处理
                        var saleout = new SaleOutInsert();
                        saleout.OID = a.ID;
                        saleout.SoID = a.SoID;
                        saleout.DocDate = DateTime.Now;
                        saleout.Status = 0;
                        saleout.ExpName = a.Express;
                        saleout.ExCode = ExCode;
                        saleout.RecMessage = a.RecMessage;
                        saleout.RecLogistics = a.RecLogistics;
                        saleout.RecDistrict = a.RecDistrict;
                        saleout.RecCity = a.RecCity;
                        saleout.RecAddress = a.RecAddress;
                        saleout.RecZip = a.RecZip;
                        saleout.RecName = a.RecName;
                        saleout.RecPhone = a.RecPhone;
                        saleout.ExWeight = a.ExWeight;
                        saleout.ExCost = a.ExCost;
                        saleout.Amount = a.Amount;
                        saleout.OrdQty = a.OrdQty;
                        saleout.SendWarehouse = a.SendWarehouse;
                        saleout.PayDate = a.PayDate;
                        saleout.SendMessage = a.SendMessage;
                        saleout.CoID = CoID;
                        saleout.Creator = UserName;
                        saleout.Modifier = UserName;
                        saleout.ExID = a.ExID;
                        saleout.ShopID = a.ShopID;
                        saleout.Distributor = a.Distributor;
                        sqlcommand = @"INSERT INTO saleout(OID,SoID,DocDate,Status,ExpName,ExCode,RecMessage,RecLogistics,RecDistrict,RecCity,RecAddress,RecZip,RecName,Distributor,
                                                                RecPhone,ExWeight,ExCost,Amount,OrdQty,SendWarehouse,PayDate,SendMessage,CoID,Creator,Modifier,ExID,ShopID) 
                                                        VALUES(@OID,@SoID,@DocDate,@Status,@ExpName,@ExCode,@RecMessage,@RecLogistics,@RecDistrict,@RecCity,@RecAddress,@RecZip,@RecName,@Distributor,
                                                                @RecPhone,@ExWeight,@ExCost,@Amount,@OrdQty,@SendWarehouse,@PayDate,@SendMessage,@CoID,@Creator,@Modifier,@ExID,@ShopID)";
                        count = CoreDBconn.Execute(sqlcommand,saleout,TransCore);
                        if(count < 0)
                        {
                            result.s = -3002;
                            return result;
                        }
                        else
                        {
                            SID = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                        }
                        //销售出库单
                        foreach(var i in item)
                        {
                            var saleoutitem = new SaleOutItemInsert();
                            saleoutitem.SID = SID;
                            saleoutitem.OID = i.OID;
                            saleoutitem.SoID = i.SoID;
                            saleoutitem.SkuAutoID = i.SkuAutoID;
                            saleoutitem.SkuID = i.SkuID;
                            saleoutitem.SkuName = i.SkuName;
                            saleoutitem.Norm = i.Norm;
                            saleoutitem.GoodsCode = i.GoodsCode;
                            saleoutitem.Qty = i.Qty;
                            saleoutitem.SalePrice = i.SalePrice;
                            saleoutitem.RealPrice = i.RealPrice;
                            saleoutitem.Amount = i.Amount;
                            saleoutitem.DiscountRate = i.DiscountRate;
                            saleoutitem.img = i.img;
                            saleoutitem.ShopSkuID = i.ShopSkuID;
                            saleoutitem.Weight = i.Weight;
                            saleoutitem.TotalWeight = i.TotalWeight;
                            saleoutitem.IsGift = i.IsGift;
                            saleoutitem.CoID = i.CoID;
                            saleoutitem.Creator = UserName;
                            saleoutitem.Modifier = UserName;
                            sqlcommand = @"INSERT INTO saleoutitem(SID,OID,SoID,SkuAutoID,SkuID,SkuName,Norm,GoodsCode,Qty,SalePrice,RealPrice,Amount,DiscountRate,img,
                                                                ShopSkuID,Weight,TotalWeight,IsGift,CoID,Creator,Modifier) 
                                                            VALUES(@SID,@OID,@SoID,@SkuAutoID,@SkuID,@SkuName,@Norm,@GoodsCode,@Qty,@SalePrice,@RealPrice,@Amount,@DiscountRate,@img,
                                                                @ShopSkuID,@Weight,@TotalWeight,@IsGift,@CoID,@Creator,@Modifier)";
                            count = CoreDBconn.Execute(sqlcommand,saleoutitem,TransCore);
                            if(count < 0)
                            {
                                result.s = -3002;
                                return result;
                            }
                        }
                        
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    count =CoreDBconn.Execute(loginsert,logs,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }   
                    var sr = new TransferNormalReturnSuccess();
                    sr.ID = a.ID;
                    sr.Status = a.Status;
                    sr.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);
                    su.Add(sr);
                    TransCore.Commit();
                }
                catch (Exception e)
                {
                    TransCore.Rollback();
                    TransCore.Dispose();
                    result.s = -1;
                    result.d = e.Message;
                }
                finally
                {
                    TransCore.Dispose();
                    CoreDBconn.Dispose();
                }
            }    
            res.SuccessIDs = su;
            res.FailIDs = fa;
            result.d = res;

            return result;
        }
        ///<summary>
        ///新增异常说明
        ///</summary>
        public static DataResult InsertOrderAbnormal(string OrderAbnormal,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                if(string.IsNullOrEmpty(OrderAbnormal))
                {
                    string sqlcommand = @"delete from orderabnormal where IsCustom = true and OrdStatus = 7";
                    int count = CoreDBconn.Execute(sqlcommand,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                }
                else
                {
                    var aa = GetAbnormalList(CoID,7).d as List<OrderAbnormal>;
                    string[] ab = OrderAbnormal.Split(',');
                    //不存在资料库的资料新怎
                    foreach(var a in ab)
                    {
                        var res = GetReasonID(a,CoID,7);
                        if (res.s > 0) continue;
                        string sqlcommand = @"INSERT INTO orderabnormal(Name,IsCustom,CoID,Creator,OrdStatus) 
                                                                VALUES(@Name,@IsCustom,@CoID,@Creator,@OrdStatus)";
                        int count = CoreDBconn.Execute(sqlcommand,new{Name=a,IsCustom=true,CoID=CoID,Creator=UserName,OrdStatus=7},TransCore);
                        if(count < 0)
                        {
                            result.s = -3002;
                            return result;
                        }
                    }
                    //删除资料
                    foreach(var a in aa)
                    {
                        if(a.iscustom == false) continue;
                        bool flag = false;
                        foreach(var b in ab)
                        {
                            if(a.label == b)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if(flag == false)
                        {
                            string sqlcommand = @"delete from orderabnormal where IsCustom = true and Name = '" + a.label + "' and OrdStatus = 7";
                            int count = CoreDBconn.Execute(sqlcommand,TransCore);
                            if(count < 0)
                            {
                                result.s = -3002;
                                return result;
                            }
                        }
                    }                    
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            var u = GetAbnormalList(CoID,7);
            result.d = u.d;
            return result;
        }
        ///<summary>
        ///单据转异常
        ///</summary>
        public static DataResult TransferAbnormal(List<int> oid,int CoID,string UserName,int AbnormalStatus,string Remark)
        {
            var result = new DataResult(1,null);
            var yy = GetReasonName(AbnormalStatus,CoID,7);
            if(yy.s == -1)
            {
                result.s = -1;
                result.d = "异常原因参数异常!";
                return result;
            }
            string StatusDec = yy.d.ToString();
            var logs = new List<LogInsert>();
            var res = new TransferAbnormalReturn();
            var su = new List<TransferAbnormalSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                sqlCommandText = "select id,soid,StatusDec,status,AbnormalStatus,coid,ExID,WarehouseID from `order` where id in @ID and coid = @Coid";
                var u = CoreDBconn.Query<Order>(sqlCommandText,new {ID = oid,Coid = CoID}).AsList();
                foreach(var a in u)
                {
                    if(a.Status == 4 || a.Status == 5 || a.Status == 6)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "待付款/已付款待审核/已审核待配快递/发货中/异常/等供销商发货的订单才可以转异常";
                        fa.Add(ff);
                        continue;
                    }
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "标记异常";
                    if(string.IsNullOrEmpty(Remark))
                    {
                        log.Remark = StatusDec;
                    }
                    else
                    {
                        log.Remark = StatusDec+"(" + Remark + ")";
                    }
                    log.CoID = CoID;
                    logs.Add(log);
                    if(a.Status == 3)
                    {
                        sqlCommandText = @"select ExCode from saleout where oid = " + a.ID + " and coid = " + CoID + " and status = 0 and BatchID = 0";
                        var sa = CoreDBconn.Query<SaleOutInsert>(sqlCommandText).AsList();
                        if(sa.Count == 0)
                        {
                            var ff = new TransferNormalReturnFail();
                            ff.ID = a.ID;
                            ff.Reason = "订单已产生批次,不能标记异常";
                            fa.Add(ff);
                            continue;
                        }
                        string ExCode = sa[0].ExCode;
                        sqlCommandText=@"update saleout set Status = 6,Modifier=@Modifier,ModifyDate=@ModifyDate where oid = @ID and coid = @Coid and status = 0";
                        count = CoreDBconn.Execute(sqlCommandText,new{Modifier=UserName,ModifyDate=DateTime.Now,ID = a.ID,Coid = CoID},TransCore);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        //快递单号处理
                        sqlCommandText = "delete from expnoused where CoID = " + CoID + " and Express = " + a.ExID + " and ExpNo = '" + ExCode + "'";
                        count = CoreDBconn.Execute(sqlCommandText,TransCore);
                        if(count < 0)
                        {
                            result.s = -3004;
                            return result;
                        }
                        sqlCommandText = @"INSERT INTO expnounused(CoID,Express,ExpNo) VALUES(@CoID,@Express,@ExpNo)";
                        count = CoreDBconn.Execute(sqlCommandText,new{CoID=CoID,Express=a.ExID,ExpNo=ExCode},TransCore);
                        if(count < 0)
                        {
                            result.s = -3002;  
                            return result;
                        }
                        //更新库存资料
                        var  ru = CoreComm.WarehouseHaddle.wareInfoByID(a.WarehouseID.ToString(),CoID.ToString()).d as dynamic;;
                        var wa = ru.Lst as List<wareInfo>;
                        int ItCoid = 0;
                        if(wa[0].itcoid == 0)
                        {
                            ItCoid = wa[0].coid;
                        }
                        else
                        {
                            ItCoid = wa[0].itcoid;
                        }
                        sqlCommandText = "select * from orderitem where oid = " + a.ID + " and coid = " + CoID;
                        var item = CoreDBconn.Query<OrderItem>(sqlCommandText).AsList();
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set PickQty = PickQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                            count = CoreDBconn.Execute(sqlCommandText,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=CoID},TransCore);     
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                            sqlCommandText = @"update inventory set PickQty =PickQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                            count = CoreDBconn.Execute(sqlCommandText,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=ItCoid},TransCore);     
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                    a.Status = 7;
                    a.AbnormalStatus = AbnormalStatus;
                    a.StatusDec = StatusDec;
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate 
                                       where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    var ss =new TransferAbnormalSuccess();
                    ss.ID =a.ID;
                    ss.Status =a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);;
                    ss.AbnormalStatus =a.AbnormalStatus;
                    ss.AbnormalStatusDec =a.StatusDec;
                    
                    su.Add(ss);
                }                
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int j = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (j < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///取消订单
        ///</summary>
        public static DataResult CancleOrder(List<int> oid,int CoID,string UserName,int reasonID,string Remark)
        {
            var result = new DataResult(1,null);
            var yy = GetReasonName(reasonID,CoID,6);
            if(yy.s == -1)
            {
                result.s = -1;
                result.d = "取消原因参数异常!";
                return result;
            }
            string StatusDec = yy.d.ToString();
            var logs = new List<LogInsert>();
            var res = new TransferAbnormalReturn();
            var su = new List<TransferAbnormalSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                sqlCommandText = "select id,soid,StatusDec,status,AbnormalStatus,coid,ExID,WarehouseID,IsPaid from `order` where id in @ID and coid = @Coid";
                var u = CoreDBconn.Query<Order>(sqlCommandText,new {ID = oid,Coid = CoID}).AsList();
                foreach(var a in u)
                {
                    if(a.Status == 4 || a.Status == 5 || a.Status == 6)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "待付款/已付款待审核/已审核待配快递/发货中/异常/等供销商发货的订单才可以取消";
                        fa.Add(ff);
                        continue;
                    }
                    ischeckPaid = a.IsPaid;
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消";
                    if(string.IsNullOrEmpty(Remark))
                    {   
                        log.Remark = StatusDec;
                    }   
                    else
                    {
                        log.Remark = StatusDec + "(" + Remark + ")";
                    }
                    log.CoID  = CoID;
                    logs.Add(log);
                    if(a.Status == 3)
                    {
                        sqlCommandText = @"select ExCode from saleout where oid = " + a.ID + " and coid = " + CoID + " and status = 0 and BatchID = 0";
                        var sa = CoreDBconn.Query<SaleOutInsert>(sqlCommandText).AsList();
                        if(sa.Count == 0)
                        {
                            var ff = new TransferNormalReturnFail();
                            ff.ID = a.ID;
                            ff.Reason = "订单已产生批次,不能取消";
                            fa.Add(ff);
                            continue;
                        }
                        string ExCode = sa[0].ExCode;
                        sqlCommandText=@"update saleout set Status = 6,Modifier=@Modifier,ModifyDate=@ModifyDate where oid = @ID and coid = @Coid and status = 0";
                        count = CoreDBconn.Execute(sqlCommandText,new{Modifier=UserName,ModifyDate=DateTime.Now,ID = a.ID,Coid = CoID},TransCore);
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        //快递单号处理
                        sqlCommandText = "delete from expnoused where CoID = " + CoID + " and Express = " + a.ExID + " and ExpNo = '" + ExCode + "'";
                        count = CoreDBconn.Execute(sqlCommandText,TransCore);
                        if(count < 0)
                        {
                            result.s = -3004;
                            return result;
                        }
                        sqlCommandText = @"INSERT INTO expnounused(CoID,Express,ExpNo) VALUES(@CoID,@Express,@ExpNo)";
                        count = CoreDBconn.Execute(sqlCommandText,new{CoID=CoID,Express=a.ExID,ExpNo=ExCode},TransCore);
                        if(count < 0)
                        {
                            result.s = -3002;  
                            return result;
                        }
                        //更新库存资料
                        var  ru = CoreComm.WarehouseHaddle.wareInfoByID(a.WarehouseID.ToString(),CoID.ToString()).d as dynamic;;
                        var wa = ru.Lst as List<wareInfo>;
                        int ItCoid = 0;
                        if(wa[0].itcoid == 0)
                        {
                            ItCoid = wa[0].coid;
                        }
                        else
                        {
                            ItCoid = wa[0].itcoid;
                        }
                        sqlCommandText = "select * from orderitem where oid = " + a.ID + " and coid = " + CoID;
                        item = CoreDBconn.Query<OrderItem>(sqlCommandText).AsList();
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set PickQty = PickQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                            count = CoreDBconn.Execute(sqlCommandText,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=CoID},TransCore);     
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                            sqlCommandText = @"update inventory set PickQty =PickQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                            count = CoreDBconn.Execute(sqlCommandText,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=ItCoid},TransCore);     
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                    if(((business.isskulock == 0 && (a.Status == 0 || a.Status == 1 || a.Status == 7 || a.Status == 8 )) || (business.isskulock == 1 && (a.Status==2 ||a.Status == 3))) && ischeckPaid == true)
                    {
                        item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + a.ID + " and coid = " + CoID).AsList();
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                    a.Status = 6;
                    a.AbnormalStatus = 0;
                    a.StatusDec = "";
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate 
                                       where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    var ss = new TransferAbnormalSuccess();
                    ss.ID =a.ID;
                    ss.Status =a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);;
                    ss.AbnormalStatus = a.AbnormalStatus;
                    ss.AbnormalStatusDec = a.StatusDec;
                    
                    su.Add(ss);
                }                
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int j = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (j < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///分销付款
        ///</summary>
        public static DataResult DistributionPay(List<int> id,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var res = new DistributionPayReturn();
            var su = new List<DistributionPaySuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var logs = new List<LogInsert>();
            var pay = new PayInfo();
            var pays = new List<PayInfo>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string wheresql = "select * from `order` where id in @ID and coid = @Coid";
                var u = CoreDBconn.Query<Order>(wheresql,new{ID = id,Coid = CoID}).AsList();
                if (u.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号参数无效!";
                    return result;
                }
                foreach(var ord in u)
                {
                    if (ord.Status != 0 && ord.Status != 1 && ord.Status != 7)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = ord.ID;
                        ff.Reason = "只有待付款/已付款待审核/异常的订单才可以新增付款!";
                        fa.Add(ff);
                        continue;
                    }
                    if(ord.DealerType != 2)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = ord.ID;
                        ff.Reason = "供销订单才可以使用分销付款!";
                        fa.Add(ff);
                        continue;
                    }
                    ischeckPaid = ord.IsPaid;
                    decimal PaidAmount=0,PayAmount=0,Amount=0;
                    if(!string.IsNullOrEmpty(ord.PaidAmount))
                    {
                        PaidAmount = decimal.Parse(ord.PaidAmount);
                    }
                    if(!string.IsNullOrEmpty(ord.PayAmount))
                    {
                        PayAmount = decimal.Parse(ord.PayAmount);
                    }
                    if(!string.IsNullOrEmpty(ord.Amount))
                    {
                        Amount = decimal.Parse(ord.Amount);
                    }
                    if(Amount - PaidAmount <= 0)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = ord.ID;
                        ff.Reason = "该笔订单已完成支付，不需再支付!";
                        fa.Add(ff);
                        continue;
                    }
                    pay = new PayInfo();
                    pay.RecID = ord.BuyerID;
                    pay.RecName = ord.RecName;
                    pay.Title = ord.InvoiceTitle;
                    pay.DataSource = 0;
                    pay.Status = 1;
                    pay.CoID = CoID;
                    pay.OID = ord.ID;
                    pay.SoID = ord.SoID;
                    pay.Payment = "供销支付";
                    pay.PayNbr = "J" + DateTime.Now.Ticks.ToString().Substring(0,12);
                    pay.PayDate = DateTime.Now;
                    pay.PayAmount = (Amount - PaidAmount).ToString();
                    pay.Amount = (Amount - PaidAmount).ToString();
                    pay.Creator = UserName;
                    pay.CreateDate = DateTime.Now;
                    pay.Confirmer = UserName;
                    pay.ConfirmDate = DateTime.Now;
                    pay.BuyerShopID = ord.BuyerShopID;
                    pays.Add(pay);
                    var log = new LogInsert();
                    log.OID = pay.OID;
                    log.SoID = pay.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "分销付款";
                    log.Remark = pay.PayAmount;
                    log.CoID = CoID;
                    logs.Add(log);
                    if(ord.StatusDec == "付款金额不等于应付金额" && ord.Status == 7)
                    {
                        log = new LogInsert();
                        log.OID = pay.OID;
                        log.SoID = pay.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "取消异常标记";
                        log.Remark = ord.StatusDec;
                        log.CoID = CoID;
                        logs.Add(log);
                        ord.Status = 1;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    if(ord.Status != 7)
                    {
                        ord.Status = 1;
                        ord.AbnormalStatus = 0;
                        ord.StatusDec = "";
                    }
                    
                    //更新订单
                    ord.PaidAmount = Amount.ToString();
                    ord.PayAmount = (Amount - PaidAmount + decimal.Parse(ord.PayAmount)).ToString();
                    if(ord.PayDate == null || ord.PayDate <= DateTime.Parse("1900-01-01"))
                    {
                        ord.PayDate = pay.PayDate;
                    }
                    if(string.IsNullOrEmpty(ord.PayNbr))
                    {
                        ord.PayNbr = pay.PayNbr;
                    }
                    ord.IsPaid = true;
                    ord.Modifier = UserName;
                    ord.ModifyDate = DateTime.Now;
                    string sqlCommand = @"update `order` set PaidAmount = @PaidAmount,PayAmount = @PayAmount,PayDate =@PayDate,PayNbr = @PayNbr,IsPaid=@IsPaid,Status=@Status,AbnormalStatus=@AbnormalStatus,
                                        StatusDec=@StatusDec,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    int j = CoreDBconn.Execute(sqlCommand,ord, TransCore);
                    if (j < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    if(business.isskulock == 0 && ischeckPaid == false)
                    {
                        item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + ord.ID + " and coid = " + CoID).AsList();
                        foreach(var i in item)
                        {
                            sqlCommand = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            j =CoreDBconn.Execute(sqlCommand,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(j < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                    var ss = new DistributionPaySuccess();
                    ss.ID = ord.ID;
                    ss.PayDate = ord.PayDate.ToString();
                    ss.PaidAmount = ord.PaidAmount;
                    ss.Status = ord.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), ord.Status);//获取名称
                    ss.AbnormalStatus = ord.AbnormalStatus;
                    ss.AbnormalStatusDec = ord.StatusDec;
                    su.Add(ss);
                }
                //新增支付单资料
                string sqlCommandText = @"INSERT INTO payinfo(PayNbr,RecID,RecName,OID,SOID,Payment,PayAccount,PayDate,Title,Amount,PayAmount,DataSource,Status,CoID,Creator,CreateDate,Confirmer,ConfirmDate,BuyerShopID) 
                                    VALUES(@PayNbr,@RecID,@RecName,@OID,@SOID,@Payment,@PayAccount,@PayDate,@Title,@Amount,@PayAmount,@DataSource,@Status,@CoID,@Creator,@CreateDate,@Confirmer,@ConfirmDate,@BuyerShopID)";
                int count = CoreDBconn.Execute(sqlCommandText,pays,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            res.SuccessIDs = su;
            res.FailIDs = fa;
            result.d = res;
            return result;
        }
        ///<summary>
        ///反取消订单
        ///</summary>
        public static DataResult RestoreCancleOrder(List<int> oid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var res = new TransferNormalReturn();
            var su = new List<TransferNormalReturnSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                sqlCommandText = "select id,soid,Amount,status,PaidAmount,coid,Type,IsPaid from `order` where id in @ID and coid = @Coid";
                var u = CoreDBconn.Query<Order>(sqlCommandText,new {ID = oid,Coid = CoID}).AsList();
                foreach(var a in u)
                {
                    if(a.Status != 6)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "取消的订单才可以反取消";
                        fa.Add(ff);
                        continue;
                    }
                    ischeckPaid = a.IsPaid;
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "反取消";
                    log.CoID  = CoID;
                    logs.Add(log);
                    
                    if(a.Amount == a.PaidAmount)
                    {
                        if(a.Type == 3)
                        {
                            a.Status = 8;
                        }
                        else
                        {
                            a.Status = 1;
                        }
                        if(business.isskulock == 0 && ischeckPaid == true)
                        {
                            item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + a.ID + " and coid = " + CoID).AsList();
                            foreach(var i in item)
                            {
                                sqlCommandText = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                                if(count < 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                            }
                        }
                    }
                    else
                    {
                        a.Status = 0;
                    }
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set Status=@Status,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    var ss = new TransferNormalReturnSuccess();
                    ss.ID =a.ID;
                    ss.Status =a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);;
                    
                    su.Add(ss);
                }                
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int j = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (j < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///修改商品
        ///</summary>
        public static DataResult ModifySku(List<int> oid,int ModifySku,decimal ModifyPrice,int DeleteSku,int AddSku,decimal AddPrice,
                                            decimal AddQty,string AddType,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var res = new ModifySkuReturn();
            var fa = new List<TransferNormalReturnFail>();
            var ss = new List<int>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlcommand = "select * from `order` where id in @ID and coid = @Coid";
                var ord = CoreDBconn.Query<Order>(sqlcommand,new{ID = oid,Coid = CoID}).AsList();
                if(ord.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号参数异常!";
                    return result;
                }
                foreach(var i in ord)
                {
                    if(i.Status  == 3 || i.Status  == 4 || i.Status  == 5|| i.Status  == 6 || i.Status  == 8 || i.Type  == 3)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = i.ID;
                        ff.Reason = "待付款/已付款待审核/已审核待配快递/异常的非天猫分销订单才可以修改商品";
                        fa.Add(ff);
                        continue;
                    }
                    ischeckPaid = i.IsPaid;
                    int j = 0;//订单明细修改记录
                    //修改商品单价
                    if(ModifySku > 0)
                    {
                        sqlcommand = "select * from orderitem where oid = " + i.ID + " and coid = " + CoID + " and skuautoid = " + ModifySku + " and IsGift = false";
                        var d = CoreDBconn.Query<OrderItem>(sqlcommand).AsList();
                        if (d.Count > 0)
                        {
                            sqlcommand = @"update orderitem set RealPrice=@RealPrice,Amount=RealPrice*Qty,DiscountRate=RealPrice/SalePrice,Modifier=@Modifier,
                                           ModifyDate = @ModifyDate where oid = @ID and coid = @Coid and skuautoid = @Sku and IsGift = false";
                            count = CoreDBconn.Execute(sqlcommand,new{RealPrice = ModifyPrice,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.ID,Coid=CoID,Sku=ModifySku},TransCore);
                            if(count <= 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                            else
                            {
                                j ++;
                            }
                            var log = new LogInsert();
                            log.OID = i.ID;
                            log.SoID = i.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "修改单价";
                            log.Remark = d[0].SkuID + ":" + d[0].RealPrice + "=>" + ModifyPrice;
                            log.CoID = CoID;
                            logs.Add(log);
                        } 
                    }
                    //删除商品
                    if(DeleteSku > 0)
                    {
                        sqlcommand = "select * from orderitem where oid = " + i.ID + " and coid = " + CoID + " and skuautoid = " + DeleteSku;
                        var d = CoreDBconn.Query<OrderItem>(sqlcommand).AsList();
                        if (d.Count > 0)
                        {
                            sqlcommand = @"delete from orderitem where oid = @ID and coid = @Coid and skuautoid = @Sku";
                            count = CoreDBconn.Execute(sqlcommand,new{ID=i.ID,Coid=CoID,Sku=DeleteSku},TransCore);
                            if(count <= 0)
                            {
                                result.s = -3004;
                                return result;
                            }
                            else
                            {
                                j ++;
                            }
                            var log = new LogInsert();
                            log.OID = i.ID;
                            log.SoID = i.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "删除商品";
                            log.Remark = d[0].SkuID + "(" + d[0].RealPrice + "*" + d[0].Qty + ")";
                            log.CoID = CoID;
                            logs.Add(log);
                        } 
                    }
                    //新增商品
                    if(AddSku > 0)
                    {
                        sqlcommand = "select * from orderitem where oid = " + i.ID + " and coid = " + CoID + " and skuautoid = " + AddSku + " and IsGift = false";
                        var d = CoreDBconn.Query<OrderItem>(sqlcommand).AsList();
                        if(d.Count == 0)
                        {
                            sqlcommand = "select skuid,skuname,norm,img,goodscode,enable,saleprice,weight from coresku where id =" + AddSku + " and coid =" + CoID;
                            var s = CoreDBconn.Query<SkuInsert>(sqlcommand).AsList();
                            if (s.Count == 0)
                            {
                                result.s = -1;
                                result.d = "添加的商品不存在!";
                                return result;
                            }
                            else
                            {
                                if (s[0].enable == false)
                                {
                                    result.s = -1;
                                    result.d = "添加的商品已经停用!";
                                    return result;
                                }
                            }
                            sqlcommand = @"INSERT INTO orderitem(oid,soid,coid,skuautoid,skuid,skuname,norm,GoodsCode,qty,saleprice,realprice,amount,img,weight,totalweight,
                                                                DiscountRate,creator,modifier) 
                                            VALUES(@OID,@Soid,@Coid,@Skuautoid,@Skuid,@Skuname,@Norm,@GoodsCode,@Qty,@Saleprice,@Realprice,@Amount,@Img,@Weight,@Totalweight,
                                                  @DiscountRate,@Creator,@Creator)";
                            var args = new
                            {
                                OID = i.ID,
                                Soid = i.SoID,
                                Skuautoid = AddSku,
                                Skuid = s[0].skuid,
                                Skuname = s[0].skuname,
                                Norm = s[0].norm,
                                GoodsCode = s[0].goodscode,
                                Qty = AddQty,
                                Saleprice = s[0].saleprice,
                                Realprice = AddPrice,
                                Amount = AddPrice * AddQty,
                                Img = s[0].img,
                                Weight = s[0].weight,
                                Totalweight =  AddQty * decimal.Parse(s[0].weight),
                                DiscountRate = AddPrice/decimal.Parse(s[0].saleprice),
                                Coid = CoID,
                                Creator = UserName
                            };
                            count = CoreDBconn.Execute(sqlcommand, args, TransCore);
                            if (count <= 0)
                            {
                                result.s = -3002;
                                return result;
                            }
                            else
                            {
                                j++;
                            }
                            var log = new LogInsert();
                            log.OID = i.ID;
                            log.SoID = i.SoID;
                            log.Type = 0;
                            log.LogDate = DateTime.Now;
                            log.UserName = UserName;
                            log.Title = "添加商品";
                            log.Remark = s[0].skuid;
                            log.CoID = CoID;
                            logs.Add(log);
                        }
                        else
                        {
                            if(AddType == "B")
                            {
                                sqlcommand = @"update orderitem set Qty=Qty + @Qty,Amount=RealPrice*Qty,TotalWeight=Weight*Qty,Modifier=@Modifier,
                                               ModifyDate = @ModifyDate where oid = @ID and coid = @Coid and skuautoid = @Sku and IsGift = false";
                                count = CoreDBconn.Execute(sqlcommand,new{Qty = AddQty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.ID,Coid=CoID,Sku=AddSku},TransCore);
                                if(count <= 0)
                                {
                                    result.s = -3003;
                                    return result;
                                }
                                else
                                {
                                    j ++;
                                }
                                var log = new LogInsert();
                                log.OID = i.ID;
                                log.SoID = i.SoID;
                                log.Type = 0;
                                log.LogDate = DateTime.Now;
                                log.UserName = UserName;
                                log.Title = "修改商品数量";
                                log.Remark = d[0].SkuID + ":" + d[0].Qty + "=>" + AddQty;
                                log.CoID = CoID;
                                logs.Add(log);
                            }
                        } 
                    }
                    if(j > 0)
                    {
                        ss.Add(i.ID);
                        sqlcommand = "select sum(Qty) as QtyTot,sum(Amount) as AmtTot,sum(TotalWeight) as WeightTot from orderitem where oid = " + i.ID + " and coid = " + CoID + " and isgift = false";
                        var su = CoreDBconn.Query<OrdSum>(sqlcommand).AsList();
                        sqlcommand = "select sum(Qty) as QtyTot,sum(Amount) as AmtTot,sum(TotalWeight) as WeightTot from orderitem where oid = " + i.ID + " and coid = " + CoID + " and isgift = true";
                        var su2 = CoreDBconn.Query<OrdSum>(sqlcommand).AsList();
                        i.OrdQty = su[0].QtyTot;
                        i.SkuAmount = (su[0].AmtTot + su2[0].AmtTot).ToString();
                        i.Amount = (decimal.Parse(i.SkuAmount) + decimal.Parse(i.ExAmount)).ToString();
                        i.ExWeight = (su[0].WeightTot + su2[0].WeightTot).ToString();
                        if(business.isskulock == 0)
                        {
                            item = CoreDBconn.Query<OrderItem>("select * from orderitem where oid = " + i.ID + " and coid = " + CoID).AsList();
                        }
                        if(i.PaidAmount == i.Amount)
                        {
                            i.IsPaid = true;
                            if(i.Status != 7)
                            {
                                i.Status = 1;
                            }
                            if(i.Status != 7 && i.Type == 3)
                            {
                                i.Status = 8;
                            }
                            if(business.isskulock == 0 && ischeckPaid == false)
                            {
                                foreach(var it in item)
                                {
                                    sqlcommand = @"update inventory_sale set LockQty = LockQty + @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                    count =CoreDBconn.Execute(sqlcommand,new{Qty = it.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = it.SkuAutoID,CoID = CoID},TransCore);
                                    if(count < 0)
                                    {
                                        result.s = -3003;
                                        return result;
                                    }
                                }
                            }
                        }
                        else
                        {
                            i.IsPaid = false;
                            if(i.Status != 7)
                            {
                                i.Status = 0;
                            }
                            if(business.isskulock == 0 && ischeckPaid == true)
                            {
                                foreach(var it in item)
                                {
                                    sqlcommand = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                                    count =CoreDBconn.Execute(sqlcommand,new{Qty = it.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = it.SkuAutoID,CoID = CoID},TransCore);
                                    if(count < 0)
                                    {
                                        result.s = -3003;
                                        return result;
                                    }
                                }
                            }
                        }
                        i.Modifier = UserName;
                        i.ModifyDate = DateTime.Now;
                        sqlcommand = @"update `order` set OrdQty=@OrdQty,SkuAmount=@SkuAmount,Amount=@Amount,ExWeight=@ExWeight,IsPaid=@IsPaid,Status=@Status,Modifier=@Modifier,
                                       ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                        count = CoreDBconn.Execute(sqlcommand,i,TransCore);
                        if(count <= 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        
                    }
                    else
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = i.ID;
                        ff.Reason = "无符合条件的资料异动!";
                        fa.Add(ff);
                        continue;
                    }
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count =CoreDBconn.Execute(loginsert,logs,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }   
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            res.FailIDs = fa;
            if(ss.Count > 0)
            {
                var rr = GetOrderListSingle(ss,CoID);
                var dd = rr.d as OrderData;
                res.SuccessIDs = dd.Ord;
            }
            result.d = res;
            return result;
        }
        ///<summary>
        ///取消审核
        ///</summary>
        public static DataResult CancleConfirmOrder(List<int> oid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var res = new TransferNormalReturn();
            var su = new List<TransferNormalReturnSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            var item = new List<OrderItem>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                sqlCommandText = "select id,soid,status,coid,Type,IsPaid from `order` where id in @ID and coid = @Coid";
                var u = CoreDBconn.Query<Order>(sqlCommandText,new {ID = oid,Coid = CoID}).AsList();
                foreach(var a in u)
                {
                    if(a.Status != 2)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "已审核待配快递的订单才可以取消审核";
                        fa.Add(ff);
                        continue;
                    }
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消审核";
                    log.CoID  = CoID;
                    logs.Add(log);
                    if(a.Type == 3)
                    {
                        a.Status = 8;
                    }
                    else
                    {
                        a.Status = 1;
                    }
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set Status=@Status,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    if(business.isskulock == 1)
                    {
                        foreach(var i in item)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            count =CoreDBconn.Execute(sqlCommandText,new{Qty = i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID = i.SkuAutoID,CoID = CoID},TransCore);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                    }
                    var ss = new TransferNormalReturnSuccess();
                    ss.ID =a.ID;
                    ss.Status =a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);;
                    
                    su.Add(ss);
                }                
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                        VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                int j = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (j < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///直接发货
        ///</summary>
        public static DataResult DirectShip(int oid,string Excode,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            if(business.ischeckfirst == false)
            {
                result.s = -1;
                result.d = "未开放[直接发货]功能模块,请确认业务流程是否设定正确!";
                return result;
            }
            var logs = new List<LogInsert>();
            var log = new LogInsert();
            var res = new DirectShipReturn();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlCommand = "select * from `order` where id = " + oid + " and coid = " + CoID;
                var u = CoreDBconn.Query<Order>(sqlCommand).AsList();
                if(u.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号参数异常";
                    return result;
                }
                if(u[0].Status != 2 && u[0].Status != 3)
                {
                    result.s = -1;
                    result.d = "已审核待配快递/发货中的订单才可以直接发货!";
                    return result;
                }
                if(u[0].Status == 3)
                {
                    sqlCommand = @"select * from saleout where oid = " + oid + " and coid = " + CoID + " and status = 0";
                    var sa = CoreDBconn.Query<SaleOutInsert>(sqlCommand).AsList();
                    if(sa.Count == 0)
                    {
                        result.s = -1;
                        result.d = "读取不到该订单的待出库销售出库单,请确认资料!";
                        return result;
                    }
                    sqlCommand = "select ExpNo from expnoused where oid =" +oid + " and coid = " +CoID;
                    var ee = CoreDBconn.Query<ExpnoUnused>(sqlCommand).AsList();
                    if(ee[0].ExCode != Excode)
                    {
                        result.s = -1;
                        result.d = "该笔订单已获取电子面单" + ee[0].ExCode + ",请输入正确的单号!";
                        return result;
                    }
                    if(sa[0].BatchID != 0)
                    {
                        result.s = -1;
                        result.d = "该笔订单已产生批次,不可直接发货!";
                        return result;
                    }
                    u[0].Status = 4;
                    u[0].ExCode = Excode;
                    u[0].Modifier = UserName;
                    u[0].ModifyDate = DateTime.Now;
                    sqlCommand = @"update `order` set Status=@Status,ExCode=@ExCode,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommand,u[0],TransCore);     
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    log = new LogInsert();
                    log.OID = u[0].ID;
                    log.SoID = u[0].SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "直接发货";
                    log.CoID = CoID;
                    logs.Add(log);
                    var  ru = CoreComm.WarehouseHaddle.wareInfoByID(u[0].WarehouseID.ToString(),CoID.ToString()).d as dynamic;;
                    var wa = ru.Lst as List<wareInfo>;
                    int ItCoid = 0;
                    if(wa[0].itcoid == 0)
                    {
                        ItCoid = wa[0].coid;
                    }
                    else
                    {
                        ItCoid = wa[0].itcoid;
                    }
                    //更新库存资料
                    sqlCommand = "select * from orderitem where oid = " + oid + " and coid = " + CoID;
                    var item = CoreDBconn.Query<OrderItem>(sqlCommand).AsList();
                    foreach(var i in item)
                    {
                        sqlCommand = @"update inventory_sale set PickQty=PickQty-@Qty,StockQty = StockQty - @Qty,LockQty = LockQty - @Qty,Modifier=@Modifier,
                                       ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                        count = CoreDBconn.Execute(sqlCommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=CoID},TransCore);     
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        sqlCommand = @"update inventory set PickQty=PickQty-@Qty,StockQty = StockQty - @Qty,Modifier=@Modifier,
                                       ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                        count = CoreDBconn.Execute(sqlCommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=ItCoid},TransCore);     
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                    //投递资料处理
                    sqlCommand = @"update saleout set Status=1,ExCode=@ExCode,Modifier=@Modifier,ModifyDate=@ModifyDate where oid = @OID  and coid = @Coid and status = 0";
                    count = CoreDBconn.Execute(sqlCommand,new{Modifier=UserName,ModifyDate=DateTime.Now,OID=oid,Coid=CoID,ExCode=Excode},TransCore);     
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    count =CoreDBconn.Execute(loginsert,logs,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }  
                }
                if(u[0].Status == 2)
                {
                    sqlCommand = "select express from expnounused where CoID = " + CoID + " and ExpNo = '" + Excode + "'";
                    var ex = CoreDBconn.Query<ExpnoUnused>(sqlCommand).AsList();
                    if(ex.Count == 0)
                    {
                        result.s = -1;
                        result.d = "快递单号参数无效!";
                        return result;
                    }
                    u[0].ExID = ex[0].Express;
                    var exp = GetExpName(ex[0].Express,CoID);
                    if(exp.s == -1)
                    {
                        result.s = -1;
                        result.d = "快递单号对应的快递公司不存在!";
                        return result;
                    }
                    u[0].Express = exp.d.ToString();
                    u[0].Status = 4;
                    u[0].ExCode = Excode;
                    u[0].ExCost = "0";//待新增方法计算
                    u[0].Modifier = UserName;
                    u[0].ModifyDate = DateTime.Now;
                    sqlCommand = @"update `order` set ExID=@ExID,Express=@Express,Status=@Status,ExCode=@ExCode,ExCost=@ExCost,Modifier=@Modifier,
                                   ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommand,u[0],TransCore);     
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    log = new LogInsert();
                    log.OID = u[0].ID;
                    log.SoID = u[0].SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "根据快递单号指定快递";
                    log.Remark = u[0].Express;
                    log.CoID = CoID;
                    logs.Add(log);
                    log = new LogInsert();
                    log.OID = u[0].ID;
                    log.SoID = u[0].SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "提交仓库发货";
                    log.CoID = CoID;
                    logs.Add(log);
                    log = new LogInsert();
                    log.OID = u[0].ID;
                    log.SoID = u[0].SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "直接发货";
                    log.CoID = CoID;
                    logs.Add(log);
                    //快递单号处理
                    sqlCommand = "delete from expnounused where CoID = " + CoID + " and Express = " + u[0].ExID + " and ExpNo = '" + Excode + "'";
                    count = CoreDBconn.Execute(sqlCommand,TransCore);
                    if(count < 0)
                    {
                        result.s = -3004;
                        return result;
                    }
                    sqlCommand = @"INSERT INTO expnoused(CoID,Express,ExpNo,OID) VALUES(@CoID,@Express,@ExpNo,@OID)";
                    count = CoreDBconn.Execute(sqlCommand,new{CoID=CoID,Express=u[0].ExID,ExpNo=Excode,OID=u[0].ID},TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    var  ru = CoreComm.WarehouseHaddle.wareInfoByID(u[0].WarehouseID.ToString(),CoID.ToString()).d as dynamic;;
                    var wa = ru.Lst as List<wareInfo>;
                    int ItCoid = 0;
                    if(wa[0].itcoid == 0)
                    {
                        ItCoid = wa[0].coid;
                    }
                    else
                    {
                        ItCoid = wa[0].itcoid;
                    }
                    int SID = 0;
                    //投递资料处理
                    var saleout = new SaleOutInsert();
                    saleout.OID = u[0].ID;
                    saleout.SoID = u[0].SoID;
                    saleout.DocDate = DateTime.Now;
                    saleout.Status = 1;
                    saleout.ExpName = u[0].Express;
                    saleout.ExCode = Excode;
                    saleout.RecMessage = u[0].RecMessage;
                    saleout.RecLogistics = u[0].RecLogistics;
                    saleout.RecDistrict = u[0].RecDistrict;
                    saleout.RecCity = u[0].RecCity;
                    saleout.RecAddress = u[0].RecAddress;
                    saleout.RecZip = u[0].RecZip;
                    saleout.RecName = u[0].RecName;
                    saleout.RecPhone = u[0].RecPhone;
                    saleout.ExWeight = u[0].ExWeight;
                    saleout.ExCost = u[0].ExCost;
                    saleout.Amount = u[0].Amount;
                    saleout.OrdQty = u[0].OrdQty;
                    saleout.SendWarehouse = u[0].SendWarehouse;
                    saleout.PayDate = u[0].PayDate;
                    saleout.SendMessage = u[0].SendMessage;
                    saleout.CoID = CoID;
                    saleout.Creator = UserName;
                    saleout.Modifier = UserName;
                    saleout.ExID = u[0].ExID;
                    saleout.ShopID = u[0].ShopID;
                    saleout.Distributor = u[0].Distributor;
                    sqlCommand = @"INSERT INTO saleout(OID,SoID,DocDate,Status,ExpName,ExCode,RecMessage,RecLogistics,RecDistrict,RecCity,RecAddress,RecZip,RecName,Distributor,
                                                            RecPhone,ExWeight,ExCost,Amount,OrdQty,SendWarehouse,PayDate,SendMessage,CoID,Creator,Modifier,ExID,ShopID) 
                                                    VALUES(@OID,@SoID,@DocDate,@Status,@ExpName,@ExCode,@RecMessage,@RecLogistics,@RecDistrict,@RecCity,@RecAddress,@RecZip,@RecName,@Distributor,
                                                            @RecPhone,@ExWeight,@ExCost,@Amount,@OrdQty,@SendWarehouse,@PayDate,@SendMessage,@CoID,@Creator,@Modifier,@ExID,@ShopID)";
                    count = CoreDBconn.Execute(sqlCommand,saleout,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    else
                    {
                        SID = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                    }
                    //更新库存资料
                    sqlCommand = "select * from orderitem where oid = " + oid + " and coid = " + CoID;
                    var item = CoreDBconn.Query<OrderItem>(sqlCommand).AsList();
                    foreach(var i in item)
                    {
                        sqlCommand = @"update inventory_sale set StockQty = StockQty - @Qty,LockQty = LockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate 
                                       where Skuautoid = @ID and coid = @Coid";
                        count = CoreDBconn.Execute(sqlCommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=CoID},TransCore);     
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        sqlCommand = @"update inventory set StockQty = StockQty - @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                        count = CoreDBconn.Execute(sqlCommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=ItCoid},TransCore);     
                        if(count < 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                        var saleoutitem = new SaleOutItemInsert();
                        saleoutitem.SID = SID;
                        saleoutitem.OID = i.OID;
                        saleoutitem.SoID = i.SoID;
                        saleoutitem.SkuAutoID = i.SkuAutoID;
                        saleoutitem.SkuID = i.SkuID;
                        saleoutitem.SkuName = i.SkuName;
                        saleoutitem.Norm = i.Norm;
                        saleoutitem.GoodsCode = i.GoodsCode;
                        saleoutitem.Qty = i.Qty;
                        saleoutitem.SalePrice = i.SalePrice;
                        saleoutitem.RealPrice = i.RealPrice;
                        saleoutitem.Amount = i.Amount;
                        saleoutitem.DiscountRate = i.DiscountRate;
                        saleoutitem.img = i.img;
                        saleoutitem.ShopSkuID = i.ShopSkuID;
                        saleoutitem.Weight = i.Weight;
                        saleoutitem.TotalWeight = i.TotalWeight;
                        saleoutitem.IsGift = i.IsGift;
                        saleoutitem.CoID = i.CoID;
                        saleoutitem.Creator = UserName;
                        saleoutitem.Modifier = UserName;
                        sqlCommand = @"INSERT INTO saleoutitem(SID,OID,SoID,SkuAutoID,SkuID,SkuName,Norm,GoodsCode,Qty,SalePrice,RealPrice,Amount,DiscountRate,img,
                                                            ShopSkuID,Weight,TotalWeight,IsGift,CoID,Creator,Modifier) 
                                                        VALUES(@SID,@OID,@SoID,@SkuAutoID,@SkuID,@SkuName,@Norm,@GoodsCode,@Qty,@SalePrice,@RealPrice,@Amount,@DiscountRate,@img,
                                                            @ShopSkuID,@Weight,@TotalWeight,@IsGift,@CoID,@Creator,@Modifier)";
                        count = CoreDBconn.Execute(sqlCommand,saleoutitem,TransCore);
                        if(count < 0)
                        {
                            result.s = -3002;
                            return result;
                        }
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    count =CoreDBconn.Execute(loginsert,logs,TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }   
                }
                res.ID = u[0].ID;
                res.Status = u[0].Status;
                res.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                res.ExID = u[0].ExID;
                res.Express = u[0].Express;
                if(!string.IsNullOrEmpty(u[0].ExID.ToString()))
                {
                    res.ExpNamePinyin = GetExpNamePinyin(CoID,u[0].ExID);
                }
                res.ExCode = u[0].ExCode;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///撤销发货
        ///</summary>
        public static DataResult CancleShip(int oid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var log = new LogInsert();
            var res = new DirectShipReturn();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlCommand = "select * from `order` where id = " + oid + " and coid = " + CoID;
                var u = CoreDBconn.Query<Order>(sqlCommand).AsList();
                if(u.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号参数异常";
                    return result;
                }
                if(u[0].Status != 4 )
                {
                    result.s = -1;
                    result.d = "已发货的订单才可以撤销发货!";
                    return result;
                }
                
                u[0].Status = 3;
                u[0].ExCode = "";
                u[0].Modifier = UserName;
                u[0].ModifyDate = DateTime.Now;
                sqlCommand = @"update `order` set Status=@Status,ExCode=@ExCode,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                count = CoreDBconn.Execute(sqlCommand,u[0],TransCore);     
                if(count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                log = new LogInsert();
                log.OID = u[0].ID;
                log.SoID = u[0].SoID;
                log.Type = 0;
                log.LogDate = DateTime.Now;
                log.UserName = UserName;
                log.Title = "撤销发货";
                log.CoID = CoID;
                logs.Add(log);
                var  ru = CoreComm.WarehouseHaddle.wareInfoByID(u[0].WarehouseID.ToString(),CoID.ToString()).d as dynamic;;
                var wa = ru.Lst as List<wareInfo>;
                int ItCoid = 0;
                if(wa[0].itcoid == 0)
                {
                    ItCoid = wa[0].coid;
                }
                else
                {
                    ItCoid = wa[0].itcoid;
                }
                //更新库存资料
                sqlCommand = "select * from orderitem where oid = " + oid + " and coid = " + CoID;
                var item = CoreDBconn.Query<OrderItem>(sqlCommand).AsList();
                foreach(var i in item)
                {
                    sqlCommand = @"update inventory_sale set PickQty=PickQty+@Qty,StockQty = StockQty + @Qty,LockQty = LockQty + @Qty,Modifier=@Modifier,
                                    ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=CoID},TransCore);     
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    sqlCommand = @"update inventory set PickQty=PickQty+@Qty,StockQty = StockQty + @Qty,Modifier=@Modifier,
                                    ModifyDate=@ModifyDate where Skuautoid = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommand,new{Qty=i.Qty,Modifier=UserName,ModifyDate=DateTime.Now,ID=i.SkuAutoID,Coid=ItCoid},TransCore);     
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }
                //投递资料处理
                sqlCommand = @"update saleout set Status=0,Modifier=@Modifier,ModifyDate=@ModifyDate where oid = @OID  and coid = @Coid and status in (1,2,3)";
                count = CoreDBconn.Execute(sqlCommand,new{Modifier=UserName,ModifyDate=DateTime.Now,OID=oid,Coid=CoID},TransCore);     
                if(count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                            VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count =CoreDBconn.Execute(loginsert,logs,TransCore);
                if(count < 0)
                {
                    result.s = -3002;
                    return result;
                }  
                
                res.ID = u[0].ID;
                res.Status = u[0].Status;
                res.StatusDec = Enum.GetName(typeof(OrdStatus), u[0].Status);
                res.ExID = u[0].ExID;
                res.Express = u[0].Express;
                if(!string.IsNullOrEmpty(u[0].ExID.ToString()))
                {
                    res.ExpNamePinyin = GetExpNamePinyin(CoID,u[0].ExID);
                }
                res.ExCode = u[0].ExCode;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///新增赠品批次
        ///</summary>
        public static DataResult InsertGiftMulti(List<int> Oid,List<int> skuid,int CoID,string Username)
        {
            var result = new DataResult(1,null);  
            var res = new ModifySkuReturn();
            var fa = new List<TransferNormalReturnFail>();
            var ss = new List<int>();
            var logs = new List<LogInsert>();
            var bu = GetConfig(CoID);
            var business = bu.d as Business;
            bool ischeckPaid = false;
            var item = new List<OrderItem>();
            string sqlCommandText = string.Empty;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                foreach(var id in Oid)
                {
                    string wheresql = "select status,soid,amount,PaidAmount,IsPaid from `order` where id =" + id + " and coid =" + CoID;
                    var u = CoreDBconn.Query<Order>(wheresql).AsList();
                    if (u.Count == 0)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = id;
                        ff.Reason = "此订单不存在!";
                        fa.Add(ff);
                        continue;
                    }
                    else
                    {
                        if (u[0].Status != 0 && u[0].Status != 1 && u[0].Status != 7)
                        {
                            var ff = new TransferNormalReturnFail();
                            ff.ID = id;
                            ff.Reason = "只有待付款/已付款待审核/异常的订单才可以添加赠品!";
                            fa.Add(ff);
                            continue;
                        }
                    }
                    ischeckPaid = u[0].IsPaid;
                    bool flag = false;
                    decimal weight = 0;
                    var rr = new List<int>();
                    foreach (int a in skuid)
                    {
                        string skusql = "select skuid,skuname,norm,img,goodscode,enable,saleprice,weight from coresku where id =" + a + " and coid =" + CoID;
                        var s = CoreDBconn.Query<SkuInsert>(skusql).AsList();
                        if (s.Count == 0)
                        {
                            continue;
                        }
                        if (s[0].enable == false)
                        {
                            continue;
                        }
                        weight = weight + decimal.Parse(s[0].weight);
                        int x = CoreDBconn.QueryFirst<int>("select count(id) from orderitem where oid = " + id + " and coid =" + CoID + " and skuautoid = " + a + " AND IsGift = true");
                        if(x == 0)
                        {
                            sqlCommandText = @"INSERT INTO orderitem(oid,soid,coid,skuautoid,skuid,skuname,norm,GoodsCode,qty,saleprice,img,weight,totalweight,IsGift,creator,modifier) 
                                            VALUES(@OID,@Soid,@Coid,@Skuautoid,@Skuid,@Skuname,@Norm,@GoodsCode,@Qty,@Saleprice,@Img,@Weight,@Weight,@IsGift,@Creator,@Creator)";
                            var args = new
                            {
                                OID = id,
                                Soid = u[0].SoID,
                                Skuautoid = a,
                                Skuid = s[0].skuid,
                                Skuname = s[0].skuname,
                                Norm = s[0].norm,
                                GoodsCode = s[0].goodscode,
                                Qty = 1,
                                Saleprice = s[0].saleprice,
                                Img = s[0].img,
                                Weight = s[0].weight,
                                Coid = CoID,
                                Creator = Username,
                                IsGift = true
                            };
                            int j = CoreDBconn.Execute(sqlCommandText, args, TransCore);
                            if (j < 0)
                            {
                                var ff = new TransferNormalReturnFail();
                                ff.ID = id;
                                ff.Reason = "新增明细失败!";
                                fa.Add(ff);
                                flag = true;
                                break;
                            }
                        }
                        else
                        {
                            sqlCommandText = @"update orderitem set qty = qty + 1,totalweight = weight * qty,modifier=@Modifier,modifydate = @ModifyDate 
                                            where oid = @ID and coid = @Coid and skuautoid = @Skuautoid and IsGift = true";
                            var args = new
                            {
                                ID = id,
                                Skuautoid = a,
                                Coid = CoID,
                                Modifier = Username,
                                ModifyDate = DateTime.Now
                            };
                            int j = CoreDBconn.Execute(sqlCommandText, args, TransCore);
                            if (j < 0)
                            {
                                var ff = new TransferNormalReturnFail();
                                ff.ID = id;
                                ff.Reason = "更新明细失败!";
                                fa.Add(ff);
                                flag = true;
                                break;
                            }
                        }
                        if(business.isskulock == 0 && ischeckPaid == true)
                        {
                            sqlCommandText = @"update inventory_sale set LockQty = LockQty + 1,Modifier=@Modifier,ModifyDate=@ModifyDate where Skuautoid = @ID and CoID = @CoID";
                            int j =CoreDBconn.Execute(sqlCommandText,new{Modifier=Username,ModifyDate=DateTime.Now,ID = a,CoID = CoID},TransCore);
                            if(j < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                        rr.Add(a);
                        var log = new LogInsert();
                        log.OID = id;
                        log.SoID = u[0].SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = Username;
                        log.Title = "添加赠品";
                        log.Remark = s[0].skuid;
                        log.CoID = CoID;
                        logs.Add(log);     
                    }    
                    if(flag == true ) continue;       
                    if(rr.Count == 0)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = id;
                        ff.Reason = "无符合条件的商品,请检查选择的商品是否存在或已经停用!";
                        fa.Add(ff);
                        continue;
                    }
                    //更新订单的数量和重量
                    sqlCommandText = @"update `order` set ExWeight = ExWeight + @ExWeight,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    int count = CoreDBconn.Execute(sqlCommandText, new { ExWeight = weight,Modifier = Username, ModifyDate = DateTime.Now, ID = id, CoID = CoID }, TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    int r = CoreDBconn.Execute(loginsert,logs, TransCore);
                    if (r < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    ss.Add(id);
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            res.FailIDs = fa;
            if(ss.Count > 0)
            {
                var rr = GetOrderListSingle(ss,CoID);
                var dd = rr.d as OrderData;
                res.SuccessIDs = dd.Ord;
            }
            result.d = res;
            result.d = res;
            return result;
        }
        ///<summary>
        ///标识自定义异常
        ///</summary>
        public static DataResult MarkCustomAbnormal(List<int> status,DateTime OrdDateStart,DateTime OrdDateEnd,string GoodsCode,string SkuID,string SkuName,
                                                    string Norm,string RecMessage,string SendMessage,string Abnormal,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                var abList = new List<int>();
                int reasonid = GetReasonID("特殊单",CoID,7).s;
                if(reasonid != -1)
                {
                    abList.Add(reasonid);
                }
                reasonid = GetReasonID("用户已申请退款",CoID,7).s;
                if(reasonid != -1)
                {
                    abList.Add(reasonid);
                }
                reasonid = GetReasonID("等待订单合并",CoID,7).s;
                if(reasonid != -1)
                {
                    abList.Add(reasonid);
                }
                reasonid = GetReasonID("线上锁定",CoID,7).s;
                if(reasonid != -1)
                {
                    abList.Add(reasonid);
                }
                var skuid = new List<int>();
                if(!string.IsNullOrEmpty(SkuID))
                {
                    string[] sku = SkuID.Split(',');
                    foreach(var a in sku)
                    {
                        var dd = new List<int>();
                        sqlCommandText = "select id from coresku where SkuID = '" + a + "'";
                        dd =  CoreDBconn.Query<int>(sqlCommandText).AsList();
                        foreach(var d in dd)
                        {
                            skuid.Add(d);
                        }
                    }
                }
                else
                {
                    if(!string.IsNullOrEmpty(GoodsCode))
                    {
                        string[] sku = GoodsCode.Split(',');
                        foreach(var a in sku)
                        {
                            var dd = new List<int>();
                            sqlCommandText = "select id from coresku where GoodsCode = '" + a + "'";
                            dd =  CoreDBconn.Query<int>(sqlCommandText).AsList();
                            foreach(var d in dd)
                            {
                                skuid.Add(d);
                            }
                        }
                    }
                }
                //检查自定义异常是否已经存在，若不存在，需先新增
                reasonid = GetReasonID(Abnormal,CoID,7).s;
                if(reasonid == -1)
                {
                    sqlCommandText = @"INSERT INTO orderabnormal(Name,IsCustom,CoID,Creator,OrdStatus) 
                                                          VALUES(@Name,@IsCustom,@CoID,@Creator,@OrdStatus)";
                    count = CoreDBconn.Execute(sqlCommandText,new{Name=Abnormal,IsCustom=true,CoID=CoID,Creator=UserName,OrdStatus=7},TransCore);
                    if(count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    else
                    {
                        reasonid = CoreDBconn.QueryFirst<int>("select LAST_INSERT_ID()");
                    }
                    
                }
                sqlCommandText = @"select ID,SoID from `order` where coid = " + CoID + " AND status in (" + string.Join(",", status) + ") AND abnormalstatus not in (" + 
                                   string.Join(",", abList) + ") AND odate > '" + OrdDateStart + "' and odate < '" + OrdDateEnd + "'" ;
                if(skuid.Count > 0)
                {
                    sqlCommandText = sqlCommandText + " and exists(select id from orderitem where oid = order.id and skuautoid in (" + string.Join(",", skuid) + ")";
                }
                if(!string.IsNullOrEmpty(SkuName))
                {
                    sqlCommandText = sqlCommandText + " and exists(select id from orderitem where oid = order.id and SkuName like '%" + SkuID + "%')";
                }
                if(!string.IsNullOrEmpty(Norm))
                {
                    sqlCommandText = sqlCommandText + " and exists(select id from orderitem where oid = order.id and Norm like '%" + SkuID + "%')";
                }
                if(!string.IsNullOrEmpty(RecMessage))
                {
                    sqlCommandText = sqlCommandText + " and RecMessage like '%" + RecMessage + "%')";
                }
                if(!string.IsNullOrEmpty(SendMessage))
                {
                    sqlCommandText = sqlCommandText + " and SendMessage like '%" + SendMessage + "%')";
                }
                var ord = CoreDBconn.Query<Order>(sqlCommandText).AsList();
                var oid = new List<int>();
                foreach(var a in ord)
                {
                    oid.Add(a.ID);
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "标记异常";
                    log.Remark = Abnormal;
                    log.CoID = CoID;
                    logs.Add(log);     
                }
                sqlCommandText = @"update `order` set Status = @Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,
                                   ModifyDate=@ModifyDate where id in @ID and coid = @Coid";
                count = CoreDBconn.Execute(sqlCommandText,new{Status = 7,AbnormalStatus = reasonid,StatusDec=Abnormal,Modifier=UserName,
                                           ModifyDate=DateTime.Now,ID=oid,Coid=CoID},TransCore);
                if(count < 0)
                {
                    result.s = -3003;
                    return result;
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///普通订单与天猫分销订单相互转换
        ///</summary>
        public static DataResult ComDisExchange(List<int> oid,int CoID,string Username)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var res = new ComDisExchangeReturn();
            var su = new List<ComDisExchangeSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                sqlCommandText = "select id,soid,type,status,coid from `order` where id in @ID and coid = @Coid";
                var ord = CoreDBconn.Query<Order>(sqlCommandText,new{ID = oid,Coid=CoID}).AsList();
                if(ord.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号无效!";
                    return result;
                }
                foreach(var a in ord)
                {
                    if(a.Type != 0 && a.Type != 3)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有未发货的普通订单&天猫分销订单才可以互相转换!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Status == 3 || a.Status == 4 || a.Status == 5 || a.Status == 6)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有未发货的普通订单&天猫分销订单才可以互相转换!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Type == 0)
                    {
                        a.Type = 3; 
                        if(a.Status == 1 || a.Status == 2)
                        {
                            a.Status = 8;
                        }
                        var log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = Username;
                        log.Title = "转订单类型";
                        log.Remark = "普通订单=>天猫分销";
                        log.CoID = CoID;
                        logs.Add(log);     
                    }
                    else
                    {
                        a.Type = 0; 
                        if(a.Status == 8)
                        {
                            a.Status = 1;
                        }
                        var log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = Username;
                        log.Title = "转订单类型";
                        log.Remark = "天猫分销=>普通订单";
                        log.CoID = CoID;
                        logs.Add(log);    
                    }
                    a.Modifier = Username;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = "update `order` set Type=@Type,Status=@Status,Modifier=@Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if(count < 0)
                    {
                        result.s = -1;
                        return result;
                    }
                    var ss = new ComDisExchangeSuccess();
                    ss.ID = a.ID;
                    ss.Type = a.Type;
                    ss.TypeString = GetTypeName(a.Type);
                    ss.Status = a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);
                    su.Add(ss);
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///转成分销+属性订单
        ///</summary>
        public static DataResult SetOrdType(List<int> oid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var res = new SetOrdTypeReturn();
            var su = new List<SetOrdTypeSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                sqlCommandText = "select id,soid,type,status,coid,SupDistributor,AbnormalStatus,StatusDec from `order` where id in @ID and coid = @Coid";
                var ord = CoreDBconn.Query<Order>(sqlCommandText,new{ID = oid,Coid=CoID}).AsList();
                if(ord.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号无效!";
                    return result;
                }
                foreach(var a in ord)
                {
                    if(a.Type == 3)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "天猫分销的订单不可操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Status != 0 && a.Status !=1  && a.Status !=7)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有异常/待付款/已付款待审核的订单才可以操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Type == 6 || a.Type == 7 || a.Type == 8 || a.Type == 9 || a.Type == 10 || a.Type == 16 || a.Type == 17 || a.Type == 18 || a.Type == 19 || a.Type == 20)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "订单已经是分销属性,不可操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Type == 0)
                    {
                        a.Type = 6; 
                    }
                    if(a.Type == 1)
                    {
                        a.Type = 7; 
                    }
                    if(a.Type == 2)
                    {
                        a.Type = 8; 
                    }
                    if(a.Type == 4)
                    {
                        a.Type = 9; 
                    }
                    if(a.Type == 5)
                    {
                        a.Type = 10; 
                    }
                    if(a.Type == 11)
                    {
                        a.Type = 16; 
                    }
                    if(a.Type == 12)
                    {
                        a.Type = 17; 
                    }
                    if(a.Type == 13)
                    {
                        a.Type = 18; 
                    }
                    if(a.Type == 14)
                    {
                        a.Type = 19; 
                    }
                    if(a.Type == 15)
                    {
                        a.Type = 20; 
                    }
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "转成分销 + 订单";
                    log.CoID = CoID;
                    logs.Add(log);     
                    if(string.IsNullOrEmpty(a.SupDistributor))
                    {
                        int reasonid = GetReasonID("不明确分销",CoID,7).s;
                        if(reasonid == -1)
                        {
                            result.s = -1;
                            result.d = "请先设定【不明确分销】的异常";
                            return result;
                        }
                        a.Status = 7;
                        a.AbnormalStatus = reasonid;
                        a.StatusDec="不明确分销";
                        log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "标记异常";
                        log.Remark = "不明确分销(转换订单类型时判断:不能通过分销规则自动判断供销商)";
                        log.CoID = CoID;
                        logs.Add(log);     
                    }
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set Type=@Type,Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,
                                       ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if(count < 0)
                    {
                        result.s = -1;
                        return result;
                    }
                    var ss = new SetOrdTypeSuccess();
                    ss.ID = a.ID;
                    ss.Type = a.Type;
                    ss.TypeString = GetTypeName(a.Type);
                    ss.Status = a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);
                    ss.AbnormalStatus = a.AbnormalStatus;
                    ss.AbnormalStatusDec = a.StatusDec;
                    su.Add(ss);
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///取消分销+属性订单
        ///</summary>
        public static DataResult CancleSetOrdType(List<int> oid,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var res = new SetOrdTypeReturn();
            var su = new List<SetOrdTypeSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            string sqlCommandText = string.Empty;
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                sqlCommandText = "select id,soid,type,status,coid,SupDistributor,AbnormalStatus,StatusDec from `order` where id in @ID and coid = @Coid";
                var ord = CoreDBconn.Query<Order>(sqlCommandText,new{ID = oid,Coid=CoID}).AsList();
                if(ord.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号无效!";
                    return result;
                }
                foreach(var a in ord)
                {
                    if(a.Type == 3)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "天猫分销的订单不可操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Status != 0 && a.Status !=1  && a.Status !=7)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有异常/待付款/已付款待审核的订单才可以操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Type == 0 || a.Type == 1 || a.Type == 2 || a.Type == 4 || a.Type == 5 || a.Type == 11 || a.Type == 12 || a.Type == 13 || a.Type == 14 || a.Type == 15)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "订单不是分销属性,不可操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Type == 6)
                    {
                        a.Type = 0; 
                    }
                    if(a.Type == 7)
                    {
                        a.Type = 1; 
                    }
                    if(a.Type == 8)
                    {
                        a.Type = 2; 
                    }
                    if(a.Type == 9)
                    {
                        a.Type = 4; 
                    }
                    if(a.Type == 10)
                    {
                        a.Type = 5; 
                    }
                    if(a.Type == 16)
                    {
                        a.Type = 11; 
                    }
                    if(a.Type == 17)
                    {
                        a.Type = 12; 
                    }
                    if(a.Type == 18)
                    {
                        a.Type = 13; 
                    }
                    if(a.Type == 19)
                    {
                        a.Type = 14; 
                    }
                    if(a.Type == 20)
                    {
                        a.Type = 15; 
                    }
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "取消订单分销 + 属性";
                    log.CoID = CoID;
                    logs.Add(log);     
                    if(a.Status == 7 && a.StatusDec == "不明确分销")
                    {
                        if(a.Amount == a.PaidAmount)
                        {
                            a.Status = 1;
                        }
                        else
                        {
                            a.Status = 0;
                        }
                        a.AbnormalStatus = 0;
                        a.StatusDec="";
                        log = new LogInsert();
                        log.OID = a.ID;
                        log.SoID = a.SoID;
                        log.Type = 0;
                        log.LogDate = DateTime.Now;
                        log.UserName = UserName;
                        log.Title = "取消异常标记";
                        log.Remark = "不明确分销";
                        log.CoID = CoID;
                        logs.Add(log);     
                    }
                    a.SupDistributor = null;
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set Type=@Type,Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,
                                       ModifyDate=@ModifyDate,SupDistributor=@SupDistributor where id = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if(count < 0)
                    {
                        result.s = -1;
                        return result;
                    }
                    var ss = new SetOrdTypeSuccess();
                    ss.ID = a.ID;
                    ss.Type = a.Type;
                    ss.TypeString = GetTypeName(a.Type);
                    ss.Status = a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);
                    ss.AbnormalStatus = a.AbnormalStatus;
                    ss.AbnormalStatusDec = a.StatusDec;
                    su.Add(ss);
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///获取供销商List
        ///</summary>
        public static DataResult GetSupDistributor(int CoID)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{  
                    //分销商
                    string sqlcommand = "select ID as value,DistributorName as label from distributor where coid =" + CoID + " and enable = true and type = 1";
                    var u = conn.Query<Filter>(sqlcommand).AsList();
                    result.d = u;                    
                    }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }  
            return result;
        }
        ///<summary>
        ///指定供销商
        ///</summary>
        public static DataResult SetSupDistributor(List<int> oid,int sd,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var logs = new List<LogInsert>();
            var res = new SetSupDistributorReturn();
            var su = new List<SetSupDistributorSuccess>();
            var fa = new List<TransferNormalReturnFail>();
            int count = 0;
            var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
            CoreDBconn.Open();
            var TransCore = CoreDBconn.BeginTransaction();
            try
            {
                string sqlCommandText = "select ID as value,DistributorName as label from distributor where coid =" + CoID + " and enable = true and type = 1 and ID = " + sd;
                var SupDistributor = CoreDBconn.Query<Filter>(sqlCommandText).AsList();
                if(SupDistributor.Count == 0)
                {
                    result.s = -1;
                    result.d = "供销商ID无效!";
                    return result;
                }
                string SupDistributorName = SupDistributor[0].label;
                sqlCommandText = "select id,soid,type,status,coid,SupDistributor,AbnormalStatus,StatusDec from `order` where id in @ID and coid = @Coid";
                var ord = CoreDBconn.Query<Order>(sqlCommandText,new{ID = oid,Coid=CoID}).AsList();
                if(ord.Count == 0)
                {
                    result.s = -1;
                    result.d = "订单单号无效!";
                    return result;
                }
                foreach(var a in ord)
                {
                    if(a.Type == 3)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "天猫分销的订单不可操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Status != 0 && a.Status !=1  && a.Status !=7)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "只有异常/待付款/已付款待审核的订单才可以操作!";
                        fa.Add(ff);
                        continue;
                    }
                    if(a.Type == 0 || a.Type == 1 || a.Type == 2 || a.Type == 4 || a.Type == 5 || a.Type == 11 || a.Type == 12 || a.Type == 13 || a.Type == 14 || a.Type == 15)
                    {
                        var ff = new TransferNormalReturnFail();
                        ff.ID = a.ID;
                        ff.Reason = "订单不是分销属性,不可操作!";
                        fa.Add(ff);
                        continue;
                    }
                    a.SupDistributor = SupDistributorName;
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "强制指定供销商";
                    log.Remark = "取消不明确分销";
                    log.CoID = CoID;
                    logs.Add(log);     
                    if(a.Status == 7 && a.StatusDec == "不明确分销")
                    {
                        if(a.Amount == a.PaidAmount)
                        {
                            a.Status = 1;
                        }
                        else
                        {
                            a.Status = 0;
                        }
                        a.AbnormalStatus = 0;
                        a.StatusDec="";
                    }
                    a.Modifier = UserName;
                    a.ModifyDate = DateTime.Now;
                    sqlCommandText = @"update `order` set SupDistributor=@SupDistributor,Status=@Status,AbnormalStatus=@AbnormalStatus,StatusDec=@StatusDec,Modifier=@Modifier,
                                       ModifyDate=@ModifyDate where id = @ID and coid = @Coid";
                    count = CoreDBconn.Execute(sqlCommandText,a,TransCore);
                    if(count < 0)
                    {
                        result.s = -1;
                        return result;
                    }
                    var ss = new SetSupDistributorSuccess();
                    ss.ID = a.ID;
                    ss.SupDistributor = a.SupDistributor;
                    ss.Status = a.Status;
                    ss.StatusDec = Enum.GetName(typeof(OrdStatus), a.Status);
                    ss.AbnormalStatus = a.AbnormalStatus;
                    ss.AbnormalStatusDec = a.StatusDec;
                    su.Add(ss);
                }
                string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                count = CoreDBconn.Execute(loginsert,logs, TransCore);
                if (count < 0)
                {
                    result.s = -3002;
                    return result;
                }
                res.SuccessIDs = su;
                res.FailIDs = fa;
                result.d = res;
                TransCore.Commit();
            }
            catch (Exception e)
            {
                TransCore.Rollback();
                TransCore.Dispose();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                TransCore.Dispose();
                CoreDBconn.Dispose();
            }
            return result;
        }
        ///<summary>
        ///重新计算赠品
        ///</summary>
        public static DataResult CalGift(string OidType,List<int> oid,string DateType,bool IsSplit,bool IsDelGift,bool IsDelPrice0,List<OrderQuery> ord,int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            string sqlCommandText = string.Empty;
            var gift = new List<GiftRule>();
            var order = new List<Order>();
            int count = 0;
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{   
                    sqlCommandText = "select * from `order` where id in @ID and coid = @Coid";
                    if(DateType == "A")
                    {
                        sqlCommandText = sqlCommandText + " order by ODate Asc";
                    } 
                    else if(DateType == "B")
                    {
                        sqlCommandText = sqlCommandText + " order by PayDate Asc";
                    } 
                    if(OidType == "B")
                    {
                        var id = new List<int>();
                        foreach(var a in ord)
                        {
                            id.Add(a.ID);
                        }
                        oid = id;
                    }
                    order = conn.Query<Order>(sqlCommandText,new{ID = oid,Coid = CoID}).AsList();
                    if(order.Count == 0)
                    {
                        result.s = -1;
                        result.d = "没有符合条件的资料";
                        return result;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            } 
            foreach(var a in order)
            {
                gift = new List<GiftRule>();
                var CoreDBconn = new MySqlConnection(DbBase.CoreConnectString);
                CoreDBconn.Open();
                var TransCore = CoreDBconn.BeginTransaction();
                try
                {
                    int i = 0;
                    if(a.Status != 0 && a.Status != 1 && a.Status != 2 && a.Status != 7) continue;
                    if(IsSplit == true && a.IsSplit == true) continue;
                    if(IsDelGift == true)
                    {
                        sqlCommandText = "Delete from orderitem where oid = "+ a.ID + " and coid = " + CoID + " and IsGift = true";
                        count = CoreDBconn.Execute(sqlCommandText,TransCore);
                        if(count < 0)
                        {
                            result.s = -3004;
                            return result;
                        }
                        else if(count > 0)
                        {
                            i ++;
                        }
                    }
                    if(IsDelPrice0 == true)
                    {
                        sqlCommandText = "Delete from orderitem where oid = "+ a.ID + " and coid = " + CoID + " and RealPrice = 0";
                        count = CoreDBconn.Execute(sqlCommandText,TransCore);
                        if(count < 0)
                        {
                            result.s = -3004;
                            return result;
                        }
                        else if(count > 0)
                        {
                            i ++;
                        }
                    }
                    sqlCommandText = "select * from orderitem where oid = "+ a.ID + " and coid = " + CoID;
                    var item = CoreDBconn.Query<OrderItem>(sqlCommandText).AsList();
                    var res = GiftHaddle.SetGiftItem(a,item,CoID);
                    if(res.s == 1)
                    {
                        var rejj = res.d as GiftInsertReturn; 
                        gift = rejj.Gift;
                        var giftNo = rejj.GiftInsert as List<GiftInsertOrder>; 
                        foreach(var no in giftNo)
                        {
                            int j = 0;
                            foreach(var it in item)
                            {
                                if(it.IsGift == false) continue;
                                if(it.SkuAutoID == no.SkuAutoID && no.IsGift == true)
                                {
                                    sqlCommandText = @"update orderitem set qty = qty + @Qty,totalweight = weight * qty,modifier=@Modifier,modifydate = @ModifyDate 
                                                       where id = @ID and coid = @Coid";
                                    var args = new {ID = it.ID,Qty = no.Qty,Coid = CoID,Modifier = UserName,ModifyDate = DateTime.Now};
                                    count = CoreDBconn.Execute(sqlCommandText, args, TransCore);
                                    if(count < 0)
                                    {
                                        result.s = -3003;
                                        return result;
                                    }
                                    j ++ ;
                                    break;
                                }
                            }
                            if(j > 0) continue;
                            string skusql = "select skuid,skuname,norm,img,goodscode,enable,saleprice,weight from coresku where id =" + no.SkuAutoID + " and coid =" + CoID;
                            var s = CoreDBconn.Query<SkuInsert>(skusql).AsList();
                            if (s.Count == 0)
                            {
                                continue;
                            }
                            if (s[0].enable == false)
                            {
                                continue;
                            }
                            sqlCommandText = @"INSERT INTO orderitem(oid,soid,coid,skuautoid,skuid,skuname,norm,GoodsCode,qty,saleprice,img,weight,totalweight,IsGift,creator,modifier) 
                                            VALUES(@OID,@Soid,@Coid,@Skuautoid,@Skuid,@Skuname,@Norm,@GoodsCode,@Qty,@Saleprice,@Img,@Weight,@Totalweight,@IsGift,@Creator,@Creator)";
                            var argss = new
                            {
                                OID = a.ID,
                                Soid = a.SoID,
                                Skuautoid = no.SkuAutoID,
                                Skuid = s[0].skuid,
                                Skuname = s[0].skuname,
                                Norm = s[0].norm,
                                GoodsCode = s[0].goodscode,
                                Qty = no.Qty,
                                Saleprice = s[0].saleprice,
                                Img = s[0].img,
                                Weight = s[0].weight,
                                Totalweight = decimal.Parse(s[0].weight)*no.Qty,
                                Coid = CoID,
                                Creator = UserName,
                                IsGift = no.IsGift
                            };
                            count = CoreDBconn.Execute(sqlCommandText, argss, TransCore);
                            if (count < 0)
                            {
                                result.s = -3002;
                                return result;
                            }
                            i++;
                        }
                    }
                    if(i == 0) continue;
                    sqlCommandText = "select sum(Qty) as QtyTot,sum(Amount) as AmtTot,sum(TotalWeight) as WeightTot from orderitem where oid = " + a.ID + " and coid = " + CoID + " and isgift = false";
                    var su = CoreDBconn.Query<OrdSum>(sqlCommandText).AsList();
                    sqlCommandText = "select sum(Qty) as QtyTot,sum(Amount) as AmtTot,sum(TotalWeight) as WeightTot from orderitem where oid = " + a.ID + " and coid = " + CoID + " and isgift = true";
                    var su2 = CoreDBconn.Query<OrdSum>(sqlCommandText).AsList();
                    //更新订单的数量和重量
                    sqlCommandText = @"update `order` set ExWeight = @ExWeight,OrdQty = @Qty,Modifier=@Modifier,ModifyDate=@ModifyDate where ID = @ID and CoID = @CoID";
                    count = CoreDBconn.Execute(sqlCommandText, new { ExWeight = su[0].WeightTot + su2[0].WeightTot,Qty = su[0].QtyTot,Modifier = UserName, ModifyDate = DateTime.Now, ID = a.ID, CoID = CoID }, TransCore);
                    if (count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                    var log = new LogInsert();
                    log.OID = a.ID;
                    log.SoID = a.SoID;
                    log.Type = 0;
                    log.LogDate = DateTime.Now;
                    log.UserName = UserName;
                    log.Title = "重新计算赠品";
                    log.CoID = CoID;
                    string loginsert = @"INSERT INTO orderlog(OID,SoID,Type,LogDate,UserName,Title,Remark,CoID) 
                                                VALUES(@OID,@SoID,@Type,@LogDate,@UserName,@Title,@Remark,@CoID)";
                    count = CoreDBconn.Execute(loginsert,log, TransCore);
                    if (count < 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    TransCore.Commit();
                }
                catch (Exception e)
                {
                    TransCore.Rollback();
                    TransCore.Dispose();
                    result.s = -1;
                    result.d = e.Message;
                }
                finally
                {
                    TransCore.Dispose();
                    CoreDBconn.Dispose();
                }
                if(gift.Count > 0)
                {
                    var CommDBconn = new MySqlConnection(DbBase.CommConnectString);
                    CommDBconn.Open();
                    var TransComm = CommDBconn.BeginTransaction();
                    try
                    {
                        foreach(var g in gift)
                        {
                            string sql = "update gift set GivenQty=@GivenQty where id=@ID and coid=@Coid";
                            count = CommDBconn.Execute(sql,a,TransComm);
                            if(count < 0)
                            {
                                result.s = -3003;
                                return result;
                            }
                        }
                        TransComm.Commit();
                    }
                    catch (Exception e)
                    {
                        TransComm.Rollback();
                        TransComm.Dispose();
                        result.s = -1;
                        result.d = e.Message;
                    }
                    finally
                    {
                        TransComm.Dispose();
                        CommDBconn.Dispose();
                    }
                }
            }
            return result;
        }
        ///<summary>
        ///订单类型List
        ///</summary>
        public static DataResult GetOrdType()                
        {
            var result = new DataResult(1,null);
            //订单类型
            var oo = new List<Filter>();
            var o = new Filter();
            o.value = "0";
            o.label = "普通订单";
            oo.Add(o);
            o = new Filter();
            o.value = "1";
            o.label = "补发订单";
            oo.Add(o);
            o = new Filter();
            o.value = "2";
            o.label = "换货订单";
            oo.Add(o);
            o = new Filter();
            o.value = "3";
            o.label = "天猫分销";
            oo.Add(o);
            o = new Filter();
            o.value = "4";
            o.label = "天猫供销";
            oo.Add(o);
            o = new Filter();
            o.value = "5";
            o.label = "协同订单";
            oo.Add(o);
            o = new Filter();
            o.value = "6";
            o.label = "普通订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "7";
            o.label = "补发订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "8";
            o.label = "换货订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "9";
            o.label = "天猫供销,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "10";
            o.label = "协同订单,分销+";
            oo.Add(o);
            o = new Filter();
            o.value = "11";
            o.label = "普通订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "12";
            o.label = "补发订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "13";
            o.label = "换货订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "14";
            o.label = "天猫供销,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "15";
            o.label = "协同订单,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "16";
            o.label = "普通订单,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "17";
            o.label = "补发订单,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "18";
            o.label = "换货订单,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "19";
            o.label = "天猫供销,分销+,供销+";
            oo.Add(o);
            o = new Filter();
            o.value = "20";
            o.label = "协同订单,分销+,供销+";
            oo.Add(o);
            result.d = oo;
            return result;
        }
        ///<summary>
        ///新增自动审单规则
        ///</summary>
        public static DataResult InsertAutoConfirmRule(OrdAutoConfirmRule rule)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string sqlcommand = @"INSERT INTO order_autoconfirm_rule(RuleName,StartDate,EndDate,PayStartDate,PayEndDate,AppointSku,ExcludeSku,MinAmt,MaxAmt,IgnoreRec,
                                                                             RecMessage,IgnoreSend,SendMessage,DiscountRate,Shop,OrdType,DelayedMinute,CoID,Creator,Modifier) 
                                                            VALUES(@RuleName,@StartDate,@EndDate,@PayStartDate,@PayEndDate,@AppointSku,@ExcludeSku,@MinAmt,@MaxAmt,@IgnoreRec,
                                                                   @RecMessage,@IgnoreSend,@SendMessage,@DiscountRate,@Shop,@OrdType,@DelayedMinute,@CoID,@Creator,@Modifier)";     
                    int count = conn.Execute(sqlcommand,rule);
                    if(count <= 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    int rtn = conn.QueryFirst<int>("select LAST_INSERT_ID()");
                    result.d = rtn;
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            return result;
        }
        ///<summary>
        ///查询单笔自动审单规则
        ///</summary>
        public static DataResult GetAutoConfirmRuleSingle(int CoID,int id)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string sqlcommand = @"select ID,RuleName,StartDate,EndDate,PayStartDate,PayEndDate,AppointSku,ExcludeSku,MinAmt,MaxAmt,IgnoreRec,RecMessage,IgnoreSend,
                                          SendMessage,DiscountRate,Shop,OrdType,DelayedMinute from order_autoconfirm_rule where id = " + id + " and coid = " + CoID;     
                    var u = conn.Query<OrdAutoConfirmRuleSingle>(sqlcommand).AsList();
                    if(u.Count > 0)
                    {
                        result.d = u[0];
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            return result;
        }
        ///<summary>
        ///更新自动审单规则
        ///</summary>
        public static DataResult UpdateAutoConfirmRule(int CoID,int id,string RuleName,string StartDate,string EndDate,string PayStartDate,string PayEndDate,string AppointSku,
                                                       string ExcludeSku,string MinAmt,string MaxAmt,string IgnoreRec,string RecMessage,string IgnoreSend,string SendMessage,
                                                       string DiscountRate,string Shop,string OrdType,string DelayedMinute,string UserName)
        {
            var p = new DynamicParameters();
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string sqlcommand = "update order_autoconfirm_rule set ";
                    int i = 0;
                    if(RuleName != null)
                    {
                        sqlcommand = sqlcommand + "RuleName = @RuleName,";
                        p.Add("@RuleName",RuleName);
                        i ++;
                    }
                    if(StartDate != null)
                    {
                        sqlcommand = sqlcommand + "StartDate = @StartDate,";
                        p.Add("@StartDate",StartDate);
                        i ++;
                    }
                    if(EndDate != null)
                    {
                        sqlcommand = sqlcommand + "EndDate = @EndDate,";
                        p.Add("@EndDate",EndDate);
                        i ++;
                    }
                    if(PayStartDate != null)
                    {
                        sqlcommand = sqlcommand + "PayStartDate = @PayStartDate,";
                        p.Add("@PayStartDate",PayStartDate);
                        i ++;
                    }
                    if(PayEndDate != null)
                    {
                        sqlcommand = sqlcommand + "PayEndDate = @PayEndDate,";
                        p.Add("@PayEndDate",PayEndDate);
                        i ++;
                    }
                    if(AppointSku != null)
                    {
                        sqlcommand = sqlcommand + "AppointSku = @AppointSku,";
                        p.Add("@AppointSku",AppointSku);
                        i ++;
                    }
                    if(ExcludeSku != null)
                    {
                        sqlcommand = sqlcommand + "ExcludeSku = @ExcludeSku,";
                        p.Add("@ExcludeSku",ExcludeSku);
                        i ++;
                    }
                    if(MinAmt != null)
                    {
                        sqlcommand = sqlcommand + "MinAmt = @MinAmt,";
                        p.Add("@MinAmt",MinAmt);
                        i ++;
                    }
                    if(MaxAmt != null)
                    {
                        sqlcommand = sqlcommand + "MaxAmt = @MaxAmt,";
                        p.Add("@MaxAmt",MaxAmt);
                        i ++;
                    }
                    if(IgnoreRec != null)
                    {
                        sqlcommand = sqlcommand + "IgnoreRec = @IgnoreRec,";
                        p.Add("@IgnoreRec",IgnoreRec);
                        i ++;
                    }
                    if(RecMessage != null)
                    {
                        sqlcommand = sqlcommand + "RecMessage = @RecMessage,";
                        p.Add("@RecMessage",RecMessage);
                        i ++;
                    }
                    if(IgnoreSend != null)
                    {
                        sqlcommand = sqlcommand + "IgnoreSend = @IgnoreSend,";
                        p.Add("@IgnoreSend",IgnoreSend);
                        i ++;
                    }
                    if(SendMessage != null)
                    {
                        sqlcommand = sqlcommand + "SendMessage = @SendMessage,";
                        p.Add("@SendMessage",SendMessage);
                        i ++;
                    }
                    if(DiscountRate != null)
                    {
                        sqlcommand = sqlcommand + "DiscountRate = @DiscountRate,";
                        p.Add("@DiscountRate",DiscountRate);
                        i ++;
                    }
                    if(Shop != null)
                    {
                        sqlcommand = sqlcommand + "Shop = @Shop,";
                        p.Add("@Shop",Shop);
                        i ++;
                    }
                    if(OrdType != null)
                    {
                        sqlcommand = sqlcommand + "OrdType = @OrdType,";
                        p.Add("@OrdType",OrdType);
                        i ++;
                    }
                    if(DelayedMinute != null)
                    {
                        sqlcommand = sqlcommand + "DelayedMinute = @DelayedMinute,";
                        p.Add("@DelayedMinute",DelayedMinute);
                        i ++;
                    }
                    if(i > 0)
                    {
                        sqlcommand = sqlcommand + "Modifier = @Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @CoID";
                        p.Add("@Modifier",UserName);
                        p.Add("@ModifyDate",DateTime.Now);
                        p.Add("@ID",id);
                        p.Add("@CoID",CoID);
                        int count = conn.Execute(sqlcommand,p);
                        if(count <= 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            return result;
        }
        ///<summary>
        ///禁用或启用规则
        ///</summary>
        public static DataResult UpdateAutoConfirmRuleEnable(int CoID,int id,bool Enable,string UserName)
        {
            var p = new DynamicParameters();
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string sqlcommand = "update order_autoconfirm_rule set enable = @Enable,Modifier = @Modifier,ModifyDate=@ModifyDate where id = @ID and coid = @CoID";
                    int count = conn.Execute(sqlcommand,new{Enable=Enable,Modifier=UserName,ModifyDate=DateTime.Now,ID =id,CoID=CoID});
                    if(count < 0)
                    {
                        result.s = -3003;
                        return result;
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            return result;
        }
        ///<summary>
        ///查询自动审单规则
        ///</summary>
        public static DataResult GetAutoConfirmRuleList(int CoID,string SortField,string SortDirection,int NumPerPage,int PageIndex)
        {
            var result = new DataResult(1,null);
            string sqlcount = "select count(id) from order_autoconfirm_rule";
            string sqlcommand = @"select ID,RuleName,AppointSku,ExcludeSku,MinAmt,MaxAmt,DiscountRate,IgnoreRec,StartDate,EndDate,DelayedMinute,Enable,CreateDate,ModifyDate 
                                  from order_autoconfirm_rule";
            string wheresql = string.Empty;
            if(!string.IsNullOrEmpty(SortField) && !string.IsNullOrEmpty(SortDirection))//排序
            {
                wheresql = wheresql + " ORDER BY "+SortField +" "+ SortDirection;
            }
            var res = new OrdAutoConfirmRuleData();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    int count = conn.QueryFirst<int>(sqlcount + wheresql);
                    decimal pagecnt = Math.Ceiling(decimal.Parse(count.ToString())/decimal.Parse(NumPerPage.ToString()));
                    int dataindex = (PageIndex - 1)* NumPerPage;
                    wheresql = wheresql + " limit " + dataindex.ToString() + " ," + NumPerPage.ToString();
                    var u = conn.Query<OrdAutoConfirmRuleList>(sqlcommand + wheresql).AsList();
                    res.Datacnt = count;
                    res.Pagecnt = pagecnt;
                    res.Rule = u;
                    result.d = res;             
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }           
            return result;
        }
        ///<summary>
        ///自动审核订单
        ///</summary>
        public static DataResult AutoConfirmOrd(int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var ID = new List<int>();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    DateTime today = DateTime.Now;
                    string sqlcommand = @"select * from order_autoconfirm_rule where enable = true and coid = " + CoID + " and '" + today + "' >= StartDate and '" + today + "' <= EndDate";
                    var u = conn.Query<OrdAutoConfirmRule>(sqlcommand).AsList();
                    foreach(var a in u)
                    {
                        sqlcommand = @"select ID from `order` where coid = " + CoID + " and status = 1";
                        if(!string.IsNullOrEmpty(a.PayStartDate) && DateTime.Parse(a.PayStartDate) > DateTime.Parse("1900-01-01"))
                        {
                            sqlcommand = sqlcommand + " and paydate >= '" + a.PayStartDate + "'";
                        }
                        if(!string.IsNullOrEmpty(a.PayEndDate) && DateTime.Parse(a.PayEndDate) > DateTime.Parse("1900-01-01"))
                        {
                            sqlcommand = sqlcommand + " and paydate <= '" + a.PayEndDate + "'";
                        }
                        if(a.IgnoreRec == true && !string.IsNullOrEmpty(a.RecMessage))
                        {
                            sqlcommand = sqlcommand + " and RecMessage not like '%" + a.RecMessage + "%'";
                        }
                        if(a.IgnoreRec == false)
                        {
                            sqlcommand = sqlcommand + " and (RecMessage = null or RecMessage = '')";
                        }
                        if(a.IgnoreSend == true && !string.IsNullOrEmpty(a.SendMessage))
                        {
                            sqlcommand = sqlcommand + " and SendMessage not like '%" + a.SendMessage + "%'";
                        }
                        if(a.IgnoreSend == false)
                        {
                            sqlcommand = sqlcommand + " and (SendMessage = null or SendMessage = '')";
                        }
                        if(!string.IsNullOrEmpty(a.Shop))
                        {
                            sqlcommand = sqlcommand + " and shopid in (" + a.Shop + ")";
                        }
                        if(!string.IsNullOrEmpty(a.OrdType))
                        {
                            sqlcommand = sqlcommand + " and Type in (" + a.OrdType + ")";
                        }
                        if(a.DelayedMinute > 0)
                        {
                            DateTime x = DateTime.Now.AddMinutes(a.DelayedMinute);
                            sqlcommand = sqlcommand + " and paydate >= '" + x + "'";
                        }
                        if(!string.IsNullOrEmpty(a.MinAmt))
                        {
                            sqlcommand = sqlcommand + " and Amount >= " + a.MinAmt;
                        }
                        if(!string.IsNullOrEmpty(a.MaxAmt))
                        {
                            sqlcommand = sqlcommand + " and Amount <= " + a.MaxAmt;
                        }
                        var ord = conn.Query<int>(sqlcommand).AsList();
                        if(ord.Count == 0) continue;
                        foreach(var o in ord)
                        {
                            sqlcommand = @"select * from orderitem where oid = " + o + " and coid = " + CoID;
                            var item = conn.Query<OrderItem>(sqlcommand).AsList();
                            if(!string.IsNullOrEmpty(a.AppointSku))
                            {
                                int j = 0;
                                foreach(var i in item)
                                {
                                    if(a.AppointSku.Contains(i.SkuID))
                                    {
                                        j ++;
                                        break;
                                    }
                                }
                                if(j == 0) continue;
                            }
                            if(!string.IsNullOrEmpty(a.ExcludeSku))
                            {
                                int j = 0;
                                foreach(var i in item)
                                {
                                    if(a.ExcludeSku.Contains(i.SkuID))
                                    {
                                        j ++;
                                        break;
                                    }
                                }
                                if(j > 0) continue;
                            }
                            if(!string.IsNullOrEmpty(a.DiscountRate))
                            {
                                decimal OAmt = 0,NAmt = 0;
                                foreach(var i in item)
                                {
                                    OAmt = OAmt + i.Qty * decimal.Parse(i.SalePrice);
                                    NAmt = NAmt + i.Qty * decimal.Parse(i.RealPrice);
                                }
                                if(NAmt/OAmt < decimal.Parse(a.DiscountRate)) continue;
                            }
                            ID.Add(o);
                        }
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }    
            if(ID.Count > 0)
            {
                result = ConfirmOrder(ID,CoID,UserName);
            }       
            return result;
        }
        ///<summary>
        ///缺货单智能提交
        ///</summary>
        public static DataResult AutoOutOfStock(int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var ID = new List<int>();
            int reasonid = GetReasonID("缺货",CoID,7).s;
            if(reasonid <= 0)
            {
                result.s = -1;
                result.d = "请先设定【缺货】的异常";
                return result;
            }
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    DateTime today = DateTime.Now;
                    string sqlcommand = @"select ID from `order` where coid = " + CoID + " and status = 7 and AbnormalStatus = " + reasonid;
                    ID = conn.Query<int>(sqlcommand).AsList();
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }    
            if(ID.Count > 0)
            {
                result = TransferNormal(ID,CoID,UserName);
                if(result.s == 1)
                {
                    result = ConfirmOrder(ID,CoID,UserName);
                }
            }       
            return result;
        }
        ///<summary>
        ///智能配快递
        ///</summary>
        public static DataResult AutoSetExpress(int CoID,string UserName)
        {
            var result = new DataResult(1,null);
            var ID = new List<int>();
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{    
                    DateTime today = DateTime.Now;
                    string sqlcommand = @"select ID from `order` where coid = " + CoID + " and status in (0,1,2,7) and (ISNULL(ExID) or ExID = 0)";
                    ID = conn.Query<int>(sqlcommand).AsList();
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            result =  SetExp(ID,CoID,"B","自动计算快递",UserName);
            return result;
        }
        ///<summary>
        ///批量审核所有符合条件的订单
        ///</summary>
        public static DataResult ConfirmOrdAll(OrderParm cp,string UserName)
        {
            var result = new DataResult(1,null);
            result = GetOrderList(cp);
            var res = result.d as OrderData;
            var ord = res.Ord as List<OrderQuery>;
            var ID = new List<int>();
            var IDN = new List<int>();
            foreach(var a in ord)
            {
                if(a.Status == 7 && a.AbnormalStatusDec == "缺货")
                {
                    ID.Add(a.ID);
                    IDN.Add(a.ID);
                }
                if(a.Status == 1)
                {
                    ID.Add(a.ID);
                }
            }
            if(IDN.Count > 0)
            {
                result = TransferNormal(IDN,cp.CoID,UserName);
            }
            if(ID.Count > 0)
            {
                result = ConfirmOrder(ID,cp.CoID,UserName);
            }
            return result;
        }
        ///<summary>
        ///新增特殊单订单识别
        ///</summary>
        public static DataResult InsertOrderSpecial(OrdSpecial rule)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string sqlcommand = @"INSERT INTO order_special(Shop,StartDate,EndDate,RecMessage,SendMessage,RecAddress,CoID,Creator,Modifier) 
                                                            VALUES(@Shop,@StartDate,@EndDate,@RecMessage,@SendMessage,@RecAddress,@CoID,@Creator,@Modifier)";     
                    int count = conn.Execute(sqlcommand,rule);
                    if(count <= 0)
                    {
                        result.s = -3002;
                        return result;
                    }
                    int rtn = conn.QueryFirst<int>("select LAST_INSERT_ID()");
                    result.d = rtn;
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            return result;
        }
        ///<summary>
        ///查询特殊单订单识别
        ///</summary>
        public static DataResult GetOrderSpecial(int CoID)
        {
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string sqlcommand = @"select ID,Shop,StartDate,EndDate,RecMessage,SendMessage,RecAddress from order_special where coid = " + CoID;     
                    var u = conn.Query<OrdSpecialSingle>(sqlcommand).AsList();
                    if(u.Count > 0)
                    {
                        result.d = u[0];
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            return result;
        }
        ///<summary>
        ///更新特殊单订单识别
        ///</summary>
        public static DataResult UpdateOrderSpecial(int CoID,string Shop,string StartDate,string EndDate,string RecMessage,string SendMessage,string RecAddress,string UserName)
        {
            var p = new DynamicParameters();
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.CoreConnectString) ){
                try{
                    string sqlcommand = "update order_special set ";
                    int i = 0;
                    if(Shop != null)
                    {
                        sqlcommand = sqlcommand + "Shop = @Shop,";
                        p.Add("@Shop",Shop);
                        i ++;
                    }
                    if(StartDate != null)
                    {
                        sqlcommand = sqlcommand + "StartDate = @StartDate,";
                        p.Add("@StartDate",StartDate);
                        i ++;
                    }
                    if(EndDate != null)
                    {
                        sqlcommand = sqlcommand + "EndDate = @EndDate,";
                        p.Add("@EndDate",EndDate);
                        i ++;
                    }
                    if(RecMessage != null)
                    {
                        sqlcommand = sqlcommand + "RecMessage = @RecMessage,";
                        p.Add("@RecMessage",RecMessage);
                        i ++;
                    }
                    if(SendMessage != null)
                    {
                        sqlcommand = sqlcommand + "SendMessage = @SendMessage,";
                        p.Add("@SendMessage",SendMessage);
                        i ++;
                    }
                    if(RecAddress != null)
                    {
                        sqlcommand = sqlcommand + "RecAddress = @RecAddress,";
                        p.Add("@RecAddress",RecAddress);
                        i ++;
                    }
                    if(i > 0)
                    {
                        sqlcommand = sqlcommand + "Modifier = @Modifier,ModifyDate=@ModifyDate where coid = @CoID";
                        p.Add("@Modifier",UserName);
                        p.Add("@ModifyDate",DateTime.Now);
                        p.Add("@CoID",CoID);
                        int count = conn.Execute(sqlcommand,p);
                        if(count <= 0)
                        {
                            result.s = -3003;
                            return result;
                        }
                    }
                }catch(Exception ex){
                    result.s = -1;
                    result.d = ex.Message;
                    conn.Dispose();
                }
            }   
            return result;
        }
    }
}