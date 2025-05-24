using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class attendance
{
    public int attendance_id { get; set; }

    public bool? status { get; set; }

    public DateTime? check_at { get; set; }

    public string note { get; set; } = null!;

    public int? user_id { get; set; }

    public int? class_session_id { get; set; }

    public virtual class_session? class_session { get; set; }

    public virtual user? user { get; set; }
}
