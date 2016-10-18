using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public class Building
{
    public int buildingNo { get; set; }
    public byte[] picture { get; set; }
    public string description { get; set; }
    public int floor { get; set; }
    public string longitude { get; set; }
    public string latitude { get; set; }
}