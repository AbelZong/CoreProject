using Microsoft.AspNetCore.Mvc;
using CoreData.CoreUser;
using Newtonsoft.Json.Linq;
using System;
using CoreModels.XyUser;
using System.Collections.Generic;

namespace CoreWebApi
{
     public class AdminController : ControllBase
     {
         [HttpGetAttribute("/core/admin/menus")]
         public ResponseResult getmenus()
         {
            //if(!checkIsAdmin() ){ return CoreResult.NewResponse(-1008, null, "Basic");}  
            var roleid = GetRoleid();
            var coid = GetCoid();        
            var m = AdminHaddle.GetMenuList(roleid, coid);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }

         //创建菜单
         [HttpPostAttribute("/core/admin/createmenus")]
         public ResponseResult createmenus([FromBodyAttribute]JObject lo)
         {

             //Menus menus = Newtonsoft.Json.JsonConvert.DeserializeObject<Menus>(lo.ToString());
             var name = lo["router"]!= null ? lo["name"].ToString():"";
             var router = lo["router"]!= null ? lo["router"].ToString():"";
             string iconName = lo["iconName"]!=null ? lo["iconName"].ToString():"";
             string iconPrefix = lo["iconPrefix"]!=null ? lo["iconPrefix"].ToString():"";
             var icon = new string[]{iconName,iconPrefix};
             var order = lo["order"]!= null ? lo["order"].ToString():"";
             var remark = lo["remark"]!= null ?lo["remark"].ToString():"";
             var parentid = lo["pid"]!= null ? lo["pid"].ToString():"0";
             var accessid = lo["accessid"]!= null ? lo["accessid"].ToString():"0";

             var uname = GetUname();
             var coid = GetCoid();        
             var m = AdminHaddle.CreatMenu(name,router,icon,order,remark,parentid,accessid,uname,coid);
             return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }
         //编辑菜单
         [HttpPostAttribute("/core/admin/modifymenus")]
         public ResponseResult modifymenus([FromBodyAttribute]JObject lo)
         {
             //Menus menus = Newtonsoft.Json.JsonConvert.DeserializeObject<Menus>(lo.ToString());
             var id = lo["id"].ToString();
             var name = lo["name"]!= null ? lo["name"].ToString():"";
             var router = lo["router"]!= null ? lo["router"].ToString():"";
             string iconName = lo["iconName"]!=null ? lo["iconName"].ToString():"";
             string iconPrefix = lo["iconPrefix"]!=null ? lo["iconPrefix"].ToString():"";
             var icon = new string[]{iconName,iconPrefix};
             var order = lo["order"]!= null ? lo["order"].ToString():"";
             var remark = lo["remark"]!= null ?lo["remark"].ToString():"";
             var parentid = lo["pid"]!= null ? lo["pid"].ToString():"";
             var accessid = lo["accessid"]!= null ? lo["accessid"].ToString():"0";

             var uname = GetUname();
             var coid = GetCoid();        
             var m = AdminHaddle.modifyMenu(id,name,router,icon,order,remark,parentid,accessid,uname,coid);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }

         //获取单条菜单
         [HttpGetAttribute("/core/admin/onemenu")]
         public ResponseResult onemenu(int id){
             var m = AdminHaddle.GetMenuById(id);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");  
         }
         
         //删除菜单
         [HttpPostAttribute("/core/admin/delmenus")]
         public ResponseResult delmenu([FromBodyAttribute]JObject lo){
            string ids = String.Join(",",lo["ids"]); 
            string coid = GetCoid();
            var m = AdminHaddle.DelMenuById(ids,coid);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");  
         }

         //获取权限列表
         [HttpGetAttribute("/core/admin/power")]
         public ResponseResult power(string Filter = "",int Page = 1,int PageSize = 20)
         {

            powerParam param = new powerParam();
            param.Filter = Filter;            
            param.page = Math.Max(Page,1);
            param.PageSize = Math.Max(PageSize,20);
            var m = AdminHaddle.getPowerList(param);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }
         //获取单个权限
         [HttpGetAttribute("/core/admin/powerQuery")]
         public ResponseResult powerQuery(string aid)
         {    
            var m = AdminHaddle.getPowerById(aid);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }
        
         //获取可用权限
         [HttpGetAttribute("/core/admin/GetViewPowerList")]
         public ResponseResult GetViewPowerList()
         {
            var m = AdminHaddle.GetViewPowerList();
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }

         //编辑菜单时获取可用权限
         [HttpGetAttribute("/core/admin/GetViewPowerListEdit")]
         public ResponseResult GetViewPowerListEdit(int ViewPowerID)
         {
            var m = AdminHaddle.GetViewPowerListEdit(ViewPowerID);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }

         //增加权限
         [HttpPostAttribute("/core/admin/createaccess")]
         public ResponseResult createaccess([FromBodyAttribute]JObject lo)
         {
            var power = Newtonsoft.Json.JsonConvert.DeserializeObject<Power>(lo.ToString());
            var m = AdminHaddle.insertPower(power);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }

         //编辑权限
         [HttpPostAttribute("/core/admin/modifyaccess")]
         public ResponseResult modifyaccess([FromBodyAttribute]JObject lo)
         {
            var power = Newtonsoft.Json.JsonConvert.DeserializeObject<Power>(lo.ToString());
            var m = AdminHaddle.upDatePower(power);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }
         
         //删除权限
         [HttpPostAttribute("/core/admin/delaccess")]
         public ResponseResult delaccess([FromBodyAttribute]JObject lo)
         {
            var ids = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(lo["IDLst"].ToString());
            var m = AdminHaddle.delpower(ids);
            return CoreResult.NewResponse(m.s, m.d, "Indentity");           
         }












     }

}