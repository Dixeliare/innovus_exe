using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class class_session
{
    public int class_session_id { get; set; }

    public int? session_number { get; set; }

    public DateOnly? date { get; set; }

    public string room_code { get; set; } = null!;

    public int week_id { get; set; }

    public int class_id { get; set; }

    public int time_slot_id { get; set; }

    public virtual _class _class { get; set; } = null!;

    public virtual ICollection<attendance> attendances { get; set; } = new List<attendance>();

    public virtual timeslot time_slot { get; set; } = null!;

    public virtual week week { get; set; } = null!;
}
