using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class sheet
{
    public int sheet_id { get; set; }

    public string sheet_url { get; set; } = null!;

    public virtual sheet_music? sheet_music { get; set; }
}
