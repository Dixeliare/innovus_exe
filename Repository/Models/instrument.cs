using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class instrument
{
    public int instrument_id { get; set; }

    public string? instrument_name { get; set; }

    public virtual ICollection<document> documents { get; set; } = new List<document>();
}
