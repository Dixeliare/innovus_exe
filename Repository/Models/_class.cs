using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class _class
{
    public int class_id { get; set; }

    public string class_code { get; set; } = null!;

    public int instrument_id { get; set; }

    public int total_students { get; set; }

    public int current_students_count { get; set; }

    public virtual ICollection<class_session> class_sessions { get; set; } = new List<class_session>();

    public virtual instrument instrument { get; set; } = null!;

    public virtual ICollection<opening_schedule> opening_schedules { get; set; } = new List<opening_schedule>();

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
