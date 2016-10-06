using System.Collections.Generic;
using System.Linq;
using CoreModels;
using CoreModels.XyUser;
using Dapper;
using MySql.Data.MySqlClient;
using System;
using Newtonsoft.Json;

namespace CoreData.CoreUser
{
    public static class AdminHaddle
    {
  
      

        public static bool isMenuExist(string name){
            bool flag = true;
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
                    string sql = "SELECT ID FROM menus WHERE menus.deleted = FALSE AND menus.`Name` = '"+name+"'; ";
                    int rnt = conn.Query<int>(sql).AsList()[0];
                    if(rnt > 0){
                        flag = true;
                    }else{
                        flag = false;
                    }
                }
                catch
                {
                    flag = false;
                    conn.Dispose();
                }
            }


            return flag;
        }


          ///<summary>
        ///获取菜单列表
        ///</summary>
        public static DataResult GetMenuList(string roleid, string coid)
        {
            var s = 1;
            var cname = "menus" + coid + roleid;

            //获取菜单缓存
            // var parent = CacheBase.Get<List<Menu>>(cname);
            // if (parent == null)
            // {
            try{
                var parent = JsonConvert.DeserializeObject<List<MenuSimple>>(JsonConvert.SerializeObject(GetMenu(roleid, coid)));
                if (parent == null)
                {
                    s = -2004;
                }
                else
                {
                    //无缓存，添加缓存
                    //CacheBase.Set<List<Menu>>(cname, parent);
                }
                return new DataResult(s, s == 1 ? parent : null);
            }catch(Exception ex){
                Console.WriteLine(ex.Message);
                return new DataResult(s,  ex.Message);
            }
                


            //}
            
        }
        ///<summary>
        ///获取菜单列表数据
        ///</summary>
        public static List<Menu> GetMenu(string roleid, string coid)
        {
            var parent = new List<Menu>();
            using (var conn = new MySqlConnection(DbBase.UserConnectString))
            {
                try
                {
                    //获取权限列表
                    var role = UserHaddle.GetRole(roleid, coid);
                    if (role.s > 1) return null;
                    var r = role.d as Role;
                    //"select name,NewIcon,NewIconPre,NavigateUrl,ParentID from menus where viewpowerid in (" + r.ViewList + ") order by ParentID,sortindex"
                    string sql = "select menus.id, menus.`Name` as `name`, NewIcon,NewIconPre,NewUrl as router,SortIndex as `order`, menus.Remark, ParentID ,power.Title as access from menus "+
                                "LEFT JOIN power on power.ID = menus.ViewPowerID where deleted = FALSE AND viewpowerid in (" + r.ViewList + ") order by ParentID,sortindex"; 

                    var child = conn.Query<Menu>(sql).AsList();
                    if (child.Count == 0){ return null;}
                    foreach (var c in child)
                    {
                        
                         c.icon = new string[]{c.NewIcon,c.NewIconPre};                                                 
                    }
                    if (child.Count == 0)
                    {
                        return null;
                    }
                    var pidarray = (from c in child select c.parentid).Distinct().ToArray();
                    var pid = string.Join(",", pidarray);
                    //"select id,name,NewIcon,NewIconPre,NavigateUrl,ParentID from menus where id in (" + pid + ") order by sortindex"
                    sql = "select menus.id, menus.`Name` as `name`,NewIcon,NewIconPre,NewUrl as router,SortIndex as `order`, menus.Remark, ParentID ,power.Title as access from menus "+
                                "LEFT JOIN power on power.ID = menus.ViewPowerID where deleted = FALSE AND  menus.id in (" + pid + ") order by sortindex"; 
                    
                    parent = conn.Query<Menu>(sql).AsList();

                    foreach (var p in parent)
                    {                        
                        p.icon = new string[]{p.NewIcon,p.NewIconPre};                    
                        p.children = (from c in child where c.parentid == p.id select c).ToList();
                    }
                }
                catch
                {
                    conn.Dispose();
                    return null;
                }
            }
            return parent ;
        }

        public static DataResult CreatMenu(string name,string router,string[] iconArr,string order,string remark,string parentid,string accessid,string uname,string coid){            
            var result = new DataResult(1,null);
            if(isMenuExist(name)){
                result.s = -2017;
            }else{
                using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                    try
                    {                                        
                        string iconfont = !string.IsNullOrEmpty(iconArr[1]) ? "menus.NewIcon='"+iconArr[0]+"',menus.NewIconPre='"+iconArr[1]+"'": "menus.NewIcon='"+iconArr[0]+"',menus.NewIconPre=''";
                        string sql = "INSERT menus SET menus.`Name`='"+name+"',menus.NewUrl='"+router+"',"+iconfont+","+
                                    "menus.SortIndex='"+order+"',menus.Remark='"+remark+"',menus.ParentID="+parentid+",menus.ViewPowerID="+accessid;
                        Console.WriteLine(sql);                                    
                        int rnt = conn.Execute(sql);
                        if(rnt > 0){
                            result.s = 1;
                        }else{
                            result.s = -2014;
                        }
                        //LogComm.InsertUserLog("新增菜单", "menu", "菜单名"+name ,uname, coid, DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        result.s = -1;
                        result.d= e.Message; 
                        conn.Dispose();
                    }
                }
            }
            
            return result;
        }

  
        public static DataResult modifyMenu(string id,string name,string router,string[] iconArr,string order,string remark,string parentid,string accessid,string uname,string coid){
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
                    
                    string iconfont = !string.IsNullOrEmpty(iconArr[1]) ? "menus.NewIcon='"+iconArr[0]+"',menus.NewIconPre='"+iconArr[1]+"'": "menus.NewIcon='"+iconArr[0]+"',menus.NewIconPre=''";                    
                    string sql = "Update menus SET menus.`Name`='"+name+"',menus.NewUrl='"+router+"',"+iconfont+","+
                                 "menus.SortIndex='"+order+"',menus.Remark='"+remark+"',menus.ParentID="+parentid+" WHERE menus.id="+id;//,menus.ViewPowerID="+accessid+"
                    Console.WriteLine(sql);             
                    int rnt = conn.Execute(sql);
                    if(rnt > 0){
                        result.s = 1;
                    }else{
                        result.s = -2016;
                    }
                    //LogComm.InsertUserLog("编辑菜单", "menu", "菜单名"+name ,uname, coid, DateTime.Now);
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d= e.Message; 
                    conn.Dispose();
                }
            }
            return result;
        }

        public static DataResult GetMenuById(int id){
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
                   string sql = "select menus.id, menus.`Name` as `name`,NewIcon,NewIconPre,NewUrl as router,SortIndex as `order`, menus.Remark, ParentID ,power.Title as access from menus "+ 
                                "LEFT JOIN power on power.ID = menus.ViewPowerID where menus.ID ="+id;
                                Console.WriteLine(sql);
                   var data = conn.Query<Menu>(sql).AsList()[0];
                   if(data!=null){                       
                       result.d = new {
                           id = data.id,
                           name = data.name,
                           router = data.router,
                           access = data.access,
                           order = data.order,
                           remark = data.remark,
                           parentid = data.parentid,
                           icon = new string[]{data.NewIcon,data.NewIconPre}
                       };
                   }else{
                       result.s = -2016;
                   }                                           
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d= e.Message; 
                    conn.Dispose();
                }
            }
            return result;
         }
         public static DataResult DelMenuById(string ids){
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
                   string sql = "";
                   var idArr = ids.Split(',');
                   foreach(string id in idArr){
                       sql += "UPDATE menus SET menus.deleted = TRUE WHERE menus.ID ="+id+";";
                   }

                   int rnt = conn.Execute(sql);
                   if(rnt>0){
                       result.s = 1;
                   }else{
                       result.s = -2016;
                   }                                           
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d= e.Message; 
                    conn.Dispose();
                }
            }
            return result;
        }

        
        //权限列表
        public static DataResult getPowerList(powerParam param){
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
                     string wheresql = " 1=1 ";
                    string totalsql = ""; 
                    var totallist = new List<Power>();
                    if(!string.IsNullOrEmpty(param.Filter)){
                        wheresql += " and (Name like '%"+ param.Filter +"%' "+
                                    " or GroupName like '%"+ param.Filter +"%' "+
                                    " or Title like '%"+ param.Filter +"%')";
                    }   
                    if(!string.IsNullOrEmpty(param.SortField)&& !string.IsNullOrEmpty(param.SortDirection))//排序
                    {
                        wheresql += " ORDER BY "+param.SortField +" "+ param.SortDirection;
                    }
                    if(param.page == 1){//pageindex 不为 1 时，不再传total 
                        totalsql = "SELECT * FROM power  WHERE  "+wheresql;
                        totallist = conn.Query<Power>(totalsql).AsList();
                    }
                                    
                    if(param.page>-1&&param.PageSize>-1){
                        wheresql += " limit "+(param.page -1)*param.PageSize +" ,"+ param.page*param.PageSize;
                    }

                    wheresql ="SELECT * FROM power  WHERE  "+wheresql; 

                    var list = conn.Query<Power>(wheresql).AsList();

                    if (list != null)
                    {
                        if(param.page == 1){                                    
                            result.d = new {
                                list = list,
                                page = param.page,
                                pageSize = param.PageSize,
                                pageTotal =  Math.Ceiling(decimal.Parse(totallist.Count.ToString())/decimal.Parse(param.PageSize.ToString())),
                                total = totallist.Count
                            };
                        }else{                       
                            result.d = new {
                                list = list,
                                page = param.page,
                            };
                        }                                          
                    }
                    else
                    {
                        result.s = -2015;
                    }                        
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d= e.Message; 
                    conn.Dispose();
                }
            }
            return result;
        }

        public static DataResult GetViewPowerList(){
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
                    string sql = "SELECT A.ID,A.Title FROM power A WHERE TYPE = 0 AND NOT EXISTS (SELECT 1 FROM menus B WHERE B.ViewPowerID = A.ID)";
                    result.d = conn.Query<ViewPower>(sql).AsList();
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d= e.Message; 
                    conn.Dispose();
                }
            }
            return result;
        }

        public static DataResult GetViewPowerListEdit(int ViewPowerID){
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
                    string sql = "SELECT A.ID,A.Title FROM power A WHERE TYPE = 0 AND NOT EXISTS (SELECT 1 FROM menus B WHERE B.ViewPowerID = A.ID AND B.VIEWPOWERID != "+ViewPowerID+")";
                    result.d = conn.Query<ViewPower>(sql).AsList();
                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d= e.Message; 
                    conn.Dispose();
                }
            }
            return result;
        }

        

        public static DataResult demo(int sys_id){
            var result = new DataResult(1,null);
            using(var conn = new MySqlConnection(DbBase.UserConnectString) ){
                try
                {
        

                }
                catch (Exception e)
                {
                    result.s = -1;
                    result.d= e.Message; 
                    conn.Dispose();
                }
            }
            return result;
        }

  
  
  
  
  
    }
}