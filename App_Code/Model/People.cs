using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class People
{
    public string userId { get; set; }
    public string password { get; set; }
    public string name { get; set; }
    public string telNo { get; set; }
    public string email { get; set; }
    public byte[] picture { get; set; }
    public string positionId { get; set; }
    public string statusId { get; set; }
    public string facultyId { get; set; }
    public string facultyName { get; set; }
    public string roomNo { get; set; }
    public string buildingNo { get; set; }
    public string permissionId { get; set; }
    public string departmentName { get; set; }
}
