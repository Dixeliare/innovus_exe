using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class day
{
    public int day_id { get; set; }

    public int? week_id { get; set; }

    public DateOnly date_of_day { get; set; }

    public string day_of_week_name { get; set; } = null!;

    public bool? is_active { get; set; }

    public virtual ICollection<class_session> class_sessions { get; set; } = new List<class_session>();

    public virtual week? week { get; set; }
}
