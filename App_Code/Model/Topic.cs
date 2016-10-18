using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[Serializable]
public class Topic
{
    public string topicNo { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public byte[] picture { get; set; }
    public string dateTime { get; set; }
    public string statusId { get; set; }
    public string userId { get; set; }
    public string buildingNo { get; set; }
}