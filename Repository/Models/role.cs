using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class role
{
    public int role_id { get; set; }

    public string? role_name { get; set; }

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
