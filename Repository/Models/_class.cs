using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class _class
{
    public int class_id { get; set; }

    public string? class_code { get; set; }

    public string? instrument { get; set; }

    public virtual ICollection<class_session> class_sessions { get; set; } = new List<class_session>();

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
