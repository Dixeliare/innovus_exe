using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class attendance_status
{
    public int status_id { get; set; }

    public string status_name { get; set; } = null!;

    public virtual ICollection<attendance> attendances { get; set; } = new List<attendance>();
}
