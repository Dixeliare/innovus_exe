using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class week
{
    public int week_id { get; set; }

    public int? week_number { get; set; }

    public DateOnly? day_of_week { get; set; }

    public int? schedule_id { get; set; }

    public virtual ICollection<class_session> class_sessions { get; set; } = new List<class_session>();

    public virtual schedule? schedule { get; set; }
}
