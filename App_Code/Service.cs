using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

[WebService(Namespace = "http://siamUMapService.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Service : System.Web.Services.WebService
{
    string sql;
    Class1 c1 = new Class1();
    String datePattern = "dd/MM/yyyy";
    String dateTimePattern = "dd/MM/yyyy HH:mm:ss";
    IFormatProvider locale = new System.Globalization.CultureInfo("en-US", true);

    public DateTime parseDate(String str, String pattern)
    {
        return DateTime.ParseExact(str, pattern, locale);
    }

    [WebMethod]
    public List<Topic> findTopicByCriteria(String title, String buildingNo, String strStartDate, String strEndDate)
    {
        strStartDate = String.IsNullOrEmpty(strStartDate) ? "01/01/1753" : strStartDate;
        strEndDate = String.IsNullOrEmpty(strEndDate) ? "31/12/9999" : strEndDate;

        DateTime startDate = parseDate(strStartDate, datePattern);
        DateTime endDate = parseDate(strEndDate, datePattern);

        DataSet ds = new DataSet();
        sql = "SELECT TopicNo, Title, Picture, DateTime, UserID, BuildingNo FROM tblTopic WHERE StatusID = 'ST1' AND Title LIKE @titleParam AND (DateTime >= @startDate AND DateTime <= @endDate)";

        if (!String.IsNullOrEmpty(buildingNo))
        {
            sql += " AND BuildingNo = @buildingNoParam";
        }

        // add order by 
        sql += " ORDER BY DateTime DESC";

        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        SqlParameter titleParam = dataAdapter.SelectCommand.Parameters.Add("@titleParam", SqlDbType.VarChar, 50);
        titleParam.Value = "%" + title.Trim() + "%";

        DateTime sDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0); //10:15:30 AM
        SqlParameter startDateParam = dataAdapter.SelectCommand.Parameters.Add("@startDate", SqlDbType.DateTime);
        startDateParam.Value = sDate;

        DateTime eDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 23, 59, 59); //10:15:30 AM
        SqlParameter endDateParam = dataAdapter.SelectCommand.Parameters.Add("@endDate", SqlDbType.DateTime);
        endDateParam.Value = eDate;

        if (!String.IsNullOrEmpty(buildingNo))
        {
            SqlParameter buildingNoParam = dataAdapter.SelectCommand.Parameters.Add("@buildingNoParam", SqlDbType.SmallInt);
            buildingNoParam.Value = Convert.ToInt16(buildingNo);
        }

        dataAdapter.Fill(ds, "Topic");

        DataRow[] drs = ds.Tables["Topic"].Select();
        List<Topic> topicList = new List<Topic>();
        foreach (DataRow dr in drs)
        {
            Topic topic = new Topic();
            topic.topicNo = DBNull.Value.Equals(dr["TopicNo"]) ? null : dr["TopicNo"].ToString();
            topic.title = DBNull.Value.Equals(dr["Title"]) ? null : dr["Title"].ToString();
            topic.picture = DBNull.Value.Equals(dr["Picture"]) ? null : (byte[])dr["Picture"];

            topic.dateTime = DBNull.Value.Equals(dr["DateTime"]) ? null : dr["DateTime"].ToString();
            if (!DBNull.Value.Equals(dr["DateTime"]))
                topic.dateTime = ((DateTime)dr["DateTime"]).ToString(dateTimePattern, locale);

            topic.userId = DBNull.Value.Equals(dr["UserID"]) ? null : dr["UserID"].ToString();
            topic.buildingNo = DBNull.Value.Equals(dr["BuildingNo"]) ? null : dr["BuildingNo"].ToString();
            topicList.Add(topic);
        }

        return topicList;
    }

    [WebMethod]
    public Topic findOneTopic(String topicId)
    {
        DataSet ds = new DataSet();
        sql = "SELECT * FROM tblTopic WHERE TopicNo = @topicIdParam";

        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        SqlParameter topicIdParam = dataAdapter.SelectCommand.Parameters.Add("@topicIdParam", SqlDbType.VarChar, 10);
        topicIdParam.Value = topicId;

        dataAdapter.Fill(ds, "Topic");

        Topic topic = null;
        if (ds.Tables["Topic"].Select().Length > 0)
        {
            DataRow dr = ds.Tables["Topic"].Rows[0];
            topic = new Topic();
            topic.topicNo = DBNull.Value.Equals(dr["TopicNo"]) ? null : dr["TopicNo"].ToString();
            topic.title = DBNull.Value.Equals(dr["Title"]) ? null : dr["Title"].ToString();
            topic.description = DBNull.Value.Equals(dr["Description"]) ? null : dr["Description"].ToString();
            topic.picture = DBNull.Value.Equals(dr["Picture"]) ? null : (byte[])dr["Picture"];

            if (!DBNull.Value.Equals(dr["DateTime"]))
                topic.dateTime = ((DateTime)dr["DateTime"]).ToString(dateTimePattern, locale);

            topic.statusId = DBNull.Value.Equals(dr["StatusID"]) ? null : dr["StatusID"].ToString();
            topic.userId = DBNull.Value.Equals(dr["UserID"]) ? null : dr["UserID"].ToString();
            topic.buildingNo = DBNull.Value.Equals(dr["BuildingNo"]) ? null : dr["BuildingNo"].ToString();
        }
        return topic;
    }

    [WebMethod]
    public bool createTopic(String title, String description, byte[] picture, String strDateTime, String userId, Int32 buildingNo)
    {
        DataSet ds = new DataSet();

        DateTime dateTime = parseDate(strDateTime, dateTimePattern);

        try
        {
            String newTopicId;
            sql = "SELECT * FROM tblTopic";
            c1.getDt(ds, sql, "Topic");

            sql = "SELECT MAX(CONVERT(INT, REPLACE(TopicNo,'T',''))) as TopicNo FROM tblTopic";
            c1.getDt(ds, sql, "LastTopic");

            if (DBNull.Value.Equals(ds.Tables["LastTopic"].Rows[0][0]))
            {
                newTopicId = "T1";
            }
            else
            {
                newTopicId = "T" + (Convert.ToInt32(ds.Tables["LastTopic"].Rows[0][0]) + 1);
            }

            DataRow dr = ds.Tables["Topic"].NewRow();
            dr["TopicNo"] = newTopicId;

            dr["Title"] = title;
            dr["Description"] = description;
            dr["Picture"] = picture;
            dr["DateTime"] = dateTime;
            dr["ReportCount"] = 0;
            dr["StatusID"] = "ST1";
            dr["UserID"] = userId;
            dr["BuildingNo"] = buildingNo;

            ds.Tables["Topic"].Rows.Add(dr);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }

        c1.updateToDB(ds, "Topic", "tblTopic");
        return true;
    }

    [WebMethod]
    public List<People> findPeopleByCriteria(String name, String facultyId, String departmentId)
    {
        DataSet ds = new DataSet();

        sql = "SELECT DISTINCT UserID, Name, Picture, RoomNo, tblFaculty.FacultyName, DepartmentName FROM tblUser ";
        sql += "INNER JOIN tblFaculty ON tblUser.FacultyID = tblFaculty.FacultyID ";
        sql += "INNER JOIN tblDepartment ON tblUser.DepartmentID = tblDepartment.DepartmentID ";
        sql += "WHERE Name LIKE @nameParam AND tblUser.PositionID IS NOT NULL AND tblUser.PositionID <> 'R1' AND tblUser.StatusID <> 'SU3'";

        if (!String.IsNullOrEmpty(facultyId))
        {
            sql += " AND tblFaculty.FacultyID = @facultyId";
        }

        if (!String.IsNullOrEmpty(departmentId))
        {
            sql += " AND tblDepartment.DepartmentID = @departmentId";
        }

        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        SqlParameter nameParam = dataAdapter.SelectCommand.Parameters.Add("@nameParam", SqlDbType.VarChar, 50);
        nameParam.Value = "%" + name.Trim() + "%";

        if (!String.IsNullOrEmpty(facultyId))
        {
            SqlParameter facultyIdParam = dataAdapter.SelectCommand.Parameters.Add("@facultyId", SqlDbType.VarChar, 3);
            facultyIdParam.Value = facultyId;
        }

        if (!String.IsNullOrEmpty(departmentId))
        {
            SqlParameter departmentIdParam = dataAdapter.SelectCommand.Parameters.Add("@departmentId", SqlDbType.VarChar, 3);
            departmentIdParam.Value = departmentId;
        }

        dataAdapter.Fill(ds, "People");

        DataRow[] drs = ds.Tables["People"].Select();
        List<People> peopleList = new List<People>();
        foreach (DataRow dr in drs)
        {
            People people = new People();
            people.userId = DBNull.Value.Equals(dr["UserID"]) ? null : dr["UserID"].ToString();
            people.name = DBNull.Value.Equals(dr["Name"]) ? null : dr["Name"].ToString();
            people.picture = DBNull.Value.Equals(dr["Picture"]) ? null : (byte[])dr["Picture"];
            people.roomNo = DBNull.Value.Equals(dr["RoomNo"]) ? null : dr["RoomNo"].ToString();
            people.facultyName = DBNull.Value.Equals(dr["FacultyName"]) ? null : dr["FacultyName"].ToString();
            people.departmentName = DBNull.Value.Equals(dr["DepartmentName"]) ? null : dr["DepartmentName"].ToString();
            peopleList.Add(people);
        }

        return peopleList;
    }

    [WebMethod]
    public List<People> findStaffByCriteria(String name, String facultyId, String departmentId)
    {
        DataSet ds = new DataSet();

        sql = "SELECT DISTINCT UserID, Name, Picture, RoomNo, tblFaculty.FacultyName, DepartmentName FROM tblUser ";
        sql += "INNER JOIN tblFaculty ON tblUser.FacultyID = tblFaculty.FacultyID ";
        sql += "INNER JOIN tblDepartment ON tblUser.DepartmentID = tblDepartment.DepartmentID ";
        sql += "WHERE Name LIKE @nameParam AND tblUser.PositionID = 'R1' AND tblUser.StatusID <> 'SU3'";

        if (!String.IsNullOrEmpty(facultyId))
        {
            sql += " AND tblFaculty.FacultyID = @facultyId";
        }

        if (!String.IsNullOrEmpty(departmentId))
        {
            sql += " AND tblDepartment.DepartmentID = @departmentId";
        }

        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        SqlParameter nameParam = dataAdapter.SelectCommand.Parameters.Add("@nameParam", SqlDbType.VarChar, 50);
        nameParam.Value = "%" + name.Trim() + "%";

        if (!String.IsNullOrEmpty(facultyId))
        {
            SqlParameter facultyIdParam = dataAdapter.SelectCommand.Parameters.Add("@facultyId", SqlDbType.VarChar, 3);
            facultyIdParam.Value = facultyId;
        }

        if (!String.IsNullOrEmpty(departmentId))
        {
            SqlParameter departmentIdParam = dataAdapter.SelectCommand.Parameters.Add("@departmentId", SqlDbType.VarChar, 3);
            departmentIdParam.Value = departmentId;
        }

        dataAdapter.Fill(ds, "People");

        DataRow[] drs = ds.Tables["People"].Select();
        List<People> peopleList = new List<People>();
        foreach (DataRow dr in drs)
        {
            People people = new People();
            people.userId = DBNull.Value.Equals(dr["UserID"]) ? null : dr["UserID"].ToString();
            people.name = DBNull.Value.Equals(dr["Name"]) ? null : dr["Name"].ToString();
            people.picture = DBNull.Value.Equals(dr["Picture"]) ? null : (byte[])dr["Picture"];
            people.roomNo = DBNull.Value.Equals(dr["RoomNo"]) ? null : dr["RoomNo"].ToString();
            people.facultyName = DBNull.Value.Equals(dr["FacultyName"]) ? null : dr["FacultyName"].ToString();
            people.departmentName = DBNull.Value.Equals(dr["DepartmentName"]) ? null : dr["DepartmentName"].ToString();
            peopleList.Add(people);
        }

        return peopleList;
    }

    [WebMethod]
    public People findOnePeople(String userId)
    {
        DataSet ds = new DataSet();
        sql = "SELECT UserID, Name, TelNo, Email, Picture, tblUser.RoomNo, DepartmentName, FacultyName, BuildingNo ";
        sql += "FROM tblUser INNER JOIN tblDepartment ON tblUser.DepartmentID = tblDepartment.DepartmentID ";
        sql += "INNER JOIN tblFaculty ON tblUser.FacultyID = tblFaculty.FacultyID ";
        sql += "LEFT JOIN tblRoom ON tblUser.RoomNo = tblRoom.RoomNo WHERE userId = @userId";

        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        SqlParameter userIdParam = dataAdapter.SelectCommand.Parameters.Add("@userId", SqlDbType.VarChar, 10);
        userIdParam.Value = userId;

        dataAdapter.Fill(ds, "People");

        People people = null;
        if (ds.Tables["People"].Select().Length > 0)
        {
            DataRow dr = ds.Tables["People"].Rows[0];
            people = new People();
            people.userId = DBNull.Value.Equals(dr["UserID"]) ? null : dr["UserID"].ToString();
            people.name = DBNull.Value.Equals(dr["Name"]) ? null : dr["Name"].ToString();
            people.telNo = DBNull.Value.Equals(dr["TelNo"]) ? null : dr["TelNo"].ToString();
            people.email = DBNull.Value.Equals(dr["Email"]) ? null : dr["Email"].ToString();
            people.picture = DBNull.Value.Equals(dr["Picture"]) ? null : (byte[])dr["Picture"];
            people.roomNo = DBNull.Value.Equals(dr["RoomNo"]) ? null : dr["RoomNo"].ToString();
            people.buildingNo = DBNull.Value.Equals(dr["BuildingNo"]) ? null : dr["BuildingNo"].ToString();
            people.departmentName = DBNull.Value.Equals(dr["DepartmentName"]) ? null : dr["DepartmentName"].ToString();
            people.facultyName = DBNull.Value.Equals(dr["FacultyName"]) ? null : dr["FacultyName"].ToString();
        }
        return people;
    }

    [WebMethod]
    public List<Faculty> findFacultyByStatus()
    {
        DataSet ds = new DataSet();
        sql = "SELECT * FROM tblFaculty WHERE StatusID <> 'S2' OR StatusID IS NULL";
        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        dataAdapter.Fill(ds, "Faculty");

        DataRow[] drs = ds.Tables["Faculty"].Select();
        List<Faculty> facultyList = new List<Faculty>();
        foreach (DataRow dr in drs)
        {
            Faculty faculty = new Faculty();
            faculty.facultyID = DBNull.Value.Equals(dr["FacultyID"]) ? null : dr["FacultyID"].ToString();
            faculty.facultyName = DBNull.Value.Equals(dr["FacultyName"]) ? null : dr["FacultyName"].ToString();
            faculty.statusID = DBNull.Value.Equals(dr["StatusID"]) ? null : dr["StatusID"].ToString();
            facultyList.Add(faculty);
        }

        return facultyList;
    }

    [WebMethod]
    public List<Building> findAllBuilding()
    {
        DataSet ds = new DataSet();
        sql = "SELECT * FROM tblBuilding";
        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        dataAdapter.Fill(ds, "Building");

        DataRow[] drs = ds.Tables["Building"].Select();
        List<Building> buildingList = new List<Building>();
        foreach (DataRow dr in drs)
        {
            Building building = new Building();
            building.buildingNo = DBNull.Value.Equals(dr["BuildingNo"]) ? 0 : Convert.ToInt16(dr["BuildingNo"]);
            building.picture = DBNull.Value.Equals(dr["Picture"]) ? null : (byte[]) dr["Picture"];
            building.description = DBNull.Value.Equals(dr["Description"]) ? null : dr["Description"].ToString();
            building.floor = DBNull.Value.Equals(dr["Floor"]) ? 0 : Convert.ToInt16(dr["Floor"]);
            building.longitude = DBNull.Value.Equals(dr["Longitude"]) ? null : dr["Longitude"].ToString();
            building.latitude = DBNull.Value.Equals(dr["Latitude"]) ? null : dr["Latitude"].ToString();
            buildingList.Add(building);
        }

        return buildingList;
    }

    [WebMethod]
    public List<Room> findAllRoom()
    {
        DataSet ds = new DataSet();
        sql = "SELECT * FROM tblRoom";
        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        dataAdapter.Fill(ds, "Room");

        DataRow[] drs = ds.Tables["Room"].Select();
        List<Room> roomList = new List<Room>();
        foreach (DataRow dr in drs)
        {
            Room room = new Room();
            room.roomNo = DBNull.Value.Equals(dr["RoomNo"]) ? null : dr["RoomNo"].ToString();
            room.buildingNo = DBNull.Value.Equals(dr["BuildingNo"]) ? 0 : Convert.ToInt16(dr["BuildingNo"]);
            room.floor = DBNull.Value.Equals(dr["Floor"]) ? 0 : Convert.ToInt16(dr["Floor"]);
            roomList.Add(room);
        }

        return roomList;
    }

    [WebMethod]
    public List<Department> findDepartmentByFacultyAndStatus(String facultyID)
    {
        DataSet ds = new DataSet();
        sql = "SELECT * FROM tblDepartment INNER JOIN tblFaculty ON tblDepartment.FacultyID = tblFaculty.FacultyID WHERE (tblDepartment.StatusID <> 'S2' OR tblDepartment.StatusID IS NULL) AND tblDepartment.FacultyID = @facultyID";
        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());

        SqlParameter facultyIdParam = dataAdapter.SelectCommand.Parameters.Add("@facultyID", SqlDbType.VarChar, 3);
        facultyIdParam.Value = facultyID;

        dataAdapter.Fill(ds, "Department");

        DataRow[] drs = ds.Tables["Department"].Select();
        List<Department> list = new List<Department>();
        foreach (DataRow dr in drs)
        {
            Department department = new Department();
            department.departmentID = DBNull.Value.Equals(dr["DepartmentID"]) ? null : dr["DepartmentID"].ToString();
            department.departmentName = DBNull.Value.Equals(dr["DepartmentName"]) ? null : dr["DepartmentName"].ToString();
            department.statusID = DBNull.Value.Equals(dr["StatusID"]) ? null : dr["StatusID"].ToString();
            department.facultyID = DBNull.Value.Equals(dr["FacultyID"]) ? null : dr["FacultyID"].ToString();
            department.facultyName = DBNull.Value.Equals(dr["FacultyName"]) ? null : dr["FacultyName"].ToString();
            list.Add(department);
        }

        return list;
    }

        [WebMethod]
    public Department findOneDepartment(String departmentId)
    {
        DataSet ds = new DataSet();
        sql = "SELECT * FROM tblDepartment INNER JOIN tblFaculty ON tblDepartment.FacultyID = tblFaculty.FacultyID WHERE DepartmentID = @departmentId";

        SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, c1.cn());
        SqlParameter departmentIdParam = dataAdapter.SelectCommand.Parameters.Add("@departmentId", SqlDbType.VarChar, 3);
        departmentIdParam.Value = departmentId;

        dataAdapter.Fill(ds, "Department");

        Department department = null;
        if (ds.Tables["Department"].Select().Length > 0)
        {
            DataRow dr = ds.Tables["Department"].Rows[0];
            department = new Department();
            department.departmentID = DBNull.Value.Equals(dr["DepartmentID"]) ? null : dr["DepartmentID"].ToString();
            department.departmentName = DBNull.Value.Equals(dr["DepartmentName"]) ? null : dr["DepartmentName"].ToString();
            department.statusID = DBNull.Value.Equals(dr["StatusID"]) ? null : dr["StatusID"].ToString();
            department.facultyID = DBNull.Value.Equals(dr["FacultyID"]) ? null : dr["FacultyID"].ToString();
            department.facultyName = DBNull.Value.Equals(dr["FacultyName"]) ? null : dr["FacultyName"].ToString();
        }
        return department;
    }
}