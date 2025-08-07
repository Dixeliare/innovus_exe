using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class sheet_music
{
    public int sheet_music_id { get; set; }

    public int? number { get; set; }

    public string? music_name { get; set; }

    public string composer { get; set; } = null!;

    public string cover_url { get; set; } = null!;

    public int? sheet_quantity { get; set; }

    public int? favorite_count { get; set; }

    public virtual ICollection<sheet> sheets { get; set; } = new List<sheet>();

    public virtual ICollection<user_favorite_sheet> user_favorite_sheets { get; set; } = new List<user_favorite_sheet>();

    public virtual ICollection<genre> genres { get; set; } = new List<genre>();
}
