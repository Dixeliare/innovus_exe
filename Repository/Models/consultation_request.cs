using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class consultation_request
{
    public int consultation_request_id { get; set; }

    public string? fullname { get; set; }

    public string? contact_number { get; set; }

    public string email { get; set; } = null!;

    public string note { get; set; } = null!;

    public bool? has_contact { get; set; }

    public int? statistic_id { get; set; }

    public int? consultation_topic_id { get; set; }

    public virtual consultation_topic? consultation_topic { get; set; }

    public virtual statistic? statistic { get; set; }
}
