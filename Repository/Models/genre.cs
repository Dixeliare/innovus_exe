using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class genre
{
    public int genre_id { get; set; }

    public string genre_name { get; set; } = null!;

    public virtual ICollection<sheet_music> sheet_musics { get; set; } = new List<sheet_music>();
}
