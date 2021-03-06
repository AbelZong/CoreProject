using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using CoreData.CoreCore;
using CoreModels.XyCore;
using System;
using System.Collections.Generic;
using CoreData.CoreComm;
using CoreData;
using CoreModels;
namespace CoreWebApi.XyCore
{
    // [AllowAnonymous]
    public class StockInitControllers : ControllBase
    {
        #region 库存期初 - 查询 - 主表
        [HttpGetAttribute("Core/XyCore/StockInit/StockInitMainLst")]
        public ResponseResult StockInitMainLst(string Skuautoid, string PageIndex, string PageSize, string SortField, string SortDirection)
        {
            var cp = new Sfc_item_param();
            int x;
            if (int.TryParse(Skuautoid, out x))
            {
                cp.Skuautoid = int.Parse(Skuautoid);
            }
            if (int.TryParse(PageIndex, out x))
            {
                cp.PageIndex = int.Parse(PageIndex);
            }
            if (int.TryParse(PageSize, out x))
            {
                cp.PageSize = int.Parse(PageSize);
            }
            //排序参数赋值
            if (!string.IsNullOrEmpty(SortField))
            {
                var res = CommHaddle.SysColumnExists(DbBase.CoreConnectString, "sfc_main", SortField);
                if (res.s == 1)
                {
                    cp.SortField = SortField;
                    if (!string.IsNullOrEmpty(SortDirection) && (SortDirection.ToUpper() == "DESC" || SortDirection.ToUpper() == "ASC"))
                    {
                        cp.SortDirection = SortDirection.ToUpper();
                    }
                }
            }
            cp.CoID = GetCoid();
            var Result = StockInitHaddle.GetStockInitMain(cp);
            return CoreResult.NewResponse(Result.s, Result.d, "General");
        }
        #endregion

        #region 库存期初 - 查询 - 子表
        [HttpGetAttribute("Core/XyCore/StockInit/StockInitItemLst")]
        public ResponseResult StockInitItemLst(string ParentID, string SkuID, string SkuName, string PageIndex, string PageSize, string SortField, string SortDirection)
        {
            var res = new DataResult(1, null);
            int x;
            var cp = new Sfc_item_param();
            if (!string.IsNullOrEmpty(SkuID))//款式编号
            {
                cp.SkuID = SkuID;
            }
            if (!string.IsNullOrEmpty(SkuName))//款式名称
            {
                cp.SkuName = SkuName;
            }
            if (int.TryParse(PageIndex, out x))
            {
                cp.PageIndex = int.Parse(PageIndex);
            }
            if (int.TryParse(PageSize, out x))
            {
                cp.PageSize = int.Parse(PageSize);
            }
            if (int.TryParse(ParentID, out x))
            {
                cp.ParentID = int.Parse(ParentID);
                cp.CoID = GetCoid();
                cp.Type = 1;//单据类型(1.期初，2.盘点，3.调拨)
                res = StockInitHaddle.GetStockInitItem(cp);
            }
            else
            {
                res.s = -1;
                res.d = "无效参数";
            }
            return CoreResult.NewResponse(res.s, res.d, "General");
        }
        #endregion

        #region 库存期初 - 新增 - 表头 
        [HttpPostAttribute("Core/XyCore/StockInit/InsertInitMain")]
        public ResponseResult InsertInitMain([FromBodyAttribute]JObject obj)
        {
            var res = new DataResult(1, null);
            int x;
            if (obj["WhID"] != null && obj["Parent_WhID"] != null)
            {
                string WhID = obj["WhID"].ToString();
                string Parent_WhID = obj["Parent_WhID"].ToString();
                if (!int.TryParse(WhID, out x) && !int.TryParse(Parent_WhID, out x))
                {
                    res.s = -1;
                    res.d = "无效参数";
                }
                else
                {
                    string CoID = GetCoid();
                    string UserName = GetUname();
                    res = StockTakeHaddle.InsertStockTakeMain(WhID, Parent_WhID, 1, CoID, UserName);
                }
            }
            else
            {
                res.s = -1;
                res.d = "无效参数";
            }
            return CoreResult.NewResponse(res.s, res.d, "General");
        }
        #endregion

        #region 库存期初 - 新增 - 子表 - 从商品资料导入
        [HttpPostAttribute("Core/XyCore/StockInit/InsertInitItem")]
        public ResponseResult InsertInitItem([FromBodyAttribute]JObject obj)
        {
            var res = new DataResult(1, null);
            int x;
            if (obj["ParentID"] != null && obj["SkuIDLst"] != null)
            {
                var ParentID = obj["ParentID"].ToString();
                var SkuIDLst = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(obj["SkuIDLst"].ToString());
                if (!int.TryParse(ParentID, out x))
                {
                    res.s = -1;
                    res.d = "无效参数";
                }
                else
                {
                    string CoID = GetCoid();
                    string UserName = GetUname();
                    res = StockTakeHaddle.InsertStockTakeItem(ParentID, SkuIDLst, 1, CoID, UserName);
                }
            }
            else
            {
                res.s = -1;
                res.d = "无效参数";
            }

            return CoreResult.NewResponse(res.s, res.d, "General");
        }
        #endregion

        #region 库存期初 - 修改 - 保存期初单价
        [HttpPostAttribute("Core/XyCore/StockInit/SaveInitPrice")]
        public ResponseResult SaveInitPrice([FromBodyAttribute]JObject obj)
        {
            var res = new DataResult(1, null);
            int x;
            Decimal y;
            if (!(obj["ID"] != null && obj["Price"] != null && int.TryParse(obj["ID"].ToString(), out x) && decimal.TryParse(obj["Price"].ToString(), out y)))
            {
                res.s = -1;
                res.d = "无效参数";
            }
            else
            {
                var ID = obj["ID"].ToString();
                var strPrice = obj["Price"].ToString();
                string CoID = GetCoid();
                string UserName = GetUname();
                var Price = decimal.Parse(strPrice);
                res = StockInitHaddle.SaveStockInitPrice(ID, Price, CoID, UserName);
            }
            return CoreResult.NewResponse(res.s, res.d, "General");
        }
        #endregion

        #region 库存期初 - 修改 - 保存期初数量
        [HttpPostAttribute("Core/XyCore/StockInit/SaveInitQty")]
        public ResponseResult SaveInitQty([FromBodyAttribute]JObject obj)
        {
            var res = new DataResult(1, null);
            int x;
            if (!(obj["ID"] != null && obj["InvQty"] != null && int.TryParse(obj["ID"].ToString(), out x) && int.TryParse(obj["InvQty"].ToString(), out x)))
            {
                res.s = -1;
                res.d = "无效参数";
            }
            else
            {
                string ID = obj["ID"].ToString();
                string InvQty = obj["InvQty"].ToString();
                string CoID = GetCoid();
                string UserName = GetUname();
                var qty = int.Parse(InvQty);
                res = StockInitHaddle.SaveStockInitQty(ID, qty, CoID, UserName);
            }
            return CoreResult.NewResponse(res.s, res.d, "General");
        }
        #endregion

        #region 库存期初 - 确认生效
        [HttpPostAttribute("Core/XyCore/StockInit/CheckInit")]
        public ResponseResult CheckInit([FromBodyAttribute]JObject obj)
        {
            var res = new DataResult(1, null);
            int x;
            if (!(obj["ID"] != null && int.TryParse(obj["ID"].ToString(), out x)))
            {
                res.s = -1;
                res.d = "无效参数";
            }
            else
            {
                string CoID = GetCoid();
                string UserName = GetUname();
                string ID = obj["ID"].ToString();
                res = StockInitHaddle.CheckStockInit(ID, CoID, UserName);
            }
            return CoreResult.NewResponse(res.s, res.d, "General");
        }
        #endregion

        #region 库存期初 - 作废期初
        [HttpPostAttribute("Core/XyCore/StockInit/UnCheckInit")]
        public ResponseResult UnCheckInit([FromBodyAttribute]JObject obj)
        {
            var res = new DataResult(1, null);
            int x;
            if (!(obj["ID"] != null && int.TryParse(obj["ID"].ToString(), out x)))
            {
                res.s = -1;
                res.d = "无效参数";
            }
            else
            {
                string CoID = GetCoid();
                string UserName = GetUname();
                string ID = obj["ID"].ToString();
                res = StockTakeHaddle.UnCheckStockTake(ID, 1, CoID, UserName);
            }
            return CoreResult.NewResponse(res.s, res.d, "General");
        }
        #endregion
    }
}