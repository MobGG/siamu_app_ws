using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public class Room
{
    public string roomNo { get; set; }
    public int buildingNo { get; set; }
    public int floor { get; set; }
}