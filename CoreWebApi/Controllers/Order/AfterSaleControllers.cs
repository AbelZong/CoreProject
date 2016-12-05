using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using CoreModels.XyCore;
using CoreData.CoreCore;
using System;
using CoreData.CoreComm;
using CoreData;
using System.Collections.Generic;
using CoreModels;
namespace CoreWebApi
{
    [AllowAnonymous]
    public class AfterSaleController : ControllBase
    {
        [HttpGetAttribute("/Core/AfterSale/GetInitASData")]
        public ResponseResult GetInitASData()
        {
            var data = AfterSaleHaddle.GetInitASData(int.Parse(GetCoid()));
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/GetAsList")]
        public ResponseResult GetAsList(string ExCode,string SoID,string OID,string ID,string BuyerShopID,string RecName,string Modifier,string RecPhone,string RecTel,
                                        string Creator,string Remark,string DateType,string DateStart,string DateEnd,string SkuID,string GoodsCode,string IsNoOID,string IsInterfaceLoad,
                                        string IsSubmitDis,string ShopID,string Status,string GoodsStatus,string Type,string OrdType,string ShopStatus,string RefundStatus,
                                        string Distributor,string IsSubmit,string IssueType,string Result,string SortField,string SortDirection,string PageIndex,string NumPerPage)
        {   
            int x;
            long l;
            var cp = new AfterSaleParm();
            cp.CoID = int.Parse(GetCoid());
            cp.ExCode = ExCode;
            if(!string.IsNullOrEmpty(SoID))
            {
                if (long.TryParse(SoID, out l))
                {
                    cp.SoID = long.Parse(SoID);
                }
            }
            if(!string.IsNullOrEmpty(OID))
            {
                if (int.TryParse(OID, out x))
                {
                    cp.OID = int.Parse(OID);
                }
            }
            if(!string.IsNullOrEmpty(ID))
            {
                if (int.TryParse(ID, out x))
                {
                    cp.ID = int.Parse(ID);
                }
            }
            cp.Modifier = Modifier;
            cp.BuyerShopID = BuyerShopID;
            cp.Creator = Creator;
            cp.RecName = RecName;
            cp.RecPhone = RecPhone;
            cp.RecTel = RecTel;
            cp.Remark = Remark;
            if(!string.IsNullOrEmpty(DateType))
            {
                if(DateType.ToUpper() == "REGISTERDATE" || DateType.ToUpper() == "MODIFYDATE" || DateType.ToUpper() == "CONFIRMDATE")
                {
                    cp.DateType = DateType;
                }
            }
            DateTime date;
            if (DateTime.TryParse(DateStart, out date))
            {
                cp.DateStart = DateTime.Parse(DateStart);
            }
            if (DateTime.TryParse(DateEnd, out date))
            {
                cp.DateEnd = DateTime.Parse(DateEnd);
            }
            cp.SkuID = SkuID;
            cp.GoodsCode = GoodsCode;
            cp.IsNoOID = IsNoOID;
            cp.IsInterfaceLoad = IsInterfaceLoad;
            cp.IsSubmitDis = IsSubmitDis;
            if(!string.IsNullOrEmpty(ShopID))
            {
                if (int.TryParse(ShopID, out x))
                {
                    cp.ShopID = int.Parse(ShopID);
                }
            }
            if(!string.IsNullOrEmpty(Status))
            {
                if (int.TryParse(Status, out x))
                {
                    cp.Status = int.Parse(Status);
                }
            }
            cp.GoodsStatus = GoodsStatus;
            if(!string.IsNullOrEmpty(Type))
            {
                if (int.TryParse(Type, out x))
                {
                    cp.Type = int.Parse(Type);
                }
            }
            if(!string.IsNullOrEmpty(OrdType))
            {
                if (int.TryParse(OrdType, out x))
                {
                    cp.OrdType = int.Parse(OrdType);
                }
            }
            if(!string.IsNullOrEmpty(ShopStatus))
            {
                string[] a = ShopStatus.Split(',');
                List<string> s = new List<string>();
                foreach(var i in a)
                {
                    s.Add(i);
                }
                cp.ShopStatus = s;
            }
            if(!string.IsNullOrEmpty(RefundStatus))
            {
                string[] a = RefundStatus.Split(',');
                List<string> s = new List<string>();
                foreach(var i in a)
                {
                    s.Add(i);
                }
                cp.RefundStatus = s;
            }
            if(!string.IsNullOrEmpty(Distributor))
            {
                if (int.TryParse(Distributor, out x))
                {
                    cp.Distributor = int.Parse(Distributor);
                }
            }
            cp.IsSubmit = IsSubmit;
            if(!string.IsNullOrEmpty(IssueType))
            {
                if (int.TryParse(IssueType, out x))
                {
                    cp.IssueType = int.Parse(IssueType);
                }
            }
            if(!string.IsNullOrEmpty(Result))
            {
                if (int.TryParse(Result, out x))
                {
                    cp.Result = int.Parse(Result);
                }
            }
            if(!string.IsNullOrEmpty(SortField))
            {
                if(CommHaddle.SysColumnExists(DbBase.CoreConnectString,"aftersale",SortField).s == 1)
                {
                    cp.SortField = SortField;
                }
            }
            if(!string.IsNullOrEmpty(SortDirection))
            {
                 if(SortDirection.ToUpper() == "ASC" || SortDirection.ToUpper() == "DESC")
                {
                    cp.SortDirection = SortDirection;
                }
            }
            if (int.TryParse(NumPerPage, out x))
            {
                cp.NumPerPage = int.Parse(NumPerPage);
            }
            if (int.TryParse(PageIndex, out x))
            {
                cp.PageIndex = int.Parse(PageIndex);
            }
            var data = AfterSaleHaddle.GetAsList(cp);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/GetASOrderList")]
        public ResponseResult GetASOrderList(string ID,string SoID,string PayNbr,string BuyerShopID,string ExCode,string RecName,string RecPhone,string RecTel,
                                             string Status,string DateStart,string DateEnd,string ShopID,string Distributor,string ExID,string SendWarehouse,
                                             string SortField,string SortDirection,string PageIndex,string NumPerPage)
        {   
            int x;
            long l;
            var cp = new ASOrderParm();
            cp.CoID = int.Parse(GetCoid());
            if(!string.IsNullOrEmpty(ID))
            {
                if (int.TryParse(ID, out x))
                {
                    cp.ID = int.Parse(ID);
                }
            }
            if(!string.IsNullOrEmpty(SoID))
            {
                if (long.TryParse(SoID, out l))
                {
                    cp.SoID = long.Parse(SoID);
                }
            }
            cp.PayNbr = PayNbr;
            cp.BuyerShopID = BuyerShopID;
            cp.ExCode = ExCode;
            cp.RecName = RecName;
            cp.RecPhone = RecPhone;
            cp.RecTel = RecTel;
            if(!string.IsNullOrEmpty(Status))
            {
                if (int.TryParse(Status, out x))
                {
                    cp.Status = int.Parse(Status);
                }
            }
            DateTime date;
            if (DateTime.TryParse(DateStart, out date))
            {
                cp.DateStart = DateTime.Parse(DateStart);
            }
            if (DateTime.TryParse(DateEnd, out date))
            {
                cp.DateEnd = DateTime.Parse(DateEnd);
            }
            if(!string.IsNullOrEmpty(ShopID))
            {
                if (int.TryParse(ShopID, out x))
                {
                    cp.ShopID = int.Parse(ShopID);
                }
            }
            if(!string.IsNullOrEmpty(ExID))
            {
                if (int.TryParse(ExID, out x))
                {
                    cp.ExID = int.Parse(ExID);
                }
            }
            if(!string.IsNullOrEmpty(SendWarehouse))
            {
                if (int.TryParse(SendWarehouse, out x))
                {
                    cp.SendWarehouse = int.Parse(SendWarehouse);
                }
            }
            cp.Distributor = Distributor;
            if(!string.IsNullOrEmpty(SortField))
            {
                if(CommHaddle.SysColumnExists(DbBase.CoreConnectString,"order",SortField).s == 1)
                {
                    cp.SortField = SortField;
                }
            }
            if(!string.IsNullOrEmpty(SortDirection))
            {
                 if(SortDirection.ToUpper() == "ASC" || SortDirection.ToUpper() == "DESC")
                {
                    cp.SortDirection = SortDirection;
                }
            }
            if (int.TryParse(NumPerPage, out x))
            {
                cp.NumPerPage = int.Parse(NumPerPage);
            }
            if (int.TryParse(PageIndex, out x))
            {
                cp.PageIndex = int.Parse(PageIndex);
            }
            var data = AfterSaleHaddle.GetASOrderList(cp);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/GetASOrderItemS")]
        public ResponseResult GetASOrderItemS(string OID)
        {   
            int x,oid;
            int CoID = int.Parse(GetCoid());
            if(!string.IsNullOrEmpty(OID))
            {
                if (int.TryParse(OID, out x))
                {
                    oid = int.Parse(OID);
                }
                else
                {
                    return CoreResult.NewResponse(-1, "订单ID参数无效", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "订单ID必填", "General"); 
            }
            var data = AfterSaleHaddle.GetASOrderItemS(oid,CoID);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/InsertASInit")]
        public ResponseResult InsertASInit()
        {  
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.InsertASInit(CoID);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }
        [HttpPostAttribute("/Core/AfterSale/InsertAfterSale")]
        public ResponseResult InsertAfterSale([FromBodyAttribute]JObject co)
        {   
            string DuType = "",Remark="",Express="",ExCode = "";
            if(co["DocumentType"] != null)
            {
                DuType = co["DocumentType"].ToString();
                if(!string.IsNullOrEmpty(DuType))
                {
                    DuType = DuType.ToUpper();
                    if(DuType != "A" && DuType != "B")
                    {
                        return CoreResult.NewResponse(-1, "请指定创建的售后单是否是无信息件", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "请指定创建的售后单是否是无信息件", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "请指定创建的售后单是否是无信息件", "General"); 
            }
            if(co["Remark"] != null)
            {
                Remark = co["Remark"].ToString();
            }
            if(co["Express"] != null)
            {
                Express = co["Express"].ToString();
            }
            if(co["ExCode"] != null)
            {
                ExCode = co["ExCode"].ToString();
            }
            int Type = 0,x,WarehouseID = -1,IssueType = 0,OID = -1;
            if(co["Type"] != null)
            {
                string Text = co["Type"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Type = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后类型参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后类型必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后类型必填", "General"); 
            }
            if(co["IssueType"] != null)
            {
                string Text = co["IssueType"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        IssueType = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "问题分类参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "问题分类必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "问题分类必填", "General"); 
            }
            if(co["WarehouseID"] != null)
            {
                string Text = co["WarehouseID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        WarehouseID = int.Parse(Text);
                    }
                }
            }
            decimal SalerReturnAmt = 0,BuyerUpAmt = 0,y;
            if(co["SalerReturnAmt"] != null)
            {
                string Text = co["SalerReturnAmt"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (decimal.TryParse(Text, out y))
                    {
                        SalerReturnAmt = decimal.Parse(Text);
                    }
                }
            }
            if(co["BuyerUpAmt"] != null)
            {
                string Text = co["BuyerUpAmt"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (decimal.TryParse(Text, out y))
                    {
                        BuyerUpAmt = decimal.Parse(Text);
                    }
                }
            }
            if(DuType == "A")
            {
                if(co["OID"] != null)
                {
                    string Text = co["OID"].ToString();
                    if(!string.IsNullOrEmpty(Text))
                    {
                        if (int.TryParse(Text, out x))
                        {
                            OID = int.Parse(Text);
                        }
                        else
                        {
                            return CoreResult.NewResponse(-1, "订单ID参数无效", "General"); 
                        }
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "订单ID必填", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "订单ID必填", "General"); 
                }
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.InsertAfterSale(DuType,CoID,Type,SalerReturnAmt,BuyerUpAmt,WarehouseID,IssueType,Remark,username,Express,ExCode,OID);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/UpdateAfterSale")]
        public ResponseResult UpdateAfterSale([FromBodyAttribute]JObject co)
        {   
            string Remark = null,Express = null,ExCode = null,ReturnAccount=null;
            if(co["Remark"] != null)
            {
                Remark = co["Remark"].ToString();
            }
            if(co["Express"] != null)
            {
                Express = co["Express"].ToString();
            }
            if(co["ExCode"] != null)
            {
                ExCode = co["ExCode"].ToString();
            }
            if(co["ReturnAccount"] != null)
            {
                ReturnAccount = co["ReturnAccount"].ToString();
            }
            int Type = -1,x,WarehouseID = -1,Result = -1,RID = -1;
            if(co["Type"] != null)
            {
                string Text = co["Type"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Type = int.Parse(Text);
                    }
                }
            }
            if(co["Result"] != null)
            {
                string Text = co["Result"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Result = int.Parse(Text);
                    }
                }
            }
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            if(co["WarehouseID"] != null)
            {
                string Text = co["WarehouseID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        WarehouseID = int.Parse(Text);
                    }
                }
            }
            decimal SalerReturnAmt = -1,BuyerUpAmt = -1,y;
            if(co["SalerReturnAmt"] != null)
            {
                string Text = co["SalerReturnAmt"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (decimal.TryParse(Text, out y))
                    {
                        SalerReturnAmt = decimal.Parse(Text);
                    }
                }
            }
            if(co["BuyerUpAmt"] != null)
            {
                string Text = co["BuyerUpAmt"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (decimal.TryParse(Text, out y))
                    {
                        BuyerUpAmt = decimal.Parse(Text);
                    }
                }
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.UpdateAfterSale(CoID,Type,SalerReturnAmt,BuyerUpAmt,ReturnAccount,WarehouseID,Remark,username,Express,ExCode,Result,RID);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/GetASOrdItem")]
        public ResponseResult GetASOrdItem(string RID)
        {  
            int rid,x;
            if(!string.IsNullOrEmpty(RID))
            {
                if (int.TryParse(RID, out x))
                {
                    rid = int.Parse(RID);
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.GetASOrdItem(CoID,rid);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/InsertASItemOrder")]
        public ResponseResult InsertASItemOrder([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            var oid = new List<int>();
            if(co["DetailID"] != null)
            {
                oid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["DetailID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "订单明细ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.InsertASItemOrder(CoID,username,RID,oid);
            if(data.s == 1)
            {
                data = AfterSaleHaddle.GetAfterSaleItem(CoID,RID);
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/GetAfterSaleItem")]
        public ResponseResult GetAfterSaleItem(string RID)
        {  
            int rid,x;
            if(!string.IsNullOrEmpty(RID))
            {
                if (int.TryParse(RID, out x))
                {
                    rid = int.Parse(RID);
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.GetAfterSaleItem(CoID,rid);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/InsertASItemSku")]
        public ResponseResult InsertASItemSku([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            var oid = new List<int>();
            if(co["SkuID"] != null)
            {
                oid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["SkuID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "商品ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.InsertASItemSku(CoID,username,RID,oid);
            var res = new InsertASItemSkuReturn();
            if(data.s == 1)
            {
                res.SuccessIDs = AfterSaleHaddle.GetAfterSaleItem(CoID,RID).d as List<AfterSaleItemQuery>;
                res.FailIDs = data.d  as List<InsertFailReason>;
                data.d = res;
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/UpdateASItem")]
        public ResponseResult UpdateASItem([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x,RDetailID = 0,Qty = -1;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            if(co["RDetailID"] != null)
            {
                string Text = co["RDetailID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RDetailID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后明细ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后明细ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后明细ID必填", "General"); 
            }
            if(co["Qty"] != null)
            {
                string Text = co["Qty"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Qty = int.Parse(Text);
                        if(Qty <= 0)
                        {
                            return CoreResult.NewResponse(-1, "数量必须大于零", "General"); 
                        }
                    }
                }
            }
            decimal Amount = -1;
            if(co["Amount"] != null)
            {
                string Text = co["Amount"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Amount = int.Parse(Text);
                        if(Amount <= 0)
                        {
                            return CoreResult.NewResponse(-1, "金额必须大于零", "General"); 
                        }
                    }
                }
            }
            if(Amount == -1 && Qty == -1)
            {
                return CoreResult.NewResponse(-1, "数量和金额必须填写其中一个", "General"); 
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.UpdateASItem(CoID,username,RID,RDetailID,Qty,Amount);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/DeleteASItem")]
        public ResponseResult DeleteASItem([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            var oid = new List<int>();
            if(co["RDetailID"] != null)
            {
                oid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["RDetailID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后明细ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.DeleteASItem(CoID,username,RID,oid);
            var res = new InsertASItemSkuReturn();
            if(data.s == 1)
            {
                data = AfterSaleHaddle.GetAfterSaleItem(CoID,RID);
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/GetAfterSaleEdit")]
        public ResponseResult GetAfterSaleEdit(string RID)
        {  
            int rid,x;
            if(!string.IsNullOrEmpty(RID))
            {
                if (int.TryParse(RID, out x))
                {
                    rid = int.Parse(RID);
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.GetAfterSaleEdit(rid,CoID);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/UpdateAfterSaleE")]
        public ResponseResult UpdateAfterSaleE([FromBodyAttribute]JObject co)
        {   
            string Remark = null,Express = null,ExCode = null,ReturnAccount=null;
            if(co["Remark"] != null)
            {
                Remark = co["Remark"].ToString();
            }
            if(co["Express"] != null)
            {
                Express = co["Express"].ToString();
            }
            if(co["ExCode"] != null)
            {
                ExCode = co["ExCode"].ToString();
            }
            if(co["ReturnAccount"] != null)
            {
                ReturnAccount = co["ReturnAccount"].ToString();
            }
            int Type = -1,x,WarehouseID = -1,Result = -1,RID = -1;
            if(co["Type"] != null)
            {
                string Text = co["Type"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Type = int.Parse(Text);
                    }
                }
            }
            if(co["Result"] != null)
            {
                string Text = co["Result"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Result = int.Parse(Text);
                    }
                }
            }
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            if(co["WarehouseID"] != null)
            {
                string Text = co["WarehouseID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        WarehouseID = int.Parse(Text);
                    }
                }
            }
            decimal SalerReturnAmt = -1,BuyerUpAmt = -1,y;
            if(co["SalerReturnAmt"] != null)
            {
                string Text = co["SalerReturnAmt"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (decimal.TryParse(Text, out y))
                    {
                        SalerReturnAmt = decimal.Parse(Text);
                    }
                }
            }
            if(co["BuyerUpAmt"] != null)
            {
                string Text = co["BuyerUpAmt"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (decimal.TryParse(Text, out y))
                    {
                        BuyerUpAmt = decimal.Parse(Text);
                    }
                }
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.UpdateAfterSale(CoID,Type,SalerReturnAmt,BuyerUpAmt,ReturnAccount,WarehouseID,Remark,username,Express,ExCode,Result,RID);
            var res = new UpdateAfterSaleEReturn();
            if(data.s == 1)
            {
                res.AfterSale = AfterSaleHaddle.GetAfterSaleSingle(RID,CoID).d as AfterSaleEdit;
                res.AfterSaleItem = AfterSaleHaddle.GetAfterSaleItem(CoID,RID).d as List<AfterSaleItemQuery>;
                res.Log = AfterSaleHaddle.GetOrderLog(RID,CoID).d as List<OrderLog>;
                data.d = res;
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/InsertASItemOrderE")]
        public ResponseResult InsertASItemOrderE([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            var oid = new List<int>();
            if(co["DetailID"] != null)
            {
                oid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["DetailID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "订单明细ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.InsertASItemOrder(CoID,username,RID,oid);
            var res = new InsertASItemOrderEReturn();
            if(data.s == 1)
            {
                res.AfterSaleItem = AfterSaleHaddle.GetAfterSaleItem(CoID,RID).d as List<AfterSaleItemQuery>;
                res.Log = AfterSaleHaddle.GetOrderLog(RID,CoID).d as List<OrderLog>;
                data.d = res;
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/InsertASItemSkuE")]
        public ResponseResult InsertASItemSkuE([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            var oid = new List<int>();
            if(co["SkuID"] != null)
            {
                oid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["SkuID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "商品ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.InsertASItemSku(CoID,username,RID,oid);
            var res = new InsertASItemSkuEReturn();
            if(data.s == 1)
            {
                res.SuccessIDs = AfterSaleHaddle.GetAfterSaleItem(CoID,RID).d as List<AfterSaleItemQuery>;
                res.FailIDs = data.d  as List<InsertFailReason>;
                res.Log = AfterSaleHaddle.GetOrderLog(RID,CoID).d as List<OrderLog>;
                data.d = res;
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/UpdateASItemE")]
        public ResponseResult UpdateASItemE([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x,RDetailID = 0,Qty = -1;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            if(co["RDetailID"] != null)
            {
                string Text = co["RDetailID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RDetailID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后明细ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后明细ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后明细ID必填", "General"); 
            }
            if(co["Qty"] != null)
            {
                string Text = co["Qty"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Qty = int.Parse(Text);
                        if(Qty <= 0)
                        {
                            return CoreResult.NewResponse(-1, "数量必须大于零", "General"); 
                        }
                    }
                }
            }
            decimal Amount = -1;
            if(co["Amount"] != null)
            {
                string Text = co["Amount"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        Amount = int.Parse(Text);
                        if(Amount <= 0)
                        {
                            return CoreResult.NewResponse(-1, "金额必须大于零", "General"); 
                        }
                    }
                }
            }
            if(Amount == -1 && Qty == -1)
            {
                return CoreResult.NewResponse(-1, "数量和金额必须填写其中一个", "General"); 
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.UpdateASItem(CoID,username,RID,RDetailID,Qty,Amount);
            if(data.s == 1)
            {
                data.d = AfterSaleHaddle.GetOrderLog(RID,CoID).d;
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/DeleteASItemE")]
        public ResponseResult DeleteASItemE([FromBodyAttribute]JObject co)
        {   
            int RID = 0,x;
            if(co["RID"] != null)
            {
                string Text = co["RID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        RID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            var oid = new List<int>();
            if(co["RDetailID"] != null)
            {
                oid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["RDetailID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后明细ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.DeleteASItem(CoID,username,RID,oid);
            var res = new InsertASItemOrderEReturn();
            if(data.s == 1)
            {
                res.AfterSaleItem = AfterSaleHaddle.GetAfterSaleItem(CoID,RID).d as List<AfterSaleItemQuery>;
                res.Log = AfterSaleHaddle.GetOrderLog(RID,CoID).d as List<OrderLog>;
                data.d = res;
            }
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/BindOrd")]
        public ResponseResult BindOrd([FromBodyAttribute]JObject co)
        {   
            int OID = 0,x;
            if(co["OID"] != null)
            {
                string Text = co["OID"].ToString();
                if(!string.IsNullOrEmpty(Text))
                {
                    if (int.TryParse(Text, out x))
                    {
                        OID = int.Parse(Text);
                    }
                    else
                    {
                        return CoreResult.NewResponse(-1, "订单ID参数无效", "General"); 
                    }
                }
                else
                {
                    return CoreResult.NewResponse(-1, "订单ID必填", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "订单ID必填", "General"); 
            }
            var rid = new List<int>();
            if(co["RID"] != null)
            {
                rid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["RID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.BindOrd(rid,OID,CoID,username);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpGetAttribute("/Core/AfterSale/RefreshAS")]
        public ResponseResult RefreshAS(string RID)
        {  
            int rid,x;
            if(!string.IsNullOrEmpty(RID))
            {
                if (int.TryParse(RID, out x))
                {
                    rid = int.Parse(RID);
                }
                else
                {
                    return CoreResult.NewResponse(-1, "售后ID参数无效", "General"); 
                }
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General"); 
            }
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.RefreshAS(rid,CoID);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }

        [HttpPostAttribute("/Core/AfterSale/CancleAfterSale")]
        public ResponseResult CancleAfterSale([FromBodyAttribute]JObject co)
        {   
            var rid = new List<int>();
            if(co["RID"] != null)
            {
                rid = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(co["RID"].ToString());
            }
            else
            {
                return CoreResult.NewResponse(-1, "售后ID必填", "General");
            }
            string username = GetUname();
            int CoID = int.Parse(GetCoid());
            var data = AfterSaleHaddle.CancleAfterSale(rid,CoID,username);
            return CoreResult.NewResponse(data.s, data.d, "General"); 
        }
    }
}