using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class sheet
{
    public int sheet_id { get; set; }

    public string sheet_url { get; set; } = null!;

    public virtual ICollection<sheet_music> sheet_musics { get; set; } = new List<sheet_music>();
}
