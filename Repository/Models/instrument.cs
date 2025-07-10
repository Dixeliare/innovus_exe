using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class instrument
{
    public int instrument_id { get; set; }

    public string? instrument_name { get; set; }

    public virtual ICollection<_class> _classes { get; set; } = new List<_class>();

    public virtual ICollection<document> documents { get; set; } = new List<document>();

    public virtual ICollection<opening_schedule> opening_schedules { get; set; } = new List<opening_schedule>();
}
