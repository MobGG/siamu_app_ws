using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public class Class1
{
    public string cn()
    {
        string con = ConfigurationManager.ConnectionStrings["sqlCon"].ToString();
        return con;
    }

    public void getDt(DataSet ds, string sql, string dtName)
    {
        SqlDataAdapter da = new SqlDataAdapter(sql, cn());
        da.Fill(ds, dtName);
    }

    public void updateToDB(DataSet ds, string dtName, string tblName)
    {
        string sql = "SELECT * FROM " + tblName;
        SqlDataAdapter da = new SqlDataAdapter(sql, cn());
        SqlCommandBuilder cb = new SqlCommandBuilder(da);
        da.Update(ds, dtName);
    }
}