using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class statistic
{
    public int statistic_id { get; set; }

    public DateOnly? date { get; set; }

    public int? new_students { get; set; }

    public decimal? monthly_revenue { get; set; }

    public int? consultation_count { get; set; }

    public int? total_students { get; set; }

    public int? consultation_request_count { get; set; }

    public virtual ICollection<consultation_request> consultation_requests { get; set; } = new List<consultation_request>();

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
