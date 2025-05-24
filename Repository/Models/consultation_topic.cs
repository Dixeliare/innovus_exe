using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class consultation_topic
{
    public int consultation_topic_id { get; set; }

    public string? consultation_topic_name { get; set; }

    public virtual ICollection<consultation_request> consultation_requests { get; set; } = new List<consultation_request>();
}
