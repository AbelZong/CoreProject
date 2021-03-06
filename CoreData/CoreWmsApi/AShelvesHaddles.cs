using System.Collections.Generic;
using System.Linq;
using System.Data;
using CoreModels;
using CoreModels.XyUser;
using CoreModels.Enum;
using CoreModels.XyComm;
using CoreModels.XyCore;
using CoreModels.WmsApi;
using CoreData.CoreComm;
using CoreData.CoreCore;
using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Text;

namespace CoreData.CoreWmsApi
{
    public static class AShelvesHaddles
    {
        /// <summary>
        /// 货品上架 - 扫条码获取Sku信息及默认货位
        /// </summary>
        public static DataResult GetUpBoxSku(AShelfParam IParam)
        {
            var result = new DataResult(1, null);
            var data = new AShelfData();
            var cp = new ASkuScanParam();
            cp.CoID = IParam.CoID;
            cp.BarCode = IParam.BoxCode;
            result = ASkuScanHaddles.GetType(cp);
            if (result.s > 0)
            {
                data.SkuAuto = result.d as ASkuScan;
                IParam.Skuautoid = data.SkuAuto.Skuautoid;
                IParam.Qty = data.SkuAuto.Qty;
            }
            result = GetUpPCode(IParam);
            if (result.s > 0)
            {
                data.PileAuto = result.d as AWmsPileAuto;
            }
            if (result.s > 0)
            {
                result.d = data;
            }
            return result;
        }

        /// <summary>
        /// 上架货位,默认货位,货物MaxQty
        /// </summary>
        public static DataResult GetUpPCode(AShelfParam IParam)
        {
            var result = new DataResult(1, null);
            using (var conn = new MySqlConnection(DbBase.CoreConnectString))
            {
                try
                {
                    var PCodeSql1 = @" SELECT
                                            wmspile.WarehouseID,
                                            wmspile.SkuID,
                                            wmspile.PCode,
                                            wmspile.MaxQty,
                                            wmspile.Qty,
                                            wmspile.Type,
                                            wmspile.ID,
                                            wmspile.Order
                                        FROM
                                            wmspile
                                        WHERE                                            
                                            wmspile.Skuautoid = @SkuID
                                        AND wmspile.CoID = @CoID
                                        AND wmspile.maxqty - wmspile.qty >= @BoxQty
                                        AND (
                                            wmspile.WarehouseID = @WarehouseID
                                            AND @WarehouseID > 0
                                            OR @WarehouseID = 0
                                        )
                                        AND (
                                            wmspile.PCode = @PCode
                                            AND IFNULL(@PCode, '') <> ''
                                            OR IFNULL(@PCode, '') = ''
                                        )";
                    var PCodeSql2 = @" SELECT
                                                wmspile.WarehouseID,
                                                wmspile.SkuID,
                                                wmspile.PCode,
                                                wmspile.MaxQty,
                                                wmspile.Qty,
                                                wmspile.Type,
                                                wmspile.ID,
                                                wmspile.Order
                                            FROM
                                                wmspile
                                            WHERE
                                                wmspile.Skuautoid = 0
                                            AND wmspile.CoID = @CoID
                                            AND wmspile.maxqty - wmspile.qty >= @BoxQty
                                            AND (
                                                wmspile.WarehouseID = @WarehouseID
                                                AND @WarehouseID > 0
                                                OR @WarehouseID = 0
                                            )
                                            AND (
                                                wmspile.PCode = @PCode
                                                AND IFNULL(@PCode, '') <> ''
                                                OR IFNULL(@PCode, '') = ''
                                            )
                      ";
                    var p = new DynamicParameters();
                    p.Add("@SkuID", IParam.Skuautoid);
                    p.Add("@CoID", IParam.CoID);
                    p.Add("@WarehouseID", IParam.WarehouseID);
                    p.Add("@BoxQty", IParam.Qty);
                    p.Add("@PCode", IParam.PCode);
                    if (IParam.Type > 0)
                    {
                        PCodeSql1 = PCodeSql1 + " AND wmspile.Type=@Type";
                        PCodeSql2 = PCodeSql2 + " AND wmspile.Type=@Type";
                        p.Add("@Type", IParam.Type);
                    }
                    if (IParam.TypeLst != null && IParam.TypeLst.Count > 0)
                    {
                        PCodeSql1 = PCodeSql1 + " AND wmspile.Type in @TypeLst";
                        PCodeSql2 = PCodeSql2 + " AND wmspile.Type in @TypeLst";
                        p.Add("@TypeLst", IParam.TypeLst);
                    }
                    var PCodeSql = @"SELECT A.WarehouseID,A.SkuID,A.PCode,A.Qty,A.maxqty,A.Type,A.ID
                                    FROM
                                        ( " + PCodeSql1 + " UNION " + PCodeSql2 + " ) AS A ORDER BY A.Order";
                    var PCLst = conn.Query<AWmsPileAuto>(PCodeSql, p).AsList();
                    if (PCLst.Count > 0)
                    {
                        result.d = PCLst[0];
                    }
                    else
                    {
                        result.s = -6002;
                    }
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d = e.Message;
                }
            }
            return result;
        }

        /// <summary>
        /// 货品上架 - 更新货位库存
        /// </summary>
        public static DataResult SetUpShelfPile(AShelfSet IParam)
        {
            var result = new DataResult(1, null);
            var CoreConn = new MySqlConnection(DbBase.CoreConnectString);
            CoreConn.Open();
            var CoreTrans = CoreConn.BeginTransaction();
            try
            {
                var asku = IParam.SkuAuto;
                //更新WmsPile
                string SubPileSql = "SELECT CoID,ID,Qty,WarehouseID,Type,PCode,Skuautoid,SkuID FROM wmspile WHERE CoID = @CoID AND Skuautoid = @Skuautoid AND Type=@Type";
                string AddPileSql = "SELECT CoID,ID,Qty,WarehouseID,Type,PCode,Skuautoid,SkuID FROM wmspile WHERE CoID = @CoID AND ID=@ID";
                var subpileLst = CoreConn.Query<AWmsPileAuto>(SubPileSql, new { CoID = IParam.CoID, Skuautoid = asku.Skuautoid, Type = IParam.Type }, CoreTrans).AsList();
                var addpileLst = CoreConn.Query<AWmsPileAuto>(AddPileSql, new { CoID = IParam.CoID, ID = IParam.PileID }, CoreTrans).AsList();
                if (subpileLst.Count <= 0 || addpileLst.Count <= 0)
                {
                    result.s = -6003;//无此SKU库存信息
                }
                else
                {

                    result = IsExist(asku.BarCode, asku.SkuType, addpileLst[0].Type, IParam.CoID);
                    if (result.s == 1)
                    {
                        subpileLst[0].Qty = subpileLst[0].Qty - asku.Qty;
                        addpileLst[0].Qty = addpileLst[0].Qty + asku.Qty;
                        if (addpileLst[0].Skuautoid > 0 && addpileLst[0].Skuautoid != asku.Skuautoid)
                        {
                            result.s = -1;
                            result.d = "库位Sku不符：" + addpileLst[0].SkuID;
                        }
                        else
                        {
                            if (addpileLst[0].Skuautoid == 0)
                            {
                                addpileLst[0].Skuautoid = asku.Skuautoid;
                                addpileLst[0].SkuID = asku.SkuID;
                            }
                            var UptLst = new List<AWmsPileAuto>();
                            UptLst.Add(subpileLst[0]);
                            UptLst.Add(addpileLst[0]);
                            CoreConn.Execute(UptPileSkuQtySql(), UptLst, CoreTrans);
                            //更新库存数量      
                            var SkuIDLst = new List<int>();
                            SkuIDLst.Add(asku.Skuautoid);
                            CoreConn.Execute(UptInvWaitInQtySql(), new { CoID = IParam.CoID, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                            //更新总库存数量
                            var res = CommHaddle.GetWareCoidList(IParam.CoID.ToString());
                            var CoIDLst = res.d as List<string>;
                            CoreConn.Execute(UptInvMainWaitInQtySql(), new { CoID = IParam.CoID, CoIDLst = CoIDLst, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                            //添加上架操作记录
                            IParam.Type = addpileLst[0].Type;
                            IParam.WarehouseID = addpileLst[0].WarehouseID;
                            IParam.PCode = addpileLst[0].PCode;
                            result = AddWmsLog(IParam);
                            if (result.s == 1)
                            {
                                var logLst = result.d as List<AWmslog>;
                                CoreConn.Execute(APurHaddles.AddWmsLogSql(), logLst, CoreTrans);
                                CoreTrans.Commit();
                                result.d = addpileLst[0].Qty;//返回现有库存数量
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CoreTrans.Rollback();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                CoreTrans.Dispose();
                CoreConn.Close();
            }
            return result;
        }

        /// <summary>
        /// 扫描货位 - 获取货位Sku信息
        /// </summary>
        public static DataResult GetAreaSku(AShelfParam IParam)
        {
            var result = new DataResult(1, null);
            using (var conn = new MySqlConnection(DbBase.CoreConnectString))
            {
                try
                {
                    var data = new AShelfData();
                    var PCodeSql = @" SELECT
                                            wmspile.WarehouseID,
                                            wmspile.Skuautoid,
                                            wmspile.SkuID,
                                            wmspile.PCode,
                                            wmspile.MaxQty,
                                            wmspile.Qty,
                                            wmspile.Type,
                                            wmspile.ID,
                                            wmspile.Order
                                        FROM
                                            wmspile
                                        WHERE                                            
                                            wmspile.PCode = @PCode
                                        AND wmspile.CoID = @CoID";
                    var p = new DynamicParameters();
                    p.Add("@PCode", IParam.PCode);
                    p.Add("@CoID", IParam.CoID);
                    if (IParam.Type > 0)
                    {
                        PCodeSql = PCodeSql + " AND wmspile.Type=@Type";
                        p.Add("@Type", IParam.Type);
                    }
                    if (IParam.TypeLst != null && IParam.TypeLst.Count > 0)
                    {
                        PCodeSql = PCodeSql + " AND wmspile.Type in @TypeLst";
                        p.Add("@TypeLst", IParam.TypeLst);
                    }
                    var PCLst = conn.Query<AWmsPileAuto>(PCodeSql, p).AsList();
                    if (PCLst.Count > 0)
                    {
                        data.PileAuto = PCLst[0];
                        string querysql = "SELECT ID AS Skuautoid,SkuID,SkuName,GoodsCode,Norm,@SkuID AS BarCode FROM coresku WHERE CoID=@CoID AND ID=@SkuID";
                        var SkuLst = conn.Query<ASkuScan>(querysql, new { CoID = IParam.CoID, SkuID = PCLst[0].Skuautoid }).AsList();
                        if (SkuLst.Count > 0)
                        {
                            data.SkuAuto = SkuLst[0];
                        }
                        result.d = data;
                    }
                    else
                    {
                        result.s = -6002;
                    }
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d = e.Message;
                }
            }
            return result;
        }

        /// <summary>
        /// 货品下架 - 更新货位库存（1.2.存储仓-  3.销退仓+）
        /// </summary>
        public static DataResult SetOffShelfPile(AShelfSet IParam)
        {
            var result = new DataResult(1, null);
            var CoreConn = new MySqlConnection(DbBase.CoreConnectString);
            CoreConn.Open();
            var CoreTrans = CoreConn.BeginTransaction();
            try
            {
                var asku = IParam.SkuAuto;
                //存储仓 数量 - 
                string SubPileSql = "SELECT CoID,ID,Qty,LockQty,WarehouseID,Type,PCode,Skuautoid,SkuID FROM wmspile WHERE CoID = @CoID AND Type in (1,2)";
                var p = new DynamicParameters();
                p.Add("@CoID", IParam.CoID);
                if (IParam.PileID > 0)
                {
                    SubPileSql = SubPileSql + "AND ID=@ID";
                    p.Add("@ID", IParam.PileID);
                }
                else if (!string.IsNullOrEmpty(IParam.PCode))
                {
                    SubPileSql = SubPileSql + "AND PCode=@PCode";
                    p.Add("@PCode", IParam.PCode);
                }
                var subpileLst = CoreConn.Query<AWmsPileAuto>(SubPileSql, p, CoreTrans).AsList();
                if (subpileLst.Count <= 0)
                {
                    result.s = -6003;//无此SKU库存信息
                }
                else
                {
                    result = IsPCodeMatch(asku.BarCode, asku.SkuType, subpileLst[0].PCode, IParam.CoID);
                    if (result.s == 1)
                    {
                        //更新WmsPile存储仓- ，销退仓 +
                        subpileLst[0].Qty = subpileLst[0].Qty - asku.Qty;
                        if (subpileLst[0].Qty <= 0)
                        {
                            subpileLst[0].Skuautoid = 0;
                            subpileLst[0].SkuID = "";
                        }
                        if (IParam.BatchID > 0) //更新拣货
                        {
                            subpileLst[0].LockQty = subpileLst[0].LockQty - asku.Qty;//拣货作业，锁定数量同步减少
                            if (subpileLst[0].LockQty < 0)
                            {
                                result.s = -6009;//库存数量不足，暂停拣货
                            }
                            result = ABatchHaddles.BatchOffShelf(IParam, CoreTrans, CoreConn);//更新拣货状态
                        }
                        if (result.s > 0)
                        {
                            var UptLst = new List<AWmsPileAuto>();
                            UptLst.Add(subpileLst[0]);
                            //销退仓 数量 +
                            string AddPileSql = "SELECT CoID,ID,Qty,WarehouseID,Type,PCode,Skuautoid,SkuID FROM wmspile WHERE CoID = @CoID AND Skuautoid = @Skuautoid AND Type=@Type";
                            var addpileLst = CoreConn.Query<AWmsPileAuto>(AddPileSql, new { CoID = IParam.CoID, Skuautoid = asku.Skuautoid, Type = IParam.Type }, CoreTrans).AsList();
                            if (addpileLst.Count > 0)
                            {
                                addpileLst[0].Qty = addpileLst[0].Qty + asku.Qty;
                                UptLst.Add(addpileLst[0]);
                                IParam.WarehouseID = addpileLst[0].WarehouseID;
                                IParam.PCode = addpileLst[0].PCode;
                            }
                            else
                            {
                                result = CommHaddle.GetWhViewAll(IParam.CoID.ToString());
                                var WhViewLst = result.d as List<Warehouse_view>;
                                WhViewLst = WhViewLst.Where(a => a.Type == IParam.Type.ToString()).AsList();
                                var np = new AWmsPile();
                                np.Skuautoid = asku.Skuautoid;
                                np.SkuID = asku.SkuID;
                                np.PCode = "";
                                np.WarehouseID = int.Parse(WhViewLst[0].ID);
                                np.WarehouseName = WhViewLst[0].WhName;
                                np.Type = IParam.Type;
                                np.Qty = asku.Qty;
                                np.Creator = IParam.Creator;
                                np.CreateDate = IParam.CreateDate;
                                np.CoID = IParam.CoID;
                                CoreConn.Execute(APurHaddles.AddWmsPile(), np, CoreTrans);
                                IParam.WarehouseID = int.Parse(WhViewLst[0].ID);
                            }
                            CoreConn.Execute(UptPileSkuQtySql(), UptLst, CoreTrans);//更新pile库存
                                                                                    //更新库存数量      
                            var SkuIDLst = new List<int>();
                            SkuIDLst.Add(asku.Skuautoid);
                            CoreConn.Execute(UptInvSaleRetuQtySql(), new { CoID = IParam.CoID, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                            //更新总库存数量
                            var res = CommHaddle.GetWareCoidList(IParam.CoID.ToString());
                            var CoIDLst = res.d as List<string>;
                            CoreConn.Execute(UptInvMainSaleRetuQtySql(), new { CoID = IParam.CoID, CoIDLst = CoIDLst, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                            //添加下架操作记录     
                            result = AddWmsLog(IParam);
                            if (result.s == 1)
                            {
                                var logLst = result.d as List<AWmslog>;
                                CoreConn.Execute(APurHaddles.AddWmsLogSql(), logLst, CoreTrans);
                                if (IParam.BatchID > 0)//添加拣货记录
                                {
                                    result = ABatchHaddles.AddBatchPicked(IParam, logLst, CoreTrans, CoreConn);//添加拣货记录
                                }
                                if (result.s > 0)
                                {
                                    CoreTrans.Commit();
                                    result.d = subpileLst[0].Qty;//返回现有库存数量
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CoreTrans.Rollback();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                CoreTrans.Dispose();
                CoreConn.Close();
            }
            return result;
        }

        /// <summary>
        /// 托盘上架 - 扫描Sku（1.2.存储仓）
        /// </summary>
        public static DataResult TrayUpScanSku(AShelfParam IParam)
        {
            var result = new DataResult(1, null);
            var cp = new ASkuScanParam();
            cp.CoID = IParam.CoID;
            cp.BarCode = IParam.BoxCode;
            result = ASkuScanHaddles.GetType(cp);
            if (result.s > 0)
            {
                var SkuAuto = result.d as ASkuScan;
                using (var conn = new MySqlConnection(DbBase.CommConnectString))
                {
                    try
                    {
                        result = IsExist(SkuAuto.BarCode, SkuAuto.SkuType, 4, IParam.CoID);
                        if (result.s > 0)
                        {
                            result.d = SkuAuto;
                        }
                    }
                    catch (Exception e)
                    {
                        result.s = -1;
                        result.d = e.Message;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 托盘上架 - 扫描Sku
        /// </summary>
        public static DataResult TrayUp(AShelfSet IParam)
        {
            var result = new DataResult(1, null);
            var CoreConn = new MySqlConnection(DbBase.CoreConnectString);
            CoreConn.Open();
            var CoreTrans = CoreConn.BeginTransaction();
            try
            {
                var asku = IParam.SkuAuto;
                //更新WmsPile
                string SubPileSql = "SELECT CoID,ID,Qty,WarehouseID,Type,PCode,Skuautoid,SkuID FROM wmspile WHERE CoID = @CoID AND Skuautoid = @Skuautoid AND Type=@Type";
                string AddPileSql = "SELECT CoID,ID,Qty,WarehouseID,Type,PCode,Skuautoid,SkuID FROM wmspile WHERE CoID = @CoID AND PCode=@PCode AND Skuautoid = @Skuautoid";
                var subpileLst = CoreConn.Query<AWmsPileAuto>(SubPileSql, new { CoID = IParam.CoID, Skuautoid = asku.Skuautoid, Type = IParam.Type }, CoreTrans).AsList();
                var addpileLst = CoreConn.Query<AWmsPileAuto>(AddPileSql, new { CoID = IParam.CoID, PCode = IParam.PCode, Skuautoid = asku.Skuautoid }, CoreTrans).AsList();
                if (subpileLst.Count <= 0)
                {
                    result.s = -6003;//无此SKU库存信息
                }
                else
                {   //托盘上架 - 有资料Qty+,无资料新增wmspile                    
                    result = IsExist(asku.BarCode, asku.SkuType, 2, IParam.CoID);
                    if (result.s == 1)
                    {
                        int re_qty;
                        var UptLst = new List<AWmsPileAuto>();
                        subpileLst[0].Qty = subpileLst[0].Qty - asku.Qty;
                        UptLst.Add(subpileLst[0]);
                        if (addpileLst.Count > 0)
                        {
                            addpileLst[0].Qty = addpileLst[0].Qty + asku.Qty;
                            UptLst.Add(addpileLst[0]);
                            IParam.WarehouseID = addpileLst[0].WarehouseID;
                            re_qty = addpileLst[0].Qty;
                        }
                        else
                        {
                            result = CommHaddle.GetWhViewAll(IParam.CoID.ToString());
                            var WhViewLst = result.d as List<Warehouse_view>;
                            WhViewLst = WhViewLst.Where(a => a.Type == "1").AsList();//存储藏 - 零数仓
                            var np = new AWmsPile();
                            np.Skuautoid = asku.Skuautoid;
                            np.SkuID = asku.SkuID;
                            np.PCode = IParam.PCode;
                            np.WarehouseID = int.Parse(WhViewLst[0].ID);
                            np.WarehouseName = WhViewLst[0].WhName;
                            np.Type = 1;
                            np.Qty = asku.Qty;
                            np.Creator = IParam.Creator;
                            np.CreateDate = IParam.CreateDate;
                            np.CoID = IParam.CoID;
                            np.PCType = 2;//0.暂存仓;1.固定货位;2.临时货位(托盘，可对应多个Sku)
                            int c1 = CoreConn.Execute(APurHaddles.AddWmsPile(), np, CoreTrans);
                            if (c1 <= 0)
                            {
                                result.s = -1;
                                result.d = "";
                            }
                            IParam.WarehouseID = int.Parse(WhViewLst[0].ID);
                            re_qty = asku.Qty;
                        }
                        CoreConn.Execute(UptPileSkuQtySql(), UptLst, CoreTrans);
                        //更新库存数量      
                        var SkuIDLst = new List<int>();
                        SkuIDLst.Add(asku.Skuautoid);
                        CoreConn.Execute(UptInvWaitInQtySql(), new { CoID = IParam.CoID, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                        //更新总库存数量
                        var res = CommHaddle.GetWareCoidList(IParam.CoID.ToString());
                        var CoIDLst = res.d as List<string>;
                        CoreConn.Execute(UptInvMainWaitInQtySql(), new { CoID = IParam.CoID, CoIDLst = CoIDLst, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                        //添加上架操作记录
                        IParam.Type = 1;
                        result = AddWmsLog(IParam);
                        if (result.s == 1)
                        {
                            var logLst = result.d as List<AWmslog>;
                            int c2 = CoreConn.Execute(APurHaddles.AddWmsLogSql(), logLst, CoreTrans);
                            CoreTrans.Commit();
                            result.d = re_qty;//返回现有库存数量
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CoreTrans.Rollback();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                CoreTrans.Dispose();
                CoreConn.Close();
            }
            return result;
        }

        /// <summary>
        /// 新增库存盘点交易（1.2.存储仓）
        /// </summary>
        public static DataResult SetAreaSkuQty(AShelfSet IParam)
        {
            var result = new DataResult(1, null);
            var CoreConn = new MySqlConnection(DbBase.CoreConnectString);
            CoreConn.Open();
            var CoreTrans = CoreConn.BeginTransaction();
            try
            {
                string PileSql = "SELECT ID,Qty,MaxQty,WarehouseID,Type,PCode,Skuautoid,SkuID FROM wmspile WHERE CoID = @CoID AND ID = @ID AND Type in (1,2)";
                string invsql = "SELECT ID,Skuautoid,StockQty FROM Inventory WHERE CoID=@CoID AND Skuautoid=@Skuautoid";
                var pileLst = CoreConn.Query<AWmsPileAuto>(PileSql, new { CoID = IParam.CoID, ID = IParam.PileID }, CoreTrans).AsList();
                if (IParam.Qty > pileLst[0].MaxQty)
                {
                    result.s = -1;
                    result.d = "超出数量上限" + pileLst[0].MaxQty.ToString();
                }
                else
                {
                    var RecordID = "INV" + CommHaddle.GetRecordID(IParam.CoID);
                    int Status = 1;//(0:待审核;1.审核通过;2.作废)     
                    result = CommHaddle.GetWhViewByID(IParam.CoID, pileLst[0].WarehouseID);
                    if (result.s > 0)
                    {
                        var inv_stock = CoreConn.Query<Sfc_InvStock>(invsql, new { CoID = IParam.CoID, Skuautoid = pileLst[0].Skuautoid }).AsList();
                        var wh_view = result.d as Warehouse_view;
                        var setqty = IParam.Qty - pileLst[0].Qty;//盘点交易数量
                        CoreConn.Execute(UptPileQtySql(), new { CoID = IParam.CoID, ID = IParam.PileID, Qty = IParam.Qty });
                        //新增操作记录
                        var log = new AWmslog();
                        log.BarCode = pileLst[0].SkuID;
                        log.Skuautoid = pileLst[0].Skuautoid;
                        log.SkuID = pileLst[0].SkuID;
                        log.WarehouseID = pileLst[0].WarehouseID;
                        log.PCode = pileLst[0].PCode;
                        log.Qty = setqty;
                        log.Contents = IParam.Contents;
                        log.Type = pileLst[0].Type;
                        log.CoID = IParam.CoID;
                        log.Creator = IParam.Creator;
                        log.CreateDate = IParam.CreateDate;
                        CoreConn.Execute(APurHaddles.AddWmsLogSql(), log, CoreTrans);
                        //盘点主表
                        var main = new Sfc_main();
                        main.CoID = IParam.CoID.ToString();
                        main.Creator = IParam.Creator;
                        main.CreateDate = IParam.CreateDate;
                        main.Type = 2;//1.期初，2.盘点
                        main.Status = Status;
                        main.WhID = wh_view.ID;
                        main.Parent_WhID = wh_view.ParentID;
                        CoreConn.Execute(StockTakeHaddle.AddSfcMainSql(), main, CoreTrans);
                        var MainID = CoreConn.QueryFirst<string>("select LAST_INSERT_ID()", CoreTrans);//获取新增id
                        //盘点子表
                        var item = new Sfc_item();
                        item.ParentID = MainID;
                        item.CoID = IParam.CoID.ToString(); ;
                        item.Creator = IParam.Creator;
                        item.CreateDate = IParam.CreateDate;
                        item.Type = 2;
                        item.WhID = wh_view.ID;
                        item.Parent_WhID = wh_view.ParentID;
                        item.Skuautoid = pileLst[0].Skuautoid;
                        item.InvQty = inv_stock[0].StockQty + setqty;//盘点数量
                        item.Qty = setqty;//交易数量   
                        CoreConn.Execute(StockTakeHaddle.AddSfcItemSql(), item, CoreTrans);
                        //交易主表
                        var inv = new Invinout();
                        inv.RefID = MainID;
                        inv.RecordID = RecordID;
                        inv.Type = 1401;
                        inv.CusType = Enum.GetName(typeof(InvE.InvType), 1401).ToString();//交易类型           ;
                        inv.Status = Status;
                        inv.WhID = wh_view.ParentID;
                        inv.LinkWhID = wh_view.ID;
                        inv.Creator = IParam.Creator;
                        inv.CreateDate = IParam.CreateDate;
                        inv.CoID = IParam.CoID.ToString();
                        CoreConn.Execute(InventoryHaddle.AddInvinoutSql(), inv, CoreTrans);
                        //交易子表
                        var inv_item = new Invinoutitem();
                        inv_item.RefID = MainID;
                        inv_item.IoID = RecordID;
                        inv_item.Type = 1401;
                        inv_item.CusType = Enum.GetName(typeof(InvE.InvType), 1401).ToString();//交易类型     ;
                        inv_item.Status = Status;
                        inv_item.WhID = wh_view.ParentID;
                        inv_item.LinkWhID = wh_view.ID;
                        inv_item.Skuautoid = pileLst[0].Skuautoid;
                        inv_item.Qty = setqty;//交易数量 
                        inv_item.Creator = IParam.Creator;
                        inv_item.CreateDate = IParam.CreateDate;
                        inv_item.CoID = IParam.CoID.ToString();
                        CoreConn.Execute(InventoryHaddle.AddInvinoutitemSql(), inv_item, CoreTrans);
                        //更新库存数量                   
                        var SkuIDLst = new List<int>();
                        SkuIDLst.Add(inv_item.Skuautoid);
                        CoreConn.Execute(InventoryHaddle.UptInvStockQtySql(), new { CoID = IParam.CoID, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                        //获取第三方仓公司ID
                        var res = CommHaddle.GetWareCoidList(IParam.CoID.ToString());
                        var CoIDLst = res.d as List<string>;
                        CoreConn.Execute(InventoryHaddle.UptInvMainStockQtySql(), new { CoID = IParam.CoID, CoIDLst = CoIDLst, SkuIDLst = SkuIDLst, Modifier = IParam.Creator, ModifyDate = IParam.CreateDate }, CoreTrans);
                        CoreTrans.Commit();
                        CoreUser.LogComm.InsertUserLog("修改库存数量-盘点数量", "Inventory", "单据ID" + MainID, IParam.Creator, IParam.CoID, DateTime.Now);
                    }
                }
            }
            catch (Exception e)
            {
                CoreTrans.Rollback();
                result.s = -1;
                result.d = e.Message;
            }
            finally
            {
                CoreTrans.Dispose();
                CoreConn.Close();
            }
            return result;
        }

        /// <summary>
        /// 新增操作记录
        /// </summary>
        public static DataResult AddWmsLog(AShelfSet IParam)
        {
            var result = new DataResult(1, null);
            using (var CoreConn = new MySqlConnection(DbBase.CoreConnectString))
            {
                try
                {
                    var asku = IParam.SkuAuto;
                    var logLst = new List<AWmslog>();
                    if (asku.SkuType < 2)
                    {
                        var log = new AWmslog();
                        log.BarCode = asku.BarCode;
                        log.Skuautoid = asku.Skuautoid;
                        log.SkuID = asku.SkuID;
                        log.WarehouseID = IParam.WarehouseID;
                        log.PCode = IParam.PCode;
                        log.Qty = asku.Qty;
                        log.Contents = IParam.Contents;
                        log.Type = IParam.Type;
                        log.CoID = IParam.CoID;
                        log.Creator = IParam.Creator;
                        log.CreateDate = IParam.CreateDate;
                        logLst.Add(log);
                    }
                    else
                    {
                        string boxsql = "SELECT BarCode,Skuautoid,SkuID,BoxCode,Qty FROM wmsbox WHERE CoID=@CoID AND BoxCode = @BoxCode";
                        var BoxSkuLst = CoreConn.Query<AWmsBox>(boxsql, new { CoID = IParam.CoID, BoxCode = asku.BarCode }).AsList();
                        if (BoxSkuLst.Count > 0)
                        {
                            var logLstA = BoxSkuLst.Select(a => new AWmslog
                            {
                                BarCode = a.BarCode,
                                Skuautoid = a.Skuautoid,
                                SkuID = a.SkuID,
                                BoxCode = a.BoxCode,
                                WarehouseID = IParam.WarehouseID,
                                PCode = IParam.PCode,
                                Qty = a.Qty,
                                Contents = IParam.Contents,
                                Type = IParam.Type,
                                CoID = IParam.CoID,
                                Creator = IParam.Creator,
                                CreateDate = IParam.CreateDate
                            }).AsList();
                            logLst.AddRange(logLstA);
                        }
                    }
                    result.d = logLst;
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d = e.Message;
                }
            }
            return result;
        }

        public static DataResult IsExist(string BarCode, int SkuType, int Type, int CoID)
        {
            var result = new DataResult(1, null);
            using (var CoreConn = new MySqlConnection(DbBase.CoreConnectString))
            {
                try
                {
                    if (SkuType != 1)
                    {
                        string sql = "SELECT PCode,WarehouseID,Contents,Type FROM wmslog WHERE CoID=@CoID ";
                        if (SkuType == 0)//0.件码(唯一码)||1.普通Sku||2.箱码
                        {
                            sql = sql + " AND BarCode=@BarCode ORDER BY ID DESC LIMIT 1";
                        }
                        else if (SkuType == 2)
                        {
                            sql = sql + " AND BoxCode=@BarCode ORDER BY ID DESC LIMIT 1";
                        }
                        var p = new DynamicParameters();
                        p.Add("@CoID", CoID);
                        p.Add("@BarCode", BarCode);
                        var logLst = CoreConn.Query<AWmslog>(sql, p).AsList();
                        if (logLst.Count > 0 && logLst[0].Type != Type && logLst[0].Type != 4)
                        {
                            result.s = -1;
                            result.d = "条码已" + logLst[0].Contents;
                        }
                    }
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d = e.Message;
                }
            }
            return result;
        }

        public static DataResult IsPCodeMatch(string BarCode, int SkuType, string PCode, int CoID)
        {
            var result = new DataResult(1, null);
            using (var CoreConn = new MySqlConnection(DbBase.CoreConnectString))
            {
                try
                {
                    if (SkuType != 1)
                    {
                        string sql = "SELECT PCode,WarehouseID,Contents FROM wmslog WHERE CoID=@CoID ";
                        if (SkuType == 0)//0.件码(唯一码)||1.普通Sku||2.箱码
                        {
                            sql = sql + " AND BarCode=@BarCode ORDER BY ID DESC LIMIT 1";
                        }
                        else if (SkuType == 2)
                        {
                            sql = sql + " AND BoxCode=@BarCode ORDER BY ID DESC LIMIT 1";
                        }
                        var p = new DynamicParameters();
                        p.Add("@CoID", CoID);
                        p.Add("@BarCode", BarCode);
                        var logLst = CoreConn.Query<AWmslog>(sql, p).AsList();
                        if (logLst.Count > 0 && logLst[0].PCode != PCode)
                        {
                            result.s = -1;
                            result.d = "Sku库位不符:" + logLst[0].PCode;
                        }
                    }
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d = e.Message;
                }
            }
            return result;
        }

        /// <summary>
        /// 检查货位库存
        /// </summary>
        // public static DataResult CheckWhPCode(AShelfParam IParam)
        // {
        //     var result = new DataResult(1, null);
        //     using (var conn = new MySqlConnection(DbBase.CoreConnectString))
        //     {
        //         try
        //         {
        //             var res = GetUpPCode(IParam);
        //             if (res.s < 0)
        //             {
        //                 result.s = res.s;
        //                 result.d = res.d;
        //             }
        //             else
        //             {
        //                 var dic = new Dictionary<string, string>();

        //             }
        //         }
        //         catch (Exception e)
        //         {
        //             result.s = -1;
        //             result.d = e.Message;
        //         }
        //     }
        //     return result;
        // }



        public static string UptPileQtySql()
        {
            string sql = "UPDATE wmspile SET Qty=@Qty WHERE CoID=@CoID AND ID=@ID";
            return sql;
        }
        public static string UptPileSkuQtySql()
        {
            string sql = "UPDATE wmspile SET Skuautoid=@Skuautoid ,SkuID=@SkuID, Qty=@Qty WHERE CoID=@CoID AND ID=@ID";
            return sql;
        }
        #region 更新仓库待入库存
        public static string UptInvWaitInQtySql()
        {
            string sql = @"UPDATE inventory 
                            SET WaitInQty = (SELECT IFNULL(SUM(Qty),0)
                                                FROM wmspile
                                                WHERE wmspile.Skuautoid = inventory.Skuautoid
                                                AND wmspile.CoID = inventory.CoID
                                                AND wmspile.Skuautoid in @SkuIDLst
                                                AND wmspile.Type = 4),
                                            IsDelete=0,Modifier=@Modifier,ModifyDate=@ModifyDate
                                            WHERE
	                                            inventory.CoID =@CoID
                                            AND Inventory.Skuautoid in @SkuIDLst";
            return sql;
        }
        public static string UptInvMainWaitInQtySql()
        {
            string sql = @"UPDATE inventory_sale
                            SET inventory_sale.WaitInQty = (
                                SELECT          
                                    IFNULL(SUM(WaitInQty),0)
                                FROM
                                    inventory
                                WHERE
                                    inventory.CoID IN @CoIDLst
                                AND inventory.Skuautoid = inventory_sale.Skuautoid
                            ),IsDelete=0,Modifier=@Modifier,ModifyDate=@ModifyDate
                            WHERE inventory_sale.CoID=@CoID
                            AND inventory_sale.Skuautoid in @SkuIDLst";
            return sql;
        }
        #endregion


        #region 更新销退仓库存

        public static string UptInvSaleRetuQtySql()
        {
            string sql = @"UPDATE inventory 
                            SET SaleRetuQty = (SELECT IFNULL(SUM(Qty),0)
                                                FROM wmspile
                                                WHERE wmspile.Skuautoid = inventory.Skuautoid
                                                AND wmspile.CoID = inventory.CoID
                                                AND wmspile.Skuautoid in @SkuIDLst
                                                AND wmspile.Type = 3),
                                            IsDelete=0,Modifier=@Modifier,ModifyDate=@ModifyDate
                                            WHERE
	                                            inventory.CoID =@CoID
                                            AND Inventory.Skuautoid in @SkuIDLst";
            return sql;
        }



        public static string UptInvMainSaleRetuQtySql()
        {
            string sql = @"UPDATE inventory_sale
                            SET inventory_sale.SaleRetuQty = (
                                SELECT
                                    IFNULL(SUM(SaleRetuQty),0)
                                FROM
                                    inventory
                                WHERE
                                    inventory.CoID IN @CoIDLst
                                AND inventory.Skuautoid = inventory_sale.Skuautoid
                            ),IsDelete=0,Modifier=@Modifier,ModifyDate=@ModifyDate
                            WHERE inventory_sale.CoID=@CoID
                            AND inventory_sale.Skuautoid in @SkuIDLst";
            return sql;
        }
        #endregion
    }
}


