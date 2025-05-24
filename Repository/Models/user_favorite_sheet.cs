using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class user_favorite_sheet
{
    public int user_id { get; set; }

    public int sheet_music_id { get; set; }

    public bool? is_favorite { get; set; }

    public virtual sheet_music sheet_music { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
