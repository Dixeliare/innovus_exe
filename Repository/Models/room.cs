using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class room
{
    public int room_id { get; set; }

    public string room_code { get; set; } = null!;

    public int? capacity { get; set; }

    public string? description { get; set; }

    public virtual ICollection<class_session> class_sessions { get; set; } = new List<class_session>();
}
