using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class schedule
{
    public int schedule_id { get; set; }

    public DateOnly? month_year { get; set; }

    public string? note { get; set; }

    public virtual user? user { get; set; }

    public virtual ICollection<week> weeks { get; set; } = new List<week>();
}
