using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class day_of_week_lookup
{
    public int day_of_week_id { get; set; }

    public string day_name { get; set; } = null!;

    public int day_number { get; set; }

    public virtual ICollection<opening_schedule> opening_schedules { get; set; } = new List<opening_schedule>();
}
