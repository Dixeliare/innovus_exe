using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class gender
{
    public int gender_id { get; set; }

    public string gender_name { get; set; } = null!;

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
