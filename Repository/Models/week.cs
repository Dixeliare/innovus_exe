using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class week
{
    public int week_id { get; set; }

    public int schedule_id { get; set; }

    public int week_number_in_month { get; set; }

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    public int? num_active_days { get; set; }

    public virtual ICollection<day> days { get; set; } = new List<day>();

    public virtual schedule schedule { get; set; } = null!;
}
