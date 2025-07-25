using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class class_session
{
    public int class_session_id { get; set; }

    public int? session_number { get; set; }

    public DateOnly? date { get; set; }

    public int day_id { get; set; }

    public int class_id { get; set; }

    public int time_slot_id { get; set; }

    public int room_id { get; set; }

    public virtual _class _class { get; set; } = null!;

    public virtual ICollection<attendance> attendances { get; set; } = new List<attendance>();

    public virtual day day { get; set; } = null!;

    public virtual room room { get; set; } = null!;

    public virtual timeslot time_slot { get; set; } = null!;
}
