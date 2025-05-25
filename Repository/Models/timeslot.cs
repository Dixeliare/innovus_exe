using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class timeslot
{
    public int timeslot_id { get; set; }

    public TimeOnly start_time { get; set; }

    public TimeOnly end_time { get; set; }

    public virtual ICollection<class_session> class_sessions { get; set; } = new List<class_session>();
}
